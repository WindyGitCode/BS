using UnityEngine;
using System.Collections;

public class GamePauseMgr : MonoBehaviour
{
    public static GamePauseMgr Instance;
    public bool isGamePause { get; private set; }
    private KeyCode pauseKey;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        pauseKey=GameDataMgr.Instance.GetGameData().pauseKey;
    }

    private void Update()
    {
        if (Input.GetKeyDown(pauseKey))
        {
            if (isGamePause)
                ResumeGame();
            else
                PauseGame();
        }
    }

    public void PauseGame()
    {
        if (isGamePause) return;
        isGamePause = true;

        // 1. 【优先】立刻打开面板、显示鼠标（时间还在正常走）
        UIMgr.Instance.ShowPanel<GamePausePanel>();
        CursorMgr.ShowMouse();

        // 2. 延迟1秒后再冻结游戏时间，给UI足够渲染时间
        StartCoroutine(PauseGameDelay());
    }

    // 恢复：先解冻时间，再关面板
    public void ResumeGame()
    {
        isGamePause = false;
        Time.timeScale = 1;
        UIMgr.Instance.HidePanel<GamePausePanel>();
        CursorMgr.HideMouse();
    }

    private IEnumerator PauseGameDelay()
    {
        // 给足 1 秒，面板、动画、UI框架 完全加载完毕
        yield return new WaitForSeconds(0.5f);

        // UI彻底显示完，再暂停
        Time.timeScale = 0;
    }
}