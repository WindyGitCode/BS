using UnityEngine;

public class TowerBase : MonoBehaviour
{
    [Header("塔等级配置")]
    public int currentLevel = 1;
    public int maxLevel = 3;

    [Header("升级价格")]
    public int[] upgradeCosts = { 100, 150 };

    [Header("升级后模型")]
    public GameObject[] levelPrefabs;

    private BuildPoint _ownBuildPoint;
    private TowerController _towerController;
    public string towerName;

    //初始化
    void Awake()
    {
        _towerController = GetComponent<TowerController>();
        if (_towerController != null)
        {
            // 刚生成就根据等级初始化数据
            _towerController.InitByLevel(currentLevel, towerName);
        }
        else
        {
            Debug.LogError("TowerBase 上缺少 TowerController 组件！");
        }
    }

    public void SetBuildPoint(BuildPoint bp)
    {
        _ownBuildPoint = bp;
    }

    public void TryUpgrade()
    {
        if (currentLevel >= maxLevel)
        {
            UIMgr.Instance.ShowPanel<TempTipPanel>().ShowTipAutoHide("已达最高等级！", 2f);
            return;
        }

        int cost = upgradeCosts[currentLevel - 1];
        if (GameDataMgr.Instance.GetGameData().gold < cost)
        {
            UIMgr.Instance.ShowPanel<TempTipPanel>().ShowTipAutoHide("金币不足！", 2f);
            return;
        }

        GameDataMgr.Instance.SpendMoney(cost);
        UpgradeTower();
    }

    void UpgradeTower()
    {
        currentLevel++;
        Vector3 pos = transform.position;
        Quaternion rot = transform.rotation;

        GameObject newTower = Instantiate(levelPrefabs[currentLevel - 2], pos, rot);
        TowerBase newTowerBase = newTower.GetComponent<TowerBase>();

        if (_ownBuildPoint != null)
        {
            _ownBuildPoint.OnTowerUpgraded(newTower, newTowerBase);
        }

        Destroy(gameObject);
        UIMgr.Instance.ShowPanel<TempTipPanel>().ShowTipAutoHide($"升级到 {currentLevel} 级！", 1.5f);
    }
}