using UnityEngine;

public class SurvivalControllerNPC : Interactable
{
    private bool _isInSafeTime;
    private GameObject _player;

    private void Start()
    {
        _player = GameObject.FindGameObjectWithTag("Player");

        // 监听安全时间开始
        EventMgr.Instance.AddListener(EventConst.SurvivalSafeTimeStart, OnSafeTimeStart);
        // 监听安全时间结束 隐藏自己
        EventMgr.Instance.AddListener(EventConst.HideSurvivalNPC, HideSelf);
    }

    public override void OnInteract()
    {
        string[] sentences;

        if (SurvivalLevelMgr.Instance.currentState == SurvivalState.Idle)
        {
            // 【未开始】显示开始游戏
            sentences = new[]
            {
                "你好，勇士！",
                "此处有大量丧尸出没，搜集物资时请注意安全。",
                "如果你需要帮助，请告诉我，我可以带你撤离此地!"
            };
        }
        else
        {
            // 【安全时间】显示撤离
            sentences = new[]
            {
                "此地不宜久留",
                "跟紧我，我带你离开这里！"
            };
        }

        DialogData dialogData = new DialogData
        {
            name = "教官",
            sentences = sentences
        };
        DialogMgr.Instance.StartDialog(dialogData, OnDialogFinish);
    }

    // 对话结束后的逻辑
    private void OnDialogFinish()
    {
        if (this == null) return;

        if (SurvivalLevelMgr.Instance.currentState == SurvivalState.Idle)
        {
            // 开始游戏
            SurvivalLevelMgr.Instance.StartGame();
            gameObject.SetActive(false); // 隐藏自己
        }
        else if (SurvivalLevelMgr.Instance.currentState == SurvivalState.SafeTime)
        {
            // 撤离 → 胜利
            SurvivalLevelMgr.Instance.WinGame();
        }
    }

    // 安全时间：显示自己 + 瞬移到玩家身边
    private void OnSafeTimeStart()
    {
        if (_player == null) return;

        gameObject.SetActive(true);
        transform.position = _player.transform.position + _player.transform.right * 4; // 瞬移到玩家右侧4米处
        transform.LookAt(_player.transform);
    }
    public void OnEnable()
    {
        if (SurvivalLevelMgr.Instance != null)
        {
            if (SurvivalLevelMgr.Instance.currentState == SurvivalState.SafeTime)
            {
                interactTip = "撤离";
            }
            else
            {
                interactTip = "开始游戏";
            }
        }
        else
        {
            //默认显示开始游戏
            interactTip = "开始游戏";
        }
    }
    private void HideSelf()
    {
        gameObject.SetActive(false);
    }
    private void OnDestroy()
    {
        EventMgr.Instance.RemoveListener(EventConst.SurvivalSafeTimeStart, OnSafeTimeStart);
        EventMgr.Instance.RemoveListener(EventConst.HideSurvivalNPC, HideSelf);
    }
}