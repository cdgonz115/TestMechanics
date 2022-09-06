using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RBMovingPlatform : MonoBehaviour
{
    public float speed;
    public float multiplier;
    // Start is called before the first frame update
    void FixedUpdate()
    {
        GetComponent<Rigidbody>().velocity = transform.forward * speed;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.GetComponent<PlayerController>())
        {
            collision.gameObject.GetComponent<PlayerController>().SetParentVelocity(transform.forward, speed * multiplier);
        }
    }
    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.GetComponent<PlayerController>())
        {
            collision.gameObject.GetComponent<PlayerController>().SetParentVelocity(Vector3.zero, speed * multiplier);
        }
    }
}
