using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class PlayerController
{
    [System.Serializable]
    public class CrouchVariables
    {
        [HideInInspector] public bool crouchBuffer;
        [HideInInspector] public bool topIsClear;
        [HideInInspector] public bool isCrouching;
        public bool holdCrouch;
        public bool slideMechanic;
        public float playerYScaleWhenCrouched = .5f;
        public float cameraDisplacement = 1;
        public bool standingUp;
    }
    void CrouchInput()
    {
        if (crouchVariables.holdCrouch)
        {
            //crouchVariables.crouchBuffer = InputManager.GetButton(PlayerInput.PlayerButton.Crouch);
            crouchVariables.crouchBuffer = Input.GetKey(KeyCode.LeftControl);
        }
        else if (Input.GetKeyDown(KeyCode.LeftControl)) crouchVariables.crouchBuffer = !crouchVariables.crouchBuffer;
        //else if (InputManager.GetButtonDown(PlayerInput.PlayerButton.Crouch)) crouchVariables.crouchBuffer = !crouchVariables.crouchBuffer;
    } 
    public void HandleCrouchInput()
    {
        crouchVariables.topIsClear = !Physics.Raycast(transform.position + newForwardandRight.normalized * capCollider.radius,
            transform.up, capCollider.height + .01f * transform.lossyScale.y, collisionMask, QueryTriggerInteraction.Ignore); // Check if thee's nothing blocking the player from standing up

            //Crouch
            if (!crouchVariables.isCrouching && crouchVariables.crouchBuffer
             && isGrounded)
            {
                capCollider.height *= crouchVariables.playerYScaleWhenCrouched;
                capCollider.center += Vector3.up * -crouchVariables.playerYScaleWhenCrouched;
                crouchVariables.isCrouching = true;
                playerCamera.AdjustCameraHeight(true, crouchVariables.cameraDisplacement);
                previousState = playerState;
                playerState = PlayerState.Crouching;

                //Sliding Mechanic
                if (crouchVariables.slideMechanic)
                    if (playerState != PlayerState.Sliding && rb.velocity.magnitude > slideVariables.velocityToSlide) StartCoroutine(SlideCoroutine());

            }
            //Stand Up
            if (crouchVariables.isCrouching && !crouchVariables.crouchBuffer && playerState != PlayerState.Sliding)
            {
                if (crouchVariables.topIsClear && !crouchVariables.standingUp) //Checks that there are no obstacles on top of the player so they can stand up
                {
                    StandUp();
                }
            }
    }
    private void StandUp()
    {
        crouchVariables.standingUp = true;
        if (Physics.Raycast(transform.position - newForwardandRight.normalized * capCollider.radius,
            transform.up, capCollider.height + .01f * transform.lossyScale.y, collisionMask, QueryTriggerInteraction.Ignore))
        {
            crouchVariables.standingUp = false;
            return;
        } 
        capCollider.height *= (1f / crouchVariables.playerYScaleWhenCrouched);
        capCollider.center += Vector3.up * crouchVariables.playerYScaleWhenCrouched;
        crouchVariables.isCrouching = false;
        playerCamera.AdjustCameraHeight(false, crouchVariables.cameraDisplacement);
        crouchVariables.standingUp = false;
        previousState = playerState;
        playerState = PlayerState.MovingInGround;
    }
}
