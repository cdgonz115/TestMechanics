using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class BaseCharacter : MonoBehaviour
{
    protected float sumOfAllAngles;
    protected float averageSlope;
    protected float surfaceSlope;
    protected int numberOfAngles;
    protected int stuckBetweenSurfacesHelper = 0;

    protected RaycastHit groundCheckHit;

    protected Vector3 currentForwardAndRightVelocity;

    protected Vector3 groundedForward;
    protected Vector3 groundedRight;

    public bool isGrounded;
    public bool groundCheck;

    [System.Serializable]
    public class GroundCheckVariables {

        [HideInInspector] public float groundCheckDistance;
        [HideInInspector] public float biggestSize = 0;
        public float maxSlope = 60;

        internal void CalculateGroundCheckDistance(SphereCollider collider, Transform transform) {
            
            if (Mathf.Abs(transform.lossyScale.x) > Mathf.Abs(transform.lossyScale.y)) biggestSize = transform.lossyScale.x;
            else biggestSize = transform.lossyScale.y;
            if (Mathf.Abs(biggestSize) < Mathf.Abs(transform.lossyScale.z)) biggestSize = transform.lossyScale.z;

            groundCheckDistance = biggestSize * collider.radius;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, characterCollider.radius - .01f);
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position + gravityDirection.normalized * 0.01f, characterCollider.radius - .01f);
    }


    protected void GroundCheck()
    {
        sumOfAllAngles = 0;
        stuckBetweenSurfacesHelper = 0;
        groundCheckHit = new RaycastHit();
        currentForwardAndRightVelocity = rb.velocity - Vector3.Project(rb.velocity, gravityDirection);

        RaycastHit[] hits = Physics.SphereCastAll(transform.position, (characterCollider.radius -.01f) * groundCheckVariables.biggestSize, gravityDirection,
           .02f * groundCheckVariables.biggestSize, collisionMask, QueryTriggerInteraction.Ignore);


        foreach (RaycastHit collision in hits)
        {
            //print(collision.point);
            if (collision.collider)
            {
                if (collision.point != Vector3.zero)
                {
                    float newSurfaceSlope = Vector3.Angle(collision.normal, -WorldGravity.singleton.GravityDirection);
                    
                    if (!groundCheckHit.collider)
                    {
                        groundCheckHit = collision;
                        surfaceSlope = newSurfaceSlope;
                        sumOfAllAngles += newSurfaceSlope;
                        numberOfAngles++;
                    }
                    else
                    {
                        if (newSurfaceSlope <= surfaceSlope)
                        {
                            groundCheckHit = collision;
                            surfaceSlope = newSurfaceSlope;
                            sumOfAllAngles += newSurfaceSlope;
                            numberOfAngles++;
                        }
                    }
                    if (newSurfaceSlope > groundCheckVariables.maxSlope) stuckBetweenSurfacesHelper++;
                }
            }
        }
        averageSlope = sumOfAllAngles / numberOfAngles * 1.0f;
        groundCheck = groundCheckHit.collider;

        if (surfaceSlope > groundCheckVariables.maxSlope)
        {
                SetInitialGravity(gravityMechanic.initialGravity);
        }

        groundedForward = Vector3.Cross(groundCheckHit.normal, -transform.right);
        groundedRight = Vector3.Cross(groundCheckHit.normal, transform.forward);

        //Character just became grounded
        if (groundCheck && !isGrounded)
        {
            //Code is missing here
            //_friction = movementVariables.groundFriction;
            CharacterLanded();
            rb.velocity = currentForwardAndRightVelocity * _friction;
            float angleOfSurfaceAndVelocity = Vector3.Angle(rb.velocity, (groundCheckHit.normal - Vector3.up * groundCheckHit.normal.y));
        }

        //Player just left the ground
        if (isGrounded && !groundCheck)
        {
            //Code is missing here
            surfaceSlope = 0;
            //_friction = movementVariables.inAirFriction;
            CharacterLeftGround();
            SetInitialGravity(gravityMechanic.initialGravity);
        }
        isGrounded = groundCheck;

    }
    void UpdateTransform () => transform.rotation = Quaternion.LookRotation(transform.forward, -gravityDirection);

    protected virtual void CharacterLanded() { }

    protected virtual void CharacterLeftGround() { }
}
