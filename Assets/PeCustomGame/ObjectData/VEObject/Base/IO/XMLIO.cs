using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Xml;

public static class XMLIO
{
	// System types
	static Type BoolType = typeof(bool);

	static Type LongType = typeof(long);
	static Type IntType = typeof(int);
	static Type ShortType = typeof(short);
	static Type SByteType = typeof(sbyte);

	static Type ULongType = typeof(ulong);
	static Type UIntType = typeof(uint);
	static Type UShortType = typeof(ushort);
	static Type ByteType = typeof(byte);

	static Type SingleType = typeof(float);
	static Type DoubleType = typeof(double);

	static Type StringType = typeof(string);

	// Other types
	static Type Vector2Type = typeof(Vector2);
	static Type Vector3Type = typeof(Vector3);
	static Type Vector4Type = typeof(Vector4);
	static Type QuaternionType = typeof(Quaternion);
	static Type Color32Type = typeof(Color32);
	static Type ColorType = typeof(Color);

	public static string WriteValue (string attr, object value, Type value_type, bool necessary, object default_value)
	{
		if (value == null)
		{
			Debug.LogError("XMLIO::WriteValue (value is null)");
			return "";
		}

		if (!necessary && value.Equals(default_value))
			return "";

		// 按照使用频率进行判断
		if (value_type == IntType || value_type == SingleType || value_type == BoolType)
		{
			return string.Format("{0}=\"{1}\" ", attr, value);
		}
		if (value_type == StringType)
		{
			return string.Format("{0}=\"{1}\" ", attr, System.Uri.EscapeDataString((string)value));
		}
		if (value_type == Vector3Type)
		{
			Vector3 vec = (Vector3)value;
			if (!necessary && default_value != null && default_value is float)
			{
				float d = (float)default_value;
				if (vec.x == d && vec.y == d && vec.z == d)
					return "";
			}
			return string.Format("{0}x=\"{1}\" {0}y=\"{2}\" {0}z=\"{3}\" ", attr, vec.x, vec.y, vec.z);
		}
		if (value_type == QuaternionType)
		{
			Quaternion q = (Quaternion)value;
			if (q == Quaternion.identity)
				return "";
			return string.Format("{0}x=\"{1}\" {0}y=\"{2}\" {0}z=\"{3}\" {0}w=\"{4}\" ", attr, q.x, q.y, q.z, q.w);
		}
		if (value_type.IsEnum)
		{
			return string.Format("{0}=\"{1}\" ", attr, value.ToString());
		}
		if (value_type == Color32Type)
		{
			Color32 c32 = (Color32)value;
			return string.Format("{0}=\"{1}{2}{3}{4}\" ", attr, 
			                     c32.a.ToString("X").PadLeft(2,'0'), 
			                     c32.r.ToString("X").PadLeft(2,'0'),
			                     c32.g.ToString("X").PadLeft(2,'0'),
			                     c32.b.ToString("X").PadLeft(2,'0'));
		}
		if (value_type == ColorType)
		{
			Color32 c32 = (Color32)((Color)value);
			return string.Format("{0}=\"{1}{2}{3}{4}\" ", attr, 
			                     c32.a.ToString("X").PadLeft(2,'0'), 
			                     c32.r.ToString("X").PadLeft(2,'0'),
			                     c32.g.ToString("X").PadLeft(2,'0'),
			                     c32.b.ToString("X").PadLeft(2,'0'));
		}
		if (value_type == Vector2Type)
		{
			Vector2 vec = (Vector2)value;
			if (!necessary && default_value != null && default_value is float)
			{
				float d = (float)default_value;
				if (vec.x == d && vec.y == d)
					return "";
			}
			return string.Format("{0}x=\"{1}\" {0}y=\"{2}\" ", attr, vec.x, vec.y);
		}
		if (value_type == Vector4Type)
		{
			Vector4 vec = (Vector4)value;
			if (!necessary && default_value != null && default_value is float)
			{
				float d = (float)default_value;
				if (vec.x == d && vec.y == d && vec.z == d && vec.w == d)
					return "";
			}
			return string.Format("{0}x=\"{1}\" {0}y=\"{2}\" {0}z=\"{3}\" {0}w=\"{4}\" ", attr, vec.x, vec.y, vec.z, vec.w);
		}
		if (value_type == LongType || value_type == ULongType || 
		    value_type == DoubleType || value_type == UIntType ||
		    value_type == ShortType || value_type == UShortType ||
		    value_type == ByteType || value_type == SByteType)
		{
			return string.Format("{0}=\"{1}\" ", attr, value);
		}

		// 未找到相应Type的转换
		Debug.LogWarning("Type '" + value_type.Name + "' cannot convert to xml");
		return "";
	}

