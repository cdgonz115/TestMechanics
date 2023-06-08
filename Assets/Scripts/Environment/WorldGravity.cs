using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldGravity : MonoBehaviour
{
    [SerializeField] private Vector3 _gravityDirection;

    public Vector3 GravityDirection
    {
        get { return _gravityDirection; }
        set { _gravityDirection = value; }
    }

    public static WorldGravity singleton;

    private void Awake()
    {
        if (singleton == null)
            singleton = this;
        else
            Destroy(gameObject);
    }
    //private void Update()
    //{
    //    if (Input.GetKey(KeyCode.W)) transform.position -= Vector3.forward * .02f;
    //    if (Input.GetKey(KeyCode.S)) transform.position += Vector3.forward * .02f;

    //    if (Input.GetKey(KeyCode.D)) transform.position -= Vector3.right * .02f;
    //    if (Input.GetKey(KeyCode.A)) transform.position += Vector3.right * .02f;

    //    if (Input.GetKey(KeyCode.Space)) transform.position += Vector3.up * .02f;
    //    if (Input.GetKey(KeyCode.LeftControl)) transform.position -= Vector3.up * .02f;

    //    if (Input.GetKey(KeyCode.P)) FindObjectOfType<PhysicsInteractableObject>().SetGravityDirection(GravityDirection);
    //}
}