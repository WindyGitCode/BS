using UnityEngine;

/// <summary>
/// 武器数据模型
/// </summary>
[System.Serializable]
public class WeaponData
{
    public E_Weapon weaponType;       // 具体类型
    public int weaponTypeCode;   // 类型代码（用于判断武器大类,1和2代表主武器，3为近战武器，4为战术武器）
    public string weaponName;         // 名称
    public int price;                 // 商店售价（0表示免费/已解锁）
    public string animControllerPath; // 动画控制器路径
    public string fireEffectPath;     // 攻击特效路径
    public string fireAudioPath;      // 攻击音效路径
    public int maxAmmo;               // 最大弹药量（近战/手雷设为0）
    public int nowAmmo;               // 当前弹药量（运行时使用，保存当前剩余弹药）
    public float fireRate;            // 攻击间隔（手雷需大于0，枪械/近战设0）
    public bool isUnlocked;           // 是否已购买
    public RuntimeAnimatorController animController;//动画控制器

    // 运行时缓存的资源（避免重复加载）
    [HideInInspector] public GameObject fireEffectPrefab;
    [HideInInspector] public AudioClip fireAudioClip;
}
/// <summary>
/// 武器类型枚举
/// </summary>
public enum E_Weapon
{
    MainGun,          // 主武器
    weapon_handgun,   // 副武器
    weapon_knife,     // 近战-匕首
    Grenade           // 战术武器
}