	public static object ReadValue (XmlElement xml, string attr, Type value_type, bool necessary, object default_value)
	{
		// 按照使用频率进行判断

		// int
		if (value_type == IntType)
		{
			try
			{
				if (xml.HasAttribute(attr))
					return XmlConvert.ToInt32(xml.Attributes[attr].Value);
			}
			catch { Debug.LogWarning("XMLIO: attribute '" + attr + "' reading error"); }
			if (necessary)
				throw new Exception("XMLIO: attribute '" + attr + "' is necessary");
			if (default_value == null)
				return (int)0;
			if (!(default_value is int))
				return (int)0;
			return (int)default_value;
		}

		// float
		if (value_type == SingleType)
		{
			try
			{
				if (xml.HasAttribute(attr))
					return XmlConvert.ToSingle(xml.Attributes[attr].Value);
			}
			catch { Debug.LogWarning("XMLIO: attribute '" + attr + "' reading error"); }
			if (necessary)
				throw new Exception("XMLIO: attribute '" + attr + "' is necessary");
			if (default_value != null)
			{
				if (default_value is int)
					return (float)((int)default_value);
				if (default_value is float)
					return (float)default_value;
				if (default_value is double)
					return (float)((double)default_value);
			}
			return (float)0f;
		}
		
		// bool
		if (value_type == BoolType)
		{
			try
			{
				if (xml.HasAttribute(attr))
					return xml.Attributes[attr].Value.ToLower() == "true";
			}
			catch { Debug.LogWarning("XMLIO: attribute '" + attr + "' reading error"); }
			if (necessary)
				throw new Exception("XMLIO: attribute '" + attr + "' is necessary");
			if (default_value == null)
				return null;
			if (!(default_value is bool))
				return false;
			return (bool)default_value;
		}

		// string
		if (value_type == StringType)
		{
			try
			{
				if (xml.HasAttribute(attr))
					return Uri.UnescapeDataString(xml.Attributes[attr].Value);
			}
			catch { Debug.LogWarning("XMLIO: attribute '" + attr + "' reading error"); }
			if (necessary)
				throw new Exception("XMLIO: attribute '" + attr + "' is necessary");
			if (default_value == null)
				return "";
			if (!(default_value is string))
				return "";
			return (string)default_value;
		}

		// Vector3
		if (value_type == Vector3Type)
		{
			float x = 0;
			float y = 0;
			float z = 0;
			bool has_x = false;
			bool has_y = false;
			bool has_z = false;
			try
			{
				if (xml.HasAttribute(attr + "x"))
				{
					x = XmlConvert.ToSingle(xml.Attributes[attr + "x"].Value);
					has_x = true;
				}
				else if (necessary)
					throw new Exception("XMLIO: attribute '" + attr + "' is necessary");
			}
			catch { Debug.LogWarning("XMLIO: attribute '" + attr + "x' reading error"); }
			try
			{
				if (xml.HasAttribute(attr + "y"))
				{
					y = XmlConvert.ToSingle(xml.Attributes[attr + "y"].Value);
					has_y = true;
				}
				else if (necessary)
					throw new Exception("XMLIO: attribute '" + attr + "' is necessary");
			}
			catch { Debug.LogWarning("XMLIO: attribute '" + attr + "y' reading error"); }
			try
			{
				if (xml.HasAttribute(attr + "z"))
				{
					z = XmlConvert.ToSingle(xml.Attributes[attr + "z"].Value);
					has_z = true;
				}
				else if (necessary)
					throw new Exception("XMLIO: attribute '" + attr + "' is necessary");
			}
			catch { Debug.LogWarning("XMLIO: attribute '" + attr + "z' reading error"); }

			if (has_x && has_y && has_z)
				return new Vector3(x,y,z);

			float dv = 0f;
			if (default_value != null)
			{
				if (default_value is int)
					dv = (float)((int)default_value);
				if (default_value is float)
					dv = ((float)default_value);
				if (default_value is double)
					dv = (float)((double)default_value);
			}
			if (!has_x)
				x = dv;
			if (!has_y)
				y = dv;
			if (!has_z)
				z = dv;
			return new Vector3(x,y,z);
		}

