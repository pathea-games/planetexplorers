using UnityEngine;
using System.Collections;
using System.Reflection;
using System;

namespace VehiclePhysics
{
	public class VehicleDemo : MonoBehaviour
	{
		public VehicleEngine engine;

		void Awake ()
		{
			Type wt = typeof(WheelCollider);
			FieldInfo[] mis = wt.GetFields(BindingFlags.NonPublic|BindingFlags.Instance);
			foreach (FieldInfo mi in mis)
			{
				Debug.Log(mi.Name);
			}
		}

		void FixedUpdate ()
		{
			Vector3 axis = Vector3.zero;
			bool handbrake = false;
			if (Input.GetKey(KeyCode.W))
			{
				axis.z += 1;
			}
			if (Input.GetKey(KeyCode.S))
			{
				axis.z += -1;
			}
			if (Input.GetKey(KeyCode.A))
			{
				axis.x += -1;
			}
			if (Input.GetKey(KeyCode.D))
			{
				axis.x += 1;
			}
			if (Input.GetKey(KeyCode.LeftShift))
			{
				axis.y += -1;
			}
			if (Input.GetKey(KeyCode.LeftControl))
			{
				axis.y += 1;
			}
			if (Input.GetKey(KeyCode.Space))
			{
				handbrake = true;
			}
			engine.Drive(axis, handbrake);
		}
	}
}