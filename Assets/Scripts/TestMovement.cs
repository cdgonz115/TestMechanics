using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestMovement : MonoBehaviour
{
    Rigidbody rb;
    float x, y, z;
    public GameObject camera;
    public float walkSpeed;
    public float runSpeed;
    public float speed;
    public float distToGround;
    public float jumpStrenght;
    public float gravityRate;
    public bool isGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        distToGround = GetComponent<CapsuleCollider>().bounds.center.y;
    }

    // Update is called once per frame
    void Update()
    {
        //Move();
        Jump();

        print(rb.velocity);
    }
    void Move()
    {
        isGrounded = (Physics.Raycast(transform.position, -Vector3.up, distToGround))? true: false;
        speed = (Input.GetKey(KeyCode.LeftShift))? runSpeed : walkSpeed;
        if (!isGrounded)
        {
            z = Input.GetAxis("Horizontal") * speed;
            x = Input.GetAxis("Vertical") * speed;
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
        rb.velocity = transform.forward + new Vector3(x, 0, z);
    }
    void Jump()
    {
        if (Input.GetKeyDown(KeyCode.Space)) rb.AddForce(new Vector3(0, jumpStrenght, z), ForceMode.Impulse);
    }
    void ApplyGravity()
    { 
        //if(isGrounded)
    }
}
