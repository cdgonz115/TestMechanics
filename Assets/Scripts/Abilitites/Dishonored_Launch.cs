using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dishonored_Launch : MonoBehaviour
{
    public GameObject indicator;
    public float maxDistance;
    public Rigidbody rb;
    public float force;
    private Vector3 launchDestination;
    public float cooldown = 2;
    public bool onCooldown;
    // Start is called before the first frame update
    void Start()
    {
        indicator.transform.position = Vector3.zero;
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
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
    private IEnumerator Cooldown()
    {
        yield return new WaitForSeconds(cooldown);
        onCooldown = false;
    }
    public void PerformLaunch()
    {
        //print(rb.velocity + " Before");
        Vector3 vector3 = (launchDestination - transform.position) * force;
        //print(vector3 + "Velocity being added");
        rb.velocity = vector3 + new Vector3( rb.velocity.x, 0, rb.velocity.z);
        TestMoveThree.singleton.SetInitialGravity(TestMoveThree.singleton.initialGravity);
        onCooldown = true;
        StartCoroutine(Cooldown());
        TestMoveThree.singleton.externalMovementEvent -= PerformLaunch;
    }
}
