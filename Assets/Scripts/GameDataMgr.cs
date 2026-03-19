using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameDataMgr
{
    private static GameDataMgr instance=new GameDataMgr();
    public static GameDataMgr Instance => instance;
    public MusicData musicData;
    public PlayerData playerData;
    private GameDataMgr()
    {
        musicData=JsonMgr.Instance.LoadData<MusicData>("MusicData");
        playerData=JsonMgr.Instance.LoadData<PlayerData>("PlayerData");
    }
    public void SaveMusicData()
    {
        JsonMgr.Instance.SaveData(musicData, "MusicData");
    }
}
