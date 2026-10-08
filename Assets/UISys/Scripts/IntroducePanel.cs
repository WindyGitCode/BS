using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IntroducePanel : BasePanel
{
    public Button back;
    public override void Init()
    {
        back.onClick.AddListener(() =>
        {
            AudioMgr.Instance.PlayBtnClick();
            UIMgr.Instance.HidePanel<IntroducePanel>();
            UIMgr.Instance.ShowPanel<BeginPanel>();
        });
    }
}
