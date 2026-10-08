using UnityEngine;
using UnityEngine.UI;

public class MakeTowerPanel : BasePanel
{
    [Header("UI绑定")]
    public Image[] towerIcons;       // 3个塔图标
    public Text[] towerNames;        // 3个塔名称
    public Text[] towerCosts;        // 3个价格
    public Button[] buildBtns;       // 3个建造按钮
    public Button btnClose;          // 关闭按钮

    private BuildPoint currentBuildPoint;

    // 只定义，不在构造时加载
    private TowerInfo[] towerList;

    [System.Serializable]
    public class TowerInfo
    {
        public string towerName;
        public int cost;
        public Sprite icon;
        public GameObject prefab;
    }

    public override void Init()
    {
        // 【修复】在这里初始化塔数据，避开构造函数调用Resources
        InitTowerData();

        // 关闭按钮
        btnClose.onClick.AddListener(() =>
        {
            AudioMgr.Instance.PlayBtnClick();
            CursorMgr.HideMouse();
            UIMgr.Instance.HidePanel<MakeTowerPanel>();
        });

        // 绑定建造按钮
        for (int i = 0; i < buildBtns.Length; i++)
        {
            int index = i;
            buildBtns[i].onClick.AddListener(() => OnBuildClick(index));
        }

        // 刷新UI
        RefreshUI();
    }

    /// <summary>
    /// 初始化三种塔的数据配置
    /// </summary>
    void InitTowerData()
    {
        towerList = new TowerInfo[]
        {
            new TowerInfo
            {
                towerName = "机关炮",
                cost = 100,
                icon = Resources.Load<Sprite>("TowerIcon/Icon1"),
                prefab = Resources.Load<GameObject>("Tower/Tower1_Lv1")
            },
            new TowerInfo
            {
                towerName = "电磁炮",
                cost = 180,
                icon = Resources.Load<Sprite>("TowerIcon/Icon2"),
                prefab = Resources.Load<GameObject>("Tower/Tower2_Lv1")
            },
            new TowerInfo
            {
                towerName = "榴弹炮",
                cost = 260,
                icon = Resources.Load<Sprite>("TowerIcon/Icon3"),
                prefab = Resources.Load<GameObject>("Tower/Tower3_Lv1")
            }
        };
    }

    // 设置当前建造点
    public void SetBuildPoint(BuildPoint point)
    {
        currentBuildPoint = point;
        RefreshUI();
    }

    // 刷新面板显示
    void RefreshUI()
    {
        if (towerList == null) return;

        for (int i = 0; i < towerList.Length; i++)
        {
            towerNames[i].text = towerList[i].towerName;
            towerCosts[i].text = towerList[i].cost + " 金币";
            towerIcons[i].sprite = towerList[i].icon;
        }
    }

    // 点击建造
    void OnBuildClick(int index)
    {
        if (currentBuildPoint == null) return;
        AudioMgr.Instance.PlayBtnClick();

        var info = towerList[index];
        currentBuildPoint.BuildTower(info.prefab, info.cost);
        UIMgr.Instance.HidePanel<MakeTowerPanel>();
        CursorMgr.HideMouse();
    }
}