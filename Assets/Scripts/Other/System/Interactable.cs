using UnityEngine;

// 所有可交互物体：NPC、宝箱、防御塔、门 都继承这个
public class Interactable : MonoBehaviour
{
    [Header("基础设置")]
    public string interactTip = "按 [F] 交互"; // 提示文字
    public bool canInteract = true;

    // 交互行为：子类去重写这个
    public virtual void OnInteract()
    {
        //Debug.Log("交互了：" + gameObject.name);
    }
}