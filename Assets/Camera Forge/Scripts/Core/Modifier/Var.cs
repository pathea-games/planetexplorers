using UnityEngine;
using System;
using System.IO;
using System.Globalization;

namespace CameraForge
{
	public enum EVarType : int
	{
		Null = 0,
		Bool,
		Int,
		Float,
		Vector,
		Quaternion,
		Color,
		String
	}
	
	public class Var
	{
		public static Var Null
		{
			get
			{
				Var v = new Var ();
				v.type = EVarType.Null;
				return v;
			}
		}
		public bool isNull { get { return type == EVarType.Null; } }

		public EVarType type;
		public bool value_b;
		public int value_i;
		public float value_f;
		public Vector4 value_v;
		public Quaternion value_q;
		public Color value_c
		{
			get
			{
				return new Color(value_v.x, value_v.y, value_v.z, value_v.w);
			}
			set
			{
				value_v = new Vector4(value.r, value.g, value.b, value.a);
			}
		}
		private string _value_str;
		public string value_str
		{
			get
			{
				if (type != EVarType.String )
					return ToEditString(false);
				else
					return _value_str;
			}
			set { _value_str = value; }
		}

		public static implicit operator Var (bool value)
		{
			Var v = new Var ();
			v.type = EVarType.Bool;
			v.value_b = value;
			v.value_f = v.value_i = value ? 1 : 0;
			v.value_v = Vector4.one * v.value_f;
			v._value_str = "";
			return v;
		}

		public static implicit operator Var (int value)
		{
			Var v = new Var ();
			v.type = EVarType.Int;
			v.value_b = (value != 0);
			v.value_f = v.value_i = value;
			v.value_v = Vector4.one * v.value_f;
			v._value_str = "";
			return v;
		}
		
		public static implicit operator Var (float value)
		{
			Var v = new Var ();
			v.type = EVarType.Float;
			v.value_b = (value != 0.0f);
			v.value_i = (int)(value);
			v.value_f = value;
			v.value_v = Vector4.one * v.value_f;
			v._value_str = "";
			return v;
		}
		
		public static implicit operator Var (double value)
		{
			Var v = new Var ();
			v.type = EVarType.Float;
			v.value_b = (value != 0.0);
			v.value_i = (int)(value);
			v.value_f = (float)value;
			v.value_v = Vector4.one * (float)v.value_f;
			v._value_str = "";
			return v;
		}

		public static implicit operator Var (Vector2 value)
		{
			Var v = new Var ();
			v.type = EVarType.Vector;
			v.value_b = (value != Vector2.zero);
			v.value_i = (int)(value.x);
			v.value_f = value.x;
			v.value_v = new Vector4(value.x, value.y, 0, 0);
			v.value_q = Quaternion.Euler(new Vector3(value.x, value.y, 0));
			v._value_str = "";
			return v;
		}
		
		public static implicit operator Var (Vector3 value)
		{
			Var v = new Var ();
			v.type = EVarType.Vector;
			v.value_b = (value != Vector3.zero);
			v.value_i = (int)(value.x);
			v.value_f = value.x;
			v.value_v = new Vector4(value.x, value.y, value.z, 0);
			v.value_q = Quaternion.Euler(new Vector3(value.x, value.y, value.z));
			v._value_str = "";
			return v;
		}
		
		public static implicit operator Var (Vector4 value)
		{
			Var v = new Var ();
			v.type = EVarType.Vector;
			v.value_b = (value != Vector4.zero);
			v.value_i = (int)(value.x);
			v.value_f = value.x;
			v.value_v = value;
			v.value_q = Quaternion.Euler(new Vector3(value.x, value.y, value.z));
			v._value_str = "";
			return v;
		}
		
		public static implicit operator Var (Quaternion value)
		{
			Var v = new Var ();
			v.type = EVarType.Quaternion;
			v.value_b = (value != Quaternion.identity);
			v.value_i = (int)(value.w);
			v.value_f = value.w;
			Vector3 euler = value.eulerAngles;
			v.value_v = new Vector4(euler.x, euler.y, euler.z, 0);
			v.value_q = value;
			v._value_str = "";
			return v;
		}
		
		public static implicit operator Var (Color value)
		{
			Var v = new Var ();
			v.type = EVarType.Color;
			v.value_b = (value != Color.clear);
			v.value_i = (int)(value.r);
			v.value_f = value.r;
			v.value_v = new Vector4(value.r, value.g, value.b, value.a);
			v.value_q = Quaternion.identity;
			v._value_str = "";
			return v;
		}
		
		public static implicit operator Var (string value)
		{
			Var v = new Var ();
			v.type = EVarType.String;
			v.value_b = (value.ToLower() != "true");
			float.TryParse(value, out v.value_f);
			v.value_i = (int)(v.value_f);
			v.value_v = Vector4.one * v.value_f;
			v.value_q = Quaternion.identity;
			v._value_str = value;
			return v;
		}

