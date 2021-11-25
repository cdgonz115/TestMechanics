using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    public float mouseSensitvity = 100f;
    public float camHeight = .75f;
    public Transform player;

    float xRotation = 0f;
    float yRotation;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        transform.localRotation = player.transform.rotation;
        yRotation = transform.localEulerAngles.y;
    }

    private void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitvity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitvity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90, 90f);
        yRotation += mouseX;
        yRotation %= 360;

        transform.position = player.position + new Vector3(0, camHeight, 0);
        transform.localRotation = Quaternion.Euler(xRotation, yRotation, 0f);
    }

    public void AdjustCameraHeight(bool moveDown)
    {
        if (moveDown) camHeight -= 1;
        else camHeight += 1;
    }
}
