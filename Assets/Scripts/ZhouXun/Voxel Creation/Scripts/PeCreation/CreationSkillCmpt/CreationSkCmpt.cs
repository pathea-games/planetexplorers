//using UnityEngine;
//using System.Collections;
//using System.Collections.Generic;
//using ItemAsset;
//using Pathea;
//using SkillSystem;

//public abstract partial class CreationSkCmpt : PeCmpt
//{
//	protected SkAliveEntity m_SkillCmpt = null;


//	protected CreationData m_CreationData;
//	private VCESceneSetting m_SceneSetting = null;
//	public int ObjectID
//	{
//		get { return m_CreationData == null ? 0 : m_CreationData.m_ObjectID; }
//		set
//		{
//			m_CreationData = CreationMgr.GetCreation(value);
//			if ( m_CreationData != null )
//				m_SceneSetting = m_CreationData.m_IsoData.m_HeadInfo.FindSceneSetting();
//		}
//	}
//	public ECreation CreationType
//	{
//		get { return m_CreationData == null ? ECreation.Null : m_CreationData.m_Attribute.m_Type; }
//	}
//	public VCESceneSetting SceneSetting { get { return m_SceneSetting; } }
//	public float HP
//	{
//		get { return 0; }
//		set {}
//	}
//	public float MaxHP
//	{
//		get { return m_CreationData == null ? 0 : m_CreationData.m_Attribute.m_Durability; }
//	}
//	[SerializeField] private string m_DebugStr;
//	protected bool m_IsWreckage = false;
//	public bool IsWreckage { get { return m_IsWreckage; } }
	
//	private CreationBound m_CreationBound;
//	public Bounds LocalBounds
//	{
//		get
//		{
//			if ( m_CreationBound == null )
//				m_CreationBound = GetComponent<CreationBound>();
//			return m_CreationBound.m_Bound;
//		}
//	}
//	private CreationController m_Controller;
//	public CreationController Controller
//	{
//		get { if ( m_Controller == null ) m_Controller = GetComponent<CreationController>(); return m_Controller; }
//	}
//	public ENetCharacter NetChar
//	{
//		get { return Controller.m_NetChar; }
//		set { Controller.m_NetChar = value; }
//	}
	
//	public virtual void Init ()
//	{
//		ObjectID = Controller.m_CreationData.m_ObjectID;
//		VCParticlePlayer[] pps = GetComponentsInChildren<VCParticlePlayer>(false);
//		m_DamageParticlePlayers = new List<VCParticlePlayer> ();
//		m_ExplodeParticlePlayers = new List<VCParticlePlayer> ();
//		foreach ( VCParticlePlayer pp in pps )
//		{
//			if ( pp.FunctionTag == VCParticlePlayer.ftDamaged )
//				m_DamageParticlePlayers.Add(pp);
//			else if ( pp.FunctionTag == VCParticlePlayer.ftExplode )
//				m_ExplodeParticlePlayers.Add(pp);
//		}
//		InitSkillCmpt();
//	}
//	protected virtual void OnCrash ()
//	{
//		DragArticleAgent dragItem = SceneMan.GetSceneObjByGo(this.gameObject) as DragArticleAgent;
//		if (null != dragItem)
//		{
//			DragArticleAgent.Destory(dragItem.id);

//			if (dragItem.itemDrag != null)
//			{
//				ItemMgr.Instance.DestroyItem(dragItem.itemDrag.itemObj.instanceId);
//			}
//		}

//		this.Explode();
//	}

//	protected void InitSkillCmpt()
//	{
//		m_SkillCmpt = Entity.GetCmpt<SkAliveEntity>();
//		if (m_SkillCmpt == null)
//			m_SkillCmpt = Entity.Add<SkAliveEntity>();

//		m_SkillCmpt.SetAttribute( AttribType.HpMax,MaxHP);
//		m_SkillCmpt.SetAttribute( AttribType.Hp,HP);
//		//m_SkillCmpt.SetAttribute( AttribType.Energy, m_CreationData.m_Fuel);
//		//m_SkillCmpt.SetAttribute( AttribType.EnergyMax,m_CreationData.m_Attribute.m_MaxFuel);

//		m_SkillCmpt.AddAttrListener(OnAttrChange);
//	}


////	protected void ChangeHp(  )
////	{
////
////	}


//	protected void OnAttrChange(AttribType attr, float oldValue, float newValue)
//	{
//		switch (attr)
//		{
//		case AttribType.Hp: 
//		{
//			HP = m_SkillCmpt.GetAttribute(AttribType.Hp);
//			if ( HP<0 && !m_IsWreckage)
//				OnCrash();
//		}break;
//		default:break;
//		}
//	}

//}
