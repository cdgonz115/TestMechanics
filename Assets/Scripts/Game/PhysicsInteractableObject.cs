using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PhysicsInteractableObject : MonoBehaviour
{
    #region Gravity
    protected float _gravityRate;
    protected Vector3 gravityDirection;
    protected Vector3 gravityCenter;
    #endregion

    #region Components
    public Rigidbody rb;
    #endregion

    #region BasicVariables
    [SerializeField] protected float minVelocity = 0.1f;
    [SerializeField] protected bool groundCheck;
    #endregion

    #region Mechanics
    public GravityVariables gravityMechanic = new GravityVariables();
    #endregion

    #region Primitive Variables
    protected float g;
    #endregion

    #region Vectors
    protected Vector3 totalVelocityToAdd;
    protected Vector3 localVelocity;
    protected Vector3 externalVelocity;
    protected Vector3 parentVelocity;
    #endregion

    [System.Serializable]
    public class PhysicsMechanic
    {
        public bool enabled;
        public virtual void EnableMechanic() => enabled = true;
        public virtual void DisableMechanic() => enabled = false;
    }
    [System.Serializable]
    public class GravityVariables : PhysicsMechanic
    {
        public float maxGravityVelocity = -39.2f;
        public float initialGravity = -.55f;
        public float gravityRate = 1.008f;
    }

    protected void Start()
    {
        SetInitialGravity(gravityMechanic.initialGravity);
        SetGravityRate(gravityMechanic.gravityRate);
        SetGravityDirection(WorldGravity.singleton?.GravityDirection??Physics.gravity);
    }
    protected void FixedUpdate()
    {
        if (parentVelocity != Vector3.zero) rb.velocity -= parentVelocity;
        totalVelocityToAdd = Vector3.zero;

        if (gravityMechanic.enabled)
        {
            ApplyGravity();
        }

        rb.velocity += totalVelocityToAdd;
        rb.velocity += parentVelocity;
        rb.velocity += externalVelocity;

        externalVelocity = Vector3.zero;
        if (rb.velocity.magnitude < minVelocity) rb.velocity = Vector3.zero;
    }
    protected void Awake()
    {
        RigidBodySetUp();
    }

    protected void OnCollisionEnter(Collision collision)
    {
        GroundCheck(collision);
    }

    protected virtual void GroundCheck(Collision collision = null)
    {
        float smallestAngle = 181;
        foreach (ContactPoint point in collision.contacts)
        {
            float angle = Vector3.Angle(gravityDirection, point.point - transform.position);
            if (angle < smallestAngle) smallestAngle = angle;
        }
        groundCheck = (smallestAngle < 3);
    }
    protected virtual void RigidBodySetUp()
    {
        rb = GetComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
    }
    public void SetParentVelocity(Vector3 direction, float magnitude) {
        if (direction == Vector3.zero) rb.velocity -= parentVelocity;
        parentVelocity = direction.normalized * magnitude;
        rb.velocity += parentVelocity;
    }
    public void AddVelocity(Vector3 direction, float magnitude) => externalVelocity += direction.normalized * magnitude;
    public void SetVelocity(Vector3 direction, float magnitude) => rb.velocity = direction.normalized * magnitude;
    protected void ApplyGravity()
    {
        if (gravityCenter != Vector3.zero) SetGravityDirection(gravityCenter - transform.position);
        if (!groundCheck) totalVelocityToAdd += (-gravityDirection) * g;
        if (g > gravityMechanic.maxGravityVelocity) g *= _gravityRate;
    }
    public void SetGravityDirection(Vector3 direction) {
        gravityDirection = direction.normalized;
        groundCheck = false;
    } 
    public void SetGravityCenter(Vector3 point) {
        gravityCenter = point;
        groundCheck = false;
    } 
    public void ToggleGravity(bool isActice)
    {
        gravityMechanic.enabled = isActice;
        groundCheck = false;
        SetInitialGravity(0);
    }
    public void SetInitialGravity(float value) => g = value;
    public void SetGravityRate(float value) => _gravityRate = value;
}
