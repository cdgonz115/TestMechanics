using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpMechanic : MonoBehaviour, MovementRequiresInput
{
    public static JumpMechanic singleton;
    float y;

    float scrollWheelDelta;
    public float jumpBuffer;
    public float _jumpBuffer;
    public float jumpStrength;
    public float jumpStregthDecreaser;
    public float jumpInAirStrength;
    public float jumpInAirControl;
    public float jumpingInitialGravity;

    public float highestPointHoldTime;
    float _highestPointHoldTimer;
    public float justJumpedCooldown;
    public float _justJumpedCooldown;
    public float coyoteTime;
    public float _coyoteTimer;

    public int inAirJumps;
    private int _inAirJumps;

    private WaitForFixedUpdate fixedUpdate;

    private void Awake()
    {
        if (singleton == null)
            singleton = this;
        else
            Destroy(gameObject);
    }
    private void Start()
    {
        BaseMovement.singleton.playerJustLanded += PlayerLanded;
        BaseMovement.singleton.playerLeftGround += PlayerLeftGround;
        fixedUpdate = new WaitForFixedUpdate();
    }
    // Update is called once per frame
    public void UpdateMechanic()
    {
        scrollWheelDelta = Input.GetAxis("Mouse ScrollWheel");
        if (Input.GetKeyDown(KeyCode.Space) || scrollWheelDelta > 0)
        {
            _jumpBuffer = jumpBuffer;
        }
    }

    public void HandleJumpInput()
    {
        if (_jumpBuffer <= 0) _jumpBuffer = 0;
        if (BaseMovement.singleton.playerState != PlayerState.Climbing)
        {
            if (_jumpBuffer > 0 && (BaseMovement.singleton.isGrounded || _coyoteTimer > 0) && BaseMovement.singleton.playerState != PlayerState.Jumping && (BaseMovement.singleton.crouchMechanic?CrouchMechanic.singleton.topIsClear:true)) StartCoroutine(JumpCoroutine(false));
            else if (BaseMovement.singleton.playerState == PlayerState.InAir && _inAirJumps > 0 && _jumpBuffer > 0)
            {
                _inAirJumps--;
                StartCoroutine(JumpCoroutine(true));
            }
        }
        if (_jumpBuffer > 0) _jumpBuffer -= Time.fixedDeltaTime;
    }
    private IEnumerator JumpCoroutine(bool inAirJump)
    {
        SetVariablesOnJump();
        BaseMovement.singleton.previousState = BaseMovement.singleton.playerState;
        BaseMovement.singleton.playerState = PlayerState.Jumping;
        y = jumpStrength;
        BaseMovement.singleton.g = jumpingInitialGravity;
        BaseMovement.singleton.totalVelocityToAdd += BaseMovement.singleton.newForwardandRight;
        BaseMovement.singleton.airControl = jumpInAirControl;
        if (inAirJump)
        {
            if ((BaseMovement.singleton.x != 0 || BaseMovement.singleton.z != 0)) BaseMovement.singleton.rb.velocity = BaseMovement.singleton.newForwardandRight.normalized * ((BaseMovement.singleton.currentForwardAndRight.magnitude < BaseMovement.singleton.maxSprintVelocity) ? BaseMovement.singleton.maxSprintVelocity : BaseMovement.singleton.currentForwardAndRight.magnitude);
            else BaseMovement.singleton.rb.velocity = Vector3.zero;
        }
        else BaseMovement.singleton.rb.velocity -= BaseMovement.singleton.rb.velocity.y * Vector3.up;
        while (BaseMovement.singleton.rb.velocity.y >= 0f && BaseMovement.singleton.playerState != PlayerState.Grounded)
        {
            y -= jumpStregthDecreaser;
            BaseMovement.singleton.totalVelocityToAdd += Vector3.up * y;
            yield return fixedUpdate;
        }
        if (BaseMovement.singleton.playerState != PlayerState.Grounded)
        {
            _highestPointHoldTimer = highestPointHoldTime;
            BaseMovement.singleton.g = 0;
            BaseMovement.singleton.rb.velocity -= Vector3.up * BaseMovement.singleton.rb.velocity.y;
            while (_highestPointHoldTimer > 0)
            {
                _highestPointHoldTimer -= Time.fixedDeltaTime;
                yield return fixedUpdate;
            }
            BaseMovement.singleton.g = BaseMovement.singleton.initialGravity;
        }
        BaseMovement.singleton.airControl = BaseMovement.singleton.inAirControl;
        if (BaseMovement.singleton.rb.velocity.magnitude >= BaseMovement.singleton.maxSprintVelocity) BaseMovement.singleton.isSprinting = true;
        BaseMovement.singleton.previousState = BaseMovement.singleton.playerState;
        if (!BaseMovement.singleton.isGrounded) BaseMovement.singleton.playerState = PlayerState.InAir;
    }
    public void SetVariablesOnJump()
    {
        _jumpBuffer = 0;
        _justJumpedCooldown = justJumpedCooldown;
    }
    public void PlayerLanded() => _inAirJumps = inAirJumps;
    public void PlayerLeftGround() => _coyoteTimer = coyoteTime;
}

