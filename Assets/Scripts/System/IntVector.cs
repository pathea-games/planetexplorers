using UnityEngine;
using System.Collections;
using System;

public struct Ints2{
	public int x,y;	
	public Ints2(int x_, int y_){
		x = x_;
		y = y_;
	}
}
public struct Ints3{
	public int x,y,z;	
	public Ints3(int x_, int y_, int z_){
		x = x_;
		y = y_;
		z = z_;
	}
}
public struct Ints4{
	public int x,y,z,w;	
	public Ints4(int x_, int y_, int z_, int w_){
		x = x_;
		y = y_;
		z = z_;
		w = w_;
	}
}

[Serializable]
public class IntVector2
{
	public int x;
	public int y;
	
	public IntVector2(){	x = y = 0;	}
	public IntVector2(int x_, int y_)
	{
		x = x_;
		y = y_;
	}

    public static IntVector2 Zero { get { return new IntVector2(0, 0); } }

    public static IntVector2 One { get { return new IntVector2(1, 1); } }

	public static IntVector2 Tmp = new IntVector2 ();

	public static IntVector2 operator*(IntVector2 mul0,IntVector2 mul1)
	{
		return new IntVector2(mul0.x*mul1.x, mul0.y*mul1.y);
	}
	public static IntVector2 operator*(IntVector2 mul0,int mul1)
	{
		return new IntVector2(mul0.x*mul1, mul0.y*mul1);
	}
	public static IntVector2 operator-(IntVector2 vec0, IntVector2 vec1)
	{
		return new IntVector2(vec0.x-vec1.x, vec0.y-vec1.y);
	}
	public static IntVector2 operator+(IntVector2 vec0, IntVector2 vec1)
	{
		return new IntVector2(vec0.x+vec1.x, vec0.y+vec1.y);
	}
	
	public static implicit operator IntVector2(Vector3 vec3)
	{
		return new IntVector2((int)vec3.x, (int)vec3.y);
	}
	public static implicit operator Vector2(IntVector2 vec)
	{
		return new Vector2((float) vec.x, (float) vec.y); 
	}
	public static implicit operator Vector3(IntVector2 vec)
	{
		return new Vector3((float) vec.x, (float) vec.y); 
	}
	public static int SqrMagnitude (IntVector2 vec)
	{
		return vec.x * vec.x + vec.y * vec.y;
	}
	public float Distance(IntVector2 vec)
	{
		return Mathf.Sqrt( Mathf.Pow(vec.x - x, 2) + 
			Mathf.Pow(vec.y - y, 2) );
	}
	public override bool Equals (object obj)
	{
		if(null == obj)
			return false;
		IntVector2 vec = (IntVector2) obj;
		//if(vec != null)
		{
			return x==vec.x && y==vec.y;
		}
		//return false;
	}
	public override int GetHashCode ()	// In Dictionary, x,y must be unmodifiable to keep hash code constant
	{
		return x+(y<<16);
	}
	public override string ToString ()
	{
		return string.Format ("[{0},{1}]", x,y);
	}
    public static IntVector2 Parse(string str)
    {
        string[] strs = str.Split(',');
        int x = Int32.Parse(strs[0].Substring(1,strs[0].Length-1));
        int y = Int32.Parse(strs[1].Substring(0,strs[1].Length-1));
        return new IntVector2(x,y);
    }

    public static void SerializeItem(uLink.BitStream stream, object obj, params object[] codecOptions)
    {
        IntVector2 v = (IntVector2)(obj);
        stream.Write<int>(v.x);
        stream.Write<int>(v.y);
    }
    public static object DeserializeItem(uLink.BitStream stream, params object[] codecOptions)
    {
        int x_ = stream.Read<int>();
        int y_ = stream.Read<int>();
        IntVector2 v = new IntVector2(x_, y_);
        return v;
    }
}

[Serializable]
public class IntVector3
{
	public int x;
	public int y;
	public int z;
	
	public static IntVector3 Zero { get { return new IntVector3(0,0,0); } }
	public static IntVector3 One { get { return new IntVector3(1,1,1); } }
	
    public static void SerializeItem(uLink.BitStream stream, object obj, params object[] codecOptions)
    {
        IntVector3 v = (IntVector3)(obj);
        stream.Write<int>(v.x);
        stream.Write<int>(v.y);
        stream.Write<int>(v.z);
    }
    public static object DeserializeItem(uLink.BitStream stream, params object[] codecOptions)
    {
        int x_ = stream.Read<int>();
        int y_ = stream.Read<int>();
        int z_ = stream.Read<int>();
        IntVector3 v = new IntVector3(x_, y_, z_);
        return v;
    }
	public static IntVector3 UnitX{		get{	return new IntVector3 (1, 0, 0);		}	}
	public static IntVector3 UnitY{		get{	return new IntVector3 (0, 1, 0);		}	}
	public static IntVector3 UnitZ{		get{	return new IntVector3 (0, 0, 1);		}	}

	public IntVector3(IntVector3 vec3)
	{
		x = vec3.x;
		y = vec3.y;
		z = vec3.z;
	}
	public IntVector3(int x_ = 0, int y_ = 0, int z_ = 0)
	{
		x = x_;		y = y_;		z = z_;
	}
	public IntVector3(Vector3 xyz)
	{
        x = Mathf.RoundToInt(xyz.x); y = Mathf.RoundToInt(xyz.y); z = Mathf.RoundToInt(xyz.z);
	}
	public IntVector3(float x_,float y_,float z_)
	{
		x = Mathf.RoundToInt(x_ + 0.001f); y = Mathf.RoundToInt(y_ + 0.001f); z = Mathf.RoundToInt(z_ + 0.001f);
	}
	
