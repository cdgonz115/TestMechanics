using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class PhysicsEntity
{
    protected float _justJumpedCooldown;
    [System.Serializable]
    public class JumpMechanic : PhysicsMechanic
    {
        public float justJumpedCooldown = .1f;
    }

    protected Vector3 jumpTargetPosition;

    protected virtual void SetJumpTargetPosition(Vector3 position)
    {
        if (position == null) jumpTargetPosition = Vector3.negativeInfinity;
        else jumpTargetPosition = position;
    }

    public virtual void Jump(Vector3 targetPosition)
    {
        Vector3 result = gravityMechanic.ProjectileLaunch(transform.position,
            targetPosition + GetColliderHeight() * -gravityDirection, gravityDirection);

        rb.velocity += result;
        _justJumpedCooldown = jumpMechanic.justJumpedCooldown;
    }
}
