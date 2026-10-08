using System.Collections.Generic;

/// <summary>
/// 怪物数据实体
/// </summary>
[System.Serializable]
public class MonsterConfig
{
    public int monsterID;       // 怪物ID
    public string monsterName;  // 怪物名称
    public int maxHP;           // 最大血量
    public int atk;             // 攻击力
    public float atkRange;      // 攻击范围
    public float moveSpeed;     // 移动速度
    public float rotateSpeed;   // 旋转速度
    public string objectPath;   // 预制体路径
    public string animationPath;// 动画控制器路径
    public float atkCD;       // 攻击间隔（秒）   
}

public class MonsterState
{
    public int currentHP;       // 当前血量（运行时使用，保存当前剩余血量)
}
/// <summary>
/// 用来包裹List，方便Json读写
/// </summary>
[System.Serializable]
public class MonsterDataList
{
    public List<MonsterConfig> monsterList;
}