using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace CameraForge
{
	public class UserVarManager
	{
		public UserVarManager ()
		{
			vars = new Dictionary<string, Var>();
			poses = new Dictionary<string, Pose>();
		}

		private static Dictionary<string, Var> global_vars = new Dictionary<string, Var>();
		private static Dictionary<string, Transform> global_objs = new Dictionary<string, Transform>();
		private Dictionary<string, Var> vars;
		private Dictionary<string, Pose> poses;

		public void SetBool (string name, bool value) { SetVar(name, value); }
		public void SetInt (string name, int value) { SetVar(name, value); }
		public void SetFloat (string name, float value) { SetVar(name, value); }
		public void SetVector (string name, Vector2 value) { SetVar(name, value); }
		public void SetVector (string name, Vector3 value) { SetVar(name, value); }
		public void SetVector (string name, Vector4 value) { SetVar(name, value); }
		public void SetQuaternion (string name, Quaternion value) { SetVar(name, value); }
		public void SetColor (string name, Color value) { SetVar(name, value); }
		public void SetString (string name, string value) { SetVar(name, value); }
		
		public static void SetGlobalBool (string name, bool value) { SetGlobalVar(name, value); }
		public static void SetGlobalInt (string name, int value) { SetGlobalVar(name, value); }
		public static void SetGlobalFloat (string name, float value) { SetGlobalVar(name, value); }
		public static void SetGlobalVector (string name, Vector2 value) { SetGlobalVar(name, value); }
		public static void SetGlobalVector (string name, Vector3 value) { SetGlobalVar(name, value); }
		public static void SetGlobalVector (string name, Vector4 value) { SetGlobalVar(name, value); }
		public static void SetGlobalQuaternion (string name, Quaternion value) { SetGlobalVar(name, value); }
		public static void SetGlobalColor (string name, Color value) { SetGlobalVar(name, value); }
		public static void SetGlobalString (string name, string value) { SetGlobalVar(name, value); }

		public static void ResetAllGlobalVars ()
		{
			global_vars.Clear();
			global_objs.Clear();
		}

		public void UnsetVar (string name) { SetVar(name, Var.Null); }
		public void SetVar (string name, Var value)
		{
			if (value == null || value.isNull)
				vars.Remove(name);
			else
				vars[name] = value;
		}
		public void SetPose (string name, Pose pose)
		{
			if (pose == null)
				poses.Remove(name);
			else
				poses[name] = pose;
		}
		public void UnsetPose (string name)
		{
			poses.Remove(name);
		}
		public static void SetTransform (string name, Transform t)
		{
			global_objs[name] = t;
		}
		public static void UnsetTransform (string name)
		{
			global_objs.Remove(name);
		}
		public static void UnsetGlobalVar (string name) { SetGlobalVar(name, Var.Null); }
		public static void SetGlobalVar (string name, Var value)
		{
			if (value == null || value.isNull)
				global_vars.Remove(name);
			else
				global_vars[name] = value;
		}

		public Var GetVar (string name)
		{
			if (vars.ContainsKey(name))
				return vars[name];
			if (global_vars.ContainsKey(name))
				return global_vars[name];
			return Var.Null;
		}

		public static Var GetGlobalVar (string name)
		{
			if (global_vars.ContainsKey(name))
				return global_vars[name];
			return Var.Null;
		}

		public Pose GetPose (string name)
		{
			if (poses.ContainsKey(name))
				return poses[name];
			return Pose.Default;
		}

		public static Transform GetTransform (string name)
		{
			if (global_objs.ContainsKey(name))
				return global_objs[name];
			return null;
		}
	}
}