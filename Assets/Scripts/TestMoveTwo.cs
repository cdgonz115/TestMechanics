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
    public float groundFriction;
    public float airFriction;

    public Vector3 actualForward;
    public Vector3 actualRight;

    public Vector3 totalVelocity;
    public Vector3 newForwardandRight;
    public Vector3 currentForwardAndRight;

    public CapsuleCollider capCollider;
    public Rigidbody rb;
    public MoveCamera moveCamera;


    public bool groundCheck;
    public bool isGrounded;

    public float initialGravity;
    public float gravityRate;
    public float maxGravity;

    public float groundCheckDistance;

    public float initialAirStrafe;
    public float airStrafeDecreaser;
    public float airStrafe;
    public float maxAirVelocity;

    public float y;
    public float jumpBuffer;
    public float jumpBufferValue;
    public bool jumpBuffering;
    public bool isJumping;
    public float jumpStrength;
    public float justJumpedCooldown;
    private float _justJumpedCooldown;

    float scrollwheel;

    public bool isCrouching;
    public bool crouchBuffer;


    public bool isSliding;
    public float slidingFriction;
    public float velocityToSlide;
    public float slideForce;


    RaycastHit hit;
    WaitForFixedUpdate fixedUpdate;

    private void Start()
    {
        groundCheckDistance = capCollider.height * .5f - capCollider.radius;
        g = initialGravity;
        fixedUpdate = new WaitForFixedUpdate();
        friction = groundFriction;
    }

    private void Update()
    {

        crouchBuffer = Input.GetKey(KeyCode.LeftControl);

        sprinting = (isCrouching)? false: Input.GetKey(KeyCode.LeftShift);
        speedIncrease = sprinting ? sprintSpeedIncrease : walkSpeedIncrease;
        maxVelocity = sprinting ? maxSprintVelocity : maxWalkVelocity;
        if (Input.GetKey(KeyCode.W)) z = speedIncrease;
        else if (Input.GetKey(KeyCode.S)) z = -speedIncrease;
        else if (isGrounded) z = 0;
        if (Input.GetKey(KeyCode.D)) x = speedIncrease;
        else if (Input.GetKey(KeyCode.A)) x = -speedIncrease;
        else if (isGrounded) x = 0;

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
        Crouch();
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
        if (groundCheck && !isGrounded) rb.velocity = rb.velocity - Vector3.up * rb.velocity.y;
        if (isGrounded && !groundCheck)
        {
            g = initialGravity;
        }
        if (groundCheck)
        {
            g = 0;
            airStrafe = initialAirStrafe;
        }
        isGrounded = groundCheck;
    }

    private void Move()
    {
        totalVelocity = Vector3.zero;
        newForwardandRight = Vector3.zero;

        actualForward = Vector3.Cross(hit.normal, -transform.right);
        actualRight = Vector3.Cross(hit.normal, transform.forward);

        currentForwardAndRight = new Vector3(rb.velocity.x, 0, rb.velocity.z);

        if (!isGrounded)
        {
            x *= airStrafe;
            z *= airStrafe;

            newForwardandRight = (transform.right * x + transform.forward * z);

            totalVelocity = newForwardandRight.normalized * currentForwardAndRight.magnitude * .25f + currentForwardAndRight * .75f;

            rb.velocity -= currentForwardAndRight * airFriction;

            //Debug.DrawLine(transform.position, transform.position + newForwardandRight.normalized * 5, Color.red);
        }
        else
        {
            newForwardandRight = (actualRight.normalized * x + actualForward.normalized * z);

            if (rb.velocity.magnitude < maxVelocity)
            {
                totalVelocity += newForwardandRight;

            }
            else if(!isCrouching)
            {
                rb.velocity = (newForwardandRight.normalized * currentForwardAndRight.magnitude * .5f + currentForwardAndRight * .5f).normalized * maxVelocity;
                totalVelocity = Vector3.zero;
            }
            if (rb.velocity.magnitude != maxVelocity || (x == 0 && z == 0))
            {
                totalVelocity -= rb.velocity * friction;
            }
        }
    }
    void Crouch()
    {
        if (!isCrouching && crouchBuffer)
        {
            capCollider.height *= .5f;
            capCollider.center += Vector3.up * -.5f;
            isCrouching = true;
            moveCamera.AdjustCameraHeight(true);

            if (isGrounded && !isSliding && rb.velocity.magnitude > velocityToSlide)StartCoroutine(SlideCoroutine());

        }
        if (isCrouching && !crouchBuffer)
        {
            capCollider.height *= 2f;
            capCollider.center += Vector3.up * .5f;
            isCrouching = false;
            moveCamera.AdjustCameraHeight(false);
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
    IEnumerator SlideCoroutine()
    {
        friction = slidingFriction;
        isSliding = true;
        totalVelocity += newForwardandRight * slideForce ;
        maxVelocity = maxWalkVelocity;
        while (rb.velocity.magnitude > maxVelocity && !isJumping)
        {
            yield return fixedUpdate;
        }
        friction = groundFriction;
        isSliding = false;
    }
    IEnumerator JumpCoroutine()
    {
        ResetJumpBuffer();
        isJumping = true;
        isGrounded = false;
        y = jumpStrength;
        g = initialGravity;
        _justJumpedCooldown = justJumpedCooldown;
        totalVelocity += newForwardandRight;
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
