using UnityEngine.Rendering;
public enum TaskType
{
    KillMonster,    // 击杀怪物
    FindChest,      // 寻找宝箱
    ReachWave,       // 到达第几波
    KillMonsterWithGrenade, // 使用手雷击杀怪物
}
[System.Serializable]
public class TaskData
{
    public int taskID;                // 任务ID
    public string taskName;           // 任务名
    public string desc;               // 任务描述
    public TaskType taskType;         // 任务类型
    public int targetCount;           // 目标数量（杀10只/开3个宝箱）
    public int rewardGold;            // 奖励金币
    public bool isCompleted;          // 是否完成
}