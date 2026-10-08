using UnityEngine;
using UnityEngine.UI;

public class SafeTimePanel : BasePanel
{
    public Text text; // 拖拽你的文本

    public override void Init()
    {
        
    }

    // 更新显示秒数
    public void SetTime(int second)
    {
        text.text = $"安全时间剩余：{second} 秒";
    }
}