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
    Vector3 newForwardandRight;
    Vector3 currentForwardAndRight;
    Vector3 velocityAtCollision;

    Vector3 lastViablePosition;

    Vector3 originalScale;
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
    #endregion

    #region Other
    private WaitForFixedUpdate fixedUpdate;
    public static PlayerController singleton;
    public LayerMask ignores;
    #endregion

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
        fixedUpdate = new WaitForFixedUpdate();
        friction = baseMovementVariables.inAirFriction;
        airControl = baseMovementVariables.inAirControl;
        SetInitialGravity(baseMovementVariables.initialGravity);
        _gravityRate = baseMovementVariables.gravityRate;
        playerState = PlayerState.InAir;
        baseMovementVariables.StartVariables(capCollider,transform);
        if (capCollider.radius * 2 * transform.lossyScale.x >=
            transform.lossyScale.y * capCollider.height) crouchMechanic = false;
    }

    void Update()
    {
        if (crouchMechanic) CrouchInput();
        MovementInput();
        if (jumpMechanic) JumpInput();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("MovingPlatform"))
        {
            originalScale = transform.localScale;
            transform.parent = collision.transform;
        }
    }
    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("MovingPlatform"))
        {
            transform.parent = null;
            transform.localScale = originalScale;
        }
    }
    private void FixedUpdate()
    {
        transform.rotation = Quaternion.Euler(0f, playerCamera.transform.localEulerAngles.y, 0f);
        GroundCheck();
        Move();
        if (useGravity)
        {
            if (crouchMechanic) HandleCrouchInput();
            if (jumpMechanic) HandleJumpInput();
            if (vaultMechanic) ClimbChecks();
            ApplyGravity();
        }
        rb.velocity += totalVelocityToAdd;
        if (rb.velocity.magnitude < baseMovementVariables.minVelocity && x == 0 && z == 0 && (isGrounded))        //If the player stops moving set its maxVelocity to walkingSpeed and set its rb velocity to 0
        {
            rb.velocity = Vector3.zero;
            isSprinting = false;
        }
        if (stuckBetweenSurfacesHelper >= 2) rb.velocity -= rb.velocity.y * Vector3.up;     //Allows the palyer to slide around when stuck between two or more surfaces
        if (vaultMechanic) ClimbChecks();
    }
}
