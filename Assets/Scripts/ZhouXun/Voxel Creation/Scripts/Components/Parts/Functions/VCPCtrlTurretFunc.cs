using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using WhiteCat;


//public class VCPCtrlTurretFunc : VCPartFunc
//{
//	public DrivingController Controller;
//	public VCPCtrlTurretProperty Property = null;
//	public List<Transform> Muzzles = null;
//	public List<Transform> MuzzleEffects = null;
//	public List<int> SkillIds = new List<int> ();
//	public Transform Turret_Yaw;
//	public Transform Turret_Pitch;

//	private List<float> CDTimes = new List<float> ();
//	private float ShareCDTime = 0;
	
//	public delegate bool FireFunc ( VCPCtrlTurretFunc ct, int skillid );
//	public event FireFunc OnFire;

//	public AudioSource m_FireSound;
//	public AudioSource m_TurnOnSound;
//	public AudioSource m_TurnOffSound;
//	public AudioSource m_TurningSound;
//	public bool m_FireSoundOneShot;
//	public bool m_TurningSoundOneShot;
//	public bool m_CanPlaySound = true;
//	public float m_SoundPhase = 0;
//	public float m_CameraShakeStrength = 0;
//	public Pathea.SkAliveEntity m_SkillCmpt = null;
	
//	void Start ()
//	{
//		if ( Controller == null )
//			return;
//		for ( int i = 0; i < Muzzles.Count; ++i )
//			CDTimes.Add(0);
//		ShareCDTime = 0;
//		m_SoundPhase = Random.value;

//		Pathea.SkAliveEntity skParent = Controller.GetComponent<Pathea.SkAliveEntity>();
//		if (skParent != null)
//		{
//			m_SkillCmpt = gameObject.AddComponent<Pathea.SkAliveEntity>();  
//			m_SkillCmpt.SkParent = skParent.SkParent;
//			m_SkillCmpt.InitSkEntity();
//			m_SkillCmpt.SetAttribute(Pathea.AttribType.Atk,Property.Attack);
//			m_SkillCmpt.SetUseParentMasks(Pathea.AttribType.Atk,false);
//		}
//	}
	
//	void UpdateCDTimes ()
//	{
//		float dt = Time.deltaTime;
//		for ( int i = 0; i < CDTimes.Count; ++i )
//			CDTimes[i] = CDTimes[i] - dt;
//		ShareCDTime -= dt;
//	}
	
//	private float _Yaw;
//	private float _Pitch;
//	private float _YawWanted;
//	private float _PitchWanted;
//	public void SetTarget( Vector3 target_wanted )
//	{
//		if ( target_wanted != Vector3.zero )
//		{
//			_NetworkTarget = target_wanted;
//			Vector3 radio = target_wanted - Turret_Pitch.position;
//			Vector3 local_n_radio = transform.InverseTransformDirection( radio ).normalized;
//			_PitchWanted = Mathf.Asin(local_n_radio.y) * Mathf.Rad2Deg;
//			_YawWanted = Mathf.Atan2(local_n_radio.x, local_n_radio.z) * Mathf.Rad2Deg;
//		}
//		else
//		{
//			_YawWanted = 0;
//			_PitchWanted = 0;
//		}
//	}

//	private Vector3 _NetworkTarget;
//	public Vector3 NetworkTarget { get { return _NetworkTarget; } }

//	public LayerMask AimLayerMask = 0;
//	public LayerMask AimObjectLayerMask = 0;

//	void Aim ()
//	{
		
//	}

//	float turning_sound_vol_target = 0;
//	void UpdateTurret ()
//	{
//		if ( Mathf.Abs(_Yaw) > 1000000 )
//			_Yaw = 0;
//		while ( _Yaw < _YawWanted - 180.0f )
//			_Yaw = _Yaw + 360.0f;
//		while ( _Yaw > _YawWanted + 180.0f )
//			_Yaw = _Yaw - 360.0f;

//		if ( m_TurningSound != null )
//		{
//			turning_sound_vol_target = (Mathf.Abs(_YawWanted - _Yaw) + Mathf.Abs(_PitchWanted - _Pitch)) / 30.0f;
//			if ( m_TurningSoundOneShot )
//			{
//				if ( turning_sound_vol_target > 2 )
//				{
//					if ( !m_TurningSound.isPlaying )
//						PlayTurningSound();
//				}
//			}
//			else
//			{
//				PlayTurningSound();
//				m_TurningSound.volume = Mathf.Lerp(m_TurningSound.volume, turning_sound_vol_target * VCGameMediator.SEVol, 0.35f);
//			}
//		}

//		_Yaw += (_YawWanted - _Yaw) * Property.MovingDamp;
//		_Pitch += (_PitchWanted - _Pitch) * Property.MovingDamp;
//		if ( _Pitch > Property.MaxPitch )
//			_Pitch = Property.MaxPitch;
//		else if ( _Pitch < Property.MinPitch )
//			_Pitch = Property.MinPitch;

