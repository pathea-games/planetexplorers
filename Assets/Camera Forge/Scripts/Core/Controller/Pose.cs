using UnityEngine;
using System;
using System.IO;

namespace CameraForge
{
	public class Pose
	{
		public static Pose Default
		{
			get
			{
				Pose v = new Pose ();
				v.position = Vector3.zero;
				v.eulerAngles = Vector3.zero;
				v.fov = 60.0f;
				v.nearClip = 0.3f;
				v.lockCursor = false;
				v.cursorPos = new Vector2(0.5f, 0.5f);
				v.saturate = 1f;
				v.motionBlur = 0f;
				return v;
			}
		}
		public static Pose Zero
		{
			get
			{
				Pose v = new Pose ();
				v.position = Vector3.zero;
				v.eulerAngles = Vector3.zero;
				v.fov = 0f;
				v.nearClip = 0f;
				v.lockCursor = false;
				v.cursorPos = Vector2.zero;
				v.saturate = 0f;
				v.motionBlur = 0f;
				return v;
			}
		}

		public Vector3 position;

		private Vector3 _eulerAngles;
		private Quaternion _rotation;

		public Vector3 eulerAngles
		{
			get { return _eulerAngles; }
			set
			{
				if (_eulerAngles != value)
				{
					_eulerAngles = value;
					_rotation = Quaternion.Euler(_eulerAngles);
				}
			}
		}

		public Quaternion rotation
		{
			get { return _rotation; }
			set
			{
				if (_rotation != value)
				{
					_rotation = value;
					_eulerAngles = _rotation.eulerAngles;
				}
			}
		}

		public float yaw
		{
			get { return _eulerAngles.y; }
			set
			{
				_eulerAngles.y = Utils.NormalizeDEG(value);
				_rotation = Quaternion.Euler(_eulerAngles);
			}
		}
		
		public float pitch
		{
			get { return -_eulerAngles.x; }
			set
			{
				_eulerAngles.x = Utils.NormalizeDEG(-value);
				_rotation = Quaternion.Euler(_eulerAngles);
			}
		}
		
		public float roll
		{
			get { return _eulerAngles.z; }
			set
			{
				_eulerAngles.z = Utils.NormalizeDEG(value);
				_rotation = Quaternion.Euler(_eulerAngles);
			}
		}
		
		public float fov;
		public float nearClip;
		public bool lockCursor;
		public Vector2 cursorPos;

		// Image Effects
		public float saturate;
		public float motionBlur;

		public override string ToString ()
		{
			return "Pos: " + position.ToString() + "\r\n" +
				   "Rot: " + eulerAngles.ToString() + "\r\n" +
				   "Fov: " + fov.ToString() + "\r\n";
		}
	}
}
