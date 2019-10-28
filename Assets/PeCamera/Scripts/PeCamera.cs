using UnityEngine;
using System.Collections.Generic;
using CameraForge;

public static class PeCamera
{
	public static void Init ()
	{
		CameraForge.CameraController.OnControllerCreate += OnControllerCreate;
		CameraForge.CameraController.OnControllerDestroy += OnControllerDestroy;
		CameraForge.CameraController.AfterControllerPlay += AfterControllerPlay;
	}
	
	public static bool inited
	{
		get { return camc != null; }
	}

	private static bool Check ()
	{
//		if (!inited)
//			Debug.LogError("PeCamera NOT Inited!");
		return inited;
	}

	private static CameraForge.CameraController camc = null;
	private static void OnControllerCreate (CameraForge.CameraController c)
	{
		if (c.camera == Camera.main)
		{
			camc = c;
			Start();
		}
	}
	private static void OnControllerDestroy (CameraForge.CameraController c)
	{
		if (c.camera == Camera.main)
		{
			UserVarManager.ResetAllGlobalVars();
			camc = null;
		}
	}
	private static void AfterControllerPlay (CameraForge.CameraController c)
	{
		SyncNearCamera();
		if (UISightingTelescope.Instance != null)
		{
//			UISightingTelescope.Instance.isShow = c.pose.lockCursor;
			UISightingTelescope.Instance.UpdateType();
		}

		if(null != Pathea.MainPlayerCmpt.gMainPlayer)
		{
			Pathea.MainPlayerCmpt.gMainPlayer.UpdateCamDirection(c.transform.forward);
		}
	}

	public static void PlayUserController (int index)
	{
		if (!Check()) return;
		camc.PlayUserController(index);
	}

	public static void SetBool (string name, bool value) { SetVar(name, value); }
	public static void SetInt (string name, int value) { SetVar(name, value); }
	public static void SetFloat (string name, float value) { SetVar(name, value); }
	public static void SetVector (string name, Vector2 value) { SetVar(name, value); }
	public static void SetVector (string name, Vector3 value) { SetVar(name, value); }
	public static void SetVector (string name, Vector4 value) { SetVar(name, value); }
	public static void SetQuaternion (string name, Quaternion value) { SetVar(name, value); }
	public static void SetColor (string name, Color value) { SetVar(name, value); }
	public static void SetString (string name, string value) { SetVar(name, value); }
	
	public static void SetGlobalBool (string name, bool value) { SetGlobalVar(name, value); }
	public static void SetGlobalInt (string name, int value) { SetGlobalVar(name, value); }
	public static void SetGlobalFloat (string name, float value) { SetGlobalVar(name, value); }
	public static void SetGlobalVector (string name, Vector2 value) { SetGlobalVar(name, value); }
	public static void SetGlobalVector (string name, Vector3 value) { SetGlobalVar(name, value); }
	public static void SetGlobalVector (string name, Vector4 value) { SetGlobalVar(name, value); }
	public static void SetGlobalQuaternion (string name, Quaternion value) { SetGlobalVar(name, value); }
	public static void SetGlobalColor (string name, Color value) { SetGlobalVar(name, value); }
	public static void SetGlobalString (string name, string value) { SetGlobalVar(name, value); }

	public static void UnsetVar (string name) { SetVar(name, Var.Null); }
	public static void SetVar (string name, Var value) { if (!Check()) return; camc.SetVar(name, value); }
	public static void SetPose (string name, Pose pose) { if (!Check()) return; camc.SetPose(name, pose); }
	public static void UnsetPose (string name) { if (!Check()) return; camc.UnsetPose(name); }
	public static void SetTransform (string name, Transform t) { UserVarManager.SetTransform(name, t); }
	public static void UnsetTransform (string name) { UserVarManager.UnsetTransform(name); }
	public static void UnsetGlobalVar (string name) { SetGlobalVar(name, Var.Null); }
	public static void SetGlobalVar (string name, Var value) { UserVarManager.SetGlobalVar(name, value); }

	public static Var GetVar (string name) { if (!Check()) return Var.Null; return camc.GetVar(name); }
	public static Var GetGlobalVar (string name) { return UserVarManager.GetGlobalVar(name); }
	public static Transform GetTransform (string name) { return UserVarManager.GetTransform(name); }
	public static Pose GetPose (string name) { if (!Check()) return Pose.Default; return camc.GetPose(name); }
	
	public static void CrossFade (string blender, int index, float speed = 0.3f)
	{
		if (!Check()) return;
		camc.CrossFade(blender, index, speed);
	}

