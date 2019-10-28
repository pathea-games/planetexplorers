using UnityEngine;
using System.Collections;
using Pathea;

public class PEPujaGun : PEGun
{
	public string	m_AimAnim;
	AnimatorCmpt 	m_Anim;
	bool 			m_AimState;

	public override void InitEquipment (PeEntity entity, ItemAsset.ItemObject itemObj)
	{
		base.InitEquipment (entity, itemObj);
		m_Anim = m_Entity.GetCmpt<AnimatorCmpt>();
	}

	public override void SetAimState(bool aimState)
	{
		m_AimState = aimState;
		if(m_AimState)
			m_MotionMgr.ContinueAction(m_HandChangeAttr.m_ActiveActionType, m_HandChangeAttr.m_ActiveActionType);
		else
			m_MotionMgr.PauseAction(m_HandChangeAttr.m_ActiveActionType, m_HandChangeAttr.m_ActiveActionType);

		if(null != m_Anim && "" != m_AimAnim)
		{
			m_Anim.SetBool(m_AimAnim, aimState);
			if(PeGameMgr.IsMulti && null != m_Entity.netCmpt && !m_Entity.netCmpt.network.hasOwnerAuth)
			{
				AiNetwork aiNetWork = m_Entity.netCmpt.network as AiNetwork;
				if(null != aiNetWork)
					aiNetWork.RequestSetBool(Animator.StringToHash(m_AimAnim), aimState);
			}
		}
	}

	public override void HoldWeapon (bool hold)
	{
		base.HoldWeapon (hold);
		SetAimState(hold);
	}

	public override bool Aimed {
		get {
			return base.Aimed && m_AimState;
		}
	}
}
