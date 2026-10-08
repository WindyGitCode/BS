using System;
using UnityEngine;

public class TaskMgr : Singleton<TaskMgr>
{
    [Header("当前接取的任务")]
    public TaskData currentTask;

    [Header("当前进度")]
    public int currentProgress;

    public bool hasTask => currentTask != null && !currentTask.isCompleted;
    private bool _hasCheckedGameEnd; // 防止重复检测
    public void Start()
    {
        // 监听事件
        EventMgr.Instance.AddListener(EventConst.GrenadeKillMonster, OnGrenadeKillMonster);
    }
    public void Update()
    {
        // 每帧检测关卡是否结束，如果结束且有任务未完成，则失败任务
        if (!_hasCheckedGameEnd
            && SurvivalLevelMgr.Instance.currentState == SurvivalState.Ended
            && hasTask)
        {
            _hasCheckedGameEnd = true;
            FailTask();
        }
    }
    private void OnGrenadeKillMonster()
    {
        if (hasTask && currentTask.taskType == TaskType.KillMonsterWithGrenade)
        {
            AddProgress(TaskType.KillMonsterWithGrenade);
        }
    }

    // ==========================================
    // 外部接口：接取任务
    // ==========================================
    public void AcceptTask(TaskData task)
    {
        currentTask = task;
        currentProgress = 0;
        currentTask.isCompleted = false;
        _hasCheckedGameEnd = false; // 重置检测
        // 打开任务面板
        UIMgr.Instance.ShowPanel<TaskPanel>();
        UpdateTaskUI();
    }

    // ==========================================
    // 外部调用：更新任务进度
    // ==========================================
    public void AddProgress(TaskType type, int add = 1)
    {
        if (!hasTask) return;
        if (currentTask.taskType != type) return;

        currentProgress += add;
        currentProgress = Mathf.Min(currentProgress, currentTask.targetCount);

        UpdateTaskUI();
        CheckComplete();
    }

    // ==========================================
    // 检查是否完成
    // ==========================================
    void CheckComplete()
    {
        if (currentProgress >= currentTask.targetCount)
        {
            CompleteTask();
        }
    }

    // ==========================================
    // 完成任务 + 发放奖励
    // ==========================================
    void CompleteTask()
    {
        currentTask.isCompleted = true;

        //// 发放奖励
        //PlayerDataMgr.Instance.AddGold(currentTask.rewardGold);

        // 提示
        TempTipPanel tip = UIMgr.Instance.ShowPanel<TempTipPanel>();
        tip.ShowTipAutoHide($"任务完成！获得{currentTask.rewardGold}金币", 2.5f);
        UpdateTaskUI();
        UIMgr.Instance.HidePanel<TaskPanel>();
    }
    // ==========================================
    // 任务失败
    // ==========================================
    void FailTask()
    {
        if (!hasTask) return;

        currentTask.isCompleted = true;

        // 关闭任务面板
        UIMgr.Instance.HidePanel<TaskPanel>();
    }
    // ==========================================
    // 更新UI
    // ==========================================
    void UpdateTaskUI()
    {
        UIMgr.Instance.ShowPanel<TaskPanel>()?.Refresh();
    }
    private void OnDestroy()
    {
        // 取消监听
        EventMgr.Instance.RemoveListener(EventConst.GrenadeKillMonster, OnGrenadeKillMonster);
    }
}