using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 关卡类型枚举
/// </summary>
public enum E_LevelType
{
    TowerDefense,  // 塔防模式
    Survival,      // 生存模式
    Endless        // 无尽模式
}

/// <summary>
/// 关卡状态枚举
/// </summary>
public enum E_LevelState
{
    Uninitialized, // 未初始化
    Ready,         // 准备就绪
    Running,       // 关卡进行中
    Completed,     // 关卡完成
    Failed,        // 关卡失败
}

/// <summary>
/// 关卡流程控制器
/// </summary>
public class LevelMgr : Singleton<LevelMgr>
{
    [Header("关卡基础配置")]
    public E_LevelType currentLevelType;//关卡类型
    public int currentLevelID;//关卡ID
    public float levelTime;//关卡已进行时间
    [Header("关卡进度数据")]
    public int nowWaveCount;//目前波数
    public int maxWaveCount;//最大波数
    public bool isCoreTowerAlive = true;//核心塔是否存活
    // 关卡状态
    public E_LevelState CurrentLevelState { get; private set; }

    // 怪物生成与管理
    public Transform bornAreaRoot;       // 怪物生成器根节点，拖拽赋值
    public List<Transform> bornPoints;  // 所有出生点
    public int totalMonsterCount;        // 当前关卡怪物总数
    public int killedMonsterCount;       // 已击杀怪物数

    //时间刷怪配置
    public GameObject[] monsterPrefabs;       // 拖怪物预制体
    public float spawnInterval;                  // 多少秒生成一只怪
    private float spawnTimer;                    // 刷怪计时器
    private int spawnedMonsterCount;             // 已生成怪物

    //其他关卡数据（根据需要添加）
    public Transform playerBornTrans;       // 玩家出生点，拖拽赋值
 //==========================================================================
    protected override void Awake()
    {
        base.Awake();
        // 全局事件监听
        EventMgr.Instance.AddListener(EventConst.TowerDestroyed, () =>
        {
            if (this == null) return;
            FailLevel();
        });
        //监听玩家死亡事件
        EventMgr.Instance.AddListener(EventConst.PlayerDeath,() =>
        {
            if (this == null) return;
            FailLevel();
        });
        //监听怪物死亡事件
        EventMgr.Instance.AddListener(EventConst.MonsterKilled, () =>
            {
                if (this == null) return;
                OnMonsterKilled();
            });
    }
    private void Start()
    {
        if (ChoosePaternPanel.GameMode == E_LevelType.TowerDefense)
        {
            InitLevelData(E_LevelType.TowerDefense, ChooseLevelPanel.levelID);
            if (CurrentLevelState == E_LevelState.Ready)
            {
                StartLevel();
            }
        }
        else if (ChoosePaternPanel.GameMode == E_LevelType.Endless)
        {
            InitEndlessData(E_LevelType.Endless, ChooseLevelPanel.levelID);
            if (CurrentLevelState == E_LevelState.Ready)
            {
                StartEndlessLevel();
            }
        }
        else
        {
            Debug.LogWarning("管理器错误，无法启动关卡！");
        }
    
    }
    private void Update()
    {
        if(AudioMgr.Instance.bgmSource.clip==null)
            AudioMgr.Instance.PlayBGM();
        if (CurrentLevelState == E_LevelState.Running)
        {
            levelTime += Time.deltaTime;
        }
        RunTimeDrivenSpawnSystem();
    }
    // 初始化关卡数据
    public void InitLevelData(E_LevelType levelType, int levelID = 1)
    {
        currentLevelType = levelType;
        currentLevelID = levelID;
        levelTime = 0;
        nowWaveCount = 1;
        killedMonsterCount = 0;
        isCoreTowerAlive = true;

        //根据关卡类型和ID设置总波数和怪物数
        switch (levelID)
        {
            case 1:
                spawnInterval=2f;
                maxWaveCount = 3;
                totalMonsterCount = 12;
                break;
            case 2:
                spawnInterval = 1.5f;
                maxWaveCount = 5;
                totalMonsterCount = 25;
                break;
            case 3:
                spawnInterval = 2f;
                maxWaveCount = 10;
                totalMonsterCount = 50;
                break;
            default:
                spawnInterval = 2f;
                maxWaveCount = 50;
                totalMonsterCount = 50;
                break;
        }
        //绑定出生点
        playerBornTrans = GameObject.Find("playerBornTrans").transform;
        if(playerBornTrans==null)
        {
            Debug.LogError("未找到玩家出生点对象，请确保场景中存在名为 'playerBornTrans' 的对象，并且已正确设置位置！");
        }
        CurrentLevelState = E_LevelState.Ready;

        Debug.Log($"关卡初始化完成 | 类型：{levelType} | ID：{levelID}");
    }
    public void InitEndlessData(E_LevelType levelType, int levelID = 1)
    {
        currentLevelType = levelType;
        currentLevelID = levelID;
        levelTime = 0;
        nowWaveCount = 1;
        killedMonsterCount = 0;
        isCoreTowerAlive = true;
        //玩家数据重置

        //设置总波数和怪物数
        maxWaveCount = 9999;
        totalMonsterCount = 150000;

        //绑定出生点
        playerBornTrans = GameObject.Find("playerBornTrans").transform;
        if (playerBornTrans == null)
        {
            Debug.LogError("未找到玩家出生点对象，请确保场景中存在名为 'playerBornTrans' 的对象，并且已正确设置位置！");
        }
        CurrentLevelState = E_LevelState.Ready;
        //OnLevelStateChanged?.Invoke(CurrentLevelState);
        Debug.Log($"关卡初始化完成 | 类型：{levelType} | ID：{levelID}");
    }

