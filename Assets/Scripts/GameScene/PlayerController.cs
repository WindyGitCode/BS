using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public Animator animator;//动画器
    public E_weapon nowWeapon;//当前武器
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
        if (scroll > 0f && nowWeapon != E_weapon.knife)
        {
            ChangeWeapon(E_weapon.knife);
        }
        else if (scroll < 0f && nowWeapon != E_weapon.Handgun)
        {
            ChangeWeapon(E_weapon.Handgun);
        }
    }
    public void ChangeWeapon(E_weapon weapon)
    {
        nowWeapon = weapon;
        switch (weapon)
        {
            case E_weapon.knife:
                animator.runtimeAnimatorController = knifeController;
                break;
            case E_weapon.Handgun:
                animator.runtimeAnimatorController = handgunController;
                break;
        }
    }
}

public enum E_weapon
{
    knife,
    Handgun
}