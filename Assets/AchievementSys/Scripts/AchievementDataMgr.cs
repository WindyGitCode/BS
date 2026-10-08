using System.Collections.Generic;
using UnityEngine;

public class AchievementDataMgr : Singleton<AchievementDataMgr>
{
    public List<AchievementData> achievementList;

    private Dictionary<string, AchievementData> _achievementDict;

    protected override void Awake()
    {
        base.Awake();
        _achievementDict = new Dictionary<string, AchievementData>();
        foreach (var data in achievementList)
        {
            _achievementDict[data.achievementID] = data;
        }
    }

    public AchievementData GetAchievement(string id)
    {
        _achievementDict.TryGetValue(id, out var data);
        return data;
    }

    public List<AchievementData> GetAllAchievements() => achievementList;
}