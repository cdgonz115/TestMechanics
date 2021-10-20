using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectilePlatformSpawner : MonoBehaviour
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
            spawned.transform.position = collision.contacts[0].point + n * spawned.transform.localScale.x * .23f;
            spawned.transform.right = direction - direction.y * Vector3.up;
            spawned.transform.localEulerAngles += new Vector3(0, 0, 35);
        }
        else 
        {
            Vector3 right = Vector3.Cross(n, Vector3.up).normalized;
            float ang = Vector3.Angle(right, -direction);
            spawned.transform.position = collision.contacts[0].point + n * spawned.transform.localScale.z * .5f;
            spawned.transform.forward = n;
            float angle = 45 - Vector3.Angle(Vector3.up, direction) * .5f;

            if (angle > 5) angle = 30;
            else if (angle > -5) angle = 0;
            else angle = -30;
            angle *= ((ang > 90) ? -1 : 1);
            spawned.transform.localEulerAngles += new Vector3(0, 0, angle);
        }
        Destroy(gameObject);

    }
    IEnumerator Despawn()
    {
        yield return new WaitForSeconds(lifetime);
        Destroy(gameObject);
    }
}
