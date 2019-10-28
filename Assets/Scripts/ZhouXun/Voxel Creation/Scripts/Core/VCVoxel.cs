using UnityEngine;
using System.Collections;

public struct VCVoxel
{
	public VCVoxel(byte v, byte t)
	{
		Volume = v;
		Type = t;
	}
	public VCVoxel(ushort val)
	{
		Volume = (byte)(val & 0xff);
		Type = (byte)(val >> 8);
	}
	
	public void PrintValue()
	{
		Debug.Log("Vol = " + Volume.ToString() + "  Type = " + Type.ToString());
	}
	
	public byte Volume;
	public byte Type;
	
	public const int c_Size = 2;
	public const float RECIPROCAL_255 = 1.0f/255.0f;
	public float VolumeF
	{
		get { return Volume / 255.0f; }
		set { Volume = (byte)(Mathf.RoundToInt(Mathf.Clamp01(value)*255.0f)); }
	}
	
	public static implicit operator VCVoxel(ushort val)
	{
		return new VCVoxel (val);
	}
	public static implicit operator ushort(VCVoxel voxel)
	{
		return (ushort)(voxel.Volume | voxel.Type << 8);
	}
	public static implicit operator VCVoxel(int val)
	{
		return new VCVoxel ((ushort)(val));
	}
	public static implicit operator int(VCVoxel voxel)
	{
		return (int)(voxel.Volume | voxel.Type << 8);
	}
}
