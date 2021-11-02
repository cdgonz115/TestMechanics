using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectilePlatformSpawner : MonoBehaviour
{
    public GameObject wall;
    public Vector3 direction;
    public float lifetime;
    public static ProjectilePlatformSpawner singleton;
    private void Awake()
    {
        if (singleton == null)
            singleton = this;
        else
            Destroy(gameObject);
    }
    private void Start()
    {
        ProjectilePlatform.singleton.DisablePlatform();
        DisableProjectile();
    }
    private void OnCollisionEnter(Collision collision)
    {
        Vector3 n = collision.contacts[0].normal;
        wall.SetActive(true);
        wall.transform.parent = null;
        if (n == Vector3.up)
        {
            wall.transform.position = collision.contacts[0].point + n * wall.transform.localScale.x * .23f;
            wall.transform.right = direction - direction.y * Vector3.up;
            wall.transform.localEulerAngles += new Vector3(0, 0, 35);
        }
        else 
        {
            Vector3 right = Vector3.Cross(n, Vector3.up).normalized;
            float ang = Vector3.Angle(right, -direction);
            wall.transform.position = collision.contacts[0].point + n * wall.transform.localScale.z * .5f;
            wall.transform.forward = n;
            float angle = 45 - Vector3.Angle(Vector3.up, direction) * .5f;

            if (angle > 5) angle = 30;
            else if (angle > -5) angle = 0;
            else angle = -30;
            angle *= ((ang > 90) ? -1 : 1);
            wall.transform.localEulerAngles += new Vector3(0, 0, angle);
        }
        DisableProjectile();
    }
    public void DisableProjectile()
    {
        GetComponent<Rigidbody>().velocity = Vector3.zero;
        gameObject.SetActive(false);
    }
}
