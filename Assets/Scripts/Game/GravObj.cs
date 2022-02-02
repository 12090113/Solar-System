using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravObj : GravityObject
{
	public Vector3 initialVelocity;
	private Rigidbody rb;
	void Awake()
	{
		rb = GetComponent<Rigidbody>();
		rb.velocity = initialVelocity;
	}

	void FixedUpdate()
	{
		// Gravity
		Vector3 gravity = NBodySimulation.CalculateAcceleration(rb.position);
		rb.AddForce(gravity, ForceMode.Acceleration);
	}
}
