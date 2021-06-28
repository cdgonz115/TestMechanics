using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicBoost : MonoBehaviour
{
    public float force;
    private void OnTriggerEnter(Collider other)
    {
        //other.GetComponent<TestMovement>().launched = true;
        other.GetComponent<Rigidbody>().velocity= transform.forward*force;
    }
}
