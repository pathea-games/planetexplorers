using UnityEngine;
using System;
using System.Collections;

public class VCEUIVector3Input : MonoBehaviour
{
	public UIInput m_XInput;
	public UIInput m_YInput;
	public UIInput m_ZInput;
	public bool m_KeepUniform = false;
    public string m_Format = "0.00";
	public Vector3 m_Vector = Vector3.zero;
	public Vector3 Vector
	{
		get { return m_Vector; }
		set
		{
			if ( !m_XInput.selected )
				m_XInput.text = value.x.ToString(m_Format);
			if ( !m_YInput.selected )
				m_YInput.text = value.y.ToString(m_Format);
			if ( !m_ZInput.selected )
				m_ZInput.text = value.z.ToString(m_Format);
			GetVectorFromInput();
		}
	}
	
	public GameObject m_EventReceiver;
	public string m_OnChangeFuncName = "OnVectorChange";
	
	void GetVectorFromInput ()
	{
		if ( !m_XInput.selected )
			try { m_Vector.x = Convert.ToSingle(m_XInput.text); } catch (Exception) { m_Vector.x = 0; }
		if ( !m_YInput.selected )
			try { m_Vector.y = Convert.ToSingle(m_YInput.text); } catch (Exception) { m_Vector.y = 0; }
		if ( !m_ZInput.selected )
			try { m_Vector.z = Convert.ToSingle(m_ZInput.text); } catch (Exception) { m_Vector.z = 0; }
	}
	
	// Use this for initialization
	void Start ()
	{
		m_lastVector = m_Vector;
	}
	
	// Update is called once per frame
	Vector3 m_lastVector;
	bool m_lastXSelected = false;
	bool m_lastYSelected = false;
	bool m_lastZSelected = false;
	string m_revertXStr = "";
	string m_revertYStr = "";
	string m_revertZStr = "";
	void Update ()
	{
		if ( m_XInput.selected && !m_lastXSelected )
		{
			m_revertXStr = m_XInput.text;
			m_XInput.text = "";
			m_lastXSelected = m_XInput.selected;
		}
		else if ( !m_XInput.selected && m_lastXSelected )
		{
			if ( m_XInput.text.Trim().Length == 0 )
				m_XInput.text = m_revertXStr;
			m_revertXStr = "";
			m_lastXSelected = m_XInput.selected;
			GetVectorFromInput();
			if (m_KeepUniform) m_Vector.y = m_Vector.z = m_Vector.x;
        }
		if ( m_YInput.selected && !m_lastYSelected )
		{
			m_revertYStr = m_YInput.text;
			m_YInput.text = "";
			m_lastYSelected = m_YInput.selected;
		}
		else if ( !m_YInput.selected && m_lastYSelected )
		{
			if ( m_YInput.text.Trim().Length == 0 )
				m_YInput.text = m_revertYStr;
			m_revertYStr = "";
			m_lastYSelected = m_YInput.selected;
			GetVectorFromInput();
			if (m_KeepUniform) m_Vector.x = m_Vector.z = m_Vector.y;
		}
		if ( m_ZInput.selected && !m_lastZSelected )
		{
			m_revertZStr = m_ZInput.text;
			m_ZInput.text = "";
			m_lastZSelected = m_ZInput.selected;
		}
		else if ( !m_ZInput.selected && m_lastZSelected )
		{
			if ( m_ZInput.text.Trim().Length == 0 )
				m_ZInput.text = m_revertZStr;
			m_revertZStr = "";
			m_lastZSelected = m_ZInput.selected;
			GetVectorFromInput();
			if (m_KeepUniform) m_Vector.x = m_Vector.y = m_Vector.z;
		}
		
		if ( m_Vector != m_lastVector )
		{
			if (m_EventReceiver == null) m_EventReceiver = gameObject;
			m_EventReceiver.SendMessage(m_OnChangeFuncName, m_Vector, SendMessageOptions.DontRequireReceiver);
			
			m_lastVector = m_Vector;
		}
		Vector = m_Vector;
	}
}
