using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class PlayerController
{
    [System.Serializable]
    public class VaultVariables
    {
        public bool climbMechanic;

        #region Climbing Checks
        [HideInInspector] public bool feetSphereCheck;
        [HideInInspector] public bool kneesCheck;
        public float minClimbCheckDistance = .1f;
        public float maxClimbCheckDistance = .6f;
        public float minClimbSlope = 65;
        public float heightAboveCamera = .25f;

        [HideInInspector] public bool feetCheck;
        [HideInInspector] public bool headCheck;
        [HideInInspector] public bool forwardCheck;
        #endregion

        #region Vault
        [Header("Vault Variables")]
        public float vaultClimbStrength = 10;
        public float vaultEndStrength = 6;
        public float vaultDuration = .8f;
        #endregion
    }
    public void ClimbChecks()
    {
        vaultVariables.headCheck = false;
        vaultVariables.forwardCheck = false;
        if (playerState != PlayerState.Grounded)
        {
            if (hit.collider && surfaceSlope!=-1)
            {

                float angleInRadians = (90f - surfaceSlope) * Mathf.Deg2Rad;

                Vector3 posToPoint = hit.point - transform.position;
                Vector3 pointProjection = Vector3.Project(posToPoint, transform.forward);

                //Debug.DrawLine(transform.position, transform.position + posToPoint, Color.cyan);
                //Debug.DrawLine(transform.position, transform.position + pointProjection, Color.cyan);

                Vector3 headCheckPosition = (playerCamera.transform.position + transform.up * vaultVariables.heightAboveCamera * transform.localScale.y + pointProjection);

                Vector3 newPosition = transform.position + pointProjection;

                Vector3 newPosToHit = hit.point - newPosition;

                float maxDistance = Mathf.Abs(newPosToHit.magnitude * Mathf.Tan(angleInRadians)) + .05f;

                //Debug.DrawLine(newPosition, newPosition + newPosToHit, Color.cyan);
                //Debug.DrawLine(newPosition, newPosition + (transform.forward) * maxDistance, Color.magenta);

                vaultVariables.forwardCheck = Physics.Raycast(newPosition, transform.forward, maxDistance, ~ignores);

                //Debug.DrawLine(newPosition, newPosition + transform.forward * maxDistance, Color.green);

                maxDistance = Mathf.Abs((hit.point - headCheckPosition).magnitude * Mathf.Tan(angleInRadians)) + .05f;
                vaultVariables.headCheck = Physics.Raycast(headCheckPosition, transform.forward, maxDistance, ~ignores);

                //Debug.DrawLine(headCheckPosition, headCheckPosition + transform.forward * maxDistance, Color.green);
                //Debug.DrawLine(hit.point, (headCheckPosition + transform.forward * maxDistance), Color.black);
                //Debug.DrawLine(hit.point, headCheckPosition, Color.black);
            }
            else
            {
                vaultVariables.headCheck = Physics.Raycast(playerCamera.transform.position +
                    transform.up * vaultVariables.heightAboveCamera,
                    transform.forward, capCollider.radius + .05f, ~ignores);

                Debug.DrawLine(playerCamera.transform.position + transform.up * vaultVariables.heightAboveCamera,
                   (playerCamera.transform.position + transform.up * vaultVariables.heightAboveCamera) + transform.forward * (capCollider.radius + .05f), Color.green);

                vaultVariables.forwardCheck = Physics.Raycast(transform.position, transform.forward,
                    capCollider.radius + .05f, ~ignores);

                Debug.DrawLine(transform.position,
                   transform.position + transform.forward * (capCollider.radius + .05f), Color.green);
            }
        }
        if (vaultVariables.forwardCheck && currentForwardAndRight.magnitude > 1)
        {
            velocityAtCollision = currentForwardAndRight;
            //if (playerState != PlayerState.Climbing) rb.velocity = Vector3.zero;              //Avoid bouncing
        }
        vaultVariables.kneesCheck = false;
        if (vaultVariables.climbMechanic) HandleClimb();
        HandleVault();

    }
    public void HandleVault()
    {
        if ((playerState == PlayerState.InAir || (playerState == PlayerState.Climbing && surfaceSlope == 0)) && vaultVariables.forwardCheck && !vaultVariables.headCheck && z > 0)
        {
            previousState = playerState;
            playerState = PlayerState.Vaulting;
            StartCoroutine(VaultCoroutine());
        }

    }
    private IEnumerator VaultCoroutine()
    {
        rb.velocity = Vector3.up * vaultVariables.vaultClimbStrength;
        float height = Camera.main.transform.position.y;
        Physics.BoxCast(transform.position - transform.forward.normalized * capCollider.radius * .5f, Vector3.one * capCollider.radius, transform.forward, out forwardHit, Quaternion.identity, 1f, ~ignores);
        vaultVariables.feetCheck = Physics.Raycast(transform.position - Vector3.up * capCollider.height * .5f, transform.forward, capCollider.radius + .1f, ~ignores);
        while ((transform.position.y - capCollider.height * .5) < height && rb.velocity.y > 0)
        {
            rb.velocity += .05f * Vector3.up;
            yield return fixedUpdate;
        }
        vaultVariables.feetCheck = false;
        previousState = playerState;
        if (!isGrounded) playerState = PlayerState.InAir;
        rb.velocity = ((forwardHit.normal.magnitude == 0) ? transform.forward : -forwardHit.normal) * vaultVariables.vaultEndStrength;
    }

}
