using OpenCover.Framework.Model;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class BasePanel : MonoBehaviour 
{
    public CanvasGroup canvasGroup;
    public UnityAction hideMeCallFunc=null;
    protected float alphaSpeed=10;
    public bool isShow=false;
    public virtual void Awake()
    {
        canvasGroup = this.GetComponent<CanvasGroup>();
        if (canvasGroup == null )
            canvasGroup = this.gameObject.AddComponent<CanvasGroup>();
    }
    public virtual void Start()
    {
        Init();
    }
    public abstract void Init();
    public void Update()
    {
        if (isShow == true && canvasGroup.alpha !=1)
        {
            canvasGroup.alpha += alphaSpeed*Time.deltaTime;
            if (canvasGroup.alpha >= 1)
            {
                canvasGroup.alpha = 1;
            }
        }
        else if(isShow == false&&canvasGroup.alpha!=0)
        {
            canvasGroup.alpha -= alphaSpeed * Time.deltaTime;
            if (canvasGroup.alpha <= 0)
            {
                canvasGroup.alpha = 0;
                hideMeCallFunc?.Invoke();
            }
        }
    }
    public virtual void ShowMe()
    {
        isShow = true;
        canvasGroup.alpha = 0;
    }
    public virtual void HideMe(UnityAction unityAction)
    {
        isShow=false;
        canvasGroup.alpha = 1;
        hideMeCallFunc = unityAction; 
    }

}
