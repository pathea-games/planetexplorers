using UnityEngine;
using System.Collections;
using System.IO;

public class VCEUIIsoItem : MonoBehaviour
{
	public VCEUIIsoList m_ParentList;
	public bool m_IsFolder = false;
	public string m_FilePath;
	public UISprite m_IsoDefaultSprite;
	public UISprite m_FolderSprite;
	public UITexture m_IsoIcon;
	
	public Material m_IsoIconMatRes;
	private Material m_IsoIconMat;
	private Texture2D m_IsoIconTex;
	
	public UILabel m_NameLabel;
	public GameObject m_HoverBtn;
	public GameObject m_SelectedSign;
	public GameObject m_DelBtn;

	private string m_Extension;
	
	// Use this for initialization
	void Start ()
	{
		m_IsoDefaultSprite.gameObject.SetActive(!m_IsFolder);
		m_FolderSprite.gameObject.SetActive(m_IsFolder);
		m_SelectedSign.SetActive(false);
		
		if ( m_IsFolder )
		{
			m_NameLabel.text = new DirectoryInfo(m_FilePath).Name + "\r\n[808080]Folder[-]";
			m_Extension = "";
		}
		else
		{
			string filename = new FileInfo(m_FilePath).Name;
			string extension = new FileInfo(m_FilePath).Extension;
			m_Extension = extension;
			int lstidx = filename.LastIndexOf(".");
			filename = filename.Substring(0, lstidx >= 0 ? lstidx : filename.Length);
            //lw:2017.1.7:ArgumentOutOfRangeException: Cannot be negative. Parameter name: length
            //System.String.Substring (Int32 startIndex, Int32 length)
            //VCEUIIsoItem.Start()
            string showExtension = string.IsNullOrEmpty(extension) || extension.Length <=1 ? "null" : extension.Substring(1, extension.Length - 1);
            m_NameLabel.text = filename + "\r\n[808080]" + showExtension.ToUpper() + " File[-]";
			if ( extension == VCConfig.s_IsoFileExt )
			{
				m_IsoDefaultSprite.color = Color.white;
			}
			else if ( extension == VCConfig.s_ObsoleteIsoFileExt )
			{
				m_IsoDefaultSprite.color = new Color (1.0f, 0.5f, 0.5f);
			}
			else
			{
				m_IsoDefaultSprite.gameObject.SetActive(false);
			}
		}
	}
	
	void OnDestroy()
	{
		if ( m_IsoIconMat != null )
		{
			Material.Destroy(m_IsoIconMat);
			m_IsoIconMat = null;
		}
		if ( m_IsoIconTex != null )
		{
			Texture2D.Destroy(m_IsoIconTex);
			m_IsoIconTex = null;
		}
	}
	
	// Update is called once per frame
	void Update ()
	{
		m_SelectedSign.SetActive(m_ParentList.m_SelectedItem == this);
		if (m_IsFolder)
			m_DelBtn.SetActive(false);
		else
			m_DelBtn.SetActive(m_ParentList.m_SelectedItem == this);
	}
	
	void OnSelectClick()
	{
		m_ParentList.m_SelectedItem = this;
		if ( m_Extension == VCConfig.s_IsoFileExt || m_Extension.Length == 0 )
			VCEStatusBar.ShowText("Double-click to open".ToLocalizationString(), 2);
		else if ( m_Extension == VCConfig.s_ObsoleteIsoFileExt )
			VCEStatusBar.ShowText("This ISO version is NOT compatible with the current build, please download the converter from our website".ToLocalizationString() + "!", Color.red, 20);
		else
			VCEStatusBar.ShowText("Corrupt file".ToLocalizationString(), 2);
	}
	
	public static string s_IsoToLoad = "";
	public static void DoLoadFromMsgBox()
	{
		VCEditor.LoadIso(s_IsoToLoad);
	}
	
	void OnSelectDbClick()
	{
		if ( m_IsFolder )
		{
			if ( Directory.Exists(m_FilePath) )
			{
				m_ParentList.m_Path = m_FilePath;
				m_ParentList.RefreshIsoList();
			}
			else
			{
				VCEStatusBar.ShowText("This folder does not exist".ToLocalizationString() + "!", 2f, false);
				m_ParentList.RefreshIsoList();
			}
		}
		else
		{
			if ( File.Exists(m_FilePath) )
			{
				if ( VCEHistory.s_Modified )
				{
					s_IsoToLoad = m_FilePath.Substring(VCConfig.s_IsoPath.Length);
					VCEMsgBox.Show(VCEMsgBoxType.LOAD_QUERY);
				}
				else
				{
					VCEditor.LoadIso(m_FilePath.Substring(VCConfig.s_IsoPath.Length));
				}
			}
			else
			{
				VCEMsgBox.Show(VCEMsgBoxType.MISSING_ISO);
				VCEStatusBar.ShowText("ISO file is missing".ToLocalizationString() + "!", 2f, false);
				m_ParentList.RefreshIsoList();
			}
		}
	}
	
	public static string s_IsoToDelete = "";
	public static void DoDeleteFromMsgBox()
	{
        try
        {
            File.Delete(s_IsoToDelete);
        } catch { }

		VCEditor.Instance.m_UI.m_IsoList.RefreshIsoList();
	}
	void OnDelClick ()
	{
		s_IsoToDelete = m_FilePath;
		VCEMsgBox.Show(VCEMsgBoxType.DELETE_ISO);
	}
}
