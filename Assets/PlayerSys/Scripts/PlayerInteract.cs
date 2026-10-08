using UnityEngine;
using UnityEngine.UI;

public class PlayerInteract : MonoBehaviour
{
    [Header("交互设置")]
    public float interactRange = 2f;
    public KeyCode interactKey;

    private GameObject interactPanel;
    private Text tipText;
    private Interactable currentTarget;

    private Canvas _canvas;

    private void Awake()
    {
        // 全局找画布
        _canvas = FindObjectOfType<Canvas>();
        interactKey=GameDataMgr.Instance.GetGameData().interactKey;
        LoadInteractUI();
    }

    private void Update()
    {
        if(_canvas == null)
        {
            _canvas = FindObjectOfType<Canvas>();
            if(_canvas != null)
                LoadInteractUI();
        }

        CheckInteractable();
        UpdateUI();

        if (currentTarget != null && Input.GetKeyDown(interactKey))
        {
            AudioMgr.Instance.PlayBtnClick();
            currentTarget.OnInteract();
        }
    }

    void LoadInteractUI()
    {
        if (_canvas == null)
        {
            Debug.LogError("场景中没有 Canvas，无法生成交互UI！");
            return;
        }

        // 加载UI预制体
        GameObject panelPrefab = Resources.Load<GameObject>("InteractPanel");
        if (panelPrefab == null)
        {
            Debug.LogError("Resources/InteractPanel 未找到");
            return;
        }

        // 实例化到 Canvas 下面
        interactPanel = Instantiate(panelPrefab, _canvas.transform);
        interactPanel.SetActive(false);
        //放置到最前
        interactPanel.transform.SetAsFirstSibling();

        // 拿文字组件
        tipText = interactPanel.GetComponentInChildren<Text>();
    }

    void CheckInteractable()
    {
        currentTarget = null;
        Collider[] cols = Physics.OverlapSphere(transform.position, interactRange);
        foreach (var col in cols)
        {
            Interactable t = col.GetComponent<Interactable>();
            if (t != null && t.canInteract)
            {
                currentTarget = t;
                break;
            }
        }
    }

    void UpdateUI()
    {
        if (interactPanel == null) return;

        if (currentTarget != null)
        {
            interactPanel.SetActive(true);
            tipText.text = currentTarget.interactTip;
        }
        else
        {
            interactPanel.SetActive(false);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactRange);
    }

    private void OnDestroy()
    {
        if (interactPanel != null)
            Destroy(interactPanel);
    }
}