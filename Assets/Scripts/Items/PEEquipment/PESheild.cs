using UnityEngine;
using Pathea;

public class PESheild : PECtrlAbleEquipment 
{
	public const string	HoldSheildAnim = "SheildBlock";
	public string	AttachBoneName = "Bone L Forearm";

	public override void InitEquipment (PeEntity entity, ItemAsset.ItemObject itemObj)
	{
		base.InitEquipment (entity, itemObj);
		if(null != m_View)
			m_View.AttachObject(gameObject, AttachBoneName);
	}
}
