//using UnityEngine;
//using System.Collections;

//public class VCDecalProperty : VCPart
//{
//	private VCDecalHandler m_Handler;
//	void Start ()
//	{
//		m_Handler = GetComponent<VCDecalHandler>();
//	}
//	public override string Desc ()
//	{
//		if ( m_Handler == null )
//			return "Error : No handler";
//		string s = "";
//		s += ("Resource: " + m_Handler.m_Guid.ToString("X").PadLeft(16, '0') + "\r\n\r\n");
//		s += ("Size: " + VCUtils.LengthToString(m_Handler.m_Size) + "\r\n");
//		s += ("Depth: " + VCUtils.LengthToString(m_Handler.m_Depth));

//		return s;
//	}
//}
