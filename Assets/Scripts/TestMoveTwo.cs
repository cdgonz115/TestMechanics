using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestMoveTwo : MonoBehaviour
{
    float z;
    float x;

    public bool sprinting;

    public float walkSpeedIncrease;
    public float sprintSpeedIncrease;
    public float speedIncrease;

    public float maxWalkVelocity;
    public float maxSprintVelocity;
    public float maxVelocity;

    public float friction;

    public Vector3 actualForward;
    public Vector3 actualRight;

    public Vector3 totalVelocity;

    public CapsuleCollider capCollider;
    public Rigidbody rb;


    public bool groundCheck;
    
    public float groundDistance;


    private void Start()
    {
        groundDistance = capCollider.height * .5f - capCollider.radius;
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
        rb.velocity += totalVelocity;

        Debug.DrawLine(transform.position, transform.position + actualForward.normalized * 5, Color.red);
        Debug.DrawLine(transform.position, transform.position + actualRight.normalized * 5, Color.red);

    }

    private void Move()
    {
        Vector3 newDirection;
        totalVelocity = Vector3.zero;

        RaycastHit hit;
        groundCheck = Physics.SphereCast(transform.position, capCollider.radius + 0.01f, -transform.up, out hit, groundDistance);

        actualForward = Vector3.Cross(hit.normal, -transform.right);
        actualRight = Vector3.Cross(hit.normal, transform.forward);


        if (rb.velocity.magnitude < maxVelocity)
        {
            newDirection = (actualRight.normalized * x + actualForward.normalized * z);

            totalVelocity += newDirection;

            z = 0;
            x = 0;
        }

        totalVelocity -= rb.velocity * friction;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position - Vector3.up * (groundDistance), capCollider.radius + 0.01f);
    }
}
