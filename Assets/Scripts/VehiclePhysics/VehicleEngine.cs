using UnityEngine;
using System;
using System.Collections;

namespace VehiclePhysics
{
	[RequireComponent(typeof(Rigidbody))]
	public class VehicleEngine : MonoBehaviour
	{
		public VehicleWheel[] wheels = new VehicleWheel[0] ;
		public float realMass = 10000f;
		public float maxPower = 0f;
		public float maxMotorTorque = 0f;
		public bool showInfo = false;

		private Rigidbody rigid;
		private float currPower = 0f;
		private float currMotorTorque = 0f;
		void Awake ()
		{
			rigid = GetComponent<Rigidbody>();
		}

		void Start ()
		{
			for (int i = 0; i < wheels.Length; ++i)
			{
				wheels[i].Init(this);
			}
		}

		public void Drive (Vector3 inputAxis, bool handBrake)
		{
			for (int i = 0; i < wheels.Length; ++i)
			{
				wheels[i].motorTorque = wheels[i].maxMotorTorque * inputAxis.z;
				wheels[i].brakeTorque = wheels[i].dynamicBrakeTorque;
				wheels[i].SetWheelTorques();
			}
		}

		void FixedUpdate ()
		{
			rigid.centerOfMass = Vector3.up *0.5f;
		}

		void LateUpdate ()
		{
			for (int i = 0; i < wheels.Length; ++i)
			{
				wheels[i].SyncModel();
			}
		}

		#if UNITY_EDITOR
		void OnGUI ()
		{
			if (showInfo)
			{
				Rect window_rect = new Rect(20,150,250,730);

				UnityEditor.EditorGUI.DrawRect(window_rect, new Color(0,0,0,0.7f));
				GUIStyle valuestyle = new GUIStyle(UnityEditor.EditorStyles.miniLabel);
				valuestyle.alignment = TextAnchor.UpperRight;
				float w = window_rect.width - 20;
				GUI.BeginGroup(window_rect);

				Vector3 velxz = rigid.velocity;
				velxz.y = 0;
				GUI.Label(new Rect(6, 6, w, 18), "Vehicle:", UnityEditor.EditorStyles.boldLabel);
				GUI.Label(new Rect(10, 26, w, 18), "Current speed:", UnityEditor.EditorStyles.miniLabel);
				GUI.Label(new Rect(10, 26, w, 18), (velxz.magnitude * 3.6f).ToString("#,##0.0") + " km/h", valuestyle);

				GUI.BeginGroup(new Rect(0,44,w+20,76));
				GUI.Label(new Rect(6, 6, w, 18), "Engine:", UnityEditor.EditorStyles.boldLabel);
				GUI.Label(new Rect(10, 26, w, 18), "Current power:", UnityEditor.EditorStyles.miniLabel);
				GUI.Label(new Rect(10, 26, w, 18), currPower.ToString("#,##0.00") + " (kW)", valuestyle);
				GUI.Label(new Rect(10, 44, w, 18), "Current motor torque:", UnityEditor.EditorStyles.miniLabel);
				GUI.Label(new Rect(10, 44, w, 18), currMotorTorque.ToString("#,##0.00") + " (Nm)", valuestyle);
				GUI.EndGroup();

				for (int i = 0; i < wheels.Length; ++i)
				{
					GUI.BeginGroup(new Rect(0,106 + i*98,w+20,98));
					GUI.Label(new Rect(6, 6, w, 18), "Wheel " + i.ToString() + ":", UnityEditor.EditorStyles.boldLabel);
					GUI.Label(new Rect(10, 26, w, 18), "Current power used:", UnityEditor.EditorStyles.miniLabel);
					GUI.Label(new Rect(10, 26, w, 18), (0).ToString("#,##0.00") + " (kW)", valuestyle);
					GUI.Label(new Rect(10, 44, w, 18), "Current motor torque:", UnityEditor.EditorStyles.miniLabel);
					GUI.Label(new Rect(10, 44, w, 18), wheels[i].motorTorque.ToString("#,##0.00") + " (Nm)", valuestyle);
					GUI.Label(new Rect(10, 62, w, 18), "Current brake torque:", UnityEditor.EditorStyles.miniLabel);
					GUI.Label(new Rect(10, 62, w, 18), wheels[i].brakeTorque.ToString("#,##0.00") + " (Nm)", valuestyle);
					GUI.Label(new Rect(10, 80, w, 18), "Current rev:", UnityEditor.EditorStyles.miniLabel);
					GUI.Label(new Rect(10, 80, w, 18), wheels[i].wheel.rpm.ToString("#,##0.00") + " (r/m)", valuestyle);
					GUI.EndGroup();
				}
				GUI.EndGroup();
			}
		}
		#endif
	}
}
