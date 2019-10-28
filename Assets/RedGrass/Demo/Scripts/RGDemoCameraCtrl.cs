using UnityEngine;
using System.Collections;

public class RGDemoCameraCtrl : MonoBehaviour 
{
	// Camera Instance
	private static RGDemoCameraCtrl m_Instance;
	public static RGDemoCameraCtrl Instance { get { return m_Instance; } }
	
	// Camera position when start.
	public Vector3 BeginTarget = new Vector3(0.0f,0.0f,0.0f);
	public float BeginYaw = 30;
	public float BeginPitch = 45;
	public float BeginDistance = 2;
	public Transform Following = null;
	
	// Camera adjusting sensitive.
	public float MoveSensitive = 0.5f;
	public float OrbitSensitive = 1.0f;
	public float ZoomSensitive = 2.0f;
	
	// Damp coef of the camera moving.
	public float Damp = 0.15f;
	
	// Camera capabilities.
	public bool CanMove = true;
	public bool CanOrbit = true;
	public bool AutoOrbit = false;
	public bool CanZoom = true;
	
	// Camera adjust limits.
	public float MaxDistance = 5.00f;
	public float MinDistance = 0.05f;
	
	// Camera position wanted.
	private Vector3 _TargetWanted;
	private float _YawWanted;
	private float _PitchWanted;
	private float _DistWanted;
	
	// Camera position now.
	private Vector3 _Target;
	private float _Yaw;
	private float _Pitch;
	private float _Dist;
	public Vector3 Eye { get { return transform.position; } }
	public Vector3 Target { get { return _Target; } }
	public float Yaw { get { return _Yaw; } }
	public float Pitch { get { return _Pitch; } }
	public float Distance { get { return _Dist; } }
	
	// Which key to control the camera ?
	private int _MoveKey = 2;
	private int _OrbitKey = 1;
	
	// Awake gose first
	void Awake ()
	{
		m_Instance = this;
		Camera.main.depthTextureMode |= DepthTextureMode.Depth;
		Camera.main.depthTextureMode |= DepthTextureMode.DepthNormals;
	}
	
	// Use this for initialization.
	void Start ()
	{
		_Target = BeginTarget;
		_Yaw = BeginYaw;
		_Pitch = BeginPitch;
		_Dist = BeginDistance;
		_TargetWanted = BeginTarget;
		_YawWanted = BeginYaw;
		_PitchWanted = BeginPitch;
		_DistWanted = BeginDistance;
	}
	
	// Reset camera immediatelly
	public void Reset ()
	{
		_Target = BeginTarget;
		_Yaw = BeginYaw;
		_Pitch = BeginPitch;
		_Dist = BeginDistance;
		_TargetWanted = BeginTarget;
		_YawWanted = BeginYaw;
		_PitchWanted = BeginPitch;
		_DistWanted = BeginDistance;	
	}
	
	// Reset camera smoothly
	public void SmoothReset()
	{
		while ( _Yaw < BeginYaw - 180.0f )
			_Yaw = _Yaw + 360.0f;
		while ( _Yaw > BeginYaw + 180.0f )
			_Yaw = _Yaw - 360.0f;
		while ( _Pitch < BeginPitch - 180.0f )
			_Pitch = _Pitch + 360.0f;
		while ( _Pitch > BeginPitch + 180.0f )
			_Pitch = _Pitch - 360.0f;
		_TargetWanted = BeginTarget;
		_YawWanted = BeginYaw;
		_PitchWanted = BeginPitch;
		_DistWanted = BeginDistance;
	}

	// LateUpdate is called once per frame.
	void LateUpdate ()
	{
		CameraCtrl();
		if ( Following != null )
		{
			_TargetWanted = Following.position;
			_Target = Following.position;
		}
		ClampParam();
		CloseToWanted();
		transform.position = CalcPostion();
		transform.LookAt(_Target);
		if ( Mathf.Cos(_Pitch / 180.0f * Mathf.PI) < 0 )
			transform.Rotate( transform.forward, Mathf.PI );
	}
	
