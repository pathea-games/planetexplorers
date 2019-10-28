using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class VCEUICostList : VCEUIAssetList
{
	public bool m_IsEditor = true;
	[HideInInspector] public CreationAttr m_NonEditorAttr = null;

	public bool IsEnough
	{
		get
		{
			if ( m_AssetItems == null )
				return false;
			if ( m_AssetItems.Count == 0 )
				return false;
			foreach ( GameObject go in m_AssetItems )
			{
				VCEUICostItem item = go.GetComponent<VCEUICostItem>();
				if ( item != null )
				{
					if ( !item.m_IsEnough )
						return false;
				}
			}
			return true;
		}
	}
	
	// Update is called once per frame
	new void Update ()
	{
		base.Update();
	}
	
	new void OnEnable ()
	{
		RefreshCostList();
	}
		
	public void RefreshCostList()
	{
		ClearItems();

		CreationAttr attr = null;
		attr = m_IsEditor ? VCEditor.s_Scene.m_CreationAttr : m_NonEditorAttr;

		if ( attr != null )
		{
			foreach ( KeyValuePair<int, int> kvp in attr.m_Cost )
			{
				if ( kvp.Value == 0 )
					continue;
				GameObject item_go = GameObject.Instantiate(m_ItemRes) as GameObject;
				Vector3 scale = item_go.transform.localScale;
				item_go.name = kvp.Key.ToString();
				item_go.transform.parent = m_ItemGroup.transform;
				item_go.transform.localPosition = Vector3.zero;
				item_go.transform.localScale = scale;
				VCEUICostItem item = item_go.GetComponent<VCEUICostItem>();
				item.m_GameItemId = kvp.Key;
				item.m_GameItemCost = kvp.Value;
				m_AssetItems.Add(item_go);
			}
		}
		RepositionGrid();
	}
}
