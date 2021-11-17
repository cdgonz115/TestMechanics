using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
public class BaseMovement : MonoBehaviour
{
    #region Variables
    #region Components
    [HideInInspector] public Rigidbody rb;
    [HideInInspector] public CapsuleCollider capCollider;
    [HideInInspector] public CrouchMechanic crouchMechanic;
    [HideInInspector] public JumpMechanic jumpMechanic;
    [HideInInspector] public VaultMechanic vaultMechanic;
    #endregion

    #region Primitive Variables
    [HideInInspector]public float x, z;
    [HideInInspector] public float g; 
    float pvX, pvZ;

    float groundCheckDistance;
    #endregion

    #region Player States
    [Header("Player States")]
    public bool isGrounded;
    bool groundCheck;
    public bool isSprinting;
    public bool onFakeGround;
    public PlayerState playerState;
    public PlayerState previousState;
    #endregion

    #region General
    [Header("General")]
    public float maxSlope;
    public float surfaceSlope;
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
    public float groundFriction;
    public float inAirFriction;
    public float friction;
    #endregion

    #region In Air
    [Header("In Air Variables")]
    [Range(0, 1)]
    public float inAirControl;
    public float airControl;
    #endregion

    #region Gravity
    [Header("Gravity Variables")]
    public float initialGravity;
    public float gravityRate;
    public float maxGravity;
    #endregion

    #region Fake Ground Checks
    private bool feetSphereCheck;
    private bool kneesCheck;
    public float fakeGroundTime;
    float _fakeGroundTimer;
    #endregion

    #region Vectors
    Vector3 groundedForward;
    Vector3 groundedRight;

    [HideInInspector] public Vector3 totalVelocityToAdd;
    [HideInInspector] public Vector3 newForwardandRight;
    [HideInInspector] public Vector3 currentForwardAndRight;
    Vector3 velocityAtCollision;

    Vector3 lastViablePosition;
    #endregion

    #region Raycast hits
    [HideInInspector] public RaycastHit hit;
    [HideInInspector] public RaycastHit feetHit;
    #endregion

    #region Events and delegates
    public delegate void PlayerBecameGrounded();
    public event PlayerBecameGrounded playerJustLanded;
    public delegate void PlayerLeftTheGround();
    public event PlayerLeftTheGround playerLeftGround;
    #endregion

    #region Other
    private WaitForFixedUpdate fixedUpdate;
    public static BaseMovement singleton;
    #endregion

    #endregion

    private void Awake()
    {
        if (singleton == null)
            singleton = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        capCollider = GetComponent<CapsuleCollider>();
        rb = GetComponent<Rigidbody>();
        if (GetComponent<CrouchMechanic>()) crouchMechanic = GetComponent<CrouchMechanic>();
        if (GetComponent<JumpMechanic>()) jumpMechanic = GetComponent<JumpMechanic>();
        if (GetComponent<VaultMechanic>()) vaultMechanic = GetComponent<VaultMechanic>();
        fixedUpdate = new WaitForFixedUpdate();
        groundCheckDistance = capCollider.height * .5f - capCollider.radius;
        friction = inAirFriction;
        g = initialGravity;
        playerState = PlayerState.InAir;
        airControl = inAirControl;
    }

