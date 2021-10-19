using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectilePlatform : MonoBehaviour
{
    public GameObject wall;
    public Vector3 direction;
    public float lifetime;

    private void Start()
    {
        StartCoroutine(Despawn());
    }
    private void OnCollisionEnter(Collision collision)
    {
        GameObject spawned = Instantiate(wall);
        Vector3 n = collision.contacts[0].normal;

        if (n == Vector3.up)
        {
            print("huh");
            spawned.transform.position = collision.contacts[0].point + n * spawned.transform.localScale.x * .3f;
            spawned.transform.right = -direction;
            //spawned.transform.up = -direction;
            //spawned.transform.localEulerAngles = new Vector3(0, 0, 45);
        }
        else 
        {
            spawned.transform.position = collision.contacts[0].point + n * spawned.transform.localScale.z * .5f;
            spawned.transform.forward = n;
            spawned.transform.localEulerAngles = new Vector3(0, 0, Vector3.Angle(Vector3.up, direction) - 90);
        }
        Destroy(gameObject);

    }
    IEnumerator Despawn()
    {
        yield return new WaitForSeconds(lifetime);
        Destroy(gameObject);
    }
}
