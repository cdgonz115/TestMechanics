using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrouchMechanic : MonoBehaviour, MovementRequiresInput
{
    public static CrouchMechanic singleton;
    public bool crouchBuffer;
    public bool topIsClear;
    public bool isCrouching;
    public bool canSlide;
    public MoveCamera moveCamera;
    private SlideMechanic slideMechanic;
    private void Awake()
    {
        if (singleton == null)
            singleton = this;
        else
            Destroy(gameObject);
    }
    void Start() 
    {
        moveCamera = GetComponent<MoveCamera>();
        if (canSlide)
        {
            if(!GetComponent<SlideMechanic>()) gameObject.AddComponent(typeof(SlideMechanic));
            slideMechanic = GetComponent<SlideMechanic>();
        }
    }
    public void UpdateMechanic() => crouchBuffer = Input.GetKey(KeyCode.LeftControl);
    public void HandleCrouchInput()
    {
        topIsClear = !Physics.Raycast(transform.position - BaseMovement.singleton.newForwardandRight.normalized * BaseMovement.singleton.capCollider.radius, Vector3.up, BaseMovement.singleton.capCollider.height + .01f); // Check if thee's nothing blocking the player from standing up
        
        if (BaseMovement.singleton.isGrounded)
        {
            //Crouch
            if (!isCrouching && crouchBuffer)
            {
                BaseMovement.singleton.capCollider.height *= .5f;
                BaseMovement.singleton.capCollider.center += Vector3.up * -.5f;
                isCrouching = true;
                moveCamera.AdjustCameraHeight(true);

                //Sliding Mechanic
                if(canSlide)
                    if (BaseMovement.singleton.playerState != PlayerState.Sliding && BaseMovement.singleton.rb.velocity.magnitude > slideMechanic.velocityToSlide) StartCoroutine(slideMechanic.SlideCoroutine());

            }
            //Stand Up
            if (isCrouching && !crouchBuffer && BaseMovement.singleton.playerState != PlayerState.Sliding)
            {
                if (topIsClear) //Checks that there are no obstacles on top of the player so they can stand up
                {
                    BaseMovement.singleton.capCollider.height *= 2f;
                    BaseMovement.singleton.capCollider.center += Vector3.up * .5f;
                    isCrouching = false;
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
