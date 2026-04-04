using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public PlayerData playerData;
    public Animator animator;
    public E_Weapon nowWeapon;
    public List<E_Weapon> gamingWeaponList;
    public float rotateSpeed;
    private float screenNormalizedFactor;
    public Transform weaponContainer;

    // 攻击相关
    private WeaponData currentWeaponData;// 当前武器数据
    private float lastFireTime;       // 上次攻击时间
    private AudioSource audioSource;  // 音效播放
    private Transform muzzlePoint;    // 枪口位置

    void Start()
    {
        playerData = PlayerMgr.Instance.GetPlayerData();
        rotateSpeed = playerData.rotateSpeed;
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        CursorController.HideMouse();
        screenNormalizedFactor = Screen.width / 1920f;
        // 武器容器
        weaponContainer=FindChildRecursively(transform, "WeaponContainer");
        // 局内武器初始化（先手动添加，后面通过背包系统数据初始化）
        gamingWeaponList = new List<E_Weapon>
        {
            E_Weapon.weapon_knife,
            E_Weapon.weapon_handgun
        };
        // 默认切换为刀
        SwitchWeapon(E_Weapon.weapon_knife);
    }

    void Update()
    {
        animator.SetFloat("Vertical", Input.GetAxis("Vertical"));
        animator.SetFloat("Horizontal", Input.GetAxis("Horizontal"));
        animator.SetBool("Roll", Input.GetKey(KeyCode.LeftShift));
        transform.Rotate(Vector3.up, Input.GetAxis("Mouse X") * rotateSpeed * screenNormalizedFactor * Time.deltaTime);
        // 攻击输入
        AttackInput();
        // 滚轮切换武器
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll > 0f) ChangeLastWeapon();
        else if (scroll < 0f) ChangeNextWeapon();
        //测试
        if(Input.GetKeyDown(KeyCode.R))
        {
            MainTower.Instance.TakeDamage(100);
        }
    }

    #region 攻击系统核心
    private void AttackInput()
    {
        if (currentWeaponData == null)
        {
            Debug.LogError("当前武器数据未设置，无法攻击");
            return;
        }
        // 动画 Fire 状态
        bool isFiring = false;
        // 1. 主武器（机枪）：长按攻击
        if (nowWeapon == E_Weapon.MainGun)
        {
            if (Input.GetMouseButton(0) && CanFire() && WeaponSystemMgr.Instance.weaponDict[nowWeapon].nowAmmo > 0)
            {
                GunAttack();
                isFiring = true;
            }
        }

        // 2. 手枪：点击攻击
        else if (nowWeapon == E_Weapon.weapon_handgun)
        {
            if (Input.GetMouseButtonDown(0) && CanFire() && WeaponSystemMgr.Instance.weaponDict[nowWeapon].nowAmmo > 0)
            {
                GunAttack();
                isFiring = true;
            }
        }

        // 3. 近战武器（刀）：点击攻击
        else if (nowWeapon == E_Weapon.weapon_knife)
        {
            if (Input.GetMouseButtonDown(0) && CanFire())
            {
                MeleeAttack();
                isFiring = true;
            }
        }

        // 4. 手雷：长按瞄准 + 松开投掷
        else if (nowWeapon == E_Weapon.Grenade)
        {
            if (Input.GetMouseButton(0))
            {
                // 瞄准逻辑（可加UI）
                animator.SetBool("Aim", true);
            }
            if (Input.GetMouseButtonUp(0) && CanFire())
            {
                GrenadeAttack();
                isFiring = true;
                animator.SetBool("Aim", false);
            }
        }

        animator.SetBool("Fire", isFiring);
    }

    // 近战攻击
    private void MeleeAttack()
    {
        lastFireTime = Time.time;
        PlaySound();
        ShowEffect();

        // 攻击范围参数（可自己调大小）
        float attackRange = 1f;   // 攻击半径
        float attackDistance = 0.8f; // 攻击距离

        // 在角色前方做一个球形检测，找到所有敌人
        Collider[] hitColliders = Physics.OverlapSphere(
            transform.position + transform.forward * attackDistance+transform.up,
            attackRange,
            1<< LayerMask.NameToLayer("Enemy")
        );

        // 遍历所有碰到的物体
        foreach (var hitCol in hitColliders)
        {
            if (hitCol.CompareTag("Enemy"))
            {
                Debug.Log("刀击中敌人：" + hitCol.gameObject.name);
                // 在这里写敌人受伤逻辑
            }
        }
    }

    // 枪械攻击
    private void GunAttack()
    {
        lastFireTime = Time.time;
        PlaySound();
        ShowEffect();

        // 射线检测
        if (Physics.Raycast(muzzlePoint.position, muzzlePoint.forward, out RaycastHit hit, 100f))
        {
            Debug.Log("击中: " + hit.collider.name);
        }
    }

    // 手雷攻击
    private void GrenadeAttack()
    {
        lastFireTime = Time.time;
        PlaySound();
        ShowEffect();
        Debug.Log("投掷手雷");
    }

    // 播放音效
    private void PlaySound()
    {
        if (currentWeaponData.fireAudioClip != null)
            audioSource.PlayOneShot(currentWeaponData.fireAudioClip);
    }

    // 显示特效
    private void ShowEffect()
    {
        if (currentWeaponData.fireEffectPrefab == null) return;

        FindMuzzlePoint();
        if (muzzlePoint != null)
            Instantiate(currentWeaponData.fireEffectPrefab, muzzlePoint.position, muzzlePoint.rotation);
    }

    // 查找枪口
    private void FindMuzzlePoint()
    {
        if (WeaponSystemMgr.Instance.weaponDict[nowWeapon].weaponTypeCode == 3) //近战武器
        {
            muzzlePoint = FindChildRecursively(transform, "MuzzlePoint");
        }
        else// 其他枪械
        {
            muzzlePoint = FindChildRecursively(weaponContainer, "MuzzlePoint");
        }
        if(muzzlePoint == null)
            Debug.Log("未找到枪口位置");
    }

    // 攻击间隔判断
    private bool CanFire()
    {
        return Time.time >= lastFireTime + currentWeaponData.fireRate;
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

    void SwitchWeapon(E_Weapon weapon)
    {
        nowWeapon = weapon;
        currentWeaponData= WeaponSystemMgr.Instance.GetWeaponData(nowWeapon);
        animator.runtimeAnimatorController = currentWeaponData.animController;// 切换动画控制器
        ChangeWeaponMesh();
        RefreshCurrentWeaponData();
        FindMuzzlePoint();
    }
    #endregion

    /// <summary>
    /// 刷新当前武器数据，子弹
    /// </summary>
    void RefreshCurrentWeaponData()
    {
        currentWeaponData = WeaponSystemMgr.Instance.GetWeaponData(nowWeapon);
        lastFireTime = 0;
    }

    /// <summary>
    /// 武器显示
    /// </summary>
    /// <param name="weapon"></param>
    public void ChangeWeaponMesh()
    {
        if (weaponContainer == null) return;
        foreach (Transform child in weaponContainer) child.gameObject.SetActive(false);
        Transform target = weaponContainer.Find(nowWeapon.ToString());
        if (target != null) target.gameObject.SetActive(true);
    }

    /// <summary>
    /// 递归查找工具(绑定子物体)
    /// </summary>
    /// <param name="parent"></param>
    /// <param name="childName"></param>
    /// <returns></returns>
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