using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GLAirbrushCursor : GLBehaviour
{
	//public Color m_Color;
	//public RaycastHit m_Rch;
	//List<Vector3> m_Vertices = new List<Vector3> ();
	public VCEFreeAirbrush m_Airbrush;
	
	public void Update ()
	{
		
	}
	
	void OnGUI ()
	{
		GUI.skin = VCEditor.Instance.m_GUISkin;
		if ( m_Airbrush.DisplayCursor )
		{
			GUI.color = m_Airbrush.m_UIColor;
			GUI.Label(new Rect(Input.mousePosition.x - 16, Screen.height - Input.mousePosition.y - 16, 32, 32), "", m_Airbrush.m_Eraser ? "AirbrushEraser" : "AirbrushPaint");
		}
	}
	
	public override void OnGL ()
	{
		
	}
}
