using UnityEngine;
using System.Collections;

public class VCEUIDecalItem : MonoBehaviour
{
	public VCEUIDecalList m_ParentList;
	public UITexture m_DecalUITex;
	public GameObject m_HoverBtn;
	public GameObject m_SelectedSign;
	private Material m_DecalUITexMat;
	public UILabel m_IndexLabel;
	public GameObject m_AddBtn;
	public GameObject m_DelBtn;

	public ulong m_GUID;

	// Use this for initialization
	void Start ()
	{
		m_DecalUITexMat = Material.Instantiate(m_DecalUITex.material) as Material;
		m_DecalUITex.material = m_DecalUITexMat;
		m_SelectedSign.SetActive(false);
		VCDecalAsset dcl = VCEAssetMgr.GetDecal(m_GUID);
		if ( dcl != null )
			m_DecalUITexMat.SetTexture("_MainTex", dcl.m_Tex);
		else
			m_DecalUITexMat.SetTexture("_MainTex", null);
	}
	
	// Update is called once per frame
	void Update ()
	{
		if ( VCEditor.DocumentOpen() )
		{
			VCDecalAsset decal = VCEAssetMgr.GetDecal(m_GUID);

			m_SelectedSign.SetActive(VCEditor.SelectedDecalGUID == m_GUID && m_GUID != 0);
			int index = VCEditor.s_Scene.m_IsoData.QueryExistDecalIndex(decal);
			if ( index >= 0 && VCEditor.s_Scene.m_IsoData.m_DecalAssets[index] == decal )
			{
				m_IndexLabel.text = (index+1).ToString() + " of " + VCIsoData.DECAL_ARR_CNT.ToString();
				m_IndexLabel.color = Color.white;
			}
			else if ( VCEditor.SelectedDecal == decal )
			{
				if ( VCEditor.SelectedDecalIndex < 0 )
				{
					m_IndexLabel.text = "FULL";
					m_IndexLabel.color = Color.red;
				}
				else
				{
					m_IndexLabel.text = (VCEditor.SelectedDecalIndex+1).ToString() + " of " + VCIsoData.DECAL_ARR_CNT.ToString();
					m_IndexLabel.color = Color.white;
				}
			}
			else
			{
				m_IndexLabel.text = "";
				m_IndexLabel.color = Color.white;
			}
			bool tempdcl = VCEAssetMgr.s_TempDecals.ContainsKey(m_GUID);
			bool existdcl = VCEAssetMgr.s_Decals.ContainsKey(m_GUID);
			m_AddBtn.SetActive(tempdcl && !existdcl);
			m_DelBtn.SetActive(!tempdcl && existdcl && index < 0 && VCEditor.SelectedDecal == decal);
		}
	}
	
	void OnSelectClick()
	{
		VCEditor.SelectedDecalGUID = m_GUID;
	}


	public void OnAddClick()
	{
		if ( VCEAssetMgr.AddDecalFromTemp(m_GUID) )
		{
			VCDecalAsset decal = VCEAssetMgr.GetDecal(m_GUID);
			VCEditor.SelectedDecalGUID = m_GUID;
			m_ParentList.RefreshDecalList();
			VCEStatusBar.ShowText("Add decal".ToLocalizationString() + " [" + decal.GUIDString + "] " + "from the current ISO".ToLocalizationString() + " !", 6f, true);
		}
		else
		{
			VCEMsgBox.Show(VCEMsgBoxType.DECAL_NOT_SAVED);
		}
		s_CurrentDelDecal = null;
	}
	
	public static VCDecalAsset s_CurrentDelDecal = null;
	public static void DoDeleteFromMsgBox()
	{
		string sguid = s_CurrentDelDecal.GUIDString;
		if ( VCEAssetMgr.DeleteDecal(s_CurrentDelDecal.m_Guid) )
		{
			VCEditor.SelectedDecal = null;
			VCEditor.Instance.m_UI.m_DecalList.RefreshDecalList();
			VCEStatusBar.ShowText("Decal".ToLocalizationString() + " [" + sguid + "] " + "has been deleted".ToLocalizationString() + " !", 6f, true);
		}
		else
		{
			VCEMsgBox.Show(VCEMsgBoxType.DECAL_NOT_SAVED);
		}
	}
	public void OnDelClick()
	{
		s_CurrentDelDecal = VCEAssetMgr.GetDecal(m_GUID);
		if ( s_CurrentDelDecal != null )
			VCEMsgBox.Show(VCEMsgBoxType.DECAL_DEL_QUERY);
	}
}
