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
        toggleMusic.isOn = BKmusic.Instance.musicData.isMusicOn;
        toggleSound.isOn = BKmusic.Instance.musicData.isSoundOn;
        sliderMusic.value = BKmusic.Instance.musicData.musicVolume;
        sliderSound.value = BKmusic.Instance.musicData.soundVolume;
        btnExit.onClick.AddListener(() =>
        {
            GameDataMgr.Instance.SaveMusicData();
            UIMgr.Instance.HidePanel<SettingPanel>();
            UIMgr.Instance.ShowPanel<BeginPanel>();
        });
        toggleMusic.onValueChanged.AddListener((isOn) =>
        {
            //设置音乐开关
            BKmusic.Instance.SetBKMusicIsOn(isOn);
            BKmusic.Instance.musicData.isMusicOn = isOn;
        });
        toggleSound.onValueChanged.AddListener((isOn) =>
        {
            //设置音效开关
            BKmusic.Instance.SetBKSoundIsOn(isOn);
            BKmusic.Instance.musicData.isSoundOn = isOn;
        });
        sliderMusic.onValueChanged.AddListener((value) =>
        {
            //设置音乐音量
            BKmusic.Instance.SetMusicVolume(value);
            BKmusic.Instance.musicData.musicVolume = value;
        });
        sliderSound.onValueChanged.AddListener((value) =>
        {
            //设置音效音量
            BKmusic.Instance.setSoundVolume(value);
            BKmusic.Instance.musicData.soundVolume = value;
        });
    }
}
