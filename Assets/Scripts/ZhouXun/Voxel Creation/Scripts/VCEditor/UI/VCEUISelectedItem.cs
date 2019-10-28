using UnityEngine;
using System.Collections;

public class VCEUISelectedItem : MonoBehaviour
{
	public GameObject m_UIGroup;
	public UISprite m_ComponentIcon;
	public UITexture m_MaterialIcon;
	public UISprite m_ColorIcon;
	public UILabel m_ItemNameLabel;
	public UITweener m_GlowIcon;
	public GameObject m_IconMask;
	public Material m_SourceIconTexture;
	private Material m_MaterialIconTexture;
	
	// Use this for initialization
	void Start ()
	{
		m_MaterialIconTexture = Material.Instantiate(m_SourceIconTexture) as Material;
	}
	
	private VCMaterial m_lastMat = null;
	private VCPartInfo m_lastPart = null;
	private VCDecalAsset m_lastDecal = null;
	private Color m_lastColor = Color.clear;
	// Update is called once per frame
	void Update ()
	{
		if ( !VCEditor.DocumentOpen() )
			return;
		if ( VCEditor.Instance.m_UI.m_MaterialTab.isChecked )
		{
			if ( VCEditor.SelectedMaterial != null )
			{
				if ( m_lastMat != VCEditor.SelectedMaterial )
				{
					m_UIGroup.SetActive(true);
					m_MaterialIconTexture.mainTexture = VCEditor.SelectedMaterial.m_DiffuseTex;
					m_MaterialIcon.material = m_MaterialIconTexture;
					m_IconMask.GetComponent<UITweener>().Reset();
					m_IconMask.GetComponent<UITweener>().Play(true);
					m_GlowIcon.Reset();
					m_GlowIcon.Play(true);
					m_MaterialIcon.gameObject.SetActive(true);
					m_ComponentIcon.gameObject.SetActive(false);
					m_ColorIcon.gameObject.SetActive(false);
					m_ItemNameLabel.text = VCEditor.SelectedMaterial.m_Name;
					m_IconMask.SetActive(true);
				}
			}
			else
			{
				m_UIGroup.SetActive(false);
			}
			m_lastMat = VCEditor.SelectedMaterial;
			m_lastDecal = null;
			m_lastPart = null;
			m_lastColor = Color.clear;
		}
		else if ( VCEditor.Instance.m_UI.m_DecalTab.isChecked )
		{
			if ( VCEditor.SelectedDecal != null )
			{
				if ( m_lastDecal != VCEditor.SelectedDecal )
				{
					m_UIGroup.SetActive(true);
					m_MaterialIconTexture.mainTexture = VCEditor.SelectedDecal.m_Tex;
					m_MaterialIcon.material = m_MaterialIconTexture;
					m_IconMask.GetComponent<UITweener>().Reset();
					m_IconMask.GetComponent<UITweener>().Play(true);
					m_GlowIcon.Reset();
					m_GlowIcon.Play(true);
					m_MaterialIcon.gameObject.SetActive(true);
					m_ComponentIcon.gameObject.SetActive(false);
					m_ColorIcon.gameObject.SetActive(false);
					m_ItemNameLabel.text = "1 " + "decal".ToLocalizationString() + "  [999999](UID = "+ VCEditor.SelectedDecal.GUIDString +")[-]";
					m_IconMask.SetActive(true);
				}
			}
			else
			{
				m_UIGroup.SetActive(false);
			}
			m_lastDecal = VCEditor.SelectedDecal;
			m_lastMat = null;
			m_lastPart = null;
			m_lastColor = Color.clear;
		}
		else if ( VCEditor.Instance.m_UI.m_PartTab.isChecked )
		{
			if ( VCEditor.SelectedPart != null )
			{
				if ( m_lastPart != VCEditor.SelectedPart )
				{
					m_ComponentIcon.spriteName = VCEditor.SelectedPart.m_IconPath.Split(',')[0];
					m_IconMask.GetComponent<UITweener>().Reset();
					m_IconMask.GetComponent<UITweener>().Play(true);
					m_GlowIcon.Reset();
					m_GlowIcon.Play(true);
					m_ComponentIcon.gameObject.SetActive(true);
					m_UIGroup.SetActive(true);
					m_MaterialIcon.gameObject.SetActive(false);
					m_ColorIcon.gameObject.SetActive(false);
					m_ItemNameLabel.text = VCEditor.SelectedPart.m_Name;
					m_IconMask.SetActive(true);
				}
			}
			else
			{
				m_UIGroup.SetActive(false);
			}
			m_lastMat = null;
			m_lastDecal = null;
			m_lastPart = VCEditor.SelectedPart;
			m_lastColor = Color.clear;
		}
		else if ( VCEditor.Instance.m_UI.m_PaintTab.isChecked )
		{
			Color color = VCEditor.SelectedColor;
			color.a = 1;
			m_ColorIcon.color = color;
			if ( m_lastColor != color )
			{
				m_IconMask.GetComponent<UITweener>().Reset();
				m_IconMask.GetComponent<UITweener>().Play(true);
				m_GlowIcon.Reset();
				m_GlowIcon.Play(true);
			}
			m_ColorIcon.gameObject.SetActive(true);
			m_UIGroup.SetActive(true);
			m_MaterialIcon.gameObject.SetActive(false);
			m_ComponentIcon.gameObject.SetActive(false);
			m_ItemNameLabel.text = "RGB ( " + (color.r*100).ToString("0") + "%, " + (color.g*100).ToString("0") + "%, " + (color.b*100).ToString("0") + "% )";
			m_IconMask.SetActive(true);
			m_lastMat = null;
			m_lastDecal = null;
			m_lastPart = null;
			m_lastColor = color;
		}
		else
		{
			m_UIGroup.SetActive(false);
			m_lastMat = null;
			m_lastDecal = null;
			m_lastPart = null;
			m_lastColor = Color.clear;
		}
	}
	
	void OnSelectedMatClick ()
	{
		if ( VCEditor.Instance.m_UI.m_MaterialTab.isChecked && VCEditor.SelectedMaterial != null )
		{
			VCEditor.DeselectBrushes();
			VCEditor.Instance.m_UI.m_MaterialWindow.ShowWindow(VCEditor.SelectedMaterial);
		}
	}
}
