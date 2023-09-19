using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractablePhysicsObject : PhysicsObject
{
    protected void Awake()
    {
        RigidBodySetUp();
    }
    protected void Start()
    {
        SetInitialGravity(gravityMechanic.initialGravityVelocity);
        SetGravityRate(gravityMechanic.gravityRate);
        SetGravityDirection(WorldGravity.singleton?.GravityDirection ?? Physics.gravity);
    }
    protected void FixedUpdate()
    {
        if (parentVelocity != Vector3.zero) rb.velocity -= parentVelocity;
        totalVelocityToAdd = Vector3.zero;
        workingVelocity = rb.velocity;

        if (gravityMechanic.enabled)
        {
            ApplyGravity();
        }

        rb.velocity += totalVelocityToAdd;
        localVelocity = rb.velocity;
        rb.velocity += parentVelocity;
        rb.velocity += externalVelocity;

        externalVelocity = Vector3.zero;
        if (rb.velocity.magnitude < _minVelocity) rb.velocity = Vector3.zero;
    }
    protected void OnCollisionEnter(Collision collision)
    {
        GroundCheck(collision);
    }
    protected void OnCollisionExit(Collision collision)
    {
        GroundCheck(collision);
    }
}
