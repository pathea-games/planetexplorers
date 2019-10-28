using UnityEngine;
using System;
using System.Collections;
using System.IO;

public class VCEUISaveWnd : MonoBehaviour
{
	public VCIsoData m_Iso;
	public UIInput m_NameInput;
	public UIInput m_AuthorInput;
	public UILabel m_AuthorLabel;
	public UIInput m_DescriptionInput;
	public UIInput m_SaveLocInput;
	public UITexture m_IconUITex;
	public UITexture m_CaptureUITex;
	public UISprite m_ColorRect;
	public UIButton m_SaveBtn;
	public UIButton m_CancelBtn;
	public UILabel m_ErrorLabel;
	
	public Material m_MatRes;
	public Transform m_ColorWndParent;
	public Vector3 m_ColorWndPos;
	
	public Camera m_CaptureCamera;
		
	private Material m_IconMat;
	private Texture2D m_IconTex;
	private Material m_CaptureMat;
	private RenderTexture m_CaptureTex;
	private VCEUIColorPickWnd m_PopupColorWnd;
	
	public void Init ()
	{
		m_IconMat = Material.Instantiate(m_MatRes) as Material;
		m_CaptureMat = Material.Instantiate(m_MatRes) as Material;
		m_IconUITex.material = m_IconMat;
		m_CaptureUITex.material = m_CaptureMat;
		m_CaptureCamera = VCEditor.Instance.m_CaptureCamera;
	}
	
	public void DestroyTex ()
	{
		if ( m_IconTex != null )
		{
			Texture2D.Destroy(m_IconTex);
			m_IconTex = null;
			if ( m_IconMat != null )
				m_IconMat.mainTexture = null;
		}
		if ( m_CaptureTex != null )
		{
			RenderTexture.Destroy(m_CaptureTex);
			m_CaptureTex = null;
			if ( m_CaptureMat != null )
				m_CaptureMat.mainTexture = null;
		}
	}
	
	void OnDestroy ()
	{
		DestroyTex();
		if ( m_IconMat != null )
		{
			Material.Destroy(m_IconMat);
			m_IconMat = null;
		}
		if ( m_CaptureMat != null )
		{
			Material.Destroy(m_CaptureMat);
			m_CaptureMat = null;
		}
	}
	
