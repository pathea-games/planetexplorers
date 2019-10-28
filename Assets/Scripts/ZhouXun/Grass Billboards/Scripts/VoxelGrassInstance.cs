using UnityEngine;
using System.Collections;
using System.IO;

/// <summary>
/// Voxel Grass instance class in [Billboard-set Grass System]. - struct
/// </summary>

public struct VoxelGrassInstance 
{
	public int m_Position_x;
	public float m_Position_y;
	public int m_Position_z;
	public float m_Density;
	public float m_Normal_x;
	public float m_Normal_z;
	public byte m_Color_r;
	public byte m_Color_g;
	public byte m_Color_b;
	public byte m_Prototype;
	
	private static System.Random s_Rand = null;
	private static Vector2[,,] s_RandTable = null;
	private const int c_RandTableSize = 32;
	private const int c_RandTableMask = c_RandTableSize - 1;

	public static void Init ()
	{
		s_Rand = new System.Random (1000);
		s_RandTable = new Vector2[c_RandTableSize,c_RandTableSize,c_RandTableSize];
		for ( int x = 0; x < c_RandTableSize; ++x )
		{
			for ( int y = 0; y < c_RandTableSize; ++y )
			{
				for ( int z = 0; z < c_RandTableSize; ++z )
				{
					s_RandTable[x,y,z] = new Vector2((float)s_Rand.NextDouble(), (float)s_Rand.NextDouble());
				}
			}
		}

	}
	
	public Vector3 Position
	{
		get { return new Vector3(m_Position_x, m_Position_y, m_Position_z); }
		set { m_Position_x = Mathf.FloorToInt(value.x); m_Position_y = value.y; m_Position_z = Mathf.FloorToInt(value.z); }
	}
	public float Density
	{
		get { return m_Density; }
		set { m_Density = value; }
	}
	public Vector2 RandAttr
	{
		get { return s_RandTable[m_Position_x & c_RandTableMask, m_Position_z & c_RandTableMask, 0]; }
	}
	public Vector2 RandAttrs(int i)
	{
		return s_RandTable[m_Position_x & c_RandTableMask, m_Position_z & c_RandTableMask, i & c_RandTableMask];
	}
	public Vector3 RandPos(int i)
	{
		Vector2 rand = s_RandTable[m_Position_x & c_RandTableMask, m_Position_z & c_RandTableMask, i & c_RandTableMask];
		float y = rand.x * m_Normal_x + rand.y * m_Normal_z;
		return new Vector3(m_Position_x + rand.x, m_Position_y - y*1.1f, m_Position_z + rand.y);
	}
	public bool IsParticle
	{
		get { return (s_RandTable[m_Position_x & c_RandTableMask, ((m_Position_x + m_Position_z) >> 5) & c_RandTableMask, m_Position_z & c_RandTableMask].x < 0.01f); }
	}
	public Vector3 Normal
	{
		get { return new Vector3(m_Normal_x, Mathf.Sqrt(1 - (m_Normal_x*m_Normal_x+m_Normal_z*m_Normal_z)), m_Normal_z); }
		set { value.Normalize(); m_Normal_x = value.x; m_Normal_z = value.z; }
	}
	public int Prototype
	{
		get { return (int)m_Prototype; }
		set { m_Prototype = (byte)value; }
	}
	public int Layer
	{
		get { return (m_Prototype & 64) >> 6; }
	}
	public Color ColorF
	{
		get { return (Color)(new Color32(m_Color_r, m_Color_g, m_Color_b, 255)); }
		set { Color32 c32 = value; m_Color_r = c32.r; m_Color_g = c32.g; m_Color_b = c32.b; }
	}
	public Color32 ColorDw
	{
		get { return new Color32(m_Color_r, m_Color_g, m_Color_b, 255); }
		set { m_Color_r = value.r; m_Color_g = value.g; m_Color_b = value.b; }
	}
	public void WriteToStream ( BinaryWriter w )
	{
		w.Write(m_Position_x);
		w.Write(m_Position_y);
		w.Write(m_Position_z);
		w.Write(m_Density);
		w.Write(m_Normal_x);
		w.Write(m_Normal_z);
		w.Write(m_Color_r);
		w.Write(m_Color_g);
		w.Write(m_Color_b);
		w.Write(m_Prototype);
	}
	public void ReadFromStream( BinaryReader r )
	{
		m_Position_x = r.ReadInt32();
		m_Position_y = r.ReadSingle();
		m_Position_z = r.ReadInt32();
		m_Density = r.ReadSingle();
		m_Normal_x = r.ReadSingle();
		m_Normal_z = r.ReadSingle();
		m_Color_r = r.ReadByte();
		m_Color_g = r.ReadByte();
		m_Color_b = r.ReadByte();
		m_Prototype = r.ReadByte();
	}
}
