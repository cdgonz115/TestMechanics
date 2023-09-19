using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class PhysicsEntity {
    protected float _friction;
    protected float _inAirFriction;
    protected float _groundedFriction;
    protected float _inAirControl;
    protected float _maxVelocity;
    protected float _acceleration;

    [System.Serializable]
    public class MovementMechanic : PhysicsMechanic
    {
        #region Variables

        #region Acceleration
        [Header("Acceleration")]
        public float walkingAcceleration = 1;
        public float sprintingAcceleration = 2;
        #endregion

        #region Velocity Caps
        [Header("Velocity Boundaries")]
        public float maxWalkVelocity = 7.5f;
        public float maxSprintVelocity = 7.5f;
        #endregion

        #region Friction
        [Header("Friction Values")]
        public float noInputFriction = .2f;
        public float groundFriction = .1f;
        public float inAirFriction = .004f;
        #endregion

        #region In Air
        [Header("In Air Variables")]
        [Range(0, 1)]
        public float inAirControl = .021f;
        public float minAirVelocity = 2f;
        #endregion

        [HideInInspector] public float startingWalkingSpeed;
        [HideInInspector] public float startingSprintSpeed;

        #endregion
    }

    protected Vector3 moveTargetPosition;

    protected virtual void SetTargetPosition(Vector3 position)
    {
        if (position == null) moveTargetPosition = Vector3.negativeInfinity;
        else moveTargetPosition = position;
    }

    protected virtual void Move()
    {
        bool hasTarget = !(moveTargetPosition.Equals(Vector3.negativeInfinity));
        Vector3 direction = hasTarget ? moveTargetPosition - transform.position : currentForwardAndRightVelocity;

        if (isGrounded)
        {
            if (direction.magnitude < .6f)
            {
                rb.velocity = Vector3.zero;
                totalVelocityToAdd = Vector3.zero;
                return;
            }
            newForwardandRight = Vector3.ProjectOnPlane(direction, groundCheckHit.normal).normalized * _acceleration;
            if (hasTarget && rb.velocity.magnitude + newForwardandRight.magnitude > _maxVelocity)
            {
                float changeInDirectionAngle = Vector3.Angle(rb.velocity, newForwardandRight);
                rb.velocity = newForwardandRight.normalized * _maxVelocity;
                if (changeInDirectionAngle > 5) rb.velocity *= .0001f;
                totalVelocityToAdd = Vector3.zero;
            }
            else
            {
                totalVelocityToAdd += hasTarget ? newForwardandRight : Vector3.zero;
                rb.velocity -= currentForwardAndRightVelocity * _friction;
            }
        }
        else
        {
            newForwardandRight = Vector3.ProjectOnPlane(direction, -gravityDirection).normalized;

            Vector3 temp1 = newForwardandRight.normalized * (currentForwardAndRightVelocity.magnitude < .1f && stuckBetweenSurfacesHelper > 1 ?
            1f : currentForwardAndRightVelocity.magnitude) * _inAirControl;

            Vector3 temp2 = currentForwardAndRightVelocity * (1f - _inAirControl);

            Vector3 newVelocity = newForwardandRight.normalized * (currentForwardAndRightVelocity.magnitude < .1f && stuckBetweenSurfacesHelper > 1 ?
            1f : currentForwardAndRightVelocity.magnitude) * _inAirControl +
            currentForwardAndRightVelocity * (1f - _inAirControl);

            if (newVelocity.magnitude < movementMechanic.minAirVelocity) newVelocity = newVelocity.normalized * movementMechanic.minAirVelocity;

            rb.velocity = -currentForwardAndRightVelocity * _friction + newVelocity + velocityGravityComponent;
        }
    }
    public virtual void SetGroundedFriction(float friction) => _groundedFriction = friction;
    public virtual void ResetGroundedFriction() => _groundedFriction = movementMechanic.groundFriction;
    public virtual void SetInAirFriction(float friction) => _inAirFriction = friction;
    public virtual void ResetInAirFriction() => _inAirFriction = movementMechanic.inAirFriction;
}
