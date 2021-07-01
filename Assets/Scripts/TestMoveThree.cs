using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestMoveThree : MonoBehaviour
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

    #region Components
    private Rigidbody rb;
    private CapsuleCollider capCollider;
    #endregion

    #region Primitive Variables
    float x, z;
    float pvX, pvZ;
    float y, g;

    float scrollWheelDelta;
    float groundCheckDistance;
    #endregion

    #region Player States
    [Header("Player States")]
    public bool isGrounded;
    bool groundCheck;
    public bool isSprinting;
    public bool isJumping;
    public PlayerState playerState;
    public PlayerState previousState;
    #endregion

    #region Player States
    [Header("Acceleration")]
    public float walkSpeedIncrease;
    public float sprintSpeedIncrease;
    public float speedIncrease;
    #endregion

    #region Player States
    [Header("Velocity Boundaries")]
    public float maxWalkVelocity;
    public float maxSprintVelocity;
    public float maxVelocity;
    public float minVelocity;
    #endregion

    #region Player States
    [Header("Friction Values")]
    public float friction;
    public float groundFriction;
    public float inAirFriction;
    public float slidingFriction;
    #endregion

    #region Player States
    [Header("In Air Variables")]
    public float inAirControl;
    #endregion

    #region Player States
    [Header("Gravity Variables")]
    public float initialGravity;
    public float gravityRate;
    public float maxGravity;
    public float jumpingGravity;
    #endregion

    #region Player States
    [Header("Jump Mrchanic Variables")]
    public float jumpBuffer;
    public float _jumpBuffer;
    //public bool jumpBuffering;
    public float jumpStrength;
    public float justJumpedCooldown;
    float _justJumpedCooldown;
    public float coyoteTime;
    float _coyoteTimer;
    #endregion


    Vector3 actualForward;
    Vector3 actualRight;

    Vector3 totalVelocityToAdd;
    Vector3 newForwardandRight;
    Vector3 currentForwardAndRight;

    RaycastHit hit;

    WaitForFixedUpdate fixedUpdate;

    void Start()
    {
        capCollider = GetComponent<CapsuleCollider>();
        rb = GetComponent<Rigidbody>();
        fixedUpdate = new WaitForFixedUpdate();
        groundCheckDistance = capCollider.height * .5f - capCollider.radius;
        friction = inAirFriction;
        g = initialGravity;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            //previousState = playerState;
            //playerState = PlayerState.Sprinting;
            isSprinting = true;
        }
        speedIncrease = (isSprinting) ? sprintSpeedIncrease : walkSpeedIncrease;
        maxVelocity = (isSprinting)? maxSprintVelocity : maxWalkVelocity;

        if (Input.GetKey(KeyCode.W)) z = speedIncrease;
        else if (Input.GetKey(KeyCode.S)) z = -speedIncrease;
        else  z = 0;                             // we check if it's grounded given that in the air the z and x values should be changing
        if (Input.GetKey(KeyCode.D)) x = speedIncrease;
        else if (Input.GetKey(KeyCode.A)) x = -speedIncrease;
        else x = 0;

        scrollWheelDelta = Input.GetAxis("Mouse ScrollWheel");
        if (Input.GetKeyDown(KeyCode.Space) || scrollWheelDelta > 0)
        {
            _jumpBuffer = jumpBuffer;
        }
    }

    private void FixedUpdate()
    {
        GroundCheck();
        Move();
        HandleJumpInput();
        ApplyGravity();
        rb.velocity += totalVelocityToAdd;
        if (rb.velocity.magnitude < minVelocity && x == 0 && z == 0 && (isGrounded))
        {
            rb.velocity = Vector3.zero;
            //previousState = playerState;
            //playerState = PlayerState.Walking;
            isSprinting = false;
        }
    }
    private void GroundCheck()
    {
        if(_coyoteTimer> 0)_coyoteTimer -= Time.fixedDeltaTime;
        //groundCheck = (_justJumpedCooldown <= 0) ? Physics.SphereCast(transform.position, capCollider.radius + 0.01f, -transform.up, out hit, groundCheckDistance) : false;
        groundCheck = Physics.SphereCast(transform.position, capCollider.radius + 0.01f, -transform.up, out hit, groundCheckDistance);
        totalVelocityToAdd = Vector3.zero;
        newForwardandRight = Vector3.zero;

        actualForward = Vector3.Cross(hit.normal, -transform.right);
        actualRight = Vector3.Cross(hit.normal, transform.forward);

        if (groundCheck && !isGrounded)
        {
            rb.velocity = rb.velocity - Vector3.up * rb.velocity.y;

            if (actualForward.y != 0) rb.velocity = (actualRight.normalized + actualForward.normalized) * rb.velocity.magnitude;
        }
        if (isGrounded && !groundCheck)
        {
            previousState = playerState;
            playerState = PlayerState.InAir;
            g = initialGravity;
            _coyoteTimer = coyoteTime;
            friction = inAirFriction;
        }
        if (groundCheck)
        {
            g = 0;
            friction = groundFriction;
            previousState = playerState;
            playerState = PlayerState.Grounded;
            //airStrafe = initialAirStrafe;
            //_inAirJumps = inAirJumps;
            //_climbingCooldown = 0;
        }


        isGrounded = groundCheck;

    }
    private void Move()
    {
        currentForwardAndRight = new Vector3(rb.velocity.x, 0, rb.velocity.z);

        if (!isGrounded)
        {
            //if (!isClimbing)
            //{
            rb.velocity -= currentForwardAndRight * friction;

            newForwardandRight = (transform.right * x + transform.forward * z);
            if (z != 0 || x != 0) 
            {
                rb.velocity = newForwardandRight.normalized * currentForwardAndRight.magnitude * .25f + currentForwardAndRight * .75f + rb.velocity.y * Vector3.up;
            } 

            //}

            //Debug.DrawLine(transform.position, transform.position + newForwardandRight.normalized * 5, Color.red);
        }
        else
        {
            newForwardandRight = (actualRight.normalized * x + actualForward.normalized * z);

            if (rb.velocity.magnitude < maxVelocity)
            {
                totalVelocityToAdd += newForwardandRight;
            }
            else
            {

                if ((z == 0 && x == 0) || (pvX < 0 && x > 0) || (x < 0 && pvX > 0) || (pvZ < 0 && z > 0) || (z < 0 && pvZ > 0)) rb.velocity *= .99f; //Decrease 
                else if (rb.velocity.magnitude < maxVelocity + 5) rb.velocity = newForwardandRight.normalized * maxVelocity;
                totalVelocityToAdd = Vector3.zero;
            }

            if (rb.velocity.magnitude != maxVelocity || (x == 0 && z == 0))
            {
                totalVelocityToAdd -= rb.velocity * friction;
            }

            pvX = x;
            pvZ = z;
        }

    }
    private void HandleJumpInput()
    {
        if (isGrounded) isJumping = false;
        if(_jumpBuffer <= 0 ) _jumpBuffer = 0;
        //if (!isClimbing)
        //{
            if (_jumpBuffer > 0 && (isGrounded || _coyoteTimer > 0) && playerState!=PlayerState.Jumping) StartCoroutine(JumpCoroutine());
            //else if (_inAirJumps > 0 && jumpBuffer > 0)
            //{
            //    _inAirJumps--;
            //    StartCoroutine(JumpCoroutine());
            //}
        //}
        if (_jumpBuffer > 0) _jumpBuffer -= Time.fixedDeltaTime;
    }
    private void ApplyGravity()
    {
        //if (playerState != PlayerState.Climbing)
        //{
            if (!isGrounded)
            {
                totalVelocityToAdd += Vector3.up * g;
            }
            if (g > maxGravity) g *= gravityRate;
        //}
    }

    private IEnumerator JumpCoroutine()
    {
        _jumpBuffer = 0;
        previousState = playerState;
        playerState = PlayerState.Jumping;
        y = jumpStrength;
        g = initialGravity;
        _justJumpedCooldown = justJumpedCooldown;
        totalVelocityToAdd += newForwardandRight;
        rb.velocity -= Vector3.up * rb.velocity.y;
        while (y > 0.1f && playerState!=PlayerState.Grounded)
        {
            y -= .05f;
            totalVelocityToAdd += Vector3.up * y;
            //if (forwardCheck && rb.velocity.y > negativeVelocityToClimb && (z > 0 || currentForwardAndRight.magnitude > 1f) && _climbingCooldown <= 0)
            //{
            //    isJumping = false;
            //    totalVelocity -= Vector3.up * y;
            //    StartCoroutine(ClimbCoroutine());
            //    yield break;
            //}
            yield return fixedUpdate;
        }
        previousState = playerState;
        playerState = PlayerState.InAir;
    }
}
