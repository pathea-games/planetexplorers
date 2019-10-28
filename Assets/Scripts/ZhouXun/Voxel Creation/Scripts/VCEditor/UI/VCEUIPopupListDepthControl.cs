using UnityEngine;
using System.Collections;

public class VCEUIPopupListDepthControl : MonoBehaviour
{
	private UIPopupList m_PopupList;
	public float m_OpenZ = -50;
	public float m_CloseZ = 0;
	// Use this for initialization
	void Start ()
	{
		m_PopupList = GetComponent<UIPopupList>();
	}
	
	// Update is called once per frame
	void Update ()
	{
		if ( m_PopupList == null ) return;
		Vector3 lpos = transform.localPosition;
		if ( m_PopupList.isOpen )
		{
			lpos.z = m_OpenZ;
			Vector3 lchildpos = m_PopupList.ChildPopupMenu.transform.localPosition;
			lchildpos.z = m_OpenZ;
			m_PopupList.ChildPopupMenu.transform.localPosition = lchildpos;
		}
		else
		{
			lpos.z = m_CloseZ;
		}
		transform.localPosition = lpos;
	}
}
