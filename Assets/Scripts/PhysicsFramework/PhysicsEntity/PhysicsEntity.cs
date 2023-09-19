using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract partial class PhysicsEntity : PhysicsObject
{
    [Space(20)]
    [Header("Collision Variables")]
    public LayerMask collisionMask;

    [Header("Mechanics")]
    #region Mechanics
    public GroundCheckMechanic groundCheckMechanic = new GroundCheckMechanic();
    public MovementMechanic movementMechanic = new MovementMechanic();
    public JumpMechanic jumpMechanic = new JumpMechanic();
    #endregion

    #region Primitive Variables
    protected float x, z;
    protected float pvX, pvZ;
    protected float y;
    #endregion

    protected virtual void UpdateTransform() => transform.rotation = Quaternion.LookRotation(transform.forward, -gravityDirection);
}
