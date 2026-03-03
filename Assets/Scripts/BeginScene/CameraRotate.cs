using System.Collections;
using UnityEngine;

public class CameraRotate : MonoBehaviour
{
    [Header("旋转速度控制")]
    public float rotateDuration = 1f; // 旋转90度总时长
    private bool _isRotating; // 防止重复旋转

    /// <summary>
    /// 逆时针旋转90度
    /// </summary>
    public void Rotate_Left()
    {
        if (_isRotating) return;
        StartCoroutine(RotateByAngle(90f)); // 逆时针
    }

    /// <summary>
    /// 顺时针旋转90度
    /// </summary>
    public void Rotate_Right()
    {
        if (_isRotating) return;
        StartCoroutine(RotateByAngle(-90f)); // 顺时针
    }

    /// <summary>
    /// 核心旋转协程
    /// </summary>
    /// <param name="angleDelta">Y轴旋转增量</param>
    private IEnumerator RotateByAngle(float angleDelta)
    {
        _isRotating = true;
        float elapsedTime = 0f;
        Quaternion startRot = transform.rotation;
        Quaternion targetRot = startRot * Quaternion.Euler(0, angleDelta, 0);

        // 平滑插值旋转
        while (elapsedTime < rotateDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = Mathf.SmoothStep(0, 1, elapsedTime / rotateDuration);
            transform.rotation = Quaternion.Lerp(startRot, targetRot, progress);
            yield return null;
        }

        // 强制对齐目标角度，避免浮点误差
        transform.rotation = targetRot;
        _isRotating = false;
    }
}