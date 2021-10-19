using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectilePlatform : MonoBehaviour
{
    public GameObject wall;

    private void OnCollisionEnter(Collision collision)
    {
        Instantiate(wall,transform.position, transform.rotation);
        Destroy(gameObject);
    }
}
