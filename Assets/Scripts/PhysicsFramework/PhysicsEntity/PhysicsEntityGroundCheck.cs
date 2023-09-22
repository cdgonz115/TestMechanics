using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class PhysicsEntity
{
    protected float sumOfAllAngles;
    protected float averageAngle;
    protected float surfaceSlope;
    protected int numberOfAngles;

    [Space(20)]
    [Tooltip("The number of invalid surfaces needed to let the Entity in the slide around")]
    [SerializeField] protected int maxNumberOfInvalidSurfaces = 1;
    [SerializeField] protected float stuckBetweenSurfacesVelocity = 2f;

    public int stuckBetweenSurfacesHelper = 0;

    protected RaycastHit groundCheckHit;

    #region Vectors
    protected Vector3 newForwardandRight;
    protected Vector3 currentForwardAndRightVelocity;
    protected Vector3 velocityGravityComponent;
    protected Vector3 newRigidBodyVelocity;

    #endregion

    [System.Serializable]
    public class GroundCheckMechanic : PhysicsMechanic
    {
        [HideInInspector] public float colliderRadius;
        [HideInInspector] public float biggestSize = 0;
        public float groundCheckMaxDistance = 0.05f;
        public float groundCheckRadiusOffset = .01f;
        public float groundCheckStartingPointOffset = .01f;
        public float maxSlope = 60;

        internal void CalculateColliderRadius(SphereCollider collider, Transform transform)
        {
            if (Mathf.Abs(transform.lossyScale.x) > Mathf.Abs(transform.lossyScale.y)) biggestSize = transform.lossyScale.x;
            else biggestSize = transform.lossyScale.y;
            if (Mathf.Abs(biggestSize) < Mathf.Abs(transform.lossyScale.z)) biggestSize = transform.lossyScale.z;

            colliderRadius = biggestSize * collider.radius;
        }
    }

    protected virtual void GroundCheck()
    {
        sumOfAllAngles = 0;
        stuckBetweenSurfacesHelper = 0;
        numberOfAngles = 0;
        groundCheckHit = new RaycastHit();
        velocityGravityComponent = Vector3.Project(rb.velocity, gravityDirection);
        currentForwardAndRightVelocity = rb.velocity - velocityGravityComponent;

        if (_justJumpedCooldown > 0) _justJumpedCooldown -= Time.fixedDeltaTime;

        RaycastHit[] hits = new RaycastHit[2];

        Physics.SphereCastNonAlloc(transform.position + (-gravityDirection * groundCheckMechanic.groundCheckStartingPointOffset),
            groundCheckMechanic.colliderRadius - groundCheckMechanic.groundCheckRadiusOffset, gravityDirection, hits,
          groundCheckMechanic.groundCheckMaxDistance * groundCheckMechanic.biggestSize, collisionMask, QueryTriggerInteraction.Ignore);

        foreach (RaycastHit hit in hits)
        {
            if (hit.collider)
            {
                if (hit.point != Vector3.zero)
                {
                    float newSurfaceSlope = Vector3.Angle(hit.normal, -gravityDirection);

                    if (!groundCheckHit.collider)
                    {
                        groundCheckHit = hit;
                        surfaceSlope = newSurfaceSlope;
                        sumOfAllAngles += newSurfaceSlope;
                        numberOfAngles++;
                    }
                    else
                    {
                        if (newSurfaceSlope <= surfaceSlope)
                        {
                            groundCheckHit = hit;
                            surfaceSlope = newSurfaceSlope;
                            sumOfAllAngles += newSurfaceSlope;
                            numberOfAngles++;
                        }
                    }
                    //Debug.Log(newSurfaceSlope);
                    if (newSurfaceSlope > groundCheckMechanic.maxSlope) stuckBetweenSurfacesHelper++;
                }
                else stuckBetweenSurfacesHelper++;
            }
        }

        averageAngle = sumOfAllAngles / numberOfAngles * 1.0f;
        groundCheck = (!jumpMechanic.enabled || _justJumpedCooldown <= 0) ? (groundCheckHit.collider) : false;

        if (surfaceSlope > groundCheckMechanic.maxSlope)
        {
            SetInitialGravity(gravityMechanic.initialGravityVelocity);
            groundCheck = false;
        }

        totalVelocityToAdd = Vector3.zero;
        newForwardandRight = Vector3.zero;

        //Character just became grounded
        if (groundCheck && !isGrounded)
        {
            Vector3 velocityOnSurfacePlane = Vector3.ProjectOnPlane(rb.velocity, groundCheckHit.normal).normalized * rb.velocity.magnitude;
            rb.velocity = velocityOnSurfacePlane;

            CharacterLanded();
            SetInitialGravity(0);
        }
        //Character just left the ground
        if (isGrounded && !groundCheck)
        {
            _justJumpedCooldown = jumpMechanic.justJumpedCooldown;
            surfaceSlope = 0;
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
