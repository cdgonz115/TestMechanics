using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestMoveTwo : MonoBehaviour
{
    float z;
    float x;

    public float g;

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
    public Vector3 newForwardandRight;
    public CapsuleCollider capCollider;
    public Rigidbody rb;


    public bool groundCheck;
    public bool isGrounded;

    public float initialGravity;
    public float gravityRate;
    public float maxGravity;

    public float groundCheckDistance;

    public float initialAirStrafe;
    public float airStrafeDecreaser;
    public float airStrafe;

    public float y;
    public float jumpBuffer;
    public float jumpBufferValue;
    public bool jumpBuffering;
    public bool isJumping;
    public float jumpStrength;
    public float justJumpedCooldown;
    private float _justJumpedCooldown;

    float scrollwheel;

    RaycastHit hit;


    private void Start()
    {
        groundCheckDistance = capCollider.height * .5f - capCollider.radius;
        g = initialGravity;
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

        scrollwheel = Input.GetAxis("Mouse ScrollWheel");
        if (Input.GetKeyDown(KeyCode.Space) || scrollwheel > 0)
        {
            jumpBuffering = true;
            jumpBuffer = jumpBufferValue;
        }
    }

    private void FixedUpdate()
    {
        GroundCheck();
        Move();
        Jump();
        ApplyGravity();
        rb.velocity += totalVelocity;
        if (rb.velocity.magnitude < .1f && x == 0 && z == 0 && (isGrounded)) rb.velocity = Vector3.zero;

        Debug.DrawLine(transform.position, transform.position + actualForward.normalized * 5, Color.red);
        Debug.DrawLine(transform.position, transform.position + actualRight.normalized * 5, Color.red);

    }

    private void GroundCheck()
    {
        if(_justJumpedCooldown>0)_justJumpedCooldown -= Time.fixedDeltaTime;
        groundCheck = (_justJumpedCooldown <=0)? Physics.SphereCast(transform.position, capCollider.radius + 0.01f, -transform.up, out hit, groundCheckDistance) : false;

        if (isGrounded && !groundCheck)
        {
            g = initialGravity;
            airStrafe = initialAirStrafe;
        }
        if (groundCheck)
        {
            g = 0;
        }
        isGrounded = groundCheck;
    }

    private void Move()
    {
        totalVelocity = Vector3.zero;
        newForwardandRight = Vector3.zero;

        actualForward = Vector3.Cross(hit.normal, -transform.right);
        actualRight = Vector3.Cross(hit.normal, transform.forward);

        if (!isGrounded)
        {
            //airStrafe -= airStrafeDecreaser;
            x *= airStrafe;
            z *= airStrafe;
            totalVelocity += ((transform.right.normalized * x + transform.forward.normalized * z));
        }
        else
        {
            if (rb.velocity.magnitude < maxVelocity)
            {
                newForwardandRight = (actualRight.normalized * x + actualForward.normalized * z);

                totalVelocity += newForwardandRight;

                z = 0;
                x = 0;
            }
            totalVelocity -= rb.velocity * friction;
        }
    }

    private void ApplyGravity()
    {
        if (!isGrounded)
        {
            totalVelocity += Vector3.up * g;
        }
        if (g > maxGravity) g *= gravityRate;
    }
    public void ResetJumpBuffer()
    {
        jumpBuffering = false;
        jumpBuffer = 0;
    }
    void Jump()
    {
        if (isGrounded)isJumping = false;
        if (jumpBuffer < 0)ResetJumpBuffer();
        if (jumpBuffer > 0 && isGrounded && !isJumping)StartCoroutine(JumpCoroutine());
        if (jumpBuffering) jumpBuffer -= Time.fixedDeltaTime;
    }
    IEnumerator JumpCoroutine()
    {
        WaitForFixedUpdate fixedUpdate = new WaitForFixedUpdate();
        ResetJumpBuffer();
        isJumping = true;
        isGrounded = false;
        y = jumpStrength;
        g = initialGravity;
        _justJumpedCooldown = justJumpedCooldown;
        while (y > 0.1f && !isGrounded)
        {
            y -= .05f;
            totalVelocity += Vector3.up * y;
            yield return fixedUpdate;
        }
    }
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position - Vector3.up * (groundCheckDistance), capCollider.radius + 0.01f);
    }
}
