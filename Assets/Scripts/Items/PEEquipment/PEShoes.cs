using UnityEngine;
using System.Collections;

public class PEShoes : PEEquipmentLogic 
{
	[SerializeField] float m_SpeedScale = 3f;

	public override void InitEquipment (Pathea.PeEntity entity, ItemAsset.ItemObject itemObj)
	{
		base.InitEquipment (entity, itemObj);
		ResetSpeed (true);
	}

	public override void RemoveEquipment ()
	{
		base.RemoveEquipment ();
		ResetSpeed (false);
	}

	public override void OnModelRebuild ()
	{
		base.OnModelRebuild ();
		ResetSpeed (true);
	}

	void ResetSpeed(bool enable)
	{
		if (null != m_Entity && null != m_Entity.biologyViewCmpt && null != m_Entity.biologyViewCmpt.monoPhyCtrl) 
			m_Entity.biologyViewCmpt.monoPhyCtrl.mSpeedTimes = enable?m_SpeedScale:1f;
	}
}
