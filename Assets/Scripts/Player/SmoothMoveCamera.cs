using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmoothMoveCamera : MonoBehaviour
{
    public float mouseSensitvity = 100f;
    public float smoothTime;
    public Transform horizontalRotationHelper;
    public Transform camHolder;

    float pX;
    float horizontalAngularVelocity;
    float verticalAngularVelocity;

    public float xRotation = 0f;


    private void Start()
    {
        horizontalRotationHelper.localRotation = transform.localRotation;
        Cursor.lockState = CursorLockMode.Locked;
    }
    public void Rotate()
    {
        //deal with vertical rotation
        pX = xRotation;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitvity * Time.deltaTime;
        xRotation -= mouseY *8;
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
        transform.localRotation = Quaternion.Euler(0f, angle, 0f);
    }
    public void VerticalRotation(float mouseY)
    {
        xRotation = Mathf.SmoothDampAngle(pX, xRotation, ref verticalAngularVelocity, smoothTime);
        camHolder.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }
    public void AdjustCameraHeight(bool moveDown)
    {
        if (moveDown) camHolder.position -= Vector3.up;
        else camHolder.position += Vector3.up;
    }
}
