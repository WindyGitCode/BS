using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chest_Grenade : Interactable
{
    public override void OnInteract()
    {
        // 打开宝箱，给予玩家奖励
        Debug.Log("你打开了宝箱，获得了手雷！");
        // 这里可以添加具体的奖励逻辑，例如增加金币、道具等
        EventMgr.Instance.Trigger<int>(EventConst.AddGrenade,20);
        UIMgr.Instance.ShowPanel<TempTipPanel>().ShowTipAutoHide("获得20颗手雷！", 2.5f);
        this.gameObject.SetActive(false); // 打开宝箱后隐藏它
    }
}