	private static Camera nearCamera;
	private static void SyncNearCamera ()
	{
		if ( camc != null )
		{
			if (nearCamera == null)
			{
				GameObject nc_res = Resources.Load("Near Camera") as GameObject;
				if ( nc_res != null )
				{
					nearCamera = (GameObject.Instantiate(nc_res) as GameObject).GetComponent<Camera>();
					nearCamera.transform.parent = camc.camera.transform;
					nearCamera.transform.localPosition = Vector3.zero;
					nearCamera.transform.localRotation = Quaternion.identity;
					nearCamera.transform.localScale = Vector3.one;
					nearCamera.depth = camc.camera.depth + 1;
				}
			}
            if (nearCamera != null)
            {
				nearCamera.farClipPlane = camc.camera.nearClipPlane + 0.01f;
				nearCamera.nearClipPlane = 0.03f;
				nearCamera.fieldOfView = camc.camera.fieldOfView;
            }
		}
	}

	public static Transform cutsceneTransform;

	// Effects
	public static PeCameraImageEffect imageEffect;

	public static void Start ()
	{
		SetVar("Obstacle LayerMask", (int)(obstacle_layermask));
		SetVar("Build Mode", false);
		SetVar("Roll Mode", false);
		lockCursorMode = !SystemSettingData.Instance.mMMOControlType || SystemSettingData.Instance.FirstPersonCtrl;
		isFirstPerson = SystemSettingData.Instance.FirstPersonCtrl;

		GameObject go = new GameObject ("Cutscene Transform");
		cutsceneTransform = go.transform;
		SetTransform("Cutscene Camera", cutsceneTransform);

		imageEffect = Camera.main.gameObject.GetComponent<PeCameraImageEffect>();
	}

    public enum ControlMode
    {
        ThirdPerson,
        MMOControl,
        FirstPerson
    }

