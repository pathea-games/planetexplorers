using UnityEngine;
using System;

namespace Pathea
{
	[Serializable]
	public class Action_SheildHold : PEAction, iEquipHideAbleAction
	{
		public override PEActionType ActionType { get {	return PEActionType.HoldShield; } }

		const int SheidBuffID = 30200177;
		
		PESheild m_Sheild;
		
		public PESheild sheild
		{
			get{ return m_Sheild; }
			set{ if(null == value) motionMgr.EndImmediately(ActionType); m_Sheild = value; }
		}

		public override bool CanDoAction (PEActionParam para = null)
		{
			return null != sheild;
		}
		
		public override void DoAction (PEActionParam para = null)
		{
			if(null == sheild || null == anim)
				return;
			anim.SetBool(PESheild.HoldSheildAnim, true);
			motionMgr.SetMaskState(PEActionMask.HoldShield, true);
//			ItemAsset.Equip equip = sheild.m_ItemObj.GetCmpt<ItemAsset.Equip>();
//			equip.AddMotionBuff(skillCmpt);
			SkillSystem.SkEntity.MountBuff(skillCmpt, SheidBuffID, null, null);
			if(null != move && !motionMgr.IsActionRunning(PEActionType.EquipmentHold))
				move.style = MoveStyle.Sword;
			if(null != skillCmpt)
				skillCmpt.SetAttribute(AttribType.EnableShieldBlock, 1);
			if(null != sheild && hideEquipInactive)
				sheild.gameObject.SetActive(true);
		}
		
		public override bool Update ()
		{
			if(null == sheild || null == anim)
				return true;
			if(null != move && !motionMgr.IsActionRunning(PEActionType.EquipmentHold))
				move.style = MoveStyle.Sword;
			return false;
		}
		
		public override void EndImmediately ()
		{
			motionMgr.SetMaskState(PEActionMask.HoldShield, false);
			if(null != anim)
				anim.SetBool(PESheild.HoldSheildAnim, false);
			if(null != skillCmpt)
				skillCmpt.SetAttribute(AttribType.EnableShieldBlock, 0);
			if(null != move && !motionMgr.IsActionRunning(PEActionType.EquipmentHold))
				move.style = move.baseMoveStyle;
			if(null != sheild && hideEquipInactive)
				sheild.gameObject.SetActive(false);
			if(null != skillCmpt)
				skillCmpt.CancelBuffById(SheidBuffID);
		}

		#region iEquipHideAbleAction implementation
		bool m_HideEquipInactive;

		public bool hideEquipInactive 
		{
			get { return m_HideEquipInactive; }
			set 
			{
				m_HideEquipInactive = value;
				if(null != sheild)
					sheild.gameObject.SetActive(!m_HideEquipInactive);
			}
		}

		#endregion
	}
}
