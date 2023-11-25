using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractablePhysicsEntity : PhysicsEntity
{
    protected void Awake()
    {
        RigidBodySetUp();
    }
    private void Start()
    {
        SetInitialGravity(gravityMechanic.initialGravityVelocity);
        SetGravityRate(gravityMechanic.gravityRate);
        SetGravityDirection(WorldGravity.singleton?.GravityDirection ?? Physics.gravity);
        SetGroundedFriction(movementMechanic.groundFriction);
        SetInAirFriction(movementMechanic.inAirFriction);
        SetJumpTargetPosition(Vector3.negativeInfinity);
        SetTargetPosition(Vector3.negativeInfinity);
        _friction = _inAirFriction;
        _maxVelocity = movementMechanic.maxSprintVelocity;
        _inAirControl = movementMechanic.inAirControl;
        runningFakeGroundCoroutine = FakeGroundCoroutine();
        fixedUpdate = new WaitForFixedUpdate();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            Vector3 mouse = new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.nearClipPlane + 1);
            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(mouse);
            Vector3 dir = worldPosition - Camera.main.transform.position;
            RaycastHit hit;
            if (Physics.Raycast(Camera.main.transform.position, dir, out hit, 1500f)) SetTargetPosition(hit.point);
        }
        if (Input.GetKeyDown(KeyCode.M))
        {
            movementMechanic.enabled = !movementMechanic.enabled;
        }
    }
    private void FixedUpdate()
    {
        if (parentVelocity != Vector3.zero) rb.velocity -= parentVelocity;
        totalVelocityToAdd = Vector3.zero;

        GroundCheck();
        if (movementMechanic.enabled) MoveToTarget();

        if (gravityMechanic.enabled) ApplyGravity();

        rb.velocity += totalVelocityToAdd;
        rb.velocity += parentVelocity;
        rb.velocity += externalVelocity;

        externalVelocity = Vector3.zero;
        maxNumberOfInvalidSurfaces = 0;
        beforeCollisionVelocity = rb.velocity;
    }
    protected void FakeFixedUpdate()
    {
        //if (rb.IsSleeping())
        //{
        //    sleepDelay = 0f;
        //    return;
        //}
        //if (rb.velocity.magnitude < _minVelocity)
        //{
        //    sleepDelay += Time.deltaTime;
        //    if (sleepDelay >= 1f)
        //    {
        //        SleepObject();
        //        return;
        //    }
        //}
        //else
        //{
        //    sleepDelay = 0f;
        //}

        //if (parentVelocity != Vector3.zero) rb.velocity -= parentVelocity;
        //totalVelocityToAdd = Vector3.zero;

        //GroundCheck();

        //if (gravityMechanic.enabled) ApplyGravity();

        //if (isGrounded)
        //{
        //    rb.velocity -= rb.velocity * friction;
        //    rb.angularVelocity -= rb.angularVelocity * friction;
        //}

        //rb.velocity += totalVelocityToAdd;
        //rb.velocity += parentVelocity;
        //rb.velocity += externalVelocity;

        //externalVelocity = Vector3.zero;
        //beforeCollisionVelocity = rb.velocity;
    }
    protected void OnCollisionEnter(Collision collision)
    {
        CheckForGroundCollision(collision);

        afterCollisionVelocityDifference = beforeCollisionVelocity - rb.velocity;
    }
    protected void OnCollisionStay(Collision collision)
    {
        CheckForGroundCollision(collision);
    }
    protected void OnCollisionExit(Collision collision)
    {
        RemoveVectorFromDictionary(collision.collider.GetInstanceID());
    }
}
