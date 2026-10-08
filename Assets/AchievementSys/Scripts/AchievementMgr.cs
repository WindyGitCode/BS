using System.Collections.Generic;
using UnityEngine;

public class AchievementMgr : Singleton<AchievementMgr>
{
    public List<AchievementData> achievementList;
    public List<AchievementData> allAchievements => achievementList;

    private HashSet<string> _unlockedIds;
    private HashSet<string> _rewardTakenIds; // 已领取奖励的成就

    protected override void Awake()
    {
        base.Awake();
        _unlockedIds = new HashSet<string>();
        _rewardTakenIds = new HashSet<string>();
        InitAllAchievements();
        LoadUnlockedData();
        LoadRewardTakenData();
    }
    private void Start()
    {
        EventMgr.Instance.AddListener(EventConst.MonsterKilled, OnMonsterKilled);
    }
    #region 配置成就
    void InitAllAchievements()
    {
        achievementList = new List<AchievementData>();

        achievementList.Add(new AchievementData
        {
            achievementID = "Kill_100",
            title = "守卫者",
            desc = "累计击杀 100 名敌人",
            type = AchievementType.KillMonster,
            targetCount = 100,
            rewardGold = 500,
            iconLocked = Resources.Load<Sprite>("Sprite/kill0"),
            iconUnlocked = Resources.Load<Sprite>("Sprite/kill1")
        });
        achievementList.Add(new AchievementData
        {
            achievementID = "OpenChest",
            title = "摸金校尉",
            desc = "累计开启100个宝箱",
            type = AchievementType.OpenChest,
            targetCount = 100,
            rewardGold = 500,
            iconLocked = Resources.Load<Sprite>("Sprite/gold0"),
            iconUnlocked = Resources.Load<Sprite>("Sprite/gold1")
        });
        achievementList.Add(new AchievementData
        {
            achievementID = "ReachWave",
            title = "坚如磐石",
            desc = "在无尽模式抵达第100个波次",
            type = AchievementType.ReachWave,
            targetCount = 1,
            rewardGold = 500,
            iconLocked = Resources.Load<Sprite>("Sprite/rock0"),
            iconUnlocked = Resources.Load<Sprite>("Sprite/rock1")
        });
    }
    #endregion

    #region 外部调用 - 增加进度
    public void AddAchievementProgress(string achId, int add = 1)
    {
        if (IsUnlocked(achId)) return;

        var gameData = GameDataMgr.Instance.GetGameData();
        if (!gameData.achievementProgressDic.ContainsKey(achId))
        {
            gameData.achievementProgressDic[achId] = 0;
        }

        gameData.achievementProgressDic[achId] += add;
        GameDataMgr.Instance.SaveGameData();

        // 检测是否满足解锁条件
        CheckAchievementComplete(achId);
    }

    // 怪物死亡专用快捷调用
    public void OnMonsterKilled()
    {
        AddAchievementProgress("Kill_100", 1);
        Debug.Log("怪物被击杀，成就进度更新");
    }
    #endregion

    #region 成就检测 & 解锁
    void CheckAchievementComplete(string achId)
    {
        var data = GetAchievement(achId);
        if (data == null) return;

        int curPro = GetAchievementProgress(achId);
        if (curPro >= data.targetCount)
        {
            //解锁成就
            Unlock(achId);
            UIMgr.Instance.ShowPanel<TempTipPanel>().ShowTipAutoHide($"成就达成：{data.title}", 3f);
        }
    }

    public void Unlock(string id)
    {
        if (_unlockedIds.Contains(id)) return;

        _unlockedIds.Add(id);
        SaveUnlockedData();

        var ach = GetAchievement(id);
        if (ach == null) return;

        // 弹出提示
        TempTipPanel tip = UIMgr.Instance.ShowPanel<TempTipPanel>();
        tip.ShowTipAutoHide($"成就解锁：{ach.title}", 3f);
    }
    #endregion

    #region 奖励领取功能
    public bool TakeAchievementReward(string achId)
    {
        if (!IsUnlocked(achId)) return false;
        if (_rewardTakenIds.Contains(achId)) return false;

        var data = GetAchievement(achId);
        if (data == null) return false;

        // 增加金币
        GameDataMgr.Instance.GetGameData().gold += data.rewardGold;
        GameDataMgr.Instance.SaveGameData();
        UIMgr.Instance.ShowPanel<TempTipPanel>().ShowTipAutoHide($"获得{data.rewardGold}个金币！");

        // 标记已领取
        _rewardTakenIds.Add(achId);
        SaveRewardTakenData();

        return true;
    }

    public bool IsRewardTaken(string achId)
    {
        return _rewardTakenIds.Contains(achId);
    }
    #endregion

    #region 公共方法
    public bool IsUnlocked(string id)
    {
        return _unlockedIds.Contains(id);
    }

    public AchievementData GetAchievement(string id)
    {
        return achievementList.Find(a => a.achievementID == id);
    }

    // 获取当前进度
    public int GetAchievementProgress(string achId)
    {
        var gameData = GameDataMgr.Instance.GetGameData();
        gameData.achievementProgressDic.TryGetValue(achId, out int pro);
        return pro;
    }
    #endregion

    #region 数据持久化
    void LoadUnlockedData()
    {
        _unlockedIds = new HashSet<string>();
        var gameData = GameDataMgr.Instance.GetGameData();

        if (!string.IsNullOrEmpty(gameData.unlockedAchievements))
        {
            string[] ids = gameData.unlockedAchievements.Split(',');
            foreach (var id in ids)
            {
                if (!string.IsNullOrEmpty(id))
                    _unlockedIds.Add(id);
            }
        }
    }
    void SaveUnlockedData()
    {
        var gameData = GameDataMgr.Instance.GetGameData();
        gameData.unlockedAchievements = string.Join(",", _unlockedIds);
        GameDataMgr.Instance.SaveGameData();
    }

    // 奖励领取记录 存储
    void LoadRewardTakenData()
    {
        _rewardTakenIds = new HashSet<string>();
        var gd = GameDataMgr.Instance.GetGameData();

        if (!string.IsNullOrEmpty(gd.rewardTakenAchievements))
        {
            var ids = gd.rewardTakenAchievements.Split(',');
            foreach (var id in ids)
            {
                if (!string.IsNullOrEmpty(id))
                    _rewardTakenIds.Add(id);
            }
        }
    }
    void SaveRewardTakenData()
    {
        var gd = GameDataMgr.Instance.GetGameData();
        gd.rewardTakenAchievements = string.Join(",", _rewardTakenIds);
        GameDataMgr.Instance.SaveGameData();
    }
    #endregion
    void OnDestroy()
    {
        EventMgr.Instance.RemoveListener(EventConst.MonsterKilled, OnMonsterKilled);
    }
}