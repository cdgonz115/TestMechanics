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
    #endregion

    #region Global Variables
    private float surfaceSlope;
    private float maxVelocity;
    private float speedIncrease;
    private float friction;
    private float airControl;
    #endregion

    #region Additional Mechanics Variables
    public BaseMovementVariables baseMovementVariables = new BaseMovementVariables();
    public CrouchVariables crouchVariables = new CrouchVariables();
    #endregion

    #region Vectors
    Vector3 groundedForward;
    Vector3 groundedRight;

    Vector3 totalVelocityToAdd;
    Vector3 newForwardandRight;
    Vector3 currentForwardAndRight;
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

    #region Components
    Rigidbody rb;
    CapsuleCollider capCollider;
    MoveCamera moveCamera;
    //[HideInInspector] public CrouchMechanic crouchMechanic;
    //[HideInInspector] public JumpMechanic jumpMechanic;
    //[HideInInspector] public VaultMechanic vaultMechanic;
    #endregion

    #region Other
    private WaitForFixedUpdate fixedUpdate;
    public static BaseMovement singleton;
    #endregion

    #endregion
    void Start()
    {
        capCollider = GetComponent<CapsuleCollider>();
        rb = GetComponent<Rigidbody>();
        fixedUpdate = new WaitForFixedUpdate();
        friction = baseMovementVariables.inAirFriction;
        airControl = baseMovementVariables.inAirControl;
        g = baseMovementVariables.initialGravity;
        playerState = PlayerState.InAir;
        baseMovementVariables.StartVariables(capCollider);
    }

    void Update()
    {
        CrouchInput();
        MovementInput();
    }

    private void FixedUpdate()
    {
        GroundCheck();
        Move();
        HandleCrouchInput();
        //if (crouchMechanic) crouchMechanic.HandleCrouchInput();
        //if (jumpMechanic) jumpMechanic.HandleJumpInput();
        ApplyGravity();
    }
}
