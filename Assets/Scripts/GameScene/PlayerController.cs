using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public Animator animator;//动画器
    public float rotateSpeed;//旋转速度
    public float atk;//攻击力
    public int money;//已有金钱
    private float screenNormalizedFactor;//屏幕尺寸归一化
    // Start is called before the first frame update
    void Start()
    {
        animator = this.GetComponent<Animator>();
        CursorController.HideMouse();
        screenNormalizedFactor = Screen.width / 1920f;
    }

    // Update is called once per frame
    void Update()
    {
        animator.SetFloat("Vertical", Input.GetAxis("Vertical"));
        animator.SetFloat("Horizontal", Input.GetAxis("Horizontal"));
        animator.SetBool("Fire", Input.GetMouseButton(0));
        animator.SetBool("Roll", Input.GetKey(KeyCode.LeftShift));
        this.transform.Rotate(Vector3.up, Input.GetAxis("Mouse X") * rotateSpeed * screenNormalizedFactor * Time.deltaTime);
    }
}
