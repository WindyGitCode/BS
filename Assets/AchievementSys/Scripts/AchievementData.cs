// ==============================
// 成就数据
// ==============================
using UnityEngine;

[System.Serializable]
public class AchievementData
{
    public string achievementID;
    public string title;
    public string desc;
    public Sprite iconLocked;
    public Sprite iconUnlocked;
    public int rewardGold;

    public AchievementType type;
    public int targetCount;
}

public enum AchievementType
{
    KillMonster,
    ReachWave,
    OpenChest,
    CompleteTask,
    SurviveSafeTime,
    WinGame
}