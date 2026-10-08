using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class MainTower : Interactable
{
    private static MainTower _instance;
    public static MainTower Instance => _instance;
    public int maxHP;
    private int currentHP;
    private int fixTower;//修塔包数量
    public bool isDead;
    private GameData gameData;
    private void Start()
    {
        _instance = this;
        isDead=false;
        maxHP = 1000;
        currentHP = maxHP;
        gameData = GameDataMgr.Instance.GetGameData();
        fixTower = gameData.TowerRepairKit;
        Debug.Log($"修塔包数量:{fixTower}");
        //事件监听
        EventMgr.Instance.AddListener(EventConst.WinGame, SetFixTowerNum);
    }
    public void UpdateHP(int maxHP,int currentHP)
    {
        this.maxHP = maxHP;
        this.currentHP = currentHP;
        // 更新UI显示
        UIMgr.Instance.GetPanel<GamingPanel>()?.UpdateTowerBlood(currentHP, maxHP);
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
            EventMgr.Instance.Trigger(EventConst.TowerDestroyed);
        }
        UpdateHP(maxHP,currentHP);
    }
    private void SetFixTowerNum()
    {
        GameDataMgr.Instance.SetFixTowerNum(fixTower);
    }
    public override void OnInteract()
    {
        base.OnInteract();
        //回复300滴血
        if (fixTower > 0)
        {
            int newHP = currentHP + 300;
            newHP = Mathf.Clamp(newHP, 0, maxHP);
            UpdateHP(maxHP, newHP);
            UIMgr.Instance.ShowPanel<TempTipPanel>().ShowTipAutoHide("主塔回复300点血量");
            fixTower--;
        }
        else
        {
            UIMgr.Instance.ShowPanel<TempTipPanel>().ShowTipAutoHide("修塔包数量不足");
        }
    }
    public void OnDestroy()
    {
        EventMgr.Instance.RemoveListener(EventConst.WinGame, SetFixTowerNum);
    }
}
