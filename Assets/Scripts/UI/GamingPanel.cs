using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GamingPanel : BasePanel
{
    //图片
    public Image towerBlood;
    public Image playerBlood;
    public Image strengh;
    //塔血量
    public Text towerNowHP;
    public Text towerMaxHP;
    //玩家血量
    public Text playerNowHP;
    public Text playerMaxHP;
    //玩家体力
    public Text nowStrengh;
    public Text maxStrengh;

    public override void Init()
    {
        
    }
    public void UpdateTowerBlood(int nowHP,int maxHP)
    {
        towerBlood.fillAmount = nowHP / maxHP;
        towerNowHP.text = nowHP.ToString();
        towerMaxHP.text = maxHP.ToString();
    }
}
