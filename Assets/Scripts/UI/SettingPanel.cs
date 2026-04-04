using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingPanel : BasePanel
{
    public Button btnExit;
    public Toggle toggleMusic;
    public Toggle toggleSound;
    public Slider sliderMusic;
    public Slider sliderSound;
    public override void Init()
    {
        toggleMusic.isOn = MusicMgr.Instance.musicData.isMusicOn;
        toggleSound.isOn = MusicMgr.Instance.musicData.isSoundOn;
        sliderMusic.value = MusicMgr.Instance.musicData.musicVolume;
        sliderSound.value = MusicMgr.Instance.musicData.soundVolume;
        btnExit.onClick.AddListener(() =>
        {
            MusicMgr.Instance.SaveMusicData();
            UIMgr.Instance.HidePanel<SettingPanel>();
            UIMgr.Instance.ShowPanel<BeginPanel>();
        });
        toggleMusic.onValueChanged.AddListener((isOn) =>
        {
            //设置音乐开关
            MusicMgr.Instance.SetBKMusicIsOn(isOn);
            MusicMgr.Instance.musicData.isMusicOn = isOn;
        });
        toggleSound.onValueChanged.AddListener((isOn) =>
        {
            //设置音效开关
            MusicMgr.Instance.SetBKSoundIsOn(isOn);
            MusicMgr.Instance.musicData.isSoundOn = isOn;
        });
        sliderMusic.onValueChanged.AddListener((value) =>
        {
            //设置音乐音量
            MusicMgr.Instance.SetMusicVolume(value);
            MusicMgr.Instance.musicData.musicVolume = value;
        });
        sliderSound.onValueChanged.AddListener((value) =>
        {
            //设置音效音量
            MusicMgr.Instance.setSoundVolume(value);
            MusicMgr.Instance.musicData.soundVolume = value;
        });
    }
}
