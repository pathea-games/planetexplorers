using UnityEngine;
using System.Collections;

//public class VCPVtolCockpitFunc : VCPCockpitFunc
//{
//	public VCPVtolCockpitProperty m_Property;
////	public HelicopterController m_Controller;
//	public float m_DefaultLandHeight = 32f;
//	public Texture2D m_LandHeightMap;
//	public Vector3 m_OriginOffset;
//	public float m_MeterPerPixel;
//	public float m_MaxHeight;
//	public float m_SeaHeight;
//	public float m_AtmosLogBase = 40f;
	/*
	public float LandHeight
	{
		get
		{
			if ( m_Controller == null )
				return 0;
			if ( m_Controller.m_UseLandHeightMap )
			{
				Vector3 m_ReletePos = transform.position - m_OriginOffset;
				int px = Mathf.Clamp(Mathf.FloorToInt(m_ReletePos.x / m_MeterPerPixel), 0, m_LandHeightMap.width - 1);
				int py = Mathf.Clamp(Mathf.FloorToInt(m_ReletePos.z / m_MeterPerPixel), 0, m_LandHeightMap.height - 1);
				Color color = m_LandHeightMap.GetPixel(px, py);
				
				return Mathf.Max(color.g * m_MaxHeight, m_SeaHeight);
			}
			else
			{
				return m_DefaultLandHeight;
			}
		}
	}
	public float FlyingHeight { get { return transform.position.y - LandHeight; } }
	public float AtmosphereCoef
	{
		get
		{
			float h = Mathf.Max(0, FlyingHeight - m_AtmosLogBase * 0.5f);
			float level = h / m_AtmosLogBase;
			return Mathf.Pow(0.5f, level);
		}
	}
	*/
//}
