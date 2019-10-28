//using UnityEngine;
//using System.Collections;
//using System.Collections.Generic;
//using ItemAsset;
//using SkillAsset;

//public abstract class DrivingSkillRunner : CreationSkillRunner
//{
//	public SkillRunner Driver
//	{
//		get
//		{

//			return null;
//		}
//	}

//	protected CameraThirdPerson m_CameraNormalMode = null;
//	protected CameraThirdPerson m_CameraArmMode = null;

//	protected virtual int DefenceType () { return 5; }

//	public override void NetworkApplyDamage(CommonInterface caster, float damage, int lifeLeft)
//	{
//		// hurt passengers
//		if (damage > 5f)
//		{
//			// Show ui to owner
//			//if (Driver == PlayerFactory.mMainPlayer)
//			//{
//			//    if (CreationHpChangeUI.Instance != null)
//			//        CreationHpChangeUI.Instance.Popup(-Mathf.CeilToInt(damage));
//			//}

//		//	ApplyDamageToPassengers(damage);
//			//if (caster != null)
//			//{
//			//    AiTower[] aits = GetComponentsInChildren<AiTower>(true);
//			//    foreach (AiTower ait in aits)
//			//        ait.aiTarget.OnDamage(caster.gameObject, damage);
//			//}
//		}

//		//m_CreationData.m_Hp = lifeLeft;
//	//	HPChangeGUI.ShowHPChange(damage, transform.position, HPChangeGUI.Instance.MonsterHurt);
//	}

//	public override void NetworkAiDeath(CommonInterface caster)
//	{
//		m_IsWreckage = true;
//		HP = -0.001f;
//		OnCrash();
//	}
        
//	internal override void ApplyHpChange (SkillAsset.SkillRunner caster, float hpChange, float damagePercent, int type)
//	{
//		// Check return
//		if ( caster == this.gameObject ) return;
		
//		string logstring = "";
		
//		// Calculate damage
//		float damage = 0;
//		if ( caster == null )
//		{
//			damage = hpChange * damagePercent;
//			if ( damage > 2f )
//				logstring = "damage = " + damage.ToString("0.0") + " type = " + type.ToString();
//		}
//		else
//		{
//			damage = caster.GetAttribute(Pathea.AttribType.Atk) * damagePercent + hpChange;
//			damage *= Random.Range(0.9f, 1.1f);
//			logstring = "basedamage = " + damage.ToString("0.0");
//			int damageType = type == 0 ? AiAsset.AiDamageTypeData.GetDamageType(caster) : type;
//			float damagescale = AiAsset.AiDamageTypeData.GetDamageScale(damageType, DefenceType());
//			damage *= damagescale;
//			logstring += " damage = " + damage.ToString("0.0") + " scale = " + damagescale.ToString("0.00") + " type = " + damageType.ToString();
//		}
//		HP = HP - damage;

//		// hurt passengers
//		if ( damage > 5f )
//		{
//			// Show ui to owner
//			//if ( Driver == PlayerFactory.mMainPlayer )
//			//{
//			//    if ( CreationHpChangeUI.Instance != null )
//			//        CreationHpChangeUI.Instance.Popup(-Mathf.CeilToInt(damage));
//			//}
			
//			//ApplyDamageToPassengers(damage);
//			//if ( caster != null )
//			//{
//			//    AiTower[] aits = GetComponentsInChildren<AiTower>(true);
//			//    foreach ( AiTower ait in aits )
//			//        ait.aiTarget.OnDamage(caster.gameObject, damage);
//			//}
//		}
		
//		// On Crash
//		if ( HP <= 0 && !m_IsWreckage )
//		{
//			m_IsWreckage = true;
//			HP = -0.001f;
//			OnCrash();
//		}
		
//		// Log
//		if ( Application.isEditor && logstring.Length > 1 )
//			Debug.Log("DrivingSkillRunner::ApplyHpChange  " + logstring);		
//	}
	

//	protected override void OnCrash ()
//	{
//		if ( locking_ui != null )
//			locking_ui.FadeOut();
//		//AllPassengersGetOff();
//		base.OnCrash();
//	}


	
//	protected bool m_Fire1Key = false;
//	protected bool m_Fire2Key = false;
//	protected bool m_Fire3Key = false;
//	protected bool m_LockKey = false;
//	protected void CaptureInput ()
//	{
//		if ( NetChar == ENetCharacter.nrOwner )
//		{
//			m_Fire1Key = InputManager.Instance.IsKeyPressed(KeyFunction.Attack_Build_Dig_Fire1);
//			m_Fire2Key = InputManager.Instance.IsKeyPressed(KeyFunction.Talk_Cancel_Fire2);
//			m_Fire3Key = Input.GetKeyDown(KeyCode.C);
//			m_LockKey = Input.GetKeyDown(KeyCode.X);
//		}
//	}
	

