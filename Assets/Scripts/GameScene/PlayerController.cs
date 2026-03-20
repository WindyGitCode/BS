using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public Animator animator;//动画器
    public E_Weapon nowWeapon;//当前武器
    public List<E_Weapon> gamingWeaponList;//武器列表
    public float rotateSpeed=80;//旋转速度
    private float screenNormalizedFactor;//屏幕尺寸归一化
    /// <summary>
    /// 角色控制器
    /// </summary>
    private RuntimeAnimatorController knifeController;
    private RuntimeAnimatorController handgunController;
    void Start()
    {
        animator = this.GetComponent<Animator>();
        CursorController.HideMouse();
        knifeController = Resources.Load<RuntimeAnimatorController>("Controller/RoleGamingController/Knife_Controller");
        handgunController = Resources.Load<RuntimeAnimatorController>("Controller/RoleGamingController/Handgun_Controller"); 

        if (knifeController == null) Debug.LogError("刀的动画控制器加载失败，检查路径：Controller/RoleGamingController/Knife_Controller");
        if (handgunController == null) Debug.LogError("手枪的动画控制器加载失败，检查路径：Controller/RoleGamingController/Handgun_Controller");
        screenNormalizedFactor = Screen.width / 1920f;
        //加载局内武器列表
        gamingWeaponList = new List<E_Weapon>();
        //从背包数据加载武器列表
        //foreach (var weapon in GameDataMgr.Instance.playerData.weaponList)
        //{
        //    gamingWeaponList.Add(weapon);
        //}
        //测试用，默认拥有刀和手枪
        gamingWeaponList.Add(E_Weapon.Knife);
        gamingWeaponList.Add(E_Weapon.Handgun);
    }
    void Update()
    {
        animator.SetFloat("Vertical", Input.GetAxis("Vertical"));
        animator.SetFloat("Horizontal", Input.GetAxis("Horizontal"));
        animator.SetBool("Fire", Input.GetMouseButton(0));
        animator.SetBool("Roll", Input.GetKey(KeyCode.LeftShift));
        this.transform.Rotate(Vector3.up, Input.GetAxis("Mouse X") * rotateSpeed * screenNormalizedFactor * Time.deltaTime);
        //鼠标滚轮切换武器（向上=刀，向下=手枪）
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll > 0f && nowWeapon != E_Weapon.Knife)
        {
            ChangeWeapon(E_Weapon.Knife);
        }
        else if (scroll < 0f && nowWeapon != E_Weapon.Handgun)
        {
            ChangeWeapon(E_Weapon.Handgun);
        }
    }
    public void ChangeWeapon(E_Weapon weapon)
    {
        nowWeapon = weapon;

        switch (weapon)
        {
            case E_Weapon.Knife:
                animator.runtimeAnimatorController = knifeController;
                break;
            case E_Weapon.Handgun:
                animator.runtimeAnimatorController = handgunController;
                break;
        }
    }
    public void ChangeLastWeapon()
    {
        nowWeapon=gamingWeaponList[(gamingWeaponList.IndexOf(nowWeapon) - 1 + gamingWeaponList.Count) % gamingWeaponList.Count];
        animator.runtimeAnimatorController = WeaponSystemMgr.Instance.weaponDict[nowWeapon].animController;
    }
    public void ChangeNextWeapon()
    {
        nowWeapon = gamingWeaponList[(gamingWeaponList.IndexOf(nowWeapon) + 1 ) % gamingWeaponList.Count];
        animator.runtimeAnimatorController = WeaponSystemMgr.Instance.weaponDict[nowWeapon].animController;
    }
}