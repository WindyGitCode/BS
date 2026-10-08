using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 武器配置管理器（仅管理静态配置数据）
/// </summary>
public class WeaponDataMgr : Singleton<WeaponDataMgr>
{
    public List<WeaponConfig> allWeapons = new List<WeaponConfig>();
    public Dictionary<E_Weapon, WeaponConfig> weaponDict = new Dictionary<E_Weapon, WeaponConfig>();

    protected override void Awake()
    {
        base.Awake();
        InitWeaponData();
    }

    private void InitWeaponData()
    {
        if (allWeapons.Count == 0)
        {
            //步枪
            AddWeapon(E_Weapon.MainGun, 1, "步枪", 1000,
                "Controller/MainGun_Controller",
                "Effects/GunFire", "Audio/MainGunReload","Audio/MainGunFire", "Icon/MainGun", 20, 2, 0.15f, 34);
            //手枪
            AddWeapon(E_Weapon.HandGun, 2, "沙漠之鹰", 500,
                "Controller/Handgun_Controller",
                "Effects/GunFire_1", "Audio/HandGunReload","Audio/HandgunFire", "Icon/HandGun", 7, 2, 0.5f, 50);
            //匕首
            AddWeapon(E_Weapon.knife, 3, "匕首", 0,
                "Controller/Knife_Controller",
                "Effects/KnifeHit", null,"Audio/KnifeFire", "Icon/Knife", 0, 0, 0.8f, 50);
            //手雷
            AddWeapon(E_Weapon.Grenade, 4, "手雷", 300,
                "Controller/Grenade_Controller",
                "Effects/GrenadeExplosion", null, "Audio/boom", "Icon/Grenade", 1, 1, 1f, 150);
        }

        foreach (var weapon in allWeapons)
        {
            if (!weaponDict.ContainsKey(weapon.weaponType))
            {
                weaponDict.Add(weapon.weaponType, weapon);
                PreLoadWeaponResources(weapon);
            }
        }
    }

    private void AddWeapon(E_Weapon type, int typeCode, string name, int price, string animPath,
                          string effectPath,string reloadAudioPath,string fireAudioPath,string iconPath, int maxAmmo, float reloadTime, float fireRate,int damage)
    {
        WeaponConfig data = new WeaponConfig();
        data.weaponType = type;
        data.weaponTypeCode = typeCode;
        data.weaponName = name;
        data.price = price;
        data.animControllerPath = animPath;
        data.fireEffectPath = effectPath;
        data.reloadAudioPath = reloadAudioPath;
        data.fireAudioPath = fireAudioPath;
        data.IconPath = iconPath;
        data.maxAmmo = maxAmmo;
        data.ReloadTime = reloadTime;
        data.fireRate = fireRate;
        data.animController = Resources.Load<RuntimeAnimatorController>(data.animControllerPath);
        data.damage = damage;
        allWeapons.Add(data);
    }

    private void PreLoadWeaponResources(WeaponConfig weapon)
    {
        if (!string.IsNullOrEmpty(weapon.fireEffectPath))
        {
            weapon.fireEffectPrefab = Resources.Load<GameObject>(weapon.fireEffectPath);
        }
        if (!string.IsNullOrEmpty(weapon.fireAudioPath))
        {
            weapon.fireAudioClip = Resources.Load<AudioClip>(weapon.fireAudioPath);
        }
        if (!string.IsNullOrEmpty(weapon.fireAudioPath))
        {
            weapon.ReloadAudioClip = Resources.Load<AudioClip>(weapon.reloadAudioPath);
        }
    }
    /// <summary>
    /// 获取武器配置
    /// </summary>
    public WeaponConfig GetWeaponConfig(E_Weapon type)
    {
        weaponDict.TryGetValue(type, out var config);
        return config;
    }
}