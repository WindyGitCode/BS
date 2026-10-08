using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TempTipPanel : BasePanel
{
    public Text tipText;
    public Text tipText1;
    public Text tipText2;
    public Text tipText3;
    public override void Init()
    {

    }

    //---------------------------------------------------------------------------------------
    public void SetTip(string tip)
    {
        tipText.text = tip;
        tipText1.text = "";
        tipText2.text = "";
        tipText3.text = "";
    }
    public void ShowTipAutoHide(string message, float showTime = 1.5f)
    {
        SetTip(message);
        UIMgr.Instance.ShowPanel<TempTipPanel>();
        StopAllCoroutines();
        StartCoroutine(AutoHideCoroutine(showTime));
    }
    //---------------------------------------------------------------------------------------
    public void SetTip1(string tip)
    {
        tipText.text ="";
        tipText1.text = tip;
        tipText2.text = "";
        tipText3.text = "";
    }
    public void ShowTip1AutoHide(string message, float showTime = 1.5f)
    {
        SetTip1(message);
        UIMgr.Instance.ShowPanel<TempTipPanel>();
        StopAllCoroutines();
        StartCoroutine(AutoHideCoroutine(showTime));
    }
    //---------------------------------------------------------------------------------------
    public void SetTip2(string tip)
    {
        tipText.text = "";
        tipText1.text = "";
        tipText2.text = tip;
        tipText3.text = "";
    }
    public void ShowTip2AutoHide(string message, float showTime = 1.5f)
    {
        SetTip2(message);
        UIMgr.Instance.ShowPanel<TempTipPanel>();
        StopAllCoroutines();
        StartCoroutine(AutoHideCoroutine(showTime));
    }
    //---------------------------------------------------------------------------------------
    public void SetTip3(string tip)
    {
        tipText.text = "";
        tipText1.text = "";
        tipText2.text = "";
        tipText3.text = tip;
    }
    public void ShowTip3AutoHide(string message, float showTime = 1.5f)
    {
        SetTip3(message);
        UIMgr.Instance.ShowPanel<TempTipPanel>();
        StopAllCoroutines();
        StartCoroutine(AutoHideCoroutine(showTime));
    }
    // ==================== 通用自动关闭 ====================
    private IEnumerator AutoHideCoroutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        UIMgr.Instance.HidePanel<TempTipPanel>();
    }
}