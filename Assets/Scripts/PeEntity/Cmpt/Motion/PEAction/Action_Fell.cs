using UnityEngine;
using System.Collections.Generic;
using NaturalResAsset;

namespace Pathea
{
	[System.Serializable]
	public class Action_Fell : PEAction
	{
		public override PEActionType ActionType { get { return PEActionType.Fell; } }

		public PEAxe	m_Axe;

		public event System.Action<TreeInfo> startFell;
		public event System.Action endFell;
		public event System.Action<TreeInfo, float> hpChange;

		bool m_EndAction;
		bool m_EndAnim;

		const float MaxHp = 255f;

		GlobalTreeInfo mOpTreeInfo;

		GlobalTreeInfo mFindTreeInfo;

		float m_FixTime;

		float m_MSGCD = 1f;
		float m_LastShowTime;

		const float FellDis = 2f;

		public GlobalTreeInfo treeInfo{ get { return mOpTreeInfo; } set { mOpTreeInfo = mFindTreeInfo = value;}}

		public bool UpdateOPTreeInfo()
		{
			if(null == m_Axe || m_Axe.durability <= PETools.PEMath.Epsilon)
				return false;
			mFindTreeInfo = null;

			GlobalTreeInfo gTreeinfo = PETools.PEUtil.RayCastTree(PeCamera.mouseRay.origin - 0.5f * PeCamera.mouseRay.direction, PeCamera.mouseRay.direction, 100f);
			GlobalTreeInfo playerTree = PETools.PEUtil.RayCastTree(trans.existent.position + Vector3.up, trans.existent.forward, FellDis);
			if(null == gTreeinfo || null == playerTree  || gTreeinfo._treeInfo != playerTree._treeInfo)
				return false;
			
			if (gTreeinfo != null)
			{
				NaturalRes resFind = NaturalResAsset.NaturalRes.GetTerrainResData(gTreeinfo._treeInfo.m_protoTypeIdx + 1000);
				if (null != resFind)
				{
					if(resFind.m_type == 9)
						mFindTreeInfo = gTreeinfo;
				}
			}

			return null != mFindTreeInfo;
		}

		public override bool CanDoAction (PEActionParam para = null)
		{
			if(null == m_Axe || null == trans)
				return false;

			if(m_Axe.durability <= PETools.PEMath.Epsilon)
			{
				if(Time.time - m_LastShowTime >= m_MSGCD)
				{
					m_LastShowTime = Time.time;
					motionMgr.Entity.SendMsg(EMsg.Action_DurabilityDeficiency);
				}
				return false;
			}

			if(null == mFindTreeInfo)
				return false;

			return base.CanDoAction(para);
		}

		public override void PreDoAction ()
		{
			base.PreDoAction ();
			mOpTreeInfo = mFindTreeInfo;
		}

		public override void DoAction (PEActionParam para = null)
		{
			if(null == m_Axe)
				return;
			
			motionMgr.SetMaskState(PEActionMask.Fell, true);

			m_EndAction = false;
			m_EndAnim = false;

			if(null != anim)
			{
				anim.ResetTrigger("ResetFullBody");
				anim.SetBool(m_Axe.fellAnim, true);
			}

			m_FixTime = 3f;

			ApplyStaminaCost();

			if(null != startFell)
				startFell(treeInfo._treeInfo);
			if(null != hpChange)
				hpChange(treeInfo._treeInfo, SkEntitySubTerrain.Instance.GetTreeHP(treeInfo.WorldPos) / MaxHp);
		}

//		public override void ResetAction (IPEActionParam para = null)
//		{
//			if(m_EndAction && null != SkEntitySubTerrain.Instance && null != treeInfo
//			   && SkEntitySubTerrain.Instance.GetTreeHP(treeInfo.WorldPos) > 0
//			   && null != m_Axe && m_Axe.durability > 0 )
//				DoAction (para);
//		}

		public override bool Update ()
		{
			m_FixTime -= Time.deltaTime;
			skillCmpt._lastestTimeOfConsumingStamina = Time.time;
			if(null != anim)
			{
				if(m_EndAction && (m_EndAnim || m_FixTime < 0))
				{
					OnEndAction();
					return true;
				}
			}
			else if(m_EndAction)
			{
				OnEndAction();
				return true;
			}
			return false;
		}

		public override void EndAction ()
		{
			m_EndAction = true;
			if(null != anim && null != m_Axe)
				anim.SetBool(m_Axe.fellAnim, false);

		}

		public override void EndImmediately ()
		{
			if(null != anim && null != m_Axe)
			{
				anim.SetTrigger("ResetFullBody");
				anim.SetBool(m_Axe.fellAnim, false);
			}
			OnEndAction();
		}

		void OnEndAction()
		{
			motionMgr.SetMaskState(PEActionMask.Fell, false);
			if(null != m_Axe && m_Axe.durability <= PETools.PEMath.Epsilon)
				motionMgr.EndAction(PEActionType.EquipmentHold);
			if(null != endFell)
				endFell();
			anim.speed = 1f;
		}

		void FellTree()
		{
			if(null != treeInfo && null != skillCmpt && null != m_Axe)
			{
				if(SkEntitySubTerrain.Instance.GetTreeHP(treeInfo.WorldPos) <= PETools.PEMath.Epsilon)
				{
					motionMgr.EndAction(ActionType);
					if(null != hpChange)
						hpChange(treeInfo._treeInfo, 0);
					return;
				}

				if(null != skillCmpt && 0 != m_Axe.m_FellSkillID)
					skillCmpt.StartSkill(SkEntitySubTerrain.Instance, m_Axe.m_FellSkillID);
				if(null != hpChange)
					hpChange(treeInfo._treeInfo, SkEntitySubTerrain.Instance.GetTreeHP(treeInfo.WorldPos) / MaxHp);
				if(SkEntitySubTerrain.Instance.GetTreeHP(treeInfo.WorldPos) <= 0
				   || m_Axe.durability <= PETools.PEMath.Epsilon)
					motionMgr.EndAction(ActionType);
				motionMgr.Entity.SendMsg(EMsg.Battle_EquipAttack, m_Axe.ItemObj);
			}
			m_FixTime = 3f;
			if(!m_EndAction)
				ApplyStaminaCost();
		}

		void ApplyStaminaCost()
		{
			if(null == m_Axe) return;
			float stamina = entity.GetAttribute(AttribType.Stamina) - m_Axe.m_StaminaCost * entity.GetAttribute(AttribType.StaminaReducePercent);
			entity.SetAttribute(AttribType.Stamina, stamina, false);
			float animSpeed = m_Axe.m_AnimSpeed;
			for(int i = 0; i < m_Axe.m_AnimDownThreshold.Length; ++i)
				if(stamina <= m_Axe.m_AnimDownThreshold[i])
					animSpeed *= 0.9f;
			anim.speed = Mathf.Clamp(animSpeed, 0.5f, 1.5f);
		}

		protected override void OnAnimEvent (string eventParam)
		{
			if(motionMgr.IsActionRunning(ActionType))
			{
				switch(eventParam)
				{
				case "Fell":
					FellTree();
					break;
				case "FellEnd":
				case "OnEndFullAnim":
					m_EndAnim = true;
					break;
				}
			}
		}
	}
}