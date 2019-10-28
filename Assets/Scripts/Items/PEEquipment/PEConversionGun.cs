using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using SkillAsset;
using ItemAsset;
using Pathea;

public class PEConversionGun : PEGun 
{
	public Transform mBackPack;
	PackageCmpt	m_Pack;

	public override void InitEquipment (Pathea.PeEntity entity, ItemObject itemObj)
	{
		base.InitEquipment (entity, itemObj);
		m_Pack = m_Entity.GetCmpt<PackageCmpt>();
		if(null != mBackPack)
		{
			if(null != m_View)
				m_View.AttachObject(mBackPack.gameObject, "Bow_box");
			else
				mBackPack.gameObject.SetActive(false);
		}
	}

	public override float magazineSize {
		get 
		{
			if(null != m_Pack)
				return 99999;
			return 0;
		}
	}

	public override float magazineValue {
		get 
		{
			if(null != m_Pack)
				return m_Pack.GetItemCount(curItemID);
			return 0;
		}
		set {
			if(null != m_Pack)
			{
				int currentCount = m_Pack.GetItemCount(curItemID);
				if(currentCount > value)
					m_Pack.Destory(curItemID, Mathf.RoundToInt(currentCount - value));
			}
		}
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
}
