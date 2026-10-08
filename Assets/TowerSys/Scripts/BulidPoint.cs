using UnityEngine;

public class BuildPoint : Interactable
{
    [Header("配置")]
    public bool isBuilt = false;
    public GameObject currentTower;
    public TowerBase currentTowerBase;

    public override void OnInteract()
    {
        if (!isBuilt)
        {
            MakeTowerPanel panel = UIMgr.Instance.ShowPanel<MakeTowerPanel>();
            CursorMgr.ShowMouse();
            panel.SetBuildPoint(this);
        }
        else
        {
            if (currentTowerBase != null)
            {
                currentTowerBase.TryUpgrade();
            }
        }
    }

    // 建造塔
    public void BuildTower(GameObject towerPrefab, int cost)
    {
        if (GameDataMgr.Instance.GetGameData().gold < cost)
        {
            UIMgr.Instance.ShowPanel<TempTipPanel>().ShowTipAutoHide("金币不足！", 2f);
            return;
        }

        GameDataMgr.Instance.SpendMoney(cost);
        currentTower = Instantiate(towerPrefab, transform.position, transform.rotation);
        currentTowerBase = currentTower.GetComponent<TowerBase>();

        //告诉塔它属于哪个建造点
        if (currentTowerBase != null)
            currentTowerBase.SetBuildPoint(this);

        isBuilt = true;
        UIMgr.Instance.ShowPanel<TempTipPanel>().ShowTipAutoHide("建造成功！", 1.5f);
    }

    //升级后由塔调用，安全更新引用
    public void OnTowerUpgraded(GameObject newTower, TowerBase newTowerBase)
    {
        currentTower = newTower;
        currentTowerBase = newTowerBase;

        // 告诉新塔它属于这个建造点
        if (currentTowerBase != null)
            currentTowerBase.SetBuildPoint(this);
    }
}