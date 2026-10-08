using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class WeaponController : MonoBehaviour
{
    private static WeaponController instance;
    public static WeaponController Instance => instance;

    // 武器核心数据
    public List<E_Weapon> gamingWeaponList;//当前游戏中可用的武器列表
    public E_Weapon nowWeapon;//记录当前武器类型，方便切换和攻击逻辑使用
    public Dictionary<E_Weapon, WeaponConfig> weaponDict;//特定武器的配置数据

    // 武器物体节点
    public Transform weaponContainer;//武器容器
    public Transform GrenadeThrowPoint;//手雷投掷点
    private Transform muzzlePoint;//枪口位置

    // 攻击管控
    private float lastFireTime;       // 上次开火时间，用于控制射速
    private bool isReloading;         //正在换弹
    private Dictionary<E_Weapon, Sprite> weaponIconCache = new Dictionary<E_Weapon, Sprite>();//icon缓存

    //弹匣与换弹系统,弹药数据
    [Header("换弹设置")]
    private int currentMainGunMagazineAmmo;   // 当前主武器弹匣里的子弹
    private int currentHeavyMagazineAmmo;   // 当前机枪弹匣里的子弹
    private int currentSnipeGunMagazineAmmo;   // 当前狙击枪弹匣里的子弹
    private int currentShotMagazineAmmo;   // 当前霰弹枪弹匣里的子弹
    private int currentHandgunMagazineAmmo;   // 当前手枪弹匣里的子弹
    private int currentGrenadeMagazineAmmo;   // 当前手雷弹匣里的子弹(弹匣量为1，丢完需要装填)

    private int ReverseMainGunAmmo;   // 步枪（主武器）备用弹药数
    private int ReverseHeavyAmmo;      //狙击枪备用弹药数
    private int ReverseSnipeAmmo;      //狙击枪备用弹药数
    private int ReverseShotAmmo;       //霰弹枪备用弹药数
    private int ReverseHandgunAmmo;    //手枪备用弹药数
    private int ReverseGrenadeAmmo;   // 手雷备用弹药数

    // 组件
    private AudioSource audioSource;//播放音效组件
    private Animator playerAnimator;//玩家动画组件
    //其他
    private GameObject bulletHolePrefab;//击中特效
    private GameData gameData;
    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        //组件获取
        audioSource = GetComponent<AudioSource>();
        playerAnimator = GetComponent<Animator>();
        weaponDict = WeaponDataMgr.Instance.weaponDict;

        // 查找子物体节点
        weaponContainer = FindChildRecursively(transform, "WeaponContainer");
        GrenadeThrowPoint = FindChildRecursively(transform, "GrenadeThrowPoint");

        //初始化当前游戏可用的武器列表（后续根据不同角色配置）
        gamingWeaponList = new List<E_Weapon>
        {
            E_Weapon.MainGun,
            //E_Weapon.HeavyGun,
            //E_Weapon.SnipeGun,
            //E_Weapon.Shotgun,
            E_Weapon.HandGun,
            E_Weapon.knife,
            E_Weapon.Grenade
        };

        // 初始化弹药
        gameData = GameDataMgr.Instance.GetGameData();
        ReverseMainGunAmmo = gameData.MainGunAmmo;   //步枪弹药数量
        ReverseGrenadeAmmo = gameData.Grenade;    //手雷数量
        ReverseSnipeAmmo = gameData.SniperAmmo;      //狙击枪弹药数量
        ReverseShotAmmo = gameData.ShotGunAmmo;       //霰弹枪弹药数量
        ReverseHeavyAmmo=gameData.HeavyGunAmmo;       //机枪弹药数量
        ReverseHandgunAmmo = gameData.HandGunAmmo;    //手枪弹药数量

        // 预加载所有武器图标
        PreLoadAllWeaponIcons();
        // 切换初始武器
        SwitchWeapon(E_Weapon.knife);

        // 注册全局事件
        EventMgr.Instance.AddListener<int>(EventConst.AddMainGunAmmo, AddMainGunAmmo);
        EventMgr.Instance.AddListener<int>(EventConst.AddGrenade, AddGrenade);
        EventMgr.Instance.AddListener(EventConst.WinGame, SetAmmo);

        //其他
        bulletHolePrefab=Resources.Load<GameObject>("Effects/hitEff");
        if (bulletHolePrefab == null)
        {
            Debug.Log("未找到枪击特效！");
        }
    }

    void Update()
    {
        // 游戏暂停 / 角色死亡 不执行武器逻辑
        if (GamePauseMgr.Instance.isGamePause || PlayerDataMgr.Instance.playerState.isDead)
            return;

        //换弹
        if (Input.GetKeyDown(gameData.reloadKey) && !isReloading)
            StartCoroutine(Reload());

        //攻击输入处理
        AttackInput();
    }

    #region 攻击系统核心
    private void AttackInput()
    {
        bool isFiring = false;
        if (isReloading) return;//换弹时不开枪

        // 主武器
        if (nowWeapon == E_Weapon.MainGun)
        {
            if (Input.GetMouseButton(0) && CanFire() && currentMainGunMagazineAmmo > 0)
            {
                FireMainGun();
                isFiring = true;
            }
            else if(currentMainGunMagazineAmmo == 0 &&ReverseMainGunAmmo!=0 && !isReloading)
            {
                StartCoroutine(Reload());
            }
        }
        // 手枪
        else if (nowWeapon == E_Weapon.HandGun)
        {
            if (Input.GetMouseButtonDown(0) && CanFire() && currentHandgunMagazineAmmo > 0)
            {
                FireHandGun();
                isFiring = true;
            }
            else if (currentHandgunMagazineAmmo == 0 && ReverseHandgunAmmo!=0 && !isReloading)
            {
                StartCoroutine(Reload());
            }
        }
        //近战
        else if (nowWeapon == E_Weapon.knife)
        {
            if (Input.GetMouseButtonDown(0) && CanFire())
            {
                MeleeAttack();
                isFiring = true;
            }
        }
        // 手雷
        else if (nowWeapon == E_Weapon.Grenade)
        {
            if (Input.GetMouseButtonDown(0) && CanFire() && currentGrenadeMagazineAmmo > 0)
            {
                FireGrenade();
                isFiring = true;
            }
            else if (currentGrenadeMagazineAmmo == 0 && !isReloading)
            {
                StartCoroutine(Reload());
            }
        }
        playerAnimator.SetBool("Fire", isFiring);
    }
    //不同武器攻击
    private void FireMainGun()
    {
        currentMainGunMagazineAmmo--;
        lastFireTime = Time.time;
        RefreshWeaponUI();
        GunAttack();
    }

    private void FireHeavyGun()
    {
        currentHeavyMagazineAmmo--;
        lastFireTime = Time.time;
        RefreshWeaponUI();
        GunAttack();
    }

    private void FireShotgun()
    {
        currentShotMagazineAmmo--;
        lastFireTime = Time.time;
        RefreshWeaponUI();
        GunAttack();
    }

    private void FireSnipeGun()
    {
        currentSnipeGunMagazineAmmo--;
        lastFireTime = Time.time;
        RefreshWeaponUI();
        GunAttack();
    }

    private void FireHandGun()
    {
        currentHandgunMagazineAmmo--;
        lastFireTime = Time.time;
        RefreshWeaponUI();
        GunAttack();
    }
    private void FireGrenade()
    {
        currentGrenadeMagazineAmmo--; // 扣1颗雷
        lastFireTime = Time.time;
        RefreshWeaponUI();
        StartCoroutine(ThrowGrenadeAfterDelay(0.5f));
    }
    //换弹功能
    private IEnumerator Reload()
    {
        Debug.Log("开始换弹");
        isReloading = true;
        if (nowWeapon != E_Weapon.Grenade)
        {
            playerAnimator.SetBool("reload", true);
        }
        AudioMgr.Instance.PlaySFX(weaponDict[nowWeapon].ReloadAudioClip,2);

        yield return new WaitForSeconds(weaponDict[nowWeapon].ReloadTime);

        switch (nowWeapon)
        {
            case E_Weapon.MainGun:
                {
                    int need = weaponDict[nowWeapon].maxAmmo - currentMainGunMagazineAmmo;
                    int fill = Mathf.Min(need, ReverseMainGunAmmo);
                    currentMainGunMagazineAmmo += fill;
                    ReverseMainGunAmmo -= fill;
                    break;
                }
            case E_Weapon.HandGun:
                {
                    int need = weaponDict[nowWeapon].maxAmmo - currentHandgunMagazineAmmo;
                    int fill = Mathf.Min(need, ReverseHandgunAmmo);
                    currentHandgunMagazineAmmo += fill;
                    ReverseHandgunAmmo -= fill;
                    break;
                }
            case E_Weapon.SnipeGun:
                {
                    int need = weaponDict[nowWeapon].maxAmmo - currentSnipeGunMagazineAmmo;
                    int fill = Mathf.Min(need, ReverseSnipeAmmo);
                    currentSnipeGunMagazineAmmo += fill;
                    ReverseSnipeAmmo -= fill;
                    break;
                }
            case E_Weapon.Shotgun:
                {
                    int need = weaponDict[nowWeapon].maxAmmo - currentShotMagazineAmmo;
                    int fill = Mathf.Min(need, ReverseShotAmmo);
                    currentShotMagazineAmmo += fill;
                    ReverseShotAmmo -= fill;
                    break;
                }
            case E_Weapon.HeavyGun:
                {
                    int need = weaponDict[nowWeapon].maxAmmo - currentHeavyMagazineAmmo;
                    int fill = Mathf.Min(need, ReverseHeavyAmmo);
                    currentHeavyMagazineAmmo += fill;
                    ReverseHeavyAmmo -= fill;
                    break;
                }
            case E_Weapon.Grenade:
                if (ReverseGrenadeAmmo > 0)
                {
                    currentGrenadeMagazineAmmo = 1;
                    ReverseGrenadeAmmo -= 1;
                }
                break;
        }
        isReloading = false;
        RefreshWeaponUI();
        Debug.Log("换弹完成");
    }

    //范围检测和射线检测
    private float meleeAttackRange = 1f;
    private float meleeAttackDistance = 0.8f;
    private void MeleeAttack()
    {
        lastFireTime = Time.time;
        PlaySound();

        Collider[] hitColliders = Physics.OverlapSphere(
            transform.position + transform.forward * meleeAttackDistance + transform.up,
            meleeAttackRange,
            1 << LayerMask.NameToLayer("Enemy")
        );

        foreach (var hitCol in hitColliders)
        {
            if (hitCol.CompareTag("Enemy"))
            {
                Debug.Log("刀击中敌人：" + hitCol.gameObject.name);
            }
            //敌人受伤事件
            hitCol.gameObject.GetComponentInParent<MonsterController>()?.TakeDamage(weaponDict[nowWeapon].damage);
        }
    }

    //调试攻击范围
    private void OnDrawGizmos()
    {
        // 只有当前是匕首 + 开启可视化才显示
        if (nowWeapon != E_Weapon.knife) return;

        // 计算攻击中心点（和MeleeAttack完全一致）
        Vector3 center = transform.position + transform.forward * meleeAttackDistance + transform.up;

        // 绘制红色半透明攻击范围球体
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(center, meleeAttackRange);

        // 绘制攻击方向线（更直观）
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position + transform.up, center);
    }

    //射线检测的层级掩码，确保只检测敌人、场景
    private int shootLayerMask => LayerMask.GetMask("Enemy", "Scene");
    private void GunAttack()
    {
        lastFireTime = Time.time;
        PlaySound();
        ShowEffect();

        // --------------------------------------------------------------------
        // 【第一步】从 主摄像机屏幕中心 发射射线，获取准星指向的世界点
        // --------------------------------------------------------------------
        Ray cameraRay = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        Vector3 targetPoint;
        // 【蓝色射线】画出摄像机中心射线（持续1秒）
        Debug.DrawLine(cameraRay.origin, cameraRay.origin + cameraRay.direction * 100f, Color.blue, 1f);
        // 射线能打到东西 → 落点就是目标点
        if (Physics.Raycast(cameraRay, out RaycastHit hitInfo, 100f, shootLayerMask))
        {
            targetPoint = hitInfo.point;
        }
        // 射线打不到东西 → 取摄像机前方100米的点
        else
        {
            targetPoint = cameraRay.GetPoint(100f);
        }
        // --------------------------------------------------------------------
        // 【第二步】枪口 指向 目标点，发射真正的攻击射线
        // --------------------------------------------------------------------
        Vector3 rayOrigin = muzzlePoint.position;
        Vector3 shootDir = (targetPoint - rayOrigin).normalized; // 枪口 → 准星点

        // 调试射线（红色）
        Debug.DrawLine(rayOrigin, rayOrigin + shootDir * 100f, Color.red, 1f);

        // --------------------------------------------------------------------
        // 【第三步】伤害检测
        // --------------------------------------------------------------------
        if (Physics.Raycast(rayOrigin, shootDir, out RaycastHit hit, 100f, shootLayerMask))
        {
            Debug.Log("准星击中: " + hit.collider.gameObject.name);
            //生成击中特效
            SpawnBulletHole(hit);
            // 敌人受伤
            if (hit.collider.gameObject.tag=="Enemy")
            {
                Debug.Log("进入受伤逻辑");
                int damaged = weaponDict[nowWeapon].damage;
                if (hit.collider.gameObject.name == "head")
                {
                    damaged *= 2;
                    Debug.Log($"进入爆头伤害加倍逻辑，damaged={damaged}");
                }
                MonsterController m= hit.collider.gameObject.GetComponentInParent<MonsterController>();
                if (m == null)
                {
                    Debug.Log("未获取到MonsterController");
                }
                else
                {
                    m.TakeDamage(damaged);
                }
                    Debug.Log($"命中物体名字：{hit.collider.gameObject.name}");
            }
        }
        // 开枪触发准星扩散
        if(nowWeapon==E_Weapon.MainGun)
            UIMgr.Instance.GetPanel<GamingPanel>()?.AddSpread();
        else
            UIMgr.Instance.GetPanel<GamingPanel>()?.SingleShotSpread();
    }

    // 生成击中特效
    void SpawnBulletHole(RaycastHit hit)
    {
        Debug.Log("进入SpawnBulletHole");
        if (bulletHolePrefab == null) 
        {
            Debug.Log("没有击中特效！");
            return;
        }
        
        // 位置：击中点 + 轻微向外偏移
        Vector3 spawnPos = hit.point + hit.normal * 0.01f;

        // 方向：弹痕正面朝向法线
        Quaternion spawnRot = Quaternion.LookRotation(hit.normal);

        // 生成
        GameObject hole = Instantiate(bulletHolePrefab, spawnPos, spawnRot);

        // 让弹痕作为被击中物体的子物体（跟随移动、不会飘）
        hole.transform.SetParent(hit.collider.transform);

        // 1秒后销毁
        Destroy(hole, 1f);
    }

    //丢手雷协程
    private IEnumerator ThrowGrenadeAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        var config = weaponDict[nowWeapon];

        GameObject grenadePrefab = Resources.Load<GameObject>("Prefabs/Grenade");
        if (grenadePrefab == null)
        {
            Debug.LogError("手雷预制体加载失败！");
            yield break;
        }
        GameObject grenadeObj = Instantiate(grenadePrefab, GrenadeThrowPoint.position, transform.rotation);
        Grenade grenade = grenadeObj.GetComponent<Grenade>();

        grenade.damage = config.damage;
        grenade.explosionRadius = 3;

        Vector3 throwDir = transform.forward * 1f + transform.up * 0.4f;
        grenade.Throw(throwDir * 10);
    }

    private void PlaySound()
    {
        if (weaponDict[nowWeapon].fireAudioClip != null)
            audioSource.PlayOneShot(weaponDict[nowWeapon].fireAudioClip);
        else
            Debug.LogWarning("武器 " + nowWeapon + " 缺少音效！");
    }

    private void ShowEffect()//显示特效
    {
        if (weaponDict[nowWeapon].fireEffectPrefab == null) return;
        FindMuzzlePoint();
        if (muzzlePoint != null)
        {
            GameObject effect = Instantiate(weaponDict[nowWeapon].fireEffectPrefab, muzzlePoint.position, muzzlePoint.rotation);
            if (nowWeapon == E_Weapon.MainGun)
            {
                effect.transform.up = transform.forward;
            }

        }
    }

    private void FindMuzzlePoint()
    {
        Transform currentWeapon = weaponContainer.Find(nowWeapon.ToString());
        muzzlePoint = FindChildRecursively(currentWeapon, "MuzzlePoint");
        if (muzzlePoint != null)
            Debug.Log(muzzlePoint.transform.parent.name);
        else
            Debug.Log("没找到MuzzlePoint");
    }

    private bool CanFire()
    {
        return Time.time >= lastFireTime + weaponDict[nowWeapon].fireRate;
    }
    #endregion

    #region 武器切换
    public void ChangeLastWeapon()
    {
        int index = gamingWeaponList.IndexOf(nowWeapon);
        nowWeapon = gamingWeaponList[(index - 1 + gamingWeaponList.Count) % gamingWeaponList.Count];
        SwitchWeapon(nowWeapon);
    }

    public void ChangeNextWeapon()
    {
        int index = gamingWeaponList.IndexOf(nowWeapon);
        nowWeapon = gamingWeaponList[(index + 1) % gamingWeaponList.Count];
        SwitchWeapon(nowWeapon);
    }

    public void SwitchWeapon(E_Weapon weapon)
    {
        nowWeapon = weapon;
        playerAnimator.runtimeAnimatorController = WeaponDataMgr.Instance.weaponDict[weapon].animController;
        ChangeWeaponMesh();
        lastFireTime = 0;
        FindMuzzlePoint();
        RefreshWeaponUI();
    }
    #endregion

    public void ChangeWeaponMesh()
    {
        if (weaponContainer == null) return;
        foreach (Transform child in weaponContainer) child.gameObject.SetActive(false);
        Transform target = weaponContainer.Find(nowWeapon.ToString());
        if (target != null) target.gameObject.SetActive(true);
    }

    public void RefreshWeaponUI()
    {
        if (!weaponDict.ContainsKey(nowWeapon)) return;
        weaponIconCache.TryGetValue(nowWeapon, out Sprite icon);

        string text = "";
        string weaponName = "";

        switch (nowWeapon)
        {
            case E_Weapon.MainGun:
                text = $"{currentMainGunMagazineAmmo} | {ReverseMainGunAmmo}";
                weaponName = "步枪";
                break;
            case E_Weapon.HandGun:
                text = $"{currentHandgunMagazineAmmo} | {ReverseHandgunAmmo}";
                weaponName = "手枪";
                break;
            case E_Weapon.HeavyGun:
                text = $"{currentHeavyMagazineAmmo} | {ReverseHeavyAmmo}";
                weaponName = "机枪";
                break;
            case E_Weapon.SnipeGun:
                text = $"{currentSnipeGunMagazineAmmo} | {ReverseSnipeAmmo}";
                weaponName = "狙击枪";
                break;
            case E_Weapon.Shotgun:
                text = $"{currentShotMagazineAmmo} | {ReverseShotAmmo}";
                weaponName = "霰弹枪";
                break;
            case E_Weapon.knife:
                text = "--";
                weaponName = "匕首";
                break;
            case E_Weapon.Grenade:
                text = $"{currentGrenadeMagazineAmmo} | {ReverseGrenadeAmmo}";
                weaponName = "手雷";
                break;
        }

        UIMgr.Instance.GetPanel<GamingPanel>()?.UpdateWeaponUI(icon, weaponName, text);
    }


    // 预加载所有武器图标 —— 游戏启动只跑一次
    void PreLoadAllWeaponIcons()
    {
        foreach (var weaponType in gamingWeaponList)
        {
            if (weaponDict.TryGetValue(weaponType, out var config))
            {
                if (!string.IsNullOrEmpty(config.IconPath))
                {
                    Sprite icon = Resources.Load<Sprite>(config.IconPath);
                    weaponIconCache[weaponType] = icon;
                }
            }
        }
    }

    //外部事件
    private void AddMainGunAmmo(int value)
    {
        ReverseMainGunAmmo += value;
        RefreshWeaponUI();
    }
    private void AddGrenade(int value)
    {
        ReverseGrenadeAmmo += value;
        RefreshWeaponUI();
    }
    private void SetAmmo()
    {
        GameDataMgr.Instance.SetAmmoNum(
            currentMainGunMagazineAmmo + ReverseMainGunAmmo,
            currentHandgunMagazineAmmo + ReverseHandgunAmmo,
            currentHeavyMagazineAmmo + ReverseHeavyAmmo,
            currentShotMagazineAmmo + ReverseShotAmmo,
            currentSnipeGunMagazineAmmo + ReverseSnipeAmmo,
            currentGrenadeMagazineAmmo + ReverseGrenadeAmmo
            );
    }
    void OnDestroy()
    {
        EventMgr.Instance.RemoveListener<int>(EventConst.AddMainGunAmmo, AddMainGunAmmo);
    }

    // 递归查找子物体
    private Transform FindChildRecursively(Transform parent, string childName)
    {
        if (parent.name == childName) return parent;
        foreach (Transform child in parent)
        {
            Transform res = FindChildRecursively(child, childName);
            if (res != null) return res;
        }
        return null;
    }
}