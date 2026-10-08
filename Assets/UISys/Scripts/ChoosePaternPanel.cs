using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ChoosePaternPanel : BasePanel
{
    public Button btnBack;
    public Button btnTower;
    public Button btnSurvival;
    public Button btnEndless;
    public Transform locklevel;
    //其他
    public bool isEndlessUnLocked = false;//是否解锁了无尽模式（根据游戏进度设置）
    public static E_LevelType GameMode;//供外部访问
    public GameObject secretUnlockBtn;//提前解锁按钮
    public override void Init()
    {
        if (secretUnlockBtn != null)
            secretUnlockBtn.SetActive(false);
        btnBack.onClick.AddListener(() =>
        {
            AudioMgr.Instance.PlayBtnClick();
            UIMgr.Instance.HidePanel<ChoosePaternPanel>();
            UIMgr.Instance.ShowPanel<BeginPanel>();
        });
        btnTower.onClick.AddListener(() =>
        {
            AudioMgr.Instance.PlayBtnClick();
            GameMode = E_LevelType.TowerDefense;
            UIMgr.Instance.HidePanel<ChoosePaternPanel>();
            UIMgr.Instance.ShowPanel<ChooseLevelPanel>();
        });
        btnSurvival.onClick.AddListener(() =>
        {
            AudioMgr.Instance.PlayBtnClick();
            GameMode = E_LevelType.Survival;
            UIMgr.Instance.HidePanel<ChoosePaternPanel>();
            UIMgr.Instance.ShowPanel<ChooseMapPanel>();
        });
        btnEndless.onClick.AddListener(() =>
        {
            AudioMgr.Instance.PlayBtnClick();
            if (!isEndlessUnLocked)
            {
                UIMgr.Instance.ShowPanel<TempTipPanel>()?.ShowTip1AutoHide("完成所有塔防关卡后开放！");
                Debug.Log("Endless模式尚未开发，敬请期待！");
            }
            else
            {
                GameMode = E_LevelType.Endless;
                UIMgr.Instance.HidePanel<ChoosePaternPanel>();
                UIMgr.Instance.ShowPanel<ChooseHeroPanel>();
            }
        });
        if (secretUnlockBtn != null)
        {
            secretUnlockBtn.GetComponent<Button>().onClick.AddListener(UnlockEndlessMode);
        }
    }
    public override void Update()
    {
        base.Update();

        if (Input.GetKeyDown(KeyCode.K))
        {
            if (secretUnlockBtn != null && !secretUnlockBtn.activeSelf)
            {
                secretUnlockBtn.SetActive(true);
            }
        }

        if (isEndlessUnLocked)
        {
            locklevel.gameObject.SetActive(false);
        }
        else
        {
            locklevel.gameObject.SetActive(true);
        }
    }
    private void UnlockEndlessMode()
    {
        isEndlessUnLocked = true;
        Debug.Log("无尽模式已解锁！");

        // 解锁后隐藏按钮
        if (secretUnlockBtn != null)
            secretUnlockBtn.SetActive(false);
    }
}
