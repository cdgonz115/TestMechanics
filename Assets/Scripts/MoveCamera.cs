using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveCamera : MonoBehaviour
{
    [SerializeField]
    private GameObject player;

    [SerializeField]
    private float _sensitivityX = 1f, _sensitivityY = 1f;


    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    void Update()
    {
        HandleLookX();
        HandleLookY();
    }

    private void HandleLookX()
    {
        float mouseX = Input.GetAxis("Mouse X");

        Vector3 rotation = player.transform.localEulerAngles;
        rotation.y += mouseX * _sensitivityX;
        player.transform.localEulerAngles = rotation;
    }

    private void HandleLookY()
    {
        float mouseY = Input.GetAxis("Mouse Y");

        Vector3 rotation = transform.localEulerAngles;
        rotation.x += -mouseY * _sensitivityY;
        rotation.x = (rotation.x > 180) ? rotation.x - 360 : rotation.x;
        rotation.x = Mathf.Clamp(rotation.x, -70, 60);
        transform.localEulerAngles = rotation;
    }
}
