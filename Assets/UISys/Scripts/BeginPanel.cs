using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class BeginPanel : BasePanel
{
    public Button btnBegin;
    public Button btnSetting;
    public Button btnAchievement;
    public Button btnIntroduction;
    public Button btnMarget;
    public Button btnExit;
    public override void Init()
    {
        btnBegin.onClick.AddListener(() =>
        {
            //触发按钮点击事件（播放音效）
            EventMgr.Instance.Trigger(EventConst.ButtonClicked);
            UIMgr.Instance.ShowPanel<ChoosePaternPanel>();
            UIMgr.Instance.HidePanel<BeginPanel>();
            Debug.Log(Application.persistentDataPath);
        });
        btnSetting.onClick.AddListener(() =>
        {
            //触发按钮点击事件（播放音效）
            EventMgr.Instance.Trigger(EventConst.ButtonClicked);
            UIMgr.Instance.ShowPanel<SettingPanel>();
            UIMgr.Instance.HidePanel<BeginPanel>();
        });
        btnIntroduction.onClick.AddListener(() =>
        {
            //触发按钮点击事件（播放音效）
            EventMgr.Instance.Trigger(EventConst.ButtonClicked);
            UIMgr.Instance.ShowPanel<IntroducePanel>();
            UIMgr.Instance.HidePanel<BeginPanel>();
        });
        btnAchievement.onClick.AddListener(() => {
            //触发按钮点击事件（播放音效）
            EventMgr.Instance.Trigger(EventConst.ButtonClicked);
            UIMgr.Instance.ShowPanel<AchievementPanel>();
            UIMgr.Instance.HidePanel<BeginPanel>();
        });
        btnMarget.onClick.AddListener(() =>
        {
            //触发按钮点击事件（播放音效）
            EventMgr.Instance.Trigger(EventConst.ButtonClicked);
            UIMgr.Instance.ShowPanel<MarketPanel>();
            UIMgr.Instance.HidePanel<BeginPanel>();
        });
        btnExit.onClick.AddListener(() =>
        {
            Application.Quit();
        });
    }
}
