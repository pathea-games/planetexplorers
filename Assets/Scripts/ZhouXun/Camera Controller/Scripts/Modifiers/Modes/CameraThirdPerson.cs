using UnityEngine;
using System.Collections;

public class CameraThirdPerson : CameraYawPitchRoll
{
	public Transform m_Character;
	public Transform m_Bone;
	public float m_BoneFactor = 0.4f;
	private Vector3 m_CharacterPos;

	public bool m_EnableZoom = true;
	public ECamKey m_ZoomAxis = ECamKey.CK_MouseWheel;
	public float m_ZoomSensitivity = 1;
	public float m_MaxDistance = 50;
	public float m_MinDistance = 1;

	public bool m_AutoSetDistWhenEnterMode;
	public float m_Distance = 10;
	public float m_DistanceWanted = 10;

	public bool m_SyncYaw = false;
	public float m_YawDamper = 5f;
	protected float m_YawWanted = 0f;
	
	public bool m_SyncPitch = false;
	public float m_PitchDamper = 5f;
	protected float m_PitchWanted = 0f;
	
	public bool m_SyncRoll = false;
	public AnimationCurve m_RollCurve = AnimationCurve.Linear(0,0,1,1);

	public float m_DistDamper = 5f;
	public float m_PosDamper = 25f;
	public float m_RollDamper = 3f;

	public bool m_AlwaysRenderCharacter = false;
	public Material m_InvisibleMaterial;

    public float m_OffsetZDis = 0;
    public float m_OffsetYDis = 0;
    public float m_OffsetXDis = 0;
	Vector3 m_CharacterOriginalPos;

    public bool m_EnableFov = true;
    public ECamKey m_FovAxis = ECamKey.CK_MouseWheel;
    public float m_FovSensitivity = 2;
    public float m_MaxFov = 20;
    public float m_MinFov = 0;

    public float m_Fov = 0;
    public float m_FovWanted = 0;
    public float m_FovDamper = 5f;

    public bool m_AutoLerp = false;

    public void EnterShoot()
    {
        //if (m_Character == null)
        //    return;

        //if (m_ShootMode)
        //    return;

        //m_LockCursor = true;
        //m_ShootReady = true;

        //m_OldDis = m_Distance;
        //m_OldMinDis = m_MinDistance;
        //m_OldMaxDis = m_MaxDistance;
        //m_MinPitch = m_ShootMinPitch;
        //m_MaxPitch = m_ShootMaxPitch;
        //m_MinDistance = 1.4f;
        //m_MaxDistance = 1.4f;
    }

    public void QuitShoot()
    {
        //if (m_Character == null)
        //    return;

        //if (!m_ShootMode)
        //    return;

        //m_LockCursor = false;
        //m_ShootMode = false;

        //m_MinPitch = m_OldMinPitch;
        //m_MaxPitch = m_OldMaxPitch;
        //m_MinDistance = m_OldMinDis;
        //m_MaxDistance = m_OldMaxDis;
        //m_DistanceWanted = m_OldDis;
    }

	public override void ModeEnter ()
	{
		base.ModeEnter();
		if ( m_AutoSetDistWhenEnterMode && m_Character != null )
		{
			m_DistanceWanted = Vector3.Distance(m_TargetCam.transform.position, m_Character.position);
			m_Distance = m_DistanceWanted;
		}

        if (m_AutoLerp)
        {
            m_Distance = Mathf.Lerp(m_Distance, m_DistanceWanted, Mathf.Clamp01(Time.deltaTime * m_DistDamper));
        }
	}

