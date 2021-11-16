using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VaultingMechanic : MonoBehaviour
{
    public bool feetSphereCheck;
    public bool kneesCheck;
    public float fakeGroundTime;
    float _fakeGroundTimer;
    public float minClimbCheckDistance;
    public float maxClimbCheckDistance;
    public float minClimbSlope;

    public bool feetCheck;
    public bool headCheck;
    public bool forwardCheck;

    private Rigidbody rb;
    private CapsuleCollider capCollider;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        capCollider = GetComponent<CapsuleCollider>();
    }
    private void ClimbingChecks()
    {
        float maxDistance = capCollider.radius * (1 + ((BaseMovement.singleton.isSprinting) ? (rb.velocity.magnitude / BaseMovement.singleton.maxSprintVelocity) : 0));
        if (BaseMovement.singleton.playerState == PlayerState.Grounded) feetSphereCheck = Physics.SphereCast(transform.position - Vector3.up * .5f, capCollider.radius + .01f, rb.velocity.normalized, out BaseMovement.singleton.feetHit, maxDistance);
        headCheck = Physics.Raycast(Camera.main.transform.position + Vector3.up * .25f, transform.forward, capCollider.radius + ((slope >= minClimbSlope) ? maxClimbCheckDistance * 2 : minClimbCheckDistance));
        forwardCheck = Physics.Raycast(transform.position, transform.forward, capCollider.radius + ((slope >= minClimbSlope) ? maxClimbCheckDistance : minClimbCheckDistance));  //forwardCheck = Physics.Raycast(transform.position, transform.forward, capCollider.radius + ((slope >= 70? capCollider.radius:.1f)));
        if (forwardCheck && currentForwardAndRight.magnitude > 1)
        {
            velocityAtCollision = currentForwardAndRight;
            //if (playerState != PlayerState.Climbing) rb.velocity = Vector3.zero;              //Avoid bouncing
        }

        if (feetSphereCheck && !onFakeGround)
        {
            Vector3 direction = feetHit.point - (transform.position - Vector3.up * .5f);
            float dist = direction.magnitude;
            kneesCheck = Physics.Raycast(transform.position - Vector3.up * capCollider.height * .24f, (direction - rb.velocity.y * Vector3.up), dist);
            if (!kneesCheck && playerState == PlayerState.Grounded && (x != 0 || z != 0)) StartCoroutine(FakeGround());
        }
        kneesCheck = false;
    }
}
