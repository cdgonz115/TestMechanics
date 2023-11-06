using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PhysicsObject : MonoBehaviour
{
    #region Components
    [Header("Components")]
    public Rigidbody rb;
    #endregion

    #region BasicVariables
    [Space(20)]
    [Header("Basic Variables")]
    [SerializeField]
    [Tooltip("The minimum velocity that the objects RigidBody can have before being rounded to zero")]
    protected float _minVelocity = 0.1f;
    #endregion

    #region Mechanics
    [Header("Gravity Variables")]
    public GravityMechanic gravityMechanic = new GravityMechanic();
    #endregion

    #region Gravity
    protected Vector3 gravityDirection;
    protected Vector3 gravityCenter;
    protected float _gravityRate;
    #endregion

    #region Internal Variables
    protected float g;
    #endregion

    #region Vectors
    protected Vector3 totalVelocityToAdd;
    protected Vector3 externalVelocity;
    protected Vector3 parentVelocity;
    protected Vector3 beforeCollisionVelocity;
    protected Vector3 afterCollisionVelocityDifference;
    #endregion

    [System.Serializable]
    public abstract class PhysicsMechanic
    {
        public bool enabled = true;
        public virtual void EnableMechanic() => enabled = true;
        public virtual void DisableMechanic() => enabled = false;
    }

    [System.Serializable]
    public class GravityMechanic : PhysicsMechanic
    {
        public float maxGravityVelocity = -39.2f;
        public float maxGravityAcceleration = -.856f;
        public float initialGravityVelocity = -.55f;
        public float gravityRate = 1.008f;

        [HideInInspector] public float fixedUpdatesForMaxAcceleration;
        public float timeForMaxAcceleration
        {
            get { return fixedUpdatesForMaxAcceleration / WorldGravity.fixedUpdatesPerSecond; }
        }

        [HideInInspector] public float exponentialVelocityConstant;
        [HideInInspector] public float exponentialVelocityScalar;

        [HideInInspector] public float aproximatedConstantAcceleration;

        public void CalculateVelocityEquationValues()
        {
            exponentialVelocityScalar = (gravityRate / Mathf.Log(gravityRate)) * initialGravityVelocity;
            exponentialVelocityConstant = initialGravityVelocity - exponentialVelocityScalar;

            float firstDivision = maxGravityAcceleration / initialGravityVelocity;
            float logOne = Mathf.Log(firstDivision);
            float logTwo = Mathf.Log(gravityRate);
            fixedUpdatesForMaxAcceleration = logOne / logTwo;

            aproximatedConstantAcceleration = maxGravityAcceleration * 50;

        }
        public float CalculateVelocityAtTime(float time)
        {
            float fixedTimeValue = Mathf.Round(time * WorldGravity.fixedUpdatesPerSecond);
            float velocity = exponentialVelocityScalar * Mathf.Pow(gravityRate, fixedTimeValue) + exponentialVelocityConstant;
            return velocity;
        }
        public float AproximateLinearAcceleration(float endTime)
        {
            if (endTime > timeForMaxAcceleration * 2) return maxGravityAcceleration;
            float fixedTimeValue = endTime * WorldGravity.fixedUpdatesPerSecond;
            float finalVelocity = CalculateVelocityAtTime(endTime);
            float linearAcceleration = (finalVelocity - initialGravityVelocity) / fixedTimeValue;
            return linearAcceleration;
        }
        public float CalculateAccelerationAtTime(float time)
        {
            float fixedTimeValue = time * WorldGravity.fixedUpdatesPerSecond;
            return initialGravityVelocity * Mathf.Pow(gravityRate, fixedTimeValue);
        }
        public Vector3 ProjectileLaunch(Vector3 initialPosition, Vector3 target, Vector3 gravityDirection)
        {
            CalculateVelocityEquationValues();

            Vector3 direction = target - (initialPosition);

            Vector3 dirX = Vector3.ProjectOnPlane(direction, -gravityDirection);
            Vector3 dirY = direction - dirX;

            float peak = dirY.magnitude * 1.1f;

            float velocityY = Mathf.Sqrt(-aproximatedConstantAcceleration * peak * 2);

            float timeUp = Mathf.Sqrt((2 * peak) / -aproximatedConstantAcceleration);

            float peakPointDelta = (dirY.magnitude - peak);

            float timeDown = Mathf.Sqrt((2 * peakPointDelta) / aproximatedConstantAcceleration);

            float totalTime = timeUp + timeDown;

            float velocityX = dirX.magnitude / totalTime;

            Vector3 final = -gravityDirection.normalized * (velocityY) + dirX.normalized * velocityX;

            return final;
        }
    }
    protected virtual void RigidBodySetUp()
    {
        rb = GetComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.useGravity = false;
    }
    public void SetParentVelocity(Vector3 direction, float magnitude)
    {
        if (direction == Vector3.zero) rb.velocity -= parentVelocity;
        parentVelocity = direction.normalized * magnitude;
        rb.velocity += parentVelocity;
    }
    public void AddVelocity(Vector3 direction, float magnitude) => externalVelocity += direction.normalized * magnitude;
    public void SetVelocity(Vector3 direction, float magnitude) => rb.velocity = direction.normalized * magnitude;
    protected void ApplyGravity()
    {
        if (gravityCenter != Vector3.zero) SetGravityDirection(gravityCenter - transform.position);

        totalVelocityToAdd += (-gravityDirection) * g;
        if (g > gravityMechanic.maxGravityAcceleration) g *= _gravityRate;
    }
    public void SetGravityDirection(Vector3 direction)
    {
        gravityDirection = direction.normalized;
        WakeUpObject();
        //groundCheck = resetGroundCheck ? false : groundCheck;
        //if (!groundCheck && isGrounded) SetInitialGravity(gravityMechanic.initialGravityVelocity);
        //isGrounded = groundCheck;
    }
    public void SetGravityCenter(Vector3 point)
    {
        gravityCenter = point;
        WakeUpObject();
        //groundCheck = resetGroundCheck;
        //isGrounded = groundCheck;
    }
    public void ToggleGravity(bool isActice)
    {
        gravityMechanic.enabled = isActice;
        WakeUpObject();
        //groundCheck = false;
        //isGrounded = groundCheck;
        SetInitialGravity(0);
    }
    public void SetInitialGravity(float value) => g = value;
    public void SetGravityRate(float value) => _gravityRate = value;
    public void StopVelocity() => rb.velocity = Vector3.zero;
    public void StopMomentum() 
    {
        StopVelocity();
        rb.angularVelocity = rb.velocity;
    }
    public void SleepObject() 
    {
        StopMomentum();
        rb.Sleep();
    }
    public void WakeUpObject() 
    {
        if (rb.IsSleeping()) rb.WakeUp();
    }
}
