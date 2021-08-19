using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dishonored_Launch : MonoBehaviour
{
    public GameObject indicator;
    public float maxDistance;
    public Rigidbody rb;
    public float force;
    public bool launched;
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
        if (Input.GetKey(KeyCode.Mouse0))
        {
            if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, maxDistance)) indicator.transform.position = hit.point;
            else indicator.transform.position = Vector3.zero;
        }
        if (Input.GetKeyUp(KeyCode.Mouse0))
        {
            if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, maxDistance))
            {
                rb.velocity += Camera.main.transform.forward * hit.distance * force;
                indicator.transform.position = Vector3.zero;
            }
        }

    }
}
