using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainTower : MonoBehaviour
{
    private static MainTower _instance;
    public static MainTower Instance => _instance;
    private int maxHP;
    private int currentHP;
    public bool isDead;
    private void Start()
    {
        _instance = this;
        isDead=false;
        maxHP = 1000;
        currentHP = maxHP;
    }
    public void UpdateHP(int maxHP,int currentHP)
    {
        this.maxHP = maxHP;
        this.currentHP = currentHP;
        // 更新UI显示
        UIMgr.Instance.GetPanel<GamingPanel>().UpdateTowerBlood(currentHP, maxHP);
    }
    public void TakeDamage(int damage)
    {
        if (isDead) return;
        currentHP -= damage;
        if (currentHP <= 0)
        {
            currentHP = 0;
            isDead = true;
            // 触发主塔被摧毁事件
            Debug.Log("主塔被摧毁，游戏结束！");
        }
        UpdateHP(maxHP,currentHP);
    }
}
