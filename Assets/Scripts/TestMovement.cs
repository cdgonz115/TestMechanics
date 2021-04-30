using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestMovement : MonoBehaviour
{
    Rigidbody rb;
    float x, y, z, g;

    [Header("Speed Variables")]
    public float friction;
    public float airDecrease;
    public float speedIncrease;
    public float walkSpeed;
    public float runSpeed;
    public float speed;


    [Header("Jump Variables")]
    public float jumpStrength;
    public float airJumpStrength;
    public float jumpDecreaseRate;
    public float jumpBuffer;
    public float jumpBufferValue;
    public float inAirHorizontalSpeed;
    public float justJumpedCooldown;
    public float _justJumpedCooldown;
    public int airJumps;
    public int _airJumps;
    public bool jumpBuffering;
    public bool isJumping;
    public bool airJumpBypass;


    [Header("Grounded Variables")]
    public Transform sphereCheck;
    public float groundDistance;
    public float initialGravity;
    public float gravityRate;
    public float maxGravity;
    public bool isGrounded;
    public bool groundCheck;
    public LayerMask mask;

    [Header("Other Variables")]
    public bool launched;
    float scrollwheel;
    public Vector3 velocity;
    public Vector3 total;
    public CapsuleCollider capCollider;
    public Vector3 test1;
    public Vector3 test2;

    public float tscale;


    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        rb = GetComponent<Rigidbody>();
        _airJumps = airJumps;
        capCollider = GetComponent<CapsuleCollider>();
        groundDistance = capCollider.height * .5f - capCollider.radius;
        Time.timeScale = tscale;
    }

    private void Update()
    {
        scrollwheel = Input.GetAxis("Mouse ScrollWheel");
        if (Input.GetKeyDown(KeyCode.Space) || scrollwheel < 0)
        {
            jumpBuffering = true;
            jumpBuffer = jumpBufferValue;
            //if(isJumping)print("queued" + isGrounded);
        }
    }
    void FixedUpdate()
    {
        Move();
        Jump();
        rb.velocity += total;
        Debug.DrawLine(transform.position, transform.position + rb.velocity, Color.red);
        //Debug.DrawLine(transform.position, transform.position + test1.normalized * 5, Color.red);
        //Debug.DrawLine(transform.position, transform.position + test2.normalized * 5, Color.red);
    }
    void Move()
    {
        z = 0;
        x = 0;
        total = Vector3.zero;
        //groundCheck = (Physics.Raycast(transform.position, -Vector3.up, groundDistance * 1.05f));
        //Physics.sphere
        RaycastHit hit;
        groundCheck = Physics.SphereCast(transform.position, capCollider.radius + 0.01f, -transform.up, out hit, groundDistance);
        //groundCheck = Physics.CheckSphere(transform.position - Vector3.up * (groundDistance), capCollider.radius + 0.01f, mask);
        print(hit.normal);
        if (isGrounded && !groundCheck)
        {
            g = initialGravity;

        }
        if (groundCheck)
        {
            g = 0;
            rb.velocity -= transform.up.normalized * rb.velocity.y;
        }
        if (!isGrounded && groundCheck)
        {
            launched = false;

        }
        isGrounded = groundCheck;
        speed = (Input.GetKey(KeyCode.LeftShift)) ? runSpeed : walkSpeed;
        velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
        if (!isGrounded)
        {
            if (Input.GetKey(KeyCode.W)) z = speedIncrease * airDecrease;
            else if (Input.GetKey(KeyCode.S)) z = -speedIncrease * airDecrease;
            if (Input.GetKey(KeyCode.D)) x = speedIncrease * airDecrease;
            else if (Input.GetKey(KeyCode.A)) x = -speedIncrease * airDecrease;
            total += (transform.right.normalized * x + transform.forward.normalized * z);
            if (g > -maxGravity) g /= gravityRate;
        }
        else
        {
            test1 = Vector3.Cross(hit.normal, -transform.right);
            test2 = Vector3.Cross(hit.normal, transform.forward);
            if (velocity.magnitude < speed)
            {
                if (Input.GetKey(KeyCode.W)) z = speedIncrease;
                else if (Input.GetKey(KeyCode.S)) z = -speedIncrease;
                if (Input.GetKey(KeyCode.D)) x = speedIncrease;
                else if (Input.GetKey(KeyCode.A)) x = -speedIncrease;
                total += (transform.right.normalized * x + transform.forward.normalized * z);
                //total += (test2.normalized * x + test1.normalized * z);

            }
            total -= velocity * friction;
        }
        total += (transform.up.normalized * g);
    }
    public void ResetJumpBuffer()
    {
        jumpBuffering = false;
        jumpBuffer = 0;
    }
    void Jump()
    {
        if (isGrounded)
        {
            //print("whoops");
            isJumping = false;
            _airJumps = airJumps;
        }
        if (jumpBuffering) jumpBuffer -= Time.deltaTime;
        if (jumpBuffer < 0)
        {
            ResetJumpBuffer();
            airJumpBypass = false;
        }
        if (jumpBuffer > 0)
        {
            if (isGrounded && !airJumpBypass)
            {
                ResetJumpBuffer();
                isJumping = true;
                y = jumpStrength;
                //_justJumpedCooldown = justJumpedCooldown;
            }
            else if (_airJumps > 0)
            {
                ResetJumpBuffer();
                _airJumps--;
                isJumping = true;
                y = airJumpStrength;
                //_justJumpedCooldown = justJumpedCooldown;
                g = initialGravity;
                airJumpBypass = false;
                if (launched)
                {
                    rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
                    rb.velocity += new Vector3(total.x, 0, total.z).normalized * inAirHorizontalSpeed;
                }
                else rb.velocity = new Vector3(total.x, 0, total.z).normalized * inAirHorizontalSpeed;
            }
        }
        if (isJumping)
        {
            //_justJumpedCooldown -= Time.deltaTime;
            //print("times");
            if (y > 1f) y /= jumpDecreaseRate;
            else
            {
                y = 0;
                isJumping = false;
            }
        }
        else y = 0;
        total += (transform.up.normalized * y);
        //print(y);
        //
    }
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position - Vector3.up * (groundDistance), capCollider.radius + 0.01f);
    }
}
