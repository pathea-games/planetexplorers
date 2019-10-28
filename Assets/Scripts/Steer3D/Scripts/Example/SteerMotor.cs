using UnityEngine;
using System.Collections;
using Steer3D;

public class SteerMotor : MonoBehaviour
{
	SteerAgent agent;
	void OnSteer ()
	{
		if (agent == null)
			agent = GetComponent<SteerAgent>();
		if (agent != null)
		{
			if (agent.forward.sqrMagnitude > 0.001f)
				transform.forward = agent.forward;
			transform.position += agent.velocity * agent.maxSpeed * Time.deltaTime;
		}
	}
}
