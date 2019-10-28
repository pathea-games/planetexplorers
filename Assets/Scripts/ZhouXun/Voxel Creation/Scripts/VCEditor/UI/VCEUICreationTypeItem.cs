using UnityEngine;
using System.Collections;
using WhiteCat;

public class VCEUICreationTypeItem : MonoBehaviour
{
	public int usage = 0;
	[HideInInspector] public VCESceneSetting m_Scene;
	public VCEUIFastCreateWnd m_ParentWnd;
	public UIEventListener m_Button;
	public UILabel m_NameLabel;
	public UISprite m_Icon;

	Vector3 offset;

	void Start()
	{
		if (usage == 0)
		{
			m_NameLabel.text = m_Scene.m_Name.ToLocalizationString();
			m_Icon.spriteName = "scenes/" + m_Scene.m_Id;
		}
		else if (usage == 1)
		{
			m_Icon.spriteName = "scenes/new";
		}
		else if (usage == 2)
		{
			m_Icon.spriteName = "scenes/load";
		}
		else if (usage == 3)
		{
			m_Icon.spriteName = "scenes/template";
		}
		else if (usage == 4)
		{
			m_Icon.spriteName = "scenes/empty";
		}
		offset = transform.localPosition - new Vector3(-100, 100, 0);
	}

	public void FadeOut ()
	{
		fadeTarget = 0;
		m_Button.onClick -= m_ParentWnd.OnItemClick;
	}

	public void FadeIn ()
	{
		m_Button.onClick += m_ParentWnd.OnItemClick;
		fadeTarget = 1;
	}

	float fade = 0;
	float fadeTarget = 1;

	void LateUpdate ()
	{
		fade = Mathf.Lerp(fade, fadeTarget, 0.15f);
		if (fadeTarget == 0 && fade < 0.005f)
			GameObject.Destroy(gameObject);
		
		if (fade > 0.995f && fade != 1)
		{
			fade = 1;
			m_NameLabel.alpha = fade * fade;
			m_Icon.alpha = fade * fade;
		}

		if (fade != 1)
		{
			if (fadeTarget == 0)
				m_Button.GetComponent<UISlicedSprite>().alpha = 0;
			m_NameLabel.alpha = fade * fade;
			m_Icon.alpha = fade * fade;

			if (fadeTarget == 0)
			{
				transform.localPosition = new Vector3(-100, 100, 0) + offset * (1.5f - fade * 0.5f);
			}
		}
	}
}
