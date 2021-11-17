using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class PlayerController
{
    [System.Serializable]
    public class CrouchVariables
    {
        public bool crouchBuffer;
        public bool topIsClear;
        public bool isCrouching;
        public bool canSlide;
        private SlideMechanic slideMechanic;
    }

    void CrouchInput()=> crouchVariables.crouchBuffer = Input.GetKey(KeyCode.LeftControl);
    public void HandleCrouchInput()
    {
        crouchVariables.topIsClear = !Physics.Raycast(transform.position - BaseMovement.singleton.newForwardandRight.normalized * BaseMovement.singleton.capCollider.radius, Vector3.up, BaseMovement.singleton.capCollider.height + .01f); // Check if thee's nothing blocking the player from standing up

        if (BaseMovement.singleton.isGrounded)
        {
            //Crouch
            if (!crouchVariables.isCrouching && crouchVariables.crouchBuffer)
            {
                BaseMovement.singleton.capCollider.height *= .5f;
                BaseMovement.singleton.capCollider.center += Vector3.up * -.5f;
                crouchVariables.isCrouching = true;
                moveCamera.AdjustCameraHeight(true);

                //Sliding Mechanic
                //if (crouchVariables.canSlide)
                    //if (BaseMovement.singleton.playerState != PlayerState.Sliding && BaseMovement.singleton.rb.velocity.magnitude > slideMechanic.velocityToSlide) StartCoroutine(slideMechanic.SlideCoroutine());

            }
            //Stand Up
            if (crouchVariables.isCrouching && !crouchVariables.crouchBuffer && BaseMovement.singleton.playerState != PlayerState.Sliding)
            {
                if (crouchVariables.topIsClear) //Checks that there are no obstacles on top of the player so they can stand up
                {
                    BaseMovement.singleton.capCollider.height *= 2f;
                    BaseMovement.singleton.capCollider.center += Vector3.up * .5f;
                    crouchVariables.isCrouching = false;
                    moveCamera.AdjustCameraHeight(false);
                }
            }
        }
        //else if (crouchBuffer && BaseMovement.singleton.playerState == PlayerState.InAir && timeSinceGrounded > .3f && distanceToGround > 5)
        //{
        //    GetComponent<DownLunge>().LungeDown(rb);
        //}
    }
}
