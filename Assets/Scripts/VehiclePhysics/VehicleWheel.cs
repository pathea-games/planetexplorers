using UnityEngine;
using System;

namespace VehiclePhysics
{
	[Serializable]
	public class VehicleWheel
	{
		public WheelCollider wheel;
		public Transform model;
		public float maxMotorTorque;
		public float staticBrakeTorque;
		public float dynamicBrakeTorque;
		public float footBrakeTorque;
		public float handBrakeTorque;

		VehicleEngine engine;
		Rigidbody rigid;

		[NonSerialized] public float motorTorque;
		[NonSerialized] public float brakeTorque;

		public void Init (VehicleEngine e)
		{
			engine = e;
			rigid = engine.GetComponent<Rigidbody>();
			wheel.wheelDampingRate = 0.25f;
			wheel.suspensionDistance = 0.2f;
			JointSpring js = new JointSpring ();
			js.spring = 3f * rigid.mass;
			js.damper = 2f * rigid.mass;
			js.targetPosition = 0.3f;
			wheel.suspensionSpring = js;
			wheel.center = new Vector3 (0f, (1f - wheel.suspensionSpring.targetPosition) * wheel.suspensionDistance, 0f);
			wheel.forceAppPointDistance = 0.7f;
			wheel.steerAngle = 0;
			wheel.brakeTorque = 0;
			wheel.motorTorque = 0;
			wheel.ConfigureVehicleSubsteps(1f, 20, 20);
		}

		public void SyncModel ()
		{
			Vector3 position;
			Quaternion rotation;
			wheel.GetWorldPose(out position, out rotation);
			if (position != Vector3.zero)
			{
				model.transform.position = position;
				model.transform.rotation = rotation;
			}
		}

		public void SetWheelTorques ()
		{
			wheel.motorTorque = motorTorque;
			wheel.brakeTorque = brakeTorque;
		}
	}
}
