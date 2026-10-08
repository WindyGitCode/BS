using System;
using System.Collections.Generic;
public static class EventConst
{
    // 玩家受伤
    public const string PlayerTakeDamage = "PlayerTakeDamage";
    //更新玩家体力
    public const string UpdatePlayerEnergy = "UpdatePlayerEnergy";
    // 更新玩家血量
    public const string UpdatePlayerHP = "UpdatePlayerHP";
    // 玩家死亡
    public const string PlayerDeath = "PlayerDeath";
    //主塔被摧毁
    public const string TowerDestroyed = "TowerDestroyed";
    //波数更新
    public const string WaveUpdated = "WaveUpdated";
    //怪物死亡
    public const string MonsterKilled = "MonsterKilled";
    //安全时间开启
    public const string SurvivalSafeTimeStart = "SurvivalSafeTimeStart";
    //隐藏npc
    public const string HideSurvivalNPC= "HideSurvivalNPC";
    //手雷击杀怪物
    public const string GrenadeKillMonster = "GrenadeKillMonster";
    //加子弹
    public const string AddMainGunAmmo = "AddMainGunAmmo";
    //加手雷
    public const string AddGrenade = "AddGrenade";
    //点击按钮
    public const string ButtonClicked = "ButtonClicked";
    //物资数量变化
    public const string GameDataChange = "GameDataChange";
    //游戏胜利
    public const string WinGame = "WinGame";
    //暴击击杀
    public const string HeadShotKill = "HeadShotKill";
    //初始化关卡
    public const string FinishDirection = "FinishDirection";
}
public class EventMgr
{
    // 单例
    private static EventMgr _instance;
    public static EventMgr Instance
    {
        get
        {
            if (_instance == null)
                _instance = new EventMgr();
            return _instance;
        }
    }

    // 无参
    private Dictionary<string, Action> _eventDict = new Dictionary<string, Action>();

    // 1参数
    private Dictionary<string, Delegate> _event1Dict = new Dictionary<string, Delegate>();

    // 2参数
    private Dictionary<string, Delegate> _event2Dict = new Dictionary<string, Delegate>();

    // 3参数
    private Dictionary<string, Delegate> _event3Dict = new Dictionary<string, Delegate>();

    #region 无参事件
    public void AddListener(string eventName, Action callback)
    {
        if (!_eventDict.ContainsKey(eventName))
            _eventDict.Add(eventName, null);

        _eventDict[eventName] -= callback; // 防重复注册
        _eventDict[eventName] += callback;
    }

    public void RemoveListener(string eventName, Action callback)
    {
        if (_eventDict.ContainsKey(eventName))
            _eventDict[eventName] -= callback;
    }

    public void Trigger(string eventName)
    {
        if (_eventDict.TryGetValue(eventName, out Action action))
            action?.Invoke();
    }
    #endregion

    #region 1参数事件
    public void AddListener<T>(string eventName, Action<T> callback)
    {
        if (!_event1Dict.ContainsKey(eventName))
            _event1Dict.Add(eventName, null);

        _event1Dict[eventName] = Delegate.Remove(_event1Dict[eventName], callback);
        _event1Dict[eventName] = Delegate.Combine(_event1Dict[eventName], callback);
    }

    public void RemoveListener<T>(string eventName, Action<T> callback)
    {
        if (_event1Dict.ContainsKey(eventName))
            _event1Dict[eventName] = Delegate.Remove(_event1Dict[eventName], callback);
    }

    public void Trigger<T>(string eventName, T arg1)
    {
        if (_event1Dict.TryGetValue(eventName, out Delegate del) && del is Action<T> action)
            action?.Invoke(arg1);
    }
    #endregion

    #region 2参数事件
    public void AddListener<T1, T2>(string eventName, Action<T1, T2> callback)
    {
        if (!_event2Dict.ContainsKey(eventName))
            _event2Dict.Add(eventName, null);

        _event2Dict[eventName] = Delegate.Remove(_event2Dict[eventName], callback);
        _event2Dict[eventName] = Delegate.Combine(_event2Dict[eventName], callback);
    }

    public void RemoveListener<T1, T2>(string eventName, Action<T1, T2> callback)
    {
        if (_event2Dict.ContainsKey(eventName))
            _event2Dict[eventName] = Delegate.Remove(_event2Dict[eventName], callback);
    }

    public void Trigger<T1, T2>(string eventName, T1 arg1, T2 arg2)
    {
        if (_event2Dict.TryGetValue(eventName, out var del) && del is Action<T1, T2> action)
            action?.Invoke(arg1, arg2);
    }
    #endregion

    #region 3参数事件
    public void AddListener<T1, T2, T3>(string eventName, Action<T1, T2, T3> callback)
    {
        if (!_event3Dict.ContainsKey(eventName))
            _event3Dict.Add(eventName, null);

        _event3Dict[eventName] = Delegate.Remove(_event3Dict[eventName], callback);
        _event3Dict[eventName] = Delegate.Combine(_event3Dict[eventName], callback);
    }

    public void RemoveListener<T1, T2, T3>(string eventName, Action<T1, T2, T3> callback)
    {
        if (_event3Dict.ContainsKey(eventName))
            _event3Dict[eventName] = Delegate.Remove(_event3Dict[eventName], callback);
    }

    public void Trigger<T1, T2, T3>(string eventName, T1 arg1, T2 arg2, T3 arg3)
    {
        if (_event3Dict.TryGetValue(eventName, out var del) && del is Action<T1, T2, T3> action)
            action?.Invoke(arg1, arg2, arg3);
    }
    #endregion

    #region 清理
    public void ClearAll()
    {
        _eventDict.Clear();
        _event1Dict.Clear();
        _event2Dict.Clear();
        _event3Dict.Clear();
    }

    public void ClearEvent(string eventName)
    {
        _eventDict.Remove(eventName);
        _event1Dict.Remove(eventName);
        _event2Dict.Remove(eventName);
        _event3Dict.Remove(eventName);
    }
    #endregion
}