using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract partial class PhysicsEntity : PhysicsObject
{
    protected WaitForFixedUpdate fixedUpdate;

    [Space(20)]
    [SerializeField]protected Collider objectCollider;
    [Space(10)]
    [Header("Collision Variables")]
    public LayerMask avoidMask;

    [Space(20)]
    [Header("Mechanics")]
    #region Mechanics
    public MovementMechanic movementMechanic = new MovementMechanic();
    public JumpMechanic jumpMechanic = new JumpMechanic();
    #endregion

    #region Primitive Variables
    protected float x, z;
    protected float pvX, pvZ;
    protected float y;
    #endregion
    protected float GetColliderRadius()
    {
        if (objectCollider is SphereCollider)
        {
            return (objectCollider as SphereCollider).radius * GetSphereBiggestSide();
        }
        else return (objectCollider as CapsuleCollider).radius * GetCapsuleBiggestSide();
    }
    protected float GetColliderHeight() 
    {
        if (objectCollider is SphereCollider)
        {
            return (objectCollider as SphereCollider).radius * GetSphereBiggestSide();
        }
        else
        {
            float biggestSide = GetCapsuleBiggestSide() * (objectCollider as CapsuleCollider).radius;
            float objectHeight = (objectCollider as CapsuleCollider).height * transform.transform.lossyScale.y * .5f;
            return ((biggestSide > objectHeight) ? biggestSide : objectHeight);
        } 
    }
    protected float GetSphereBiggestSide()
    {
        float biggest = GetCapsuleBiggestSide();
        if (biggest < transform.lossyScale.y) return transform.lossyScale.y;
        else return biggest;
    }
    protected float GetCapsuleBiggestSide() {
        if (transform.lossyScale.x < transform.lossyScale.z) return transform.lossyScale.z;
        else return transform.lossyScale.x;
    }
    protected override void RigidBodySetUp()
    {
        base.RigidBodySetUp();
        rb.constraints = RigidbodyConstraints.FreezeRotation;
    }
    protected virtual void UpdateTransform() => transform.rotation = Quaternion.LookRotation(transform.forward, -gravityDirection);
    protected virtual void RemoveVectorFromDictionary(int id)
    {
        if (groundCollisionNormals.ContainsKey(id))groundCollisionNormals.Remove(id);   
    }
}
