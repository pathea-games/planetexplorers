using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Security.Cryptography;

public static class VCUtils
{
	public static Vector3 ClampInBound(Vector3 vec, Bounds bound)
	{
		if ( vec.x < bound.min.x )
			vec.x = bound.min.x;
		else if ( vec.x > bound.max.x )
			vec.x = bound.max.x;
		if ( vec.y < bound.min.y )
			vec.y = bound.min.y;
		else if ( vec.y > bound.max.y )
			vec.y = bound.max.y;
		if ( vec.z < bound.min.z )
			vec.z = bound.min.z;
		else if ( vec.z > bound.max.z )
			vec.z = bound.max.z;
		return vec;
	}
	public static List<string> ExplodeString(string s, char delimiter)
	{
		List<string> _retval = new List<string> ();
		string temp = "";
		foreach ( char c in s )
		{
			if ( c == delimiter )
			{
				_retval.Add(temp);
				temp = "";
			}
			else
			{
				temp += c;
			}
		}
		_retval.Add(temp);
		return _retval;
	}
	
	public static string Capital(string s, bool everyword = false)
	{
		string temp = "";
		bool done = false;
		foreach ( char c in s )
		{
			if ( everyword && c == ' ' )
			{
				done = false;
			}
			if ( c >= 'A' && c <= 'Z' )
			{
				done = true;
			}
			if ( !done && c >= 'a' && c <= 'z' )
			{
				temp += (char)(c - 32);
				done = true;
			}
			else
			{
				temp += c;
			}
		}
		return temp;
	}
	public static Color HSB2RGB(float h, float s, float b)
	{
		h = h % 360;
	    Color rgb = new Color (0,0,0,1);
	    int i = (int) ((h / 60) % 6);  
	    float f = (h / 60) - i;  
	    float p = b * (1 - s);  
	    float q = b * (1 - f * s);  
	    float t = b * (1 - (1 - f) * s);  
	    switch (i)
		{  
	    case 0:  
	        rgb.r = b;  
	        rgb.g = t;  
	        rgb.b = p;  
	        break;  
	    case 1:  
	        rgb.r = q;  
	        rgb.g = b;  
	        rgb.b = p;  
	        break;  
	    case 2:  
	        rgb.r = p;  
	        rgb.g = b;  
	        rgb.b = t;  
	        break;  
	    case 3:  
	        rgb.r = p;  
	        rgb.g = q;  
	        rgb.b = b;  
	        break;  
	    case 4:  
	        rgb.r = t;  
	        rgb.g = p;  
	        rgb.b = b;  
	        break;  
	    case 5:  
	        rgb.r = b;  
	        rgb.g = p;  
	        rgb.b = q;  
	        break;  
	    default:  
	        break;
	    }  
	    return rgb;  
	}
	public static Vector3 RGB2HSB(Color rgb, float nanH = 0, float nanS = 0)
	{
	    rgb.r = Mathf.Clamp01(rgb.r);
	    rgb.g = Mathf.Clamp01(rgb.g);
	    rgb.b = Mathf.Clamp01(rgb.b);
		
	    float max = Mathf.Max(rgb.r, rgb.g, rgb.b);
	    float min = Mathf.Min(rgb.r, rgb.g, rgb.b);

	    float hsbB = max;  
	    float hsbS = max < 0.01f ? nanS : (max - min) / max;  
	    float hsbH = 0;
		
		if ( Mathf.Abs(max - min) < 0.005f )
		{
			hsbH = nanH;
		}
		else
		{
		    if (max == rgb.r && rgb.g >= rgb.b)
			{
		        hsbH = (rgb.g - rgb.b) * 60f / (max - min) + 0;  
		    }
			else if (max == rgb.r && rgb.g < rgb.b)
			{
		        hsbH = (rgb.g - rgb.b) * 60f / (max - min) + 360;  
		    }
			else if (max == rgb.g)
			{
		        hsbH = (rgb.b - rgb.r) * 60f / (max - min) + 120;  
		    }
			else if (max == rgb.b)
			{
		        hsbH = (rgb.r - rgb.g) * 60f / (max - min) + 240;  
		    }
		}
	    return new Vector3(hsbH, hsbS, hsbB);
	}
	
