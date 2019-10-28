//using UnityEngine;
//using System.Collections;
//using System.Collections.Generic;
//using ItemAsset;
//using SkillAsset;
//
//
//[RequireComponent (typeof (CSEntityObject))]
//
//public class ColonyRunner : AiObject
//{
//	public CSEntity m_Entity;
//	
//	
//	// Defence type
//	[HideInInspector] public int m_DefenceType = 5;
//	
//	#region UNITY_INTERNAL_FUNC
//	// Use this for initialization
//	protected new void Start () 
//	{
//		// Check return
//		if ( GameConfig.IsInVCE ) return;
//		// Add to ai object
//        //AiManager.Manager.RegisterAiObject(this);
//		// Set camp
//		m_camp = 1;
//		m_harm = 1;
//		// Set defence type
//		m_DefenceType = 5;
//		
//	}
//	
//	// Update is called once per frame
//	protected new void Update()
//	{	
//		if (m_Entity == null)
//			return;
//		
//		if (m_Entity.BaseData.m_Durability <= 0F  && !m_isDead )
//		{
//			OnDeath();
//		}
//		
//	}
//	#endregion
//	
//	#region SKILL_RUNNER_FUNC
//	// Apply HP Change
////	internal override void ApplyHpChange ( SkillRunner caster, float hpChange, float damagePercent, int type )
////	{
////		// Check return
////		if ( caster == this.gameObject ) return;
////		
////		if (m_Entity == null)	return;
////		
////		string logstring = "";
////		// Calculate damage
////		float damage = 0;
////		if ( caster == null )
////		{
////			damage = hpChange * damagePercent;
////			if ( damage > 2 )
////				logstring = "damage = " + damage.ToString("0.0") + " type = " + type.ToString();
////
////			m_Entity.OnDamaged(null, damage);
////		}
////		else
////		{
////			damage = caster.GetAttribute(Pathea.AttribType.Atk) * damagePercent + hpChange;
////			damage *= Random.Range(0.9f, 1.1f);
////			logstring = "basedamage = " + damage.ToString("0.0");
////			int damageType = type == 0 ? AiAsset.AiDamageTypeData.GetDamageType(caster) : type;
////			float damagescale = AiAsset.AiDamageTypeData.GetDamageScale(damageType, m_DefenceType);
////			damage *= damagescale;
////			logstring += " damage = " + damage.ToString("0.0") + " scale = " + damagescale.ToString("0.00") + " type = " + damageType.ToString();
////
////			m_Entity.OnDamaged(caster.gameObject, damage);
////
////		}
////
//////		m_Entity.BaseData.m_Durability -= damage;
////////		m_Entity.ExcuteEvent(CSConst.eetHurt);
//////		m_Entity.SetHealthState(CSConst.ehtHurt);
////
////		//HPChangeGUI.ShowHPChange(damage, transform.position, HPChangeGUI.Instance.CreationHurt);
////		
////		// Log
////		if ( Application.isEditor && logstring.Length > 1 )
////			Debug.Log("CreationRunner::ApplyHpChange  " + logstring);
////
////	}
//	
//	// ColonyRunner On death
//	public new void OnDeath()
//	{
////		m_Entity.BaseData.m_Durability = -0.001f;
//		m_isDead = true;
//		
//		//DrawItemManager.Instance.RemoveWithObject(gameObject);
//		//m_Entity.DestroySelf();
//		m_Entity.m_Creator.RemoveEntity(m_Entity.ID);
//	}
//	#endregion
//}
