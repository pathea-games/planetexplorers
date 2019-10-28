using UnityEngine;

namespace Pathea
{
	[System.Serializable]
	public class Action_Throw : PEAction 
	{
		public override PEActionType ActionType { get { return PEActionType.Throw; } }		
		public SkillSystem.SkEntity targetEntity{ get; set; }
		PEGrenade m_Grenade;
		public PEGrenade grenade
		{
			get{ return m_Grenade; }
			set
			{
				if(null == value)
					motionMgr.EndImmediately(ActionType);
				m_Grenade = value;
			}
		}

		bool m_StartThrow;
		bool m_AnimEnd;
		bool m_Throwed;

		public override bool CanDoAction (PEActionParam para = null)
		{
			return null != grenade;
		}

		public override void DoAction (PEActionParam para = null)
		{
			m_AnimEnd = false;
			if(null != anim)
			{
				anim.ResetTrigger("ResetUpbody");
				anim.SetTrigger("ThrowGrenade");
			}
			m_StartThrow = false;
			m_Throwed = false;
			
			if(null != grenade && null != grenade.m_AttackRanges)
				motionMgr.Entity.SendMsg(EMsg.Battle_OnAttack, grenade.m_AttackRanges, grenade.transform, grenade.ItemObj.protoId);
		}

		public override void ResetAction (PEActionParam para = null)
		{
			base.ResetAction (para);
		}

		public override bool Update ()
		{
			if(null == anim || null == viewCmpt || !viewCmpt.hasView || null == grenade)
				return true;

			if(m_StartThrow)
			{
				m_StartThrow = false;
				ThrowGrenade();
			}

			if(m_AnimEnd)
			{
				motionMgr.SetMaskState(PEActionMask.Throw, false);
				
				if(null != grenade.m_Model)
				{
					grenade.m_Model.SetActive(true);
				}
				if(null != grenade && 0 == grenade.m_ItemObj.stackCount && null != equipCmpt)
				{
					int itemID = grenade.m_ItemObj.instanceId;
					equipCmpt.TakeOffEquipment(grenade.m_ItemObj, false);
					ItemAsset.ItemMgr.Instance.DestroyItem(itemID);
				}
				return true;
			}
			return false;
		}

		public override void EndImmediately ()
		{
			motionMgr.SetMaskState(PEActionMask.Throw, false);
			if(null != anim)
			{
				anim.SetTrigger("ResetUpbody");
				anim.ResetTrigger("ThrowGrenade");
			}
		}

		void ThrowGrenade()
		{
			if(null != grenade && null != skillCmpt)
			{
				int itemCost = grenade.itemCost ? 1 : 0;
				if (GameConfig.IsMultiMode)
				{
					PlayerNetwork.mainPlayer.RequestThrow(motionMgr.Entity.Id, grenade.m_ItemObj.instanceId, itemCost);
				}
				else
				{
					grenade.m_ItemObj.stackCount -= itemCost;
				}

				SkillSystem.ShootTargetPara target = new SkillSystem.ShootTargetPara();
				if(null != ikCmpt)
					target.m_TargetPos = ikCmpt.aimTargetPos;
				else
					target.m_TargetPos = motionMgr.Entity.position + motionMgr.Entity.forward;

				skillCmpt.StartSkill(targetEntity, grenade.m_SkillID, target);

				if(null != grenade.m_Model)
				{
					grenade.m_Model.SetActive(false);
				}
			}
		}

		protected override void OnAnimEvent (string eventParam)
		{
			if(motionMgr.IsActionRunning(ActionType))
			{
				if(!m_Throwed && "ThrowGrenade" == eventParam)
				{
					m_StartThrow = true;
					m_Throwed = true;
				}
				else if("ThrowAnimEnd" == eventParam)
					m_AnimEnd = true;
			}
		}
	}
}
