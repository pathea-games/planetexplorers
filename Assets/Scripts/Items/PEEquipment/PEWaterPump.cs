using UnityEngine;
using Pathea;

public class PEWaterPump : PEAimAbleEquip 
{
	public Transform mBackPack;
	
	EquipmentActiveEffect m_Effect;
	
	public override void InitEquipment (Pathea.PeEntity entity, ItemAsset.ItemObject itemObj)
	{
		base.InitEquipment (entity, itemObj);
		if(null != mBackPack)
		{
			if(null != m_View)
				m_View.AttachObject(mBackPack.gameObject, "Bow_box");
			else
				mBackPack.gameObject.SetActive(false);
		}
		m_Effect = GetComponent<EquipmentActiveEffect>();
	}
	
	public override void RemoveEquipment ()
	{
		base.RemoveEquipment ();
		if(null != m_View && null != mBackPack)
		{
			m_View.DetachObject(mBackPack.gameObject);
			Destroy(mBackPack.gameObject);
		}
	}

	protected override void UpdateHideState ()
	{
		base.UpdateHideState ();		
		if(null == m_Entity) return;
		MainPlayerCmpt mainCmpt = m_Entity.GetComponent<MainPlayerCmpt>();
		if(null != mainCmpt)
			mBackPack.gameObject.SetActive(!mainCmpt.firstPersonCtrl);
	}
		
	public override void SetActiveState (bool active)
	{
		base.SetActiveState (active);
		if(null != m_Effect)
			m_Effect.SetActiveState(active);
	}
}