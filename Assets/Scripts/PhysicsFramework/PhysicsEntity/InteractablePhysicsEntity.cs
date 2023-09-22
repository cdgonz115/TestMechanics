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
        groundCheckMechanic.CalculateColliderRadius(characterCollider, transform);
        _friction = _inAirFriction;
        _maxVelocity = movementMechanic.maxSprintVelocity;
        _acceleration = movementMechanic.sprintingAcceleration;
        _inAirControl = movementMechanic.inAirControl;
        _justJumpedCooldown = jumpMechanic.justJumpedCooldown;
        //Time.timeScale = .1f;
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
        workingVelocity = rb.velocity;

        GroundCheck();
        if (movementMechanic.enabled) MoveToTarget();
        if (gravityMechanic.enabled) ApplyGravity();

        rb.velocity += totalVelocityToAdd;
        localVelocity = rb.velocity;
        rb.velocity += parentVelocity;
        rb.velocity += externalVelocity;

        externalVelocity = Vector3.zero;
        if (rb.velocity.magnitude < _minVelocity) rb.velocity = Vector3.zero;
    }
}
