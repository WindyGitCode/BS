using UnityEngine;
using UnityEngine.UI;

// 道具枚举
public enum ItemType
{
    MedKit,
    StaminaPotion,
    TowerRepairKit,
    MainGunAmmo,
    HandGunAmmo,
    HeavyGunAmmo,
    ShotGunAmmo,
    SniperAmmo,
    Grenade
}

public class MarketPanel : BasePanel
{
    public Button back;

    [Header("金币显示")]
    public Text goldText;

    [Header("背包道具数量 Text (9个)")]
    public Text text_MedKit;
    public Text text_StaminaPotion;
    public Text text_TowerRepairKit;
    public Text text_MainGunAmmo;
    public Text text_HandGunAmmo;
    public Text text_HeavyGunAmmo;
    public Text text_ShotGunAmmo;
    public Text text_SniperAmmo;
    public Text text_Grenade;

    [Header("商店购买 Button (9个)")]
    public Button btn_MedKit;
    public Button btn_StaminaPotion;
    public Button btn_TowerRepairKit;
    public Button btn_MainGunAmmo;
    public Button btn_HandGunAmmo;
    public Button btn_HeavyGunAmmo;
    public Button btn_ShotGunAmmo;
    public Button btn_SniperAmmo;
    public Button btn_Grenade;

    private GameData gameData;

    public override void Init()
    {
        gameData = GameDataMgr.Instance.GetGameData();

        // 返回按钮
        back.onClick.AddListener(() =>
        {
            AudioMgr.Instance.PlayBtnClick();
            UIMgr.Instance.ShowPanel<BeginPanel>();
            UIMgr.Instance.HidePanel<MarketPanel>();
        });

        // 绑定所有购买按钮
        BindAllShopButtons();

        // 刷新UI
        RefreshUI();
    }

    private void BindAllShopButtons()
    {
        // 格式：购买类型, 获得数量, 花费金币
        btn_MedKit.onClick.AddListener(() => BuyItem(ItemType.MedKit, 1, 10));
        btn_StaminaPotion.onClick.AddListener(() => BuyItem(ItemType.StaminaPotion, 1, 15));
        btn_TowerRepairKit.onClick.AddListener(() => BuyItem(ItemType.TowerRepairKit, 1, 30));

        btn_MainGunAmmo.onClick.AddListener(() => BuyItem(ItemType.MainGunAmmo, 30, 50));
        btn_HandGunAmmo.onClick.AddListener(() => BuyItem(ItemType.HandGunAmmo, 30, 60));
        btn_HeavyGunAmmo.onClick.AddListener(() => BuyItem(ItemType.HeavyGunAmmo, 30, 40));
        btn_ShotGunAmmo.onClick.AddListener(() => BuyItem(ItemType.ShotGunAmmo, 30, 100));
        btn_SniperAmmo.onClick.AddListener(() => BuyItem(ItemType.SniperAmmo, 30, 100));
        btn_Grenade.onClick.AddListener(() => BuyItem(ItemType.Grenade, 1, 15));
    }

    /// <summary>
    /// 购买函数
    /// </summary>
    public bool BuyItem(ItemType itemType, int addCount, int costGold)
    {
        AudioMgr.Instance.PlayBtnClick();

        bool costSuccess = GameDataMgr.Instance.SpendMoney(costGold);
        if (!costSuccess)
        {
            UIMgr.Instance.ShowPanel<TempTipPanel>().ShowTip3AutoHide("金币不足!", 1.2f);
            return false;
        }

        // 增加道具
        switch (itemType)
        {
            case ItemType.MedKit: gameData.MedKit += addCount; break;
            case ItemType.StaminaPotion: gameData.StaminaPotion += addCount; break;
            case ItemType.TowerRepairKit: gameData.TowerRepairKit += addCount; break;
            case ItemType.MainGunAmmo: gameData.MainGunAmmo += addCount; break;
            case ItemType.HandGunAmmo: gameData.HandGunAmmo += addCount; break;
            case ItemType.HeavyGunAmmo: gameData.HeavyGunAmmo += addCount; break;
            case ItemType.ShotGunAmmo: gameData.ShotGunAmmo += addCount; break;
            case ItemType.SniperAmmo: gameData.SniperAmmo += addCount; break;
            case ItemType.Grenade: gameData.Grenade += addCount; break;
        }

        // 保存数据
        GameDataMgr.Instance.SaveGameData();

        // 刷新显示
        RefreshUI();

        UIMgr.Instance.ShowPanel<TempTipPanel>().ShowTip3AutoHide("购买成功!", 1.2f);
        return true;
    }

    /// <summary>
    /// 刷新所有UI显示
    /// </summary>
    public void RefreshUI()
    {
        goldText.text = "￥:"+gameData.gold.ToString();

        text_MedKit.text = gameData.MedKit.ToString();
        text_StaminaPotion.text = gameData.StaminaPotion.ToString();
        text_TowerRepairKit.text = gameData.TowerRepairKit.ToString();
        text_MainGunAmmo.text = gameData.MainGunAmmo.ToString();
        text_HandGunAmmo.text = gameData.HandGunAmmo.ToString();
        text_HeavyGunAmmo.text = gameData.HeavyGunAmmo.ToString();
        text_ShotGunAmmo.text = gameData.ShotGunAmmo.ToString();
        text_SniperAmmo.text = gameData.SniperAmmo.ToString();
        text_Grenade.text = gameData.Grenade.ToString();
    }
}