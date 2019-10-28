using UnityEngine;
using System.Collections;
using Pathea;

public class PECtrlAbleEquipment : PEEquipment
{
	protected Motion_Equip 	m_MotionEquip;
	protected MotionMgrCmpt	m_MotionMgr;
	public PEActionType[] 	m_RemoveEndAction;

	ItemAsset.Durability	m_Durability;
	public float durability{ get { return (null != m_Durability) ? m_Durability.floatValue.current : 100f; } }

	public override void InitEquipment (PeEntity entity, ItemAsset.ItemObject itemObj)
	{
		base.InitEquipment (entity, itemObj);
		m_MotionEquip = m_Entity.GetCmpt<Motion_Equip>();
		m_MotionMgr = m_Entity.GetCmpt<MotionMgrCmpt>();
		m_Durability = itemObj.GetCmpt<ItemAsset.Durability>();
	}

	public override bool CanTakeOff ()
	{
		if(null != m_MotionMgr)
		{
			foreach(PEActionType actionType in m_RemoveEndAction)
			{
				if(m_MotionMgr.IsActionRunning(actionType))
					return false;
			}
		}
		return true;
	}
}
