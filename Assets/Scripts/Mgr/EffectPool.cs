using System;
using System.Collections.Generic;
using UnityEngine;

public class EffectPool : MonoBehaviour
{
    // 单例
    public static EffectPool Instance { get; private set; }

    // 对象池核心容器
    private Dictionary<string, Queue<GameObject>> _poolDict = new Dictionary<string, Queue<GameObject>>();

    // 正在使用中的对象（用于自动回收）
    private Dictionary<GameObject, float> _usingObjects = new Dictionary<GameObject, float>();

    [Header("自动回收设置（秒）")]
    public float autoRecycleTime = 5f;   // 特效默认回收时间
    public float checkInterval = 1f;    // 每几秒检查一次超时

    private float _checkTimer;

    private void Awake()
    {
        // 单例初始化
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 从池子里获取一个特效
    /// </summary>
    public GameObject Get(GameObject prefab, Vector3 pos, Quaternion rot)
    {
        if (prefab == null) return null;
        string key = prefab.name;

        // 1. 池子里有 直接取
        if (_poolDict.ContainsKey(key) && _poolDict[key].Count > 0)
        {
            GameObject obj = _poolDict[key].Dequeue();
            obj.SetActive(true);
            obj.transform.SetPositionAndRotation(pos, rot);

            _usingObjects.Add(obj, Time.time);
            return obj;
        }

        // 2. 池子里没有 新建
        GameObject newObj = Instantiate(prefab, pos, rot);
        newObj.name = key;
        _usingObjects.Add(newObj, Time.time);
        return newObj;
    }

    /// <summary>
    /// 回收特效回池子
    /// </summary>
    public void Recycle(GameObject obj)
    {
        if (obj == null || !obj.activeSelf) return;

        string key = obj.name;

        // 从使用中移除
        if (_usingObjects.ContainsKey(obj))
            _usingObjects.Remove(obj);

        // 加入池子
        obj.SetActive(false);
        if (!_poolDict.ContainsKey(key))
            _poolDict[key] = new Queue<GameObject>();

        _poolDict[key].Enqueue(obj);
    }

    /// <summary>
    /// 预加载一批特效到池子里
    /// </summary>
    public void Preload(GameObject prefab, int count)
    {
        if (prefab == null || count <= 0) return;
        string key = prefab.name;

        if (!_poolDict.ContainsKey(key))
            _poolDict[key] = new Queue<GameObject>();

        for (int i = 0; i < count; i++)
        {
            GameObject obj = Instantiate(prefab);
            obj.name = key;
            obj.SetActive(false);
            _poolDict[key].Enqueue(obj);
        }
    }

    /// <summary>
    /// 清空整个对象池
    /// </summary>
    public void ClearAll()
    {
        foreach (var queue in _poolDict.Values)
        {
            while (queue.Count > 0)
                Destroy(queue.Dequeue());
        }

        foreach (var obj in _usingObjects.Keys)
            Destroy(obj);

        _poolDict.Clear();
        _usingObjects.Clear();
    }

    private void Update()
    {
        // 定时检查自动回收
        _checkTimer += Time.deltaTime;
        if (_checkTimer >= checkInterval)
        {
            AutoRecycleOverdueEffects();
            _checkTimer = 0;
        }
    }

    /// <summary>
    /// 自动回收超时未使用的特效
    /// </summary>
    private void AutoRecycleOverdueEffects()
    {
        List<GameObject> needRecycle = new List<GameObject>();

        foreach (var pair in _usingObjects)
        {
            if (pair.Key == null) continue;
            if (Time.time - pair.Value >= autoRecycleTime)
                needRecycle.Add(pair.Key);
        }

        foreach (var obj in needRecycle)
            Recycle(obj);
    }
}