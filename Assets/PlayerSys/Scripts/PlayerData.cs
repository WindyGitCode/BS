using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerConfig
{
    public string heroName; //英雄名称
    public int maxHealth=200;
    public float maxEnergy =200;
}
public class PlayerState
{
    public string currentHeroName;  //英雄名称
    public List<WeaponConfig> gamingWeaponList; //局内武器列表
    public int currentHealth;       // 当前血量
    public float currentEnergy;     // 当前体力
    public int gamingMoney;         //局内金钱
    public bool isDead;              //是否死亡
}
