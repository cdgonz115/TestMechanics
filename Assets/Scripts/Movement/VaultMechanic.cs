using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VaultMechanic : MonoBehaviour
{
    #region Climbing Checks
    [Space]
    public bool feetSphereCheck;
    public bool kneesCheck;
    public float minClimbCheckDistance;
    public float maxClimbCheckDistance;
    public float minClimbSlope;

    public bool feetCheck;
    public bool headCheck;
    public bool forwardCheck;
    #endregion

    #region Vault
    [Header("Vault Variables")]
    public float vaultClimbStrength = 10;
    public float vaultEndStrength = 6;
    public float vaultDuration = .8f;
    #endregion

    private Rigidbody rb;
    private CapsuleCollider capCollider;
    private WaitForFixedUpdate fixedUpdate;
    private ClimbMechanic climbMechanic;

    [HideInInspector] public Vector3 velocityAtCollision;

    [HideInInspector] public RaycastHit forwardHit;


    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        capCollider = GetComponent<CapsuleCollider>();
        fixedUpdate = new WaitForFixedUpdate();
        if (GetComponent<ClimbMechanic>()) climbMechanic = GetComponent<ClimbMechanic>();
    }
    public void ClimbChecks()
    {
        float maxDistance = capCollider.radius * (1 + ((BaseMovement.singleton.isSprinting) ? (rb.velocity.magnitude / BaseMovement.singleton.maxSprintVelocity) : 0));
        if (BaseMovement.singleton.playerState == PlayerState.Grounded) feetSphereCheck = Physics.SphereCast(transform.position - Vector3.up * .5f, capCollider.radius + .01f, rb.velocity.normalized, out BaseMovement.singleton.feetHit, maxDistance);
        headCheck = Physics.Raycast(Camera.main.transform.position + Vector3.up * .25f, transform.forward, capCollider.radius + ((BaseMovement.singleton.surfaceSlope >= minClimbSlope) ? maxClimbCheckDistance * 2 : minClimbCheckDistance));
        forwardCheck = Physics.Raycast(transform.position, transform.forward, capCollider.radius + ((BaseMovement.singleton.surfaceSlope >= minClimbSlope) ? maxClimbCheckDistance : minClimbCheckDistance));  //forwardCheck = Physics.Raycast(transform.position, transform.forward, capCollider.radius + ((slope >= 70? capCollider.radius:.1f)));
        if (forwardCheck && BaseMovement.singleton.currentForwardAndRight.magnitude > 1)
        {
            velocityAtCollision = BaseMovement.singleton.currentForwardAndRight;
            //if (playerState != PlayerState.Climbing) rb.velocity = Vector3.zero;              //Avoid bouncing
        }
        kneesCheck = false;
        if (climbMechanic) climbMechanic.HandleClimb();
    }
    public void HandleVault()
    {
        if ((BaseMovement.singleton.playerState == PlayerState.InAir || (BaseMovement.singleton.playerState == PlayerState.Climbing && BaseMovement.singleton.surfaceSlope == 0)) && forwardCheck && !headCheck && BaseMovement.singleton.z > 0)
        {
            BaseMovement.singleton.previousState = BaseMovement.singleton.playerState;
            BaseMovement.singleton.playerState = PlayerState.Vaulting;
            StartCoroutine(VaultCoroutine());
        }
        
    }
    private IEnumerator VaultCoroutine()
    {
        rb.velocity = Vector3.up * vaultClimbStrength;
        float height = Camera.main.transform.position.y;
        Physics.BoxCast(transform.position - transform.forward.normalized * capCollider.radius * .5f, Vector3.one * capCollider.radius, transform.forward, out forwardHit, Quaternion.identity, 1f);
        feetCheck = (Physics.Raycast(transform.position - Vector3.up * capCollider.height * .5f, transform.forward, capCollider.radius + .1f));
        while ((transform.position.y - capCollider.height * .5) < height && rb.velocity.y > 0)
        {
            //feetCheck = (Physics.Raycast(transform.position - Vector3.up * capCollider.height * .5f, transform.forward, capCollider.radius + .1f));
            rb.velocity += .05f * Vector3.up;
            yield return fixedUpdate;
        }
        feetCheck = false;
        BaseMovement.singleton.previousState = BaseMovement.singleton.playerState;
        if (!BaseMovement.singleton.isGrounded) BaseMovement.singleton.playerState = PlayerState.InAir;
        rb.velocity = ((forwardHit.normal.magnitude == 0) ? transform.forward : -forwardHit.normal) * vaultEndStrength;
    }
}
