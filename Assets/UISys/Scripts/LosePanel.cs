using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LosePanel : BasePanel
{
    public Button confirm;
    public override void Init()
    {
        confirm.onClick.AddListener(() =>
        {
            AudioMgr.Instance.PlayBtnClick();
            UIMgr.Instance.HidePanel<LosePanel>();
            UIMgr.Instance.HidePanel<GamingPanel>();
            SceneManager.LoadScene("BeginScene");
        });
    }
    public override void ShowMe()
    {
        base.ShowMe();
        CursorMgr.ShowMouse();
    }
}
