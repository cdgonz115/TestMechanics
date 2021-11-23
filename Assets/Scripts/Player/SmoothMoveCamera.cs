using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmoothMoveCamera : MonoBehaviour
{
    public float mouseSensitvity = 100f;
    public float smoothTime;
    public Transform horizontalRotationHelper;
    public Transform verticalRotationHelper;
    public Transform camHolder;

    float pX;
    float verticalAngularVelocity;
    float horizontalAngularVelocity;

    public float xRotation = 0f;


    private void Start()
    {
        horizontalRotationHelper.localRotation = transform.localRotation;
        Cursor.lockState = CursorLockMode.Locked;
    }
    private void Update()
    {
        //deal with vertical rotation
        pX = xRotation;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitvity * Time.deltaTime;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90, 90f);
        VerticalRotation(mouseY);
        
        //deal with horizontal rotation
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitvity * Time.deltaTime;
        HorizontalRotation(mouseX);
    }
    public void HorizontalRotation(float mouseX)
    {
        horizontalRotationHelper.Rotate(Vector3.up * mouseX, Space.Self);
        float angle = Mathf.SmoothDampAngle(
            transform.localEulerAngles.y, horizontalRotationHelper.localEulerAngles.y, ref horizontalAngularVelocity, smoothTime);
        //print(angle + " H");
        transform.localRotation = Quaternion.Euler(0f, angle, 0f);
    }
    public void VerticalRotation(float mouseY)
    {
       // verticalRotationHelper.Rotate(Vector3.right * xRotation, Space.Self);
        //float angle = Mathf.SmoothDampAngle(transform.localEulerAngles.x, verticalRotationHelper.localEulerAngles.x, ref verticalAngularVelocity, smoothTime);
        //xRotation = Mathf.SmoothDampAngle(pX, xRotation, ref verticalAngularVelocity, smoothTime);
        //print(xRotation + " Y");
        camHolder.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }

}
