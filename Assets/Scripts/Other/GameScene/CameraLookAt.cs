using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class CameraLookAt : MonoBehaviour
{
    public Transform target;//摄像机跟随的目标（角色身上的挂载点）
    public Vector3 cameraOffset;//摄像机相对于角色的偏移
    public float targetHeightOffet;
    public float moveSpeed;//摄像机跟随移动速度
    public float rotationSpeed;//摄像机旋转速度
    private Vector3 cameraPos; //摄像机位置
    private Quaternion cameraRotation;

    private void Start()
    {
        cameraOffset=new Vector3(0, 4.5f, -3);
        targetHeightOffet=2.9f;
        moveSpeed=30f;
        rotationSpeed=30f;
    }
    public void Update()
    {
        //找到角色身上的摄像机挂载点
        if(target == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            target = playerObj.transform.Find("LookAtPoint");
            return;
        }
        //摄像机位置
        cameraPos = target.position + target.forward * cameraOffset.z + target.up * cameraOffset.y + target.right * cameraOffset.x;
        this.transform.position = Vector3.Lerp(this.transform.position, cameraPos, moveSpeed * Time.deltaTime);
        //旋转
        cameraRotation=Quaternion.LookRotation(target.position + Vector3.up * targetHeightOffet - this.transform.position);
        this.transform.rotation = Quaternion.Slerp(this.transform.rotation, cameraRotation, rotationSpeed * Time.deltaTime);
    }
}
