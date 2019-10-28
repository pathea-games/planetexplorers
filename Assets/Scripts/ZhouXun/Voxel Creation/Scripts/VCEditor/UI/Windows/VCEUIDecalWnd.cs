using UnityEngine;
using System.Collections;
using System.IO;

public class VCEUIDecalWnd : MonoBehaviour
{
	public GameObject m_Window;
	public UILabel m_UIDLabel;
	public UITexture m_DecalUITex;
	private Material m_DecalUIMat;
	public UIInput m_PathInput;
	private string m_LastPath;
	public UILabel m_ErrorLabel;
	public GameObject m_CreateBtnGO;

	private VCDecalAsset m_TmpDecal;

	// Use this for initialization
	void Start ()
	{
		m_DecalUIMat = Material.Instantiate(m_DecalUITex.material) as Material;
		m_DecalUITex.material = m_DecalUIMat;
		m_LastPath = "";
	}
	
	// Update is called once per frame
	void Update ()
	{
		if ( !WindowVisible() )
			return;
		if ( VCEditor.SelectedDecal != null )
			HideWindow();
		if ( m_PathInput.text.Length > 0 )
		{
			string s = m_PathInput.text.Replace("\\", "/");
			if ( m_PathInput.text != s )
			{
				m_PathInput.text = s;
			}
		}
		if ( m_LastPath.Length < 4 )
		{
			if ( m_TmpDecal != null )
			{
				m_TmpDecal.Destroy();
				m_TmpDecal = null;
			}
		}
		if ( m_LastPath != m_PathInput.text )
		{
			m_LastPath = m_PathInput.text;
			if ( m_TmpDecal != null )
			{
				m_TmpDecal.Destroy();
				m_TmpDecal = null;
			}
			Texture2D tmptex = VCUtils.LoadTextureFromFile(m_PathInput.text);
			if ( tmptex != null )
			{
				m_TmpDecal = new VCDecalAsset ();
				m_TmpDecal.Import(tmptex.EncodeToPNG());
				Texture2D.Destroy(tmptex);
			}
		}

		if ( m_TmpDecal != null && m_TmpDecal.m_Tex != null )
		{
			m_UIDLabel.text = m_TmpDecal.GUIDString;
			m_DecalUIMat.mainTexture = m_TmpDecal.m_Tex;
			m_DecalUITex.gameObject.SetActive(true);
			if ( VCEAssetMgr.GetDecal(m_TmpDecal.m_Guid) != null )
			{
				m_ErrorLabel.text = "The same decal image already exist".ToLocalizationString() + " !";
			}
			else if ( m_TmpDecal.m_Tex.width > 512 || m_TmpDecal.m_Tex.height > 512 )
			{
				m_ErrorLabel.text = "Decal size must smaller than 512px".ToLocalizationString() + " !";
			}
			else
			{
				m_ErrorLabel.text = "";
			}
		}
		else
		{
			m_UIDLabel.text = "0000000000000000";
			m_DecalUITex.gameObject.SetActive(false);
			m_DecalUIMat.mainTexture = null;
			m_ErrorLabel.text = "Please specify a decal image".ToLocalizationString() + " (*.png)";
		}
		m_CreateBtnGO.SetActive(m_ErrorLabel.text.Trim().Length < 1);
	}

	void OnDestroy()
	{
		if ( m_TmpDecal != null )
		{
			m_TmpDecal.Destroy();
			m_TmpDecal = null;
		}
	}
	public bool WindowVisible ()
	{
		return m_Window.activeInHierarchy;
	}
	
	public void ShowWindow ()
	{
		m_TmpDecal = new VCDecalAsset ();
		m_Window.SetActive(true);
	}
	public void HideWindow ()
	{
		m_PathInput.text = "";
		if ( m_TmpDecal != null )
		{
			m_TmpDecal.Destroy();
			m_TmpDecal = null;
		}
		m_Window.SetActive(false);
	}

	public void OnOKClick ()
	{
		if ( m_ErrorLabel.text.Trim().Length < 1 && m_TmpDecal != null && m_TmpDecal.m_Tex != null )
		{
			VCDecalAsset newdcl = new VCDecalAsset ();
			newdcl.Import(m_TmpDecal.Export());
			VCEAssetMgr.s_Decals.Add(newdcl.m_Guid, newdcl);
			if ( !VCEAssetMgr.CreateDecalDataFile(newdcl) )
			{
				VCEMsgBox.Show(VCEMsgBoxType.DECAL_NOT_SAVED);
			}
			
			VCEditor.SelectedDecal = newdcl;
			VCEditor.Instance.m_UI.m_DecalList.RefreshDecalList();
			VCEditor.SelectedDecal = newdcl;

			VCEStatusBar.ShowText("Added new decal".ToLocalizationString() + " !", 4f, true);
		}
	}

	public void OnCloseClick ()
	{
		HideWindow();
	}
}
