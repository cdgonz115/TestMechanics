using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

[RequireComponent(typeof(Rigidbody))]
public class PhysicsInteractableObject : MonoBehaviour
{
    #region Gravity
    public float _gravityRate;
    public Vector3 gravityDirection;
    protected Vector3 gravityCenter;
    #endregion

    #region Components
    public Rigidbody rb;
    #endregion

    #region BasicVariables
    [SerializeField] protected float minVelocity = 0.1f;
    [SerializeField] protected bool groundCheck;
    public bool isGrounded;
    #endregion

    #region Mechanics
    public GravityMechanic gravityMechanic = new GravityMechanic();
    #endregion

    #region Primitive Variables
    public float g;
    #endregion

    #region Vectors
    protected Vector3 totalVelocityToAdd;
    protected Vector3 localVelocity;
    protected Vector3 externalVelocity;
    protected Vector3 parentVelocity;
    protected Vector3 workingVelocity;
    #endregion

    public struct LaunchData
    {
        public readonly Vector3 initialVelocity;
        public readonly float timeToTarget;

        public LaunchData(Vector3 initialVelocity, float timeToTarget)
        {
            this.initialVelocity = initialVelocity;
            this.timeToTarget = timeToTarget;
        }

    }

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

        public float timeForMaxAcceleration;
        public float gravityRateInSeconds;

        public float exponentialVelocityConstant;
        public float exponentialVelocityScalar;

        public float aproximatedConstantAcceleration;
        public float aproximatedFixedTimeConstantAcceleration;
         