		// Quaternion
		if (value_type == QuaternionType)
		{
			float x = 0;
			float y = 0;
			float z = 0;
			float w = 0;
			try
			{
				if (xml.HasAttribute(attr + "x"))
					x = XmlConvert.ToSingle(xml.Attributes[attr + "x"].Value);
				else return Quaternion.identity;
			}
			catch { Debug.LogWarning("XMLIO: attribute '" + attr + "x' reading error"); return Quaternion.identity; }
			try
			{
				if (xml.HasAttribute(attr + "y"))
					y = XmlConvert.ToSingle(xml.Attributes[attr + "y"].Value);
				else return Quaternion.identity;
			}
			catch { Debug.LogWarning("XMLIO: attribute '" + attr + "y' reading error"); return Quaternion.identity; }
			try
			{
				if (xml.HasAttribute(attr + "z"))
					z = XmlConvert.ToSingle(xml.Attributes[attr + "z"].Value);
				else return Quaternion.identity;
			}
			catch { Debug.LogWarning("XMLIO: attribute '" + attr + "z' reading error"); return Quaternion.identity; }
			try
			{
				if (xml.HasAttribute(attr + "w"))
					w = XmlConvert.ToSingle(xml.Attributes[attr + "w"].Value);
				else return Quaternion.identity;
			}
			catch { Debug.LogWarning("XMLIO: attribute '" + attr + "w' reading error"); return Quaternion.identity; }
			Vector4 qv = new Vector4 (x,y,z,w);
			if (qv.magnitude < 0.001f)
				return Quaternion.identity;
			qv.Normalize();
			Quaternion q = new Quaternion (qv.x,qv.y,qv.z,qv.w);
			return q;
		}

		// Enum
		if (value_type.IsEnum)
		{
			try
			{
				if (xml.HasAttribute(attr))
				{
					return System.Enum.Parse(value_type, xml.Attributes[attr].Value);
				}
			}
			catch { Debug.LogWarning("XMLIO: attribute '" + attr + "' reading error"); }
			if (necessary)
				throw new Exception("XMLIO: attribute '" + attr + "' is necessary");
			if (default_value == null)
			{
				if (default_value is string)
					return System.Enum.Parse(value_type, (string)default_value);
				if (default_value.GetType() == value_type)
					return default_value;
			}
			return System.Enum.ToObject(value_type, 0);
		}

		// Color32
		if (value_type == Color32Type)
		{
			string color_str = "FFFFFFFF";
			try
			{
				if (xml.HasAttribute(attr))
				{
					color_str = xml.Attributes[attr].Value;
					return StringToColor32(color_str);
				}
			}
			catch { Debug.LogWarning("XMLIO: attribute '" + attr + "' reading error"); }
			if (necessary)
				throw new Exception("XMLIO: attribute '" + attr + "' is necessary");
			if (default_value != null)
			{
				if (default_value is string)
					color_str = (string)default_value;
			}
			try { return StringToColor32(color_str); }
			catch { return new Color32(255,255,255,255); }
		}

		// Color
		if (value_type == ColorType)
		{
			string color_str = "FFFFFFFF";
			try
			{
				if (xml.HasAttribute(attr))
				{
					color_str = xml.Attributes[attr].Value;
					return (Color)(StringToColor32(color_str));
				}
			}
			catch { Debug.LogWarning("XMLIO: attribute '" + attr + "' reading error"); }
			if (necessary)
				throw new Exception("XMLIO: attribute '" + attr + "' is necessary");
			if (default_value != null)
			{
				if (default_value is string)
					color_str = (string)default_value;
			}
			try { return (Color)(StringToColor32(color_str)); }
			catch { return Color.white; }
		}

		// Vector2
		if (value_type == Vector2Type)
		{
			float x = 0;
			float y = 0;
			bool has_x = false;
			bool has_y = false;
			try
			{
				if (xml.HasAttribute(attr + "x"))
				{
					x = XmlConvert.ToSingle(xml.Attributes[attr + "x"].Value);
					has_x = true;
				}
				else if (necessary)
					throw new Exception("XMLIO: attribute '" + attr + "' is necessary");
			}
			catch { Debug.LogWarning("XMLIO: attribute '" + attr + "x' reading error"); }
			try
			{
				if (xml.HasAttribute(attr + "y"))
				{
					y = XmlConvert.ToSingle(xml.Attributes[attr + "y"].Value);
					has_y = true;
				}
				else if (necessary)
					throw new Exception("XMLIO: attribute '" + attr + "' is necessary");
			}
			catch { Debug.LogWarning("XMLIO: attribute '" + attr + "y' reading error"); }

			if (has_x && has_y)
				return new Vector2(x,y);
			
			float dv = 0f;
			if (default_value != null)
			{
				if (default_value is int)
					dv = (float)((int)default_value);
				if (default_value is float)
					dv = ((float)default_value);
				if (default_value is double)
					dv = (float)((double)default_value);
			}
			if (!has_x)
				x = dv;
			if (!has_y)
				y = dv;
			return new Vector2(x,y);
		}

