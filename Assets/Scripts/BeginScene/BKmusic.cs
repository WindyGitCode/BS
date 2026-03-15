using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BKmusic : MonoBehaviour
{
    private static BKmusic instance;
    public static BKmusic Instance => instance;
    private AudioSource audioSource;
    public MusicData musicData;
    public void Awake()
    {
        instance=this;
        audioSource = GetComponent<AudioSource>();
        musicData = GameDataMgr.Instance.musicData;
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
}