    public static System.Action<ControlMode> onControlModeChange;
	public static void Update ()
	{
		if (inited)
		{
			// System Settings
			SetGlobalFloat("Rotate Sensitivity", SystemSettingData.Instance.CameraSensitivity * 3.5f);
			SetGlobalFloat("Original Fov", SystemSettingData.Instance.CameraFov);
			SetGlobalBool("Inverse X", SystemSettingData.Instance.CameraHorizontalInverse);
			SetGlobalBool("Inverse Y", SystemSettingData.Instance.CameraVerticalInverse);

			// Character
			if (Pathea.MainPlayerCmpt.gMainPlayer != null)
			{
				Pathea.PeEntity entity = Pathea.MainPlayerCmpt.gMainPlayer.Entity;
				Pathea.BiologyViewCmpt vcmpt = entity.viewCmpt as Pathea.BiologyViewCmpt;
				Pathea.PeTrans trans = entity.peTrans;
				Pathea.PassengerCmpt psgr = entity.GetCmpt<Pathea.PassengerCmpt>();

				SetTransform("Anchor", trans.camAnchor);
				if(vcmpt.monoModelCtrlr != null){
					SetTransform("Character", Pathea.MainPlayerCmpt.gMainPlayer._camTarget);
					SetTransform("Bone Neck M", Pathea.MainPlayerCmpt.gMainPlayer._bneckModel);
				}
				if(vcmpt.monoRagdollCtrlr != null){
					SetTransform("Bone Neck R", Pathea.MainPlayerCmpt.gMainPlayer._bneckRagdoll);
				}
			    bool is_rag_doll = vcmpt.IsRagdoll;
				SetVar("Is Ragdoll", is_rag_doll);

				mainPlayerPosTracker.Record(trans.position, Time.time);
				SetVar("Character Velocity", mainPlayerPosTracker.aveVelocity);

				drivePosTracker.breakDistance = 10;
				drivePosTracker.maxRecord = 4;
				drivePosTracker.Record(trans.position, Time.time);
				SetVar("Driving Velocity", drivePosTracker.aveVelocity);
				SetVar("Rigidbody Velocity", drivePosTracker.aveVelocity);
			
				activitySpaceSize = Utils.EvaluateActivitySpaceSize(trans.camAnchor.position, 0.5f, 50f, Vector3.up, 4f, obstacle_layermask);
				SetVar("Activity Space Size", activitySpaceSize);

				// Some vars
				SetBool("Lock Cursor Mode", lockCursorMode || PeInput.UsingJoyStick);
				SetVar("Arouse Cursor", arouseCursor);
				SetVar("Roll Mode", Pathea.MainPlayerCmpt.isCameraRollable);

				if (GetVar("Build Mode").value_b)
				{
                    //lz-2017.05.18 Tutorial模式下建造不允许进入自由视角
                    if (PeInput.Get(PeInput.LogicFunction.Build_FreeBuildModeOnOff) && !Pathea.PeGameMgr.IsTutorial)
					{
						freeLook = !freeLook;
						camc.CrossFade("Global Blend", freeLook ? 0 : 1, 0.3f);
					}
				}
				else
				{
					int mode = 1;
					if (psgr != null)
					{
						WhiteCat.CarrierController dc = psgr.drivingController;
						if (dc != null)
						{
							mode = 2;
							SetVar("Vehicle Arm", dc.isAttackMode);
						}
					}
					camc.CrossFade("Global Blend", mode, 0.3f);
					freeLook = false;				}
				UpdateShake();
			}
            // Internal Logic
#if true
            if (PeInput.Get(PeInput.LogicFunction.ChangeContrlMode))
            {
                if (SystemSettingData.Instance.FirstPersonCtrl)
                {           // F3->F1
                    lockCursorMode = false;
                    SystemSettingData.Instance.mMMOControlType = true;
                    SystemSettingData.Instance.FirstPersonCtrl = false;
                    SystemSettingData.Instance.dataDirty = true;
                    if (onControlModeChange != null)
                        onControlModeChange.Invoke(ControlMode.ThirdPerson);
                }
                else if (SystemSettingData.Instance.mMMOControlType)
                {   // F1->F2
                    lockCursorMode = true;
                    SystemSettingData.Instance.mMMOControlType = false;
                    SystemSettingData.Instance.FirstPersonCtrl = false;
                    SystemSettingData.Instance.dataDirty = true;
                    if (onControlModeChange != null)
                        onControlModeChange.Invoke(ControlMode.MMOControl);
                }
                else
                {                                                   // F2->F3
                    lockCursorMode = true;
                    SystemSettingData.Instance.FirstPersonCtrl = true;
                    SystemSettingData.Instance.dataDirty = true;
                    if (onControlModeChange != null)
                        onControlModeChange.Invoke(ControlMode.FirstPerson);
                }
            }
#else
			if (PeInput.Get(PeInput.LogicFunction.F1Mode)){
				lockCursorMode = false;
				SystemSettingData.Instance.mMMOControlType = true;
				SystemSettingData.Instance.FirstPersonCtrl = false;
				SystemSettingData.Instance.dataDirty = true;
			}else if (PeInput.Get(PeInput.LogicFunction.F2Mode)){
				lockCursorMode = true;
				SystemSettingData.Instance.mMMOControlType = false;
				SystemSettingData.Instance.FirstPersonCtrl = false;
				SystemSettingData.Instance.dataDirty = true;
			}else if (PeInput.Get(PeInput.LogicFunction.F3Mode)){
				lockCursorMode = true;
				SystemSettingData.Instance.FirstPersonCtrl = true;
				SystemSettingData.Instance.dataDirty = true;
			}
#endif
			if (shootModeIndex == 0 && shootModeTime > 0)
			{
				shootModeTime -= Time.deltaTime;
				if (shootModeTime <= 0)
				{
					camc.CrossFade("3rd Person Blend", 0, 0.05f);
					camc.CrossFade("1st Person Blend", 0, 0.05f);
				}
			}

			if (isFirstPerson)
			{
				SetVar("1st Offset Up", camModeData.offsetUp);
				SetVar("1st Offset", camModeData.offset);
				SetVar("1st Offset Down", camModeData.offsetDown);
			}

			// Mouse states
			SetGlobalVar("Mouse On Scroll", UIMouseEvent.opAnyScroll);
			SetGlobalVar("Mouse On GUI", UIMouseEvent.onAnyGUI);
			SetGlobalVar("Mouse Op GUI", UIMouseEvent.opAnyGUI);
		}
	}

	public static void RecordHistory ()
	{
		if (camc != null) {
			camc.RecordHistory ();
		}
	}

	// Shake Effect
	static bool shakeEnabled0 = false;
	static float shakeDuration0 = 0;
	static float shakeTime0 = 0;

	public static void ApplySkCameraEffect (int id, Pathea.PESkEntity skEntity)
	{
		Pathea.MainPlayerCmpt mainPlayerCmpt = skEntity.GetComponent<Pathea.MainPlayerCmpt>();
		if (mainPlayerCmpt != null)
		{
			if (id == 1)
				PlayAttackShake ();
		}
	}

