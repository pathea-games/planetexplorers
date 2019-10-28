using UnityEngine;
using System.Collections;

public class PEHelmet : PEEquipment
{
	public string m_AttachBoneName = "Bip01 Head";

	public override void InitEquipment (Pathea.PeEntity entity, ItemAsset.ItemObject itemObj)
	{
		base.InitEquipment (entity, itemObj);
		m_View.AttachObject(gameObject, m_AttachBoneName);
	}
}
