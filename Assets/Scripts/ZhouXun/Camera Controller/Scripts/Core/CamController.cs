using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CamController : MonoBehaviour
{
	// Target Camera
	public Camera m_TargetCam;

	// Inspector Vars
	public Transform m_ModeGroup;
	public Transform m_EffectGroup;
	public Transform m_ConstraintGroup;

	// Debug Tools
	public string m_DebugParam;
	public bool m_PushMode;
	public bool m_AddEffect;
	public bool m_PopMode;
	public bool m_RemoveMode;
	public bool m_AddConstraint;

	// Modifiers Collections
	private Stack<CamMode> m_Modes;
	private List<CamEffect> m_Effects;
	private List<CamConstraint> m_Constraints;

	// Events
	public delegate void DCamNotify ();
	public delegate void DCamModeNotify (CamMode mode);
	public event DCamNotify AfterInit;
	public event DCamNotify BeforeDestroy;
	public event DCamNotify BeforeAllModifier;
	public event DCamNotify AfterAllMode;
	public event DCamNotify AfterAllEffect;
	public event DCamNotify AfterAllModifier;
	public event DCamModeNotify OnSwitchMode;

	// Interaction
	public bool m_MouseOnGUI = false;
	public bool m_MouseOpOnGUI = false;
	public bool m_MouseOnScroll = false;

	#region PROPERTIES
	public string currentModeName
	{
		get
		{
			CleanUp();
			if ( m_Modes.Count > 0 )
			{
				CamMode mode = m_Modes.Peek();
				if ( mode != null )
					return mode.name;
			}
			return "";
		}
	}
	
	public CamMode currentMode
	{
		get
		{
			CleanUp();
			if ( m_Modes.Count > 0 )
			{
				CamMode mode = m_Modes.Peek();
				if ( mode != null )
					return mode;
			}
			return null;
		}
	}

	private CamMode currentModeRaw
	{
		get
		{
			if ( m_Modes.Count > 0 )
				return m_Modes.Peek();
			return null;
		}
	}
	
	private bool TargetCameraEnable
	{
		get
		{
			if ( m_TargetCam == null )
				return false;
			if ( !m_TargetCam.gameObject.activeInHierarchy )
				return false;
			if ( !m_TargetCam.enabled )
				return false;
			return true;
		}
	}

	public Vector3 mousePosition
	{
		get
		{
			CamMode mode = currentMode;
			if ( mode != null && mode.m_LockCursor )
				return m_TargetCam.ViewportToScreenPoint(new Vector3(mode.m_TargetViewportPos.x, mode.m_TargetViewportPos.y, 0));
			return Input.mousePosition;
		}
	}
	
	public Vector3 mouseViewportPosition
	{
		get
		{
			CamMode mode = currentMode;
			if ( mode != null && mode.m_LockCursor )
				return new Vector3(mode.m_TargetViewportPos.x, mode.m_TargetViewportPos.y, 0);
			return m_TargetCam.ScreenToViewportPoint(Input.mousePosition);
		}
	}
	
	public Ray mouseRay
	{
		get
		{
			return m_TargetCam.ScreenPointToRay(mousePosition);
		}
	}

	public Vector3 forward { get { return m_TargetCam.transform.forward; } }
	public Vector3 horzForward
	{
		get
		{
			Vector3 horz_right = Vector3.Cross(Vector3.up, m_TargetCam.transform.forward).normalized;
			return Vector3.Cross(horz_right, Vector3.up).normalized;
		}
	}
	public float yaw
	{
		get
		{
			return Mathf.Atan2(forward.x, forward.z) * Mathf.Rad2Deg;
		}
	}

	public Transform character
	{
		get
		{
			CamMode mode = currentMode;
			if ( mode is CameraThirdPerson )
				return (mode as CameraThirdPerson).m_Character;
			if ( mode is CameraFirstPerson )
				return (mode as CameraFirstPerson).m_Character;
			return null;
		}
	}

	// End PROPERTIES
	#endregion

	// Camera Parameters
	public struct CameraParam
	{
		public float fov;
		public float farClip;
		public float nearClip;
		public int cullingMask;
	}
	private CameraParam m_SavedParams;
	public void SaveParams ()
	{
		m_SavedParams = new CameraParam ();
		if ( TargetCameraEnable )
		{
			m_SavedParams.cullingMask = m_TargetCam.cullingMask;
			m_SavedParams.fov = m_TargetCam.fieldOfView;
			m_SavedParams.farClip = m_TargetCam.farClipPlane;
			m_SavedParams.nearClip = m_TargetCam.nearClipPlane;
		}
	}
	
	public void LoadParam ()
	{
		if ( TargetCameraEnable )
		{
			m_TargetCam.cullingMask = m_SavedParams.cullingMask;
			m_TargetCam.fieldOfView = m_SavedParams.fov;
			m_TargetCam.farClipPlane = m_SavedParams.farClip;
			m_TargetCam.nearClipPlane = m_SavedParams.nearClip;
		}
	}

	#region COLLECTION_FUNCS
	// Init Camera Controller
	public void Init ()
	{
		m_Modes = new Stack<CamMode> ();
		m_Effects = new List<CamEffect> ();
		m_Constraints = new List<CamConstraint> ();
		if ( AfterInit != null )
			AfterInit();
	}

	// Destroy Camera Controller
	public void Destroy ()
	{
		if ( BeforeDestroy != null )
			BeforeDestroy();
		ClearAllModifiers();
		m_Modes = null;
		m_Effects = null;
		m_Constraints = null;
	}

	// Add Modifiers
	public CamMode PushMode (string mode_name)
	{
		CamMode mode = CreateModifier("CamModes/" + mode_name) as CamMode;
		if ( mode != null )
		{
			mode.m_Controller = this;
			CamMode curr = currentMode;
			if ( curr != null )
				curr.enabled = false;
			m_Modes.Push(mode);
			mode.m_TargetCam = m_TargetCam;
			if ( OnSwitchMode != null )
				OnSwitchMode(mode);
		}
		return mode;
	}

	public CamMode ReplaceMode (CamMode search, string replace_name)
	{
		if (search == null)
			return null;
		CleanUp();
		CamMode retval = search;
		CamMode[] modes = m_Modes.ToArray();
		for (int i = 0; i < modes.Length; ++i)
		{
			if (modes[i] == search)
			{
				CamMode mode = CreateModifier("CamModes/" + replace_name) as CamMode;
				if ( mode != null )
				{
					mode.m_Controller = this;
					mode.m_TargetCam = m_TargetCam;
					modes[i] = mode;
					GameObject.Destroy(search.gameObject);
					if (i == modes.Length - 1)
					{
						if ( OnSwitchMode != null )
							OnSwitchMode(mode);
					}
					retval = mode;
					break;
				}
			}
		}
		m_Modes.Clear();
		for (int i = modes.Length - 1; i >= 0; --i)
			m_Modes.Push(modes[i]);
		return retval;
	}

	public CamEffect AddEffect (string eff_name)
	{
		CamEffect eff = CreateModifier("CamEffects/" + eff_name) as CamEffect;
		eff.m_Controller = this;
		if ( eff != null )
			m_Effects.Add(eff);
		return eff;
	}
	
	public CamConstraint AddConstraint (string cons_name)
	{
		CamConstraint cons = CreateModifier("CamConstraints/" + cons_name) as CamConstraint;
		cons.m_Controller = this;
		if ( cons != null )
			m_Constraints.Add(cons);
		return cons;
	}

	// Find Modifiers
	public CamMode FindMode (string mode_name)
	{
		foreach ( CamMode mode in m_Modes )
		{
			if ( mode == null )
				continue;
			if ( mode.name.ToLower() == mode_name.ToLower() )
				return mode;
		}
		return null;
	}

	public CamEffect FindEffect (string eff_name)
	{
		return m_Effects.Find(iter => CamEffect.MatchName(iter, eff_name));
	}
	
	public CamConstraint FindConstraint (string cons_name)
	{
		return m_Constraints.Find(iter => CamConstraint.MatchName(iter, cons_name));
	}

	// Remove Modifiers
	public void PopMode ()
	{
		CleanUp();
		if ( m_Modes.Count > 0 )
		{
			CamMode mode = m_Modes.Pop();
			if ( mode != null )
				GameObject.Destroy(mode.gameObject);

			CamMode curr = currentMode;
			if ( curr != null )
			{
				curr.enabled = true;
				curr.m_TargetCam = m_TargetCam;
				curr.ModeEnter();
				if ( OnSwitchMode != null )
					OnSwitchMode(curr);
			}
		}
	}

	public void RemoveMode (string mode_name)
	{
		CamMode mode = FindMode(mode_name);
		if ( mode != null )
			GameObject.Destroy(mode.gameObject);
	}

	public void RemoveEffect (string eff_name)
	{
		CamEffect eff = FindEffect(eff_name);
		if ( eff != null )
			GameObject.Destroy(eff.gameObject);
	}

	public void RemoveConstraint (string cons_name)
	{
		CamConstraint cons = FindConstraint(cons_name);
		if ( cons != null )
			GameObject.Destroy(cons.gameObject);
	}

	// Clear Modifiers
	public void ClearModes ()
	{
		if ( m_Modes != null )
		{
			foreach ( CamMode mode in m_Modes )
			{
				if ( mode != null )
				{
					GameObject.Destroy(mode.gameObject);
				}
			}
			m_Modes.Clear();
		}
	}
	
	public void ClearEffects ()
	{
		if ( m_Effects != null )
		{
			foreach ( CamEffect eff in m_Effects )
			{
				if ( eff != null )
				{
					GameObject.Destroy(eff.gameObject);
				}
			}
			m_Effects.Clear();
		}
	}
	
	public void ClearConstraints ()
	{
		if ( m_Constraints != null )
		{
			foreach ( CamConstraint cons in m_Constraints )
			{
				if ( cons != null )
				{
					GameObject.Destroy(cons.gameObject);
				}
			}
			m_Constraints.Clear();
		}
	}

	public void ClearAllModifiers ()
	{
		ClearConstraints();
		ClearEffects();
		ClearModes();
		Cursor.lockState = Screen.fullScreen? CursorLockMode.Confined: CursorLockMode.None;
#if UNITY_5
		Cursor.visible = true;
#else		
		Screen.showCursor = true;
#endif
	}

	// Create Modifier
	private CamModifier CreateModifier (string respath)
	{
		GameObject mod_res = Resources.Load(respath) as GameObject;
		if ( mod_res == null )
			return null;
		GameObject mod_go = GameObject.Instantiate(mod_res) as GameObject;
		if ( mod_go == null )
			return null;
		mod_go.name = mod_res.name;
		CamModifier cam_mod = mod_go.GetComponent<CamModifier>();
		cam_mod.name = mod_res.name;
		if ( cam_mod == null )
		{
			GameObject.Destroy(mod_go);
			return null;
		}
		if ( cam_mod is CamMode )
			mod_go.transform.parent = m_ModeGroup;
		else if ( cam_mod is CamEffect )
			mod_go.transform.parent = m_EffectGroup;
		else if ( cam_mod is CamConstraint )
			mod_go.transform.parent = m_ConstraintGroup;
		else
			mod_go.transform.parent = transform;
		return cam_mod;
	}
	
	void CleanUp ()
	{
		bool popped = false;
		while ( m_Modes.Count > 0 && m_Modes.Peek() == null )
		{
			m_Modes.Pop();
			popped = true;
		}
		if ( popped )
		{
			CamMode curr = currentModeRaw;
			if ( curr != null )
			{
				curr.enabled = true;
				curr.m_TargetCam = m_TargetCam;
				curr.ModeEnter();
				if ( OnSwitchMode != null )
					OnSwitchMode(curr);
			}
		}
	}

	void UpdateModes ()
	{
		CleanUp();
		if ( m_Modes.Count > 0 )
		{
			CamMode topmode = m_Modes.Peek();
			foreach ( CamMode mode in m_Modes )
			{
				if ( mode != null )
				{
					if ( mode != topmode )
					{
						if ( mode.enabled )
							mode.enabled = false;
					}
					else
					{
						if ( !mode.enabled )
							mode.enabled = true;
					}
				}
			}
		}
		else
		{
			Cursor.lockState = Screen.fullScreen? CursorLockMode.Confined: CursorLockMode.None;
#if UNITY_5
			Cursor.visible = true;
#else			
			Screen.showCursor = true;
#endif
		}
	}
	
	// End COLLECTION_FUNCS
	#endregion

	#region MONOBEHAVIOUR_FUNCS
	void Awake ()
	{
		Init();
	}

	void Start ()
	{
		SaveParams();
	}

	void OnDestroy ()
	{
		Destroy();
	}

	void Update ()
	{
		UpdateDebugTools();
		if ( TargetCameraEnable )
		{
			UpdateModes();
			if ( m_Modes.Count > 0 )
			{
				CamMode mode = m_Modes.Peek();
				if ( mode != null )
					mode.UserInput();
			}
		}
	}

	void LateUpdate ()
	{
		if ( TargetCameraEnable )
		{
			// Load pre-set camera parameters
			LoadParam();

			// BeforeAllModifier Event
			if ( BeforeAllModifier != null )
				BeforeAllModifier();

			// Modes
			if ( m_Modes.Count > 0 )
			{
				CamMode mode = m_Modes.Peek();
				if ( mode != null )
				{
					mode.m_TargetCam = m_TargetCam;
					mode.Do();
				}
			}

			// AfterAllMode Event
			if ( AfterAllMode != null )
				AfterAllMode();

			// Effects
			foreach ( CamEffect eff in m_Effects )
			{
				if ( eff != null )
				{
					eff.m_TargetCam = m_TargetCam;
					eff.Do();
				}
			}

			// AfterAllEffect Event
			if ( AfterAllEffect != null )
				AfterAllEffect();

			// Constraints
			foreach ( CamConstraint constraint in m_Constraints )
			{
				if ( constraint != null )
				{
					constraint.m_TargetCam = m_TargetCam;
					constraint.Do();
				}
			}

			// AfterAllModifier Event
			if ( AfterAllModifier != null )
				AfterAllModifier();

			// Interactions
			m_MouseOnGUI = false;
			m_MouseOpOnGUI = false;
			m_MouseOnScroll = false;
		}
	}
	// End MONOBEHAVIOUR_FUNCS
	#endregion

	void UpdateDebugTools ()
	{
		if ( Application.isEditor )
		{
			if ( m_PushMode )
			{
				m_PushMode = false;
				PushMode(m_DebugParam);
			}
			if ( m_AddEffect )
			{
				m_AddEffect = false;
				AddEffect(m_DebugParam);
			}
			if ( m_RemoveMode )
			{
				m_RemoveMode = false;
				RemoveMode(m_DebugParam);
			}
			if ( m_PopMode )
			{
				m_PopMode = false;
				PopMode();
			}
			if ( m_AddConstraint )
			{
				m_AddConstraint = false;
				AddConstraint(m_DebugParam);
			}
		}
	}
}