		public override string ToString ()
		{
			if (isNull)
				return "{null}";
			return string.Format ("{0} [{1}]", value_str, type.ToString());
		}

		public string ToShortString ()
		{
			if (isNull)
				return "{null}";
			return string.Format ("{0}", value_str);
		}

		public string ToEditString (bool editing)
		{
			if (type == EVarType.Null)
			{
				if (editing)
					return "";
				else
					return "?";
			}
			if (type == EVarType.Bool)
				return value_b.ToString();
			if (type == EVarType.Int)
				return value_i.ToString();
			if (type == EVarType.Float)
				return value_f.ToString();
			if (type == EVarType.Vector)
			{
				string fmt = "";
				if (!editing) fmt = "0.##";
				string s = value_v.x.ToString(fmt) + "," + value_v.y.ToString(fmt) + "," + value_v.z.ToString(fmt);
				if (value_v.w != 0)
					s = s + "," + value_v.w.ToString(fmt);
				return s;
			}
			if (type == EVarType.Quaternion)
			{
				string fmt = "";
				if (!editing) fmt = "0.##";
				Vector3 vec = value_q.eulerAngles;
				string s = "q:" + vec.x.ToString(fmt) + "," + vec.y.ToString(fmt) + "," + vec.z.ToString(fmt);
				return s;
			}
			if (type == EVarType.Color)
			{
				string fmt = "";
				if (!editing) fmt = "0.##";
				string s = value_c.r.ToString(fmt) + "," + value_c.g.ToString(fmt) + "," 
					+ value_c.b.ToString(fmt) + "," + value_c.a.ToString(fmt);
				return s;
			}
			if (type == EVarType.String)
			{
				if (editing)
					return "'" + _value_str;
				else
					return "\"" + _value_str + "\"";
			}
			return "?";
		}

		public static Var Parse (string str)
		{
			str = str.Trim();
			// Null
			if (string.IsNullOrEmpty(str))
			{
				return 0f;
			}
			// String
			if (str[0] == '\'')
			{
				return str.Substring(1, str.Length-1);
			}
			// Quaternion
			if (str.Length >= 2 && str.Substring(0,2) == "q:")
			{
				Vector3 euler = Vector3.zero;
				string val = str.Substring(2, str.Length - 2);
				string[] xyz = val.Split(new string[] {","}, System.StringSplitOptions.None);
				for (int i = 0; i < xyz.Length && i < 3; ++i)
				{
					float v = 0.0f;
					float.TryParse(xyz[i].Trim(), out v);
					euler[i] = v;
				}
				return Quaternion.Euler(euler);
			}
			// Bool
			bool b;
			if (bool.TryParse(str, out b))
			{
				return b;
			}
			// Float
			float f;
			if (float.TryParse(str, NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite |
			                   NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint |
			                   NumberStyles.AllowExponent, NumberFormatInfo.CurrentInfo, out f))
			{
				return f;
			}
			// Vector
			Vector4 vector = Vector3.zero;
			string[] xyzw = str.Split(new string[] {","}, System.StringSplitOptions.None);
			for (int i = 0; i < xyzw.Length && i < 4; ++i)
			{
				float vec = 0.0f;
				if (!float.TryParse(xyzw[i].Trim(), out vec))
				{
					return Var.Null;
				}
				vector[i] = vec;
			}
			return vector;
		}

		internal void Write (BinaryWriter w)
		{
			if (isNull)
			{
				w.Write((int)0);
				return;
			}
			w.Write((int)(type));
			w.Write(value_b);
			w.Write(value_i);
			w.Write(value_i);
			w.Write(value_f);
			w.Write(value_v.x);
			w.Write(value_v.y);
			w.Write(value_v.z);
			w.Write(value_v.w);
			w.Write(value_q.x);
			w.Write(value_q.y);
			w.Write(value_q.z);
			w.Write(value_q.w);
			w.Write(value_v.x);
			w.Write(value_v.y);
			w.Write(value_v.z);
			w.Write(value_v.w);
			if (_value_str == null)
				_value_str = "";
			w.Write(_value_str);
		}

		internal void Read (BinaryReader r)
		{
			type = (EVarType)(r.ReadInt32());
			if (isNull)
				return;
			value_b = r.ReadBoolean();
			value_i = r.ReadInt32();
			value_i = r.ReadInt32();
			value_f = r.ReadSingle();
			value_v.x = r.ReadSingle();
			value_v.y = r.ReadSingle();
			value_v.z = r.ReadSingle();
			value_v.w = r.ReadSingle();
			value_q.x = r.ReadSingle();
			value_q.y = r.ReadSingle();
			value_q.z = r.ReadSingle();
			value_q.w = r.ReadSingle();
			r.ReadSingle();
			r.ReadSingle();
			r.ReadSingle();
			r.ReadSingle();
			_value_str = r.ReadString();
		}
	}
}
