using UnityEngine;
using System.Collections.Generic;

namespace CameraForge
{
	public static class InputModule
	{
		public static Var Axis (string axis)
		{
			if (axisFuncMap.ContainsKey(axis))
			{
				DAxisFunc func = axisFuncMap[axis];
				if (func != null)
				{
					object value = func();
					if (value is bool)
						return (bool)value;
					else if (value is int)
						return (int)value;
					else if (value is float)
						return (float)value;
					else if (value is Vector2)
						return (Vector2)value;
					else if (value is Vector3)
						return (Vector3)value;
					else if (value is Vector4)
						return (Vector4)value;
					else
						return Var.Null;
				}
			}

			if (axis == "Mouse Left Button")
			{
				return Input.GetMouseButton(0);
			}
			else if (axis == "Mouse Right Button")
			{
				return Input.GetMouseButton(1);
			}
			else if (axis == "Mouse Middle Button")
			{
				return Input.GetMouseButton(2);
			}
			else if (axis.Substring(axis.Length - 4, 4) == " Key")
			{
				string str_keycode = axis.Substring(0, axis.Length-4);
				KeyCode keycode = (KeyCode)System.Enum.Parse(typeof(KeyCode), str_keycode);
				return Input.GetKey(keycode);
			}

			return Input.GetAxis(axis);
		}

		public delegate object DAxisFunc ();
		private static Dictionary<string, DAxisFunc> axisFuncMap = new Dictionary<string, DAxisFunc>();

		public static void SetAxis (string axis, DAxisFunc func)
		{
			axis = axis.Trim();
			if (string.IsNullOrEmpty(axis))
				return;
			if (func == null)
				axisFuncMap.Remove(axis);
			axisFuncMap[axis] = func;
		}

		public static void ResetAxes ()
		{
			axisFuncMap.Clear();
		}
	}
}