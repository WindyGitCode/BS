using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main : MonoBehaviour
{
    public void Awake()
    {
        UIMgr.Instance.ShowPanel<BeginPanel>();
    }
    private void Update()
    {

    }
}
