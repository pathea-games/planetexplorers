using UnityEngine;
using System.Collections;
using System.Collections.Generic;
/*
public class CSUI_MenuItem : MonoBehaviour 
{
	[SerializeField]
	private UILabel		m_Description;
	
	public string 		Description		{ get{ return m_Description.text; } set { m_Description.text = value; } }
	
	[SerializeField]
	private Color	m_NormalColor;
	[SerializeField]
	private Color   m_DisabledColor;
	[SerializeField]
	private Color 	m_DumpedColor;

	public object m_Value;
	
	// Reference CS Entity
	//public List<CSEntity>	m_Entities = new List<CSEntity>();
	public CSEntity		m_Entity;
	
	public int m_Type;

	public bool m_Dumped;
	
	void OnActivate(bool active)
	{
		CSUI_Main.Instance.ChangeWindow(this, active);
		CSUI_Main.Instance.m_ActiveMI = this;
	}
	
	// Use this for initialization
	void Start () 
	{
	
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (!m_Dumped)
		{
			if (m_Entity != null )
			{
				if (m_Entity.IsRunning)
					m_Description.color = m_NormalColor;
				else
					m_Description.color = m_DisabledColor;
			}
			else
				m_Description.color = m_NormalColor;
		}
		else
			m_Description.color = m_DumpedColor;
	}
}*/
