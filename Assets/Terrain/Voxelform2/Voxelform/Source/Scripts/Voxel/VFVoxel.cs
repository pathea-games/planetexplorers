using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// This structure contains voxel volume information which is compared against isolevel to create surfaces.
/// It's also used to calculate vertex normals. 
/// </summary>
/// <remarks>
/// The reason for using a structure is to allow straight forward compacting floating point values,
/// and to allow easy user data customization without requiring significant code changes.
/// </remarks>
public struct VFVoxel
{
	public byte Volume;
	public byte Type;
	public const int c_shift = 1;
	public const int c_VTSize = 2;	// These 2 parts will be used to compute mesh.

	public enum EType : byte{
		MUD = 3,				//deprecated
		TERRAIN_TYPE_MAX = 64,	//deprecated

		WaterSourceBeg = 128,
		WaterSourceEnd = 249,
		Reserved0 = 250,
		Reserved1 = 251,
		Reserved2 = 252,
		Reserved3 = 253,
		Reserved4 = 254,
		Reserved5 = 255,
	}

	/// <summary>
	/// Creates a Voxel with volume data. 
	/// </summary>
	public VFVoxel(byte volume)
	{
		Volume = volume;
		Type = 0;
	}

	/// <summary>
	/// Creates a Voxel with volume data. 
	/// </summary>
	public VFVoxel(byte volume, byte type)
	{
		Volume = volume;
		Type = type;
	}
	
	public VFVoxel(byte volume, byte type, byte owner)
	{
		Volume = volume;
		Type = type;
	}
	
	public bool IsBuilding{
		get{	return Type >= (byte)EType.TERRAIN_TYPE_MAX;	}
	}
	public const float RECIPROCAL_255 = 1.0f/255.0f;
	public static byte ToNormByte(float vol)
	{
		return (byte)(vol*255);
	}
}
public class CVFVoxel
{
	public VFVoxel _value;
	public CVFVoxel(VFVoxel v)
	{
		_value = v;
	}
	public static implicit operator VFVoxel(CVFVoxel cv)
	{
		return cv._value;
	}
	public static implicit operator CVFVoxel(VFVoxel v)
	{
		return new CVFVoxel(v);
	}
}