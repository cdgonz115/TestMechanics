using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestMovement : MonoBehaviour
{
    Rigidbody rb;
    float x, y, z, g;

    public float walkSpeed;
    public float runSpeed;
    public float speed;

    public float jumpStrenght;
    public float jumpDecreaseRte;
    public float jumpBuffer;
    public float jumpBufferValue;
    public bool jumpBuffering;
    public bool isJumping;


    public Transform sphereCheck;
    public float groundDistance;
    public float initialGravity;
    public float gravityRate;
    public bool isGrounded;
    public bool groundCheck;
    public LayerMask mask;

    public Vector3 velocity;


    [Header("Camera Stuff")]
    public float mouseSensitvity = 100f;
    public Transform player;
    public Transform camera;
    float xRotation = 0f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        rb = GetComponent<Rigidbody>();
        //groundDistance = GetComponent<CapsuleCollider>().height / 2f;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            jumpBuffering = true;
            jumpBuffer = jumpBufferValue;
        }
        if (jumpBuffering) jumpBuffer -= Time.deltaTime;
    }
    void FixedUpdate()
    {
        Move();
        Jump();
        //print(rb.velocity);
    }
    void Move()
    {
        //groundCheck = (Physics.Raycast(transform.position, -Vector3.up, groundDistance * 1.05f));
        //Physics.sphere
        groundCheck = Physics.CheckSphere(sphereCheck.position, groundDistance, mask);
        if (isGrounded && !groundCheck)
        {
            g = initialGravity;
        }
        if (groundCheck) g = 0;
        isGrounded = groundCheck;
        speed = (Input.GetKey(KeyCode.LeftShift))? runSpeed : walkSpeed;
        if (!isGrounded)
        {
            x = Input.GetAxis("Horizontal") * speed;
            z = Input.GetAxis("Vertical") * speed;
            if(g>-39.2f)g /= gravityRate;
        }
        else 
        {
            if (Input.GetKey(KeyCode.W)) z = speed;
            else if (Input.GetKey(KeyCode.S)) z = -speed;
            else z = 0;
            if (Input.GetKey(KeyCode.D)) x = speed;
            else if (Input.GetKey(KeyCode.A)) x = -speed;
            else x = 0;
        }
        rb.velocity = (transform.right * x + transform.forward * z);
        rb.velocity += (transform.up * g);
    }
    void CameraMovement()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitvity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitvity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90, 90f);

        camera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        player.Rotate(Vector2.up * mouseX);
    }
    void Jump()
    {
        if (isJumping && isGrounded) isJumping = false;
        if (isGrounded && jumpBuffer>0)
        {
            isJumping = true; //rb.AddForce(new Vector3(0, jumpStrenght, 0), ForceMode.Impulse);
            y = jumpStrenght;
        }
        if (isJumping)
        {
            if (y > 1f) y /= jumpDecreaseRte;
            else
            {
                y = 0;
                isJumping = false;
            }
        }
        else y = 0;
        rb.velocity += (transform.up * y);
        //print(y);
        //
    }
    void ApplyGravity()
    { 
        //if(isGrounded)
    }
}
