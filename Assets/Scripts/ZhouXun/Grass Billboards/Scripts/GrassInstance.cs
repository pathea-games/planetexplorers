using UnityEngine;
using System;
using System.Collections;
using System.IO;

/// <summary>
/// Grass instance class in [Billboard-set Grass System].
/// </summary>
public class GrassInstance 
{
	public const int BytesLengths = 24;
	private Vector3 m_Position;
	private float m_Normal_x;
	private float m_Normal_z;
	private byte m_Color_r;
	private byte m_Color_g;
	private byte m_Color_b;
	private byte m_Prototype;
	
	public Vector3 Position
	{
		get { return m_Position; }
		set { m_Position = value; }
	}
	public Vector3 Normal
	{
		get { return new Vector3(m_Normal_x, 1-Mathf.Sqrt(m_Normal_x*m_Normal_x+m_Normal_z*m_Normal_z), m_Normal_z); }
		set { value.Normalize(); m_Normal_x = value.x; m_Normal_z = value.z; }
	}
	public int Prototype
	{
		get { return (int)m_Prototype; }
		set { m_Prototype = (byte)value; }
	}
	public int Layer
	{
		get { return (m_Prototype & 32) >> 5; }
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
		w.Write(m_Position.x);
		w.Write(m_Position.y);
		w.Write(m_Position.z);
		w.Write(m_Normal_x);
		w.Write(m_Normal_z);
		w.Write(m_Color_r);
		w.Write(m_Color_g);
		w.Write(m_Color_b);
		w.Write(m_Prototype);
	}
	public void ReadFromStream( BinaryReader r )
	{
		m_Position.x = r.ReadSingle();
		m_Position.y = r.ReadSingle();
		m_Position.z = r.ReadSingle();
		m_Normal_x = r.ReadSingle();
		m_Normal_z = r.ReadSingle();
		m_Color_r = r.ReadByte();
		m_Color_g = r.ReadByte();
		m_Color_b = r.ReadByte();
		m_Prototype = r.ReadByte();
	}
}
