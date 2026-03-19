using UnityEngine;
using System;

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
/// 关卡流程控制器（单例模式，全局唯一）
/// </summary>
public class LevelController : Singleton<LevelController>
{
    // 关卡基础配置
    [Header("关卡基础配置")]
    public E_LevelType currentLevelType; // 当前关卡类型
    public int currentLevelID = 1;     // 当前关卡ID（塔防模式：1-20；生存/无尽：固定为1）
    public float levelTime;            // 关卡已进行时间（秒）
    public PlayerData playerData;          // 玩家数据（供UI展示）

    // 关卡进度数据
    [Header("关卡进度数据")]
    public int enemyKilledCount;       // 已击杀敌人数量
    public int towerBuiltCount;        // 已建造防御塔数量（仅塔防模式）
    public int waveCount;              // 当前波数（塔防/无尽模式）
    public int maxWaveCount;           // 最大波数（塔防模式：固定值；无尽模式：0表示无限）
    public bool isCoreTowerAlive = true;// 核心防御塔存活状态（仅塔防模式）

    // 关卡状态
    public E_LevelState CurrentLevelState { get; private set; }

    // 事件定义（供UI/成就系统监听）
    public event Action<E_LevelState> OnLevelStateChanged;
    public event Action<int> OnWaveChanged;
    public event Action OnLevelCompleted;
    public event Action OnLevelFailed;

/// <summary>
/// 初始化关卡数据（切换关卡/模式时调用）
/// </summary>
/// <param name="levelType">关卡类型</param>
/// <param name="levelID">关卡ID</param>
    public void InitLevelData(E_LevelType levelType, int levelID = 1)
    {
        // 重置所有关卡数据
        playerData=GameDataMgr.Instance.playerData;
        currentLevelType = levelType;
        currentLevelID = levelID;
        levelTime = 0;
        enemyKilledCount = 0;
        towerBuiltCount = 0;
        waveCount = 0;
        isCoreTowerAlive = true;
        
        // 根据关卡类型初始化专属配置
        switch (levelType)
        {
            case E_LevelType.TowerDefense:
                maxWaveCount = 10 + (levelID - 1) * 2; // 塔防关卡波数随ID递增
                break;
            case E_LevelType.Survival:
                maxWaveCount = 0; // 生存模式无固定波数
                break;
            case E_LevelType.Endless:
                maxWaveCount = 0; // 无尽模式无限波数
                break;
        }

        // 更新关卡状态
        CurrentLevelState = E_LevelState.Ready;
        OnLevelStateChanged?.Invoke(CurrentLevelState);
        Debug.Log($"关卡初始化完成 | 类型：{levelType} | ID：{levelID}");
    }

    /// <summary>
    /// 开始当前关卡
    /// </summary>
    public void StartLevel()
    {
        if (CurrentLevelState != E_LevelState.Ready)
        {
            Debug.LogWarning("当前关卡未准备就绪，无法启动！");
            return;
        }

        CurrentLevelState = E_LevelState.Running;
        OnLevelStateChanged?.Invoke(CurrentLevelState);
        //实例化角色,绑定角色脚本
        GameObject hero = Resources.Load<GameObject>($"Prefabs/Role/{playerData.nowHeroName}");
        GameObject heroObj= Instantiate(hero, new Vector3(220, 29, 220), Quaternion.identity);
        heroObj.AddComponent<PlayerController>();
        heroObj.GetComponent<Animator>().runtimeAnimatorController = Resources.Load<RuntimeAnimatorController>($"Controller/RoleGamingController/Handgun_Controller");
        Debug.Log($"关卡启动 | 类型：{currentLevelType} | ID：{currentLevelID}");
    }
    /// <summary>
    /// 更新关卡波数（塔防/无尽模式）
    /// </summary>
    public void NextWave()
    {
        if (CurrentLevelState != E_LevelState.Running) return;

        waveCount++;
        OnWaveChanged?.Invoke(waveCount);
        Debug.Log($"当前波数更新：{waveCount}");

        // 塔防模式：波数达到最大值则关卡完成
        if (currentLevelType == E_LevelType.TowerDefense && waveCount >= maxWaveCount)
        {
            CompleteLevel();
        }
    }

    /// <summary>
    /// 标记关卡完成
    /// </summary>
    public void CompleteLevel()
    {
        if (CurrentLevelState != E_LevelState.Running) return;

        CurrentLevelState = E_LevelState.Completed;
        OnLevelStateChanged?.Invoke(CurrentLevelState);
        OnLevelCompleted?.Invoke();
        Time.timeScale = 1; // 恢复游戏时间
        Debug.Log($"关卡完成 | 耗时：{levelTime:F1}秒 | 击杀敌人：{enemyKilledCount}");
    }

    /// <summary>
    /// 标记关卡失败
    /// </summary>
    public void FailLevel()
    {
        if (CurrentLevelState != E_LevelState.Running) return;

        CurrentLevelState = E_LevelState.Failed;
        OnLevelStateChanged?.Invoke(CurrentLevelState);
        OnLevelFailed?.Invoke();
        Time.timeScale = 1; // 恢复游戏时间
        Debug.Log($"关卡失败 | 核心塔是否存活：{isCoreTowerAlive}");
    }

    protected override void Awake()
    {
        base.Awake();
        InitLevelData(E_LevelType.TowerDefense, 1);
        if (CurrentLevelState == E_LevelState.Ready)
        {
            StartLevel();
        }
    }
    /// <summary>
    /// 更新关卡进度（每帧调用）
    /// </summary>
    private void Update()
    {
        if (CurrentLevelState == E_LevelState.Running)
        {
            levelTime += Time.deltaTime;
        } 

    }

    /// <summary>
    /// 获取关卡进度信息（供UI展示）
    /// </summary>
    /// <returns>格式化的进度字符串</returns>
    public string GetLevelProgressInfo()
    {
        return currentLevelType switch
        {
            E_LevelType.TowerDefense => $"塔防关卡{currentLevelID} | 波数：{waveCount}/{maxWaveCount} | 击杀：{enemyKilledCount}",
            E_LevelType.Survival => $"生存模式 | 耗时：{levelTime:F1}秒 | 击杀：{enemyKilledCount}",
            E_LevelType.Endless => $"无尽模式 | 波数：{waveCount} | 击杀：{enemyKilledCount}",
            _ => "未知关卡类型"
        };
    }

    /// <summary>
    /// 重置关卡控制器
    /// </summary>
    public void ResetController()
    {
        CurrentLevelState = E_LevelState.Uninitialized;
        levelTime = 0;
        enemyKilledCount = 0;
        waveCount = 0;
        Time.timeScale = 1;
        Debug.Log("关卡控制器已重置");
    }

}

/// <summary>
/// 通用单例模板
/// </summary>
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
            //DontDestroyOnLoad(gameObject); // 跨场景保留
        }
    }
}
