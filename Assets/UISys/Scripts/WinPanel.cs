using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class WinPanel : BasePanel
{
    public Button BackToBegin;
    public Button NextLevel;
    public override void Init()
    {
        BackToBegin.onClick.AddListener(() =>
        {
            AudioMgr.Instance.PlayBtnClick();
            GamePauseMgr.Instance.ResumeGame();
            CursorMgr.ShowMouse();
            UIMgr.Instance.HidePanel<WinPanel>();
            UIMgr.Instance.HidePanel<GamingPanel>();
            SceneManager.LoadScene("BeginScene");
        });
        NextLevel.onClick.AddListener(() =>
        {
            AudioMgr.Instance.PlayBtnClick();
            Debug.Log("进入下一关");
            GamePauseMgr.Instance.ResumeGame();
            UIMgr.Instance.HidePanel<WinPanel>();
            ChooseLevelPanel.levelID++;
            SceneManager.LoadScene($"TowerDefence_{ChooseLevelPanel.levelID}");
        });
    }
    public override void ShowMe()
    {
        base.ShowMe();
        //暂停游戏，隐藏暂停界面
        GamePauseMgr.Instance.PauseGame();
        UIMgr.Instance.HidePanel<GamePausePanel>();

        CursorMgr.ShowMouse();
    }

}
