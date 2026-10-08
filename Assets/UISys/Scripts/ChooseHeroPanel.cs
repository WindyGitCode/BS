using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ChooseHeroPanel : BasePanel
{
    public Camera cam;
    public Button btnBegin;
    public Button btnBack;
    public Button turnLeft;
    public Button turnRight;
    public Text heroName;//供面板刷新名字
    public List<GameObject> heroList;  // 从Resources加载的英雄预制体列表
    private int currentHeroIndex = 0;  // 当前显示的英雄索引
    private GameObject currentHeroObj; // 当前场景中显示的英雄对象
    // 关卡信息（外部任意时刻修改）
    public static E_LevelType selectedGameMode;
    public static int selectedLevelID;
    [Header("角色信息面板")]
    public GameObject heroInfo_0;
    public GameObject heroInfo_1;
    public GameObject heroInfo_2;
    public GameObject heroInfo_3;
    public GameObject heroInfo_4;
    //temp
    public GameObject unlockMask; 

    public override void Init()
    {
        selectedGameMode = ChoosePaternPanel.GameMode;
        selectedLevelID = ChooseLevelPanel.levelID;
        // 初始化相机
        cam = GameObject.Find("camera").GetComponent<Camera>();
        cam.gameObject.GetComponent<CameraRotate>().Rotate_Right();

        // 初始化英雄列表
        InitHeroList();

        // 加载第一个英雄
        if (heroList.Count > 0)
        {
            LoadHero(currentHeroIndex);
        }

        // 注册按钮事件
        btnBegin.onClick.AddListener(() =>
        {
            AudioMgr.Instance.PlayBtnClick();
            // 未解锁拦截
            if (unlockMask.activeSelf)
            {
                TempTipPanel tip = UIMgr.Instance.ShowPanel<TempTipPanel>();
                tip.ShowTipAutoHide("当前英雄未解锁，无法选择！", 2f);
                return;
            }
            UIMgr.Instance.HidePanel<ChooseHeroPanel>();
            //刷新玩家数据管理器中的当前角色配置
            PlayerDataMgr.Instance.playerState.currentHeroName = heroList[currentHeroIndex].name;
            PlayerDataMgr.Instance.SwitchHeroConfig(PlayerDataMgr.Instance.playerState.currentHeroName);
            //Debug.Log($"选择了英雄：{PlayerDataMgr.Instance.playerState.currentHeroName}，准备进入游戏场景...");
            // 根据选择的游戏模式加载对应场景
            LoadSceneByGameMode();
        });

        btnBack.onClick.AddListener(() =>
        {
            AudioMgr.Instance.PlayBtnClick();
            UIMgr.Instance.HidePanel<ChooseHeroPanel>();
            UIMgr.Instance.ShowPanel<BeginPanel>();
        });

        // 注册左右切换按钮事件
        turnLeft.onClick.AddListener(() =>
        {
            AudioMgr.Instance.PlayBtnClick();
            LoadHero(currentHeroIndex - 1);
        });
        turnRight.onClick.AddListener(() =>
        {
            AudioMgr.Instance.PlayBtnClick();
            LoadHero(currentHeroIndex + 1);
        });
    }
    //选择场景
    void LoadSceneByGameMode()
    {
        switch (selectedGameMode)
        {
            case E_LevelType.TowerDefense:
                SceneManager.LoadScene($"TowerDefence_{ChooseLevelPanel.levelID}");
                break;
            case E_LevelType.Survival:
                SceneManager.LoadScene($"Survival_{ChooseMapPanel.mapID}");
                break;
            case E_LevelType.Endless:
                SceneManager.LoadScene("EndlessScene");
                break;
            default:
                Debug.LogError("未知的游戏模式，无法加载场景");
                break;
        }
    }
    #region 刷新英雄名字
    private List<string> heroNameList = new List<string>()
    {
        "警官",
        "工程师",
        "医疗兵",
        "火焰兵",
        "指挥官"
    };
    private void RefreshHeroName()
    {
        if (heroName == null) return;
        if (currentHeroIndex < 0 || currentHeroIndex >= heroNameList.Count) return;

        heroName.text = heroNameList[currentHeroIndex];
    }
    #endregion

    // 初始化英雄列表（从Resources文件夹加载）
    private void InitHeroList()
    {
        heroList = new List<GameObject>();

        // 按指定名称列表加载
        string[] heroNames = {
            "ToonSoldiers_gunner",
            "ToonSoldiers_engineer",
            "ToonSoldiers_medic",
            "ToonSoldiers_flammer",
            "ToonSoldiers_officer"
        };
        foreach (string name in heroNames)
        {
            GameObject heroPrefab = Resources.Load<GameObject>($"Prefabs/Role/{name}");
            if (heroPrefab != null)
            {
                // 安全获取动画组件
                Animator anim = heroPrefab.GetComponent<Animator>();
                if (anim != null)
                {
                    anim.runtimeAnimatorController = Resources.Load<RuntimeAnimatorController>("Controller/ChoosePanel_Pose_Controller");
                }
                heroList.Add(heroPrefab);
            }
            else
            {
                Debug.LogWarning($"未找到英雄预制体：Resources/Prefabs/Role/{name}");
            }
        }
    }

    /// <summary>
    /// 加载指定索引的英雄
    /// </summary>
    /// <param name="index">英雄列表索引</param>
    private void LoadHero(int index)
    {
        // 边界检查
        if (heroList.Count == 0)
        {
            Debug.Log("英雄列表为空，请检查Resources目录下的英雄预制体");
            return;
        }

        // 确保索引在有效范围内（循环切换）
        index = (index + heroList.Count) % heroList.Count;
        currentHeroIndex = index;

        // 销毁当前显示的英雄
        if (currentHeroObj != null)
        {
            Destroy(currentHeroObj);
        }

        // 实例化新英雄
        GameObject heroPrefab = heroList[currentHeroIndex];
        currentHeroObj = Instantiate(heroPrefab);

        // 设置英雄位置和旋转
        currentHeroObj.transform.position = new Vector3(218, 29, 218); // 英雄显示位置
        currentHeroObj.transform.rotation = Quaternion.Euler(new Vector3(0, -180, 0));  // 初始旋转
        currentHeroObj.transform.localScale = Vector3.one;        // 缩放
        //刷新英雄名字
        RefreshHeroName();
        // 更新角色信息
        ShowHeroInfoByIndex(currentHeroIndex);
        //temp
        string heroName = heroPrefab.name;
         unlockMask.SetActive(heroName != "ToonSoldiers_engineer" && heroName != "ToonSoldiers_gunner");
    }
    //角色信息更新
    private void ShowHeroInfoByIndex(int index)
    {
        // 全部隐藏
        heroInfo_0?.SetActive(false);
        heroInfo_1?.SetActive(false);
        heroInfo_2?.SetActive(false);
        heroInfo_3?.SetActive(false);
        heroInfo_4?.SetActive(false);

        // 显示当前 index 对应的信息面板
        switch (index)
        {
            case 0: heroInfo_0?.SetActive(true); break;
            case 1: heroInfo_1?.SetActive(true); break;
            case 2: heroInfo_2?.SetActive(true); break;
            case 3: heroInfo_3?.SetActive(true); break;
            case 4: heroInfo_4?.SetActive(true); break;
        }
    }
    private void OnDestroy()
    {
        if (currentHeroObj != null)
            Destroy(currentHeroObj);

        if (cam != null && cam.gameObject.GetComponent<CameraRotate>() != null)
            cam.gameObject.GetComponent<CameraRotate>().Rotate_Left();
    }
}