		// Vector4
		if (value_type == Vector4Type)
		{
			float x = 0;
			float y = 0;
			float z = 0;
			float w = 0;
			bool has_x = false;
			bool has_y = false;
			bool has_z = false;
			bool has_w = false;
			try
			{
				if (xml.HasAttribute(attr + "x"))
				{
					x = XmlConvert.ToSingle(xml.Attributes[attr + "x"].Value);
					has_x = true;
				}
				else if (necessary)
					throw new Exception("XMLIO: attribute '" + attr + "' is necessary");
			}
			catch { Debug.LogWarning("XMLIO: attribute '" + attr + "x' reading error"); }
			try
			{
				if (xml.HasAttribute(attr + "y"))
				{
					y = XmlConvert.ToSingle(xml.Attributes[attr + "y"].Value);
					has_y = true;
				}
				else if (necessary)
					throw new Exception("XMLIO: attribute '" + attr + "' is necessary");
			}
			catch { Debug.LogWarning("XMLIO: attribute '" + attr + "y' reading error"); }
			try
			{
				if (xml.HasAttribute(attr + "z"))
				{
					z = XmlConvert.ToSingle(xml.Attributes[attr + "z"].Value);
					has_z = true;
				}
				else if (necessary)
					throw new Exception("XMLIO: attribute '" + attr + "' is necessary");
			}
			catch { Debug.LogWarning("XMLIO: attribute '" + attr + "z' reading error"); }
			try
			{
				if (xml.HasAttribute(attr + "w"))
				{
					w = XmlConvert.ToSingle(xml.Attributes[attr + "w"].Value);
					has_w = true;
				}
				else if (necessary)
					throw new Exception("XMLIO: attribute '" + attr + "' is necessary");
			}
			catch { Debug.LogWarning("XMLIO: attribute '" + attr + "w' reading error"); }

			if (has_x && has_y && has_z && has_w)
				return new Vector4(x,y,z,w);
			
			float dv = 0f;
			if (default_value != null)
			{
				if (default_value is int)
					dv = (float)((int)default_value);
				if (default_value is float)
					dv = ((float)default_value);
				if (default_value is double)
					dv = (float)((double)default_value);
			}
			if (!has_x)
				x = dv;
			if (!has_y)
				y = dv;
			if (!has_z)
				z = dv;
			if (!has_w)
				w = dv;
			return new Vector4(x,y,z,w);
		}

		// double
		if (value_type == DoubleType)
		{
			try
			{
				if (xml.HasAttribute(attr))
					return XmlConvert.ToDouble(xml.Attributes[attr].Value);
			}
			catch { Debug.LogWarning("XMLIO: attribute '" + attr + "' reading error"); }
			if (necessary)
				throw new Exception("XMLIO: attribute '" + attr + "' is necessary");
			if (default_value != null)
			{
				if (default_value is int)
					return (double)((int)default_value);
				if (default_value is float)
					return (double)default_value;
				if (default_value is double)
					return (double)((double)default_value);
			}
			return (double)0.0;
		}

		// Other numbers
		if (value_type == LongType || value_type == ULongType || value_type == UIntType ||
		    value_type == ShortType || value_type == UShortType ||
		    value_type == ByteType || value_type == SByteType)
		{
			ulong ul = 0;
			bool has = false;
			try
			{
				if (xml.HasAttribute(attr))
				{
					ul = XmlConvert.ToUInt64(xml.Attributes[attr].Value);
					has = true;
				}
			}
			catch { Debug.LogWarning("XMLIO: attribute '" + attr + "' reading error"); }

			if (!has)
			{
				if (necessary)
					throw new Exception("XMLIO: attribute '" + attr + "' is necessary");
				if (default_value != null)
				{
					if (default_value is ulong)
						ul = (ulong)(default_value);
					if (default_value is long)
						ul = (ulong)((long)default_value);
					if (default_value is int)
						ul = (ulong)((int)default_value);
					if (default_value is uint)
						ul = (ulong)((uint)default_value);
				}
			}

			if (value_type == LongType)
				return (long)ul;
			else if (value_type == ULongType)
				return ul;
			else if (value_type == UIntType)
				return (uint)ul;
			else if (value_type == ShortType)
				return (short)ul;
			else if (value_type == UShortType)
				return (ushort)ul;
			else if (value_type == ByteType)
				return (byte)ul;
			else //(value_type == SByteType)
				return (sbyte)ul;
		}

		if (necessary)
			throw new Exception("XMLIO: attribute '" + attr + "' is necessary and not readable");

		Debug.Log("XMLIO: attribute '" + attr + "' is not readable");
		return null;
	}

	static Color32 StringToColor32 (string str)
	{
		long l = Convert.ToInt64(str, 16);
		Color32 c32;
		c32.b = (byte)(l & 0xff);
		l = (long)(l >> 8);
		c32.g = (byte)(l & 0xff);
		l = (long)(l >> 8);
		c32.r = (byte)(l & 0xff);
		l = (long)(l >> 8);
		c32.a = (byte)(l & 0xff);
		return c32;
	}
}
