using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using WhiteCat;

public class VCEUIPartDescList : MonoBehaviour
{
	public UITable m_Table;
	public VCEUIPartDescItem m_Res;
	public List<VCEUIPartDescItem> m_Items;
	private int m_DirtyCounter = 0;
	public void SetDirty () { m_Table.Reposition(); m_DirtyCounter = 2; }
	public Transform m_BGTrans;

	public void SyncList ( List<VCPart> list_target )
	{
		if ( list_target == null )
		{
			Clear();
			return;
		}

		List<VCEUIPartDescItem> del_list = new List<VCEUIPartDescItem> ();
		foreach ( VCEUIPartDescItem pdi in m_Items )
		{
			if ( !list_target.Remove(pdi.m_PartProp) )
			{
				del_list.Add(pdi);
			}
		}
		foreach ( VCEUIPartDescItem del in del_list )
			Remove(del);
		foreach ( VCPart pp in list_target )
			Add(pp);
	}

	public void Add ( VCPart part_prop )
	{
		if ( Exists(part_prop) )
			return;
		VCEUIPartDescItem pdi = VCEUIPartDescItem.Instantiate(m_Res) as VCEUIPartDescItem;
		pdi.transform.parent = this.transform;
		pdi.transform.localScale = Vector3.one;
		pdi.gameObject.name = part_prop.gameObject.name;
		pdi.Set(part_prop);
		m_Items.Add(pdi);
		SetDirty();
	}

	public void Clear ()
	{
		foreach ( VCEUIPartDescItem pdi in m_Items )
		{
			pdi.gameObject.SetActive(false);
			pdi.transform.parent = null;
			GameObject.Destroy(pdi.gameObject);
		}
		m_Items.Clear();
		SetDirty();
	}

	public bool Exists ( VCPart part_prop )
	{
		foreach ( VCEUIPartDescItem pdi in m_Items )
		{
			if ( pdi.m_PartProp == part_prop )
			{
				return true;
			}
		}
		return false;
	}

	public bool Remove ( VCPart part_prop )
	{
		VCEUIPartDescItem remove_tar = null;
		foreach ( VCEUIPartDescItem pdi in m_Items )
		{
			if ( pdi.m_PartProp == part_prop )
			{
				remove_tar = pdi;
				pdi.gameObject.SetActive(false);
				pdi.transform.parent = null;
				GameObject.Destroy(pdi.gameObject);
			}
		}
		if ( remove_tar != null )
		{
			m_Items.Remove(remove_tar);
			SetDirty();
			return true;
		}
		return false;
	}

	public bool Remove ( VCEUIPartDescItem item )
	{
		item.gameObject.SetActive(false);
		item.transform.parent = null;
		GameObject.Destroy(item.gameObject);
		bool retval = m_Items.Remove(item);
		SetDirty();
		return retval;
	}

	void Awake ()
	{
		m_Items = new List<VCEUIPartDescItem> ();
	}

	void Update ()
	{
		if ( m_DirtyCounter > 0 )
		{
			m_Table.Reposition();
			m_DirtyCounter--;
		}
		if ( m_Items.Count > 0 )
		{
			m_BGTrans.gameObject.SetActive(true);
			Vector3 sc = m_BGTrans.localScale;
            sc.y = m_Table.mVariableHeight + 8;
			m_BGTrans.localScale = sc;
		}
		else
		{
			m_BGTrans.gameObject.SetActive(false);
		}
	}
}