//	protected void Update ()
//	{
//		base.Update();
//		// Check
//		if ( ObjectID == 0 ) return;
		
//		// On Crash
//		if ( HP <= 0 && !m_IsWreckage && NetChar == ENetCharacter.nrOwner )
//		{
//			m_IsWreckage = true;
//			HP = -0.001f;
//			OnCrash();
//		}

//		if ( HP > 0 )
//		{

//			// Arm Mode Switch
//			//if ( Driver == PlayerFactory.mMainPlayer )
//			//{
//			//    if ( Input.GetKeyDown(KeyCode.F) )
//			//    {
//			//        if ( dc.m_Cockpit.m_ArmMode )
//			//        {
//			//            dc.QuitArmMode();
//			//            PlayTurnOffSounds();
//			//        }
//			//        else
//			//        {
//			//            dc.EnterArmMode();
//			//            PlayTurnOnSounds();
//			//        }
//			//    }
//			//}
//			//else
//			//{
//			//    if ( locking_ui != null )
//			//        locking_ui.FadeOut();
//			//}

//			//// When Arm Mode
//			//if ( Driver == PlayerFactory.mMainPlayer &&
//			//     dc.m_Cockpit.m_ArmMode )
//			//{
//			//    if ( m_CameraArmMode == null )
//			//    {
//			//        m_CameraArmMode = PECameraMan.Instance.m_Controller.PushMode("3rd Person Vehicle Arm") as CameraThirdPerson;
//			//        m_CameraArmMode.m_Character = dc.m_Cockpit.m_CameraPoint;
//			//        m_CameraArmMode.ModeEnter();
//			//    }
//			//    CaptureInput();
//			//    if ( dc.m_SelectedMissileLaunchers.Count > 0 )
//			//        OperateMissileLaunchers(dc);
//			//    if ( m_Fire1Key )
//			//    {
//			//        foreach ( VCPCtrlTurretFunc ctf in dc.m_CtrlTurrets )
//			//            ctf.Firing();
//			//    }
//			//    if ( m_Fire2Key )
//			//    {
//			//        foreach ( VCPFrontCannonFunc fcf in dc.m_FrontCannons )
//			//            fcf.Firing();
//			//    }
//			//    if ( m_Fire3Key )
//			//    {
//			//        foreach ( VCPMissileLauncherFunc mlf in dc.m_SelectedMissileLaunchers )
//			//            mlf.Firing();
//			//    }
//			//}
//			//else
//			//{
//			//    if ( m_CameraArmMode != null )
//			//    {
//			//        GameObject.Destroy(m_CameraArmMode.gameObject);
//			//        m_CameraArmMode = null;
//			//    }
//			//    if ( locking_ui != null )
//			//        locking_ui.FadeOut();
//			//}

//			//// UI
//			//if ( Driver == PlayerFactory.mMainPlayer && GameGui_N.Instance != null )
//			//{
//			//    float hp01 = HP/MaxHP;
//			//    float fuel01 = m_CreationData.m_Fuel/m_CreationData.m_Attribute.m_MaxFuel;
//			//    Vector3 vel = rigidbody.velocity;
//			//    vel.y = 0;
//			//    float speed01 = vel.magnitude/35;
//			//    MainMidGui_N.Instance.SetCarrierHp(hp01, hp01 > 0.999f ? "FULL" : (hp01*100).ToString("0.0") + "%");
//			//    MainMidGui_N.Instance.SetCarrierFuel(fuel01,  fuel01 > 0.999f ? "FULL" : (fuel01*100).ToString("0.0") + "%");
//			//    MainMidGui_N.Instance.SetCarrierSpeed(speed01, (vel.magnitude*3.6f).ToString("0") + " km/h" );

//			//    CreationDrivingTips.Instance.ClearWarning();
//			//    if ( hp01 < 0.2f )
//			//        CreationDrivingTips.Instance.AddWarning("Your carrier was severely damaged !");
//			//    if ( fuel01 == 0f )
//			//        CreationDrivingTips.Instance.AddWarning("NO FUEL");
//			//    else if ( fuel01 < 0.15f )
//			//        CreationDrivingTips.Instance.AddWarning("Your carrier will soon run out of fuel !");

