using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestMoveTwo : MonoBehaviour
{
    public enum PlayerState
    { 
        NotMoving,
        Grounded,
        Crouching,
        Sliding,
        Jumping,
        Climbing,
        Vaulting,
        InAir,
    };

    PlayerState playerState;

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

    public int inAirJumps;
    private int _inAirJumps;


    public float coyoteTimer;
    private float _coyoteTimer;

    float scrollwheel;

    public bool isCrouching;
    public bool crouchBuffer;


    public bool isSliding;
    public float slidingFriction;
    public float velocityToSlide;
    public float slideForce;

    public bool topCheck;
    public bool forwardCheck;

    public float climbingTime;
    public float _climbingTime;
    public float climbingForce;
    public float _climbingForce;
    public float initialClimbingGravity;
    public float climbingGravity;
    public float climbingGravityMultiplier;
    public bool isClimbing;
    public float climbingStrafe;
    public float _climbingStrafe;
    public float climbingStrafeDecreaser;
    public float negativeVelocityToClimb;

    public float vaultingHorizontalForce;

    RaycastHit hit;
    RaycastHit forwardHit;
    WaitForFixedUpdate fixedUpdate;

    private void Start()
    {
        groundCheckDistance = capCollider.height * .5f - capCollider.radius;
        g = initialGravity;
        fixedUpdate = new WaitForFixedUpdate();
        friction = groundFriction;
        //Time.timeScale = .1f;
    }

    private void Update()
    {

        crouchBuffer = Input.GetKey(KeyCode.LeftControl);

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            sprinting = (isCrouching) ? false : true;
        }
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
        ForwardCheck();
        Move();
        Crouch();
        HanldeJumpImput();
        ApplyGravity();
        rb.velocity += totalVelocity;
        if (rb.velocity.magnitude < .1f && x == 0 && z == 0 && (isGrounded))
        {
            rb.velocity = Vector3.zero;
            sprinting = false;
        } 

        Debug.DrawLine(transform.position, transform.position + actualForward.normalized * 5, Color.red);
        Debug.DrawLine(transform.position, transform.position + actualRight.normalized * 5, Color.red);
    }

    private void GroundCheck()
    {
        _coyoteTimer -= Time.fixedDeltaTime;
        if(_justJumpedCooldown>0)_justJumpedCooldown -= Time.fixedDeltaTime;
        groundCheck = (_justJumpedCooldown <=0)? Physics.SphereCast(transform.position, capCollider.radius + 0.01f, -transform.up, out hit, groundCheckDistance) : false;
        if (groundCheck && !isGrounded) rb.velocity = rb.velocity - Vector3.up * rb.velocity.y;
        if (isGrounded && !groundCheck)
        {
            g = initialGravity;
            _coyoteTimer = coyoteTimer;
        }
        if (groundCheck)
        {
            g = 0;
            airStrafe = initialAirStrafe;
            _inAirJumps = inAirJumps;
        }
        isGrounded = groundCheck;
    }
    void ForwardCheck()
    {

        topCheck = (Physics.Raycast(Camera.main.transform.position, transform.forward, capCollider.radius + .1f));

        forwardCheck =   (Physics.Raycast(transform.position, transform.forward, capCollider.radius + .1f));
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
            if (!isClimbing)
            {
                x *= airStrafe;
                z *= airStrafe;

                newForwardandRight = (transform.right * x + transform.forward * z);

                totalVelocity = newForwardandRight.normalized * currentForwardAndRight.magnitude * .25f + currentForwardAndRight * .75f;

                rb.velocity -= currentForwardAndRight * airFriction;
            }

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
                if (z == 0 && x == 0) rb.velocity *= .99f;
                else rb.velocity = newForwardandRight.normalized * maxVelocity;
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
    void HanldeJumpImput()
    {
        if (isGrounded) isJumping = false;
        if (jumpBuffer < 0) ResetJumpBuffer();
        if (jumpBuffer > 0 && (isGrounded || _coyoteTimer > 0) && !isJumping) StartCoroutine(JumpCoroutine());
        else if (_inAirJumps > 0 && jumpBuffer > 0)
        {
            _inAirJumps--;
            StartCoroutine(JumpCoroutine());
        }
        if (jumpBuffering) jumpBuffer -= Time.fixedDeltaTime;
    }
    public void ResetJumpBuffer()
    {
        jumpBuffering = false;
        jumpBuffer = 0;
    }
    private void ApplyGravity()
    {
        if (!isClimbing)
        {
            if (!isGrounded)
            {
                totalVelocity += Vector3.up * g;
            }
            if (g > maxGravity) g *= gravityRate;
        }
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
        sprinting = false;
    }
    IEnumerator JumpCoroutine()
    {
        bool continueSprint = isSliding;
        ResetJumpBuffer();
        isJumping = true;
        isGrounded = false;
        y = jumpStrength;
        g = initialGravity;
        _justJumpedCooldown = justJumpedCooldown;
        totalVelocity += newForwardandRight;
        rb.velocity -= Vector3.up * rb.velocity.y;
        while (y > 0.1f && !isGrounded)
        {
            y -= .05f;
            totalVelocity += Vector3.up * y;
            if (forwardCheck && rb.velocity.y > negativeVelocityToClimb)
            {
                isJumping = false;
                totalVelocity -= Vector3.up * y;
                StartCoroutine(ClimbCoroutine());
                yield break;
            }
            yield return fixedUpdate;
        }
        isJumping = false;
        sprinting = sprinting? sprinting :continueSprint;
    }
    IEnumerator ClimbCoroutine()
    {
        _climbingTime = climbingTime;
        isClimbing = true;
        _climbingForce = climbingForce;
        rb.velocity = Vector3.up * climbingForce;
        climbingGravity = initialClimbingGravity;
        //Physics.SphereCast(transform.position, 1, transform.forward, out forwardHit, capCollider.radius + .1f);
        Physics.BoxCast(transform.position + transform.forward.normalized * capCollider.radius, Vector3.one * capCollider.radius,transform.forward, out forwardHit);
        _climbingStrafe = climbingStrafe;
        while (forwardCheck && _climbingTime > 0 && rb.velocity.y > 0 && !isJumping)
        {
            rb.velocity -= Vector3.up * climbingGravity;
            rb.velocity+= Vector3.Cross(forwardHit.normal, Vector3.up).normalized * x * _climbingStrafe;
            Debug.DrawLine(transform.position, transform.position + Vector3.Cross(forwardHit.normal, Vector3.up).normalized * 5, Color.red);
            climbingGravity *= climbingGravityMultiplier;
            _climbingTime -= Time.fixedDeltaTime;
            if (forwardCheck && !topCheck) 
            {
                StartCoroutine(VaultCoroutine());
                yield break;
            }
            _climbingStrafe -= .001f;

            yield return fixedUpdate;
        }
        rb.velocity = Vector3.up * climbingForce + Vector3.Cross(forwardHit.normal, Vector3.up).normalized * x *vaultingHorizontalForce;
        isClimbing = false;
        while (rb.velocity.y > 0)
        {
            if (forwardCheck && !topCheck)
            {
                isClimbing = true;
                StartCoroutine(VaultCoroutine());
                yield break;
            } 
            yield return fixedUpdate;
        }
    }
    IEnumerator VaultCoroutine()
    {
        rb.velocity = Vector3.up * climbingForce;
        float height = Camera.main.transform.position.y;
        while (transform.position.y < height + capCollider.height)
        {
            rb.velocity += Vector3.up;
            yield return fixedUpdate;
        }
        isClimbing = false;
        rb.velocity = -forwardHit.normal * 10;

    }
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position - Vector3.up * (groundCheckDistance), capCollider.radius + 0.01f);
    }
}
