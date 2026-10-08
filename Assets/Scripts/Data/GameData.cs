using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//账号数据
public class GameData
{
    public int gold; // 金钱
    // 9种道具数量
    public int MedKit;//医疗包
    public int StaminaPotion;//体力剂
    public int TowerRepairKit;//修塔包
    public int MainGunAmmo;//步枪子弹
    public int HandGunAmmo;//手枪子弹
    public int HeavyGunAmmo;//机枪子弹
    public int ShotGunAmmo;//霰弹枪子弹
    public int SniperAmmo;//狙击枪子弹
    public int Grenade;//手雷

    //键位
    public KeyCode skillKey;
    public KeyCode reloadKey;
    public KeyCode interactKey;
    public KeyCode rollKey;
    public KeyCode dialogKey;
    public KeyCode pauseKey;
    public KeyCode useHPItemKey;
    public KeyCode useSPItemKey;

    //已解锁成就，用逗号分隔ID
    public string unlockedAchievements; 
    // 成就进度：key=成就ID，value=当前进度
    public Dictionary<string, int> achievementProgressDic = new Dictionary<string, int>();
    // 已领取奖励的成就
    public string rewardTakenAchievements;
}