	public static void PlayAttackShake ()
	{
		PlayShakeEffect(0, 0.4f, 0.2f);
	}
	public static void PlayShakeEffect (int index, float during, float delay)
	{
		if (index == 0)
		{
			shakeEnabled0 = true;
			shakeDuration0 = during;
			shakeTime0 = -delay;
		}
	}
	public static void UpdateShake ()
	{
		if (shakeEnabled0)
		{
			shakeTime0 += Time.deltaTime;
			if (shakeTime0 > shakeDuration0)
				shakeEnabled0 = false;
		}
		SetVar("ShakeEnabled0", shakeEnabled0);
		SetVar("ShakeTime0", shakeTime0);
		SetVar("ShakeDuration0", shakeDuration0);
	}


	private static bool lockCursorMode = false;
	public static bool cursorLocked
	{
		get { return camc.pose.lockCursor; }
	}
	
	public static bool arouseCursor
	{
		get
		{
			if (UIStateMgr.Instance != null)
			{
                if (UIStateMgr.Instance.CurShowGui)
					return true;
			}
			if (Input.GetKey(KeyCode.Tab))
				return true;
			return false;
		}
	}

	public static Vector2 cursorPos
	{
		get
		{
			if (Check())
				return camc.pose.cursorPos;
			return Vector2.one * 0.5f;
		}
	}

	public static Vector3 mousePos
	{
		get
		{
			if (camc == null)
				return Input.mousePosition;
			if (camc.pose.lockCursor)
				return new Vector3(camc.pose.cursorPos.x * Screen.width, camc.pose.cursorPos.y * Screen.height, 0);
			return Input.mousePosition;
		}
	}

    public static Ray mouseRay
    {
        get
        {
			return Camera.main.ScreenPointToRay(mousePos);
        }
    }

	public static Ray cursorRay
	{
		get
		{
			return Camera.main.ScreenPointToRay(
				new Vector3(camc.pose.cursorPos.x * Screen.width,
			    camc.pose.cursorPos.y * Screen.height, 0));
		}
	}

	private static HistoryPosTracker mainPlayerPosTracker = new HistoryPosTracker ();
	private static HistoryPosTracker drivePosTracker = new HistoryPosTracker ();
	public static float activitySpaceSize
	{
		get;
		private set;
	}

	private static int shootModeIndex = 0;
	private static float shootModeTime = 0f;

	private static CameraModeData camModeData = CameraModeData.DefaultCameraData;
	public static CameraModeData cameraModeData
	{
		get { return camModeData; }
		set
		{
			camModeData = value;
			if (camModeData != null)
			{
				shootModeIndex = camModeData.camModeIndex3rd;
				if (camModeData.camModeIndex3rd > 0)
				{
					shootModeTime = 0.7f;
					camc.CrossFade("3rd Person Blend", camModeData.camModeIndex3rd, 0.23f);
					camc.CrossFade("1st Person Blend", camModeData.camModeIndex1st, 0.23f);
				}
				SetVar("1st Offset Up", camModeData.offsetUp);
				SetVar("1st Offset", camModeData.offset);
				SetVar("1st Offset Down", camModeData.offsetDown);
			}
		}
	}

	private static bool isFirstPerson = false;
	public static bool is1stPerson
	{
		get { return isFirstPerson; }
		set
		{
			isFirstPerson = value;
			if (isFirstPerson)
				camc.CrossFade("1/3 Person Blend", 1, 0.23f);
			else
				camc.CrossFade("1/3 Person Blend", 0, 0.23f);
		}
	}

	private static bool freeLook = false;
	public static bool isFreeLook
	{
		get { return freeLook; }
	}

	// LayerMasks
	private static LayerMask obstacle_layermask = 
			(1 << Pathea.Layer.VFVoxelTerrain) |
			(1 << Pathea.Layer.Default) |
			(1 << Pathea.Layer.Building) |
			(1 << Pathea.Layer.Unwalkable) |
			(1 << Pathea.Layer.SceneStatic) |
			//(1 << Pathea.Layer.TreeStatic) |
			//(1 << Pathea.Layer.GIEProductLayer) |
			//(1 << Pathea.Layer.NearTreePhysics) |
			(1 << Pathea.Layer.ProxyPlayer);

	public static bool fpCameraCanRotate
	{
		get
		{
			return GetVar("FirstPersonCameraLock").value_b;
		}

		set
		{
			SetBool("FirstPersonCameraLock", value);
		}
	}
}
