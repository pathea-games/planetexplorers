using UnityEngine;
using System.Collections;

// Almost each type of elem corresponding to a kind of functor
public class DHElem {
	public const int AxisSize = 1;
	public const int SquareSize = AxisSize*AxisSize;
	public const int CubicSize = SquareSize*AxisSize;
	public enum FunctorType{
		FUNCTOR_,
	}
	
	public VFVoxel[] vData = new VFVoxel[CubicSize];
	public DHElem(byte type)
	{
		int len = vData.Length;
		for(int i = 0; i < len; i ++)
		{
			vData[i] = new VFVoxel(255,type);
		}
	}
	public DHElem(byte vol,byte type)
	{
		int len = vData.Length;
		for(int i = 0; i < len; i ++)
		{
			vData[i] = new VFVoxel(vol,type);
		}
	}
	public byte Volume{
		get{return vData[0].Volume;}
		set{
			int len = vData.Length;
			for(int i = 0; i < len; i ++)
			{
				vData[i].Volume = value;
			}
		}
	}
	public byte Type{
		get{return vData[0].Type;}
		set{
			int len = vData.Length;
			for(int i = 0; i < len; i ++)
			{
				vData[i].Type = value;
			}
		}
	}
}
