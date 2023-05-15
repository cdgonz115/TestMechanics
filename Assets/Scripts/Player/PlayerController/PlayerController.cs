using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class PlayerController : MonoBehaviour
{
    #region Variables

    #region Movement Mechanics
    [Header("Additional Mechanics")]
    public bool jumpMechanic;
    public bool crouchMechanic;
    public bool vaultMechanic;
    private bool movementDisabled;
    #endregion

    #region Additional Mechanics Variables
    public BaseMovementVariables baseMovementVariables = new BaseMovementVariables();
    public CrouchVariables crouchVariables = new CrouchVariables();
    public SlideVariables slideVariables = new SlideVariables();
    public JumpVariables jumpVariables = new JumpVariables();
    public VaultVariables vaultVariables = new VaultVariables();
    public ClimbVariables climbVariables = new ClimbVariables();
    #endregion

    #region Player States
    [Header("Player States")]
    public bool isGrounded;
    bool groundCheck;
    public bool isSprinting;
    public bool onFakeGround;
    public PlayerState playerState;
    public PlayerState previousState;
    public PlayerState animationState;
    #endregion

    #region Primitive Variables
    private float x, z;
    private float g;
    private float pvX, pvZ;
    private float y;
    #endregion

    #region Global Variables

    #region Basic Movement
    private float surfaceSlope;
    private float numerOfAngles;
    private float sumOfAllAngles;
    private float maxVelocity;
    private float speedIncrease;
    private float friction;
    private float airControl;
    private float _gravityRate;
    [HideInInspector] public bool useGravity = true;
    #endregion

    #region Jump
    private float _jumpBuffer;
    private float _highestPointHoldTimer;
    private float _justJumpedCooldown;
    private float _coyoteTimer;
    private int _inAirJumps;
    #endregion

    #region InAirVariables
    private int stuckBetweenSurfacesHelper;
    #endregion

    #endregion

    #region Vectors
    Vector3 groundedForward;
    Vector3 groundedRight;

    Vector3 totalVelocityToAdd;
    Vector3 externalVelocity;
    Vector3 parentVelocity;
    Vector3 newForwardandRight;
    Vector3 currentForwardAndRight;
    Vector3 velocityAtCollision;

    Vector3 lastViablePosition;
    #endregion

    #region Raycast hits
    [HideInInspector] public RaycastHit hit;
    [HideInInspector] public RaycastHit feetHit;
    [HideInInspector] public RaycastHit forwardHit;
    [HideInInspector] public RaycastHit rayToGround;
    #endregion

    #region Events and delegates
    public delegate void PlayerBecameGrounded();
    public event PlayerBecameGrounded playerJustLanded;
    public delegate void PlayerLeftTheGround();
    public event PlayerLeftTheGround playerLeftGround;
    #endregion

    #region Components
    Rigidbody rb;
    CapsuleCollider capCollider;
    public PlayerCamera playerCamera;
    public Animator animator;
    #endregion

    #region Other
    private WaitForFixedUpdate fixedUpdate;
    public static PlayerController singleton;
    public LayerMask collisionMask;
    #endregion

    public enum PlayerState
    {
        NotMoving = 0,
        MovingInGround = 1,
        Sprinting = 2,
        Crouching = 3,
        Sliding = 4,
        Jumping = 5,
        Climbing = 6,
        Vaulting = 7,
        InAir = 8,
    };

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
        Time.timeScale = .1f;
        lastViablePosition = transform.position;
        capCollider = GetComponent<CapsuleCollider>();
        rb = GetComponent<Rigidbody>();
        fixedUpdate = new WaitForFixedUpdate();
        friction = baseMovementVariables.inAirFriction;
        airControl = baseMovementVariables.inAirControl;
        SetInitialGravity(baseMovementVariables.initialGravity);
        SetGravityRate(baseMovementVariables.gravityRate);
        playerState = PlayerState.InAir;
        baseMovementVariables.StartVariables(capCollider, transform);
        if (capCollider.radius * 2 * transform.lossyScale.x >=
            transform.lossyScale.y * capCollider.height) crouchMechanic = false;
        //if (InputManager.inputMode == InputManager.InputMode.controller)
        //{
        //    baseMovementVariables.holdSprint = false;
        //    crouchVariables.holdCrouch = false;
        //}
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (Time.timeScale == 1) Time.timeScale = .1f;
            else Time.timeScale = 1;
        }
        if (Input.GetKeyDown(KeyCode.P)) Time.timeScale = 0;
        if (!movementDisabled)
        {
            if (crouchMechanic) CrouchInput();
            MovementInput();
            if (jumpMechanic) JumpInput();
        }
    }
    private void FixedUpdate()
    {
        if (parentVelocity != Vector3.zero) rb.velocity -= parentVelocity;
        transform.rotation = Quaternion.Euler(0f, playerCamera.transform.localEulerAngles.y, 0f);
        GroundCheck();
        Move();
        if (useGravity)
        {
            if (crouchMechanic) HandleCrouchInput();
            if (jumpMechanic) HandleJumpInput();
            ApplyGravity();
            if (vaultMechanic) ClimbChecks();
        }
        rb.velocity += totalVelocityToAdd;
        rb.velocity += externalVelocity;
        rb.velocity += parentVelocity;
        externalVelocity = Vector3.zero;
        if (rb.velocity.magnitude < baseMovementVariables.minVelocity && isGrounded) rb.velocity = Vector3.zero;    //If the players velocity goes below the miunimum velocity set it's rb velocity to 0
        if (stuckBetweenSurfacesHelper >= 2) rb.velocity -= rb.velocity.y * Vector3.up;     //Allows the player to slide around when stuck between two or more surfaces
        //print(playerState);
    }
    private void LateUpdate()
    {
        ChangeAnimation();
    }

    public void UpdateRespawnPoint() => lastViablePosition = transform.position;
    /// <summary>
    /// Reset the players position to the one set by a checkpoint
    /// </summary>
    public void ResetPosition()
    {
        rb.velocity = Vector3.zero;
        SetInitialGravity(baseMovementVariables.initialGravity);
        transform.position = lastViablePosition;
        previousState = playerState;
        playerState = PlayerState.InAir;
    }
    public void AddVelocity(Vector3 direction, float magnitude) => externalVelocity += direction.normalized * magnitude;
    public void SetVelocity(Vector3 direction, float magnitude) => rb.velocity = direction.normalized * magnitude;

    public void SetParentVelocity(Vector3 direction, float magnitude) => parentVelocity = direction.normalized * magnitude;
    public void DisableMovement()
    {
        x = 0;
        z = 0;
        movementDisabled = true;
    }
    public void EnableMovement() => movementDisabled = false;

    public void ChangeAnimation()
    {
        if (playerState == animationState) return;
        animator.SetInteger("PlayerState", (int)playerState);
        animationState = playerState;
    }
}
