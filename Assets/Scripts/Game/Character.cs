using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class Character : PhysicsInteractableObject
{
    #region Components
    public GroundCheckMechanic groundCheckMechanic = new GroundCheckMechanic();
    public MovementMechanic movementMechanic = new MovementMechanic();
    public JumpMechanic jumpMechanic = new JumpMechanic();
    public SphereCollider characterCollider;
    #endregion

    #region Primitive Variables
    protected float x, z;
    protected float pvX, pvZ;
    protected float y;
    #endregion
    public LayerMask collisionMask;

    bool isJumping;

    float _timer = 0;
    public float timerDuration = 1;
    private new void Start()
    {
        base.Start();
        groundCheckMechanic.CalculateGroundCheckDistance(characterCollider, transform);
        SetTargetPosition(Vector3.negativeInfinity);
        SetGroundedFriction(movementMechanic.groundFriction);
        SetInAirFriction(movementMechanic.inAirFriction);
        SetJumpTargetPosition(new Vector3(-119.5f, 56, -494.7f));
        _friction = _inAirFriction;
        _maxVelocity = movementMechanic.maxSprintVelocity;
        _acceleration = movementMechanic.sprintingAcceleration;
        _inAirControl = movementMechanic.inAirControl;
        _justJumpedCooldown = jumpMechanic.justJumpedCooldown;
    }

    private new void OnCollisionEnter(Collision collision)
    {
    }
    private new void OnCollisionExit(Collision collision)
    {
        
    }
    // private void Update()
    // {
    //     //DrawPath();

    //     jumpStartPosition = (!isJumping) ? transform.position : jumpStartPosition;

    //     if (Input.GetKeyDown(KeyCode.E))
    //     {
    //         if (Time.timeScale == 1) Time.timeScale = .1f;
    //         else Time.timeScale = 1;
    //     }
    //     if (Input.GetKeyDown(KeyCode.Mouse0))
    //     {
    //         Vector3 mouse = new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.nearClipPlane+1);
    //         Vector3 worldPosition = Camera.main.ScreenToWorldPoint(mouse);
    //         Vector3 dir = worldPosition - Camera.main.transform.position;
    //         RaycastHit hit;
    //         if (Physics.Raycast(Camera.main.transform.position, dir, out hit, 1500f)) SetJumpTargetPosition(hit.point);
    //     }
    //     if (Input.GetKeyDown(KeyCode.M))
    //     {
    //         movementMechanic.enabled = !movementMechanic.enabled;
    //     }
    //     if (Input.GetKeyDown(KeyCode.J))
    //     {
    //         isJumping = true;
    //         if (jumpMechanic.enabled) Jump(jumpTargetPosition);
    //     }
    //     _timer -= Time.deltaTime;
    //     if (_timer <= 0) { Time.timeScale = 0; _timer = 1000; }
    // }

    private new void FixedUpdate()
    {
        if (parentVelocity != Vector3.zero) rb.velocity -= parentVelocity;
        totalVelocityToAdd = Vector3.zero;
        workingVelocity = rb.velocity;

        GroundCheck();
        if (movementMechanic.enabled) Move();
        if (gravityMechanic.enabled) ApplyGravity();

        rb.velocity += totalVelocityToAdd;
        localVelocity = rb.velocity;
        rb.velocity += parentVelocity;
        rb.velocity += externalVelocity;

        externalVelocity = Vector3.zero;
        if (rb.velocity.magnitude < minVelocity) rb.velocity = Vector3.zero;
    }
}
