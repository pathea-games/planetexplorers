using UnityEngine;
using System.Collections;

public class VCEUIExportWnd : MonoBehaviour
{
	public VCEUIStatisticsPanel m_StatPanel;
	public UILabel m_ErrorLabel;
	public UIButton m_ExportButton;
	public UIInput m_InputField;
	
	public void Init ()
	{
		m_StatPanel.Init();
	}
	
	public void Show ()
	{
		this.gameObject.SetActive(true);
		VCEditor.Instance.m_MirrorGroup.SetActive(false);
		VCEditor.Instance.m_GLGroup.SetActive(false);
		m_StatPanel.OnCreationInfoRefresh();
		m_StatPanel.SetIsoIcon();
	}
	
	public void Hide ()
	{
		VCEditor.Instance.m_MirrorGroup.SetActive(true);
		VCEditor.Instance.m_GLGroup.SetActive(true);
		this.gameObject.SetActive(false);
		m_InputField.gameObject.SetActive(false);
		m_InputField.text = "";
	}
	
	public void OnCancelClick ()
	{
		Hide();
	}
	
	public void OnExportClick ()
	{
		// Make Creation
		int r = VCEditor.MakeCreation();
		if ( r == -4 ) // Inventory is full
			return;
        if (!VCEditor.s_MultiplayerMode)
		    VCEMsgBox.Show( (r == 0) ? VCEMsgBoxType.EXPORT_OK : VCEMsgBoxType.EXPORT_FAILED );
	}
	
	void Update ()
	{
		// input switch
		if ( Input.GetKeyDown(KeyCode.C) && 
			 Input.GetKey(KeyCode.LeftControl) && 
			 Input.GetKey(KeyCode.LeftAlt) )
		{
			m_InputField.gameObject.SetActive(!m_InputField.gameObject.activeInHierarchy);
		}
		
		// Check if can export
		if ( !VCEditor.s_ConnectedToGame )
		{
			m_ErrorLabel.text = "Export cannot be done outside the game".ToLocalizationString() + "!";
			m_ExportButton.gameObject.SetActive(false);
		}
		//else if ( VCEditor.s_MultiplayerMode && 
		//        ( false ) )
		//{
		//	m_ErrorLabel.text = "Vehicles can't be exported in MP yet".ToLocalizationString() + "!";
		//	m_ExportButton.gameObject.SetActive(false);
		//}
		else if ( VCEditor.s_Scene.m_CreationAttr.m_Type == ECreation.Null || VCEditor.s_Scene.m_CreationAttr.m_Errors.Count > 0 )
		{
			m_ErrorLabel.text = "Your creation has some errors".ToLocalizationString() + "!";
			m_ExportButton.gameObject.SetActive(false);
		}
		else if ( !m_StatPanel.m_CostList.IsEnough )
		{
			m_ErrorLabel.text = "Not enough items".ToLocalizationString() + "!";
			m_ExportButton.gameObject.SetActive(false);
		}
        else if (GameUI.Instance != null && GameUI.Instance.mSkillWndCtrl != null && GameUI.Instance.mSkillWndCtrl._SkillMgr != null)
        {
            if (!GameUI.Instance.mSkillWndCtrl._SkillMgr.CheckUnlockCusProduct((int)(VCEditor.s_Scene.m_IsoData.m_HeadInfo.Category)))
            {
                m_ErrorLabel.text = "You need to unlock the skill first".ToLocalizationString() + "!";
                m_ExportButton.gameObject.SetActive(false);
            }
            else
            {
                m_ErrorLabel.text = "";
                m_ExportButton.gameObject.SetActive(true);
            }
                
        }
        else
        {
            m_ErrorLabel.text = "";
            m_ExportButton.gameObject.SetActive(true);
        }
	}
	
	void OnInputSubmit ()
	{
		if ( m_InputField.text.ToLower() == "show me the item" )
		{
			VCEditor.Instance.m_CheatWhenMakeCreation = !VCEditor.Instance.m_CheatWhenMakeCreation;
		}
		m_InputField.text = "";
	}
}