	public void Show (VCIsoData iso)
	{
		DestroyTex();
		m_Iso = iso;
		ReadIso();
		m_CaptureTex = new RenderTexture (256, 256, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
		m_CaptureMat.mainTexture = m_CaptureTex;
		m_CaptureCamera.targetTexture = m_CaptureTex;
		m_CaptureCamera.enabled = true;
		VCEditor.Instance.m_MainCamera.GetComponent<VCECamera>().ControlMode = 2;
		VCEditor.Instance.m_MirrorGroup.SetActive(false);
		VCEditor.Instance.m_GLGroup.SetActive(false);
		this.gameObject.SetActive(true);
	}
	public void Hide ()
	{
		VCEditor.Instance.m_MainCamera.GetComponent<VCECamera>().ControlMode = 1;
		VCEditor.Instance.m_MirrorGroup.SetActive(true);
		VCEditor.Instance.m_GLGroup.SetActive(true);
		this.gameObject.SetActive(false);
		m_Iso = null;
		m_CaptureCamera.enabled = false;
		m_CaptureCamera.targetTexture = null;
		DestroyTex();
		if ( VCEditor.Instance.m_UI.m_ISOTab.isChecked )
			VCEditor.Instance.m_UI.m_IsoList.RefreshIsoList();
	}
	
	public void ReadIso ()
	{
		if ( m_Iso != null )
		{
			m_NameInput.text = m_Iso.m_HeadInfo.Name;
			if ( m_Iso.m_HeadInfo.Author.Trim().Length > 0 )
			{
				m_AuthorInput.gameObject.SetActive(false);
				m_AuthorInput.text = m_Iso.m_HeadInfo.Author.Trim();
				m_AuthorLabel.text = m_Iso.m_HeadInfo.Author.Trim();
			}
			else
			{
				m_AuthorInput.gameObject.SetActive(true);
				m_AuthorInput.text = "";
				m_AuthorLabel.text = "";
			}
			m_last_input_name = m_NameInput.text;
			m_DescriptionInput.text = m_Iso.m_HeadInfo.Desc;
			m_SaveLocInput.text = VCEditor.s_Scene.m_DocumentPath.Substring(0, VCEditor.s_Scene.m_DocumentPath.Length - VCConfig.s_IsoFileExt.Length);
			m_IconTex = new Texture2D (2,2);
			m_IconTex.LoadImage(m_Iso.m_HeadInfo.IconTex);
			m_IconMat.mainTexture = m_IconTex;
			GuessFilepath();
		}
	}
	
	public void GuessFilepath ()
	{
		string n = m_NameInput.text;
		if ( n.Trim().Length < 1 )
		{
			n = "Untitled";
		}
		string filename = VCUtils.MakeFileName(n);
		string s = m_SaveLocInput.text;
		int lstidx = Mathf.Max(s.LastIndexOf("\\"), s.LastIndexOf("/"));
		if ( lstidx >= 0 )
			m_SaveLocInput.text = s.Substring(0, lstidx+1) + filename;
		else
			m_SaveLocInput.text = filename;
		
		m_SaveLocInput.text = m_SaveLocInput.text.Trim();
		string test_fn = m_SaveLocInput.text;
		int p_pos = test_fn.LastIndexOf(" (");
		int n_pos = test_fn.LastIndexOf(")");
		int i = 2;
		if ( p_pos > 0 && n_pos == test_fn.Length - 1 )
		{
			try
			{
				i = Convert.ToInt32(test_fn.Substring(p_pos + 2, n_pos - p_pos - 2));
				test_fn = test_fn.Substring(0, p_pos);
			}
			catch (Exception) { i = 2; }
		}
		while ( File.Exists(VCConfig.s_IsoPath + m_SaveLocInput.text + VCConfig.s_IsoFileExt) 
			&& m_SaveLocInput.text + VCConfig.s_IsoFileExt != VCEditor.s_Scene.m_DocumentPath )
		{
			m_SaveLocInput.text = test_fn + " (" + i.ToString() + ")";
			++i;
		}
	}
	
	string m_last_input_name = "";
	void Update ()
	{
		if ( Input.GetKeyDown(KeyCode.F10) )
		{
			OnCaptureClick();
		}
		if ( m_last_input_name != m_NameInput.text )
		{
			m_last_input_name = m_NameInput.text;
			//if ( !File.Exists(VCConfig.s_IsoPath + VCEditor.s_Scene.m_DocumentPath) )
			GuessFilepath();
		}
		if ( m_PopupColorWnd != null )
		{
			m_ColorRect.color = m_PopupColorWnd.m_ColorPick.FinalColor;
		}
		string savepath = m_SaveLocInput.text.Trim();
		if ( savepath.Length > 0 )
		{
			if ( savepath[0] == '/' || savepath[0] == '\\' || savepath[0] == '.' )
			{
				m_SaveBtn.gameObject.SetActive(false);
				m_ErrorLabel.text = "Invalid file save path".ToLocalizationString() + " !";
			}
			else
			{
				FileInfo fi = new FileInfo (VCConfig.s_IsoPath + m_SaveLocInput.text);
				string filename = fi.Name.Trim();
				if ( filename.Length > 0 )
				{
					if ( filename[0] == '.' )
					{
						m_SaveBtn.gameObject.SetActive(false);
						m_ErrorLabel.text = "Invalid file save path".ToLocalizationString() + " !";
					}
					else
					{
						m_SaveBtn.gameObject.SetActive(true);
						m_ErrorLabel.text = "";
					}
				}
				else
				{
					m_SaveBtn.gameObject.SetActive(false);
					m_ErrorLabel.text = "Invalid file save path".ToLocalizationString() + " !";
				}
			}
		}
		else
		{
			m_SaveBtn.gameObject.SetActive(false);
			m_ErrorLabel.text = "No save path specified".ToLocalizationString() + " !";
		}
		m_Iso.m_HeadInfo.Name = m_NameInput.text;
		m_Iso.m_HeadInfo.Desc = m_DescriptionInput.text;

		// Capture camera
		m_CaptureCamera.transform.position = VCEditor.Instance.m_MainCamera.transform.position;
		m_CaptureCamera.transform.rotation = VCEditor.Instance.m_MainCamera.transform.rotation;
		m_CaptureCamera.backgroundColor = m_ColorRect.color;
		m_CaptureCamera.aspect = 1;
		m_CaptureCamera.nearClipPlane = VCEditor.Instance.m_MainCamera.nearClipPlane;
		m_CaptureCamera.farClipPlane = VCEditor.Instance.m_MainCamera.farClipPlane;
		m_CaptureCamera.fieldOfView = VCEditor.Instance.m_MainCamera.fieldOfView;
	}
	
	public void OnCancelClick ()
	{
		Hide();
	}
	
	public static string s_SaveTargetForOverwrite = "";
	public static void DoSaveForOverwrite()
	{
		bool result = VCEditor.s_Scene.SaveIsoAs(s_SaveTargetForOverwrite);
		VCEMsgBox.Show(result ? VCEMsgBoxType.SAVE_OK : VCEMsgBoxType.SAVE_FAILED);
	}
	
	public void OnSaveClick ()
	{
		m_Iso.m_HeadInfo.Author = m_AuthorInput.text;
		string targetpath = VCConfig.s_IsoPath + m_SaveLocInput.text + VCConfig.s_IsoFileExt;
		FileInfo fi_doc = new FileInfo (VCConfig.s_IsoPath + VCEditor.s_Scene.m_DocumentPath);
		FileInfo fi_tar = new FileInfo (targetpath);
		bool result = false;
		if ( fi_doc.FullName.ToLower() == fi_tar.FullName.ToLower() )
		{
			result = VCEditor.s_Scene.SaveIso();
			VCEMsgBox.Show(result ? VCEMsgBoxType.SAVE_OK : VCEMsgBoxType.SAVE_FAILED);
		}
		else
		{
			if ( File.Exists(fi_tar.FullName) )
			{
				s_SaveTargetForOverwrite = m_SaveLocInput.text + VCConfig.s_IsoFileExt;
				VCEMsgBox.Show(VCEMsgBoxType.REPLACE_QUERY);
			}
			else
			{
				result = VCEditor.s_Scene.SaveIsoAs(m_SaveLocInput.text + VCConfig.s_IsoFileExt);
				VCEMsgBox.Show(result ? VCEMsgBoxType.SAVE_OK : VCEMsgBoxType.SAVE_FAILED);
			}
		}
		Hide();
	}
	public void OnCaptureClick ()
	{
		if ( m_IconTex != null )
		{
			Texture2D.Destroy(m_IconTex);
			m_IconMat.mainTexture = null;
		}
		m_IconTex = new Texture2D ( 256,256,TextureFormat.ARGB32, true );
		m_IconMat.mainTexture = m_IconTex;
		RenderTexture.active = m_CaptureTex;
		m_IconTex.ReadPixels( new Rect( 0,0,256,256 ), 0,0, true );
		m_IconTex.Apply();
		RenderTexture.active = null;
		m_IconTex.filterMode = FilterMode.Trilinear;
		m_Iso.m_HeadInfo.IconTex = m_IconTex.EncodeToJPG(25);
		VCEditor.Instance.m_UI.m_StatPanel.SetIsoIcon();
	}
	public void OnBGColorClick ()
	{
		m_PopupColorWnd = VCEditor.Instance.m_UI.ShowColorPickWindow(m_ColorWndParent, m_ColorWndPos, m_ColorRect.color);
	}
}
