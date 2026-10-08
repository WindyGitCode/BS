using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private static PlayerController instance;
    public static PlayerController Instance => instance;

    // 当前角色的配置
    public PlayerConfig playerConfig;
    //角色控制相关
    public float rotateSpeed;   //水平旋转速度
    public float verticalLookSpeed;    // 垂直旋转速度
    public float minVerticalAngle;    // 最低俯视角
    public float maxVerticalAngle;     // 最高仰视角
    private float currentVerticalAngle;  // 当前垂直角度

    //组件
    public Animator animator;//动画组件
    private AudioSource audioSource;  // 音效组件

    //物体数量
    public int HPItem;//医疗包数量
    public int SpItem;//体力剂数量

    //键位
    public KeyCode RollKey;//翻滚键位
    public KeyCode HPItemKey;//使用血包键位
    public KeyCode SPItemKey;//使用体力剂键位

    [Header("道具冷却")]
    public float itemCooldown = 5f; // 统一冷却5秒
    private float hpItemCoolTimer;
    private float spItemCoolTimer;
    private bool canUseHPItem = true;
    private bool canUseSPItem = true;

    //其他
    private float screenNormalizedFactor;//适配屏幕分辨率的归一化系数
    private AudioClip damageSound;//受伤音效
    private float damageTimer;//音效播放计时器
    private GameData gameData;
    void Start()
    {
        //常规
        instance = this;
        gameData=GameDataMgr.Instance.GetGameData();
        screenNormalizedFactor = Screen.width / 1920f;
        CursorMgr.HideMouse();
        //摄像机数据
        rotateSpeed = 80;
        verticalLookSpeed = 80f;
        minVerticalAngle = -45f;
        maxVerticalAngle = 60f;
        currentVerticalAngle = 0f;
        //角色数据初始化
        playerConfig = PlayerDataMgr.Instance.GetPlayerConfig();
        PlayerDataMgr.Instance.playerState.currentHealth = playerConfig.maxHealth;//初始血量等于最大血量
        PlayerDataMgr.Instance.playerState.currentEnergy = playerConfig.maxEnergy;//初始体力等于最大体力
        PlayerDataMgr.Instance.playerState.isDead = false;//初始状态未死亡
        damageSound = Resources.Load<AudioClip>("Audio/playerDamage");//音效加载
        if (damageSound == null)
            Debug.Log("未加载到damageSound");
        //组件获取
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        rb = GetComponent<Rigidbody>();

        //物品数量获取
        HPItem = gameData.MedKit;
        SpItem= gameData.StaminaPotion;

        //键位获取
        RollKey=gameData.rollKey;
        HPItemKey=gameData.useHPItemKey;
        SPItemKey=gameData.useSPItemKey;

        // 事件监听
        EventMgr.Instance.AddListener<MonsterConfig>(EventConst.PlayerTakeDamage, (mo) => { TakeDamage(mo.atk); });
        EventMgr.Instance.AddListener(EventConst.WinGame, SetItem);

        //刷新物品UI
        RefreshItemUI();
    }

    void Update()
    {
        //游戏暂停时或角色死亡时不响应输入
        if (GamePauseMgr.Instance.isGamePause||PlayerDataMgr.Instance.playerState.isDead)
            return;
        //移动输入
        animator.SetFloat("Vertical", Input.GetAxis("Vertical"));
        animator.SetFloat("Horizontal", Input.GetAxis("Horizontal"));

        //角色水平旋转
        transform.Rotate(Vector3.up, Input.GetAxis("Mouse X") * rotateSpeed * screenNormalizedFactor * Time.deltaTime);

        // 垂直旋转
        float mouseY = Input.GetAxis("Mouse Y") * verticalLookSpeed * Time.deltaTime;
        currentVerticalAngle -= mouseY;
        currentVerticalAngle = Mathf.Clamp(currentVerticalAngle, minVerticalAngle, maxVerticalAngle);
        // 应用垂直旋转到相机跟随点
        Transform lookAtPoint = transform.Find("LookAtPoint");
        if (lookAtPoint != null)
        {
            lookAtPoint.localEulerAngles = new Vector3(currentVerticalAngle, 0, 0);
        }

        //翻滚
        RollInput();
        //武器切换输入处理
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll > 0f) WeaponController.Instance.ChangeLastWeapon();
        else if (scroll < 0f) WeaponController.Instance.ChangeNextWeapon();
        //体力自动恢复
        StrengthAutoAdd();

        //技能相关
        if (Input.GetKeyDown(gameData.skillKey) && canUseSkill)
        {
            UseCurrentHeroSkill();
        }
        // 技能冷却倒计时
        if (!canUseSkill)
        {
            skillCoolTimer -= Time.deltaTime;
            if (skillCoolTimer <= 0)
            {
                canUseSkill = true;
            }
        }
        // 技能冷却
        UpdateSkillCooldown();

        //使用道具
        if (Input.GetKeyDown(HPItemKey)) UseHPItem();
        if (Input.GetKeyDown(SPItemKey)) UseSPItem();
        //道具倒计时
        UpdateItemCooldown();

        //计时器累加
        damageTimer += Time.deltaTime;
    }


    #region 生命系统
    public void TakeDamage(int damage)
    {
        if(PlayerDataMgr.Instance.playerState.isDead == true)
        {
            return;
        }
        //播放受伤音效
        if (damageTimer > 3)
        {
            damageTimer = 0;
            AudioMgr.Instance.PlaySFX(damageSound);
        }
        PlayerDataMgr.Instance.playerState.currentHealth -= damage;
        if (PlayerDataMgr.Instance.playerState.currentHealth < 0)
            PlayerDataMgr.Instance.playerState.currentHealth = 0;

        if (PlayerDataMgr.Instance.playerState.currentHealth <= 0)
        {
            Dead();
            EventMgr.Instance.Trigger(EventConst.PlayerDeath);
        }
        //更新面板血量
        EventMgr.Instance.Trigger<int, int>(EventConst.UpdatePlayerHP, PlayerDataMgr.Instance.playerState.currentHealth, playerConfig.maxHealth);
    }

    public void Dead()
    {
        PlayerDataMgr.Instance.playerState.isDead = true;
        animator.SetBool("die",true);
        EventMgr.Instance.Trigger("PlayerDeath");
    }
    #endregion

    #region 翻滚和体力回复
    //翻滚输入
    private void RollInput()
    {
        if (Input.GetKeyDown(RollKey))
        {
            if(PlayerDataMgr.Instance.playerState.currentEnergy > 30)
            {
                 animator.SetBool("Roll", true);
                PlayerDataMgr.Instance.playerState.currentEnergy -= 30;
                 EventMgr.Instance.Trigger<float, float>(EventConst.UpdatePlayerEnergy, PlayerDataMgr.Instance.playerState.currentEnergy, playerConfig.maxEnergy);
            }
        }
    }

    //体力自动恢复
    private float energyRefreshTimer = 0f;
    private void StrengthAutoAdd()
    {
        if(PlayerDataMgr.Instance.playerState.currentEnergy < playerConfig.maxEnergy)
        {
            PlayerDataMgr.Instance.playerState.currentEnergy += 3 * Time.deltaTime;//每秒恢复3点体力
            if (PlayerDataMgr.Instance.playerState.currentEnergy > playerConfig.maxEnergy)
                PlayerDataMgr.Instance.playerState.currentEnergy = playerConfig.maxEnergy;
            energyRefreshTimer += Time.deltaTime;
            if (energyRefreshTimer >= 1)//每秒刷新一次UI
            {
                EventMgr.Instance.Trigger<float, float>(EventConst.UpdatePlayerEnergy, PlayerDataMgr.Instance.playerState.currentEnergy, playerConfig.maxEnergy);
                energyRefreshTimer = 0f; // 重置计时器
            }
        }
        else
        {
             energyRefreshTimer = 0f;
        }
    }
    #endregion

    #region 技能系统核心
    [Header("技能设置")]
    public float backJumpPower = 100f;     // 后退力度
    public float jumpPower = 5f;         // 起跳高度
    public float skillCooldown = 5f;     // 冷却时间
    private float skillCoolTimer;
    private bool canUseSkill = true;

    private Rigidbody rb; // 缓存刚体，避免频繁GetComponent

    // 警官专属：向后跳跃技能
    private void UseCurrentHeroSkill()
    {
        string currentHeroName = PlayerDataMgr.Instance.playerState.currentHeroName;
        if (currentHeroName == "ToonSoldiers_gunner")
        {
            StartCoroutine(RealBackJump());
            //回血
            int newHp = PlayerDataMgr.Instance.playerState.currentHealth + 30;
            newHp = Mathf.Clamp(newHp, 0, playerConfig.maxHealth);
            PlayerDataMgr.Instance.playerState.currentHealth = newHp;
            EventMgr.Instance.Trigger<int, int>(EventConst.UpdatePlayerHP, newHp, playerConfig.maxHealth);
            //回体力
            float newSp = PlayerDataMgr.Instance.playerState.currentEnergy + 100;
            newSp = Mathf.Clamp(newSp, 0, playerConfig.maxEnergy);
            PlayerDataMgr.Instance.playerState.currentEnergy = newSp;
            EventMgr.Instance.Trigger<float, float>(EventConst.UpdatePlayerEnergy, newSp, playerConfig.maxEnergy);

            RefreshItemUI();
        }
    }

    // 真实物理后跳：先起跳 + 平稳向后滑行 有抛物线 不瞬移
    private IEnumerator RealBackJump()
    {
        canUseSkill = false;
        skillCoolTimer = skillCooldown;
        //发事件给gamingPanel
        EventMgr.Instance.Trigger("Update_Skill_CD", 1f);

        // 水平向后方向
        Vector3 backDir = -transform.forward;
        backDir.y = 0;
        backDir.Normalize();

        // 第一步：只给向上起跳力，不用清空velocity
        rb.AddForce(Vector3.up * jumpPower, ForceMode.Impulse);

        // 等待极短时间，让人物离地
        yield return new WaitForSeconds(0.08f);

        // 第二步：持续给向后的水平推力，形成跳跃滑行感
        float spendTime = 0.35f;
        float timer = 0;
        while (timer < spendTime)
        {
            timer += Time.deltaTime;
            rb.AddForce(backDir * backJumpPower * Time.deltaTime * 8f, ForceMode.Impulse);
            yield return null;
        }
        UIMgr.Instance.ShowPanel<TempTipPanel>().ShowTipAutoHide("使用技能");
        Debug.Log("后跳闪避完成");
    }
    //技能冷却更新
    private void UpdateSkillCooldown()
    {
        if (!canUseSkill)
        {
            skillCoolTimer -= Time.deltaTime;
            float progress = skillCoolTimer / skillCooldown;

            // 发送冷却进度（和物品一样）
            EventMgr.Instance.Trigger("Update_Skill_CD", progress);

            if (skillCoolTimer <= 0)
            {
                canUseSkill = true;
                EventMgr.Instance.Trigger("Update_Skill_CD", 0f);
            }
        }
    }
    #endregion

    #region 物品使用
    //使用医疗包和体力剂的逻辑
    public void UseHPItem()
    {
        if (!canUseHPItem)
        {
            UIMgr.Instance.ShowPanel<TempTipPanel>().ShowTipAutoHide("道具冷却中");
            return;
        }
        if (PlayerDataMgr.Instance.playerState.isDead) return;
        if (PlayerDataMgr.Instance.playerState.currentHealth >= playerConfig.maxHealth)
        {
            UIMgr.Instance.ShowPanel<TempTipPanel>().ShowTipAutoHide("血量已满");
            return;
        }
        if (gameData.MedKit <= 0)
        {
            UIMgr.Instance.ShowPanel<TempTipPanel>().ShowTipAutoHide("医疗包不足");
            return;
        }

        // 开始冷却
        canUseHPItem = false;
        hpItemCoolTimer = itemCooldown;

        HPItem--;

        int newHp = PlayerDataMgr.Instance.playerState.currentHealth + 30;
        newHp = Mathf.Clamp(newHp, 0, playerConfig.maxHealth);
        PlayerDataMgr.Instance.playerState.currentHealth = newHp;

        GameDataMgr.Instance.SaveGameData();
        EventMgr.Instance.Trigger<int, int>(EventConst.UpdatePlayerHP, newHp, playerConfig.maxHealth);
        UIMgr.Instance.ShowPanel<TempTipPanel>().ShowTipAutoHide("使用医疗包 +30");

        RefreshItemUI();
    }

    public void UseSPItem()
    {
        if (!canUseSPItem)
        {
            UIMgr.Instance.ShowPanel<TempTipPanel>().ShowTipAutoHide("道具冷却中");
            return;
        }
        if (PlayerDataMgr.Instance.playerState.isDead) return;
        if (PlayerDataMgr.Instance.playerState.currentEnergy >= playerConfig.maxEnergy)
        {
            UIMgr.Instance.ShowPanel<TempTipPanel>().ShowTipAutoHide("体力已满");
            return;
        }
        if (gameData.StaminaPotion <= 0)
        {
            UIMgr.Instance.ShowPanel<TempTipPanel>().ShowTipAutoHide("体力剂不足");
            return;
        }

        canUseSPItem = false;
        spItemCoolTimer = itemCooldown;

        SpItem--;

        float newSp = PlayerDataMgr.Instance.playerState.currentEnergy + 100;
        newSp = Mathf.Clamp(newSp, 0, playerConfig.maxEnergy);
        PlayerDataMgr.Instance.playerState.currentEnergy = newSp;

        GameDataMgr.Instance.SaveGameData();
        EventMgr.Instance.Trigger<float, float>(EventConst.UpdatePlayerEnergy, newSp, playerConfig.maxEnergy);
        UIMgr.Instance.ShowPanel<TempTipPanel>().ShowTipAutoHide("使用体力剂 +100");

        RefreshItemUI();
    }
    //冷却
    private void UpdateItemCooldown()
    {
        // 医疗包冷却
        if (!canUseHPItem)
        {
            hpItemCoolTimer -= Time.deltaTime;
            float progress =hpItemCoolTimer / itemCooldown;
            EventMgr.Instance.Trigger<float>("Update_HP_Item_CD", progress);

            if (hpItemCoolTimer <= 0)
            {
                canUseHPItem = true;
            }
        }

        // 体力剂冷却
        if (!canUseSPItem)
        {
            spItemCoolTimer -= Time.deltaTime;
            float progress =spItemCoolTimer / itemCooldown;
            EventMgr.Instance.Trigger<float>("Update_SP_Item_CD", progress);

            if (spItemCoolTimer <= 0)
            {
                canUseSPItem = true;
            }
        }
    }
    // 通知UI刷新数量
    private void RefreshItemUI()
    {
        EventMgr.Instance.Trigger<int, int>("Update_Item_Count", HPItem, SpItem);
    }

    //胜利结算物品数量
    public void SetItem()
    {
        GameDataMgr.Instance.SetItemNum(gameData.gold, HPItem, SpItem);
    }
    #endregion
    public void OnDestroy()
    {
        EventMgr.Instance.RemoveListener(EventConst.WinGame, SetItem);
    }
}