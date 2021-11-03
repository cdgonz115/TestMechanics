using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DownLunge : MonoBehaviour
{
    public float lungeForce;
    public bool lungedUsed;
    public float lungeDuration;

    private void Start()
    {
        TestMoveThree.singleton.playerJustLanded += ResetAbility;
    }
    public void LungeDown(Rigidbody rb)
    {
        if (!lungedUsed) StartCoroutine(Lunge(rb));
    } 
    public void ResetAbility() => lungedUsed = false;

    private IEnumerator Lunge(Rigidbody rb)
    {
        if( rb.velocity.y > 0) rb.velocity -= Vector3.up * rb.velocity.y;
        rb.velocity += -Vector3.up * lungeForce;
        lungedUsed = true;
        yield return new WaitForSeconds(lungeDuration);
        rb.velocity = new Vector3(rb.velocity.x, 0 , rb.velocity.z);
        TestMoveThree.singleton.SetInitialGravity();
    }
}
