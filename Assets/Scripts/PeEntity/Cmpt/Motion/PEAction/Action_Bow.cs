using UnityEngine;
using System;

namespace Pathea
{
	[Serializable]
	public class Action_BowHold : Action_AimEquipHold
	{
		public override PEActionType ActionType { get { return PEActionType.BowHold; } }

		PEBow		m_Bow;
		public PEBow bow
		{
			get { return m_Bow; }
			set 
			{
				aimAbleEquip = value;
				m_Bow = value;
			}
		}

		public bool   	m_IgnoreItem = false;

		public override bool CanDoAction (PEActionParam para = null)
		{
			if(m_IgnoreItem)
				return true;

			return null != packageCmpt && null != bow && packageCmpt.GetItemCount(bow.curItemID) > 0;
		}

		public override void OnModelBuild ()
		{
			base.OnModelBuild ();
			if(null != bow)
			{
				bow.SetArrowShowState(true);
				bow.SetBowOpenState(true);
			}
		}
		
		public override void EndAction ()
		{
			if(null == anim || null == bow)
				return;
			base.EndAction();
			bow.SetBowOpenState(false);
			bow.SetArrowShowState(false);
		}
		
		public override void EndImmediately ()
		{
			base.EndImmediately();
			if(null != bow)
			{
				bow.SetArrowShowState(false);
				bow.SetBowOpenState(false);
			}
		}
		
		public override void ChangeHoldState(bool hold, bool checkState = true, bool isReattach = true)
		{
			base.ChangeHoldState(hold, checkState, isReattach);
			if(!hold)
			{
				if(null != bow)
				{
					bow.SetArrowShowState(false);
					bow.SetBowOpenState(false);
				}
			}
		}

		protected override void OnAnimEvent (string eventParam)
		{
			base.OnAnimEvent (eventParam);

			if(null != bow && motionMgr.IsActionRunning(ActionType))
			{
				switch(eventParam)
				{
				case "ShowArrow":
					bow.SetArrowShowState(true);
					break;
				case "OpenBow":
					bow.SetBowOpenState(true);
					break;
				case "CloseBow":
					bow.SetBowOpenState(false);
					break;
				}
			}
		}
	}
	
	[Serializable]
	public class Action_BowPutOff : PEAction
	{
		public override PEActionType ActionType { get { return PEActionType.BowPutOff; } }
		
		public override void DoAction (PEActionParam para = null)
		{
			motionMgr.EndAction(PEActionType.BowHold);
		}
	}
	
	[Serializable]
	public class Action_BowShoot : PEAction
	{
		public override PEActionType ActionType { get { return PEActionType.BowShoot; } }
		public IKAimCtrl		ikAim { get; set; }
		public SkillSystem.SkEntity targetEntity{ get; set; }
		
		PEBow		m_Bow;
		public PEBow bow
		{
			get { return m_Bow; }
			set { if(null == value) motionMgr.EndImmediately(ActionType); m_Bow = value;}
		}

		public bool   	m_IgnoreItem = false;
		
		public override bool CanDoAction (PEActionParam para = null)
		{
			if(null == skillCmpt || null == bow || skillCmpt.IsSkillRunning(bow.skillID))
				return false;
			if(bow.durability <= PETools.PEMath.Epsilon)
			{
				motionMgr.Entity.SendMsg(EMsg.Action_DurabilityDeficiency);
				return false;
			}

			return base.CanDoAction(para);
		}
		
		public override void DoAction (PEActionParam para = null)
		{
			if(null == skillCmpt || null == bow)
				return;

			SkillSystem.ShootTargetPara target = new SkillSystem.ShootTargetPara();
			if(null != ikAim)
				target.m_TargetPos = ikAim.targetPos;
			else
				target.m_TargetPos = motionMgr.transform.forward;

			if(null != skillCmpt.StartSkill(targetEntity, bow.skillID, target))
			{
                if (GameConfig.IsMultiMode)
                {
                    NetworkInterface net = NetworkInterface.Get(motionMgr.Entity.Id);
                    if (null != net && net.hasOwnerAuth)
                    {
                        if (!m_IgnoreItem)
                            PlayerNetwork.mainPlayer.RequestItemCost(motionMgr.Entity.Id, bow.curItemID, 1);
                    }
                }
                else
                {
                    if (null != packageCmpt && !m_IgnoreItem)
                        packageCmpt.Destory(bow.curItemID, 1);
                }
					
				bow.SetArrowShowState(false);
				bow.SetBowOpenState(false);

				motionMgr.Entity.SendMsg(EMsg.Battle_EquipAttack, bow.ItemObj);
				
				if(null != bow.m_AttackMode)
					motionMgr.Entity.SendMsg(EMsg.Battle_OnAttack, bow.m_AttackMode[0], bow.transform, bow.curItemID);
			}
		}
		
