using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractablePhysicsObject : PhysicsObject
{
    float sleepDelay;
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
        if (rb.IsSleeping())
        {
            sleepDelay = 0f;
            return;
        }
        if (rb.velocity.magnitude < _minVelocity)
        {
            sleepDelay += Time.deltaTime;
            if (sleepDelay >= 1f)
            {
                SleepObject();
                return;
            }
        }
        else
        {
            sleepDelay = 0f;
        }

        if (parentVelocity != Vector3.zero) rb.velocity -= parentVelocity;
        totalVelocityToAdd = Vector3.zero;

        if (gravityMechanic.enabled) ApplyGravity();

        rb.velocity += totalVelocityToAdd;
        rb.velocity += parentVelocity;
        rb.velocity += externalVelocity;

        externalVelocity = Vector3.zero;
        beforeCollisionVelocity = rb.velocity;
    }
}