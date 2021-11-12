using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrouchingMechanic : MonoBehaviour
{
    public static CrouchingMechanic singleton;
    public bool crouchBuffer;
    public bool topIsClear;
    public bool isCrouching;
    public MoveCamera moveCamera;
    private void Awake()
    {
        if (singleton == null)
            singleton = this;
        else
            Destroy(gameObject);
    }
    void Start()=> moveCamera = GetComponent<MoveCamera>();
    void Update() => crouchBuffer = Input.GetKey(KeyCode.LeftControl);
    public void Crouch()
    {
        topIsClear = !Physics.Raycast(transform.position - BaseMovement.singleton.newForwardandRight.normalized * BaseMovement.singleton.capCollider.radius, Vector3.up, BaseMovement.singleton.capCollider.height + .01f); // Check if thee's nothing blocking the player from standing up
        
        if (BaseMovement.singleton.isGrounded)
        {
            //Crouch
            if (!isCrouching && crouchBuffer)
            {
                BaseMovement.singleton.capCollider.height *= .5f;
                BaseMovement.singleton.capCollider.center += Vector3.up * -.5f;
                BaseMovement.singleton.blockSprinting = true;
                isCrouching = true;
                moveCamera.AdjustCameraHeight(true);

                //if (BaseMovement.singleton.playerState != PlayerState.Sliding && BaseMovement.singleton.rb.velocity.magnitude > velocityToSlide) StartCoroutine(SlideCoroutine());

            }
            //Stand Up
            if (isCrouching && !crouchBuffer && BaseMovement.singleton.playerState != PlayerState.Sliding)
            {
                if (topIsClear) //Checks that there are no obstacles on top of the player so they can stand up
                {
                    BaseMovement.singleton.capCollider.height *= 2f;
                    BaseMovement.singleton.capCollider.center += Vector3.up * .5f;
                    isCrouching = false;
                    BaseMovement.singleton.blockSprinting = false;
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
