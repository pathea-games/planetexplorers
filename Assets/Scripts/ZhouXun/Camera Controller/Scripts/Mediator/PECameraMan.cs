using UnityEngine;
using System.Collections;
#if UNITY_5
using SSAOEffect = SSAOPro;
using AntialiasingAsPostEffect = UnityStandardAssets.ImageEffects.Antialiasing;
using DepthOfField34 = UnityStandardAssets.ImageEffects.DepthOfField;
#endif

public class PECameraMan : MonoBehaviour
{
    private static PECameraMan s_Instance;
	public static PECameraMan Instance
    {
        get
        {
			return s_Instance;
        }
    }

	public static void Create ()
	{
		Object.Instantiate(Resources.Load("Camera Controller"));
	}

	public CamController m_Controller;

//	public Camera m_NearCamera = null;
	public GameObject m_AmbientLights;

	private int m_ControlType = 1;
	public int ControlType { get { return m_ControlType; } }

	#region MONOBEHAVIOUR_FUNCS
	void Awake ()
	{
		s_Instance = this;

		if ( m_Controller.m_TargetCam == null )
			m_Controller.m_TargetCam = CamMediator.MainCamera;

		LoadControlType();
		ChangeNormalMode();

		//m_Controller.AfterAllModifier += SyncNearCam;
	}

	void Start ()
	{
        PECameraMan.ApplySysSetting();
        //CameraController oldCamCtrl = GameObject.FindObjectOfType<CameraController>();
        //if (null != oldCamCtrl)
        //{
        //    FreeCamera freeCamera = oldCamCtrl.GetComponent<FreeCamera>();
        //    if (null != freeCamera)
        //    {
        //        Destroy(freeCamera);
        //    }
        //    Destroy(oldCamCtrl);
        //}

        AddTerrainConstraint();

//		UIOnScrollMouse.OnScroll += HandleUIOnScroll;
//		UIOnMouseMove.MouseOnGui += HandleMouseOnUI;
//		UIOnMouseMove.MouseOpGui += HandleMouseOpUI;
	}

	void Update ()
	{
		CreateNearCamera();
		CreateAmbientLight();
		UpdateControlType();
		UpdateCameraCursor();
		UpdateSyncRoll();
	}
	#endregion

	#region CAMERA_MODES
    T PushMode<T>(string prefabName) where T : CamMode
    {
        T mode = m_Controller.PushMode(prefabName) as T;
        if (null == mode)
        {
            Debug.LogError("push camera mode " + prefabName + " error.");
        }
        return mode;
    }

    public void RemoveCamMode(CamMode mode)
    {
        if (null == mode)
            return;        
        GameObject.Destroy(mode.gameObject);
    }

	CameraYawPitchRoll m_NormalMode = null;
	public CameraYawPitchRoll SetNormalMode(Transform target)
	{
		string name = "Normal Mode F" + m_ControlType.ToString();
		CameraYawPitchRoll mode = PushMode<CameraYawPitchRoll>(name);
		if (mode is CameraThirdPerson)
			(mode as CameraThirdPerson).m_Character = target;
		if (mode is CameraFirstPerson)
			(mode as CameraFirstPerson).m_Character = target;
		mode.ModeEnter();
		m_NormalMode = mode;
		return mode;
	}

	private CameraYawPitchRoll ChangeNormalMode()
	{
		if (m_NormalMode != null)
		{
			CameraThirdPerson tp = m_NormalMode as CameraThirdPerson;
			CameraFirstPerson fp = m_NormalMode as CameraFirstPerson;
			Transform tar = null;
			if (tp != null)
				tar = tp.m_Character;
			if (fp != null)
				tar = fp.m_Character;
			string name = "Normal Mode F" + m_ControlType.ToString();
			m_NormalMode = m_Controller.ReplaceMode(m_NormalMode, name) as CameraYawPitchRoll;
			if (m_NormalMode is CameraThirdPerson)
				(m_NormalMode as CameraThirdPerson).m_Character = tar;
			if (m_NormalMode is CameraFirstPerson)
				(m_NormalMode as CameraFirstPerson).m_Character = tar;
			if (m_Controller.currentMode == m_NormalMode)
				m_NormalMode.ModeEnter();
		}
		return m_NormalMode;
	}

	public CameraThirdPerson EnterShoot(Transform target, float curDist)
	{
		string name = "3rd Person Shoot";
		CameraThirdPerson mode = PushMode<CameraThirdPerson>(name);
		mode.m_Character = target;
        mode.m_Distance = curDist;
		mode.ModeEnter();
        //mode.EnterShoot();
		return mode;
	}

