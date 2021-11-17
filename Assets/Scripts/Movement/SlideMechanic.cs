using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CrouchMechanic))]
public class SlideMechanic : MonoBehaviour
{
    public float velocityToSlide = 13;
    public float slideForce = 1.1f;
    public float downwardSlideForce = 1.05f;
    public float slidingFriction = 0.02f;
    public float downwardSlideAcceleration = -0.001f;
    [Range(0, 1)]
    public float slideControl = 0.025f;

    private WaitForFixedUpdate fixedUpdate;

    #region Components
    Rigidbody rb;
    #endregion

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        fixedUpdate = new WaitForFixedUpdate();
    }
    public IEnumerator SlideCoroutine()
    {
        float angle = Vector3.Angle(rb.velocity, Vector3.up);
        BaseMovement.singleton.friction = (angle > 90) ? downwardSlideAcceleration : slidingFriction;
        BaseMovement.singleton.previousState = BaseMovement.singleton.playerState;
        BaseMovement.singleton.playerState = PlayerState.Sliding;
        BaseMovement.singleton.totalVelocityToAdd += rb.velocity * ((angle > 90) ? downwardSlideForce : slideForce);
        BaseMovement.singleton.maxVelocity = BaseMovement.singleton.maxWalkVelocity;
        BaseMovement.singleton.isSprinting = false;
        while (rb.velocity.magnitude > BaseMovement.singleton.maxVelocity)
        {
            if (BaseMovement.singleton.playerState == PlayerState.Jumping) yield break;
            rb.velocity = BaseMovement.singleton.newForwardandRight.normalized * rb.velocity.magnitude * slideControl + rb.velocity * (1f - slideControl);
            if (!BaseMovement.singleton.isGrounded)
            {
                BaseMovement.singleton.friction = BaseMovement.singleton.inAirFriction;
                BaseMovement.singleton.previousState = PlayerState.Sliding;
                BaseMovement.singleton.isSprinting = true;
                yield break;
            }
            //if (!crouchBuffer)
            //{
            //    if (rb.velocity.magnitude > maxWalkVelocity) isSprinting = true;
            //    previousState = playerState;
            //    playerState = PlayerState.Grounded;
            //    yield break;
            //}
            yield return fixedUpdate;
        }
        BaseMovement.singleton.friction = BaseMovement.singleton.groundFriction;
        BaseMovement.singleton.previousState = BaseMovement.singleton.playerState;
        BaseMovement.singleton.playerState = PlayerState.Grounded;
    }
}
