using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChoosePaternPanel : BasePanel
{
    public Button btnBack;
    public Button btnTower;
    public Button btnChallenge;
    public override void Init()
    {
        btnBack.onClick.AddListener(() =>
        {
            UIMgr.Instance.HidePanel<ChoosePaternPanel>();
            UIMgr.Instance.ShowPanel<BeginPanel>();
        });
        btnTower.onClick.AddListener(() =>
        {
            UIMgr.Instance.HidePanel<ChoosePaternPanel>();
            UIMgr.Instance.ShowPanel<ChooseHeroPanel>();
        });
        btnChallenge.onClick.AddListener(() =>
        {
            UIMgr.Instance.HidePanel<ChoosePaternPanel>();
            UIMgr.Instance.ShowPanel<ChooseLevelPanel>();
        });
    }
}
