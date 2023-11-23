using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class BaseCharacter : MonoBehaviour
{
    #region Movement
    private float _friction;
    private float _iAirControl;
    private float _maxVelocity;
    #endregion

    Vector3 newForwardAndRight;

    public class MovementVariables : CharacterMechanicVariables
    {
        #region Variables

        #region Acceleration
        [Header("Acceleration")]
        public float walkSpeedAcceleration = 1;
        public float sprintSpeedAcceleration = 2;
        #endregion

        #region Velocity Caps
        [Header("Velocity Boundaries")]
        public float maxWalkVelocity = 7.5f;
        public float minVelocity = .1f;
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

    private void Move()
    {
        newForwardAndRight = Vector3.zero;
        if (!isGrounded)
        {
            rb.velocity -= currentForwardAndRightVelocity * _friction;
            newForwardAndRight = (transform.right * x + transform.forward * z);

            if (z != 0 || x != 0)
            {
                //If the game detects the player beeing stuck between two surfaces then it guarantees a min velocity to avoid a case where the stuck player's in air velocity would get stuck on zero 
                Vector3 newVelocity = newForwardAndRight.normalized * (newForwardAndRight.magnitude < .1f && stuckBetweenSurfacesHelper > 1 ?
                    1f : currentForwardAndRightVelocity.magnitude) * _iAirControl +
                    currentForwardAndRightVelocity * (1f - _iAirControl);
                //if (newVelocity.magnitude < movementVariables.minAirVelocity) newVelocity = newVelocity.normalized * movementVariables.minAirVelocity;
                rb.velocity = newVelocity + rb.velocity.y * Vector3.up;
            }
        }
        else 
        {
            newForwardAndRight = (groundedRight.normalized * x + groundedForward.normalized * z);
            if (groundCheckHit.normal.normalized == -gravityDirection.normalized)
            {
                newForwardAndRight = new Vector3(newForwardAndRight.x, 0, newForwardAndRight.z);
                rb.velocity = (rb.velocity - Vector3.up * rb.velocity.y).normalized * rb.velocity.magnitude;
            }
        }
    }
    private void SetXInput(float value) { x = value; }

    private void SetZInput(float value) { y = value; }
}
