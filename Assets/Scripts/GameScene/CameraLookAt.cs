using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class CameraLookAt : MonoBehaviour
{
    public Transform target;
    public Vector3 cameraOffset;
    public float targetHeightOffet;
    public float moveSpeed;
    public float rotationSpeed;
    private Vector3 cameraPos;
    private Quaternion cameraRotation;

    public void Update()
    {
        if(target == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            target = playerObj.transform.Find("LookAtPoint");
            return;
        }
        cameraPos=target.position + target.forward * cameraOffset.z + target.up * cameraOffset.y + target.right * cameraOffset.x;
        this.transform.position = Vector3.Lerp(this.transform.position, cameraPos, moveSpeed * Time.deltaTime);
        //Ðý×ª
        cameraRotation=Quaternion.LookRotation(target.position + Vector3.up * targetHeightOffet - this.transform.position);
        this.transform.rotation = Quaternion.Slerp(this.transform.rotation, cameraRotation, rotationSpeed * Time.deltaTime);
    }
}