	// We can set target directly by using SetTarget()
	public void SetTarget( Vector3 newTarget )
	{
		_TargetWanted = newTarget;
	}
	
	// We can set target directly by using SetTarget()
	public void SetDistance( float dist )
	{
		_DistWanted = dist;
	}
	public void SetYaw( float yaw )
	{
		_YawWanted = yaw;
	}
	public void SetPitch( float pitch )
	{
		_PitchWanted = pitch;
	}
	
	void CameraCtrl()
	{
		// Get some input axes.
		float dx, dy, dw;
		dx = Input.GetAxis("Mouse X");
		dy = Input.GetAxis("Mouse Y");
		dw = Input.GetAxis("Mouse ScrollWheel");
		// Do orbit the camera.
		if ( Input.GetMouseButton(_OrbitKey) && CanOrbit || AutoOrbit )
		{
			_YawWanted = _YawWanted - dx * OrbitSensitive * 5.0f;				
			_PitchWanted = _PitchWanted - dy * OrbitSensitive * 5.0f; 
			// 5.0 is a test coef
		}
		// Do move the camera.
		else if ( Input.GetMouseButton(_MoveKey) && CanMove )
		{
			Vector3 offset = (transform.right * _Dist * MoveSensitive * dx) + 
				             (transform.up * _Dist * MoveSensitive * dy);
			_TargetWanted = _TargetWanted - offset * 0.1f;
			// 0.1 is a test coef
		}
		// Do zoom the camera.
		if ( dw != 0 && CanZoom )
		{
			_DistWanted = _DistWanted * Mathf.Pow( ZoomSensitive + 1.0f, -dw );
		}
	}
	
	// To Clamp parameter _Dist etc.
	void ClampParam()
	{
		if ( MaxDistance < MinDistance )
			MaxDistance = MinDistance;
		if ( _DistWanted > MaxDistance )
			_DistWanted = MaxDistance;
		else if ( _DistWanted < MinDistance )
			_DistWanted = MinDistance;
	}
	
	// Calculate next parameter should be.
	void CloseToWanted()
	{
		if ( (_Target - _TargetWanted).magnitude < 0.0002 )
			_Target = _TargetWanted;
		else
			_Target = _Target + (_TargetWanted - _Target) * Damp;
		if ( Mathf.Abs(_Yaw - _YawWanted) < 0.1 )
			_Yaw = _YawWanted;
		else
			_Yaw = _Yaw + (_YawWanted - _Yaw) * Damp;
		if ( Mathf.Abs(_Pitch - _PitchWanted) < 0.1 )
			_Pitch = _PitchWanted;
		else
			_Pitch = _Pitch + (_PitchWanted - _Pitch) * Damp;
		if ( Mathf.Abs(_Dist - _DistWanted) < 0.0002 )
			_Dist = _DistWanted;
		else
			_Dist = _Dist + (_DistWanted - _Dist) * Damp;
	}
	
	// Calculate position of the camera right now.
	Vector3 CalcPostion()
	{
		Vector3 retval = new Vector3(0.0f,0.0f,0.0f);
		retval.x = _Dist * Mathf.Cos(_Yaw / 180.0f * Mathf.PI) * Mathf.Cos(_Pitch / 180.0f * Mathf.PI);
		retval.z = _Dist * Mathf.Sin(_Yaw / 180.0f * Mathf.PI) * Mathf.Cos(_Pitch / 180.0f * Mathf.PI);
		retval.y = _Dist * Mathf.Sin(_Pitch / 180.0f * Mathf.PI);
		retval = retval + _Target;
		return retval;
	}
	
	// The pick ray
	public Ray PickRay()
	{
		return this.GetComponent<Camera>().ScreenPointToRay( Input.mousePosition );
	}
}