//			//    // Aiming GUI
//			//    if ( ControlledWeaponTarget.magnitude > 0.01f )
//			//    {
//			//        // Aiming
//			//        Ray aimRay = PeCamera.mouseRay;
//			//        RaycastHit rch;
//			//        AimingEnemy = Physics.SphereCast(aimRay, 0.3F, 400F, AimObjectLayerMask);
//			//    }
//			//    else
//			//    {
//			//        AimingEnemy = false;
//			//    }
				
//			//    // Aim GUI update
//			//    AimRotation += Time.deltaTime * 180;
//			//    AimStateCounter += (AimingEnemy ? Time.deltaTime : -Time.deltaTime);
//			//    AimStateCounter = Mathf.Clamp(AimStateCounter, 0f, 0.6f);
//			//}
//		}
//		if ( Controller.m_Active && NetChar == ENetCharacter.nrOwner )
//		{
//			CalcImpulse();
//			// Ensure safe
//			ensuresafe_frame++;
//			if ( ensuresafe_frame < 100 )
//				m_Impulse = Vector3.zero;
//			ApplyImpulseDamage();
//		}
//	}
	
//	// Average velocity
//	private Vector3 m_AveVelocity = Vector3.zero;
//	private List<Vector3> m_HistoryPos = new List<Vector3> ();
//	private List<float> m_HistoryTime = new List<float> ();
//	public Vector3 AverageVelocity { get { return m_AveVelocity; } }
//	// Impulse
//	private Vector3 m_Impulse = Vector3.zero;
//	private Vector3 m_LastVelocity = Vector3.zero;
//	public Vector3 Impulse { get { return m_Impulse; } }

//	protected virtual void CalcImpulse ()
//	{
//		if ( HP > 0 )
//		{
//			// Calculate average velocity
//			float now = Time.fixedTime;
//			m_HistoryTime.Add(now);
//			m_HistoryPos.Add(transform.position);
//			while ( m_HistoryTime.Count > 1 && now - m_HistoryTime[1] > 1.0f )
//			{
//				m_HistoryTime.RemoveAt(0);
//				m_HistoryPos.RemoveAt(0);
//			}
//			int last = m_HistoryTime.Count - 1;
//			if ( last > 0 )
//				m_AveVelocity = (m_HistoryPos[last] - m_HistoryPos[0]) / (m_HistoryTime[last] - m_HistoryTime[0]);
//			else
//				m_AveVelocity = Vector3.zero;
			
//			if ( NetChar == ENetCharacter.nrOwner )
//			{
//				// Calculate impulse
//				Vector3 delta_vel = GetComponent<Rigidbody>().velocity - m_LastVelocity;
//				float delta_speed = Mathf.Abs(GetComponent<Rigidbody>().velocity.magnitude - m_LastVelocity.magnitude) * 1.5f;
//				float max_magn = AverageVelocity.magnitude * 2.0f;
//				if ( delta_vel.magnitude > max_magn )
//					delta_vel = delta_vel.normalized * max_magn;
//				if ( delta_speed > max_magn )
//					delta_speed = max_magn;
//				m_LastVelocity = GetComponent<Rigidbody>().velocity;
//				if ( Time.deltaTime > 0.001f )
//				{
//					if ( GetComponent<Rigidbody>().angularDrag < 5f )
//						m_Impulse = delta_vel * 25;
//					else
//						m_Impulse = (delta_vel.normalized * delta_speed) * 25;
//				}
//				else
//				{
//					m_Impulse = Vector3.zero;
//				}
//			}
//			else
//			{
//				m_Impulse = Vector3.zero;
//			}
//		}
//	}

//	protected virtual void ApplyImpulseDamage()
//	{
//		// nothing
//	}

//	protected void FixedUpdate()
//	{
//		base.FixedUpdate();
//	}


//	private SkillRunner raycast_target = null;
//	private SkillRunner locking_target = null;
//	private float locking_time_cost = 0.01f;
//	private float locking_time = 0f;
//	private MissileLockerUI locking_ui = null;
	
//	// Start locking or abort/end locking (null)
	


	

//	int ensuresafe_frame = 0;
	

//	private float AimStateCounter = 0;
//	private float AimRotation = 0;
//	private bool AimingEnemy = false;
//	private string AimingResOuter = "GUI/U3DGUI/Textures/aim_outer";
//	private string AimingResInner = "GUI/U3DGUI/Textures/aim_inner";
//	private string AimingResLine = "GUI/U3DGUI/Textures/aim_line";

//}
