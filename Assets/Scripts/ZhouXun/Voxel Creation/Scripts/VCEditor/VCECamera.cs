using UnityEngine;
using System;
using System.Collections;

public class VCECamera : MonoBehaviour
{
	// Camera Instance
	private static VCECamera s_Instance;
	public static VCECamera Instance { get { return s_Instance; } }

	public int ControlMode = 2;
	
	// Camera position when start.
	public Vector3 BeginTarget = new Vector3(0.0f,0.0f,0.0f);
	public float BeginYaw = 30;
	public float BeginPitch = 45;
	public float BeginRoll = 0;
	public float BeginDistance = 2;
	public Vector3 BeginForward = new Vector3 (0.6f, -0.4f, 1.0f);
	public Vector3 BeginUp = new Vector3 (0.0f, 1.0f, 0.0f);
	
	// Camera adjusting sensitive.
	public float MoveSensitive = 0.5f;
	public float OrbitSensitive = 1.0f;
	public float ZoomSensitive = 2.0f;
	
	// Damp coef of the camera moving.
	public float Damp = 0.15f;
	
	// Camera capabilities.
	public bool CanMove = true;
	public bool CanOrbit = true;
	public bool CanZoom = true;
	public bool CanRoll = false;
	
	// Camera adjust limits.
	public float MaxDistance = 50.00f;
	public float MinDistance = 0.05f;
	
	// Camera position wanted.
	private Vector3 _TargetWanted;
	private float _YawWanted;
	private float _PitchWanted;
	private float _RollWanted;
	private float _DistWanted;
	
	private Vector3 _ForwardWanted;
//	private Vector3 _UpWanted;
	
	// Camera position now.
	private Vector3 _Target;
	[SerializeField] private float _Yaw;
	[SerializeField] private float _Pitch;
	[SerializeField] private float _Roll;
	[SerializeField] private float _Dist;
	private Vector3 _Forward;
	private Vector3 _Up;
	public Vector3 Eye { get { return transform.position; } }
	public Vector3 Target { get { return _Target; } }
	public float Yaw { get { return _Yaw; } }
	public float Pitch { get { return _Pitch; } }
	public float Roll { get { return _Roll; } }
	public float Distance { get { return _Dist; } }
	
	// Which key to control the camera ?
	private int _MoveKey = 2;
	private int _OrbitKey = 1;
	private KeyCode _RollKey = KeyCode.Z;
	
	// The pick ray
	public Ray PickRay { get { return this.GetComponent<Camera>().ScreenPointToRay( Input.mousePosition ); } }
	
	void Awake ()
	{
		s_Instance = this;
	}
	
	// Use this for initialization
	void Start ()
	{
		Reset();
	}
	
	// Reset camera immediatelly
	public void Reset ()
	{
		_Target = BeginTarget;
		_Yaw = BeginYaw;
		_Pitch = BeginPitch;
		_Roll = BeginRoll;
		_Dist = BeginDistance;
		_Forward = BeginForward;
		_Up = BeginUp;
		
		_TargetWanted = BeginTarget;
		_YawWanted = BeginYaw;
		_PitchWanted = BeginPitch;
		_RollWanted = BeginRoll;
		_DistWanted = BeginDistance;
		_ForwardWanted = BeginForward;
		//_UpWanted = BeginUp;
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
		while ( _Roll < BeginRoll - 180.0f )
			_Roll = _Roll + 360.0f;
		while ( _Roll > BeginRoll + 180.0f )
			_Roll = _Roll - 360.0f;
		
		_TargetWanted = BeginTarget;
		_YawWanted = BeginYaw;
		_PitchWanted = BeginPitch;
		_RollWanted = BeginRoll;
		_DistWanted = BeginDistance;
		_ForwardWanted = BeginForward;
		//_UpWanted = BeginUp;
	}

	public void FixView ()
	{
		CanOrbit = false;
		CanZoom = false;
		CanMove = false;
		CanRoll = false;
	}

	public void FreeView ()
	{
		CanOrbit = true;
		CanZoom = true;
		CanMove = true;
		CanRoll = true;
	}
	
	// We can set target directly by using SetTarget()
	public void SetTarget( Vector3 newTarget )
	{
		_TargetWanted = newTarget;
	}
	// We can set distance directly by using SetDistance()
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
	public void SetRoll( float roll )
	{
		_RollWanted = roll;
	}

	private void NormalizeVectors()
	{
		_ForwardWanted.Normalize();
		_Forward.Normalize();
		
		Vector3 right = Vector3.Cross(_Up, _Forward);
		_Up = Vector3.Cross(_Forward, right);
		_Up.Normalize();
	}
	