    /// 启动关卡
    public void StartLevel()
    {
        if (CurrentLevelState != E_LevelState.Ready)
        {
            return;
        }
        CurrentLevelState = E_LevelState.Running;
        //显示游戏界面
        GamingPanel gamingPanel=UIMgr.Instance.ShowPanel<GamingPanel>();

        //加载英雄
        GameObject hero = Resources.Load<GameObject>($"Prefabs/Role/{PlayerDataMgr.Instance.playerState.currentHeroName}");
        GameObject heroObj = Instantiate(hero, playerBornTrans.position, Quaternion.identity);
        heroObj.AddComponent<PlayerController>();
        heroObj.AddComponent<WeaponController>();

        //开始提示
        TempTipPanel p = UIMgr.Instance.ShowPanel<TempTipPanel>();
        p.ShowTipAutoHide("关卡开始！准备迎战敌人！", 2f);

        //获取出生点
        CollectBornPoints();

        //初始波数开启
        StartCoroutine(InitWaveEvent());
    }

    public void StartEndlessLevel()
    {
        if (CurrentLevelState != E_LevelState.Ready)
        {
            //Debug.LogWarning("当前关卡未准备就绪，无法启动！");
            return;
        }
        CurrentLevelState = E_LevelState.Running;
        //OnLevelStateChanged?.Invoke(CurrentLevelState);
        //显示游戏界面
        GamingPanel gamingPanel = UIMgr.Instance.ShowPanel<GamingPanel>();
        //加载英雄
        GameObject hero = Resources.Load<GameObject>($"Prefabs/Role/{PlayerDataMgr.Instance.playerState.currentHeroName}");
        GameObject heroObj = Instantiate(hero, playerBornTrans.position, Quaternion.identity);
        heroObj.AddComponent<PlayerController>();
        heroObj.AddComponent<WeaponController>();
        //开始提示
        TempTipPanel p = UIMgr.Instance.ShowPanel<TempTipPanel>();
        p.ShowTipAutoHide("无尽模式开始！挑战极限吧！", 2f);
        //获取出生点
        CollectBornPoints();
        //初始波数提示
        StartCoroutine(InitWaveEvent());
        Debug.Log($"无尽模式启动 | ID：{currentLevelID}");
    }
    private IEnumerator InitWaveEvent()
    {
        yield return null; // 等一帧，所有UI绑定完毕
        EventMgr.Instance.Trigger(EventConst.WaveUpdated, nowWaveCount, maxWaveCount);
    }

    // 关卡完成
    public void CompleteLevel()
    {
        Debug.Log("所有敌人已击杀,关卡胜利！");
        EventMgr.Instance.Trigger(EventConst.WinGame);
        UIMgr.Instance.ShowPanel<WinPanel>();
    }

