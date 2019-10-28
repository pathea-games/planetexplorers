using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CreationDrivingTips : MonoBehaviour
{
	private static CreationDrivingTips s_Instance;
	public static CreationDrivingTips Instance { get { return s_Instance; } }
	public UILabel m_TipLabel;
	public UILabel m_WarningLabel;
	private List<string> m_Warnings;

	public string m_TestTip;
	public string m_TestWarning;
	public bool m_TestShowTip = false;
	public bool m_TestShowWarning = false;
	public bool m_TestHideWarning = false;

	private float m_TipAliveTime = 0f;

	// Use this for initialization
	void Awake ()
	{
		s_Instance = this;
		m_TipLabel.text = "";
		m_TipLabel.color = new Color(1,1,1,0);
		m_WarningLabel.text = "";
		m_WarningLabel.color = new Color(1,0,0,0);
		m_TipAliveTime = 0f;
		m_Warnings = new List<string> ();
	}

	void OnEnable ()
	{
		s_Instance = this;
		m_TipLabel.text = "";
		m_TipLabel.color = new Color(1,1,1,0);
		m_WarningLabel.text = "";
		m_WarningLabel.color = new Color(1,0,0,0);
		m_TipAliveTime = 0f;
		m_Warnings = new List<string> ();
	}

	void OnDestroy ()
	{
		s_Instance = null;
	}

	// Update is called once per frame
	void Update ()
	{
		if ( m_TestShowTip )
		{
			m_TestShowTip = false;
			ShowTip(m_TestTip, 3f);
		}
		if ( m_TestShowWarning )
		{
			m_TestShowWarning = false;
			ShowWarning(m_TestWarning);
		}
		if ( m_TestHideWarning )
		{
			m_TestHideWarning = false;
			HideWarning();
		}
		m_TipAliveTime -= Time.deltaTime;
		if ( m_TipAliveTime <= 0 )
			HideTip();
		if ( m_Warnings.Count > 0 )
		{
			ShowWarning("");
			int t = Mathf.FloorToInt(Time.time/1.6f);
			t = t % m_Warnings.Count;
			m_WarningLabel.text = m_Warnings[t];
		}
		else
		{
			HideWarning();
		}
	}

	public void ShowTip (string text, float duration)
	{
		m_TipLabel.transform.localPosition = new Vector3(0,0,0);
		m_TipLabel.pivot = UIWidget.Pivot.Bottom;
		m_TipLabel.text = text;
		m_TipLabel.GetComponent<TweenColor>().Play(true);
		m_TipAliveTime = duration;
	}

	public void HideTip ()
	{
		m_TipLabel.GetComponent<TweenColor>().Play(false);
	}
	
	public void ClearWarning ()
	{
		m_Warnings.Clear();
	}
	public void AddWarning (string text)
	{
		m_Warnings.Add(text);
	}

	public void ShowWarning (string text)
	{
		m_WarningLabel.text = text;
		m_WarningLabel.GetComponent<TweenColor>().Play(true);
	}

	public void HideWarning ()
	{
		m_WarningLabel.GetComponent<TweenColor>().Play(false);
	}
}
