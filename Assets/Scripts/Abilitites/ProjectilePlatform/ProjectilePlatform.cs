using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectilePlatform : MonoBehaviour
{
    public float despawnTimer;
    public GameObject parent;
    public static ProjectilePlatform singleton;
    private void Awake()
    {
        if (singleton == null)
            singleton = this;
        else
            Destroy(gameObject);
    }
    private void OnEnable()
    {
        parent = transform.parent.gameObject;
    }

    public void PlayerStepedOnPlatform() => StartCoroutine(Disable());
    IEnumerator Disable()
    {
        yield return new WaitForSeconds(despawnTimer);
        DisablePlatform();
    }
    public void DisablePlatform()
    {
        transform.parent = parent.transform;
        gameObject.SetActive(false);
    }
}
