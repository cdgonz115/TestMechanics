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
            //print("huh");
            spawned.transform.position = collision.contacts[0].point + n * spawned.transform.localScale.x * .3f;
            spawned.transform.right = -direction;
            //spawned.transform.up = -direction;
            //spawned.transform.localEulerAngles = new Vector3(0, 0, 45);
        }
        else 
        {
            Vector3 right = Vector3.Cross(n, Vector3.up).normalized;
            float ang = Vector3.Angle(right, -direction);
            //print(ang);
            spawned.transform.position = collision.contacts[0].point + n * spawned.transform.localScale.z * .5f;
            spawned.transform.forward = n;
            float angle = Vector3.Angle(Vector3.up, direction) - 90 * ((ang > 90) ? 1 : 2);
            //print(angle);
            spawned.transform.localEulerAngles = new Vector3(0, 0, angle) ;
        }
        Destroy(gameObject);

    }
    IEnumerator Despawn()
    {
        yield return new WaitForSeconds(lifetime);
        Destroy(gameObject);
    }
}
