using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootProjectile : MonoBehaviour
{
    public KeyCode button;
    public GameObject projecTilePrefab;
    public float projectileSpeed;
    public bool used;
    private void Start()
    {
        TestMoveThree.singleton.playerJustLanded += ResetAbility;
    }

    private void Update()
    {
        if (Input.GetKeyDown(button) & !used)
        {
            used = true;
            GameObject projectile = Instantiate(projecTilePrefab,transform.position + Vector3.up * .5f, transform.rotation);
            projectile.transform.forward = Camera.main.transform.forward;
            projectile.GetComponent<ProjectilePlatformSpawner>().direction = Camera.main.transform.forward;
            projectile.GetComponent<Rigidbody>().velocity = projectile.transform.forward * projectileSpeed;
        }
    }
    void ResetAbility()
    {
        if (TestMoveThree.singleton.hit.collider)
        {
            if (!TestMoveThree.singleton.hit.collider.gameObject.CompareTag("SpawnablePlatform")) used = false;
        }
    }

}
