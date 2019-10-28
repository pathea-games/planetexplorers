using UnityEngine;
using System.Collections;
using Pathea;

public class PEEnergySheild : PECtrlAbleEquipment
{
	public EnergySheildHandler 	m_Handler;

	public GameObject m_SubPart;

	public string attachBone = "Bip01 Spine3";
	public string subPartAttachBone = "Bip01 R Clavicle";

	float m_LastNetValue;
	UTimer m_Time;

	public override void InitEquipment (Pathea.PeEntity entity, ItemAsset.ItemObject itemObj)
	{
		base.InitEquipment (entity, itemObj);
		if(null != m_SubPart)
			m_View.AttachObject(m_SubPart, subPartAttachBone);
		m_View.AttachObject(gameObject, attachBone);
	}

	public override void RemoveEquipment ()
	{
		base.RemoveEquipment ();
		if(null != m_SubPart)
		{
			m_View.DetachObject(m_SubPart);
			GameObject.Destroy(m_SubPart);
		}
	}

	protected virtual void Update()
	{
		bool active = (null != m_Entity) && m_Entity.GetAttribute(AttribType.Shield) > 0;

		if(null != m_Handler)
		{
			m_Handler.gameObject.layer = Pathea.Layer.Damage;
			if(m_Handler.gameObject.activeSelf != active)
				m_Handler.gameObject.SetActive(active);
		}
	}
}
