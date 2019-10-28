using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ItemAsset;
using SkillAsset;

//public abstract partial class CreationSkillRunner : SkillRunner
//{
//	protected CreationData m_CreationData;
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
//		set { }
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
//	}
//	protected virtual void OnCrash ()
//	{
//		DragArticleAgent.Destory(ObjectID);
//		ItemMgr.Instance.DestroyItem(ObjectID);
//		this.Explode();
//	}
	
//	internal override byte GetBuilderId() { return 0; }
//	internal override float GetAtkDist(ISkillTarget target) { return 1f; }
////	internal override float GetAtk() { return 0f; }
////	internal override float GetDef() { return 0f; }
////	internal override float GetBaseAtk() { return 0f; }
////	internal override float GetDurAtk(ESkillTargetType resType) { return 0f; }
////	internal override short GetResMultiplier() { return 0; }
//	internal override ItemPackage GetItemPackage() { return null; }
//	internal override bool IsEnemy(ISkillTarget target) { return true; }
//	internal override ISkillTarget GetTargetInDist(float dist, int targetMask) { return null; }
//	internal override List<ISkillTarget> GetTargetlistInScope(EffScope scope, int targetMask, ISkillTarget target) { return null; }
//	internal override void ApplyDistRepel(SkillRunner caster, float distRepel) {}
//	internal override void ApplyComfortChange(float comfortChange) {}
//	internal override void ApplySatiationChange(float satiationChange) {}
//	internal override void ApplyThirstLvChange(float thirstLvChange) {}
////	internal override void ApplyBuffPermanent(EffSkillBuff buff) {}
//	internal override void ApplyAnim(List<string> animName) {}
	
//	internal override Transform GetCastTransform (SkillAsset.EffItemCast cast)
//	{
//		if ( cast == null )
//			return transform;
//		else
//			return VCUtils.GetChildByName(transform, cast.m_castPosName);
//	}
//}
