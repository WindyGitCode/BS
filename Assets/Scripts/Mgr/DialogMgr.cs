using UnityEngine;
using UnityEngine.UI;
using System;

public class DialogMgr : MonoBehaviour
{
    public static DialogMgr Instance;
    // 路径自己对应你的预制体位置
    private const string DialogPanelPath = "DialogPanel";

    private GameObject dialogPanel;
    private Text dialogText;
    private Text dialogNameText;

    private DialogData curDialogData;
    private int curSentenceIndex;
    public bool IsDialoging { get; private set; }
    public KeyCode nextDiaKey;
    // 结束事件
    private Action onDialogFinished;

    protected void Awake()
    {
        // 单例安全赋值
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
        // 初始化加载对话面板UI
        InitDialogUI();
        nextDiaKey=GameDataMgr.Instance.GetGameData().dialogKey;
    }

    /// <summary>
    /// 初始化
    /// </summary>
    void InitDialogUI()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("场景无Canvas，无法加载对话面板！");
            return;
        }

        GameObject panelPrefab = Resources.Load<GameObject>(DialogPanelPath);
        if (panelPrefab == null)
        {
            Debug.LogError($"Resources 路径不存在：{DialogPanelPath}");
            return;
        }

        // 实例化到Canvas下
        dialogPanel = Instantiate(panelPrefab, canvas.transform);
        dialogPanel.SetActive(false);
        // 确保在最前面显示
        dialogPanel.transform.SetAsLastSibling();
        // 自动获取子物体Text
        Transform textTrans = dialogPanel.transform.Find("DialogText");
        if (textTrans != null) dialogText = textTrans.GetComponent<Text>();
        else Debug.LogError("找不到 DialogText！");

        Transform nameTrans = dialogPanel.transform.Find("DialogNameText");
        if (nameTrans != null) dialogNameText = nameTrans.GetComponent<Text>();
        else Debug.LogError("找不到 DialogNameText！");
    }

    /// <summary>
    /// 外部统一调用的开启对话接口
    /// </summary>
    public void StartDialog(DialogData data, Action onFinished = null)
    {
        if (IsDialoging || data == null || data.sentences == null || data.sentences.Length == 0)
            return;

        curDialogData = data;
        curSentenceIndex = 0;
        IsDialoging = true;
        this.onDialogFinished = onFinished; // 绑定当前NPC的结束事件

        dialogPanel.SetActive(true);
        ShowCurrentSentence();
    }

    void ShowCurrentSentence()
    {
        dialogText.text = curDialogData.sentences[curSentenceIndex];
        dialogNameText.text = curDialogData.name;
    }

    private void Update()
    {
        if (!IsDialoging) return;

        // 空格 下一句
        if (Input.GetKeyDown(nextDiaKey))
        {
            NextSentence();
        }
    }

    void NextSentence()
    {
        curSentenceIndex++;

        // 全部读完 -> 结束对话
        if (curSentenceIndex >= curDialogData.sentences.Length)
        {
            CloseDialog();
            return;
        }

        ShowCurrentSentence();
    }

    void CloseDialog()
    {
        IsDialoging = false;
        dialogPanel.SetActive(false);
        // 触发当前NPC的结束事件
        onDialogFinished?.Invoke();
    }
}
[System.Serializable]
public class DialogData
{
    public string name;
    public string[] sentences;
}