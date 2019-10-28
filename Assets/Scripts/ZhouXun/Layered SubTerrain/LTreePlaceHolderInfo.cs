using UnityEngine;
using System;
using System.Collections;

public class LTreePlaceHolderInfo
{
	public LTreePlaceHolderInfo( Vector3 offset, float heightscale, float widthscale )
	{
		m_Offset = offset;
		m_HeightScale = heightscale;
		m_WidthScale = widthscale;
	}
	
	public Vector3 m_Offset;
	public Vector3 TerrOffset { get { return new Vector3(m_Offset.x/LSubTerrConstant.SizeF, m_Offset.y/LSubTerrConstant.HeightF, m_Offset.z/LSubTerrConstant.SizeF); } }
	public float m_HeightScale;
	public float m_WidthScale;
}
