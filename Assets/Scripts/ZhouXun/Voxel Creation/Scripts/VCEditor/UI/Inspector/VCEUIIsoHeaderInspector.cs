using UnityEngine;
using System.Collections;
using System.IO;

public class VCEUIIsoHeaderInspector : VCEUIInspector
{
	public GameObject m_HeaderGroup;
	public UILabel m_ErrorMessageLabel;
	public UILabel m_NameLabel;
	public UILabel m_CategoryLabel;
	public UILabel m_SizeLabel;
	public UITexture m_IconTexture;
	public UILabel m_DescriptionLabel;
	public UILabel m_FileSizeLabel;
	public Material m_IconMatRes;
	private Material m_IconMat;
	private Texture2D m_IconTex;
	
	private string m_FilePath = "";
	public string FilePath
	{
		get { return m_FilePath; }
		set
		{
			if ( m_FilePath != value )
			{
				m_FilePath = value;
				Refresh();
			}
		}
	}
	
	void OnDestroy ()
	{
		DestroyIconMat();
	}
	
	void CreateIconMat (byte[] texbuf)
	{
		m_IconMat = Material.Instantiate(m_IconMatRes) as Material;
		m_IconTex = new Texture2D (2,2);
		m_IconTex.LoadImage(texbuf);
		m_IconMat.mainTexture = m_IconTex;
		m_IconTexture.material = m_IconMat;
	}
	void DestroyIconMat ()
	{
		m_IconTexture.material = null;
		if ( m_IconMat != null )
		{
			Material.Destroy(m_IconMat);
			m_IconMat = null;
		}
		if ( m_IconTex != null )
		{
			Texture2D.Destroy(m_IconTex);
			m_IconTex = null;
		}
	}
	
	void Refresh ()
	{
		if ( m_FilePath.Length < 1 )
		{
			m_HeaderGroup.SetActive(false);
			m_ErrorMessageLabel.gameObject.SetActive(false);
			DestroyIconMat();
		}
		else
		{
			VCIsoHeadData isoheader;
			int file_size = VCIsoData.ExtractHeader(m_FilePath, out isoheader);
			if ( file_size > 0 )
			{
				m_ErrorMessageLabel.gameObject.SetActive(false);
				m_NameLabel.color = new Color (1f, 1f, 1f, 1f);
				m_NameLabel.text = isoheader.Name;
				if ( isoheader.Name.Trim().Length < 1 )
				{
					m_NameLabel.color = new Color (0.7f, 0.7f, 0.7f, 0.5f);
					m_NameLabel.text = "< No name >".ToLocalizationString();
				}
				m_CategoryLabel.text = VCConfig.s_Categories[isoheader.Category].m_Name.ToLocalizationString();
				m_SizeLabel.text = "Width".ToLocalizationString() + ": " + isoheader.xSize.ToString()
						+ "\r\n" + "Depth".ToLocalizationString() + ": " + isoheader.zSize.ToString()
						+ "\r\n" + "Height".ToLocalizationString() + ": " + isoheader.ySize.ToString();
				m_DescriptionLabel.color = new Color (1f, 1f, 1f, 1f);
				m_DescriptionLabel.text = isoheader.Desc;
				if ( isoheader.Desc.Trim().Length < 1 )
				{
					m_DescriptionLabel.color = new Color (0.7f, 0.7f, 0.7f, 0.5f);
					m_DescriptionLabel.text = "< No description >".ToLocalizationString();
				}
				string version = "v";
				version += ((isoheader.Version >> 24) & 0xff).ToString();
				version += ".";
				version += ((isoheader.Version >> 16) & 0xff).ToString();
				m_FileSizeLabel.text = "";
				if ( isoheader.Author != null && isoheader.Author.Trim().Length > 0 )
					m_FileSizeLabel.text = "Author".ToLocalizationString() + ": [00FFFF]" + isoheader.Author.Trim() + "[-]\r\n";
				if ( isoheader.Version > VCIsoData.ISO_VERSION )
					m_FileSizeLabel.text += ("ISO " + "Version".ToLocalizationString() + ": [FF0000]" + version + "[-]\r\n");
				else
					m_FileSizeLabel.text += ("ISO " + "Version".ToLocalizationString() + ": [00FFFF]" + version + "[-]\r\n");
				m_FileSizeLabel.text +=	("File Size".ToLocalizationString() + ": [00FFFF]" + (file_size>>10).ToString("#,##0") + " KB[-]");
				CreateIconMat(isoheader.IconTex);
				m_HeaderGroup.SetActive(true);
			}
			else
			{
				m_HeaderGroup.SetActive(false);
				DestroyIconMat();
				
				VCEUIIsoItem isoitem = VCEditor.Instance.m_UI.m_IsoList.m_SelectedItem;
				if ( isoitem != null )
				{
					FileInfo fi = new FileInfo (isoitem.m_FilePath);
					if ( fi.Extension == VCConfig.s_IsoFileExt )
					{
						m_ErrorMessageLabel.text = "Corrupt file data".ToLocalizationString();
					}
					else if ( fi.Extension == VCConfig.s_ObsoleteIsoFileExt )
					{
						m_ErrorMessageLabel.text = "Obsoleted ISO format".ToLocalizationString() + "\r\n\r\n" +
							"[FF8080]" + "This ISO version is NOT compatible with the current build, please download the converter from our website".ToLocalizationString() + "![-]";
					}
					else
					{
						m_ErrorMessageLabel.text = "Unknown file format".ToLocalizationString();
					}
				}
				else
				{
					Debug.LogError("This cannot happen");
				}
				m_ErrorMessageLabel.gameObject.SetActive(true);
			}
		}
	}
}
