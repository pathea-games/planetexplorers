//using UnityEngine;
//using System.Collections;
//using System.Collections.Generic;
//using System.IO;
//
//[System.Serializable]
//public class CSSimulatorAttr
//{
//	public float m_Dps			= 1;	
//	public float m_Hp			= 100;
//
//	[System.Serializable]
//	public class SingleAtk
//	{
//		public float m_CD 		= -1;
//		public bool IsValid()
//		{
//			return m_CD > 0;
//		}
//	}
//	public SingleAtk m_SingleAtk;
//
//	[System.Serializable]
//	public class AOE
//	{
//		public float m_CD			= -1;
//		public float m_Radius 	    = 0f;
//		public int   m_Count		= 0;
//
//		public bool IsValid()
//		{
//			return (m_CD > 0 && m_Radius > 0 && m_Count > 0);
//		}
//	}
//	public AOE  m_AOE;	
//	
//
//	[System.Serializable]
//	public class Therapy
//	{
//		public float m_Val   = 0f;
//		public float m_CD	 = -1f;
//
//		public bool IsValid()
//		{
//			return (m_Val > 0 && m_CD > 0);
//		}
//	}
//	public Therapy  m_Terapy;
//
//
//	public CSSimulatorAttr()
//	{
//		m_SingleAtk = new SingleAtk();
//		m_AOE = new AOE();
//		m_Terapy = new Therapy();
//	}
//
//	public CSSimulatorAttr(CSSimulatorAttr attr)
//	{
//		m_SingleAtk = new SingleAtk();
//		m_AOE = new AOE();
//		m_Terapy = new Therapy();
//
//		m_Dps = attr.m_Dps;
//		m_Hp  = attr.m_Hp;
//		m_SingleAtk.m_CD 		= attr.m_SingleAtk.m_CD;
//		m_AOE.m_CD 				= attr.m_AOE.m_CD;
//		m_AOE.m_Radius			= attr.m_AOE.m_Radius;
//		m_AOE.m_Count			= attr.m_AOE.m_Count;
//		m_Terapy.m_CD			= attr.m_Terapy.m_CD;
//		m_Terapy.m_Val			= attr.m_Terapy.m_Val;
//	}
//	
//}
//
//
//public class CSSimulator : MonoBehaviour
//{
//	CSSimulatorData m_Data;
//	public CSSimulatorData Data { get { return m_Data; }  set {m_Data = value; }}
//
//	private int ID { get { return m_Data.ID;} }
//
//	public float Dps
//	{
//		get { return m_Data.m_Dps; }
//		set { m_Data.m_Dps = value; }
//	}
//
//	public float Hp
//	{
//		get { return m_Data.m_HP; }
//		set { 
//			m_Data.m_HP = value >  m_Data.m_HP ? m_Data.m_HP : value;
//			if (noticeHpChanged != null)
//				noticeHpChanged(m_Data.m_HP/m_Data.m_MaxHP);
//		}
//	}
//
//	public float MaxHp { get { return m_Data.m_MaxHP; } }
//
//	public float AtkCD { get { return m_Data.m_SingleAtkCD; } }
//
//	public float AOECD { get { return m_Data.m_AOECD; } }
//	public float Radius
//	{
//		get { return m_Data.m_AOERadius; } 
//		set 
//		{
//			m_Data.m_AOERadius = value;
//		}
//	}
//
//	public Vector3 Position
//	{
//		get { return m_Data.m_Pos; }
//		set
//		{
//			m_Data.m_Pos = value;
//		}
//	}
//	
//	public float TherapyVal
//	{
//		get { return m_Data.m_TherapyVal; }
//		set 
//		{
//			m_Data.m_TherapyVal = value;
//		}
//	}
//	public float TherapyCD
//	{
//		get { return m_Data.m_TherapyCD; }
//		set
//		{
//			m_Data.m_TherapyCD = value;
//		}
//	}
//
//	// Event 
//	public delegate void AttributeChangedDel(float val);
//	public AttributeChangedDel noticeHpChanged;
//	public AttributeChangedDel noticeDpsChanged;
//
//	public CSSimulatorMgr m_Mgr;
//
////	Transform m_Trans;
//	GameObject m_Go;
//
//
//	#region  DEBUG_USE
//
////	[SerializeField] float m_HP;
////	[SerializeField] float m_MaxHP;
//
//	#endregion
//
//	public void Init(CSSimulatorAttr attr)
//	{
//		if (m_Data == null)
//			return;
//
//		m_Data.m_HP  	= attr.m_Hp;
//		m_Data.m_MaxHP	= attr.m_Hp;
//		m_Data.m_Dps    = attr.m_Dps;
//
//		m_Data.m_SingleAtkCD = attr.m_SingleAtk.m_CD;
//
//		m_Data.m_AOECD 		= attr.m_AOE.m_CD;
//		m_Data.m_AOECount 	= attr.m_AOE.m_Count;
//		m_Data.m_AOERadius 	= attr.m_AOE.m_Radius;
//
//		m_Data.m_TherapyCD  = attr.m_Terapy.m_CD;
//		m_Data.m_TherapyVal = attr.m_Terapy.m_Val;
//	}
//	
//
//	public void SyncHP (float hp_percent)
//	{
//		m_Data.m_HP = Mathf.Max(MaxHp * hp_percent, 0 );
//	}
//
//	void Awake()
//	{
//		m_SingleAtkTimer = new UTimer();
//		m_SingleAtkTimer.ElapseSpeed = 1;
//		m_AOETimer = new UTimer();
//		m_AOETimer.ElapseSpeed = 1;
//	}
//	
//	void Start()
//	{
////		m_Trans = transform;
//		m_Go = gameObject;
//	}
//	
//	UTimer m_SingleAtkTimer;
//	UTimer m_AOETimer;
//	void Update ()
//	{
//		if (m_Mgr == null)
//		{
//			Debug.LogError("Manager is not exist.");
//			return;
//		}
//		
//		float dt = Time.deltaTime;
//		if (Hp <= 0.0f)
//			return;
//		
//		// Single attack
//		if (m_Data.IsSingleAtkValid())
//		{
//			if (m_SingleAtkTimer.Second >= AtkCD)
//			{
//				SPPlayerBase.Single.ApplySimulateDamage(Dps);
//				m_SingleAtkTimer.Tick = 0;
//			}
//			
//			m_SingleAtkTimer.Update(dt);
//		}
//		
//		// AOE
//		if (m_Data.IsAOEAtkValid())
//		{
//			if (m_AOETimer.Second >= AOECD)
//			{
//				SPPlayerBase.Single.ApplySimulateDamage(Dps, Position, Radius);
//				m_AOETimer.Tick = 0;
//			}
//			
//			m_AOETimer.Update(dt);
//		}
//		
//		if (m_Data.IsTherapyValid())
//		{
//			
//		}
//		
//		// Update name
//		m_Go.name = Position.ToString(); 
//
//
//#if UNITY_EDITOR
////		m_HP = Hp;
////		m_MaxHP = MaxHp;
//#endif
//	}
//
//#if false
//	public int ID;
//
//	[SerializeField]
//	CSSimulatorAttr m_Attr;
//
//	[SerializeField]
//	float m_Hp;
//	[SerializeField]
//	Vector3 m_Position;
//
//
//	public float  Dps 	
//	{
//		get { return m_Attr.m_Dps; }
//		set 
//		{
//			m_Attr.m_Dps = value;
//		}
//	}
//
//	public float Hp
//	{
//		get { return m_Hp; }
//		set
//		{
//			m_Hp = value > m_Attr.m_Hp ? m_Attr.m_Hp : value;
//
//			if (noticeHpChanged != null)
//				noticeHpChanged(m_Hp/MaxHp);
//		}
//	}
//
//	public float MaxHp { get { return m_Attr.m_Hp; } }
//
//	public float AtkCD
//	{
//		get { return m_Attr.m_SingleAtk.m_CD; }
//	}
//	public float AOECD
//	{
//		get { return m_Attr.m_AOE.m_CD; }
//	}
//
//	public float Radius
//	{
//		get { return m_Attr.m_AOE.m_Radius; } 
//		set 
//		{
//			m_Attr.m_AOE.m_Radius = value;
//		}
//	}
//
//	public Vector3 Position
//	{
//		get { return m_Position; }
//		set
//		{
//			m_Position = value;
//		}
//	}
//
//	public float TherapyVal
//	{
//		get { return m_Attr.m_Terapy.m_Val; }
//		set 
//		{
//			m_Attr.m_Terapy.m_Val = value;
//		}
//	}
//
//	public float TherapyCD
//	{
//		get { return m_Attr.m_Terapy.m_CD; }
//		set
//		{
//			m_Attr.m_Terapy.m_CD = value;
//		}
//	}
//
//
//
//
//	Transform m_Trans;
//	GameObject m_Go;
//
//	// Event 
//	public delegate void AttributeChangedDel(float val);
//	public AttributeChangedDel noticeHpChanged;
//	public AttributeChangedDel noticeDpsChanged;
//
//	public CSSimulatorMgr m_Mgr;
//
//	public void Init(CSSimulatorAttr attr)
//	{
//		m_Attr = new CSSimulatorAttr(attr);
//		m_Hp	   		= attr.m_Hp;
//
//	}
//
//	public void SyncHP (float hp_percent)
//	{
//		m_Hp = Mathf.Max(MaxHp * hp_percent, 0 );
//	}
//	
//
//	void Awake()
//	{
//		m_SingleAtkTimer = new UTimer();
//		m_SingleAtkTimer.ElapseSpeed = 1;
//		m_AOETimer = new UTimer();
//		m_AOETimer.ElapseSpeed = 1;
//	}
//
//	void Start()
//	{
//		m_Trans = transform;
//		m_Go = gameObject;
//	}
//
//	UTimer m_SingleAtkTimer;
//	UTimer m_AOETimer;
//	void Update ()
//	{
//		if (m_Mgr == null)
//		{
//			Debug.LogError("Manager is not exist.");
//			return;
//		}
//
//		float dt = Time.deltaTime;
//		if (Hp <= 0.0f)
//			return;
//
//		// Single attack
//		if (m_Attr.m_SingleAtk.IsValid())
//		{
//			if (m_SingleAtkTimer.Second >= AtkCD)
//			{
//				SPPlayerBase.Single.ApplySimulateDamage(Dps);
//				m_SingleAtkTimer.Tick = 0;
//			}
//			
//			m_SingleAtkTimer.Update(dt);
//		}
//
//		// AOE
//		if (m_Attr.m_AOE.IsValid())
//		{
//			if (m_AOETimer.Second >= AOECD)
//			{
//				SPPlayerBase.Single.ApplySimulateDamage(Dps, Position, Radius);
//				m_AOETimer.Tick = 0;
//			}
//			
//			m_AOETimer.Update(dt);
//		}
//
//		if (m_Attr.m_Terapy.IsValid())
//		{
//
//		}
//
//		// Update name
//		m_Go.name = Position.ToString(); 
//	}
//
//	#endif
//
//	#region IMPROT & EXPORT
//
////	/// <summary>
////	/// <CSVD> Simulator import
////	/// </summary>
////	public bool Import( BinaryReader r, int VERSION )
////	{
////		switch ( VERSION )
////		{
////		case 0x0114:
////		{
//////			ID 		= r.ReadInt32();
////			m_Hp 	= r.ReadSingle();
////			m_Position = new Vector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
////
////			m_Attr.Import(r, VERSION);
////
////		}return true;
////		default:
////			break;
////		}
////
////		return false;
////	}
////
////	public void Export(BinaryWriter w)
////	{
//////		w.Write(ID);
////		w.Write(m_Hp);
////		w.Write(m_Position.x);
////		w.Write(m_Position.y);
////		w.Write(m_Position.z);
////
////		m_Attr.Export(w);
////		
////	}
//	#endregion
//}
//
//
