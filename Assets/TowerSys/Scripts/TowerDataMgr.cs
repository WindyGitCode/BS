public class TowerDataMgr
{
    /// <summary>
    /// 根据等级和炮塔名获取炮台数据
    /// </summary>
    public static TowerData GetTowerDataByLevel(int level,string towerName)
    {
        //TowerData1_1中前数字代表炮塔类型，后数字代表等级
        TowerData TowerData1_1 = new TowerData
        {
            towerName = "1",
            attackRange = 6.5f,
            attackInterval = 0.6f,
            damage = 34,
            bulletSpeed = 20f
        };
        TowerData TowerData1_2 = new TowerData
        {
            towerName = "1",
            attackRange = 6.5f,
            attackInterval = 0.5f,
            damage = 34,
            bulletSpeed = 20f
        };
        TowerData TowerData1_3 = new TowerData
        {
            towerName = "1",
            attackRange = 6.5f,
            attackInterval = 0.4f,
            damage = 34,
            bulletSpeed = 20f
        };
        TowerData TowerData2_1 = new TowerData
        {
            towerName = "2",
            attackRange = 6.5f,
            attackInterval = 0.7f,
            damage = 34,
            bulletSpeed = 20f
        };
        TowerData TowerData2_2 = new TowerData
        {
            towerName = "2",
            attackRange = 6.5f,
            attackInterval = 0.6f,
            damage = 34,
            bulletSpeed = 20f
        };
        TowerData TowerData2_3 = new TowerData
        {
            towerName = "2",
            attackRange = 6.5f,
            attackInterval = 0.5f,
            damage = 34,
            bulletSpeed = 20f
        };
        TowerData TowerData3_1 = new TowerData
        {
            towerName = "3",
            attackRange = 6.5f,
            attackInterval = 1.3f,
            damage = 35,
            bulletSpeed = 20f
        };
        TowerData TowerData3_2 = new TowerData
        {
            towerName = "3",
            attackRange = 6.5f,
            attackInterval = 1f,
            damage = 35,
            bulletSpeed = 20f
        };
        TowerData TowerData3_3 = new TowerData
        {
            towerName = "3",
            attackRange = 6.5f,
            attackInterval = 1f,
            damage = 50,
            bulletSpeed = 20f
        };

        switch (level)
        {
            case 1:
                if (towerName == "1")
                    return TowerData1_1;
                else if (towerName == "2")
                    return TowerData2_1;
                else if (towerName == "3")
                    return TowerData3_1;
                else
                    return GetTowerDataByLevel(1, "1");
            case 2:
                 if (towerName == "1")
                    return TowerData1_2;
                 else if (towerName == "2")
                    return TowerData2_2;
                 else if (towerName == "3")
                    return TowerData3_2;
                 else
                    return GetTowerDataByLevel(1, "2");
            case 3:
                  if (towerName == "1")
                    return TowerData1_3;
                else if (towerName == "2")
                    return TowerData2_3;
                else if (towerName == "3")
                    return TowerData3_3;
                else
                    return GetTowerDataByLevel(1, "3");
            default:
                 return GetTowerDataByLevel(1, "1");
        }
    }
    public static TowerData GetDefaultTowerData()
    {
        return GetTowerDataByLevel(1, "1");
    }
}