using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class PlayerController
{
    [System.Serializable]
    public class BaseMovementVariables
    {
        #region Variables

        #region Primitive Variables
        [HideInInspector] public float x, z;
        [HideInInspector] public float g;
        [HideInInspector]public float pvX, pvZ;

        public float groundCheckDistance;
        #endregion

        #region General
        [Header("General")]
        public float maxSlope = 60;
        public float surfaceSlope;
        #endregion

        #region Acceleration
        [Header("Acceleration")]
        public float walkSpeedIncrease = 1;
        public float sprintSpeedIncrease = 2;
        public float speedIncrease;
        #endregion

        #region Velocity Caps
        [Header("Velocity Boundaries")]
        public float maxWalkVelocity = 7.5f;
        public float maxSprintVelocity = 15;
        public float maxVelocity;
        public float minVelocity = .1f;
        #endregion

        #region Friction
        [Header("Friction Values")]
        public float groundFriction = .1f;
        public float inAirFriction = .004f;
        public float friction;
        #endregion

        #region In Air
        [Header("In Air Variables")]
        [Range(0, 1)]
        public float inAirControl = .021f;
        public float airControl;
        #endregion

        #region Gravity
        [Header("Gravity Variables")]
        public float initialGravity = -.55f;
        public float gravityRate = 1.008f;
        public float maxGravity = -39.2f;
        #endregion

        #region Fake Ground Checks
        public bool feetSphereCheck;
        public bool kneesCheck;
        public float fakeGroundTime = .1f;
        public float _fakeGroundTimer;
        #endregion

        #endregion

        public void StartVariables(CapsuleCollider capCollider)
        {
            friction = inAirFriction;
            g = initialGravity;
            airControl = inAirControl;
            groundCheckDistance = capCollider.height * .5f - capCollider.radius;
        }
    }

    private void MovementInput()
    {
        //if (Input.GetKeyDown(KeyCode.LeftShift))
        //    if (crouchMechanic) isSprinting = (crouchMechanic.isCrouching ? false : true);
        //    else isSprinting = true;

        baseMovementVariables.speedIncrease = (isSprinting) ? baseMovementVariables.sprintSpeedIncrease : baseMovementVariables.walkSpeedIncrease;
        baseMovementVariables.maxVelocity = (isSprinting) ? baseMovementVariables.maxSprintVelocity : baseMovementVariables.maxWalkVelocity;

        if (Input.GetKey(KeyCode.W)) baseMovementVariables.z = baseMovementVariables.speedIncrease;
        else if (Input.GetKey(KeyCode.S)) baseMovementVariables.z = -baseMovementVariables.speedIncrease;
        else baseMovementVariables.z = 0;
        if (Input.GetKey(KeyCode.D)) baseMovementVariables.x = baseMovementVariables.speedIncrease;
        else if (Input.GetKey(KeyCode.A)) baseMovementVariables.x = -baseMovementVariables.speedIncrease;
        else baseMovementVariables.x = 0;

        //if (jumpMechanic) jumpMechanic.UpdateMechanic();

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (Time.timeScale == 1f) Time.timeScale = .1f;
            else Time.timeScale = 1;
        }
        else if (Input.GetKeyDown(KeyCode.Q)) Time.timeScale = 0;
    }
    private void GroundCheck()
    {
        //if (jumpMechanic)
        //{
        //    if (jumpMechanic._coyoteTimer > 0) jumpMechanic._coyoteTimer -= Time.fixedDeltaTime;
        //    if (jumpMechanic._justJumpedCooldown > 0) jumpMechanic._justJumpedCooldown -= Time.fixedDeltaTime;
        //}
        //groundCheck = (!jumpMechanic || jumpMechanic._justJumpedCooldown <= 0) ? Physics.SphereCast(transform.position, capCollider.radius, -transform.up, out hit, baseMovementVariables.groundCheckDistance + 0.01f) : false;
        baseMovementVariables.surfaceSlope = Vector3.Angle(hit.normal, Vector3.up);
        if (baseMovementVariables.surfaceSlope > baseMovementVariables.maxSlope)
        {
            groundCheck = false;
            if (playerState != PlayerState.Climbing && playerState != PlayerState.Jumping && playerState != PlayerState.InAir)
            {
                previousState = playerState;
                playerState = PlayerState.InAir;
                baseMovementVariables.g = baseMovementVariables.initialGravity;
            }
        }
        totalVelocityToAdd = Vector3.zero;
        newForwardandRight = Vector3.zero;

        groundedForward = Vector3.Cross(hit.normal, -transform.right);
        groundedRight = Vector3.Cross(hit.normal, transform.forward);

        //If close to a small step, raise the player to the height of the step for better feeling movement
        if (onFakeGround)
        {
            if (groundCheck) onFakeGround = false;
            else
            {
                groundCheck = true;
                groundedForward = transform.forward;
                groundedRight = transform.right;
            }
        }
        //Player just landed
        if (groundCheck && (playerState == PlayerState.Jumping || playerState == PlayerState.InAir || playerState == PlayerState.Climbing))
        {
            lastViablePosition = transform.position;
            //timeSinceGrounded = 0;
            if (playerJustLanded != null) playerJustLanded();
            rb.velocity = rb.velocity - Vector3.up * rb.velocity.y;
            float angleOfSurfaceAndVelocity = Vector3.Angle(rb.velocity, (hit.normal - Vector3.up * hit.normal.y));
            if (!onFakeGround && hit.normal.y != 1 && angleOfSurfaceAndVelocity < 5 && baseMovementVariables.z > 0)
                rb.velocity = (groundedRight * baseMovementVariables.x + groundedForward * baseMovementVariables.z).normalized * rb.velocity.magnitude;          //This is to prevent the weird glitch where the player bounces on slopes if they land on them without jumping
            baseMovementVariables.friction = baseMovementVariables.groundFriction;
            //_climbingCooldown = 0;
            previousState = playerState;
            playerState = PlayerState.Grounded;
            baseMovementVariables.g = 0;
        }
        //Player just left the ground
        if (isGrounded && !groundCheck)
        {
            if (playerState != PlayerState.Jumping)
            {
                previousState = playerState;
                playerState = PlayerState.InAir;
                SetInitialGravity();
            }
            baseMovementVariables.friction = baseMovementVariables.inAirFriction;
            if (playerLeftGround != null) playerLeftGround();
        }
        isGrounded = groundCheck;

        float maxDistance = capCollider.radius * (1 + ((isSprinting) ? (rb.velocity.magnitude / baseMovementVariables.maxSprintVelocity) : 0));
        if (playerState == PlayerState.Grounded) baseMovementVariables.feetSphereCheck = Physics.SphereCast(transform.position - Vector3.up * .5f, capCollider.radius + .01f, rb.velocity.normalized, out feetHit, maxDistance);
        if (baseMovementVariables.feetSphereCheck && !onFakeGround)
        {
            Vector3 direction = feetHit.point - (transform.position - Vector3.up * .5f);
            float dist = direction.magnitude;
            baseMovementVariables.kneesCheck = Physics.Raycast(transform.position - Vector3.up * capCollider.height * .24f, (direction - rb.velocity.y * Vector3.up), dist);
            if (!baseMovementVariables.kneesCheck && playerState == PlayerState.Grounded && (baseMovementVariables.x != 0 || baseMovementVariables.z != 0))
            {
                StartCoroutine(FakeGround());
                isGrounded = true;
            }
        }
    }
    private void Move()
    {
        currentForwardAndRight = new Vector3(rb.velocity.x, 0, rb.velocity.z);

        if (!isGrounded)
        {
            if (playerState != PlayerState.Climbing && playerState != PlayerState.Vaulting)
            {
                rb.velocity -= currentForwardAndRight * baseMovementVariables.friction;

                newForwardandRight = (transform.right * baseMovementVariables.x + transform.forward * baseMovementVariables.z);
                if (baseMovementVariables.z != 0 || baseMovementVariables.x != 0)
                {
                    rb.velocity = newForwardandRight.normalized * currentForwardAndRight.magnitude * baseMovementVariables.airControl + currentForwardAndRight * (1f - baseMovementVariables.airControl) + rb.velocity.y * Vector3.up;
                }

            }
        }
        else
        {
            newForwardandRight = (groundedRight.normalized * baseMovementVariables.x + groundedForward.normalized * baseMovementVariables.z);
            if (hit.normal.y == 1)
            {
                newForwardandRight = new Vector3(newForwardandRight.x, 0, newForwardandRight.z);
                rb.velocity = (rb.velocity - Vector3.up * rb.velocity.y).normalized * rb.velocity.magnitude;
            }

            if (rb.velocity.magnitude < baseMovementVariables.maxVelocity)
            {
                totalVelocityToAdd += newForwardandRight;
            }
            else if (playerState != PlayerState.Sliding)
            {
                if ((baseMovementVariables.z == 0 && baseMovementVariables.x == 0) || (baseMovementVariables.pvX < 0 && baseMovementVariables.x > 0)
                    || (baseMovementVariables.x < 0 && baseMovementVariables.pvX > 0) || (baseMovementVariables.pvZ < 0 && baseMovementVariables.z > 0)
                    || (baseMovementVariables.z < 0 && baseMovementVariables.pvZ > 0)) rb.velocity *= .99f; //If the palyer changes direction when going at the maxSpeed then decrease speed for smoother momentum shift
                else if (rb.velocity.magnitude < baseMovementVariables.maxVelocity + 1f) rb.velocity = newForwardandRight.normalized * baseMovementVariables.maxVelocity;
                totalVelocityToAdd = Vector3.zero;
            }

            if (rb.velocity.magnitude != baseMovementVariables.maxVelocity || (baseMovementVariables.x == 0 && baseMovementVariables.z == 0))
            {
                totalVelocityToAdd -= rb.velocity * baseMovementVariables.friction;
            }

            baseMovementVariables.pvX = baseMovementVariables.x;
            baseMovementVariables.pvZ = baseMovementVariables.z;
        }
    }
    public void SetInitialGravity() => baseMovementVariables.g = baseMovementVariables.initialGravity;
    private void ApplyGravity()
    {
        if (playerState != PlayerState.Climbing)
        {
            if (!isGrounded)
            {
                totalVelocityToAdd += Vector3.up * baseMovementVariables.g;
            }
            if (baseMovementVariables.g > baseMovementVariables.maxGravity) baseMovementVariables.g *= baseMovementVariables.gravityRate;
        }
    }
    private IEnumerator FakeGround()
    {
        onFakeGround = true;
        transform.position = new Vector3(transform.position.x, feetHit.point.y + 1f, transform.position.z);
        baseMovementVariables.g = 0;
        baseMovementVariables._fakeGroundTimer = baseMovementVariables.fakeGroundTime;
        while (baseMovementVariables._fakeGroundTimer > 0 && onFakeGround)
        {
            baseMovementVariables._fakeGroundTimer -= Time.fixedDeltaTime;
            yield return fixedUpdate;
        }
        onFakeGround = false;
    }
    public void ResetPosition()
    {
        rb.velocity = Vector3.zero;
        baseMovementVariables.g = 0;
        transform.position = lastViablePosition;

    }
}
