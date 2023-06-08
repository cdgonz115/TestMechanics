using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{

    public float speed;
    public float multiplier;

    // Update is called once per frame
    void Update()
    {
        transform.position += transform.forward * speed * Time.deltaTime;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.GetComponent<PhysicsInteractableObject>())
        {
            collision.gameObject.GetComponent<PhysicsInteractableObject>().SetParentVelocity(transform.forward, speed * multiplier);
        }
    }
    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.GetComponent<PhysicsInteractableObject>())
        {
            collision.gameObject.GetComponent<PhysicsInteractableObject>().SetParentVelocity(Vector3.zero, speed * multiplier);
        }
    }
}