    void Update()
    {
        if (crouchMechanic) crouchMechanic.UpdateMechanic();
        if (Input.GetKeyDown(KeyCode.LeftShift))
            if (crouchMechanic) isSprinting = (crouchMechanic.isCrouching ? false : true);
            else isSprinting = true; 

        speedIncrease = (isSprinting) ? sprintSpeedIncrease : walkSpeedIncrease;
        maxVelocity = (isSprinting) ? maxSprintVelocity : maxWalkVelocity;

        if (Input.GetKey(KeyCode.W)) z = speedIncrease;
        else if (Input.GetKey(KeyCode.S)) z = -speedIncrease;
        else z = 0;
        if (Input.GetKey(KeyCode.D)) x = speedIncrease;
        else if (Input.GetKey(KeyCode.A)) x = -speedIncrease;
        else x = 0;

        if (jumpMechanic) jumpMechanic.UpdateMechanic();

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (Time.timeScale == 1f) Time.timeScale = .1f;
            else Time.timeScale = 1;
        }
        else if (Input.GetKeyDown(KeyCode.Q)) Time.timeScale = 0;
    }

    private void FixedUpdate()
    {
        GroundCheck();
        Move();
        if (crouchMechanic) crouchMechanic.HandleCrouchInput();
        if (jumpMechanic) jumpMechanic.HandleJumpInput();
        ApplyGravity();
        rb.velocity += totalVelocityToAdd;
        if (rb.velocity.magnitude < minVelocity && x == 0 && z == 0 && (isGrounded))        //If the player stops moving set its maxVelocity to walkingSpeed and set its rb velocity to 0
        {
            rb.velocity = Vector3.zero;
            isSprinting = false;
        }
        vaultMechanic.ClimbChecks();
        vaultMechanic.HandleVault();
    }

    private void GroundCheck()
    {
        if (jumpMechanic)
        {
            if (jumpMechanic._coyoteTimer > 0) jumpMechanic._coyoteTimer -= Time.fixedDeltaTime;
            if (jumpMechanic._justJumpedCooldown > 0) jumpMechanic._justJumpedCooldown -= Time.fixedDeltaTime;
        }
        groundCheck = (!jumpMechanic || jumpMechanic._justJumpedCooldown <= 0) ? Physics.SphereCast(transform.position, capCollider.radius, -transform.up, out hit, groundCheckDistance + 0.01f) : false;
        surfaceSlope = Vector3.Angle(hit.normal, Vector3.up);
        if (surfaceSlope > maxSlope)
        {
            groundCheck = false;
            if (playerState != PlayerState.Climbing && playerState != PlayerState.Jumping && playerState != PlayerState.InAir)
            {
                previousState = playerState;
                playerState = PlayerState.InAir;
                g = initialGravity;
            }
        }
        totalVelocityToAdd = Vector3.zero;
        newForwardandRight = Vector3.zero;

        groundedForward = Vector3.Cross(hit.normal, -transform.right);
        groundedRight = Vector3.Cross(hit.normal, transform.forward);

        //If close to a small step, raise the player to the height of the step for better feeling movement
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
        //Player just landed
        if (groundCheck && (playerState == PlayerState.Jumping || playerState == PlayerState.InAir || playerState == PlayerState.Climbing))
        {
            lastViablePosition = transform.position;
            //timeSinceGrounded = 0;
            if (playerJustLanded != null) playerJustLanded();
            rb.velocity = rb.velocity - Vector3.up * rb.velocity.y;
            float angleOfSurfaceAndVelocity = Vector3.Angle(rb.velocity, (hit.normal - Vector3.up * hit.normal.y));
            if (!onFakeGround && hit.normal.y != 1 && angleOfSurfaceAndVelocity < 5 && z > 0) rb.velocity = (groundedRight * x + groundedForward * z).normalized * rb.velocity.magnitude;          //This is to prevent the weird glitch where the player bounces on slopes if they land on them without jumping
            friction = groundFriction;
            //_climbingCooldown = 0;
            previousState = playerState;
            playerState = PlayerState.Grounded;
            g = 0;
        }
        //Player just left the ground
        if (isGrounded && !groundCheck)
        {
            if (playerState != PlayerState.Jumping)
            {
                previousState = playerState;
                playerState = PlayerState.InAir;
                g = initialGravity;
            }
            friction = inAirFriction;
            if (playerLeftGround != null) playerLeftGround();
        }
        isGrounded = groundCheck;

        float maxDistance = capCollider.radius * (1 + ((isSprinting) ? (rb.velocity.magnitude / maxSprintVelocity) : 0));
        if (playerState == PlayerState.Grounded) feetSphereCheck = Physics.SphereCast(transform.position - Vector3.up * .5f, capCollider.radius + .01f, rb.velocity.normalized, out feetHit, maxDistance);
        if (feetSphereCheck && !onFakeGround)
        {
            Vector3 direction = feetHit.point - (transform.position - Vector3.up * .5f);
            float dist = direction.magnitude;
            kneesCheck = Physics.Raycast(transform.position - Vector3.up * capCollider.height * .24f, (direction - rb.velocity.y * Vector3.up), dist);
            if (!kneesCheck && playerState == PlayerState.Grounded && (x != 0 || z != 0))
            {
                StartCoroutine(FakeGround());
                isGrounded = true;
            } 
        }
    }
    private void Move()
    {
        currentForwardAndRight = new Vector3(rb.velocity.x, 0, rb.velocity.z);

        if (!isGrounded)
        {
            if (playerState != PlayerState.Climbing && playerState != PlayerState.Vaulting)
            {
                rb.velocity -= currentForwardAndRight * friction;

                newForwardandRight = (transform.right * x + transform.forward * z);
                if (z != 0 || x != 0)
                {
                    rb.velocity = newForwardandRight.normalized * currentForwardAndRight.magnitude * airControl + currentForwardAndRight * (1f - airControl) + rb.velocity.y * Vector3.up;
                }

            }
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
            else if (playerState != PlayerState.Sliding)
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
    public void SetInitialGravity() => g = initialGravity;
    private void ApplyGravity()
    {
        if (playerState != PlayerState.Climbing)
        {
            if (!isGrounded)
            {
                totalVelocityToAdd += Vector3.up * g;
            }
            if (g > maxGravity) g *= gravityRate;
        }
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
    public void ResetPosition()
    {
        rb.velocity = Vector3.zero;
        g = 0;
        transform.position = lastViablePosition;

    }
}
