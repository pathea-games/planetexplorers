using UnityEngine;
using System.Collections;

public class VCEMirrorSetting
{
	public bool m_XPlane = false;
	public bool m_YPlane = false;
	public bool m_ZPlane = false;
	public bool m_XYAxis = false;
	public bool m_YZAxis = false;
	public bool m_ZXAxis = false;
	public bool m_Point = false;

	public byte m_Mask = 7;
	public bool XPlane_Masked { get { return m_XPlane && (m_Mask & 1) == 1; } }
	public bool YPlane_Masked { get { return m_YPlane && (m_Mask & 2) == 2; } }
	public bool ZPlane_Masked { get { return m_ZPlane && (m_Mask & 4) == 4; } }
	public bool XYAxis_Masked { get { return m_XYAxis && (m_Mask & 3) == 3; } }
	public bool YZAxis_Masked { get { return m_YZAxis && (m_Mask & 6) == 6; } }
	public bool ZXAxis_Masked { get { return m_ZXAxis && (m_Mask & 5) == 5; } }
	public bool Point_Masked  { get { return m_Point  && (m_Mask & 7) == 7; } }

	public bool Enabled
	{
		get
		{
			return (m_XPlane || m_YPlane || m_ZPlane ||
			        m_XYAxis || m_YZAxis || m_ZXAxis || m_Point);
		}
	}
	public bool Enabled_Masked
	{
		get
		{
			return (XPlane_Masked || YPlane_Masked || ZPlane_Masked ||
			        XYAxis_Masked || YZAxis_Masked || ZXAxis_Masked || Point_Masked);
		}
	}

	public int MirrorCount
	{
		get
		{
			// Assume the mirror setting is valid.
			return ((m_XPlane ? 1 : 0) +
			        (m_YPlane ? 1 : 0) +
			        (m_ZPlane ? 1 : 0) +
			        (m_XYAxis ? 1 : 0) +
			        (m_YZAxis ? 1 : 0) +
			        (m_ZXAxis ? 1 : 0) +
			        (m_Point ? 1 : 0));
		}
	}
	
	public int MirrorCount_Masked
	{
		get
		{
			// Assume the mirror setting is valid.
			return ((XPlane_Masked ? 1 : 0) +
			        (YPlane_Masked ? 1 : 0) +
			        (ZPlane_Masked ? 1 : 0) +
			        (XYAxis_Masked ? 1 : 0) +
			        (YZAxis_Masked ? 1 : 0) +
			        (ZXAxis_Masked ? 1 : 0) +
			        (Point_Masked ? 1 : 0));
		}
	}

	public float m_PosX = 0.0f;
	public float m_PosY = 0.0f;
	public float m_PosZ = 0.0f;

	private float m_VoxelPosX = 0.0f;
	private float m_VoxelPosY = 0.0f;
	private float m_VoxelPosZ = 0.0f;
	
	private int m_ColorPosX = 0;
	private int m_ColorPosY = 0;
	private int m_ColorPosZ = 0;
	
	private float m_WorldPosX = 0.0f;
	private float m_WorldPosY = 0.0f;
	private float m_WorldPosZ = 0.0f;
	public Vector3 WorldPos { get { return new Vector3(m_WorldPosX,m_WorldPosY,m_WorldPosZ); } }

	public void Reset (int size_x, int size_y, int size_z)
	{
		m_XPlane = false;
		m_YPlane = false;
		m_ZPlane = false;
		m_XYAxis = false;
		m_YZAxis = false;
		m_ZXAxis = false;
		m_Point = false;
		m_Mask = 7;
		m_PosX = (float)(size_x) * 0.5f;
		m_PosY = (float)(size_y) * 0.5f;
		m_PosZ = (float)(size_z) * 0.5f;
		if ( m_PosY > 50f )
			m_PosY = 50f;
	}
	public void Validate ()
	{
		if ( m_Point )
		{
			m_XPlane = m_YPlane = m_ZPlane = false;
			m_XYAxis = m_YZAxis = m_ZXAxis = false;
		}
		if ( m_XYAxis )
		{
			m_XPlane = m_YPlane = false;
		}
		if ( m_YZAxis )
		{
			m_YPlane = m_ZPlane = false;
		}
		if ( m_ZXAxis )
		{
			m_ZPlane = m_XPlane = false;
		}
	}

	//
	// ----------------------------- Mirror Calculation -----------------------------
	//
	public IntVector3[] Output = new IntVector3[8];
	public VCComponentData[] ComponentOutput = new VCComponentData[8];
	public int OutputCnt = 0;

	public void CalcPrepare (float voxel_size)
	{
		Validate();
		m_VoxelPosX = m_PosX - 0.5f;
		m_VoxelPosY = m_PosY - 0.5f;
		m_VoxelPosZ = m_PosZ - 0.5f;
		m_ColorPosX = Mathf.RoundToInt(m_PosX*2 + 1);
		m_ColorPosY = Mathf.RoundToInt(m_PosY*2 + 1);
		m_ColorPosZ = Mathf.RoundToInt(m_PosZ*2 + 1);
		m_WorldPosX = m_PosX * voxel_size;
		m_WorldPosY = m_PosY * voxel_size;
		m_WorldPosZ = m_PosZ * voxel_size;
		for ( int i = 0; i < 8; ++i )
		{
			Output[i] = new IntVector3 (0,0,0);
			ComponentOutput[i] = null;
		}
	}

