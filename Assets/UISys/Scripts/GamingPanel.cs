using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GamingPanel : BasePanel
{
    //图片
    public Image towerBlood;
    public Image playerBlood;
    public Image strengh;
    //塔血量
    public Text txt_towerHP;
    //玩家血量
    public Text txt_playerHP;
    //玩家体力
    public Text txt_Strengh;
    //武器信息
    public Image weaponImage;
    public Text txt_weaponName;
    public Text txt_bullet;
    // 道具相关
    public Text hpItemCountText;    // 医疗包数量
    public Text spItemCountText;    // 体力剂数量
    public Image hpItemCoolMask;    // 医疗包冷却遮罩
    public Image spItemCoolMask;    // 体力剂冷却遮罩
    //波数和金币
    public Text txt_wave;
    public Text txt_gold;
    //其他（按需添加）
    private float maxTowerBloodWidth;
    private float maxPlayerBloodWidth;
    [Header("需要隐藏的UI物体")]
    public List<GameObject> objectsToHide; 
    private GameData gameData;
    public Image skillCoolMask;    // 技能冷却遮罩
    [Header("暴击击杀UI")]
    public GameObject critKillUI;
    private Coroutine showCritKillCoroutine;

    public override void Init()
    {
        if (towerBlood != null)
            maxTowerBloodWidth = towerBlood.rectTransform.sizeDelta.x;
        if (playerBlood != null)
            maxPlayerBloodWidth = playerBlood.rectTransform.sizeDelta.x;
        gameData = GameDataMgr.Instance.GetGameData();
        UpdatePlayerBlood(PlayerDataMgr.Instance.GetPlayerConfig().maxHealth, PlayerDataMgr.Instance.GetPlayerConfig().maxHealth);
        UpdatePlayerStrengh(PlayerDataMgr.Instance.GetPlayerConfig().maxEnergy, PlayerDataMgr.Instance.GetPlayerConfig().maxEnergy);
        UpdateGold();

        // 初始化准星数据
        if (crosshair1 != null)
        {
            defaultCrossSize1 = crosshair1.sizeDelta.x;
            defaultCrossSize2 = crosshair1.sizeDelta.y;
            currentSpread = 0;
        }
        maxSpread = 8000f;            // 最大扩散值
        spreadPerShot = 100f;         // 每枪增加多少扩散
        spreadDecay = 3f;           // 扩散自然回落速度

         //监听事件
        EventMgr.Instance.AddListener<int,int>(EventConst.UpdatePlayerHP, UpdatePlayerBlood);
        EventMgr.Instance.AddListener<float, float>(EventConst.UpdatePlayerEnergy, UpdatePlayerStrengh);
        EventMgr.Instance.AddListener<int, int>(EventConst.WaveUpdated,UpdateWave);
        EventMgr.Instance.AddListener(EventConst.GameDataChange, UpdateGold);

        // 物品数量和冷却进度监听
        EventMgr.Instance.AddListener<int, int>("Update_Item_Count", UpdateItemCount);
        EventMgr.Instance.AddListener<float>("Update_HP_Item_CD", UpdateHPItemCool);
        EventMgr.Instance.AddListener<float>("Update_SP_Item_CD", UpdateSPItemCool);

        //技能UI监听
        EventMgr.Instance.AddListener<float>("Update_Skill_CD", UpdateSkillCool);
        EventMgr.Instance.AddListener(EventConst.HeadShotKill, OnCritKill);

        //放置到最前
        transform.SetAsFirstSibling();
        
    }
    public override void Update()
    {
        base.Update();
        // 准星自动缓慢回弹
        currentSpread = Mathf.Lerp(currentSpread, 0, Time.deltaTime * spreadDecay);
        float size = defaultCrossSize1 + currentSpread;
        crosshair1.sizeDelta = new Vector2(defaultCrossSize1, size);
        crosshair2.sizeDelta = new Vector2(size, defaultCrossSize2);
    }
    public void UpdateTowerBlood(int nowHP,int maxHP)
    {
        //血条长度更新
        float ratio = (float)nowHP / maxHP;
        towerBlood.rectTransform.sizeDelta = new Vector2(ratio * maxTowerBloodWidth, towerBlood.rectTransform.sizeDelta.y);
        //血量文本更新
        txt_towerHP.text =string.Format("{0}/{1}", nowHP, maxHP);
    }
    public void UpdatePlayerBlood(int nowHP, int maxHP)
    {
        //血条长度更新
        float ratio = (float)nowHP / maxHP;
        playerBlood.rectTransform.sizeDelta = new Vector2(ratio * maxPlayerBloodWidth, playerBlood.rectTransform.sizeDelta.y);
        //血量文本更新
        txt_playerHP.text = string.Format("{0}/{1}", nowHP, maxHP);
    }
    public void UpdatePlayerStrengh(float nowStrengh, float maxStrengh)
    {
        //体力条长度更新
        float ratio = (float)nowStrengh / maxStrengh;
        strengh.rectTransform.sizeDelta = new Vector2(ratio * maxPlayerBloodWidth, strengh.rectTransform.sizeDelta.y);
        //体力文本更新
        txt_Strengh.text = string.Format("{0}/{1}", (int)nowStrengh, maxStrengh);
    }
    public void UpdateWeaponUI(Sprite icon,string name, string bulletText)
    {
        weaponImage.sprite= icon;
        txt_weaponName.text = name;
        txt_bullet.text = bulletText;
    }
    public void UpdateWave(int nowWave, int maxWave)
    {
        Debug.Log("面板波次更新");
        //波数文本更新
        txt_wave.text = $"剩余波次：{nowWave}/{maxWave}";
    }
    //外部调用
    public void UpdateGold()
    {
        txt_gold.text = $"金币：{gameData.gold}";
    }

    #region 道具相关
    // 刷新数量
    private void UpdateItemCount(int hpCount, int spCount)
    {
        hpItemCountText.text = hpCount.ToString();
        spItemCountText.text = spCount.ToString();
    }

    // 医疗包冷却（0~1）
    private void UpdateHPItemCool(float progress)
    {
        hpItemCoolMask.fillAmount = progress;
    }

    // 体力剂冷却
    private void UpdateSPItemCool(float progress)
    {
        spItemCoolMask.fillAmount = progress;
        Debug.Log(progress);
    }
    #endregion
    private void OnDestroy()
    {
        EventMgr.Instance.RemoveListener<int, int>(EventConst.UpdatePlayerHP, UpdatePlayerBlood);
        EventMgr.Instance.RemoveListener<int, int>(EventConst.WaveUpdated, UpdateWave);
        EventMgr.Instance.RemoveListener<float, float>(EventConst.UpdatePlayerEnergy, UpdatePlayerStrengh);
        EventMgr.Instance.RemoveListener(EventConst.GameDataChange, UpdateGold);
        EventMgr.Instance.RemoveListener<int, int>("Update_Item_Count", UpdateItemCount);
        EventMgr.Instance.RemoveListener<float>("Update_HP_Item_CD", UpdateHPItemCool);
        EventMgr.Instance.RemoveListener<float>("Update_SP_Item_CD", UpdateSPItemCool);
        EventMgr.Instance.RemoveListener<float>("Update_Skill_CD", UpdateSkillCool);
        EventMgr.Instance.RemoveListener(EventConst.HeadShotKill, OnCritKill);
    }

    //技能冷却更新
    private void UpdateSkillCool(float progress)
    {
        skillCoolMask.fillAmount = progress;
    }

    //生存模式隐藏主塔血量UI
    public void HideAssignedObjects()
    {
        foreach (var obj in objectsToHide)
        {
            if (obj != null)
                obj.SetActive(false);
        }
    }

    #region 准星系统
    [Header("准星设置")]
    public RectTransform crosshair1;          // 拖拽赋值你的准星1
    public RectTransform crosshair2;          // 拖拽赋值你的准星2
    public float defaultCrossSize1;          // 准星1原始大小
    public float defaultCrossSize2;          // 准星2原始大小
    public float maxSpread;                 // 最大扩散值
    public float spreadPerShot;       // 每枪增加多少扩散
    public float spreadDecay;           // 扩散自然回落速度
    private float currentSpread;
    //连发持续扩散
    public void AddSpread()
    {
        currentSpread += spreadPerShot;
        currentSpread = Mathf.Min(currentSpread, maxSpread);
    }

    // 单发一次性跳变
    public void SingleShotSpread()
    {
        currentSpread += spreadPerShot;
        currentSpread = Mathf.Min(currentSpread, maxSpread);
    }
    #endregion

    //暴击UI显示
    private void OnCritKill()
    {
        ShowCritKillUI(2f); // 显示2秒
    }

    public void ShowCritKillUI(float delay)
    {
        if (critKillUI == null) return;

        // 先关掉上次协程，防止重复显示
        if (showCritKillCoroutine != null)
            StopCoroutine(showCritKillCoroutine);

        showCritKillCoroutine = StartCoroutine(ShowCritUIDelay(delay));
    }

    private IEnumerator ShowCritUIDelay(float delay)
    {
        critKillUI.SetActive(true);
        yield return new WaitForSeconds(delay);
        critKillUI.SetActive(false);
    }
}