        public void CalculateVelocityEquationValues()
        {
            exponentialVelocityScalar = (gravityRate / Mathf.Log(gravityRate)) * initialGravityVelocity;
            exponentialVelocityConstant = initialGravityVelocity - exponentialVelocityScalar;

            gravityRateInSeconds = Mathf.Pow(gravityRate, WorldGravity.fixedUpdatesPerSecond);

            //maxGravityAcceleration = initialGravityVelocity * gravityRate ^ x
            //maxGravityAcceleration/initialGravityVelocity = gravityRate ^ x
            //Log(maxGravityAcceleration/initialGravityVelocity) = Log(gravityRate)* x
            //Log(maxGravityAcceleration/initialGravityVelocity)/Log(gravityRate) = x

            float firstDivision = maxGravityAcceleration / initialGravityVelocity;
            float logOne = Mathf.Log(firstDivision);
            float logTwo = Mathf.Log(gravityRate);
            timeForMaxAcceleration = logOne / logTwo;

            float finalVelocity = CalculateVelocityAtFixedTime(timeForMaxAcceleration);

            //print("V Fixed: "+ CalculateVelocityAtFixedTime(timeForMaxAcceleration));
            //print("V Reg: "+ CalculateVelocityAtTime(timeForMaxAcceleration));

            //1.110296

            aproximatedConstantAcceleration = maxGravityAcceleration * 50;
            aproximatedFixedTimeConstantAcceleration = aproximatedConstantAcceleration / WorldGravity.fixedUpdatesPerSecond;
             
        }
        public float CalculateVelocityAtFixedTime(float time)
        {
            float fixedTimeValue = Mathf.Round(time * WorldGravity.fixedUpdatesPerSecond);
            float velocity = exponentialVelocityScalar * Mathf.Pow(gravityRate, fixedTimeValue) + exponentialVelocityConstant;
            return velocity;
        }
        public float CalculateVelocityAtTime(float time)
        {
            float gravityRateInSeconds = Mathf.Pow(gravityRate, WorldGravity.fixedUpdatesPerSecond);
            float velocity = exponentialVelocityScalar * Mathf.Pow(gravityRateInSeconds, time) + exponentialVelocityConstant;
            return velocity;
        }
        public float CalculateFixedLinearAcceleration(float endTime)
        {
            if (endTime > timeForMaxAcceleration * 2) return maxGravityAcceleration;
            float fixedTimeValue = endTime * WorldGravity.fixedUpdatesPerSecond;
            float finalVelocity = CalculateVelocityAtFixedTime(endTime);
            float linearAcceleration = (finalVelocity - initialGravityVelocity) / fixedTimeValue;
            return linearAcceleration;
        }
        public float CalculateLinearAcceleration(float endTime)
        {
            if (endTime > timeForMaxAcceleration * 2) return maxGravityAcceleration;
            float finalVelocity = CalculateVelocityAtTime(endTime);
            float linearAcceleration = (finalVelocity - initialGravityVelocity) / endTime;
            return linearAcceleration;
        }
        public float GetAccelerationAtFixedTime(float time) {
            float fixedTimeValue = time * WorldGravity.fixedUpdatesPerSecond;
            return initialGravityVelocity * Mathf.Pow(gravityRate, fixedTimeValue);
        }
        public float GetAccelerationAtTime(float time) {
            Debug.Log(time);
            return initialGravityVelocity * Mathf.Pow(gravityRateInSeconds, time);
        }
        public LaunchData ProjectileLaunch(Vector3 initialPosition,Vector3 target, Vector3 gravityDirection)
        {
            CalculateVelocityEquationValues();

            Vector3 direction = target - (initialPosition);

            Vector3 dirX = Vector3.ProjectOnPlane(direction, -gravityDirection);
            Vector3 dirY = direction - dirX;

            float peak = dirY.magnitude * 1.1f;

            //print("average Acceleration: " + aproximatedConstantAcceleration);  

            //Debug.Log("acceleration = "+ GetAccelerationAtTime(timeForMaxAcceleration));

            float fixedTimeForMaxAcceleration = Mathf.Round(timeForMaxAcceleration * WorldGravity.fixedUpdatesPerSecond);

            float velocityOverEstimate = maxGravityAcceleration * fixedTimeForMaxAcceleration;
            //print(velocityOverEstimate);
            //print(CalculateVelocityAtFixedTime(timeForMaxAcceleration));

            float difference = velocityOverEstimate - aproximatedConstantAcceleration;

            //print(difference);

            float velocityY = Mathf.Sqrt(-aproximatedConstantAcceleration * peak * 2);

            //print(velocityY);

            //velocityY += difference;

            //print(velocityY);

            float timeUp = Mathf.Sqrt((2 * peak) / -aproximatedConstantAcceleration);

            //print("Time up: "+timeUp + " V.S. Max Time: " + timeForMaxAcceleration);

            float peakPointDelta = (dirY.magnitude - peak);

            //print(peakPointDelta);

            float timeDown = Mathf.Sqrt((2 * peakPointDelta) / aproximatedConstantAcceleration);

            float totalTime = timeUp + timeDown;

            float velocityX = dirX.magnitude / totalTime;

            //print("time Up" + timeUp);

            //print("time Down" + timeDown);

            //print("velocityUp " + velocityY);

            Vector3 final = -gravityDirection.normalized * (velocityY)
                + dirX.normalized * velocityX;
            return new LaunchData(final, timeUp + timeDown);
        }
    }
    protected void Awake()
    {
        RigidBodySetUp();
    }
    protected void Start()
    {
        SetInitialGravity(gravityMechanic.initialGravityVelocity);
        SetGravityRate(gravityMechanic.gravityRate);
        SetGravityDirection(WorldGravity.singleton?.GravityDirection??Physics.gravity);
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
        if (rb.velocity.magnitude < minVelocity) rb.velocity = Vector3.zero;
    }
    protected void OnCollisionEnter(Collision collision)
    {
        GroundCheck(collision);
    }
    protected void OnCollisionExit(Collision collision)
    {
        GroundCheck(collision);
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
    public void SetParentVelocity(Vector3 direction, float magnitude) {
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
            if (g > gravityMechanic.maxGravityAcceleration)g *= _gravityRate;
        }
        
    }
    public void SetGravityDirection(Vector3 direction, bool resetGroundCheck = false) {
        gravityDirection = direction.normalized;
        groundCheck = resetGroundCheck?false:groundCheck;
        if (!groundCheck && isGrounded) SetInitialGravity(gravityMechanic.initialGravityVelocity);
        isGrounded = groundCheck;
    } 
    public void SetGravityCenter(Vector3 point, bool resetGroundCheck = false) {
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
