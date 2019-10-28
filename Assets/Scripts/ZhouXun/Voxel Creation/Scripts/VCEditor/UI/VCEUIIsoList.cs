using UnityEngine;
using System.IO;
using System.Linq;

public class VCEUIIsoList : VCEUIAssetList
{
	public GameObject m_UplevelBtn;
	public VCEUIIsoItem m_SelectedItem = null;
	public GameObject m_EmptyItem;
	public string m_Path = "";
	
	public VCEUIIsoHeaderInspector m_IsoHeaderInspectorRes;
	public VCEUIIsoHeaderInspector m_IsoHeaderInspector;
	
	public override void Init ()
	{
		base.Init ();
		m_Path = VCConfig.s_IsoPath;
	}
	
	public void CreateInspector()
	{
		DestroyInspector();
		m_IsoHeaderInspector = VCEUIIsoHeaderInspector.Instantiate(m_IsoHeaderInspectorRes) as VCEUIIsoHeaderInspector;
		m_IsoHeaderInspector.transform.parent = VCEditor.Instance.m_UI.m_InspectorGroup;
		m_IsoHeaderInspector.transform.localPosition = Vector3.zero;
		m_IsoHeaderInspector.transform.localScale = Vector3.one;
		m_IsoHeaderInspector.gameObject.SetActive(true);
	}
	
	public void DestroyInspector()
	{
		if ( m_IsoHeaderInspector != null )
		{
			GameObject.Destroy(m_IsoHeaderInspector.gameObject);
			m_IsoHeaderInspector = null;
		}
	}
	
	// Update is called once per frame
	new void Update ()
	{
		base.Update();
		if ( m_Path.Length <= VCConfig.s_IsoPath.Length )
			m_UplevelBtn.SetActive(false);
		else
			m_UplevelBtn.SetActive(true);
		
		if ( m_SelectedItem != null )
			m_IsoHeaderInspector.FilePath = m_SelectedItem.m_IsFolder ? "" : m_SelectedItem.m_FilePath;
		else
			m_IsoHeaderInspector.FilePath = "";
	}
	
	public void RefreshIsoList()
	{
		string fullpath = m_Path;
		
		if ( !Directory.Exists(fullpath) )
			Directory.CreateDirectory(fullpath);
		
		string[] dirs = Directory.GetDirectories(fullpath);
        dirs = dirs.OrderByDescending(a => { return Directory.GetLastWriteTime(a).Ticks; }).ToArray();
        string[] files = Directory.GetFiles(fullpath);
        files = files.OrderByDescending(a => { return Directory.GetLastWriteTime(a).Ticks; }).ToArray();
		
		ClearItems();
		m_SelectedItem = null;
		
		foreach ( string s in dirs )
		{
			GameObject item_go = GameObject.Instantiate(m_ItemRes) as GameObject;
			Vector3 scale = item_go.transform.localScale;
            item_go.name = "[Dir] " + new DirectoryInfo(s).Name;
			item_go.transform.parent = m_ItemGroup.transform;
			item_go.transform.localPosition = Vector3.zero;
			item_go.transform.localScale = scale;
			VCEUIIsoItem iso_item = item_go.GetComponent<VCEUIIsoItem>();
			iso_item.m_ParentList = this;
			iso_item.m_HoverBtn.AddComponent<UIDragPanelContents>();
			iso_item.m_IsFolder = true;
			iso_item.m_FilePath = s;
			m_AssetItems.Add(item_go);
		}
		foreach ( string s in files )
		{
			GameObject item_go = GameObject.Instantiate(m_ItemRes) as GameObject;
			Vector3 scale = item_go.transform.localScale;
			item_go.name = "[File] " + new FileInfo(s).Name;
			item_go.transform.parent = m_ItemGroup.transform;
			item_go.transform.localPosition = Vector3.zero;
			item_go.transform.localScale = scale;
			VCEUIIsoItem iso_item = item_go.GetComponent<VCEUIIsoItem>();
			iso_item.m_ParentList = this;
			iso_item.m_HoverBtn.AddComponent<UIDragPanelContents>();
			iso_item.m_IsFolder = false;
			iso_item.m_FilePath = s;
			m_AssetItems.Add(item_go);
		}
		if ( dirs.Length + files.Length == 0 )
		{
			m_EmptyItem.SetActive(true);
		}
		else
		{
			m_EmptyItem.SetActive(false);
		}
		RepositionGrid();
		RepositionList();
	}
	
	void OnUplevelClick ()
	{
		if ( m_Path.Length > 0 )
			m_Path = new DirectoryInfo(m_Path).Parent.FullName;
		RefreshIsoList();
	}
}