	public override void UserInput ()
	{
        //if (m_ShootMode)
        //{
        //    if (m_SyncYaw && m_Character != null && !CamInput.GetKey(m_RotateKey))
        //    {
        //        m_YawWanted = Mathf.Atan2(m_Character.transform.forward.x, m_Character.transform.forward.z) * Mathf.Rad2Deg;
        //        int trunc = Mathf.RoundToInt((m_Yaw - m_YawWanted) / 360.0f);
        //        m_Yaw -= trunc * 360.0f;
        //        m_Yaw = Mathf.Lerp(m_Yaw, m_YawWanted, Mathf.Clamp01(Time.deltaTime * m_YawDamper));
        //    }
        //    base.UserInput();
        //    //if (m_EnableFov && !m_Controller.m_MouseOnScroll)
        //    //{
        //    //    float delta_fov = m_FovSensitivity * CamInput.GetAxis(m_FovAxis) * 10;
        //    //    m_FovWanted += delta_fov;
        //    //    m_FovWanted = Mathf.Clamp(m_FovWanted, Mathf.Max(-30, m_MinFov), Mathf.Min(90, m_MaxFov));
        //    //    m_Fov = Mathf.Lerp(m_Fov, m_FovWanted, Mathf.Clamp01(Time.deltaTime * m_FovDamper));
        //    //}
        //}
        //else
        //{
            if (m_SyncYaw && m_Character != null && !CamInput.GetKey(m_RotateKey))
            {
                m_YawWanted = Mathf.Atan2(m_Character.transform.forward.x, m_Character.transform.forward.z) * Mathf.Rad2Deg;
                int trunc = Mathf.RoundToInt((m_Yaw - m_YawWanted) / 360.0f);
                m_Yaw -= trunc * 360.0f;
                m_Yaw = Mathf.Lerp(m_Yaw, m_YawWanted, Mathf.Clamp01(Time.deltaTime * m_YawDamper));
            }
            if (m_SyncPitch && m_Character != null && !CamInput.GetKey(m_RotateKey))
            {
                m_PitchWanted = -Mathf.Asin(Mathf.Clamp(m_Character.transform.forward.y, -1, 1)) * Mathf.Rad2Deg;
                m_PitchWanted = Mathf.Clamp(m_PitchWanted, m_MinPitch, m_MaxPitch);
                m_Pitch = Mathf.Lerp(m_Pitch, m_PitchWanted, Mathf.Clamp01(Time.deltaTime * m_PitchDamper));
            }
            base.UserInput();
            if (m_EnableZoom && !m_Controller.m_MouseOnScroll)
            {
                float delta_z = Mathf.Pow(m_ZoomSensitivity + 1F, CamInput.GetAxis(m_ZoomAxis));
                m_DistanceWanted *= delta_z;
                m_DistanceWanted = Mathf.Clamp(m_DistanceWanted, Mathf.Max(0.001f, m_MinDistance), m_MaxDistance);
                m_Distance = Mathf.Lerp(m_Distance, m_DistanceWanted, Mathf.Clamp01(Time.deltaTime * m_DistDamper));

                //if (m_ShootReady)
                //{
                //    if (m_Distance < 1.41f && m_Distance > 1.39f)
                //    {
                //        m_ShootMode = true;
                //        m_ShootReady = false;
                //    }
                //}

            }
        //}
	}

	public override void Do ()
	{
		if ( m_Character != null )
		{
			if (m_Bone != null)
				m_CharacterPos = m_Character.position * (1 - m_BoneFactor) + m_Bone.position * m_BoneFactor;
			else
				m_CharacterPos = m_Character.position;

            m_CharacterPos += m_TargetCam.transform.right * m_OffsetXDis + m_TargetCam.transform.up * m_OffsetYDis + m_TargetCam.transform.forward * m_OffsetZDis;
		}
		else
		{
			m_CharacterPos = m_TargetCam.transform.position + m_TargetCam.transform.forward * m_Distance;
		}

		if ( m_SyncRoll && m_Character != null )
		{
			float roll_wanted = 0;
			Vector3 horz_right = Vector3.Cross(Vector3.up, m_Character.forward).normalized;
			if ( horz_right.magnitude > 0.01f )
			{
				float angle_r = Vector3.Angle(horz_right, m_Character.up);
				float angle_l = 180 - angle_r;
				float angle_y = Vector3.Angle(Vector3.up, m_Character.up);
				float roll_r = 0, roll_l = 0;
				if ( angle_y < 90 )
				{
					roll_r = angle_r - 90;
					roll_l = angle_l - 90;
				}
				else
				{
					roll_r = 270 - angle_r;
					roll_l = 270 - angle_l;
				}
				roll_r = m_RollCurve.Evaluate(Mathf.Abs(roll_r) / 180.0f) * 180.0f * Mathf.Sign(roll_r);
				roll_l = m_RollCurve.Evaluate(Mathf.Abs(roll_l) / 180.0f) * 180.0f * Mathf.Sign(roll_l);
				roll_wanted = Mathf.Lerp(roll_r, roll_l, (1 - Vector3.Dot(m_TargetCam.transform.forward, m_Character.forward))*0.5f);
			}
			while ( m_Roll - roll_wanted > 180 )
				m_Roll -= 360;
			while ( m_Roll - roll_wanted <= -180 )
				m_Roll += 360;
			m_Roll = Mathf.Lerp(m_Roll, roll_wanted, Mathf.Clamp01(Time.deltaTime * m_RollDamper));
		}

		base.Do();
		Vector3 pos = m_CharacterPos - m_TargetCam.transform.forward * m_Distance;
		m_TargetCam.transform.position = Vector3.Lerp(m_TargetCam.transform.position, pos, Mathf.Clamp01(Time.deltaTime * m_PosDamper));

		// Render character
		if ( m_AlwaysRenderCharacter && m_Character != null && m_InvisibleMaterial != null )
		{
//			MeshFilter[] mfs = m_Character.root.GetComponentsInChildren<MeshFilter>(true);
//			foreach (MeshFilter mf in mfs)
//			{
//				Graphics.DrawMesh(mf.mesh, mf.transform.position, mf.transform.rotation,  m_InvisibleMaterial, mf.gameObject.layer); 
//			}
//			SkinnedMeshRenderer[] smrs = m_Character.GetComponentsInChildren<SkinnedMeshRenderer>(true);
//			foreach (SkinnedMeshRenderer smr in smrs)
//			{
//				if ( smr != null )
//				{
//
//				}
//			}
		}
	}
}