	public static Texture2D LoadTextureFromFile(string filename)
	{
		if ( !File.Exists(filename) )
			return null;

		byte[] texbuf = null;
		try
		{
			FileStream fs = new FileStream (filename, FileMode.Open);
			if ( fs.Length > 4194304 || fs.Length < 8 )
			{
				fs.Close();
				return null;
			}
			texbuf = new byte [(int)fs.Length];
			fs.Read(texbuf, 0, (int)fs.Length);
			fs.Close();
		}
		catch (Exception)
		{
			return null;
		}
		Texture2D tex = new Texture2D (2,2);
		if ( !tex.LoadImage(texbuf) )
		{
			Texture2D.Destroy(tex);
			return null;
		}
		return tex;
	}
	
	// Make a string to a valid filename
	public static string MakeFileName( string name )
	{
		string retval = "";
		for ( int i = 0; i < name.Length; i++ )
		{
			if ( name[i] == '/' || name[i] == '\\' || name[i] == ':' || name[i] == '*' || name[i] == '\r' || name[i] == '\n' ||
				 name[i] == '?' || name[i] == '\"' || name[i] == '<' || name[i] == '>' || name[i] == '|' || name[i] == '\b' )
			{
				retval = retval + " ";
			}
			else
			{
				retval = retval + name[i];
			}
		}
		return retval;
	}
	
	// Make a string to a single line
	public static bool MakeSingleLine( ref string s )
	{
		string retval = "";
		bool isSingleLine = true;
		for ( int i = 0; i < s.Length; i++ )
		{
			if ( s[i] == '\r' || s[i] == '\n' )
			{
				isSingleLine = false;
			}
			else
			{
				retval = retval + s[i];
			}
		}
		s = retval;
		return isSingleLine;
	}
	
	public static bool IsInteger(float x)
	{
		return Mathf.Abs(Mathf.Round(x) - x) < 0.00001f;
	}
	
	public static string LengthToString(float l)
	{
		if ( IsInteger(l) )
			return l.ToString("0") + " m";
		else if ( IsInteger(l*100) )
			return (l*100).ToString("0") + " cm";
		else if ( IsInteger(l*1000) )
			return (l*1000).ToString("0") + " mm";
		else if ( IsInteger(l*3) )
			return (l*3).ToString("0") + "/3 m";
		else if ( IsInteger(l*30) )
			return (l*30).ToString("0") + "/30 m";
		else
			return (l*100).ToString("0.00") + " cm";
	}
	
	public static string VolumeToString(float v)
	{
		if ( v == 0 )
			return "0";
		else if ( v < 0.001 )
			return (v*1000000).ToString("#,##0.0") + " cm^3";
		else if ( v < 1 )
			return (v*1000).ToString("#,##0.00") + " L";
		else if ( v < 100 )
			return v.ToString("0.00") + " m^3";
		else if ( v < 10000 )
			return v.ToString("#,##0.0") + " m^3";
		else
			return v.ToString("#,##0") + " m^3";
	}
	
	public static string WeightToString(float w)
	{
		if ( w == 0 )
			return "0";
		else if ( w < 0.001 )
			return (w*1000000).ToString("#,##0.0") + " mg";
		else if ( w < 1 )
			return (w*1000).ToString("#,##0.0") + " g";
		else if ( w < 1000 )
			return w.ToString("0.00") + " kg";
		else if ( w < 100000 )
			return (w*0.001).ToString("#,##0.00") + " T";
		else
			return (w*0.001).ToString("#,##0.0") + " T";
	}
	
	public static bool VectorApproximate(Vector3 a, Vector3 b, string format)
	{
		string sa = a.x.ToString(format) + " " + a.y.ToString(format) + " " + a.z.ToString(format);
		string sb = b.x.ToString(format) + " " + b.y.ToString(format) + " " + b.z.ToString(format);
		return (sa == sb);
	}
	
	public static T GetComponentOrOnParent <T> (GameObject go) where T : Component
	{
		Transform trans = go.transform;
		T component = null;
		while ( trans != null )
		{
			T _c = trans.GetComponent<T>();
			if ( _c != null )
				component = _c;
			trans = trans.parent;
		}
		return component;
	}
	
