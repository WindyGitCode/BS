using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 怪物数据管理器
/// 功能：读取JSON怪物配置 → 缓存数据 → 提供全局查询接口
/// </summary>
public class MonsterDataMgr
{
    private static MonsterDataMgr instance=new MonsterDataMgr();
    public static MonsterDataMgr Instance=>instance;
    // 所有怪物配置列表
    private MonsterDataList _monsterDataList;

    // 字典缓存：key = monsterID, value = MonsterData
    private Dictionary<int, MonsterConfig> monsterDataDict = new Dictionary<int, MonsterConfig>();

    private MonsterDataMgr()
    {
        //Debug.Log("正在初始化怪物数据管理器...");
        LoadAllMonsterDataFromJson();
        //Debug.Log("怪物数据管理器已初始化");
    }

    /// <summary>
    /// 从 Resources/Json/MonsterData.json 读取所有怪物配置
    /// </summary>
    private void LoadAllMonsterDataFromJson()
    {
        //通过JsonMgr加载数据
        _monsterDataList = JsonMgr.Instance.LoadData<MonsterDataList>("MonsterConfig");
        if ( _monsterDataList != null)
        {
            //Debug.LogFormat("从JSON加载怪物数据完成，加载到 {0} 条记录", _monsterDataList.monsterList.Count);
        }
        else
        {
            Debug.LogError("加载怪物数据失败，未找到JSON文件或解析错误");
            return;
        }
        // 构建字典（方便通过ID快速查找）
        monsterDataDict.Clear();
        foreach (MonsterConfig data in _monsterDataList.monsterList)
        {
            if (!monsterDataDict.ContainsKey(data.monsterID))
            {
                monsterDataDict.Add(data.monsterID, data);
            }
        }
    }

    /// <summary>
    /// 通过ID获取怪物数据（给MonsterController调用）
    /// </summary>
    public MonsterConfig GetMonsterDataByID(int monsterID)
    {
        if (monsterDataDict.TryGetValue(monsterID, out MonsterConfig data))
        {
            return data;
        }
        else
        {
            Debug.LogError($"未找到ID为 {monsterID} 的怪物数据");
            return null;
        }
    }

    /// <summary>
    /// 获取所有怪物数据（给怪物生成器用）
    /// </summary>
    public List<MonsterConfig> GetAllMonsterData()
    {
        return _monsterDataList.monsterList;
    }
}