using UnityEngine;
using System.Collections;

public class VCEUIPartItem : MonoBehaviour
{
	public VCEUIPartList m_ParentList;
	public VCPartInfo m_PartInfo;
	public UISprite m_IconSprite;
	public UILabel m_NameLabel;
	public GameObject m_HoverBtn;
	public GameObject m_SelectedSign;
	
	// Use this for initialization
	void Start ()
	{
		m_IconSprite.spriteName = m_PartInfo.m_IconPath.Split(',')[0];
		m_NameLabel.text = VCUtils.Capital(ItemAsset.ItemProto.GetName(m_PartInfo.m_ItemID), true);
		m_SelectedSign.SetActive(false);
	}
	
	// Update is called once per frame
	void Update ()
	{
		m_SelectedSign.SetActive(VCEditor.SelectedPart == m_PartInfo);
	}
	
	void OnSelectClick()
	{
		VCEditor.SelectedPart = m_PartInfo;
	}
}
