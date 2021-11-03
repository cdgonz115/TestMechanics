using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootProjectile : MonoBehaviour
{
    public KeyCode button;
    public GameObject projectile;
    public float projectileSpeed;
    public bool thrown;
    public bool used;

    private void Start()
    {
        TestMoveThree.singleton.playerJustLanded += CheckValidLanding;
    }

    private void Update()
    {
        if (Input.GetKeyDown(button))
        {
            if (!thrown)
            {
                if (!used)
                {
                    thrown = true;
                    projectile.SetActive(true);
                    projectile.transform.parent = null;
                    projectile.transform.position = transform.position + Vector3.up * .5f;
                    projectile.transform.forward = Camera.main.transform.forward;
                    ProjectilePlatformSpawner.singleton.direction = Camera.main.transform.forward;
                    projectile.GetComponent<Rigidbody>().velocity = projectile.transform.forward * projectileSpeed;
                }
            }
            else ResetAbility();
        }
    }
    void CheckValidLanding()
    {
        if (TestMoveThree.singleton.hit.collider)
        {
            if (!TestMoveThree.singleton.hit.collider.gameObject.CompareTag("PlayerPlatform"))
            {
                if (!ProjectilePlatform.singleton.gameObject.activeSelf) thrown = false;
                used = false;
            }
            else
            {
              used = true;
              ProjectilePlatform.singleton.PlayerStepedOnPlatform();
            }
        }
        else thrown = false;
    }
    void ResetAbility()
    {
        projectile.transform.parent = gameObject.transform;
        if (ProjectilePlatform.singleton.gameObject.activeSelf) ProjectilePlatform.singleton.DisablePlatform();
        if (projectile.activeSelf) ProjectilePlatformSpawner.singleton.DisableProjectile();
        CheckValidLanding();
    }

}
