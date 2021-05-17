﻿using UnityEngine;
using Obi;

public class SlimeMovement : MonoBehaviour
{

	[SerializeField]
	Transform playerInputSpace = default;

	[SerializeField, Range(0f, 100f)]
	float maxSpeed = 10f;

	[SerializeField, Range(0f, 100f)]
	float maxAcceleration = 10f, maxAirAcceleration = 1f;

	[SerializeField, Range(0f, 10f)]
	float jumpHeight = 2f;

	[SerializeField, Range(0, 5)]
	int maxAirJumps = 0;

	[SerializeField, Range(0, 90)]
	float maxGroundAngle = 25f, maxStairsAngle = 50f;

	[SerializeField, Range(0f, 100f)]
	float maxSnapSpeed = 100f;

	[SerializeField, Min(0f)]
	float probeDistance = 1f;

	[SerializeField]
	LayerMask probeMask = -1, stairsMask = -1;

	Rigidbody bodyDefault, connectedBody, previousConnectedBody;

	Vector3 velocity, desiredVelocity, connectionVelocity;

	Vector3 connectionWorldPosition, connectionLocalPosition;

	Vector3 upAxis, rightAxis, forwardAxis;

	bool desiredJump;

	Vector3 contactNormal, steepNormal;

	public int groundContactCount, steepContactCount;

	bool OnGround => groundContactCount > 0;

	bool OnSteep => steepContactCount > 0;

	int jumpPhase;

	float minGroundDotProduct, minStairsDotProduct;

	int stepsSinceLastGrounded, stepsSinceLastJump;

	//obistuff
	public Transform relativePosition;
	ObiSoftbody softbody;
	ObiActor actor;
	ObiRigidbody body;
	public ObiSolver solver;

	void OnValidate()
	{
		minGroundDotProduct = Mathf.Cos(maxGroundAngle * Mathf.Deg2Rad);
		minStairsDotProduct = Mathf.Cos(maxStairsAngle * Mathf.Deg2Rad);
	}

	void Awake()
	{
		bodyDefault = GetComponent<Rigidbody>();
		body = GetComponent<ObiRigidbody>();
		OnValidate();
	}
	private void Start()
	{
		softbody = GetComponent<ObiSoftbody>();
		actor = GetComponent<ObiActor>();
		softbody.solver.OnCollision += Solver_OnCollision;
	}

	private void OnDestroy()
	{
		softbody.solver.OnCollision -= Solver_OnCollision;
	}

