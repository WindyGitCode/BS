using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 武器系统管理器
/// </summary>
public class WeaponSystemMgr : Singleton<WeaponSystemMgr>
{
    // 所有武器配置（可从JSON加载，也可在Inspector配置）
    public List<WeaponData> allWeapons = new List<WeaponData>();
    // 资源缓存（避免重复Load）
    public Dictionary<E_Weapon, WeaponData> weaponDict = new Dictionary<E_Weapon, WeaponData>();
    protected override void Awake()
    {
        base.Awake();
        // 初始化武器字典+预加载资源
        InitWeaponData();
    }

    /// <summary>
    /// 初始化武器数据（加载配置+预加载资源）
    /// </summary>
    private void InitWeaponData()
    {
        // 方式1：从JSON加载（推荐，适配商店配置）
        // allWeapons = JsonMgr.Instance.LoadData<List<WeaponData>>("WeaponConfig");

        // 方式2：手动初始化（测试用，可替换为JSON加载）
        if (allWeapons.Count == 0)
        {
            AddWeapon(E_Weapon.Knife, 0 , "匕首", 0, "Controller/RoleGamingController/Knife_Controller",
                      "Effects/KnifeHit", "Audio/KnifeHit", 0, 0, true);
            AddWeapon(E_Weapon.Handgun, 1,"沙漠之鹰", 500, "Controller/RoleGamingController/Handgun_Controller",
                      "Effects/GunFire", "Audio/HandgunFire", 20, 0.2f, false);
            AddWeapon(E_Weapon.MainGun, 2,"AK47", 2000, "Controller/RoleGamingController/Rifle_Controller",
                      "Effects/GunFire", "Audio/RifleFire", 30, 0.1f, false);
            AddWeapon(E_Weapon.Grenade,3, "破片手雷", 300, "Controller/RoleGamingController/Grenade_Controller",
                      "Effects/GrenadeExplode", "Audio/GrenadeExplode", 0, 2f, false);
        }

        // 构建字典+预加载资源
        foreach (var weapon in allWeapons)
        {
            if (!weaponDict.ContainsKey(weapon.weaponType))
            {
                weaponDict.Add(weapon.weaponType, weapon);
                // 预加载资源
                PreLoadWeaponResources(weapon);
            }
        }
    }

    /// <summary>
    /// 添加武器配置（手动初始化用）
    /// </summary>
    private void AddWeapon(E_Weapon type, int typeCode, string name, int price, string animPath,
                          string effectPath, string audioPath, int maxAmmo, float fireRate, bool isUnlocked)
    {
        WeaponData data = new WeaponData();
        data.weaponType = type;
        data.weaponName = name;
        data.price = price;
        data.animControllerPath = animPath;
        data.fireEffectPath = effectPath;
        data.fireAudioPath = audioPath;
        data.maxAmmo = maxAmmo;
        data.fireRate = fireRate;
        data.isUnlocked = isUnlocked;
        allWeapons.Add(data);
    }

    /// <summary>
    /// 预加载武器资源（避免切换时卡顿）
    /// </summary>
    private void PreLoadWeaponResources(WeaponData weapon)
    {
        // 加载动画控制器
        if (!string.IsNullOrEmpty(weapon.animControllerPath))
        {
            weapon.animController = Resources.Load<RuntimeAnimatorController>(weapon.animControllerPath);
            if (weapon.animController == null)
                Debug.LogError($"武器{weapon.weaponName}动画控制器加载失败，路径：{weapon.animControllerPath}");
        }
        // 加载特效
        if (!string.IsNullOrEmpty(weapon.fireEffectPath))
        {
            weapon.fireEffectPrefab = Resources.Load<GameObject>(weapon.fireEffectPath);
            if (weapon.fireEffectPrefab == null)
                Debug.LogError($"武器{weapon.weaponName}特效加载失败，路径：{weapon.fireEffectPath}");
        }
        // 加载音效
        if (!string.IsNullOrEmpty(weapon.fireAudioPath))
        {
            weapon.fireAudioClip = Resources.Load<AudioClip>(weapon.fireAudioPath);
            if (weapon.fireAudioClip == null)
                Debug.LogError($"武器{weapon.weaponName}音效加载失败，路径：{weapon.fireAudioPath}");
        }
    }

    /// <summary>
    /// 购买武器（商店调用）
    /// </summary>
    public bool BuyWeapon(E_Weapon weaponType)
    { 
        if (weaponDict.TryGetValue(weaponType, out WeaponData weapon))
        {
            // 校验是否已解锁/金币是否足够
            if (weapon.isUnlocked)
            {
                Debug.Log($"武器{weapon.weaponName}已解锁，无需重复购买");
                return true;
            }
            if (GameDataMgr.Instance.playerData.money < weapon.price)
            {
                Debug.Log($"金币不足，无法购买{weapon.weaponName}（需{weapon.price}，当前{GameDataMgr.Instance.playerData.money}）");
                return false;
            }
            // 扣除金币+标记解锁
            GameDataMgr.Instance.playerData.money -= weapon.price;
            weapon.isUnlocked = true;
            // 保存玩家数据
            JsonMgr.Instance.SaveData(GameDataMgr.Instance.playerData, "PlayerData");
            Debug.Log($"成功购买{weapon.weaponName}，剩余金币：{GameDataMgr.Instance.playerData.money}");
            return true;
        }
        Debug.LogError($"未找到武器类型：{weaponType}");
        return false;
    }

    /// <summary>
    /// 获取武器数据
    /// </summary>
    public WeaponData GetWeaponData(E_Weapon weaponType)
    {
        if (weaponDict.TryGetValue(weaponType, out WeaponData data))
            return data;
        return null;
    }
}
