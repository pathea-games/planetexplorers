using UnityEngine;
using System.Collections;
using Pathea;

public class PEAttachEquip : PEEquipment
{
	public string m_AttachBoneName = "mountMain";
	
	public override void InitEquipment (Pathea.PeEntity entity, ItemAsset.ItemObject itemObj)
	{
		base.InitEquipment (entity, itemObj);
		m_View.AttachObject(gameObject, m_AttachBoneName);
	}
}