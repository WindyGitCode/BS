using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMgr
{
    private static PlayerMgr instance=new PlayerMgr();
    public static PlayerMgr Instance => instance;
    public PlayerData playerData;//玩家数据
    private PlayerMgr()
    {
        playerData = JsonMgr.Instance.LoadData<PlayerData>("PlayerData");//加载玩家数据
    }
    public void SavePlayerData()
    {
        JsonMgr.Instance.SaveData(playerData, "PlayerData");
    }
    public void LoadPlayerData()
    {
        JsonMgr.Instance.LoadData<PlayerData>("PlayerData");
    }
    public void UpdatePlayerData(PlayerData newData)
    {
        playerData = newData;
        SavePlayerData();
    }
    public PlayerData GetPlayerData()
    {
        playerData = JsonMgr.Instance.LoadData<PlayerData>("PlayerData");
        return playerData;
    }
}
