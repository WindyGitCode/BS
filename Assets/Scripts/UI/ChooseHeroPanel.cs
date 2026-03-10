using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ChooseHeroPanel : BasePanel
{
    public Camera cam;
    public Button btnBegin;
    public Button btnBack;
    public override void Init()
    {
        cam = GameObject.Find("camera").GetComponent<Camera>();
        cam.gameObject.GetComponent<CameraRotate>().Rotate_Right();
        btnBegin.onClick.AddListener(() =>
        {
            SceneManager.LoadScene("gameScene");
        });
        btnBack.onClick.AddListener(() =>
        {
            UIMgr.Instance.HidePanel<ChooseHeroPanel>();
            UIMgr.Instance.ShowPanel<BeginPanel>();
        });
    }
    private void OnDestroy()
    {
        cam.gameObject.GetComponent<CameraRotate>().Rotate_Left();
    }
}
