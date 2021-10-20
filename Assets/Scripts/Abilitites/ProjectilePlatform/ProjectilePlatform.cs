using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectilePlatform : MonoBehaviour
{
    public float despawnTimer;
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Player") StartCoroutine(Despawn());
    }

    IEnumerator Despawn()
    {
        yield return new WaitForSeconds(despawnTimer);
        Destroy(gameObject);
    }
}
