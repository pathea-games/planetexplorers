using UnityEngine;
using System.Collections;

public class RTreePlaceHolderInfo
{
	public RTreePlaceHolderInfo( Vector3 offset, float heightscale, float widthscale )
	{
		m_Offset = offset;
		m_HeightScale = heightscale;
		m_WidthScale = widthscale;
	}
	
	public Vector3 m_Offset;
	public Vector3 TerrOffset { get { return new Vector3(m_Offset.x/RSubTerrConstant.TerrainSize.x, m_Offset.y/RSubTerrConstant.TerrainSize.y, m_Offset.z/RSubTerrConstant.TerrainSize.z); } }
	public float m_HeightScale;
	public float m_WidthScale;
}
