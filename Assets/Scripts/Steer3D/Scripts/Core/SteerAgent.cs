using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Steer3D
{
	public enum Dimension
	{
		TwoD = 2,
		ThreeD = 3
	}

	public partial class SteerAgent : MonoBehaviour
	{
		public Dimension dimension = Dimension.TwoD;
		public float maxSpeed = 5f;
		public float inertia = 1f;
		public MonoBehaviour motor;

		private Vector3 steerVector = Vector3.zero;
		private Vector3 force = Vector3.zero;

		private static float steerRadius = 0.2f;

		public bool manualUpdate = false;

		public Vector3 velocity
		{
			get
			{
				float len = steerVector.magnitude;
				if (len < steerRadius * 1.00001f)
					return Vector3.zero;
				Vector3 vel = steerVector / len * (len - steerRadius);
				if (vel.sqrMagnitude < 0.0000001f)
					return Vector3.zero;
				return vel;
			}
			set
			{
				float len = value.magnitude;
				if (len < 0.0001f)
					steerVector = steerVector.normalized * steerRadius;
				else
					steerVector = value.normalized * (len + steerRadius);
			}
		}

		public Vector3 forward
		{
			get { return steerVector.normalized; }
		}

		public Vector3 position
		{
			get { return transform.position; }
		}

		Vector3 _velocityToSteerVec (Vector3 vel)
		{
			float len = vel.magnitude;
			if (len < 0.0001f)
				return steerVector.normalized * steerRadius;
			else
				return vel.normalized * (len + steerRadius);
		}
		
		void ResetUpdateState ()
		{
			force = Vector3.zero;
		}

		public void AddDesiredVelocity (Vector3 desired_vel, float multiplier, float spherical = 0.5f)
		{
			Vector3 desired_sv = _velocityToSteerVec(desired_vel);
			Vector3 fn = desired_sv - steerVector;
			Vector3 desired_sv_sp = Vector3.Slerp(steerVector, desired_sv, Mathf.Clamp(1f - spherical, 0.02f, 1f));
			Vector3 fsp = desired_sv_sp - steerVector;
			float lfn = fn.magnitude;
			float lfsp = fsp.magnitude;
			if (lfn > 0.0001f && lfsp > 0.0001f)
				fsp *= lfn / lfsp;
			else
				return;
			force += fsp * multiplier * 5.0f; // 5.0 for exp
		}

		void UpdateSteer ()
		{
			inertia = Mathf.Max(0.001f, inertia);
			steerVector += (force / inertia * Time.deltaTime);
			if (dimension == Dimension.TwoD)
				steerVector.y = 0f;
			if (steerVector.magnitude < steerRadius)
				steerVector = steerVector.normalized * steerRadius;
		}

		void MotorMessage ()
		{
			if (motor != null)
				motor.SendMessage("OnSteer");
		}

		void Start ()
		{
			steerVector = transform.forward;
			if (dimension == Dimension.TwoD)
			{
				steerVector.y = 0;
				steerVector.Normalize();
				if (steerVector.magnitude < 0.001f)
					steerVector = Vector3.forward;
			}
			steerVector *= steerRadius;
		}

		void Update ()
		{
			if (!manualUpdate)
			{
				ManualUpdate();
			}
		}

		public void ManualUpdate ()
		{
			UpdateBehaviours();
			UpdateSteer();
			MotorMessage();
			ResetUpdateState();
		}


	}
}
