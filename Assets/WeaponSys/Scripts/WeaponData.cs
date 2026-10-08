using UnityEngine;

/// <summary>
/// 武器数据模型
/// </summary>
[System.Serializable]
public class WeaponConfig
{
    public E_Weapon weaponType;       // 具体类型
    public int weaponTypeCode;   // 类型代码（用于判断武器大类,1和2代表主武器，3为近战武器，4为战术武器）
    public string weaponName;         // 名称
    public int price;                 // 商店售价（0表示免费/已解锁）
    public string animControllerPath; // 动画控制器路径
    public string fireEffectPath;     // 攻击特效路径
    public string reloadAudioPath;     // 装弹音效路径
    public string fireAudioPath;      // 攻击音效路径
    public string IconPath;           // 图标路径
    public int maxAmmo;               // 弹匣最大弹药量
    public float ReloadTime;            //换弹耗时
    public float fireRate;            // 攻击间隔（手雷需大于0，枪械/近战设0）
    public int damage;              // 伤害值
    public RuntimeAnimatorController animController;//动画控制器

    // 运行时缓存的资源（避免重复加载）
    [HideInInspector] public GameObject fireEffectPrefab;
    [HideInInspector] public AudioClip fireAudioClip;
    [HideInInspector] public AudioClip ReloadAudioClip;
}
/// <summary>
/// 武器类型枚举
/// </summary>
public enum E_Weapon
{
    MainGun,   // 主武器（步枪）
    HandGun,   // 手枪
    HeavyGun,  //机枪
    SnipeGun,  //狙击枪
    Shotgun,   //霰弹枪
    knife,     // 近战-匕首
    Grenade    // 手雷
}
