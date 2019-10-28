using UnityEngine;
using System.Collections;
using Pathea;

public class PEGloves : PeSword
{
	public override void InitEquipment (Pathea.PeEntity entity, ItemAsset.ItemObject itemObj)
	{
		m_Entity = entity;
		m_ItemObj = itemObj;
		m_View = m_Entity.biologyViewCmpt;
		m_MotionEquip = m_Entity.motionEquipment;
		m_MotionMgr = m_Entity.motionMgr;
		m_View.AttachObject(gameObject, m_HandChangeAttr.m_PutOffBone);
	}
}
