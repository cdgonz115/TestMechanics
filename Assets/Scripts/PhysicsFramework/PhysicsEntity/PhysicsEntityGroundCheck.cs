using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class PhysicsEntity
{
    protected float sumOfAllAngles;
    protected float averageAngle;
    protected float surfaceSlope;
    protected int numberOfAngles;
    public int maxNumberOfSurfaces;

    protected int stuckBetweenSurfacesHelper = 0;

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
        [HideInInspector] public float groundCheckDistance;
        [HideInInspector] public float biggestSize = 0;
        public float maxSlope = 60;

        internal void CalculateGroundCheckDistance(SphereCollider collider, Transform transform)
        {
            if (Mathf.Abs(transform.lossyScale.x) > Mathf.Abs(transform.lossyScale.y)) biggestSize = transform.lossyScale.x;
            else biggestSize = transform.lossyScale.y;
            if (Mathf.Abs(biggestSize) < Mathf.Abs(transform.lossyScale.z)) biggestSize = transform.lossyScale.z;

            groundCheckDistance = biggestSize * collider.radius;
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

        RaycastHit[] hits = Physics.SphereCastAll(transform.position, (characterCollider.radius - .01f) * groundCheckMechanic.biggestSize, gravityDirection,
          .02f * groundCheckMechanic.biggestSize, collisionMask, QueryTriggerInteraction.Ignore);

        foreach (RaycastHit hit in hits)
        {
            if (hit.collider)
            {
                if (hit.point != Vector3.zero)
                {
                    float newSurfaceSlope = Vector3.Angle(hit.normal, -WorldGravity.singleton.GravityDirection);

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
                    if (newSurfaceSlope > groundCheckMechanic.maxSlope) stuckBetweenSurfacesHelper++;
                }
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
        //_friction = _groundedFriction;
    }
    protected virtual void CharacterLeftGround() {
        //_friction = _inAirFriction;
    }
}
