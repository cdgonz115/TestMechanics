using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class PhysicsEntity
{
    #region GroundCheck Variables
    protected Dictionary<int, Vector3> collisionsAverage = new Dictionary<int, Vector3>();
    protected Dictionary<int, Vector3> groundCollisionNormals = new Dictionary<int, Vector3>();
    protected Vector3 averageNormal;
    protected bool groundCheck;
    public bool isGrounded;
    #endregion

    [SerializeField]protected float maxOffsetAngleFromContacts;

    public GameObject plane;

    [Space(20)]
    [Tooltip("The number of invalid surfaces needed to let the Entity in the slide around")]
    [SerializeField] protected int maxNumberOfInvalidSurfaces = 1;
    [SerializeField] protected int maxSlope = 45;
    [SerializeField] protected float stuckBetweenSurfacesVelocity = 2f;

    public int stuckBetweenSurfacesHelper = 0;

    #region Vectors
    protected Vector3 newForwardandRight;
    protected Vector3 currentForwardAndRightVelocity;
    protected Vector3 velocityGravityComponent;
    protected Vector3 newRigidBodyVelocity;
    protected Vector3 frictionToApply;
    #endregion
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
                    maxNumberOfInvalidSurfaces++;
                }
            }
        }
        if (sumOfAllContactNormals.Equals(Vector3.zero)) RemoveVectorFromDictionary(collision.collider.GetInstanceID());
        else groundCollisionNormals[collision.collider.GetInstanceID()] = sumOfAllContactNormals;
        Debug.DrawRay(transform.position + GetColliderHeight() * gravityDirection,sumOfAllContactNormals, Color.cyan);
    }
    protected virtual void GroundCheck()
    {
        velocityGravityComponent = Vector3.Project(rb.velocity, gravityDirection);
        currentForwardAndRightVelocity = rb.velocity - velocityGravityComponent;

        Vector3 sumOfAllNormals = groundCollisionNormals.Count < 1 ? gravityDirection : Vector3.zero;

        foreach (KeyValuePair<int, Vector3> keyedItem in groundCollisionNormals)
        {
            //print("it happened? " + keyedItem.Value);
            sumOfAllNormals += keyedItem.Value;
        }
        averageNormal = sumOfAllNormals.normalized;
        float normalsSlope = Vector3.Angle(sumOfAllNormals, -gravityDirection);

        plane.transform.position = transform.position + GetColliderHeight() * gravityDirection;
        plane.transform.up = sumOfAllNormals;

        groundCheck = (normalsSlope < maxSlope);

        totalVelocityToAdd = Vector3.zero;
        newForwardandRight = Vector3.zero;

        if (groundCheck && !isGrounded)
        {
            _friction = _groundedFriction;
            Vector3 velocityOnSurfacePlane = Vector3.ProjectOnPlane(rb.velocity, averageNormal).normalized * rb.velocity.magnitude;
            rb.velocity = velocityOnSurfacePlane;

            CharacterLanded();
            SetInitialGravity(0);
            //isJumping = false;
        }
        ////Character just left the ground
        if (isGrounded && !groundCheck)
        {
            //_justJumpedCooldown = jumpMechanic.justJumpedCooldown;
            //Time.timeScale = .1f;
            //_timer = timerDuration;
            //surfaceSlope = 0;
            _friction = _inAirFriction;
            CharacterLeftGround();
            SetInitialGravity(gravityMechanic.initialGravityVelocity);
        }
        isGrounded = groundCheck;
    }
    protected virtual void LegacyCheckForGroundCollision(Collision collision)
    {
        Debug.DrawLine(transform.position, transform.position + gravityDirection, Color.red);
        Vector3 sumOfAllContactDirections = Vector3.zero;
        foreach (ContactPoint contact in collision.contacts)
        {
            float angle = Vector3.Angle(contact.normal, -gravityDirection);
            Vector3 directionToContact = contact.point - transform.position;
            Debug.DrawLine(transform.position, transform.position + directionToContact, Color.cyan);

            if (angle < 90) sumOfAllContactDirections += directionToContact;
        }
        print(sumOfAllContactDirections.Equals(Vector3.zero));
        if (sumOfAllContactDirections.Equals(Vector3.zero)) RemoveVectorFromDictionary(collision.collider.GetInstanceID());
        else collisionsAverage[collision.collider.GetInstanceID()] = sumOfAllContactDirections;
    }
    protected virtual void LegacyGroundCheck()
    {
        Vector3 sumOfAllContactDirections = collisionsAverage.Count < 1 ? -gravityDirection : Vector3.zero;
        foreach (KeyValuePair<int, Vector3> keyedItem in collisionsAverage)
        {
            sumOfAllContactDirections += keyedItem.Value;
        }
        float contactsToGravityAngle = Vector3.Angle(sumOfAllContactDirections, gravityDirection);
        Debug.DrawLine(transform.position, transform.position + sumOfAllContactDirections, Color.magenta);

        groundCheck = (contactsToGravityAngle < maxOffsetAngleFromContacts);

        isGrounded = groundCheck;
    }
    protected virtual void CharacterLanded() {
        _friction = _groundedFriction;
    }
    protected virtual void CharacterLeftGround() {
        _friction = _inAirFriction;
    }
}
