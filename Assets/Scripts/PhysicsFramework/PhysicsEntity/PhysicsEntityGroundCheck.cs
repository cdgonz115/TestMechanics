using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class PhysicsEntity
{
    #region GroundCheck Variables
    protected Dictionary<int, Vector3> groundCollisionNormals = new Dictionary<int, Vector3>();
    protected Vector3 averageNormal;
    protected bool groundCheck;
    public bool isGrounded;

    [Space(20)]
    [Tooltip("The number of invalid surfaces needed to let the Entity in the slide around")]
    [SerializeField] protected int maxNumberOfInvalidSurfaces = 1;
    [SerializeField] protected int maxSlope = 45;
    [SerializeField] protected float stuckBetweenSurfacesVelocity = 2f;

    protected int stuckBetweenSurfacesHelper = 0;

    #endregion

    #region Vectors
    protected Vector3 newForwardandRight;
    protected Vector3 currentForwardAndRightVelocity;
    protected Vector3 velocityGravityComponent;
    protected Vector3 newRigidBodyVelocity;
    protected Vector3 frictionToApply;
    #endregion


    #region Fake Ground Checks
    [Header("Fake Ground Variables")]
    public float fakeGroundTime = .1f;
    protected bool onFakeGround;
    protected float _fakeGroundTimer;
    protected IEnumerator runningFakeGroundCoroutine;
    #endregion



    #region FeetCheck
    [Header("Feet Check Variable")]
    public bool feetCheck;
    protected RaycastHit feetHit;
    [SerializeField]protected float feetCheckDistance;
    #endregion

    #region KneesCheck
    [Header("Knees Check Variable")]
    public bool kneesCheck;
    protected RaycastHit kneestHit;
    [SerializeField] protected float kneesCheckDistance;

    Vector3 usedValue;
    Vector3 usedValue2;
    #endregion

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(usedValue, GetColliderRadius());

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(usedValue + usedValue2, GetColliderRadius());
    }

    protected virtual void CheckForGroundCollision(Collision collision)
    {
        Vector3 sumOfAllContactNormals = Vector3.zero;
        foreach (ContactPoint contact in collision.contacts)
        {
            float angle = Vector3.Angle(contact.normal, -gravityDirection);
            if (angle < 90) {
                sumOfAllContactNormals += contact.normal;
                Debug.DrawRay(transform.position + GetColliderHeight() * gravityDirection, contact.normal, Color.yellow);
                if (angle > maxSlope) {
                    stuckBetweenSurfacesHelper++;
                }
            }
        }
        if (sumOfAllContactNormals.Equals(Vector3.zero)) RemoveVectorFromDictionary(collision.collider.GetInstanceID());
        else groundCollisionNormals[collision.collider.GetInstanceID()] = sumOfAllContactNormals;
        Debug.DrawRay(transform.position + GetColliderHeight() * gravityDirection,sumOfAllContactNormals, Color.cyan);
    }
    protected virtual void ForwardChecks()
    {
        usedValue = currentForwardAndRightVelocity;
        if (isGrounded) {

            feetCheck = Physics.SphereCast(
                transform.position + (GetColliderHeight() - GetColliderRadius()) * gravityDirection,
                GetColliderRadius(), currentForwardAndRightVelocity, out feetHit, feetCheckDistance, collisionMask, QueryTriggerInteraction.Ignore);
        }

        feetCheck = (feetHit.collider != null);
        
        kneesCheck = false;

        if (feetCheck)
        {
            Vector3 kneesOrigin = transform.position + gravityDirection * GetColliderHeight() * .24f;
            Vector3 kneesCheckDirection = Vector3.ProjectOnPlane(feetHit.point - kneesOrigin, -gravityDirection);

            Debug.DrawLine(kneesOrigin, kneesOrigin + kneesCheckDirection, Color.green);

            usedValue = kneesOrigin;
            usedValue2 = kneesCheckDirection;

            kneesCheck = Physics.Raycast(kneesOrigin,kneesCheckDirection, kneesCheckDirection.magnitude , collisionMask,QueryTriggerInteraction.Ignore);

       
            if (!kneesCheck)
            {
                if (onFakeGround) StopCoroutine(runningFakeGroundCoroutine);
                StartCoroutine(runningFakeGroundCoroutine);
            } 
        }
    }
    protected virtual void GroundCheck()
    {
        velocityGravityComponent = Vector3.Project(rb.velocity, gravityDirection);
        currentForwardAndRightVelocity = rb.velocity - velocityGravityComponent;

        Vector3 sumOfAllNormals = groundCollisionNormals.Count < 1 ? gravityDirection : Vector3.zero;

        foreach (KeyValuePair<int, Vector3> keyedItem in groundCollisionNormals)
        {
            sumOfAllNormals += keyedItem.Value;
        }
        averageNormal = sumOfAllNormals.normalized;
        float normalsSlope = Vector3.Angle(sumOfAllNormals, -gravityDirection);

        groundCheck = (normalsSlope < maxSlope);

        totalVelocityToAdd = Vector3.zero;
        newForwardandRight = Vector3.zero;

        ////Character just got grounded
        if (groundCheck && !isGrounded)
        {
            _friction = _groundedFriction;
            Vector3 velocityOnSurfacePlane = Vector3.ProjectOnPlane(rb.velocity, averageNormal).normalized * rb.velocity.magnitude;
            rb.velocity = velocityOnSurfacePlane;

            CharacterLanded();
            SetInitialGravity(0);
        }
        ////Character just left the ground
        if (isGrounded && !groundCheck)
        {
            _friction = _inAirFriction;
            CharacterLeftGround();
            SetInitialGravity(gravityMechanic.initialGravityVelocity);
        }
        isGrounded = groundCheck || onFakeGround;
        ForwardChecks();
    }
    protected IEnumerator FakeGroundCoroutine()
    {
        print("In coroutine");
        onFakeGround = true;
        transform.position = new Vector3(transform.position.x, feetHit.point.y + GetColliderHeight() * transform.lossyScale.y, transform.position.z);

        SetInitialGravity(0);
        float _fakeGroundTimer = fakeGroundTime;
        while (_fakeGroundTimer > 0 && onFakeGround)
        {
            _fakeGroundTimer -= Time.fixedDeltaTime;
            yield return fixedUpdate;
        }
        onFakeGround = false;
    }
    protected virtual void CharacterLanded() {
        _friction = _groundedFriction;
    }
    protected virtual void CharacterLeftGround() {
        _friction = _inAirFriction;
    }

}
