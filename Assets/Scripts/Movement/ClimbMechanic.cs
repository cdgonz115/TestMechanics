using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(VaultMechanic))]
public class ClimbMechanic : MonoBehaviour
{
    #region Climb
    [Header("Climbing Variables")]
    public float negativeVelocityToClimb = -45;
    public float climbingDuration = 1;
    float _climbingTime;
    public float climbAcceleration = .5f;
    public float maxClimbingVelocity = 10;
    public float initialClimbingGravity = .5f;
    float _climbingGravity;
    public float climbingGravityMultiplier = 1.005f;
    public float climbingStrafe = .3f;
    float _climbingStrafe;
    public float climbStrafeDecreaser = .001f;
    public float maxClimbStrafeVelocity = 5;
    public float climbingStrafeFriction = .01f;
    public float endOfClimbJumpStrength = 3;
    public float endOfClimbJumpHeight = 4;
    public float climbingCooldown = 2;
    float _climbingCooldown;
    #endregion

    #region WallJump
    [Header("WallJump Variables")]
    public float wallJumpHeightStrenght = 5;
    public float wallJumpNormalStrength = 5;
    #endregion

    #region Components
    VaultMechanic vaultMechanic;
    JumpMechanic jumpMechanic;
    Rigidbody rb;
    CapsuleCollider capCollider;
    private WaitForFixedUpdate fixedUpdate;
    #endregion

    private void Start()
    {
        vaultMechanic = GetComponent<VaultMechanic>();
        jumpMechanic = GetComponent<JumpMechanic>();
        rb = GetComponent<Rigidbody>();
        fixedUpdate = new WaitForFixedUpdate();
        capCollider = GetComponent<CapsuleCollider>();
    }
    public void HandleClimb()
    {
        if (_climbingCooldown > 0) _climbingCooldown -= Time.fixedDeltaTime;
        if (BaseMovement.singleton.playerState == PlayerState.InAir && vaultMechanic.forwardCheck 
            && rb.velocity.y > negativeVelocityToClimb 
            && (BaseMovement.singleton.z > 0 || BaseMovement.singleton.currentForwardAndRight.magnitude > 0f) 
            && _climbingCooldown <= 0)
        {
            BaseMovement.singleton.previousState = BaseMovement.singleton.playerState;
            BaseMovement.singleton.playerState = PlayerState.Climbing;
            StartCoroutine(ClimbCoroutine());
        }
    }
    private IEnumerator ClimbCoroutine()
    {
        _climbingTime = climbingDuration;
        if(jumpMechanic)jumpMechanic._justJumpedCooldown = 0;
        _climbingGravity = initialClimbingGravity;
        Physics.BoxCast(transform.position - transform.forward.normalized * capCollider.radius * .5f, Vector3.one * capCollider.radius, transform.forward, out vaultMechanic.forwardHit, Quaternion.identity, 1f);
        _climbingStrafe = climbingStrafe;
        Vector3 playerOnWallRightDirection = Vector3.Cross(vaultMechanic.forwardHit.normal, Vector3.up).normalized;
        Vector3 originalHorizontalClimbingDirection = Vector3.Project(vaultMechanic.velocityAtCollision, playerOnWallRightDirection);
        Vector3 upwardDirection = (BaseMovement.singleton.surfaceSlope == 0) ? Vector3.up : -Vector3.Cross(BaseMovement.singleton.hit.normal, playerOnWallRightDirection).normalized;
        rb.velocity = originalHorizontalClimbingDirection;
        while (!BaseMovement.singleton.isGrounded && vaultMechanic.forwardCheck && BaseMovement.singleton.playerState == PlayerState.Climbing && _climbingTime > 0)
        {
            if (jumpMechanic._jumpBuffer > 0)
            {
                rb.velocity += Vector3.up * wallJumpHeightStrenght + vaultMechanic.forwardHit.normal * wallJumpNormalStrength;
                BaseMovement.singleton.g = jumpMechanic.jumpingInitialGravity;
                jumpMechanic.SetVariablesOnJump();
                _climbingCooldown = climbingCooldown;
                BaseMovement.singleton.previousState = BaseMovement.singleton.playerState;
                BaseMovement.singleton.playerState = PlayerState.InAir;
                yield break;
            }
            rb.velocity += upwardDirection.normalized * ((BaseMovement.singleton.z > 0) ? (rb.velocity.y > maxClimbingVelocity ? 0 : climbAcceleration) : (originalHorizontalClimbingDirection.magnitude > 7.5f) ? 0 : -_climbingGravity);
            rb.velocity += (BaseMovement.singleton.currentForwardAndRight.magnitude < maxClimbStrafeVelocity) ? playerOnWallRightDirection * BaseMovement.singleton.x * _climbingStrafe : Vector3.zero - BaseMovement.singleton.currentForwardAndRight * climbingStrafeFriction;
            _climbingGravity *= climbingGravityMultiplier;
            _climbingTime -= Time.fixedDeltaTime;
            _climbingStrafe -= climbStrafeDecreaser;

            yield return fixedUpdate;
        }
        if (BaseMovement.singleton.playerState == PlayerState.Vaulting) yield break;
        rb.velocity += Vector3.up * endOfClimbJumpHeight + playerOnWallRightDirection * BaseMovement.singleton.x + vaultMechanic.forwardHit.normal * endOfClimbJumpStrength;
        _climbingCooldown = climbingCooldown;
        BaseMovement.singleton.previousState = BaseMovement.singleton.playerState;
        if (!BaseMovement.singleton.isGrounded) BaseMovement.singleton.playerState = PlayerState.InAir;
        else rb.velocity = -Vector3.up * rb.velocity.y;
        BaseMovement.singleton.g = BaseMovement.singleton.initialGravity;
        StartCoroutine(EndOfClimbAirControl());
    }
    private IEnumerator EndOfClimbAirControl()
    {
        BaseMovement.singleton.airControl = jumpMechanic.jumpInAirControl;
        yield return new WaitForSeconds(.5f);
        BaseMovement.singleton.airControl = BaseMovement.singleton.inAirControl;
    }
}
