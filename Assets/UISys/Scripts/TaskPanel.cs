using UnityEngine;
using UnityEngine.UI;

public class TaskPanel : BasePanel
{
    public Text txtName;
    public Text txtDesc;
    private string strProgress;

    public override void Init()
    {

    }

    // À¢–¬œ‘ æ
    public void Refresh()
    {
        var task = TaskMgr.Instance.currentTask;
        txtName.text = task.taskName;
        strProgress = $"({TaskMgr.Instance.currentProgress}/{task.targetCount})";
        txtDesc.text = task.desc + strProgress;
    }
}