using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Obi;

[RequireComponent(typeof(ObiActor))]
public class MovingSphere : MonoBehaviour
{
    public Transform relativePosition;

    [SerializeField, Range(0f, 100f)]
    float maxSpeed = 10f;

    [SerializeField, Range(0f, 100f)]
    float maxAcceleration = 10f, maxAirAcceleration = 1f;

    [SerializeField, Range(0f, 10f)]
    float jumpHeight = 2f;

    [SerializeField, Range(0, 5)]
    int maxAirJumps = 0;

    [SerializeField, Range(0, 90)]
    float maxGroundAngle = 25f;

    Rigidbody bodyD;

    Vector3 velocity, desiredVelocity;

    Vector3 contactNormal;

    bool desiredJump;

    public int groundContactCount;

    bool OnGround => groundContactCount > 0;
    public bool debugground;

    int jumpPhase;

    float minGroundDotProduct;

    //obistuff
    ObiSoftbody softbody;
    ObiActor actor;
    ObiRigidbody body;

    // Start is called before the first frame update
    void Start()
    {
        softbody = GetComponent<ObiSoftbody>();
        actor = GetComponent<ObiActor>();
        softbody.solver.OnCollision += Solver_OnCollision;
 
    }

    void OnValidate()
    {
        minGroundDotProduct = Mathf.Cos(maxGroundAngle * Mathf.Deg2Rad);
    }

    void ClearState()
    {
        groundContactCount = 0;
        contactNormal = Vector3.zero;
    }

    void Update()
	{
        debugground = OnGround;
        Vector2 playerInput;
        playerInput.x = Input.GetAxis("Horizontal");
        playerInput.y = Input.GetAxis("Vertical");
        playerInput = Vector2.ClampMagnitude(playerInput, 1f);

        desiredVelocity =
            new Vector3(playerInput.x, 0f, playerInput.y) * maxSpeed;

        desiredJump |= Input.GetButtonDown("Jump");

    }
    private void FixedUpdate()
    {
        UpdateState();
        if (desiredJump)
        {
            desiredJump = false;
            Jump();
        }
        //actor.AddForce(desiredVelocity,ForceMode.Acceleration);
        softbody.AddForce(desiredVelocity, ForceMode.Acceleration);

        ClearState();
    }

    void UpdateState()
    {
        if (OnGround)
        {
            jumpPhase = 0;
            if (groundContactCount > 1)
            {
                contactNormal.Normalize();
            }
        }
        else
        {
            contactNormal = Vector3.up;
        }
    }


    public void MoveToCenter()
    {
        float maxSpeedChange = maxAcceleration * Time.deltaTime;
        Vector3 dir = relativePosition.position - transform.position;
        softbody.AddForce(dir * maxSpeed * Time.deltaTime, ForceMode.Acceleration);
    }

    public void Jump()
    {
        Vector3 jumpDirection;
        if (OnGround)
        {
            jumpDirection = contactNormal;
        }
        else if (jumpPhase < maxAirJumps)
        {
            jumpDirection = contactNormal;
        }
        else
        {
            return;
        }
        jumpPhase += 1;
        float jumpSpeed = Mathf.Sqrt(-2f * Physics.gravity.y * jumpHeight*10);
        float alignedSpeed = Vector3.Dot(velocity, jumpDirection);
        if (alignedSpeed > 0f)
        {
            jumpSpeed = Mathf.Max(jumpSpeed - alignedSpeed, 0f);
        }
        softbody.AddForce(contactNormal * jumpSpeed *1000*Time.deltaTime, ForceMode.Impulse);
    }

    private void Solver_OnCollision(ObiSolver solver, ObiSolver.ObiCollisionEventArgs e)
    {
        var world = ObiColliderWorld.GetInstance();
        foreach (Oni.Contact contact in e.contacts)
        {
            Vector3 normal = contact.normal;
            if (normal.y >= minGroundDotProduct)
            {
                groundContactCount += 1;
                contactNormal += normal;
            }
        }
    }
}
