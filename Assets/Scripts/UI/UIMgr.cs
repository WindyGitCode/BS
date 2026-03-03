using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class UIMgr
{
    private static UIMgr instance = new UIMgr();
    public static UIMgr Instance => instance;
    public Transform canvasTrans;
    private Dictionary<string, BasePanel> panelDic=new Dictionary<string, BasePanel>();
    private UIMgr()
    {
        GameObject canvas = Resources.Load<GameObject>("UI/canvas");
        GameObject Icanvas= GameObject.Instantiate(canvas);
        GameObject.DontDestroyOnLoad(Icanvas);
        canvasTrans = Icanvas.transform;
    }
    public T ShowPanel<T>() where T : BasePanel
    {
        //显示面板
        string panelName=typeof(T).Name;
        if(panelDic.ContainsKey(panelName))
            return panelDic[panelName]as T;//字典已有面板 直接返回
        //字典没有面板 添加面板
        GameObject panelObj = Resources.Load<GameObject>("UI/"+panelName);
        GameObject Ipanel= GameObject.Instantiate(panelObj);
        Ipanel.transform.SetParent(canvasTrans, false);
        T panel=Ipanel.GetComponent<T>();
        //if(!panelDic.ContainsKey(panelName))
        panelDic.Add(panelName, panel.GetComponent<T>());
        panel.ShowMe();
        return panel;
    }
    public void HidePanel<T>(bool isFade=true) where T : BasePanel
    {
        string panelName= typeof(T).Name;
        if (panelDic.ContainsKey(panelName))
        {
            if (isFade)
            {
                panelDic[panelName].HideMe(() =>
                {
                    GameObject panelObj = panelDic[panelName].gameObject;
                    GameObject.Destroy(panelObj);
                    panelDic.Remove(panelName);
                });
            }
            else
            {
                GameObject panelObj = panelDic[panelName].gameObject;
                GameObject.Destroy(panelObj);
                panelDic.Remove(panelName);
            }
        }
    }
    public T GetPanel<T>() where T : BasePanel
    {
        string panelName=typeof(T).Name;
        if(panelDic.ContainsKey(panelName))
        return panelDic[panelName]as T;
        return null;
    }
}