	public CameraThirdPerson EnterVehicle(Transform target)
	{
		string name = "3rd Person Vehicle";
		CameraThirdPerson mode = PushMode<CameraThirdPerson>(name);
		mode.m_Character = target;
		mode.ModeEnter();
		return mode;
	}

	public CameraThirdPerson EnterVehicleArm(Transform target)
	{
		string name = "3rd Person Vehicle Arm";
		CameraThirdPerson mode = PushMode<CameraThirdPerson>(name);
		mode.m_Character = target;
		mode.ModeEnter();
		return mode;
	}

	public CameraFreeLook EnterFreeLook()
	{
		string name = "Free Look";
		CameraFreeLook mode = PushMode<CameraFreeLook>(name);
		mode.ModeEnter();
		return mode;
	}

	public CameraThirdPerson EnterBuild(Transform target)
	{
		return null;
	}

	public CameraFreeLook EnterFreeLookBuild()
	{
		string name = "Free Look Build";
		CameraFreeLook mode = PushMode<CameraFreeLook>(name);
		mode.ModeEnter();
		return mode;
	}
	
	public CameraThirdPerson EnterFollow(Transform target)
    {
		string name = "Follow";
		CameraThirdPerson mode = PushMode<CameraThirdPerson>(name);
		mode.m_Character = target;
		mode.ModeEnter();
		return mode;
	}

	public CameraYawPitchRoll EnterClimb(Transform target)
    {
		int ct = m_ControlType;
		if (ct == 3)
			ct = 2;
		string name = "Normal Mode F" + ct.ToString();
		CameraYawPitchRoll mode = PushMode<CameraYawPitchRoll>(name);
		if (mode is CameraThirdPerson)
			(mode as CameraThirdPerson).m_Character = target;
		if (mode is CameraFirstPerson)
			(mode as CameraFirstPerson).m_Character = target;
		mode.ModeEnter();
		return mode;
	}

	bool _sync_roll = false;
	public bool SyncRoll
	{
		get { return _sync_roll; }
		set { _sync_roll = value; }
	}
	#endregion

    public CameraShakeEffect ShakeEffect(string prefabName, Vector3 pos, float strength = 1, float max_strength = 20, float delayTime = 0, float lifeTime = 5)
    {
        CameraShakeEffect se = m_Controller.AddEffect(prefabName) as CameraShakeEffect;
		se.m_Multiplier = Mathf.Clamp(strength * 150 / (pos-camPosition).sqrMagnitude, 0, max_strength);
		se.Invoke("Shake", delayTime);

        DestroyTimer dt = se.GetComponent<DestroyTimer>();
        dt.m_LifeTime = lifeTime;

        return se;
    }

    public CameraHitEffect HitEffect(Vector3 dir, float delayTime = 0, float lifeTime = 1)
    {
        CameraHitEffect che = m_Controller.FindEffect("Hit Effect") as CameraHitEffect;
        if (che != null)
            return null;

        che = m_Controller.AddEffect("Hit Effect") as CameraHitEffect;
        if (che == null)
            return null;

        che.m_Dir = dir;
        che.m_RotDir = Vector3.Cross(dir, m_Controller.m_TargetCam.transform.up);
        return che;
    }

    public CameraShootEffect ShootEffect(Vector3 dir, float delayTime = 0, float lifeTime = 1)
    {
        //CameraShootEffect che = m_Controller.FindEffect("Shoot Effect") as CameraShootEffect;
        //if (che != null)
        //    return null;

        CameraShootEffect che = m_Controller.AddEffect("Shoot Effect") as CameraShootEffect;
        if (che == null)
            return null;

        che.m_Dir = dir;
        che.m_RotDir = Vector3.Cross(dir, m_Controller.m_TargetCam.transform.up);
        return che;
    }

    public CameraShootEffect BowShootEffect(Vector3 dir, float delayTime = 0, float lifeTime = 1)
    {
        //CameraShootEffect che = m_Controller.FindEffect("Bow Shoot Effect") as CameraShootEffect;
        //if (che != null)
        //    return null;

        CameraShootEffect che = m_Controller.AddEffect("Bow Shoot Effect") as CameraShootEffect;
        if (che == null)
            return null;

        che.m_Dir = dir;
        che.m_RotDir = Vector3.Cross(dir, m_Controller.m_TargetCam.transform.up);
        return che;
    }

