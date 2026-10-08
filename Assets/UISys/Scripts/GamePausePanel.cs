

using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GamePausePanel : BasePanel
{
    public Button continueGame;
    public Button backToMainMenu;
    public override void Init()
    {
        backToMainMenu.onClick.AddListener(() =>
        {
            AudioMgr.Instance.PlayBtnClick();
            GamePauseMgr.Instance.ResumeGame();
            CursorMgr.ShowMouse();
            UIMgr.Instance.HidePanel<GamingPanel>();
            if(ChoosePaternPanel.GameMode!=E_LevelType.TowerDefense)
                SurvivalLevelMgr.Instance.LoseGame();
            else if(ChoosePaternPanel.GameMode == E_LevelType.TowerDefense)
            {
                LevelMgr.Instance.FailLevel();
            }     
        });
        continueGame.onClick.AddListener(() =>
        {
            AudioMgr.Instance.PlayBtnClick();
            GamePauseMgr.Instance.ResumeGame();
        });
        
    }
}