using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dishonored_Launch : MonoBehaviour
{
    public GameObject indicator;
    public float maxDistance;
    public float timeToReachTarget;
    public Rigidbody rb;
    public float force;
    private Vector3 launchDestination;
    public float cooldown = 2;
    public bool onCooldown;
    private float xzFrictionCompesator;
    private float calculatedYVelocityLost = -15.25889f;
    // Start is called before the first frame update
    void Start()
    {
        indicator.transform.position = Vector3.zero;
        rb = GetComponent<Rigidbody>();
        TestMoveThree.singleton.setVariablesOnOtherScripts += SetVariablesDependentOnMovementScript;
        TestMoveThree.singleton.playerJustLanded += ResetAbility;
    }
    void SetVariablesDependentOnMovementScript()
    {
        xzFrictionCompesator = Mathf.Pow(1.0f - TestMoveThree.singleton.inAirFriction, timeToReachTarget * 50); 
    }
    void Update()
    {
        RaycastHit hit;
        if (Input.GetKey(KeyCode.Mouse1))
        {
            if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, maxDistance)) indicator.transform.position = hit.point;
            else indicator.transform.position = Vector3.zero;
        }
        if (Input.GetKeyUp(KeyCode.Mouse1) && !onCooldown)
        {
            if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, maxDistance))
            {
                launchDestination = indicator.transform.position;
                indicator.transform.position = Vector3.zero;
                TestMoveThree.singleton.externalMovementEvent += PerformLaunch;
            }
        }

    }
    public void ResetAbility() => onCooldown = false;
    public void PerformLaunch()
    {
        Vector3 direction = (launchDestination - transform.position);
        float yDistance = direction.y;
        Vector3 forceVector = direction - Vector3.up * yDistance;
        forceVector = forceVector / xzFrictionCompesator;
        forceVector.y = yDistance - calculatedYVelocityLost;

        rb.velocity = forceVector + new Vector3( rb.velocity.x, 0, rb.velocity.z);
        TestMoveThree.singleton.SetInitialGravity();
        onCooldown = true;
        TestMoveThree.singleton.externalMovementEvent -= PerformLaunch;
    }
    private void OnDestroy()
    {
        TestMoveThree.singleton.setVariablesOnOtherScripts -= SetVariablesDependentOnMovementScript;
        TestMoveThree.singleton.playerJustLanded += ResetAbility;
    }
}