	private void Solver_OnCollision(ObiSolver solver, ObiSolver.ObiCollisionEventArgs e)
	{
		var world = ObiColliderWorld.GetInstance();
		foreach (Oni.Contact contact in e.contacts)
		{
			if (contact.distance<0.01)
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

	void Update()
	{
		Vector2 playerInput;
		playerInput.x = Input.GetAxis("Horizontal");
		playerInput.y = Input.GetAxis("Vertical");
		playerInput = Vector2.ClampMagnitude(playerInput, 1f);

		if (playerInputSpace)
		{
			rightAxis = ProjectDirectionOnPlane(playerInputSpace.right, upAxis);
			forwardAxis =
				ProjectDirectionOnPlane(playerInputSpace.forward, upAxis);
		}
		else
		{
			rightAxis = ProjectDirectionOnPlane(Vector3.right, upAxis);
			forwardAxis = ProjectDirectionOnPlane(Vector3.forward, upAxis);
		}
		desiredVelocity =
			new Vector3(playerInput.x, 0f, playerInput.y) * maxSpeed;

		desiredJump |= Input.GetButtonDown("Jump");
	}

	void FixedUpdate()
	{
		Vector3 gravity;
		gravity = new Vector3(0, -9.8f, 0);
		UpdateState();
		AdjustVelocity();
		if (OnGround)
		{
			gravity = Vector3.zero;
		}
		if (desiredJump)
		{
			desiredJump = false;
			Jump(gravity);
		}

		//velocity += gravity;
		Vector3 velocityPlane = new Vector3(velocity.x,0,velocity.z);
		Vector3 velocityVertical = new Vector3(0, velocity.y, 0);
		//float y = velocity.y
		//body.UpdateVelocities(velocity,Vector3.zero);
		//bodyDefault.velocity = velocity;
		//print(velocity);
		softbody.AddForce(velocity, ForceMode.VelocityChange);
		softbody.AddForce(gravity, ForceMode.Acceleration);
		//print(velocity + " velocity");
		//print(actor.solver.velocities[0]);
		ApplyVelocitys();
		//print(softbody.solver.velocities[1]);
		ClearState();
	}

	void ApplyVelocitys()
	{
		float speedclamp = 10;
		velocity.y = 0;
		for (int i = 0; i < actor.solverIndices.Length; ++i)
		{
			int solverIndex = actor.solverIndices[i];
			if (OnGround)
			{			
				actor.solver.velocities[solverIndex] = new Vector4(Mathf.Clamp(actor.solver.velocities[solverIndex].x, -speedclamp, speedclamp),
																	actor.solver.velocities[solverIndex].y,
																	Mathf.Clamp(actor.solver.velocities[solverIndex].z, -speedclamp, speedclamp),
																	actor.solver.velocities[solverIndex].w);
			}
			else
			{
				actor.solver.velocities[solverIndex] = new Vector4(Mathf.Clamp(actor.solver.velocities[solverIndex].x, -speedclamp/2, speedclamp/2),
													actor.solver.velocities[solverIndex].y,
													Mathf.Clamp(actor.solver.velocities[solverIndex].z, -speedclamp/2, speedclamp/2),
													actor.solver.velocities[solverIndex].w);
			}

		}
	}


	void ClearState()
	{
		groundContactCount = steepContactCount = 0;
		contactNormal = steepNormal = connectionVelocity = Vector3.zero;
		previousConnectedBody = connectedBody;
		connectedBody = null;
		velocity = Vector3.zero;
	}

	void UpdateState()
	{
		stepsSinceLastGrounded += 1;
		stepsSinceLastJump += 1;
		velocity = bodyDefault.velocity;
		if (OnGround || SnapToGround() || CheckSteepContacts())
		{
			stepsSinceLastGrounded = 0;
			if (stepsSinceLastJump > 1)
			{
				jumpPhase = 0;
			}
			if (groundContactCount > 1)
			{
				contactNormal.Normalize();
			}
		}
		else
		{
			contactNormal = upAxis;
		}

		if (connectedBody)
		{
			if (connectedBody.isKinematic || connectedBody.mass >= bodyDefault.mass)
			{
				UpdateConnectionState();
			}
		}
	}

	void UpdateConnectionState()
	{
		if (connectedBody == previousConnectedBody)
		{
			Vector3 connectionMovement =
				connectedBody.transform.TransformPoint(connectionLocalPosition) -
				connectionWorldPosition;
			connectionVelocity = connectionMovement / Time.deltaTime;
		}
		connectionWorldPosition = relativePosition.position;
		connectionLocalPosition = connectedBody.transform.InverseTransformPoint(
			connectionWorldPosition
		);
	}

	bool SnapToGround()
	{
		if (stepsSinceLastGrounded > 1 || stepsSinceLastJump <= 2)
		{
			return false;
		}
		float speed = velocity.magnitude;
		if (speed > maxSnapSpeed)
		{
			return false;
		}
		if (!Physics.Raycast(
			relativePosition.position, -upAxis, out RaycastHit hit,
			probeDistance, probeMask
		))
		{
			return false;
		}

		float upDot = Vector3.Dot(upAxis, hit.normal);
		if (upDot < GetMinDot(hit.collider.gameObject.layer))
		{
			return false;
		}

		groundContactCount = 1;
		contactNormal = hit.normal;
		float dot = Vector3.Dot(velocity, hit.normal);
		contactNormal = (contactNormal + new Vector3(0,1,0)).normalized;
		if (dot > 0f)
		{
			velocity = (velocity - contactNormal * dot).normalized * speed; 
		}
		connectedBody = hit.rigidbody;
		return true;
	}

	bool CheckSteepContacts()
	{
		if (steepContactCount > 1)
		{
			steepNormal.Normalize();
			float upDot = Vector3.Dot(upAxis, steepNormal);
			if (upDot >= minGroundDotProduct)
			{
				steepContactCount = 0;
				groundContactCount = 1;
				contactNormal = steepNormal;
				return true;
			}
		}
		return false;
	}

	void AdjustVelocity()
	{
		Vector3 xAxis = ProjectDirectionOnPlane(rightAxis, contactNormal);
		Vector3 zAxis = ProjectDirectionOnPlane(forwardAxis, contactNormal);

		Vector3 relativeVelocity = velocity - connectionVelocity;
		float currentX = Vector3.Dot(relativeVelocity, xAxis);
		float currentZ = Vector3.Dot(relativeVelocity, zAxis);

		float acceleration = OnGround ? maxAcceleration : maxAirAcceleration;
		float maxSpeedChange = acceleration * Time.deltaTime;

		float newX =
			Mathf.MoveTowards(currentX, desiredVelocity.x, maxSpeedChange);
		float newZ =
			Mathf.MoveTowards(currentZ, desiredVelocity.z, maxSpeedChange);

		velocity += xAxis * (newX - currentX) + zAxis * (newZ - currentZ);
	}

	void Jump(Vector3 gravity)
	{
		
		Vector3 jumpDirection;
		if (OnGround)
		{
			jumpDirection = contactNormal;
		}
		else if (OnSteep)
		{
			jumpDirection = steepNormal;
			jumpPhase = 0;
		}
		else if (maxAirJumps > 0 && jumpPhase <= maxAirJumps)
		{
			if (jumpPhase == 0)
			{
				jumpPhase = 1;
			}
			jumpDirection = contactNormal;
		}
		else
		{
			return;
		}

		stepsSinceLastJump = 0;
		jumpPhase += 1;
		gravity = new Vector3(0, -9.8f, 0);
		float jumpSpeed = Mathf.Sqrt(2f * gravity.magnitude * jumpHeight);
		jumpDirection = (jumpDirection + upAxis).normalized;
		jumpDirection += upAxis;
		float alignedSpeed = Vector3.Dot(velocity, jumpDirection);
		if (alignedSpeed > 0f)
		{
			jumpSpeed = Mathf.Max(jumpSpeed - alignedSpeed, 0f);

		}
		softbody.AddForce(jumpDirection * jumpSpeed , ForceMode.VelocityChange);
		//velocity += jumpDirection * jumpSpeed;
	}

	void OnCollisionEnter(Collision collision)
	{
		EvaluateCollision(collision);
	}

	void OnCollisionStay(Collision collision)
	{
		EvaluateCollision(collision);
	}

	void EvaluateCollision(Collision collision)
	{
		float minDot = GetMinDot(collision.gameObject.layer);
		for (int i = 0; i < collision.contactCount; i++)
		{
			Vector3 normal = collision.GetContact(i).normal;
			float upDot = Vector3.Dot(upAxis, normal);
			if (upDot >= minDot)
			{
				groundContactCount += 1;
				contactNormal += normal;
				connectedBody = collision.rigidbody;
			}
			else if (upDot > -0.01f)
			{
				steepContactCount += 1;
				steepNormal += normal;
				if (groundContactCount == 0)
				{
					connectedBody = collision.rigidbody;
				}
			}
		}
	}

	Vector3 ProjectDirectionOnPlane(Vector3 direction, Vector3 normal)
	{
		return (direction - normal * Vector3.Dot(direction, normal)).normalized;
	}

	float GetMinDot(int layer)
	{
		return (stairsMask & (1 << layer)) == 0 ?
			minGroundDotProduct : minStairsDotProduct;
	}
}
