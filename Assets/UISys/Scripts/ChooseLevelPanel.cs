using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChooseLevelPanel : BasePanel
{
    public Button btnBack;
    public Button btnLevel_1;
    public Button btnLevel_2;
    public Button btnLevel_3;
    public Button btnLevel_4;
    public Button btnLevel_5;
    public Button btnLevel_6;
    public Button btnLevel_7;
    public Button btnLevel_8;
    public Button btnLevel_9;
    public Button btnLevel_10;

    public Button btnLevel_2_Mask;
    public Button btnLevel_3_Mask;
    public Button btnLevel_4_Mask;
    public Button btnLevel_5_Mask;
    public Button btnLevel_6_Mask;
    public Button btnLevel_7_Mask;
    public Button btnLevel_8_Mask;
    public Button btnLevel_9_Mask;
    public Button btnLevel_10_Mask;
    //其他
    public static int levelID;
    public override void Init()
    {
        btnBack.onClick.AddListener(() =>
        {
            AudioMgr.Instance.PlayBtnClick();
            UIMgr.Instance.HidePanel<ChooseLevelPanel>();
            UIMgr.Instance.ShowPanel<ChoosePaternPanel>();
        });
        btnLevel_1.onClick.AddListener(() =>
        {
            AudioMgr.Instance.PlayBtnClick();
            levelID = 1;
            UIMgr.Instance.HidePanel<ChooseLevelPanel>();
            UIMgr.Instance.ShowPanel<ChooseHeroPanel>();
        });
        btnLevel_2.onClick.AddListener(() =>
        {
            AudioMgr.Instance.PlayBtnClick();
            levelID = 2;
            UIMgr.Instance.HidePanel<ChooseLevelPanel>();
            UIMgr.Instance.ShowPanel<ChooseHeroPanel>();
        });
        btnLevel_3.onClick.AddListener(() =>
        {
            AudioMgr.Instance.PlayBtnClick();
            levelID = 3;
            UIMgr.Instance.HidePanel<ChooseLevelPanel>();
            UIMgr.Instance.ShowPanel<ChooseHeroPanel>();
        });

        //遮罩点击提示
        btnLevel_2_Mask.onClick.AddListener(() =>
        {
            AudioMgr.Instance.PlayBtnClick();
            UIMgr.Instance.ShowPanel<TempTipPanel>()?.ShowTip1AutoHide("完成前置关卡后开放！");
        });
        btnLevel_3_Mask.onClick.AddListener(() =>
        {
            AudioMgr.Instance.PlayBtnClick();
            UIMgr.Instance.ShowPanel<TempTipPanel>()?.ShowTip1AutoHide("完成前置关卡后开放！");
        });
        btnLevel_4_Mask.onClick.AddListener(() =>
        {
            AudioMgr.Instance.PlayBtnClick();
            UIMgr.Instance.ShowPanel<TempTipPanel>()?.ShowTip1AutoHide("完成前置关卡后开放！");
        });
        btnLevel_5_Mask.onClick.AddListener(() =>
        {
            AudioMgr.Instance.PlayBtnClick();
            UIMgr.Instance.ShowPanel<TempTipPanel>()?.ShowTip1AutoHide("完成前置关卡后开放！");
        });
        btnLevel_6_Mask.onClick.AddListener(() =>
        {
            AudioMgr.Instance.PlayBtnClick();
            UIMgr.Instance.ShowPanel<TempTipPanel>()?.ShowTip1AutoHide("完成前置关卡后开放！");
        });
        btnLevel_7_Mask.onClick.AddListener(() =>
        {
            AudioMgr.Instance.PlayBtnClick();
            UIMgr.Instance.ShowPanel<TempTipPanel>()?.ShowTip1AutoHide("完成前置关卡后开放！");
        });
        btnLevel_8_Mask.onClick.AddListener(() =>
        {
            AudioMgr.Instance.PlayBtnClick();
            UIMgr.Instance.ShowPanel<TempTipPanel>()?.ShowTip1AutoHide("完成前置关卡后开放！");
        });
        btnLevel_9_Mask.onClick.AddListener(() =>
        {
            AudioMgr.Instance.PlayBtnClick();
            UIMgr.Instance.ShowPanel<TempTipPanel>()?.ShowTip1AutoHide("完成前置关卡后开放！");
        });
        btnLevel_10_Mask.onClick.AddListener(() =>
        {
            AudioMgr.Instance.PlayBtnClick();
            UIMgr.Instance.ShowPanel<TempTipPanel>()?.ShowTip1AutoHide("完成前置关卡后开放！");
        });
    }
}
