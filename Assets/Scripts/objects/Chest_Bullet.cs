using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chest_Bullet : Interactable
{

    public override void OnInteract()
    {
        // 打开宝箱，给予玩家奖励
        Debug.Log("你打开了宝箱，获得了子弹！");
        // 发事件
        EventMgr.Instance.Trigger<int>(EventConst.AddMainGunAmmo, 30);
        UIMgr.Instance.ShowPanel<TempTipPanel>().ShowTipAutoHide("获得30发步枪子弹！", 2.5f);
        this.gameObject.SetActive(false); // 打开宝箱后隐藏它
    }


}
