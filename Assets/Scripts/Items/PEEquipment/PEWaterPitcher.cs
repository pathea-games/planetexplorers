using UnityEngine;
using System.Collections;

public class PEWaterPitcher : PECtrlAbleEquipment
{
	const string AttachBone = "mountMain"; 
	public override void InitEquipment (Pathea.PeEntity entity, ItemAsset.ItemObject itemObj)
	{
		base.InitEquipment (entity, itemObj);
		m_View.AttachObject(gameObject, AttachBone);
	}
}
