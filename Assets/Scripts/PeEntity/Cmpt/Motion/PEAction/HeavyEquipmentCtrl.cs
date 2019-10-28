using UnityEngine;
using Pathea;

public class HeavyEquipmentCtrl
{
	public Motion_Move_Human moveCmpt;

	public HumanPhyCtrl phyCtrl;

	public IKCmpt ikCmpt;

	public MotionMgrCmpt motionMgr;

	IHeavyEquipment m_HeavyEquipment;

	bool m_InWater;
	
	public IHeavyEquipment heavyEquipment
	{
		get
		{
			if(null == m_HeavyEquipment || m_HeavyEquipment.Equals(null))
				return null;
			return m_HeavyEquipment; 
		}
		set
		{
			m_HeavyEquipment = value;
			if(null != moveCmpt)
				moveCmpt.style = (null != m_HeavyEquipment) ? m_HeavyEquipment.baseMoveStyle : moveCmpt.baseMoveStyle;

			if(null != ikCmpt)
				ikCmpt.SetSpineEffectDeactiveState(this.GetType(), null != m_HeavyEquipment);

			if(null != motionMgr)
				motionMgr.SetMaskState(PEActionMask.HeavyEquipment, null != m_HeavyEquipment);
		}
	}

	public void Update () 
	{
		if(null != heavyEquipment && null != phyCtrl)
		{
			if(m_InWater != phyCtrl.spineInWater)
			{
				m_InWater = phyCtrl.spineInWater;
				moveCmpt.style = phyCtrl.spineInWater ? moveCmpt.baseMoveStyle : heavyEquipment.baseMoveStyle;
				heavyEquipment.HidEquipmentByUnderWater(phyCtrl.spineInWater);
			}
		}
	}
}
