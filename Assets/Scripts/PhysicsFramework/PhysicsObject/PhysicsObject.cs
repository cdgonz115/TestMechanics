using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PhysicsObject : MonoBehaviour
{
    #region Components
    [Header("Components Variables")]
    public Rigidbody rb;
    public SphereCollider characterCollider;
    #endregion

    #region BasicVariables
    [Space(20)]
    [SerializeField] protected float _minVelocity = 0.1f;
    protected bool groundCheck;
    protected bool isGrounded;
    #endregion

    #region Mechanics
    [Header("Gravity Variables")]
    public GravityMechanic gravityMechanic = new GravityMechanic();
    #endregion

    #region Gravity
    public Vector3 gravityDirection;
    protected Vector3 gravityCenter;
    protected float _gravityRate;
    #endregion

    #region Primitive Variables
    protected float g;
    #endregion

    #region Vectors
    protected Vector3 totalVelocityToAdd;
    protected Vector3 localVelocity;
    protected Vector3 externalVelocity;
    protected Vector3 parentVelocity;
    protected Vector3 workingVelocity;
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

        public float fixedUpdatesForMaxAcceleration;
        public float timeForMaxAcceleration
        {
            get { return fixedUpdatesForMaxAcceleration / WorldGravity.fixedUpdatesPerSecond; }
        }

        public float exponentialVelocityConstant;
        public float exponentialVelocityScalar;

        public float aproximatedConstantAcceleration;

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
    protected virtual void GroundCheck(Collision collision = null)
    {
        float smallestAngle = 181;
        foreach (ContactPoint point in collision.contacts)
        {
            float angle = Vector3.Angle(gravityDirection, point.point - transform.position);
            if (angle < smallestAngle) smallestAngle = angle;
        }
        groundCheck = (smallestAngle < 3);
        if (!groundCheck && isGrounded) SetInitialGravity(gravityMechanic.initialGravityVelocity);
        if (groundCheck && !isGrounded) SetInitialGravity(0);
        isGrounded = groundCheck;
    }
    protected virtual void RigidBodySetUp()
    {
        rb = GetComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
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
        if (!isGrounded)
        {
            totalVelocityToAdd += (-gravityDirection) * g;
            if (g > gravityMechanic.maxGravityAcceleration) g *= _gravityRate;
        }

    }
    public void SetGravityDirection(Vector3 direction, bool resetGroundCheck = false)
    {
        gravityDirection = direction.normalized;
        groundCheck = resetGroundCheck ? false : groundCheck;
        if (!groundCheck && isGrounded) SetInitialGravity(gravityMechanic.initialGravityVelocity);
        isGrounded = groundCheck;
    }
    public void SetGravityCenter(Vector3 point, bool resetGroundCheck = false)
    {
        gravityCenter = point;
        groundCheck = resetGroundCheck;
        isGrounded = groundCheck;
    }
    public void ToggleGravity(bool isActice)
    {
        gravityMechanic.enabled = isActice;
        groundCheck = false;
        isGrounded = groundCheck;
        SetInitialGravity(0);
    }
    public void SetInitialGravity(float value) => g = value;
    public void SetGravityRate(float value) => _gravityRate = value;
}