	private void CameraCtrl()
	{
		// Get some input axes.
		float dx, dy, dw;
		dx = Input.GetAxis("Mouse X");
		dy = Input.GetAxis("Mouse Y");
		dw = Input.GetAxis("Mouse ScrollWheel");
		// Do orbit the camera.
		if ( Input.GetMouseButton(_OrbitKey) && CanOrbit )
		{
			if ( ControlMode == 1 )
			{
				if ( Input.GetKey(_RollKey) )
				{
					if ( CanRoll )
						_RollWanted = _RollWanted - dx * OrbitSensitive * 5.0f;
				}
				else
				{
					_YawWanted = _YawWanted - dx * OrbitSensitive * 5.0f;
					_PitchWanted = _PitchWanted - dy * OrbitSensitive * 5.0f;
					// 5.0 is a test coef
				}
			}
			else if ( ControlMode == 2 )
			{
				if ( Input.GetKey(_RollKey) )
				{
					
				}
				else
				{
					Vector3 right = Vector3.Cross(_Up, _Forward);
					_ForwardWanted = _ForwardWanted + dx * OrbitSensitive * right * 0.1f;
					_ForwardWanted = _ForwardWanted + dy * OrbitSensitive * _Up * 0.1f;
				}
			}
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

		// Walk
		if ( !Input.GetKey(KeyCode.LeftShift) && 
		    !Input.GetKey(KeyCode.RightShift) &&
		    !Input.GetKey(KeyCode.LeftControl) &&
		    !Input.GetKey(KeyCode.RightControl) &&
		    !Input.GetKey(KeyCode.LeftAlt) &&
		    !Input.GetKey(KeyCode.RightAlt) &&
		    !UICamera.inputHasFocus )
		{
			if ( Input.GetKey(KeyCode.W) )
			{
				_TargetWanted = _TargetWanted + transform.forward * Time.deltaTime * _DistWanted;
			}
			if ( Input.GetKey(KeyCode.S) )
			{
				_TargetWanted = _TargetWanted - transform.forward * Time.deltaTime * _DistWanted;
			}
			if ( Input.GetKey(KeyCode.A) )
			{
				_TargetWanted = _TargetWanted - transform.right * Time.deltaTime * _DistWanted;
			}
			if ( Input.GetKey(KeyCode.D) )
			{
				_TargetWanted = _TargetWanted + transform.right * Time.deltaTime * _DistWanted;
			}
			if ( Input.GetKey(KeyCode.Q) )
			{
				_TargetWanted = _TargetWanted + transform.up * Time.deltaTime * _DistWanted;
			}
			if ( Input.GetKey(KeyCode.Z) )
			{
				_TargetWanted = _TargetWanted - transform.up * Time.deltaTime * _DistWanted;
			}
		}
		if ( VCEditor.DocumentOpen() && !Input.GetMouseButton(_MoveKey) )
		{
			Bounds b = new Bounds (VCEditor.s_Scene.m_Setting.EditorWorldSize * 0.5f, VCEditor.s_Scene.m_Setting.EditorWorldSize * 3);
			if ( _TargetWanted.x < b.min.x )
				_TargetWanted.x = b.min.x;
			if ( _TargetWanted.y < b.min.y )
				_TargetWanted.y = b.min.y;
			if ( _TargetWanted.z < b.min.z )
				_TargetWanted.z = b.min.z;

			if ( _TargetWanted.x > b.max.x )
				_TargetWanted.x = b.max.x;
			if ( _TargetWanted.y > b.max.y )
				_TargetWanted.y = b.max.y;
			if ( _TargetWanted.z > b.max.z )
				_TargetWanted.z = b.max.z;
		}
	}
	
	// To Clamp parameter _Dist etc.
	private void ClampParam()
	{
		if ( MaxDistance < MinDistance )
			MaxDistance = MinDistance;
		if ( _DistWanted > MaxDistance )
			_DistWanted = MaxDistance;
		else if ( _DistWanted < MinDistance )
			_DistWanted = MinDistance;
	}
	
	// Calculate next parameter should be.
	private void CloseToWanted()
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
		if ( Mathf.Abs(_Roll - _RollWanted) < 0.1 )
			_Roll = _RollWanted;
		else
			_Roll = _Roll + (_RollWanted - _Roll) * Damp;
		if ( Mathf.Abs(_Dist - _DistWanted) < 0.0002 )
			_Dist = _DistWanted;
		else
			_Dist = _Dist + (_DistWanted - _Dist) * Damp;
		if ( (_Forward - _ForwardWanted).magnitude < 0.0002 )
			_Forward = _ForwardWanted;
		else
			_Forward = _Forward + (_ForwardWanted - _Forward) * Damp;
	}
	
	// Calculate position of the camera right now.
	private void CalcTransform()
	{
		if ( ControlMode == 1 )
		{
			Vector3 pos = new Vector3(0.0f,0.0f,0.0f);
			pos.x = _Dist * Mathf.Cos(_Yaw / 180.0f * Mathf.PI) * Mathf.Cos(_Pitch / 180.0f * Mathf.PI);
			pos.z = _Dist * Mathf.Sin(_Yaw / 180.0f * Mathf.PI) * Mathf.Cos(_Pitch / 180.0f * Mathf.PI);
			pos.y = _Dist * Mathf.Sin(_Pitch / 180.0f * Mathf.PI);
			pos = pos + _Target;
			transform.position = pos;
			transform.LookAt(_Target);
			transform.Rotate( transform.forward, _Roll / 180.0f * Mathf.PI );
			if ( Mathf.Cos(_Pitch / 180.0f * Mathf.PI) < 0 )
				transform.Rotate( transform.forward, Mathf.PI );
		}
		else if ( ControlMode == 2 )
		{
			Vector3 pos = _Target;
			pos -= (_Forward * _Dist);
			transform.position = pos;
			transform.LookAt(_Target, _Up);
		}
	}
	// Update is called once per frame
	void Update ()
	{
	
	}
	
	// LateUpdate is called once per frame.
	void LateUpdate ()
	{
		NormalizeVectors();
		CameraCtrl();
		ClampParam();
		CloseToWanted();
		NormalizeVectors();
		CalcTransform();
	}
	
	void OnDestroy ()
	{
//		s_Instance = null;
	}
}
