using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class BaseCharacter : MonoBehaviour
{
    #region Gravity
    protected float _gravityRate;
    public Vector3 gravityDirection;
    protected Vector3 gravityCenter;

    private ObjectGravity gravity = new ObjectGravity();

    #endregion
    [System.Serializable]
    public class GravityVariables : CharacterMechanicVariables
    {
        public float maxGravityVelocity = -39.2f;
        public float initialGravity = -.55f;
        public float gravityRate = 1.008f;
    }
    protected void ApplyGravity()
    {
        if (gravityCenter != Vector3.zero) SetCharacterGravityDirection(gravityCenter - transform.position);
        if (!groundCheck)
        {
            totalVelocityToAdd += (-gravityDirection) * g;
        }
        if (g > gravityMechanic.maxGravityVelocity) g *= _gravityRate;

        rb.velocity += totalVelocityToAdd;
    }
    public void SetCharacterGravityDirection(Vector3 direction) => gravityDirection = direction.normalized;
    
    public void SetGravityCenter(Vector3 point) => gravityCenter = point;
    
    public void ToggleGravity(bool isActice)
    {
        gravityMechanic.enabled = isActice;
        SetInitialGravity(0);
    }
    public void SetInitialGravity(float value) => g = value;
    public void SetGravityRate(float value) => _gravityRate = value;
}
