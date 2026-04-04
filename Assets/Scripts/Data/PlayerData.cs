using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerData
{
    public int money;//全局金钱
    //局内配置项
    public int nowHealth=100;
    public int maxHealth=100;
    public float energy =100;
    public float rotateSpeed = 80;//旋转速度
    public float moveSpeed;//移动速度

    //入场配置项
    public string nowHeroName;//当前英雄名称
    public int gamingMoney;//局内金钱
    public List<WeaponData> gamingWeaponList;//局内武器列表
}
