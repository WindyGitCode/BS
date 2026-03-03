using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BeginPanel : BasePanel
{
    public Button btnBegin;
    public Button btnSetting;
    public Button btnAbout;
    public Button btnExit;

    public override void Init()
    {
        btnBegin.onClick.AddListener(() =>
        {
            UIMgr.Instance.ShowPanel<ChoosePaternPanel>();
            UIMgr.Instance.HidePanel<BeginPanel>();
        });
        btnSetting.onClick.AddListener(() =>
        {
            UIMgr.Instance.ShowPanel<SettingPanel>();
                UIMgr.Instance.HidePanel<BeginPanel>();
        });
        btnAbout.onClick.AddListener(() => {
            //#
        });
        btnExit.onClick.AddListener(() =>
        {
            //
        });
    }
}
