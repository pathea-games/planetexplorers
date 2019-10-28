using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class VCEUIDecalList : VCEUIAssetList
{
	// Update is called once per frame
	new void Update ()
	{
		base.Update();
	}

	public void RefreshDecalList()
	{
		ClearItems();
		foreach ( KeyValuePair<ulong, VCDecalAsset> kvp in VCEAssetMgr.s_Decals )
		{
			GameObject item_go = GameObject.Instantiate(m_ItemRes) as GameObject;
			Vector3 scale = item_go.transform.localScale;
			item_go.name = "_" + kvp.Value.GUIDString;
			item_go.transform.parent = m_ItemGroup.transform;
			item_go.transform.localPosition = Vector3.zero;
			item_go.transform.localScale = scale;
			VCEUIDecalItem decal_item = item_go.GetComponent<VCEUIDecalItem>();
			decal_item.m_GUID = kvp.Value.m_Guid;
			decal_item.m_ParentList = this;
			m_AssetItems.Add(item_go);
		}
		foreach ( KeyValuePair<ulong, VCDecalAsset> kvp in VCEAssetMgr.s_TempDecals )
		{
			GameObject item_go = GameObject.Instantiate(m_ItemRes) as GameObject;
			Vector3 scale = item_go.transform.localScale;
			item_go.name = "_" + kvp.Value.GUIDString;
			item_go.transform.parent = m_ItemGroup.transform;
			item_go.transform.localPosition = Vector3.zero;
			item_go.transform.localScale = scale;
			VCEUIDecalItem decal_item = item_go.GetComponent<VCEUIDecalItem>();
			decal_item.m_GUID = kvp.Value.m_Guid;
			decal_item.m_ParentList = this;
			m_AssetItems.Add(item_go);
		}
		RepositionGrid();
	}

	void OnAddDecalClick ()
	{
		VCEditor.SelectedDecal = null;
		VCEditor.Instance.m_UI.m_DecalWindow.ShowWindow();
	}
}