    public static Transform GetChildByName (Transform parent, string child_name)
    {
        if ( child_name == "" )
            return null;
        foreach ( Transform it in parent )
        {
            if ( it.name.Equals(child_name) )
			{
                return it;
			}
            else
            {
                Transform child = GetChildByName(it, child_name);
                if (child != null)
                    return child;
            }
        }
        return null;
    }

	public static Vector3 RandPosInBoundingBox(Bounds bbox)
	{
		return new Vector3(bbox.min.x + UnityEngine.Random.value * bbox.size.x, 
		                   bbox.min.y + UnityEngine.Random.value * bbox.size.y, 
		                   bbox.min.z + UnityEngine.Random.value * bbox.size.z);
	}

	public static void ISOCut ( VCIsoData iso, VCEAction action )
	{
		List<VCEAlterVoxel> tmpModifies = new List<VCEAlterVoxel> ();
		foreach ( KeyValuePair<int, VCVoxel> kvp in iso.m_Voxels )
		{
			int pos0 = kvp.Key + 1;
			int pos1 = kvp.Key - 1;
			int pos2 = kvp.Key + (1 << 10);
			int pos3 = kvp.Key - (1 << 10);
			int pos4 = kvp.Key + (1 << 20);
			int pos5 = kvp.Key - (1 << 20);
			if ( kvp.Value.Volume < VCEMath.MC_ISO_VALUE ) 
			{
				if ( iso.GetVoxel(pos0).Volume < VCEMath.MC_ISO_VALUE 
				  && iso.GetVoxel(pos1).Volume < VCEMath.MC_ISO_VALUE 
				  && iso.GetVoxel(pos2).Volume < VCEMath.MC_ISO_VALUE 
				  && iso.GetVoxel(pos3).Volume < VCEMath.MC_ISO_VALUE 
				  && iso.GetVoxel(pos4).Volume < VCEMath.MC_ISO_VALUE 
				  && iso.GetVoxel(pos5).Volume < VCEMath.MC_ISO_VALUE )
				{
					VCEAlterVoxel modify = new VCEAlterVoxel (kvp.Key, kvp.Value, new VCVoxel(0,0));
					tmpModifies.Add(modify);
					action.Modifies.Add(modify);
				}
			}
		}
		foreach ( VCEAlterVoxel t in tmpModifies )
			t.Redo();
	}

	public static int CompressEulerAngle ( Vector3 eulerAngle )
	{
		eulerAngle.x = eulerAngle.x % 360.0f;
		eulerAngle.y = eulerAngle.y % 360.0f;
		eulerAngle.z = eulerAngle.z % 360.0f;
		if ( eulerAngle.x < 0.0f )
			eulerAngle.x += 360.0f;
		if ( eulerAngle.y < 0.0f )
			eulerAngle.y += 360.0f;
		if ( eulerAngle.z < 0.0f )
			eulerAngle.z += 360.0f;
		int x = Mathf.RoundToInt((eulerAngle.x/360.0f)*1024.00f);
		int y = Mathf.RoundToInt((eulerAngle.y/360.0f)*2048.00f);
		int z = Mathf.RoundToInt((eulerAngle.z/360.0f)*1024.00f);
		return (x & 1023) | ((z & 1023) << 10) | ((y & 2047) << 20);
	}

	public static Vector3 UncompressEulerAngle ( int data )
	{
		int x = data & 1023;
		int z = (data >> 10) & 1023;
		int y = (data >> 20) & 2047;

		return new Vector3( (float)(x*0.3515625), (float)(y*0.17578125), (float)(z*0.3515625) );
	}

	public static short CompressSmallFloat ( float f )
	{
		return (short)(Mathf.RoundToInt(Mathf.Clamp(f*400f, -32768, 32767)));
	}
	public static float UncompressSmallFloat ( short s )
	{
		return (float)(s*0.0025);
	}
	public static bool IsSeat (EVCComponent type)
	{
		return 
				type == EVCComponent.cpVehicleCockpit ||
				type == EVCComponent.cpVtolCockpit ||
				type == EVCComponent.cpShipCockpit ||
				type == EVCComponent.cpSideSeat;
	}
}
