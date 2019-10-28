using UnityEngine;
using System.Collections;

public class VCEUIMaterialItem : MonoBehaviour
{
	public VCEUIMaterialList m_ParentList;
	public VCMaterial m_Material;
	public UILabel m_NameLabel;
	public UILabel m_IndexLabel;
	public Material m_SourceIconTexture;
	public UITexture m_MaterialIcon;
	private Material m_MaterialIconTexture;
	public GameObject m_SelectedGlow;
	public GameObject m_AddBtn;
	public GameObject m_DelBtn;
	
	// Use this for initialization
	void Start ()
	{
		m_NameLabel.text = m_Material.m_Name;
		m_MaterialIconTexture = Material.Instantiate(m_SourceIconTexture) as Material;
		m_MaterialIconTexture.mainTexture = m_Material.m_DiffuseTex;
		m_MaterialIcon.material = m_MaterialIconTexture;
	}
	
	// Update is called once per frame
	void Update ()
	{
		if ( VCEditor.DocumentOpen() )
		{
			int index = VCEditor.s_Scene.m_IsoData.QueryMaterialIndex(m_Material);
			if ( index >= 0 && VCEditor.s_Scene.m_IsoData.m_Materials[index] == m_Material )
			{
				m_IndexLabel.text = "No." + (index+1).ToString();
				m_IndexLabel.color = Color.white;
			}
			else if ( VCEditor.SelectedMaterial == m_Material )
			{
				if ( VCEditor.SelectedVoxelType < 0 )
				{
					m_IndexLabel.text = "ISO FULL";
					m_IndexLabel.color = Color.red;
				}
				else
				{
					m_IndexLabel.text = "No." + (VCEditor.SelectedVoxelType+1).ToString();
					m_IndexLabel.color = Color.white;
				}
			}
			else
			{
				m_IndexLabel.text = "";
				m_IndexLabel.color = Color.white;
			}
			bool tempmat = VCEAssetMgr.s_TempMaterials.ContainsKey(m_Material.m_Guid);
			bool existmat = VCEAssetMgr.s_Materials.ContainsKey(m_Material.m_Guid);
			m_AddBtn.SetActive(tempmat && !existmat);
			m_DelBtn.SetActive(!tempmat && existmat && index < 0 && VCEditor.SelectedMaterial == m_Material);
		}
	}
	
	void OnDestroy ()
	{
		Material.Destroy(m_MaterialIconTexture);
	}
	
	public void OnItemClick ()
	{
		VCEditor.SelectedMaterial = m_Material;
		VCEditor.Instance.m_UI.m_MaterialWindow.Reset(m_Material);

		// If nothing selected, set a primary general brush
		if ( !VCEditor.SelectedGeneralBrush )
		{
			if ( VCEditor.VoxelSelection.Count == 0 )
			{
				if ( VCEditor.Instance.m_UI.m_VoxelBrushItemGroup.m_LastGeneralBrush != null )
					VCEditor.Instance.m_UI.m_VoxelBrushItemGroup.m_LastGeneralBrush.GetComponent<UICheckbox>().isChecked = true;
			}
		}
		s_CurrentDelMat = null;
	}
	public void OnItemDbClick ()
	{
		if ( !VCEditor.Instance.m_UI.m_MaterialWindow.WindowVisible() )
			VCEditor.Instance.m_UI.m_MaterialWindow.ShowWindow(m_Material);
		s_CurrentDelMat = null;
	}
	
	public void OnAddClick()
	{
		if ( VCEAssetMgr.AddMaterialFromTemp(m_Material.m_Guid) )
		{
			VCEditor.SelectedMaterial = m_Material;
			VCEditor.Instance.m_UI.m_MaterialWindow.Reset(m_Material);
			VCEditor.Instance.m_UI.m_MaterialList.RefreshMaterialList(VCEditor.Instance.m_UI.m_MatterPopupList.selection);
			VCEStatusBar.ShowText("Add material".ToLocalizationString() + " [" + m_Material.m_Name + "] " + "from the current ISO".ToLocalizationString() + " !", 6f, true);
		}
		else
		{
			VCEMsgBox.Show(VCEMsgBoxType.MATERIAL_NOT_SAVED);
		}
		s_CurrentDelMat = null;
	}
	
	public static VCMaterial s_CurrentDelMat = null;
	public static void DoDeleteFromMsgBox()
	{
		if ( VCEAssetMgr.DeleteMaterial(s_CurrentDelMat.m_Guid) )
		{
			VCEditor.SelectedMaterial = null;
			if ( VCEditor.Instance.m_UI.m_MaterialWindow.WindowVisible() )
				VCEditor.Instance.m_UI.m_MaterialWindow.HideWindow();
			VCEditor.Instance.m_UI.m_MaterialList.RefreshMaterialList(VCEditor.Instance.m_UI.m_MatterPopupList.selection);
			VCEStatusBar.ShowText("Material".ToLocalizationString() + " [" + s_CurrentDelMat.m_Name + "] " + "has been deleted".ToLocalizationString() + " !", 6f, true);
		}
		else
		{
			VCEMsgBox.Show(VCEMsgBoxType.MATERIAL_NOT_SAVED);
		}
	}
	public void OnDelClick()
	{
		s_CurrentDelMat = m_Material;
		VCEMsgBox.Show(VCEMsgBoxType.MATERIAL_DEL_QUERY);
	}
}
