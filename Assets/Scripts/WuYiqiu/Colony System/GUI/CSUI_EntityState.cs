using UnityEngine;
using System.Collections;
using ItemAsset;

public class CSUI_EntityState : MonoBehaviour 
{

	#region UI_WIDGET

	[SerializeField] UISlicedSprite 	m_IconUI;
	[SerializeField] UILabel		  	m_NameUI;

	[SerializeField] UILabel			m_LifeUI;
	[SerializeField] UISlider			m_LifeProgressUI;

	#endregion

	public CSEntity		m_RefCommon;

	// Use this for initialization
	void Start () 
	{
	
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (m_RefCommon == null)
			return;

		m_NameUI.text = m_RefCommon.Name;
		ItemProto itemData = ItemProto.GetItemData(m_RefCommon.ItemID);
		if (itemData != null)
		{
			string[] iconStr = ItemProto.GetItemData(m_RefCommon.ItemID).icon;
			if (iconStr.Length != 0)
				m_IconUI.spriteName = iconStr[0];
			else
				m_IconUI.spriteName = "";
		}

		float percent  = m_RefCommon.BaseData.m_Durability / m_RefCommon.m_Info.m_Durability;
		m_LifeProgressUI.sliderValue = percent;

		string str = "";
		str += Mathf.RoundToInt(m_RefCommon.BaseData.m_Durability).ToString();
		str += " / ";
		str += Mathf.RoundToInt(m_RefCommon.m_Info.m_Durability).ToString();
		m_LifeUI.text = str;
	}
}
