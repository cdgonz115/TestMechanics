using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class PhysicsEntity
{
    #region GroundCheck Variables
    protected Dictionary<int, Vector3> groundCollisionNormals = new Dictionary<int, Vector3>();
    protected Vector3 averageNormal;
    protected bool groundCheck;
    public bool isGrounded;

    [Space(20)]
    [Tooltip("The number of invalid surfaces needed to let the Entity in the slide around")]
    [SerializeField] protected int maxNumberOfInvalidSurfaces = 1;
    [SerializeField] protected int maxSlope = 45;
    [SerializeField] protected float stuckBetweenSurfacesVelocity = 2f;

    protected int stuckBetweenSurfacesHelper = 0;

    #endregion

    #region Vectors
    protected Vector3 newForwardandRight;
    protected Vector3 currentForwardAndRightVelocity;
    protected Vector3 velocityGravityComponent;
    protected Vector3 newRigidBodyVelocity;
    protected Vector3 frictionToApply;
    #endregion
    protected virtual void CheckForGroundCollision(Collision collision)
    {
        Vector3 sumOfAllContactNormals = Vector3.zero;
        foreach (ContactPoint contact in collision.contacts)
        {
            float angle = Vector3.Angle(contact.normal, -gravityDirection);
            if (angle < 90) {
                sumOfAllContactNormals += contact.normal;
                Debug.DrawRay(transform.position + GetColliderHeight() * gravityDirection, contact.normal, Color.yellow);
                if (angle > maxSlope) {
                    stuckBetweenSurfacesHelper++;
                }
            }
        }
        if (sumOfAllContactNormals.Equals(Vector3.zero)) RemoveVectorFromDictionary(collision.collider.GetInstanceID());
        else groundCollisionNormals[collision.collider.GetInstanceID()] = sumOfAllContactNormals;
        Debug.DrawRay(transform.position + GetColliderHeight() * gravityDirection,sumOfAllContactNormals, Color.cyan);
    }
    protected virtual void GroundCheck()
    {
        velocityGravityComponent = Vector3.Project(rb.velocity, gravityDirection);
        currentForwardAndRightVelocity = rb.velocity - velocityGravityComponent;

        Vector3 sumOfAllNormals = groundCollisionNormals.Count < 1 ? gravityDirection : Vector3.zero;

        foreach (KeyValuePair<int, Vector3> keyedItem in groundCollisionNormals)
        {
            sumOfAllNormals += keyedItem.Value;
        }
        averageNormal = sumOfAllNormals.normalized;
        float normalsSlope = Vector3.Angle(sumOfAllNormals, -gravityDirection);

        groundCheck = (normalsSlope < maxSlope);

        totalVelocityToAdd = Vector3.zero;
        newForwardandRight = Vector3.zero;

        ////Character jsut got grounded
        if (groundCheck && !isGrounded)
        {
            _friction = _groundedFriction;
            Vector3 velocityOnSurfacePlane = Vector3.ProjectOnPlane(rb.velocity, averageNormal).normalized * rb.velocity.magnitude;
            rb.velocity = velocityOnSurfacePlane;

            CharacterLanded();
            SetInitialGravity(0);
        }
        ////Character just left the ground
        if (isGrounded && !groundCheck)
        {
            _friction = _inAirFriction;
            CharacterLeftGround();
            SetInitialGravity(gravityMechanic.initialGravityVelocity);
        }
        isGrounded = groundCheck;
    }
    protected virtual void CharacterLanded() {
        _friction = _groundedFriction;
    }
    protected virtual void CharacterLeftGround() {
        _friction = _inAirFriction;
    }
}