//		if ( Turret_Yaw == Turret_Pitch )
//		{
//			Turret_Yaw.localEulerAngles = new Vector3 (-_Pitch, _Yaw, 0);
//		}
//		else
//		{
//			Turret_Yaw.localEulerAngles = new Vector3 (0, _Yaw, 0);
//			Turret_Pitch.localEulerAngles = new Vector3 (-_Pitch, 0, 0);
//		}
//	}

//	bool try_firing = false;
//	void Update ()
//	{
//		if ( Controller == null )
//			return;
//		UpdateCDTimes();
//		Aim();
//		UpdateTurret();
//		if ( !try_firing || !m_CanPlaySound )
//		{
//			MuteFireSound();
//		}
//		try_firing = false;
//		if ( m_FireSound != null && !m_FireSoundOneShot )
//		{
//			sound_vol = Mathf.Lerp(sound_vol, sound_vol_target, 0.4f);
//			m_FireSound.volume = Mathf.Clamp01(sound_vol) * VCGameMediator.SEVol;
//			if ( sound_vol < 0.005f )
//				m_FireSound.Stop();
//		}
//	}
	
//	public void Firing ()
//	{
//	}
	
//	public void PlayMuzzleEffect (int index)
//	{
//		ParticleSystem[] pss = MuzzleEffects[index].GetComponentsInChildren<ParticleSystem>();
//		foreach ( ParticleSystem ps in pss )
//		{
//			ps.Play();
//		}
//	}

//	public void PlayFireSound ()
//	{
//		if ( m_FireSound != null && m_CanPlaySound )
//		{
//			if ( m_FireSoundOneShot )
//			{
//				if ( m_FireSound.isPlaying )
//					m_FireSound.Stop();
//				m_FireSound.time = 0;
//				m_FireSound.pitch = Random.Range(0.97f, 1.03f);
//				m_FireSound.volume = VCGameMediator.SEVol;
//				m_FireSound.Play();
//				PECameraMan.Instance.ShakeEffect("Small Shake Effect", m_FireSound.transform.position, m_CameraShakeStrength, 20);
//			}
//			else
//			{
//				m_FireSound.pitch = Random.Range(0.97f, 1.03f);
//				sound_vol_target = 4;
//				if ( !m_FireSound.isPlaying )
//				{
//					m_FireSound.time = m_SoundPhase * m_FireSound.clip.length;
//					m_FireSound.volume = VCGameMediator.SEVol;
//					m_FireSound.Play();
//				}
//			}
//		}
//	}

//	float sound_vol_target = 0;
//	float sound_vol = 0;
//	public void MuteFireSound ()
//	{
//		if ( m_FireSound != null )
//		{
//			if ( !m_FireSoundOneShot )
//			{
//				sound_vol_target = 0;
//			}
//		}
//	}
	
//	public void PlayTurnOnSound ()
//	{
//		if ( m_TurnOnSound != null && m_CanPlaySound )
//		{
//			if ( m_TurnOnSound.isPlaying )
//				m_TurnOnSound.Stop();
//			m_TurnOnSound.time = 0;
//			m_TurnOnSound.pitch = Random.Range(0.96f, 1.04f);
//			m_TurnOnSound.volume = VCGameMediator.SEVol;
//			m_TurnOnSound.Play();
//		}
//	}
	
//	public void PlayTurnOffSound ()
//	{
//		if ( m_TurnOffSound != null && m_CanPlaySound )
//		{
//			if ( m_TurnOffSound.isPlaying )
//				m_TurnOffSound.Stop();
//			m_TurnOffSound.time = 0;
//			m_TurnOffSound.pitch = Random.Range(0.96f, 1.04f);
//			m_TurnOffSound.volume = VCGameMediator.SEVol;
//			m_TurnOffSound.Play();
//		}
//	}

//	public void PlayTurningSound ()
//	{
//		if ( m_TurningSound != null && m_CanPlaySound )
//		{
//			if ( m_TurningSoundOneShot )
//			{
//				if ( m_TurningSound.isPlaying )
//					m_TurningSound.Stop();
//				m_TurningSound.time = 0;
//				m_TurningSound.pitch = Random.Range(0.97f, 1.03f);
//				m_TurningSound.volume = VCGameMediator.SEVol;
//				m_TurningSound.Play();
//			}
//			else
//			{
//				m_TurningSound.pitch = 1;
//				if ( !m_TurningSound.isPlaying )
//				{
//					m_TurningSound.time = m_SoundPhase * m_TurningSound.clip.length;
//					m_TurningSound.volume = VCGameMediator.SEVol;
//					m_TurningSound.Play();
//				}
//			}
//		}
//	}
//}
