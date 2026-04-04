using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class TipPanel : BasePanel
{
    public Button btnConfirm;
    public Text tipText;
    public bool cursorState;
    public override void Init()
    {
        btnConfirm.onClick.AddListener(() =>
        {
            UIMgr.Instance.HidePanel<TipPanel>();
        });
    }
    public override void ShowMe()
    {
        base.ShowMe();
        cursorState = Cursor.visible;
        CursorController.ShowMouse();
    }
    public override void HideMe(UnityAction unityAction)
    {
        base.HideMe(unityAction);
        if(!cursorState)
            CursorController.HideMouse();
    }
}
