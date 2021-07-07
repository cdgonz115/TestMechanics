using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestMoveThree : MonoBehaviour
{
    public enum PlayerState
    {
        NotMoving,
        Grounded,
        Sliding,
        Jumping,
        Climbing,
        Vaulting,
        InAir,
    };

    #region Components
    private Rigidbody rb;
    private CapsuleCollider capCollider;
    private MoveCamera moveCamera;
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
    public bool isCrouching;
    public bool onFakeGround;
    public PlayerState playerState;
    public PlayerState previousState;
    #endregion

    #region General
    [Header("General")]
    public float maxSlope;
    #endregion

    #region Acceleration
    [Header("Acceleration")]
    public float walkSpeedIncrease;
    public float sprintSpeedIncrease;
    public float speedIncrease;
    #endregion

    #region Velocity Caps
    [Header("Velocity Boundaries")]
    public float maxWalkVelocity;
    public float maxSprintVelocity;
    public float maxVelocity;
    public float minVelocity;
    #endregion

    #region Friction
    [Header("Friction Values")]
    public float friction;
    public float groundFriction;
    public float inAirFriction;
    public float slidingFriction;
    #endregion

    #region In Air
    [Header("In Air Variables")]
    [Range(0, 1)]
    public float inAirControl;
    #endregion

    #region Gravity
    [Header("Gravity Variables")]
    public float initialGravity;
    public float gravityRate;
    public float maxGravity;
    public float jumpingInitialGravity;
    #endregion

    #region Jump
    [Header("Jump Variables")]
    public float jumpBuffer;
    float _jumpBuffer;
    public float jumpStrength;
    public float jumpStregthDecreaser;
    public float jumpInAirForce;
    public float highestPointHoldTime;
    float _highestPointHoldTimer;
    public float justJumpedCooldown;
    float _justJumpedCooldown;
    public float coyoteTime;
    float _coyoteTimer;
    #endregion

    #region Crouch
    [Header("Crouch Variables")]
    public bool crouchBuffer;
    public bool topIsClear;
    #endregion

    #region Slide
    [Header("Slide Variables")]
    public float velocityToSlide;
    public float slideForce;
    [Range(0,1)]
    public float slideControl;
    #endregion

    #region Climbing Checks
    [Space]
    public bool feetSphereCheck;
    public bool kneesCheck;
    public float fakeGroundTime;
    float _fakeGroundTimer;

    public bool feetCheck;
    public bool headCheck;
    public bool forwardCheck;
    #endregion

    #region Vault
    [Header("Vault Variables")]
    public float negativeVelocityToClimb;
    public float climbingTime;
    float _climbingTime;
    public float climbingForce;
    float _climbingForce;
    public float initialClimbingGravity;
    public float climbingGravity;
    public float climbingGravityMultiplier;
    public float climbingStrafe;
    float _climbingStrafe;
    public float climbingStrafeDecreaser;
    public float climbingCooldown;
    float _climbingCooldown;
    #endregion

    //#region Vault
    //[Header("Vault Variables")]
    //public float va
    //#endregion

    #region Vectors
    Vector3 groundedForward;
    Vector3 groundedRight;

    Vector3 totalVelocityToAdd;
    Vector3 newForwardandRight;
    Vector3 currentForwardAndRight;
    #endregion

    #region Raycast hits
    RaycastHit hit;
    RaycastHit forwardHit;
    RaycastHit feetHit;
    #endregion

    WaitForFixedUpdate fixedUpdate;

    void Start()
    {
        capCollider = GetComponent<CapsuleCollider>();
        rb = GetComponent<Rigidbody>();
        moveCamera = GetComponent<MoveCamera>();
        fixedUpdate = new WaitForFixedUpdate();
        groundCheckDistance = capCollider.height * .5f - capCollider.radius;
        friction = inAirFriction;
        g = initialGravity;
        playerState = PlayerState.InAir;
        //Time.timeScale = .1f;
    }

    void Update()
    {
        crouchBuffer = Input.GetKey(KeyCode.LeftControl);

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            isSprinting = (isCrouching) ? false : true;
        }
        speedIncrease = (isSprinting) ? sprintSpeedIncrease : walkSpeedIncrease;
        maxVelocity = (isSprinting)? maxSprintVelocity : maxWalkVelocity;

        if (Input.GetKey(KeyCode.W)) z = speedIncrease;
        else if (Input.GetKey(KeyCode.S)) z = -speedIncrease;
        else  z = 0;                             
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
        Crouch();
        HandleJumpInput();
        ApplyGravity();
        rb.velocity += totalVelocityToAdd;
        if (rb.velocity.magnitude < minVelocity && x == 0 && z == 0 && (isGrounded))        //If the player stops moving set its maxVelocity to walkingSpeed and set its rb velocity to 0
        {
            rb.velocity = Vector3.zero;
            isSprinting = false;
        }
        ClimbingChecks();
        HandleVault();
        //Debug.DrawLine(transform.position, transform.position + actualForward.normalized * 5, Color.red);
        //Debug.DrawLine(transform.position, transform.position + actualRight.normalized * 5, Color.red);
    }
    private void GroundCheck()
    {
        if(_coyoteTimer> 0)_coyoteTimer -= Time.fixedDeltaTime;
        //groundCheck = (_justJumpedCooldown <= 0) ? Physics.SphereCast(transform.position, capCollider.radius + 0.01f, -transform.up, out hit, groundCheckDistance) : false;
        groundCheck = Physics.SphereCast(transform.position, capCollider.radius, -transform.up, out hit, groundCheckDistance + 0.01f);
        if (Vector3.Angle(hit.normal, Vector3.up) > maxSlope)
        {
            if(!isGrounded)
            {
                groundCheck = false;
                previousState = playerState;
                playerState = PlayerState.InAir;
                g = initialGravity;
            }
            
        }
        totalVelocityToAdd = Vector3.zero;
        newForwardandRight = Vector3.zero;

        groundedForward = Vector3.Cross(hit.normal, -transform.right);
        groundedRight = Vector3.Cross(hit.normal, transform.forward);

        if (onFakeGround)
        {
            if (groundCheck) onFakeGround = false;
            else
            {
                groundCheck = true;
                groundedForward = transform.forward;
                groundedRight = transform.right;
            } 
        }
        if (groundCheck && (playerState == PlayerState.Jumping || playerState == PlayerState.InAir))
        {
            rb.velocity = rb.velocity - Vector3.up * rb.velocity.y;
            if (!onFakeGround && hit.normal.y != 1)rb.velocity = (groundedRight* x + groundedForward* z).normalized * rb.velocity.magnitude;          //This is to prevent the weird glitch where the player bounces on slopes if they land on them without jumping
            friction = groundFriction;
            previousState = playerState;
            playerState = PlayerState.Grounded;
            g = 0;
        }
        if (isGrounded && !groundCheck)
        {
            if (playerState != PlayerState.Jumping)
            {
                previousState = playerState;
                playerState = PlayerState.InAir;
                g = initialGravity;
            }
            _coyoteTimer = coyoteTime;
            friction = inAirFriction;
        }
        isGrounded = groundCheck;
    }
    private void ClimbingChecks()
    {
        float maxDistance = capCollider.radius * (1 + ((isSprinting)?(rb.velocity.magnitude / maxSprintVelocity): 0) );
        if (playerState == PlayerState.Grounded) feetSphereCheck = Physics.SphereCast(transform.position - Vector3.up * .5f, capCollider.radius + .01f, rb.velocity.normalized, out feetHit, maxDistance);

        headCheck = (Physics.Raycast(Camera.main.transform.position + Vector3.up * .25f, transform.forward, capCollider.radius + .1f));
        //Debug.DrawLine(transform.position - Vector3.up * capCollider.height * .24f, (transform.position - Vector3.up * capCollider.height * .24f) + transform.forward * 5, Color.red);
        forwardCheck = (Physics.Raycast(transform.position, transform.forward, capCollider.radius + .1f));
        
        if (feetSphereCheck && !onFakeGround)
        {
            kneesCheck = Physics.Raycast(transform.position - Vector3.up * capCollider.height * .24f, transform.forward, maxDistance + capCollider.radius);
            if (!kneesCheck && playerState == PlayerState.Grounded && (x != 0 || z != 0))StartCoroutine(FakeGround());
        }
        else if (feetHit.collider && playerState == PlayerState.Grounded && !onFakeGround && feetHit.distance < capCollider.radius + .5f)
        {
            print("working");
            rb.velocity = Vector3.zero;
            isSprinting = false;
        }
        kneesCheck = false;
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
                if (playerState == PlayerState.Jumping) totalVelocityToAdd += newForwardandRight.normalized * jumpInAirForce;  
                rb.velocity = newForwardandRight.normalized * currentForwardAndRight.magnitude * inAirControl + currentForwardAndRight * (1f - inAirControl) + rb.velocity.y * Vector3.up;
            } 

            //}

            //Debug.DrawLine(transform.position, transform.position + newForwardandRight.normalized * 5, Color.red);
        }
        else
        {
            newForwardandRight = (groundedRight.normalized * x + groundedForward.normalized * z);
            if (hit.normal.y == 1) 
            {
                newForwardandRight = new Vector3(newForwardandRight.x, 0, newForwardandRight.z);
                rb.velocity = (rb.velocity - Vector3.up * rb.velocity.y).normalized * rb.velocity.magnitude;
            }

            if (rb.velocity.magnitude < maxVelocity)
            {
                totalVelocityToAdd += newForwardandRight;
            }
            else if(playerState != PlayerState.Sliding)
            {
                if ((z == 0 && x == 0) || (pvX < 0 && x > 0) || (x < 0 && pvX > 0) || (pvZ < 0 && z > 0) || (z < 0 && pvZ > 0)) rb.velocity *= .99f; //Decrease 
                else if (rb.velocity.magnitude < maxVelocity + 1f) rb.velocity = newForwardandRight.normalized * maxVelocity;
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
    private void Crouch()
    {
        topIsClear = !Physics.Raycast(transform.position - newForwardandRight.normalized * capCollider.radius, Vector3.up, capCollider.height + .01f); // Check if thee's nothing blocking the player from standing up
        //Crouch
        if (!isCrouching && crouchBuffer)
        {
            capCollider.height *= .5f;
            capCollider.center += Vector3.up * -.5f;
            isCrouching = true;
            moveCamera.AdjustCameraHeight(true);

            if (isGrounded && playerState != PlayerState.Sliding && rb.velocity.magnitude > velocityToSlide) StartCoroutine(SlideCoroutine());

        }
        //Stand Up
        if (isCrouching && !crouchBuffer)
        {
            if (topIsClear) //Checks that there are no obstacles on top of the player so they can stand up
            {
                capCollider.height *= 2f;
                capCollider.center += Vector3.up * .5f;
                isCrouching = false;
                moveCamera.AdjustCameraHeight(false);
            }
        }
    }
    private void HandleJumpInput()
    {
        if (isGrounded) isJumping = false;
        if(_jumpBuffer <= 0 ) _jumpBuffer = 0;
        //if (!isClimbing)
        //{
            if (_jumpBuffer > 0 && (isGrounded || _coyoteTimer > 0) && playerState!=PlayerState.Jumping && topIsClear) StartCoroutine(JumpCoroutine());
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

    private void HandleVault()
    {
        if (forwardCheck && !headCheck && z > 0) StartCoroutine(VaultCoroutine());
    }
    private IEnumerator FakeGround()
    {
        onFakeGround = true;
        transform.position = new Vector3(transform.position.x, feetHit.point.y + 1f, transform.position.z);
        g = 0;
        _fakeGroundTimer = fakeGroundTime;
        while (_fakeGroundTimer > 0 && onFakeGround)
        {
            _fakeGroundTimer -= Time.fixedDeltaTime;
            yield return fixedUpdate;
        }
        onFakeGround = false;
    }
    private IEnumerator SlideCoroutine()
    {
        friction = slidingFriction;
        previousState = playerState;
        playerState = PlayerState.Sliding;
        totalVelocityToAdd += currentForwardAndRight * slideForce;
        maxVelocity = maxWalkVelocity;
        isSprinting = false;
        while (rb.velocity.magnitude > maxVelocity)
        {
            rb.velocity = newForwardandRight.normalized * rb.velocity.magnitude * slideControl + rb.velocity * (1f - slideControl);
            if (!isGrounded)
            {
                friction = inAirFriction;
                previousState = PlayerState.Sliding;
                isSprinting = true;
                yield break;
            }
            if (!crouchBuffer)
            {
                if (rb.velocity.magnitude > maxWalkVelocity) isSprinting = true;
                previousState = playerState;
                playerState = PlayerState.Grounded;
                yield break;
            }
            yield return fixedUpdate;
        }
        friction = groundFriction;
        previousState = playerState;
        playerState = PlayerState.Grounded;
    }
    private IEnumerator JumpCoroutine()
    {
        _jumpBuffer = 0;
        previousState = playerState;
        playerState = PlayerState.Jumping;
        y = jumpStrength;
        g = jumpingInitialGravity;
        _justJumpedCooldown = justJumpedCooldown;
        totalVelocityToAdd += newForwardandRight;
        rb.velocity -= Vector3.up * rb.velocity.y;
        while (rb.velocity.y >= 0f && playerState!=PlayerState.Grounded)
        {
            y -= jumpStregthDecreaser;
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

        if(playerState != PlayerState.Grounded)
        {
            _highestPointHoldTimer = highestPointHoldTime;
            g = 0;
            rb.velocity -= rb.velocity.y * Vector3.up;
            while (_highestPointHoldTimer > 0)
            {
                _highestPointHoldTimer -= Time.fixedDeltaTime;
                yield return fixedUpdate;
            }
            g = initialGravity;
        }
        previousState = playerState;
        if(!isGrounded)playerState = PlayerState.InAir;
    }
    private IEnumerator VaultCoroutine()
    {
        print("vaulted");
        previousState = playerState;
        playerState = PlayerState.Climbing;
        rb.velocity = Vector3.up * climbingForce;
        float height = Camera.main.transform.position.y;
        Physics.BoxCast(transform.position - transform.forward.normalized * capCollider.radius * .5f, Vector3.one * capCollider.radius, transform.forward, out forwardHit, Quaternion.identity, 1f);
        feetCheck = (Physics.Raycast(transform.position - Vector3.up * capCollider.height * .5f, transform.forward, capCollider.radius + .1f));
        while (feetCheck)
        {
            feetCheck = (Physics.Raycast(transform.position - Vector3.up * capCollider.height * .5f, transform.forward, capCollider.radius + .1f));
            rb.velocity += Vector3.up;
            yield return fixedUpdate;
        }
        feetCheck = false;
        previousState = playerState;
        if (!isGrounded) playerState = PlayerState.InAir;
        rb.velocity = -forwardHit.normal * 10;

    }
}
