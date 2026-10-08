using UnityEngine;
using UnityEngine.UI;

public class AchievementPanel : BasePanel
{
    [Header("UI绑定")]
    public Text txtTitle;        // 成就名称
    public Text txtDesc;         // 成就描述
    public Image imgIcon;        // 成就图标
    public Text txtProgress;     // 成就进度
    public Button btnPre;        // 上一个
    public Button btnNext;       // 下一个
    public Button btnBack;       // 返回

    [Header("成就奖励")]
    public Button btnGetReward;  // 领取奖励按钮

    private int currentIndex = 0; // 当前看第几个成就

    public override void Init()
    {
        // 按钮绑定
        btnPre.onClick.AddListener(ShowPrevious);
        btnNext.onClick.AddListener(ShowNext);
        btnBack.onClick.AddListener(() =>
        {
            AudioMgr.Instance.PlayBtnClick();
            UIMgr.Instance.HidePanel<AchievementPanel>();
            UIMgr.Instance.ShowPanel<BeginPanel>();
        });
        // 绑定领取奖励按钮
        btnGetReward.onClick.AddListener(OnGetRewardClick);

        // 打开面板默认显示第一个成就
        ShowAchievementAt(0);
    }

    /// <summary>
    /// 显示指定索引的成就
    /// </summary>
    void ShowAchievementAt(int index)
    {
        var allAch = AchievementMgr.Instance.allAchievements;

        // 越界保护
        if (allAch == null || allAch.Count == 0) return;
        if (index < 0) index = 0;
        if (index >= allAch.Count) index = allAch.Count - 1;

        currentIndex = index;
        var data = allAch[currentIndex];

        // 刷新UI
        txtTitle.text = data.title;
        txtDesc.text = data.desc;

        // 已解锁 / 未解锁 显示不同图标
        bool unlocked = AchievementMgr.Instance.IsUnlocked(data.achievementID);
        imgIcon.sprite = unlocked ? data.iconUnlocked : data.iconLocked;
        int currentPro = AchievementMgr.Instance.GetAchievementProgress(data.achievementID);

        bool rewardTaken = AchievementMgr.Instance.IsRewardTaken(data.achievementID);

        if (unlocked)
        {
            txtProgress.text = "已完成";

            // 已完成 + 未领奖 → 显示按钮
            btnGetReward.gameObject.SetActive(!rewardTaken);
        }
        else
        {
            txtProgress.text = $"进度：{currentPro}/{data.targetCount}";
            btnGetReward.gameObject.SetActive(false); // 未完成 → 隐藏
        }
    }

    /// <summary>
    /// 点击领取成就奖励
    /// </summary>
    void OnGetRewardClick()
    {
        AudioMgr.Instance.PlayBtnClick();

        var achData = AchievementMgr.Instance.allAchievements[currentIndex];
        bool ok = AchievementMgr.Instance.TakeAchievementReward(achData.achievementID);

        if (ok)
        {
            btnGetReward.gameObject.SetActive(false); // 领取后隐藏按钮
            UIMgr.Instance.ShowPanel<TempTipPanel>().ShowTipAutoHide($"领取成功！获得 {achData.rewardGold} 金币", 2f);
        }
    }

    void ShowPrevious()
    {
        AudioMgr.Instance.PlayBtnClick();
        ShowAchievementAt(currentIndex - 1);
    }

    void ShowNext()
    {
        AudioMgr.Instance.PlayBtnClick();
        ShowAchievementAt(currentIndex + 1);
    }
}