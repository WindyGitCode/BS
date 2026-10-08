using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SurvivalState
{
    Idle,       // 未开始
    Fighting,   // 战斗中
    SafeTime,   // 安全时间
    Ended       // 已结束
}

public class SurvivalLevelMgr : Singleton<SurvivalLevelMgr>
{
    [Header("地图ID")]
    public int mapID;

    [Header("无限波次配置（只配1组，无限循环）")]
    public int waveMonsterCount = 10;       // 每波怪数量
    public GameObject monsterPrefab;        // 怪物预制体

    [Header("安全时间")]
    public float safeTimeDuration = 15f;    // 每波后安全时间

    [Header("刷怪设置")]
    public float spawnInterval = 2f;
    public List<Transform> bornPoints;

    // 运行时状态
    public SurvivalState currentState { get; private set; }
    public int currentWave { get; private set; }
    public int aliveMonsters { get; private set; }
    private int spawnedThisWave;                           // 本波已生成
    public float safeTimeRemaining { get; private set; }
    private float spawnTimer;
    // ======================================================
    //其他关卡数据（根据需要添加）
    public Transform playerBornTrans;       // 玩家出生点，拖拽赋值
    private Coroutine _safeRoutine;         //安全时间协程
    //GameData gameData = GameDataMgr.Instance.GetGameData();

    protected override void Awake()
    {
        base.Awake();
        //事件监听
        EventMgr.Instance.AddListener(EventConst.MonsterKilled, OnMonsterKilled);
        EventMgr.Instance.AddListener(EventConst.PlayerDeath, OnPlayerDeath);
        EventMgr.Instance.AddListener(EventConst.FinishDirection, InitLevel);
    }

    private void Start()
    {
        
    }

    private void InitLevel()
    {
        currentState = SurvivalState.Idle;
        CollectBornPoints();
        //显示游戏界面，隐藏主塔血量UI
        GamingPanel p = UIMgr.Instance.ShowPanel<GamingPanel>();
        p.HideAssignedObjects();
        //加载英雄
        GameObject hero = Resources.Load<GameObject>($"Prefabs/Role/{PlayerDataMgr.Instance.playerState.currentHeroName}");
        GameObject heroObj = Instantiate(hero, playerBornTrans.position, Quaternion.identity);
        heroObj.AddComponent<PlayerController>();
        heroObj.AddComponent<WeaponController>();
    }
    private void Update()
    {
        if (currentState == SurvivalState.Fighting)
        {
            RunSpawnSystem();
        }
        else if (currentState == SurvivalState.SafeTime)
        {
            RunSafeTime();
        }
        else if (currentState == SurvivalState.Ended)
        {
            // 游戏结束后可能的结算逻辑
        }
    }
    // 外部接口：开始游戏
    public void StartGame()
    {
        if (currentState != SurvivalState.Idle) return;

        currentState = SurvivalState.Fighting;
        currentWave = 1;
        aliveMonsters = 0;
        spawnTimer = 0;
        // 发送事件：游戏开始
        EventMgr.Instance.Trigger(EventConst.WaveUpdated, currentWave, 9999);
    }

    // 外部接口：NPC 主动结束,游戏胜利
    public void WinGame()
    {
        StopAllCoroutines();
        currentState = SurvivalState.Ended;
        EventMgr.Instance.Trigger(EventConst.WinGame);
        UIMgr.Instance.HidePanel<SafeTimePanel>();
        UIMgr.Instance.ShowPanel<WinPanel>();
    }

    // ======================================================
    // 外部/内部：玩家死亡 = 失败
    // ======================================================
    public void LoseGame()
    {
        
        StopAllCoroutines();
        currentState = SurvivalState.Ended;
        LoseAllObject();
        UIMgr.Instance.HidePanel<SafeTimePanel>();
        UIMgr.Instance.ShowPanel<LosePanel>();
    }
    //物资丢失
    public void LoseAllObject()
    {
        //丢失所有物品
        GameDataMgr.Instance.SetFixTowerNum(0);
        GameDataMgr.Instance.SetItemNum(0, 0, 0);
        GameDataMgr.Instance.SetAmmoNum(0, 0, 0, 0, 0, 0);
    }

