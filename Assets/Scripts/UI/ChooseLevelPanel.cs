using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChooseLevelPanel : BasePanel
{
    public Button btnNext;
    public Button btnBack;
    public override void Init()
    {
        btnNext.onClick.AddListener(() =>
        {
            UIMgr.Instance.HidePanel<ChooseLevelPanel>();
            UIMgr.Instance.ShowPanel<ChooseHeroPanel>();
        });
        btnBack.onClick.AddListener(() =>
        {
            UIMgr.Instance.HidePanel<ChooseLevelPanel>();
            UIMgr.Instance.ShowPanel<ChoosePaternPanel>();
        });
    }
}
