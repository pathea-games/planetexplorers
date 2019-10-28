using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class VCEUIAssetList : MonoBehaviour
{
	public UIPanel m_Panel;
	public float m_OriginY = 0;
	public float m_YReserve = 487;
	public float ClipCenterX = 80;
	public float ClipSizeX = 250;
	public GameObject m_ItemGroup;
	public GameObject m_ItemRes;
	
	protected List<GameObject> m_AssetItems;
		
	public virtual void Init ()
	{
		ClearItems();
		m_AssetItems = new List<GameObject> ();
	}
	
	public void OnEnable()
	{
		RepositionList();
	}
	
	protected void ClearItems()
	{
		if ( m_AssetItems != null )
		{
			foreach ( GameObject go in m_AssetItems )
			{
				go.SetActive(false);
				go.transform.parent = null;
				GameObject.Destroy(go);
			}
			m_AssetItems.Clear();
		}
	}
	
	// Update is called once per frame
	protected void Update ()
	{
		if ( m_YReserve != 0 )
		{
			float panelsize = Screen.height - m_YReserve;
			m_Panel.clipRange = new Vector4 (ClipCenterX, -panelsize*0.5f-m_Panel.transform.localPosition.y+m_OriginY, ClipSizeX, panelsize);
		}
		else
		{
			Vector4 clip_range = m_Panel.clipRange;
			m_Panel.clipRange = new Vector4 (ClipCenterX, clip_range.y, ClipSizeX, clip_range.w);
		}
	}
	
	public void RepositionGrid()
	{
        Invoke("_RepositionGrid", 0f);
	}

    private void _RepositionGrid()
    {
        m_ItemGroup.GetComponent<UIGrid>().Reposition();
    }

    public void RepositionList()
	{
		Invoke("_RepositionList", 0.05f);
	}
	private void _RepositionList()
	{
		Vector3 pos = m_Panel.transform.localPosition;
		pos.y = m_OriginY;
		m_Panel.transform.localPosition = pos;
	}
}
