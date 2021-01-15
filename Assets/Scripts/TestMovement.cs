﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestMovement : MonoBehaviour
{
    Rigidbody rb;
    float x, y, z, g;

    [Header("Speed Variables")]
    public float friction;
    public float airDecrease;
    public float speedIncrease;
    public float walkSpeed;
    public float runSpeed;
    public float speed;


    [Header("Jump Variables")]
    public float jumpStrenght;
    public float jumpDecreaseRate;
    public float jumpBuffer;
    public float jumpBufferValue;
    public bool jumpBuffering;
    public bool isJumping;


    [Header("Grounded Variables")]
    public Transform sphereCheck;
    public float groundDistance;
    public float initialGravity;
    public float gravityRate;
    public bool isGrounded;
    public bool groundCheck;
    public LayerMask mask;

    public Vector3 velocity;
    public Vector3 total;


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
        rb.velocity += total;
        Debug.DrawLine(transform.position,transform.position + rb.velocity, Color.red);
    }
    void Move()
    {
        z = 0;
        x = 0;
        total = Vector3.zero;
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
        velocity = new Vector3(rb.velocity.x,0,rb.velocity.z);
        if (!isGrounded)
        {
            if (Input.GetKey(KeyCode.W)) z = speedIncrease * airDecrease;
            else if (Input.GetKey(KeyCode.S)) z = -speedIncrease * airDecrease;
            if (Input.GetKey(KeyCode.D)) x = speedIncrease * airDecrease;
            else if (Input.GetKey(KeyCode.A)) x = -speedIncrease * airDecrease;
            total += (transform.right.normalized * x + transform.forward.normalized * z);
            if (g>-39.2f)g /= gravityRate;
        }
        else 
        {
            if (velocity.magnitude < speed)
            {
                if (Input.GetKey(KeyCode.W)) z = speedIncrease;
                else if (Input.GetKey(KeyCode.S)) z = -speedIncrease;
                if (Input.GetKey(KeyCode.D)) x = speedIncrease;
                else if (Input.GetKey(KeyCode.A)) x = -speedIncrease;
                total += (transform.right.normalized * x + transform.forward.normalized * z);
            }
            if (total.magnitude == 0) total -= velocity * friction;
        }
        total += (transform.up.normalized * g);
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
            if (y > 1f) y /= jumpDecreaseRate;
            else
            {
                y = 0;
                isJumping = false;
            }
        }
        else y = 0;
        total += (transform.up.normalized * y);
        //print(y);
        //
    }
    void ApplyGravity()
    { 
        //if(isGrounded)
    }
}
