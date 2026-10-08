using System;
using UnityEngine;

public class TaskNPC : Interactable
{
    TaskData task1;
    TaskData task2;
    private void Start()
    {
        task1 = new TaskData
        {
            taskID = 1,
            taskName = "爆破大师",
            desc = "使用手雷击杀5只僵尸",
            taskType = TaskType.KillMonsterWithGrenade,
            targetCount = 5,
            rewardGold = 200
        };
    }

    private void OnDialogFinish()
    {
        TaskMgr.Instance.AcceptTask(task1); // 接取任务
        gameObject.SetActive(false);
    }

    public override void OnInteract()
    {
        string[] sentences = new[]
        {
            "你好，勇士！",
            "我接到组织命令来此处清剿丧尸",
            "如果你能够帮我分担压力我将感激不尽",
            "试着用手雷去击杀一些敌人吧",
            "任务完成后我会给你一些必要的报酬"
        };

        DialogData dialogData = new DialogData
        {
            name = "士兵",
            sentences = sentences
        };
        DialogMgr.Instance.StartDialog(dialogData, OnDialogFinish);
    }

    public void OnEnable()
    {
        interactTip = "对话";
    }
}