    // 关卡失败
    public void FailLevel()
    {
        UIMgr.Instance.ShowPanel<LosePanel>();
        if (CurrentLevelState != E_LevelState.Running) return;
        isCoreTowerAlive = false;
        CurrentLevelState = E_LevelState.Failed;

        //设置物品数量(本事件用于设置物资数量，关卡模式胜利和失败结算方式一样，故发胜利事件)
        EventMgr.Instance.Trigger(EventConst.WinGame);
        Debug.Log("游戏结束！");
    }
   
    #region 怪物生成与管理
    // 收集所有出生点
    public void CollectBornPoints()
    {
        bornPoints.Clear();
        if (bornAreaRoot != null)
        {
            foreach (Transform t in bornAreaRoot)
            {
                bornPoints.Add(t);
            }
        }
        Debug.Log("已收集出生点：" + bornPoints.Count + " 个");
    }

    // 随机出生点，生成一只怪物
    // 随机获取一个怪物预制体
    private GameObject GetRandomMonsterPrefab()
    {
        if (monsterPrefabs == null || monsterPrefabs.Length == 0)
            return null;

        int ranIdx = UnityEngine.Random.Range(0, monsterPrefabs.Length);
        return monsterPrefabs[ranIdx];
    }

    // 随机出生点，生成一只怪物
    public void SpawnMonster(GameObject monsterPrefab)
    {
        if (bornPoints.Count == 0 || monsterPrefab == null) return;

        // 随机选一个出生点
        int randomIndex = UnityEngine.Random.Range(0, bornPoints.Count);
        Transform point = bornPoints[randomIndex];

        // 生成怪物
        GameObject monster = Instantiate(monsterPrefab, point.position, point.rotation);
        Debug.Log("生成怪物：" + monster.name);
    }

    // 敌人死亡时调用（波次刷新、怪物计数、 判断胜利）
    public void OnMonsterKilled()
    {
        killedMonsterCount++;
        Debug.Log($"已击杀：{killedMonsterCount}/{totalMonsterCount}");
        // 判断是否全部击杀
        if (killedMonsterCount >= totalMonsterCount)
        {
            CompleteLevel();
        }
        // 波次进度算法
        // 进度比例
        float progress = (float)killedMonsterCount / totalMonsterCount;
        // 计算对应波数
        int targetWave = 1 + Mathf.RoundToInt(progress * (maxWaveCount - 1));
        // 防止越界 & 只增不减
        targetWave = Mathf.Clamp(targetWave, 1, maxWaveCount);
        // 波数变化才更新、触发事件
        if (targetWave > nowWaveCount)
        {
            //Debug.Log($"目标波数：{targetWave}");
            //OnWaveChanged?.Invoke(nowWaveCount);
            nowWaveCount = targetWave;
            // 提示波数更新
            Debug.Log($"当前波数：{nowWaveCount}/{maxWaveCount}");
            TempTipPanel panel = UIMgr.Instance.ShowPanel<TempTipPanel>();
            panel.ShowTipAutoHide($"第 {nowWaveCount} 波僵尸 来袭！", 2f);
            //发事件
            EventMgr.Instance.Trigger(EventConst.WaveUpdated, nowWaveCount, maxWaveCount);
        }
    }
    /// <summary>
    /// 时间驱动刷怪器
    /// </summary>
    private void RunTimeDrivenSpawnSystem()
    {
        //时间到自动刷怪
        spawnTimer += Time.deltaTime;
        if (spawnTimer >= spawnInterval)
        {
            spawnTimer = 0;

            if (spawnedMonsterCount < totalMonsterCount)
            {
                // 随机拿一种怪物生成
                GameObject ranMonster = GetRandomMonsterPrefab();
                SpawnMonster(ranMonster);
                spawnedMonsterCount++;
            }
        }
    }
    #endregion

    private void OnDestroy()
    {
        
    }
}

// 通用单例模板
public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;
    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<T>();
                if (_instance == null)
                {
                    GameObject obj = new GameObject(typeof(T).Name);
                    _instance = obj.AddComponent<T>();
                }
            }
            return _instance;
        }
    }

    protected virtual void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            _instance = this as T;
        }
    }
}