    // ======================================================
    // 波次开始（无限循环）
    // ======================================================
    void StartNextWave()
    {
        currentState = SurvivalState.Fighting;
        currentWave++;
        spawnedThisWave = 0;   // 本波生成数量重置
        aliveMonsters = 0;     // 存活怪物重置
        spawnTimer = 0;         //计时器重置

        // 发送波数更新事件
        EventMgr.Instance.Trigger(EventConst.WaveUpdated, currentWave, 9999);
    }

    // ======================================================
    // 刷怪逻辑
    // ======================================================
    void RunSpawnSystem()
    {
        spawnTimer += Time.deltaTime;
        if (spawnTimer >= spawnInterval)
        {
            spawnTimer = 0;

            // 刷若干只怪，直到本波生成满
            if (spawnedThisWave < waveMonsterCount)
            {
                SpawnMonster();
                spawnedThisWave++;
                aliveMonsters++;
                Debug.Log($"生成怪物，存活 = {aliveMonsters} | 本波已生成：{spawnedThisWave}/{waveMonsterCount}");
            }
        }
    }

    void SpawnMonster()
    {
        if (bornPoints.Count == 0 || monsterPrefab == null) return;
        int rand = Random.Range(0, bornPoints.Count);
        var point = bornPoints[rand];
        //PoolMgr.GetInstance().GetObj(monsterPrefab.name, (obj) =>
        //{
        //    obj.transform.position = point.position;
        //    obj.transform.rotation = point.rotation;
        //});
        Instantiate(monsterPrefab, point.position, point.rotation);
    }

    // ======================================================
    // 安全时间
    // ======================================================
    void StartSafeTime()
    {
        Debug.Log("进入安全时间");
        currentState = SurvivalState.SafeTime;
        safeTimeRemaining = safeTimeDuration;
        //  启动UI倒计时协程
        SafeTimePanel panel = UIMgr.Instance.ShowPanel<SafeTimePanel>();
        _safeRoutine = StartCoroutine(SafeTimeRoutine(panel));
        //触发事件：安全时间开始
        EventMgr.Instance.Trigger(EventConst.SurvivalSafeTimeStart);
    }

    void RunSafeTime()
    {
        if (safeTimeRemaining > 0)
        {
            safeTimeRemaining -= Time.deltaTime;
            // 这里更新UI显示剩余安全时间
        }
        else
        {
            Debug.Log("安全时间结束，进入下一波");
            safeTimeRemaining = 0;
            StartNextWave(); // 进下一波
        }
    }
    private IEnumerator SafeTimeRoutine(SafeTimePanel panel)
    {
        int remaining = Mathf.RoundToInt(safeTimeDuration);

        // 每秒更新一次
        while (remaining > 0)
        {
            panel.SetTime(remaining);
            yield return new WaitForSeconds(1);
            remaining--;
        }

        // 安全时间结束，关面板，隐藏npc，刷新下一波
        UIMgr.Instance.HidePanel<SafeTimePanel>();
        EventMgr.Instance.Trigger(EventConst.HideSurvivalNPC);
        StartNextWave();
    }

    // ======================================================
    // 怪物死亡时调用
    // ======================================================
    public void OnMonsterKilled()
    {
        aliveMonsters--;
        Debug.Log($"怪物死亡，剩余存活：{aliveMonsters}");

        // 本波怪全部死光,进入安全时间
        if (aliveMonsters <= 0 && spawnedThisWave >= waveMonsterCount && currentState == SurvivalState.Fighting)
        {
            StartSafeTime();
        }
    }

    // ======================================================
    // 玩家死亡
    // ======================================================
    void OnPlayerDeath()
    {
        if (currentState is SurvivalState.Fighting or SurvivalState.SafeTime)
        {
            LoseGame();
        }
    }
    // ======================================================
    // 收集出生点
    // ======================================================
    void CollectBornPoints()
    {
        bornPoints.Clear();
        GameObject root = GameObject.Find("MonsterBornRoot");
        if (root == null) return;
        foreach (Transform t in root.transform)
            bornPoints.Add(t);
        Debug.Log($"收集到 {bornPoints.Count} 个怪物出生点");
    }
    void OnDestroy()
    {
        EventMgr.Instance.RemoveListener(EventConst.MonsterKilled, OnMonsterKilled);
        EventMgr.Instance.RemoveListener(EventConst.PlayerDeath, OnPlayerDeath);
        EventMgr.Instance.RemoveListener(EventConst.FinishDirection, InitLevel);
    }
}