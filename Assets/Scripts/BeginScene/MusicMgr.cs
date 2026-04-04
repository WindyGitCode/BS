using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicMgr : MonoBehaviour
{
    private static MusicMgr instance;
    public static MusicMgr Instance => instance;
    private AudioSource audioSource;
    public MusicData musicData;
    public void Awake()
    {
        instance=this;
        audioSource = GetComponent<AudioSource>();
        musicData = JsonMgr.Instance.LoadData<MusicData>("MusicData");
        SetBKMusicIsOn(musicData.isMusicOn);
        SetMusicVolume(musicData.musicVolume);
    }
    public void SetBKMusicIsOn(bool isOn)
    {
        audioSource.mute=!isOn;
    }
    public void SetBKSoundIsOn(bool isOn)
    {
        //设置音效开关
    }
    public void SetMusicVolume(float volume)
    {
        audioSource.volume=volume;
    }
    public void setSoundVolume(float volume)
    {
        //设置音效音量
    }
    public void SaveMusicData()
    {
        JsonMgr.Instance.SaveData(musicData, "MusicData");
    }
}
