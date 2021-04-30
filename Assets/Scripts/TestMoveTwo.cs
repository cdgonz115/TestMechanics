using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestMoveTwo : MonoBehaviour
{
    float z;
    float x;

    float g;

    public bool sprinting;

    public float walkSpeedIncrease;
    public float sprintSpeedIncrease;
    public float speedIncrease;

    public float maxWalkVelocity;
    public float maxSprintVelocity;
    public float maxVelocity;
    public float minVelocity;

    public float friction;

    public Vector3 actualForward;
    public Vector3 actualRight;

    public Vector3 totalVelocity;

    public CapsuleCollider capCollider;
    public Rigidbody rb;


    public bool groundCheck;
    public bool isGrounded;

    public float initialGravity;
    
    public float groundCheckDistance;

    public float startingAirStrafe;
    public float airStrafeDecreaser;
    public float airStrafe;


    private void Start()
    {
        groundCheckDistance = capCollider.height * .5f - capCollider.radius;
    }

    private void Update()
    {
        sprinting = Input.GetKey(KeyCode.LeftShift);
        speedIncrease = sprinting ? sprintSpeedIncrease : walkSpeedIncrease;
        maxVelocity = sprinting ? maxSprintVelocity : maxWalkVelocity;
        if (Input.GetKey(KeyCode.W)) z = speedIncrease;
        else if (Input.GetKey(KeyCode.S)) z = -speedIncrease;
        if (Input.GetKey(KeyCode.D)) x = speedIncrease;
        else if (Input.GetKey(KeyCode.A)) x = -speedIncrease;
    }

    private void FixedUpdate()
    {
        Move();
        if (rb.velocity.magnitude > minVelocity) rb.velocity += totalVelocity;
        else rb.velocity = Vector3.zero;

        Debug.DrawLine(transform.position, transform.position + actualForward.normalized * 5, Color.red);
        Debug.DrawLine(transform.position, transform.position + actualRight.normalized * 5, Color.red);

    }

    private void Move()
    {
        if (isGrounded && !groundCheck)
        {
            g = initialGravity;
            //airStrafe = startingAirStrafe;

        }
        if (groundCheck)
        {
            g = 0;
            //rb.velocity -= transform.up.normalized * rb.velocity.y;
        }
        isGrounded = groundCheck;

        Vector3 newDirection;
        totalVelocity = Vector3.zero;

        RaycastHit hit;
        groundCheck = Physics.SphereCast(transform.position, capCollider.radius + 0.01f, -transform.up, out hit, groundCheckDistance);

        actualForward = Vector3.Cross(hit.normal, -transform.right);
        actualRight = Vector3.Cross(hit.normal, transform.forward);

        if (!isGrounded)
        {
            x *= airStrafe;
            z *= airStrafe;
            totalVelocity += ((transform.right.normalized * x + transform.forward.normalized * z));
        }
        else
        {
            if (rb.velocity.magnitude < maxVelocity)
            {
                newDirection = (actualRight.normalized * x + actualForward.normalized * z);

                totalVelocity += newDirection;

                z = 0;
                x = 0;
            }
            totalVelocity -= rb.velocity * friction;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position - Vector3.up * (groundCheckDistance), capCollider.radius + 0.01f);
    }
}
