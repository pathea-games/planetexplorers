using UnityEngine;
using System.Collections;
using Pathea;
using ItemAsset;

public class PEEquipmentLogic : MonoBehaviour 
{
	protected ItemObject 	m_ItemObj;
	protected PeEntity		m_Entity;
	protected Motion_Equip	m_Equip;

	public ItemObject itemObject{ get { return m_ItemObj;}}
	
	public virtual void InitEquipment(PeEntity entity, ItemObject itemObj)
	{
		m_ItemObj = itemObj;
		m_Entity = entity;
		m_Equip = m_Entity.GetCmpt<Motion_Equip>();
	}
	
	public virtual void RemoveEquipment()
	{
		GameObject.Destroy(gameObject);
	}

	public virtual void OnModelRebuild() { }
}
