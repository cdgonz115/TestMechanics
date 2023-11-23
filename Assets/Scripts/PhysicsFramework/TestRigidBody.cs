using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestRigidBody : MonoBehaviour
{
    private void FixedUpdate()
    {
        foreach (InteractablePhysicsObject obj in FindObjectsOfType<InteractablePhysicsObject>())
        {
            obj.SetGravityCenter(transform.position);
        }
    }
}
