using UnityEngine;
using System.Collections;
using Pathea;

public class PEFlashLight : PECtrlAbleEquipment
{
	public Transform aimTrans;

	public override void InitEquipment (PeEntity entity, ItemAsset.ItemObject itemObj)
	{
		base.InitEquipment (entity, itemObj);
		m_View.AttachObject(gameObject, "mountOff");
	}
}