    public CameraShootEffect LaserShootEffect(Vector3 dir, float delayTime = 0, float lifeTime = 1)
    {
        //CameraShootEffect che = m_Controller.FindEffect("Laser Shoot Effect") as CameraShootEffect;
        //if (che != null)
        //    return null;

        CameraShootEffect che = m_Controller.AddEffect("Laser Shoot Effect") as CameraShootEffect;
        if (che == null)
            return null;

        che.m_Dir = dir;
        che.m_RotDir = Vector3.Cross(dir, m_Controller.m_TargetCam.transform.up);
        return che;
    }

    public CameraShootEffect ShotgunShootEffect(Vector3 dir, float delayTime = 0, float lifeTime = 1)
    {
        //CameraShootEffect che = m_Controller.FindEffect("Shotgun Shoot Effect") as CameraShootEffect;
        //if (che != null)
        //    return null;

        CameraShootEffect che = m_Controller.AddEffect("Shotgun Shoot Effect") as CameraShootEffect;
        if (che == null)
            return null;

        che.m_Dir = dir;
        che.m_RotDir = Vector3.Cross(dir, m_Controller.m_TargetCam.transform.up);
        return che;
    }

    public CamEffect AddWalkEffect()
    {
        return m_Controller.AddEffect("Walk Effect");
    }

    public CamConstraint AddTerrainConstraint()
    {
        return m_Controller.AddConstraint("Terrain Constraint");
    }

    public Vector3 mousePos
    {
        get
        {
            return m_Controller.mousePosition;
        }
    }

    public Ray mouseRay
    {
        get
        {
            if (null == m_Controller)
            {
                Camera.main.ScreenPointToRay(Input.mousePosition);
            }

            return m_Controller.mouseRay;
        }
    }

	public Vector3 camPosition
	{
		get { return m_Controller.m_TargetCam.transform.position; }
	}

    public Vector3 forward
    {
        get { return m_Controller.forward;}
    }

    public Vector3 horzForward
    {
        get { return m_Controller.horzForward; }
    }

    public bool underWater
    {
        get
        {
            if (null == m_Controller
                || null == m_Controller.m_TargetCam)
            {
                return false;
            }

            return PETools.PE.PointInWater(m_Controller.m_TargetCam.transform.position) > 0.5f;
        }
    }

    #region CameraEffect
	public static void DepthScEnable(bool enable)
    {
//        if (null == m_Controller || null == m_Controller.m_TargetCam)
//        {
//            return;
//        }

        DepthOfField34 depthSc = Camera.main.GetComponent<DepthOfField34>();
        if (null != depthSc)
        {
            depthSc.enabled = enable;
        }
    }

	public static void SSAOEnable(bool enable)
    {
//        if (null == m_Controller || null == m_Controller.m_TargetCam)
//        {
//            return;
//        }

		SSAOEffect ssao = Camera.main.GetComponent<SSAOEffect>();
        if (null != ssao)
        {
            ssao.enabled = enable;
        }
    }

    public static void AAEnable(bool enable)
    {
//        if (null == m_Controller || null == m_Controller.m_TargetCam)
//        {
//            return;
//        }

        AntialiasingAsPostEffect AA = Camera.main.GetComponent<AntialiasingAsPostEffect>();
        if (null != AA)
        {
            AA.enabled = enable;
        }
    }

//	public static void CamFOV(float v)
//    {
////        if (null == m_Controller || null == m_Controller.m_TargetCam)
////        {
////            return;
////        }
//
//        m_Controller.m_TargetCam.fieldOfView = v;
//        m_Controller.SaveParams();
//    }

    public static void ApplySysSetting()
    {
		if (Camera.main == null)
			return;

        AAEnable(SystemSettingData.Instance.mAntiAliasing > 0);
        DepthScEnable(SystemSettingData.Instance.mDepthBlur);
        SSAOEnable(SystemSettingData.Instance.mSSAO);
		Camera.main.hdr = SystemSettingData.Instance.HDREffect;
		PeCamera.SetVar("Camera Inertia", SystemSettingData.Instance.CamInertia);
        PeCamera.SetVar("Drive Camera Inertia", SystemSettingData.Instance.DriveCamInertia);  //log: lz-2016.05.18 功能 #1924在Options内增加载具的摄像机跟随灵敏度调整功能
//        CamFOV(SystemSettingData.Instance.CameraFov);
    }

    #endregion

