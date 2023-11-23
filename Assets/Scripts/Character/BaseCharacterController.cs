using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseCharacterController : BaseCharacter
{
    void Start()
    {
        SetInitialGravity(gravityMechanic.initialGravity);
        SetGravityRate(gravityMechanic.gravityRate);
        groundCheckVariables.CalculateGroundCheckDistance(characterCollider, transform);
    }

    private void FixedUpdate()
    {
        if (parentVelocity != Vector3.zero) rb.velocity -= parentVelocity;
        totalVelocityToAdd = Vector3.zero;

        GroundCheck();
        if (gravityMechanic.enabled)
        {
            ApplyGravity();
        }
        rb.velocity += parentVelocity;
        rb.velocity += externalVelocity;
        externalVelocity = Vector3.zero;
        if (rb.velocity.magnitude < minVelocity && isGrounded) rb.velocity = Vector3.zero;
        if (stuckBetweenSurfacesHelper >= 2) rb.velocity -= Vector3.Project(rb.velocity, gravityDirection);

    }
}
