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
        btnExit.onClick.AddListener(() =>
        {
            UIMgr.Instance.HidePanel<SettingPanel>();
            UIMgr.Instance.ShowPanel<BeginPanel>();
        });
        toggleMusic.onValueChanged.AddListener((isOn) =>
        {
            //设置音乐开关
        });
        toggleSound.onValueChanged.AddListener((isOn) =>
        {
            //设置音效开关
        });
        sliderMusic.onValueChanged.AddListener((value) =>
        {
            //设置音乐音量
        });
        sliderSound.onValueChanged.AddListener((value) =>
        {
            //设置音效音量
        });
    }
}
