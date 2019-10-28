using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System;

public class UVKeyCount
{
	public int svn_key;
	public ushort count;
	public const int length = 6;
	
	public UVKeyCount(int key = 0, ushort _count = 0 )
    {
        svn_key = key;
        count = _count;
    }
}
public class UpdateVector
{
	public byte xyz0, xyz1;
	public byte voxelData0, voxelData1;
	public const int length = 4;
	
	public int Data
	{
		get
		{
			int data = voxelData1;
			data <<= 8;
			data |= voxelData0;
			data <<= 8;
			data |= xyz1;
			data <<= 8;
			data |= xyz0;
			return data;
		}
		
		set
		{
			xyz0 = (byte)(value);
			xyz1 = (byte)(value >> 8);
			voxelData0 = (byte)(value >> 16);
			voxelData1 = (byte)(value >> 24);
		}
	}
};
public class ByteArrayHelper{
	
	public static ushort to_ushort(byte[] arr, int ofs)
	{
		return (ushort)((ushort)arr[ofs] + ((ushort)arr[ofs + 1] << 8));
	}
	public static int to_int(byte[] arr, int ofs)
	{
		int res = (arr[ofs] + 
			(arr[ofs + 1] << 8) +
			(arr[ofs + 2] << 16) +
			(arr[ofs + 3] << 24));
		return res;
	}
	public static IntVector3 to_IntVector3(byte[] arr, int ofs)
	{
		IntVector3 res = new IntVector3(arr[ofs], arr[ofs + 1], arr[ofs + 2]);
		return res;
	}
	
	
	public static void ushort_to(byte[] arr, int ofs, ushort val)
	{
		arr[ofs] = (byte)(val & 0xff);
		arr[ofs + 1] = (byte)((val >> 8) & 0xff);
	}
	public static void int_to(byte[] arr, int ofs, int val)
	{
		arr[ofs] = (byte)(val & 0xff);
		arr[ofs + 1] = (byte)((val >> 8) & 0xff);
		arr[ofs + 2] = (byte)((val >> 16) & 0xff);
		arr[ofs + 3] = (byte)((val >> 24) & 0xff);
	}
	public static void IntVector3_to(byte[] arr, int ofs, IntVector3 val)
	{
		arr[ofs] = (byte)val.x;
		arr[ofs + 1] = (byte)val.y;
		arr[ofs + 2] = (byte)val.z;
	}
}
