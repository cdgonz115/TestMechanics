using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootProjectile : MonoBehaviour
{
    public KeyCode button;
    public GameObject projecTilePrefab;
    public float projectileSpeed;

    private void Update()
    {
        if (Input.GetKeyDown(button))
        {
            GameObject projectile = Instantiate(projecTilePrefab,transform.position + Vector3.up * .5f, transform.rotation);
            projectile.transform.forward = Camera.main.transform.forward;
            projectile.GetComponent<ProjectilePlatform>().direction = Camera.main.transform.forward;
            projectile.GetComponent<Rigidbody>().velocity = projectile.transform.forward * projectileSpeed;
        }
    }
}
