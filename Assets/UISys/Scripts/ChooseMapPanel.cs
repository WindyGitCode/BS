using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// 地图数据结构
[System.Serializable]
public class MapData
{
    public string mapName;         // 地图名称
    public string mapDesc;         // 地图介绍
    public Sprite mapSprite;       // 地图预览图
    public string sceneName;       // 对应场景名
    // 关卡战斗配置
    public int maxWaveCount;     // 最大波数
    public int totalMonster;     // 总怪物数量
}

public class ChooseMapPanel : BasePanel
{
    [Header("切换按钮")]
    public Button lastMap;
    public Button nextMap;
    public Button btnNext;
    public Button btnBack;
    [Header("UI组件")]
    public Text mapName;
    public Text mapDescription;
    public Image mapImage;

    private List<MapData> mapDataList;
    private int curMapIndex; // 当前选中地图下标
    public static int mapID; // 关卡ID


    public override void Init()
    {
        InitMapConfigData();
        // 固定按钮逻辑
        btnNext.onClick.AddListener(() =>
        {
            AudioMgr.Instance.PlayBtnClick();
            // 记录选择的地图ID
            mapID = curMapIndex+1; 
            UIMgr.Instance.HidePanel<ChooseMapPanel>();
            UIMgr.Instance.ShowPanel<ChooseHeroPanel>();
        });

        btnBack.onClick.AddListener(() =>
        {
            AudioMgr.Instance.PlayBtnClick();
            UIMgr.Instance.HidePanel<ChooseMapPanel>();
            UIMgr.Instance.ShowPanel<ChoosePaternPanel>();
        });

        // 左右切换地图
        lastMap.onClick.AddListener(PrevMap);
        nextMap.onClick.AddListener(NextMap);

        // 初始化渲染第一张
        curMapIndex = 0;
        RefreshMapUI();
    }

    // 刷新当前地图所有UI
    void RefreshMapUI()
    {
        if (mapDataList == null || mapDataList.Count == 0) return;

        MapData data = mapDataList[curMapIndex];
        mapName.text = data.mapName;
        mapDescription.text = data.mapDesc;
        mapImage.sprite = data.mapSprite;
    }

    // 上一张地图
    void PrevMap()
    {
        AudioMgr.Instance.PlayBtnClick();
        curMapIndex--;
        if (curMapIndex < 0)
            curMapIndex = mapDataList.Count - 1;

        RefreshMapUI();

    }

    // 下一张地图
    void NextMap()
    {
        AudioMgr.Instance.PlayBtnClick();
        curMapIndex++;
        if (curMapIndex >= mapDataList.Count)
            curMapIndex = 0;

        RefreshMapUI();

    }
    void InitMapConfigData()
    {
        mapDataList = new List<MapData>();

        // 地图1 配置
        mapDataList.Add(new MapData()
        {
            mapName = "森林小镇",
            mapDesc = "适合新手的简易防守地图，怪物数量较少，难度偏低。",
            mapSprite = Resources.Load<Sprite>("Sprite/map1"),
            sceneName = "TownScene",
            maxWaveCount = 8,
            totalMonster = 25
        });

        // 地图2 配置
        mapDataList.Add(new MapData()
        {
            mapName = "高原雪地",
            mapDesc = "雪天场景，怪物刷新速度更快，波次更多。",
            mapSprite = Resources.Load<Sprite>("Sprite/map2"),
            sceneName = "FactoryScene",
            maxWaveCount = 12,
            totalMonster = 40
        });

        // 地图3 配置
        mapDataList.Add(new MapData()
        {
            mapName = "军区防线",
            mapDesc = "高难度生存地图，大量僵尸围攻，适合熟练玩家。",
            mapSprite = Resources.Load<Sprite>("Sprite/map3"),
            sceneName = "StreetScene",
            maxWaveCount = 15,
            totalMonster = 60
        });

    }
    // 外部获取当前选中地图（给关卡用）
    public MapData GetCurSelectMap()
    {
        if (mapDataList == null) return null;
        return mapDataList[curMapIndex];
    }
}