	public void MirrorVoxel (IntVector3 ipos)
	{
		Output[0].x = ipos.x;
		Output[0].y = ipos.y;
		Output[0].z = ipos.z;
		OutputCnt = 1;

		if ( m_XPlane )
			MirrorVoxelOnce(1);
		if ( m_YPlane )
			MirrorVoxelOnce(2);
		if ( m_ZPlane )
			MirrorVoxelOnce(4);
		if ( m_XYAxis )
			MirrorVoxelOnce(3);
		if ( m_YZAxis )
			MirrorVoxelOnce(6);
		if ( m_ZXAxis )
			MirrorVoxelOnce(5);
		if ( m_Point )
			MirrorVoxelOnce(7);
	}
	public void MirrorColor (IntVector3 ipos)
	{
		Output[0].x = ipos.x;
		Output[0].y = ipos.y;
		Output[0].z = ipos.z;
		OutputCnt = 1;
		
		if ( m_XPlane )
			MirrorColorOnce(1);
		if ( m_YPlane )
			MirrorColorOnce(2);
		if ( m_ZPlane )
			MirrorColorOnce(4);
		if ( m_XYAxis )
			MirrorColorOnce(3);
		if ( m_YZAxis )
			MirrorColorOnce(6);
		if ( m_ZXAxis )
			MirrorColorOnce(5);
		if ( m_Point )
			MirrorColorOnce(7);
	}
	public void MirrorComponent (VCComponentData cdata)
	{
		ComponentOutput[0] = cdata.Copy();
		OutputCnt = 1;

		if ( XPlane_Masked )
			MirrorComponentOnce(1);
		if ( YPlane_Masked )
			MirrorComponentOnce(2);
		if ( ZPlane_Masked )
			MirrorComponentOnce(4);
		if ( XYAxis_Masked )
			MirrorComponentOnce(3);
		if ( YZAxis_Masked )
			MirrorComponentOnce(6);
		if ( ZXAxis_Masked )
			MirrorComponentOnce(5);
		if ( Point_Masked )
			MirrorComponentOnce(7);
	}
	private void MirrorVoxelOnce (int mirror)
	{
		for ( int i = 0; i < OutputCnt; ++i )
		{
			Output[i+OutputCnt].x = ((mirror & 1) > 0)  ?  ( (int)(2.0f*m_VoxelPosX + 0.001f) - Output[i].x )  :  (Output[i].x);
			Output[i+OutputCnt].y = ((mirror & 2) > 0)  ?  ( (int)(2.0f*m_VoxelPosY + 0.001f) - Output[i].y )  :  (Output[i].y);
			Output[i+OutputCnt].z = ((mirror & 4) > 0)  ?  ( (int)(2.0f*m_VoxelPosZ + 0.001f) - Output[i].z )  :  (Output[i].z);
		}
		OutputCnt = OutputCnt << 1;
	}
	private void MirrorColorOnce (int mirror)
	{
		for ( int i = 0; i < OutputCnt; ++i )
		{
			Output[i+OutputCnt].x = ((mirror & 1) > 0)  ?  (( m_ColorPosX << 1 ) - Output[i].x)  :  (Output[i].x);
			Output[i+OutputCnt].y = ((mirror & 2) > 0)  ?  (( m_ColorPosY << 1 ) - Output[i].y)  :  (Output[i].y);
			Output[i+OutputCnt].z = ((mirror & 4) > 0)  ?  (( m_ColorPosZ << 1 ) - Output[i].z)  :  (Output[i].z);
		}
		OutputCnt = OutputCnt << 1;
	}
	private void MirrorComponentOnce (int mirror)
	{
		for ( int i = 0; i < OutputCnt; ++i )
		{
			VCComponentData real = ComponentOutput[i];
			VCComponentData image = ComponentOutput[i].Copy();
			ComponentOutput[i+OutputCnt] = image;
			image.m_Position.x = ((mirror & 1) > 0)  ?  (2.0f * m_WorldPosX - real.m_Position.x)  :  (real.m_Position.x);
			image.m_Position.y = ((mirror & 2) > 0)  ?  (2.0f * m_WorldPosY - real.m_Position.y)  :  (real.m_Position.y);
			image.m_Position.z = ((mirror & 4) > 0)  ?  (2.0f * m_WorldPosZ - real.m_Position.z)  :  (real.m_Position.z);

			bool xsym = true;
			if ( image is VCPartData )
				xsym = (VCConfig.s_Parts[image.m_ComponentId].m_Symmetric == 1);
			if ( image is VCDecalData )
				xsym = true;

			if ( xsym )
			{
				if ( (mirror & 1) > 0 )
				{
					//image.m_Rotation.x = image.m_Rotation.x;
					image.m_Rotation.y = -image.m_Rotation.y;
					image.m_Rotation.z = -image.m_Rotation.z;
					if ( image is IVCMultiphaseComponentData )
						(image as IVCMultiphaseComponentData).InversePhase();
				}
				if ( (mirror & 2) > 0 )
				{
					image.m_Rotation.x = -image.m_Rotation.x;
					//image.m_Rotation.y = image.m_Rotation.y;
					image.m_Rotation.z = 180f-image.m_Rotation.z;
					if ( image is IVCMultiphaseComponentData )
						(image as IVCMultiphaseComponentData).InversePhase();
				}
				if ( (mirror & 4) > 0 )
				{
					//image.m_Rotation.x = image.m_Rotation.x;
					image.m_Rotation.y = 180f-image.m_Rotation.y;
					image.m_Rotation.z = -image.m_Rotation.z;
					if ( image is IVCMultiphaseComponentData )
						(image as IVCMultiphaseComponentData).InversePhase();
				}
			}
			else
			{
				if ( (mirror & 1) > 0 )
				{
					if ( image is IVCMultiphaseComponentData )
						(image as IVCMultiphaseComponentData).InversePhase();
				}
			}

			image.m_Rotation = VCEMath.NormalizeEulerAngle(image.m_Rotation);
		}
		OutputCnt = OutputCnt << 1;
	}
}
