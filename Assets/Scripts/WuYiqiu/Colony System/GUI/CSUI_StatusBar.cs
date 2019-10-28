using UnityEngine;
using System.Collections;

public class CSUI_StatusBar : MonoBehaviour 
{

	private static CSUI_StatusBar s_Instance = null;
	public static CSUI_StatusBar Instance { get { return s_Instance; } }
	
	private string m_text = "";
	private float m_textRemainTime = 0;
	private float m_typeEffectTime = 0;
	private bool m_showTypeEffect = false;
	private Color m_typeEffectColor = new Color(0.0f, 0.2f, 1.0f, 0.0f);
	
	void Awake ()
	{
		s_Instance = this;
	}
	void OnDestroy ()
	{
		s_Instance = null;
	}
	// Use this for initialization
	void Start ()
	{
		
	}
	
	// Update is called once per frame
	void Update ()
	{
		m_textRemainTime -= Time.deltaTime;
		m_typeEffectTime += Time.deltaTime;
		if ( m_textRemainTime < 0 )
		{
			m_textRemainTime = 0;
			m_text = "";
		}
		if ( m_showTypeEffect )
		{
			float time_per_letter = 0.05f;
			int current_letter = (int)(m_typeEffectTime/time_per_letter);
			string temp = "";
			for ( int i = 0; i < m_text.Length && i <= current_letter; ++i )
			{
				if ( i < current_letter - 4 )
				{
					temp += m_text[i];
				}
				else if ( i < current_letter )
				{
					float delta = (current_letter - i) * 0.20f;
					string colorstring = "";
					Color32 c32 = Color.Lerp(m_typeEffectColor, Color.white, delta);
					colorstring += c32.r.ToString("X").PadLeft(2,'0');
					colorstring += c32.g.ToString("X").PadLeft(2,'0');
					colorstring += c32.b.ToString("X").PadLeft(2,'0');
					temp += ("["+colorstring+"]" +  m_text[i] + "[-]");
				}
				else
				{
					string colorstring = "";
					Color32 c32 = m_typeEffectColor;
					colorstring += c32.r.ToString("X").PadLeft(2,'0');
					colorstring += c32.g.ToString("X").PadLeft(2,'0');
					colorstring += c32.b.ToString("X").PadLeft(2,'0');
					temp += ("["+colorstring+"]" +  m_text[i] + "[-]");
				}
			}
			GetComponent<UILabel>().text = temp;
		}
		else
		{
			GetComponent<UILabel>().text = m_text;
		}
	}
	
	public static void ShowText( string text, Color effectColor, float remain = 0f )
	{
		if ( s_Instance == null ) return;
		ShowText(text, remain, true);
		s_Instance.m_typeEffectColor = effectColor;
	}
	public static void ShowText( string text, float remain = 0f, bool typeeffect = false )
	{
		if ( s_Instance == null ) return;
		s_Instance.m_text = text;
		if ( remain == 0f )
			s_Instance.m_textRemainTime = 100000000;
		else
			s_Instance.m_textRemainTime = remain;
		s_Instance.m_typeEffectTime = 0;
		s_Instance.m_showTypeEffect = typeeffect;
	}
	public static void ShowTextLowPriority( string text, Color effectColor, float remain = 0f )
	{
		if ( s_Instance == null ) return;
		if ( s_Instance.m_text.Length == 0 )
			ShowText(text, effectColor, remain);
	}
	public static void ShowTextLowPriority( string text, float remain = 0f, bool typeeffect = false )
	{
		if ( s_Instance == null ) return;
		if ( s_Instance.m_text.Length == 0 )
			ShowText(text, remain, typeeffect);
	}
	public static void ClearText()
	{
		if ( s_Instance == null ) return;
		s_Instance.m_text = "";
		s_Instance.m_textRemainTime = 0;
		s_Instance.m_typeEffectTime = 0;
		s_Instance.m_showTypeEffect = false;
	}
}
