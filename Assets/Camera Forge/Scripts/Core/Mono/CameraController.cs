using UnityEngine;
using System.Collections.Generic;

namespace CameraForge
{
	[RequireComponent(typeof(Camera))]
	[AddComponentMenu("Camera Forge/Camera Controller")]
	public class CameraController : MonoBehaviour
	{
		[Header(">> Controllers")]
		[SerializeField] ControllerAsset startControllerAsset;
		[SerializeField] ControllerAsset updateControllerAsset;
		[SerializeField] ControllerAsset[] userControllerAsset = new ControllerAsset[0];

		public Controller start_controller;
		public Controller update_controller;
		public Controller[] user_controllers;

		private Camera _camera;
		public new Camera camera { get { return _camera; } }
		private Pose _pose;
		public Pose pose
		{
			get { return _pose; }
		}

		[Header(">> Settings")]
		public bool manualUpdate = false;
		public bool manualCursor = false;

#if UNITY_EDITOR

		[Header(">> Debug Params")]

		public bool debugStartController = false;
		public bool debugUpdateController = false;
		public bool debugUserController = false;
		public int debugUserControllerIndex = 0;
		public static Controller debugController = null;
#endif
		bool ignoreLockCursor = false;

		public delegate void DEventNotify (CameraController camc);
		public static event DEventNotify OnControllerCreate;
		public static event DEventNotify OnControllerDestroy;
		public static event DEventNotify BeforeControllerPlay;
		public static event DEventNotify AfterControllerPlay;

		void Awake ()
		{
			_camera = GetComponent<Camera>();
			uservars = new UserVarManager ();
			history = new Pose[maxHistoryPoses];

			if (startControllerAsset != null)
			{
				startControllerAsset.Load();
				start_controller = startControllerAsset.controller;
				start_controller.executor = this;
			}

			if (updateControllerAsset != null)
			{
				updateControllerAsset.Load();
				update_controller = updateControllerAsset.controller;
				update_controller.executor = this;
			}

			user_controllers = new Controller[userControllerAsset.Length] ;
			for (int i = 0; i < userControllerAsset.Length; ++i)
			{
				userControllerAsset[i].Load();
				user_controllers[i] = userControllerAsset[i].controller;
				user_controllers[i].executor = this;
			}

			if (OnControllerCreate != null)
				OnControllerCreate(this);
		}

		void OnDestroy ()
		{
			if (OnControllerDestroy != null)
				OnControllerDestroy(this);
		}

		void Start ()
		{
			if (start_controller != null)
			{
				PlayController(start_controller);
			}
		}

		void LateUpdate ()
		{
			if (update_controller != null && !manualUpdate)
			{
				PlayController(update_controller);
 			}

#if UNITY_EDITOR
			if (debugStartController)
			{
				debugStartController = false;
				debugController = start_controller;
			}
			if (debugUpdateController)
			{
				debugUpdateController = false;
				debugController = update_controller;
			}
			if (debugUserController)
			{
				debugUserController = false;
				if (debugUserControllerIndex >= 0 && 
				    debugUserControllerIndex < user_controllers.Length)
					debugController = user_controllers[debugUserControllerIndex];
			}
			if (Input.GetKeyDown(KeyCode.Keypad9))
			{
				ignoreLockCursor = !ignoreLockCursor;
			}
#endif
		}

		public void ManualUpdate ()
		{
			PlayController(update_controller);
		}

		public void RecordHistory ()
		{
			Pose p = pose;
			p.position = _camera.transform.position;
			p.rotation = _camera.transform.rotation;
			p.fov = _camera.fieldOfView;
			p.nearClip = _camera.nearClipPlane;

			RecordHistory(p);
		}

		float _last_playtime = -1;
		void PlayController (Controller c)
		{
			if (BeforeControllerPlay != null)
				BeforeControllerPlay(this);
			Pose p = c.final.Calculate();
			_camera.transform.position = p.position;
			_camera.transform.rotation = p.rotation;
			_camera.fieldOfView = p.fov;
			_camera.nearClipPlane = p.nearClip;
			_pose = p;
			if (ignoreLockCursor)
			{
				Cursor.lockState = Screen.fullScreen? CursorLockMode.Confined: CursorLockMode.None;
				Cursor.visible = true;
			}
			else
			{
				if (!manualCursor)
				{
					Cursor.lockState = _pose.lockCursor ? CursorLockMode.Locked : Screen.fullScreen? CursorLockMode.Confined: CursorLockMode.None;
					Cursor.visible = _pose.lockCursor ? false : true;
				}
			}
			RecordHistory(p);
			float dt = 0;
			if (_last_playtime >= 0)
			{
				dt = Time.time - _last_playtime;
				_last_playtime = Time.time;
			}
			c.Tick(dt);
			if (AfterControllerPlay != null)
				AfterControllerPlay(this);
		}

		public void PlayUserController (int index)
		{
			if (index >= 0 && index < user_controllers.Length)
			{
				if (user_controllers[index] != null)
					PlayController(user_controllers[index]);
			}
		}

		public UserVarManager uservars;
		public Pose[] history;
		private int history_count = 0;
		public const int maxHistoryPoses = 128;
		void RecordHistory (Pose p)
		{
			for (int i = maxHistoryPoses - 1; i > 0; --i)
			{
				history[i] = history[i - 1];
			}
			history[0] = p;
			history_count++;
			if (history_count > maxHistoryPoses)
				history_count = maxHistoryPoses;
		}
		public Pose GetHistoryPose (int index)
		{
			if (index < 0)
				index = 0;
			if (index > history_count - 1)
				index = history_count - 1;

			Pose p = null;
			if (history != null && history_count != 0)
			{
				p = history[index];
				if (p != null)
					return p;
			}
			p = Pose.Default;
			p.position = transform.position;
			p.rotation = transform.rotation;
			p.fov = _camera.fieldOfView;
			p.nearClip = _camera.nearClipPlane;
			return p;
		}

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

		public void UnsetVar (string name) { SetVar(name, Var.Null); }
		public void SetVar (string name, Var value) { uservars.SetVar(name, value); }
		public void SetPose (string name, Pose pose) { uservars.SetPose(name, pose); }
		public void UnsetPose (string name) { uservars.UnsetPose(name); }
		public static void SetTransform (string name, Transform t) { UserVarManager.SetTransform(name, t); }
		public static void UnsetTransform (string name) { UserVarManager.UnsetTransform(name); }
		public static void UnsetGlobalVar (string name) { SetGlobalVar(name, Var.Null); }
		public static void SetGlobalVar (string name, Var value) { UserVarManager.SetGlobalVar(name, value); }
		
		public Var GetVar (string name) { return uservars.GetVar(name); }
		public static Var GetGlobalVar (string name) { return UserVarManager.GetGlobalVar(name); }
		public static Transform GetTransform (string name) { return UserVarManager.GetTransform(name); }
		public Pose GetPose (string name) { return uservars.GetPose(name); }

		// CrossFade a blender
		public void CrossFade (string blender, int index, float speed = 0.3f)
		{
			foreach (PoseNode node in update_controller.posenodes)
			{
				if (node is PoseBlend)
				{
					PoseBlend blend = node as PoseBlend;
					if (blend.Name.value.value_str == blender)
					{
						blend.CrossFade(index, speed);
					}
				}
			}
		}

		// TODO:  add/del/edit modifier
	}
}
