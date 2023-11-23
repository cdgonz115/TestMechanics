using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ObjectGravity : MonoBehaviour
{
    #region Gravity
    protected float _gravityRate;
    public Vector3 gravityDirection;
    protected Vector3 gravityCenter;

    public float maxGravityVelocity = -39.2f;
    public float initialGravity = -.55f;
    public float gravityRate = 1.008f;

    public bool selfAutonomous;

    private float g;

    public Action applyGravity { get; set; }

    private void Start()
    {
        applyGravity = () => GravityApplied();
    }
    #endregion
    public virtual Vector3 GravityApplied()
    {
        Vector3 totalVelocityToAdd = Vector3.zero;
        if (gravityCenter != Vector3.zero) SetCharacterGravityDirection(gravityCenter - transform.position);
        //if (!groundCheck)
        //{
        //    totalVelocityToAdd += (-gravityDirection) * g;
        //}
        //if (g > gravityMechanic.maxGravityVelocity) g *= _gravityRate;

        return totalVelocityToAdd;
    }
    public void SetCharacterGravityDirection(Vector3 direction) => gravityDirection = direction.normalized;

    public void SetGravityCenter(Vector3 point) => gravityCenter = point;

    private void OnEnable()
    {
        SetInitialGravity(0);
    }
    private void OnDisable()
    {
        SetInitialGravity(0);
    }
    public void SetInitialGravity(float value) => g = value;
    public void SetGravityRate(float value) => _gravityRate = value;
   
}
