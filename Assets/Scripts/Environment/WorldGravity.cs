using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldGravity : MonoBehaviour
{
    [SerializeField] private Vector3 _gravityDirection;
    [HideInInspector] public static float fixedUpdatesPerSecond;

    public Vector3 GravityDirection
    {
        get { return _gravityDirection; }
        set { _gravityDirection = value; }
    }

    public static WorldGravity singleton;

    private void Awake()
    {
        if (singleton == null)
            singleton = this;
        else
            Destroy(gameObject);

        fixedUpdatesPerSecond = 1f / Time.fixedDeltaTime;
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            GravityDirection = -GravityDirection;
            foreach (InteractablePhysicsObject obj in FindObjectsOfType<InteractablePhysicsObject>())
            {
                obj.SetGravityDirection(GravityDirection);
            }
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            foreach (InteractablePhysicsObject obj in FindObjectsOfType<InteractablePhysicsObject>())
            {
                obj.SetGravityDirection(GravityDirection);
            }
        }
        //    if (input.getkey(keycode.w)) transform.position -= vector3.forward * .02f;
        //    if (input.getkey(keycode.s)) transform.position += vector3.forward * .02f;

        //    if (input.getkey(keycode.d)) transform.position -= vector3.right * .02f;
        //    if (input.getkey(keycode.a)) transform.position += vector3.right * .02f;

        //    if (input.getkey(keycode.space)) transform.position += vector3.up * .02f;
        //    if (input.getkey(keycode.leftcontrol)) transform.position -= vector3.up * .02f;

        //    if (input.getkey(keycode.p)) findobjectoftype<physicsinteractableobject>().setgravitydirection(gravitydirection);
    }
}