		public override bool Update ()
		{
			if(null == skillCmpt || null == bow)
				return true;
			
			if( GameConfig.IsMultiMode )
			{
				NetworkInterface net = NetworkInterface.Get(motionMgr.Entity.Id);
				if (null != net && !net.hasOwnerAuth)
					return true;
			}

			if(!skillCmpt.IsSkillRunning(bow.skillID))
			{
				if(m_IgnoreItem ||	null != packageCmpt && packageCmpt.GetItemCount(bow.curItemID) > 0)
				{
					PEActionParamN param = PEActionParamN.param;
					param.n = bow.curItemIndex; 
					motionMgr.DoAction(PEActionType.BowReload, param);
				}
				else
					motionMgr.DoAction(PEActionType.BowPutOff);
				
				bow.OnShoot();
				return true;
			}

			return true;
		}
		
//		public override void EndImmediately ()
//		{
//			if(null != anim)
//				anim.SetTrigger("ResetUpbody");
//		}
	}

	
	[Serializable]
	public class Action_BowReload : PEAction
	{
		public override PEActionType 	ActionType { get { return PEActionType.BowReload; } }
		
		PEBow		m_Bow;
		public PEBow bow
		{
			get { return m_Bow; }
			set { if(null == value) motionMgr.EndImmediately(ActionType); m_Bow = value;}
		}
		
		int				m_TargetAmmoIndex;
		AudioController	m_Audio;

		bool 			m_AnimEnd;

		public bool   	m_IgnoreItem = false;
		
		public override bool CanDoAction (PEActionParam para = null)
		{
			PEActionParamN paramN = para as PEActionParamN;
			int targetAmmIndex = paramN.n;

			if(m_IgnoreItem)
				return null != bow && null != packageCmpt;
			
			return null != bow && null != packageCmpt && (targetAmmIndex != bow.curItemIndex || packageCmpt.GetItemCount(bow.curItemID) > 0);
		}
		
		public override void DoAction (PEActionParam para = null)
		{
			motionMgr.SetMaskState(PEActionMask.BowReload, true);
			PEActionParamN paramN = para as PEActionParamN;
			m_TargetAmmoIndex = paramN.n;
			if(null != bow && null != anim && motionMgr.IsActionRunning(PEActionType.BowHold))
			{
				anim.SetTrigger(bow.m_ReloadAnim);
				m_AnimEnd = false;
			}
			if(null != bow && null != ikCmpt && null != ikCmpt.m_IKAimCtrl)
				ikCmpt.m_IKAimCtrl.StartSyncAimAxie();
		}
		
		public override bool Update ()
		{
			if(null == bow || null == anim || motionMgr.GetMaskState(PEActionMask.InWater))
			{
				motionMgr.SetMaskState(PEActionMask.BowReload, false);
				return true;
			}
			if(null == anim || m_AnimEnd)
			{
				motionMgr.SetMaskState(PEActionMask.BowReload, false);
				bow.curItemIndex = m_TargetAmmoIndex;
				if(null != ikCmpt && null != ikCmpt.m_IKAimCtrl)
					ikCmpt.m_IKAimCtrl.EndSyncAimAxie();
				return true;
			}
			return false;
		}
		
		public override void EndImmediately ()
		{
			motionMgr.SetMaskState(PEActionMask.BowReload, false);
			if(null != anim)
			{
				if(null != bow)
					anim.ResetTrigger(bow.m_ReloadAnim);
				anim.SetTrigger("ResetUpbody");
			}
			if(null != bow && null != ikCmpt && null != ikCmpt.m_IKAimCtrl)
				ikCmpt.m_IKAimCtrl.EndSyncAimAxie();
			if(null != bow && motionMgr.IsActionRunning(PEActionType.BowHold))
			{
				bow.SetArrowShowState(true);
				bow.SetBowOpenState(true);
			}
		}

		protected override void OnAnimEvent (string eventParam)
		{
			if("ReloadEnd" == eventParam)
				m_AnimEnd = true;
		}
	}
}