	public Vector3 ToVector3()
	{
		return new Vector3((float)x,(float)y,(float)z);
	}
	public static IntVector3 operator*(IntVector3 mul0,IntVector3 mul1)
	{
		return new IntVector3(mul0.x*mul1.x, mul0.y*mul1.y, mul0.z*mul1.z);
	}
	public static IntVector3 operator*(IntVector3 mul0,int mul1)
	{
		return new IntVector3(mul0.x*mul1, mul0.y*mul1, mul0.z*mul1);
	}
	public static IntVector3 operator/(IntVector3 div0,int div1)
	{
		return new IntVector3(div0.x/div1, div0.y/div1, div0.z/div1);
	}
	public static IntVector3 operator-(IntVector3 vec0, IntVector3 vec1)
	{
		return new IntVector3(vec0.x-vec1.x, vec0.y-vec1.y, vec0.z-vec1.z);
	}
	public static IntVector3 operator+(IntVector3 vec0, IntVector3 vec1)
	{
		return new IntVector3(vec0.x+vec1.x, vec0.y+vec1.y, vec0.z+vec1.z);
	}
	
	public static implicit operator IntVector3(Vector3 vec3)
	{
        return new IntVector3(Mathf.RoundToInt(vec3.x), Mathf.RoundToInt(vec3.y), Mathf.RoundToInt(vec3.z));
	}
	public static implicit operator Vector3(IntVector3 vec)
	{
		return new Vector3((float) vec.x, (float) vec.y, (float) vec.z); 
	}
	public static int SqrMagnitude (IntVector3 vec)
	{
		return vec.x * vec.x + vec.y * vec.y + vec.z * vec.z;
	}
	public float Distance(IntVector3 vec)
	{
		return Mathf.Sqrt( Mathf.Pow(vec.x - x, 2) + 
			Mathf.Pow(vec.y - y, 2) + 
			Mathf.Pow(vec.z - z, 2) );
	}
	public override bool Equals (object obj)
	{
		IntVector3 vec = (IntVector3) obj;
		//if(vec != null)
		{
			return x==vec.x && y==vec.y && z==vec.z;
		}
		//return false;
	}
	public override int GetHashCode ()	// In Dictionary, x,y,z must be unmodifiable to keep hash code constant
	{
		return x+(z<<11)+(y<<22);
	}
	public override string ToString ()
	{
		return string.Format ("[{0},{1},{2}]", x,y,z);
	}	
}

[Serializable]
public class IntVector4
{
	public int x;
	public int y;
	public int z;
	public int w;
	
	public IntVector4()
	{	x = 0;		y = 0;		z = 0;		w = 0;	}
	public IntVector4(int x_, int y_, int z_, int w_)
	{	x = x_;		y = y_;		z = z_;		w = w_;	}
	public IntVector4(IntVector3 v3, int w_)
	{	x = v3.x;	y = v3.y;	z = v3.z;	w = w_;	}
	public IntVector4(IntVector4 v4)
	{	x = v4.x;	y = v4.y;	z = v4.z;	w = v4.w;	}
	public IntVector4(Vector3 xyz, int w_)
	{
		x = Mathf.RoundToInt(xyz.x); y = Mathf.RoundToInt(xyz.y); z = Mathf.RoundToInt(xyz.z); w = w_;
	}

	public static IntVector4 Zero{ get { return new IntVector4(0,0,0,0); } }

	public IntVector3 ToIntVector3()
	{
		return new IntVector3(x, y, z);
	}
	public Vector3 ToVector3()
	{
		return new Vector3((float)x,(float)y,(float)z);
	}
	public IntVector3 XYZ{
		get{	return new IntVector3(x,y,z);		}
	}
	public static int SqrMagnitude (IntVector4 vec)
	{
		return vec.x * vec.x + vec.y * vec.y + vec.z * vec.z + vec.w * vec.w;
	}
	public bool ContainInTerrainNode(Vector3 pos)
	{
		int extend = (VoxelTerrainConstants._numVoxelsPerAxis<<w);
		return 	pos.x >= x && pos.x < x+extend &&
				pos.y >= y && pos.y < y+extend &&
				pos.z >= z && pos.z < z+extend ;
	}
	public bool Contains(Vector3 pos)
	{
		return 	pos.x >= x && pos.x < x+w &&
				pos.y >= y && pos.y < y+w &&
				pos.z >= z && pos.z < z+w ;
	}
	public override bool Equals (object obj)
	{
		IntVector4 vec = (IntVector4) obj;
		//if(vec != null)
		{
			return x==vec.x && y==vec.y && z==vec.z && w==vec.w;
		}
		//return false;
	}
	public override int GetHashCode ()	// In Dictionary, x,y,z must be unmodifiable to keep hash code constant
	{
		// In our game, w is used as lod almost, so 4bits can be enough to represent it
		// y+1 to avoid 0
		return (x&0x3ff)+((z&0x3ff)<<10)+(((y+1)&0xff)<<20)+(w<<28);
	}
	public override string ToString ()
	{
		return string.Format ("[{0},{1},{2},{3}]", x,y,z,w);
	}
	public static void SerializeItem(uLink.BitStream stream, object obj, params object[] codecOptions)
	{
		IntVector4 v = (IntVector4)(obj);
		stream.Write<int>(v.x);
		stream.Write<int>(v.y);
		stream.Write<int>(v.z);
		stream.Write<int>(v.w);
	}
	public static object DeserializeItem(uLink.BitStream stream, params object[] codecOptions)
	{
		int x_ = stream.Read<int>();
		int y_ = stream.Read<int>();
		int z_ = stream.Read<int>();
		int w_ = stream.Read<int>();
		IntVector4 v = new IntVector4(x_, y_, z_,w_);
		return v;
	}
}