	void UpdateControlType()
	{
		if (Input.GetKeyDown(KeyCode.F1))
		{
			m_ControlType = 1;
			ChangeNormalMode();
			SaveControlType();
		}
		if (Input.GetKeyDown(KeyCode.F2))
		{
			m_ControlType = 2;
			ChangeNormalMode();
			SaveControlType();
		}
//		if (Input.GetKeyDown(KeyCode.F3))
//		{
//			m_ControlType = 3;
//			ChangeNormalMode();
//			SaveControlType();
//		}
	}
	void SaveControlType()
	{
		PlayerPrefs.SetInt("ControlType", ControlType);
		PlayerPrefs.Save();
	}
	void LoadControlType()
	{
		if (PlayerPrefs.HasKey("ControlType"))
			m_ControlType = PlayerPrefs.GetInt("ControlType");
		else
			m_ControlType = 1;
		if (m_ControlType < 1)
			m_ControlType = 1;
		if (m_ControlType > 2)
			m_ControlType = 2;
	}

	void UpdateSyncRoll ()
	{
		CameraThirdPerson tp = m_Controller.currentMode as CameraThirdPerson;
		CameraFirstPerson fp = m_Controller.currentMode as CameraFirstPerson;
		CameraYawPitchRoll ypr = m_Controller.currentMode as CameraYawPitchRoll;
		if (tp != null)
			tp.m_SyncRoll = _sync_roll;
		if (fp != null)
			fp.m_SyncRoll = _sync_roll;
		if (!_sync_roll)
		{
			if (ypr != null)
			{
				ypr.m_Roll *= 0.85f;
				if (Mathf.Abs(ypr.m_Roll) < 0.005f)
					ypr.m_Roll = 0;
			}
		}
	}

	void CreateNearCamera ()
	{
//		if ( m_Controller.m_TargetCam != null )
//		{
//			if ( m_NearCamera == null )
//			{
//				GameObject nc_res = Resources.Load("Near Camera") as GameObject;
//				if ( nc_res != null )
//				{
//					m_NearCamera = (GameObject.Instantiate(nc_res) as GameObject).GetComponent<Camera>();
//					m_NearCamera.transform.parent = m_Controller.m_TargetCam.transform;
//					m_NearCamera.transform.localPosition = Vector3.zero;
//					m_NearCamera.transform.localRotation = Quaternion.identity;
//					m_NearCamera.transform.localScale = Vector3.one;
//					m_NearCamera.depth = m_Controller.m_TargetCam.depth + 1;
//				}
//			}
//		}
	}
	
	void CreateAmbientLight ()
	{
		if ( m_Controller.m_TargetCam != null )
		{
			if ( m_AmbientLights == null )
			{
				GameObject al_res = Resources.Load("Camera ambient lights") as GameObject;
				if ( al_res != null )
				{
					m_AmbientLights = GameObject.Instantiate(al_res) as GameObject;
					m_AmbientLights.transform.parent = m_Controller.m_TargetCam.transform;
					m_AmbientLights.transform.localPosition = Vector3.zero;
					m_AmbientLights.transform.localRotation = Quaternion.identity;
					m_AmbientLights.transform.localScale = Vector3.one;
				}
			}
		}
	}
	
	void UpdateCameraCursor()
	{
		if (null == m_Controller)
			return;
		
		CamMode camMode = m_Controller.currentMode;
		
		if (null == camMode)
			return;
		
		//bool isFirstPerson = camMode is CameraFirstPerson;
		
		if ((camMode.m_Tag & 1) == 0)
			return;

		if (UIStateMgr.Instance != null)
		{
            camMode.m_LockCursor = !UIStateMgr.Instance.CurShowGui;
            camMode.m_ShowTarget = !UIStateMgr.Instance.CurShowGui;
		}
	}
	

	void SyncNearCam ()
	{
//		if ( m_NearCamera != null )
//		{
//			m_NearCamera.farClipPlane = m_Controller.m_TargetCam.nearClipPlane + 0.1f;
//			m_NearCamera.nearClipPlane = 0.1f;//0.001f; //fix error : Screen position out of view frustum
//			m_NearCamera.fieldOfView = m_Controller.m_TargetCam.fieldOfView;
//		}
	}
	
//	void HandleUIOnScroll ()
//	{
//		m_Controller.m_MouseOnScroll = true;
//	}
//	void HandleMouseOnUI ()
//	{
//		m_Controller.m_MouseOnGUI = true;
//	}
//	void HandleMouseOpUI ()
//	{
//		m_Controller.m_MouseOpOnGUI = true;
//	}
}
