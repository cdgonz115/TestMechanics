using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class BaseCharacter : MonoBehaviour
{

    [System.Serializable]
    public class CharacterMechanicVariables
    {
        public bool enabled;
        public virtual void EnableMechanic() => enabled = true;

        public virtual void DisableMechanic() => enabled = false;
    }

    #region Components
    public Rigidbody rb;
    public SphereCollider characterCollider;
    #endregion

    #region Mechanics
    public GravityVariables gravityMechanic = new GravityVariables();
    public GroundCheckVariables groundCheckVariables = new GroundCheckVariables();
    //public MovementVariables movementVariables = new MovementVariables();
    #endregion

    #region Player States
    [Header("Player States")]
    //public bool isSprinting;
    //public bool onFakeGround;
    #endregion

    #region Primitive Variables
    private float x, z;
    private float g;
    private float pvX, pvZ;
    private float y;
    #endregion

    #region CharacterDefaults
    public float minVelocity;
    #endregion

    #region Vectors
    protected Vector3 totalVelocityToAdd;
    protected Vector3 localVelocity;
    protected Vector3 externalVelocity;
    public Vector3 parentVelocity;
    #endregion

    #region Raycast hits
    [HideInInspector] public RaycastHit groundHit;
    //[HideInInspector] public RaycastHit feetHit;
    //[HideInInspector] public RaycastHit forwardHit;
    //[HideInInspector] public RaycastHit rayToGround;
    #endregion

    #region Other
    private WaitForFixedUpdate fixedUpdate;
    public static PlayerController singleton;
    public LayerMask collisionMask;
    #endregion

    void Start()
    {
        SetInitialGravity(gravityMechanic.initialGravity);
        SetGravityRate(gravityMechanic.gravityRate);
        SetCharacterGravityDirection(WorldGravity.singleton.GravityDirection);
        groundCheckVariables.CalculateGroundCheckDistance(characterCollider, transform);

        gravity.applyGravity = () => ApplyGravity();
    }

    private void Update()
    {
        Debug.DrawLine(transform.position, transform.position + gravityDirection * (characterCollider.radius + .01f), Color.red);
    }

    private void FixedUpdate()
    {
        if (parentVelocity != Vector3.zero) rb.velocity -= parentVelocity;
        totalVelocityToAdd = Vector3.zero;

        GroundCheck();
        if (gravityMechanic.enabled)
        {
            ApplyGravity();
        }
        rb.velocity += parentVelocity;
        rb.velocity += externalVelocity;
        externalVelocity = Vector3.zero;
        if (rb.velocity.magnitude < minVelocity && isGrounded) rb.velocity = Vector3.zero;
        if (stuckBetweenSurfacesHelper >= 2) rb.velocity -= Vector3.Project(rb.velocity, gravityDirection);

    }
    public void SetParentVelocity(Vector3 direction, float magnitude) => parentVelocity = direction.normalized * magnitude;
    public void AddVelocity(Vector3 direction, float magnitude) => externalVelocity += direction.normalized * magnitude;
    public void SetVelocity(Vector3 direction, float magnitude) => rb.velocity = direction.normalized * magnitude;
}
