using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chest_Gold : Interactable
{
    // Start is called before the first frame update
    public override void OnInteract()
    {
        // 打开宝箱，给予玩家奖励
        Debug.Log("你打开了宝箱，获得了金币！");
        // 这里可以添加具体的奖励逻辑，例如增加金币、道具等
        GameDataMgr.Instance.AddMoney(50); // 假设奖励是50金币
        UIMgr.Instance.ShowPanel<TempTipPanel>().ShowTipAutoHide("获得50金币！", 2.5f);
        this.gameObject.SetActive(false); // 打开宝箱后隐藏它
    }
}
