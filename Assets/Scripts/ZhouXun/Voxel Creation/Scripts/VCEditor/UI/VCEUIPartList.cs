using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class VCEUIPartList : VCEUIAssetList
{
	public UIPopupList m_TypePopupList;
	private string m_lastSelection;
	
	// Update is called once per frame
	new void Update ()
	{
		base.Update();
		if ( m_lastSelection != m_TypePopupList.selection )
		{
			m_lastSelection = m_TypePopupList.selection;
			RefreshPartList(m_TypePopupList.selection);
			VCEditor.SelectedPart = null;
			RepositionList();
		}
	}
	
	public void InitTypeList ()
	{
		m_TypePopupList.items.Clear();
		m_TypePopupList.items.Add("All".ToLocalizationString());
		List<EVCComponent> typelist = VCConfig.s_Categories[VCEditor.s_Scene.m_Setting.m_Category].m_PartTypes;

		// 因为添加了车的电池给飞机和船，所以这里只显示一份电池类别
		bool hasCell = false;

		bool canAdd;

		foreach ( EVCComponent typeid in typelist )
		{
			if ( typeid != EVCComponent.cpAbstract)
			{
				if (typeid == EVCComponent.cpVehicleFuelCell || typeid == EVCComponent.cpVtolFuelCell)
				{
					if (hasCell) canAdd = false;
					else canAdd = hasCell = true;
				}
				else canAdd = true;

				if (canAdd)
					m_TypePopupList.items.Add(VCUtils.Capital(VCConfig.s_PartTypes[typeid].m_ShortName, true).ToLocalizationString());
			}
		}
		m_TypePopupList.selection = "All".ToLocalizationString();
	}
	
	public void RefreshPartList(string filter)
	{
		ClearItems();

		// 因为添加了车的电池给飞机和船，所以这里需要判断多种类别
		
		List<EVCComponent> com_types = new List<EVCComponent> (2);
		List<EVCComponent> typelist = VCConfig.s_Categories[VCEditor.s_Scene.m_Setting.m_Category].m_PartTypes;

		filter = filter.ToLower();
		foreach ( EVCComponent typeid in typelist )
		{
			if ( filter == VCConfig.s_PartTypes[typeid].m_ShortName.ToLower() )
			{
				com_types.Add(typeid);
			}
		}
		
		int order = 0;
		foreach ( KeyValuePair<int, VCPartInfo> kvp in VCConfig.s_Parts )
		{
			int typeindex = -1;
			
			for ( int i = 0; i < typelist.Count; ++i )
			{
				if ( kvp.Value.m_Type == typelist[i] )
				{
					typeindex = i;
					break;
				}
			}

			if ( typeindex >= 0 )
			{
				if ( com_types.Count == 0 || com_types.Contains(kvp.Value.m_Type) )
				{
					GameObject item_go = GameObject.Instantiate(m_ItemRes) as GameObject;
					Vector3 scale = item_go.transform.localScale;
					item_go.name = typeindex.ToString("00") + (++order).ToString("000") + " " + kvp.Value.m_Name;
					item_go.transform.parent = m_ItemGroup.transform;
					item_go.transform.localPosition = Vector3.zero;
					item_go.transform.localScale = scale;
					VCEUIPartItem part_item = item_go.GetComponent<VCEUIPartItem>();
					part_item.m_HoverBtn.AddComponent<UIDragPanelContents>();
					part_item.m_ParentList = this;
					part_item.m_PartInfo = kvp.Value;
					m_AssetItems.Add(item_go);
				}
			}
		}

		RepositionGrid();
	}
}
