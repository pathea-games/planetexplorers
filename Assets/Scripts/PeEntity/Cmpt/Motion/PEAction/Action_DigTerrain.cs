using UnityEngine;
using System;
using System.Collections.Generic;
using ItemAsset;
using NaturalResAsset;

namespace Pathea
{
	[Serializable]
	public class Action_DigTerrain : PEAction
	{
		public override PEActionType ActionType { get { return PEActionType.Dig; } }

		public HumanPhyCtrl		m_PhyCtrl;

		PEDigTool	m_DigTool;
		public PEDigTool digTool
		{
			get { return m_DigTool; }
			set
			{
				if(null == value)
					motionMgr.EndImmediately(ActionType);
				m_DigTool = value;
			}
		}

		public PECrusher crusher { get { return digTool as PECrusher; } }

		public float m_MaxDigDis = 3f;

		public Vector3 digPos
		{
			private set{ m_CurrentPos = value; }
			get{ return m_DigPos; }
		}

		Vector3 m_CurrentPos;
		Vector3 m_DigPos;

		bool m_CanDig;
		bool m_EndDig;
		bool m_EndAnim;

		UTimer m_FixTimer;

		string m_AnimName;

		bool m_FirstDig;

		public Action_DigTerrain()
		{
			m_FixTimer = new UTimer();
			m_FixTimer.ElapseSpeed = -1f;
		}

		public void UpdateDigPos()
		{
			if(null != digTool && motionMgr.Entity == Pathea.PeCreature.Instance.mainPlayer)
			{
				if(null != digTool.m_Indicator)
				{
					digTool.m_Indicator.show = motionMgr.GetMaskState(digTool.m_HandChangeAttr.m_HoldActionMask) && digTool.durability > PETools.PEMath.Epsilon;
					digTool.m_Indicator.disEnable = (Vector3.Distance(digTool.m_Indicator.digPos, trans.position + Vector3.up) < m_MaxDigDis);
					m_CanDig = digTool.m_Indicator.active;
					m_CurrentPos = digTool.m_Indicator.digPos;
				}
				else
					m_CanDig = false;
			}
		}

		public override bool CanDoAction (PEActionParam para = null)
		{
			if(null == digTool || !m_CanDig)
				return false;
			if(digTool.durability <= PETools.PEMath.Epsilon)
			{
				motionMgr.Entity.SendMsg(EMsg.Action_DurabilityDeficiency);
				return false;
			}

			return base.CanDoAction(para);
		}

		public override void DoAction (PEActionParam para = null)
		{
			if(null != digTool)
			{
				motionMgr.SetMaskState(digTool.m_DigMask, true);
				m_AnimName = digTool.m_AnimName;

				anim.ResetTrigger("ResetFullBody");
				anim.SetBool(m_AnimName, true);

				m_EndDig = false;
				m_EndAnim = false;
			}
			m_DigPos = m_CurrentPos;


			if(null != m_PhyCtrl)
				m_PhyCtrl.m_SubAcc = Vector3.down;

			m_FirstDig = true;

			ApplyStaminaCost();
		}

		public override bool Update ()
		{
			if(null == m_DigTool)
			{
				motionMgr.EndImmediately(ActionType);
				return true;
			}

			if(m_EndDig)
			{
				m_FixTimer.Update(Time.deltaTime);
				if(m_EndAnim || m_FixTimer.Second <= 0)
				{
					OnEndAction();
					return true;
				}
			}
			
			if(null != crusher)
				crusher.UpdateEnCost();			
			skillCmpt._lastestTimeOfConsumingStamina = Time.time;
			return false;
		}

		public override void EndAction ()
		{
			anim.SetBool(m_AnimName, false);
			m_EndDig = true;
			m_FixTimer.Second = 5f;
		}

		public override void EndImmediately ()
		{
			anim.SetBool(m_AnimName, false);
			anim.SetTrigger("ResetFullBody");
			OnEndAction();
		}

		void OnEndAction()
		{
			anim.speed = 1f;
			motionMgr.SetMaskState(PEActionMask.Dig, false);
			motionMgr.SetMaskState(PEActionMask.DrillingDig, false);
			if(null != m_PhyCtrl)
				m_PhyCtrl.m_SubAcc = Vector3.zero;
		}

		void DigTerrain()
		{
			if((m_FirstDig || m_CanDig) && null != digTool && null != skillCmpt && null != packageCmpt)
			{
				if(!m_FirstDig)
					m_DigPos = m_CurrentPos;
				if(null != skillCmpt && 0 != digTool.m_SkillID)
					skillCmpt.StartSkill(VFVoxelTerrain.self, digTool.m_SkillID);
				
				motionMgr.Entity.SendMsg(EMsg.Battle_EquipAttack, digTool.m_ItemObj);

				if(digTool.durability <= PETools.PEMath.Epsilon)
				{
					motionMgr.EndAction(ActionType);
					motionMgr.Entity.SendMsg(EMsg.Action_DurabilityDeficiency);
				}

				if(!m_EndDig)
					ApplyStaminaCost();
			}
			m_FirstDig = false;
		}
		
		void ApplyStaminaCost()
		{
			if(null == digTool) return;
			float stamina = entity.GetAttribute(AttribType.Stamina) - digTool.m_StaminaCost * entity.GetAttribute(AttribType.StaminaReducePercent);
			entity.SetAttribute(AttribType.Stamina, stamina, false);
			float animSpeed = digTool.m_AnimSpeed;
			for(int i = 0; i < digTool.m_AnimDownThreshold.Length; ++i)
				if(stamina <= digTool.m_AnimDownThreshold[i])
					animSpeed *= 0.9f;
			anim.speed = Mathf.Clamp(animSpeed, 0.5f, 1.5f);
		}

		protected override void OnAnimEvent (string eventParam)
		{
			switch(eventParam)
			{
			case "DigTerrain":
				DigTerrain();
				break;
			case "DigEnd":
			case "OnEndFullAnim":
				m_EndAnim = true;
				break;
			case "ChangeTarget":
				if(!m_FirstDig && m_CanDig)
					m_DigPos = m_CurrentPos;
				break;
			}
		}
	}
}
