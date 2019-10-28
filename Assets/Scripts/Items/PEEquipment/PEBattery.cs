using UnityEngine;
using Pathea;

public class PEBattery : PEEquipment 
{
	const string	AttachBone = "Bip01 L Thigh";

	public override void InitEquipment (Pathea.PeEntity entity, ItemAsset.ItemObject itemObj)
	{
		base.InitEquipment (entity, itemObj);
		BiologyViewCmpt view = m_Entity.biologyViewCmpt;
		if(null != view)
			view.AttachObject(gameObject, AttachBone);
	}
}
