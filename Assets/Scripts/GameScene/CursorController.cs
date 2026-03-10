using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorController : MonoBehaviour
{
    // 隐藏鼠标
    public static void HideMouse()
    {
        Cursor.visible = false; // 隐藏鼠标视觉显示
        Cursor.lockState = CursorLockMode.Locked; // 锁定鼠标到屏幕中心
    }

    // 显示鼠标
    public static void ShowMouse()
    {
        Cursor.visible = true; // 显示鼠标
        Cursor.lockState = CursorLockMode.None; // 解锁鼠标
    }
}
