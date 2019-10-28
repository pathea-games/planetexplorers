using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class VCEUIMaterialList : VCEUIAssetList
{
	public UIPopupList m_MatterPopupList;
	private string m_lastSelection;
	
	// Update is called once per frame
	new void Update ()
	{
		base.Update();
		if ( m_lastSelection != m_MatterPopupList.selection )
		{
			m_lastSelection = m_MatterPopupList.selection;
			RefreshMaterialList(m_MatterPopupList.selection);
			VCEditor.SelectedMaterial = null;
			RepositionList();
		}
		foreach ( GameObject go in m_AssetItems )
		{
			VCEUIMaterialItem item = go.GetComponent<VCEUIMaterialItem>();
			if ( item.m_Material == VCEditor.SelectedMaterial )
			{
				item.m_SelectedGlow.SetActive(true);
			}
			else
			{
				item.m_SelectedGlow.SetActive(false);
			}
		}
	}
	
	public void InitMatterList ()
	{
		m_MatterPopupList.items.Clear();
		m_MatterPopupList.items.Add("All".ToLocalizationString());
		foreach ( KeyValuePair<int, VCMatterInfo> kvp in VCConfig.s_Matters )
		{
			m_MatterPopupList.items.Add(VCUtils.Capital(kvp.Value.Name, true).ToLocalizationString());
		}
		m_MatterPopupList.selection = "All".ToLocalizationString();
	}
	
	public void RefreshMaterialList(string filter)
	{
		ClearItems();
		
		int matterid = -1;
		foreach ( KeyValuePair<int, VCMatterInfo> kvp in VCConfig.s_Matters )
		{
			if ( filter.ToLower() == kvp.Value.Name.ToLower() )
			{
				matterid = kvp.Value.ItemIndex;
				break;
			}
		}
		foreach ( KeyValuePair<ulong, VCMaterial> kvp in VCEAssetMgr.s_Materials )
		{
			if ( matterid == -1 || matterid == kvp.Value.m_MatterId )
			{
				GameObject item_go = GameObject.Instantiate(m_ItemRes) as GameObject;
				Vector3 scale = item_go.transform.localScale;
				string _name = "_Asset " + /*kvp.Value.m_MatterId.ToString("00") + " " + */kvp.Value.m_Name;
				//_name = _name.Replace("Default ", "    __ ");
				item_go.name = _name;
				item_go.transform.parent = m_ItemGroup.transform;
				item_go.transform.localPosition = Vector3.zero;
				item_go.transform.localScale = scale;
				VCEUIMaterialItem mat_item = item_go.GetComponent<VCEUIMaterialItem>();
				item_go.AddComponent<UIDragPanelContents>();
				mat_item.m_ParentList = this;
				mat_item.m_Material = kvp.Value;
				mat_item.m_MaterialIcon.gameObject.AddComponent<UIDragPanelContents>();
				m_AssetItems.Add(item_go);
			}
		}
		foreach ( KeyValuePair<ulong, VCMaterial> kvp in VCEAssetMgr.s_TempMaterials )
		{
			if ( matterid == -1 || matterid == kvp.Value.m_MatterId )
			{
				GameObject item_go = GameObject.Instantiate(m_ItemRes) as GameObject;
				Vector3 scale = item_go.transform.localScale;
				string _name = "_Iso " + /*kvp.Value.m_MatterId.ToString("00") + " " + */kvp.Value.m_Name;
				//_name = _name.Replace("Default ", "    __ ");
				item_go.name = _name;
				item_go.transform.parent = m_ItemGroup.transform;
				item_go.transform.localPosition = Vector3.zero;
				item_go.transform.localScale = scale;
				VCEUIMaterialItem mat_item = item_go.GetComponent<VCEUIMaterialItem>();
				item_go.AddComponent<UIDragPanelContents>();
				mat_item.m_ParentList = this;
				mat_item.m_Material = kvp.Value;
				mat_item.m_MaterialIcon.gameObject.AddComponent<UIDragPanelContents>();
				m_AssetItems.Add(item_go);
			}
		}
		RepositionGrid();
	}
	
	public void ListFocusOn(VCMaterial focus_item)
	{
		
		if ( focus_item != null )
		{
			Vector3 pos = m_Panel.transform.localPosition;
			foreach ( GameObject itemgo in m_AssetItems )
			{
				VCEUIMaterialItem item = itemgo.GetComponent<VCEUIMaterialItem>();
				if ( item.m_Material == focus_item )
				{
					pos.y = m_OriginY - itemgo.transform.localPosition.y - 8;
				}
			}
			m_Panel.transform.localPosition = pos;
		}
		else
		{
			RepositionList();
		}
	}
	
	// ------------ For Material Editor -----------------------------------------
	public void RefreshMaterialListThenFocusOnSelected()
	{
		RefreshMaterialList(m_MatterPopupList.selection);
		MaterialListFocusOnSelected();
	}
	
	private VCMaterial m_TempSelected;
	public void MaterialListFocusOnSelected()
	{
		m_TempSelected = VCEditor.SelectedMaterial;
		if ( m_MatterPopupList.selection != "All".ToLocalizationString() && VCEditor.SelectedMaterial != null )
			m_MatterPopupList.selection = VCUtils.Capital(VCConfig.s_Matters[VCEditor.SelectedMaterial.m_MatterId].Name, true);
		Invoke("MaterialListFocusOnSelectedInvoke", 0.2f);
	}
	private void MaterialListFocusOnSelectedInvoke()
	{
		VCEditor.SelectedMaterial = m_TempSelected;
		m_TempSelected = null;
		ListFocusOn(VCEditor.SelectedMaterial);
	}
	// ------------ GUI Events --------------------------------------------------
	public void OnAddMaterialClick()
	{
		VCEditor.SelectedMaterial = null;
		VCEditor.Instance.m_UI.m_MaterialWindow.ShowWindow(null);
	}
}
