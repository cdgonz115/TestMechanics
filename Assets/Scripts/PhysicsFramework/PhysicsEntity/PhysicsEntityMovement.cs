using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class PhysicsEntity {
    protected float _friction;
    protected float _inAirFriction;
    protected float _groundedFriction;
    protected float _inAirControl;
    protected float _maxVelocity;

    [System.Serializable]
    public class MovementMechanic : PhysicsMechanic
    {
        #region Variables

        #region Velocity Caps
        [Header("Velocity Boundaries")]
        public float maxWalkVelocity = 7.5f;
        public float maxSprintVelocity = 7.5f;
        #endregion

        #region Friction
        [Header("Friction Values")]
        public float groundFriction = .1f;
        public float inAirFriction = .004f;
        #endregion

        #region In Air
        [Header("In Air Variables")]
        [Range(0, 1)]
        public float inAirControl = .021f;
        #endregion

        #endregion
    }

    protected Vector3 moveTargetPosition;
    protected Vector3 localVelocity;
    protected bool hasTargetPosition;
    protected virtual void SetTargetPosition(Vector3 position)
    {
        hasTargetPosition = (!moveTargetPosition.Equals(Vector3.negativeInfinity));
        moveTargetPosition = position;
    }
    protected virtual void MoveToTarget()
    {
        Vector3 direction = hasTargetPosition ? moveTargetPosition - transform.position : currentForwardAndRightVelocity;

        if (isGrounded)
        {
            if (direction.magnitude < GetColliderRadius() + 1)
            {
                totalVelocityToAdd = -rb.velocity;
                frictionToApply = Vector3.zero;
                return;
            }
            newForwardandRight = Vector3.ProjectOnPlane(direction, averageNormal).normalized;

            if(hasTargetPosition)
            {
                if ((rb.velocity + newForwardandRight).magnitude > _maxVelocity)
                {
                    rb.velocity = newForwardandRight.normalized * _maxVelocity;
                    totalVelocityToAdd = Vector3.zero;
                    frictionToApply = Vector3.zero;
                }
                else
                {
                    totalVelocityToAdd += newForwardandRight;
                }
            }
            else totalVelocityToAdd = Vector3.zero;
        }
        else
        {
            newForwardandRight = Vector3.ProjectOnPlane(direction, -gravityDirection).normalized;

            Vector3 newVelocity = newForwardandRight.normalized * currentForwardAndRightVelocity.magnitude * _inAirControl +
            currentForwardAndRightVelocity * (1f - _inAirControl);

            if (stuckBetweenSurfacesHelper > maxNumberOfInvalidSurfaces &&
                newVelocity.magnitude < stuckBetweenSurfacesVelocity)
            {
                frictionToApply = Vector3.zero;
                newVelocity = newForwardandRight.normalized * stuckBetweenSurfacesVelocity;
            }
            rb.velocity = newVelocity + velocityGravityComponent;
        }
    }
    public virtual void SetGroundedFriction(float friction) => _groundedFriction = friction;
    public virtual void ResetGroundedFriction() => _groundedFriction = movementMechanic.groundFriction;
    public virtual void SetInAirFriction(float friction) => _inAirFriction = friction;
    public virtual void ResetInAirFriction() => _inAirFriction = movementMechanic.inAirFriction;
}
