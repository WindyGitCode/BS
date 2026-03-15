using System.Collections;
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
    public string heroName;
    public List<GameObject> heroList;  // 从Resources加载的英雄预制体列表
    private int currentHeroIndex = 0;  // 当前显示的英雄索引
    private GameObject currentHeroObj; // 当前场景中显示的英雄对象

    public override void Init()
    {
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
            UIMgr.Instance.HidePanel<ChooseHeroPanel>();
            SceneManager.LoadScene("TowerDefenceScene");
        });

        btnBack.onClick.AddListener(() =>
        {
            UIMgr.Instance.HidePanel<ChooseHeroPanel>();
            UIMgr.Instance.ShowPanel<BeginPanel>();
        });

        // 注册左右切换按钮事件
        turnLeft.onClick.AddListener(() =>
        {
            LoadHero(currentHeroIndex - 1);
        });
        turnRight.onClick.AddListener(() =>
        {
            LoadHero(currentHeroIndex + 1);
        });
    }

    /// <summary>
    /// 初始化英雄列表（从Resources文件夹加载）
    /// </summary>
    private void InitHeroList()
    {
        heroList = new List<GameObject>();

        // 按指定名称列表加载
        string[] heroNames = {
            "ToonSoldiers_engineer",
            "ToonSoldiers_gunner",
            "ToonSoldiers_medic",
            "ToonSoldiers_flammer",
            "ToonSoldiers_officer"
        };
        foreach (string name in heroNames)
        {
            GameObject heroPrefab = Resources.Load<GameObject>($"Prefabs/Role/{name}");
            if (heroPrefab != null)
            {
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
        currentHeroObj.transform.position = new Vector3(217, 29, 218); // 英雄显示位置
        currentHeroObj.transform.rotation = Quaternion.Euler(new Vector3(0, -180, 0));  // 初始旋转
        currentHeroObj.transform.localScale = Vector3.one;        // 缩放
    }
    private void OnDestroy()
    {
        Destroy(currentHeroObj);
        cam.gameObject.GetComponent<CameraRotate>().Rotate_Left();
    }
}
