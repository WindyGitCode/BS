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
            Debug.Log("BeginPanelClick");
        });
        btnSetting.onClick.AddListener(() =>
        {
            //#
        });
        btnAbout.onClick.AddListener(() => {
            //#
        });
        btnExit.onClick.AddListener(() =>
        {
            Application.Quit();
        });
    }
}
