using System;
using System.Globalization;
using UnityEngine;
using System.Collections;

namespace PeCustom
{
	public struct RANGE
	{
		public enum RANGETYPE
		{
			Anywhere = 0,
			Box = 1,
			Sphere = 2,
			Circle = 3
		}

		public static RANGE anywhere
		{
			get
			{
				RANGE r = new RANGE();
				r.type = RANGETYPE.Anywhere;
				r.inverse = false;
				return r;
			}
		}

		public static RANGE nowhere
		{
			get
			{
				RANGE r = new RANGE();
				r.type = RANGETYPE.Anywhere;
				r.inverse = true;
				return r;
			}
		}

		public RANGETYPE type;
		public Vector3 center;
		public Vector3 extend;
		public float radius;
		public bool inverse;

		public bool Contains (Vector3 position)
		{
			if (type == RANGETYPE.Anywhere)
				return inverse ^ true;
			if (type == RANGETYPE.Box)
			{
				return inverse ^ 
					   (Mathf.Abs(position.x - center.x) <= extend.x &&
					    Mathf.Abs(position.y - center.y) <= extend.y &&
						Mathf.Abs(position.z - center.z) <= extend.z);
			}
			if (type == RANGETYPE.Sphere)
			{
				return inverse ^ (Vector3.Distance(position, center) <= radius);
			}
			if (type == RANGETYPE.Circle)
			{
				position.y = center.y;
				return inverse ^ (Vector3.Distance(position, center) <= radius);
			}
			return false;
		}
	}

	public struct DIRRANGE
	{
		public enum DIRRANGETYPE
		{
			Anydirection = 0,
			Cone = 1,
			Fan = 2,
		}

		public static DIRRANGE anydir
		{
			get
			{
				DIRRANGE r = new DIRRANGE();
				r.type = DIRRANGETYPE.Anydirection;
				r.inverse = false;
				return r;
			}
		}

		public static DIRRANGE nodir
		{
			get
			{
				DIRRANGE r = new DIRRANGE();
				r.type = DIRRANGETYPE.Anydirection;
				r.inverse = true;
				return r;
			}
		}

		public DIRRANGETYPE type;
		public Vector3 directrix;
		public Vector2 error;
		public bool inverse;

		public bool Contains (Vector3 dir)
		{
			if (type == DIRRANGETYPE.Anydirection)
				return inverse ^ true;
			if (type == DIRRANGETYPE.Cone)
				return inverse ^ (Vector3.Angle(directrix, dir) <= error.x);
			if (type == DIRRANGETYPE.Fan)
			{
				Vector3 _dir = dir;
				Vector3 _directrix = directrix;
				_dir.y = _directrix.y = 0;
				bool yaw = Vector3.Angle(_dir, _directrix) <= error.x;
				_dir = dir.normalized;
				_directrix = directrix.normalized;
				float _p1 = Mathf.Asin(_dir.y) * Mathf.Rad2Deg;
				float _p2 = Mathf.Asin(_directrix.y) * Mathf.Rad2Deg;
				bool pitch = Mathf.Abs(_p1 - _p2) <= error.y;
				return inverse ^ (yaw && pitch);
			}
			return false;
		}
	}

	public struct OBJECT
	{
		public enum OBJECTTYPE
		{
			Null = 0,
			AnyObject = 1,
			Player = 2,
			WorldObject = 3,
			ItemProto = 4,
			MonsterProto = 5
		}

		public static OBJECT Null
		{
			get
			{
				OBJECT obj = new OBJECT();
				obj.type = OBJECTTYPE.Null;
				return obj;
			}
		}

		public static OBJECT Any
		{
			get
			{
				OBJECT obj = new OBJECT();
				obj.type = OBJECTTYPE.AnyObject;
				obj.Group = -1;
				obj.Id = -1;
				return obj;
			}
		}

		public bool isSpecificEntity
		{
			get { return isPlayerId || isNpoId; }
		}

		public bool isPlayerId
		{
			get { return type == OBJECTTYPE.Player && Id >= 0 && Group == 0; }
		}

		public bool isForceId
		{
			get { return type == OBJECTTYPE.Player && Id == -1 && Group >= 0; }
		}

		public bool isCurrentPlayer
		{
			get { return type == OBJECTTYPE.Player && Id == 0 && Group == -1; }
		}

		public bool isAnyPlayer
		{
			get { return type == OBJECTTYPE.Player && Id == -1 && Group == -1; }
		}

		public bool isAnyOtherPlayer
		{
			get { return type == OBJECTTYPE.Player && Id == -2 && Group == -1; }
		}

		public bool isAnyOtherForce
		{
			get { return type == OBJECTTYPE.Player && Id == -1 && Group == -2; }
		}

		public bool isAllyForce
		{
			get { return type == OBJECTTYPE.Player && Id == -1 && Group == -3; }
		}

		public bool isEnemyForce
		{
			get { return type == OBJECTTYPE.Player && Id == -1 && Group == -4; }
		}

		public bool isNpoId
		{
			get { return type == OBJECTTYPE.WorldObject && Id > 0 && Group >= 0; }
		}

		public bool isAnyNpo
		{
			get { return type == OBJECTTYPE.WorldObject && Id == -1 && Group == -1; }
		}

		public bool isAnyNpoInSpecificWorld
		{
			get { return type == OBJECTTYPE.WorldObject && Id == -1 && Group >= 0; }
		}

		public bool isPrototype
		{
			get { return type == OBJECTTYPE.ItemProto || type == OBJECTTYPE.MonsterProto; }
		}

		public bool isAnyPrototype
		{
			get { return isPrototype && Id == -1 && Group == -1; }
		}

		public bool isAnyPrototypeInCategory
		{
			get { return isPrototype && Id == -1 && Group >= 0; }
		}

        public bool isSpecificPrototype
        {
			get { return isPrototype && Id >= 0; }
        }

		public OBJECTTYPE type;
		public int Group;
		public int Id;
	}

	public static class SEType
	{
		public static NumberStyles floatStyle = NumberStyles.AllowLeadingWhite |
			NumberStyles.AllowTrailingWhite |
			NumberStyles.AllowLeadingSign |
			NumberStyles.AllowDecimalPoint;
		#region VECTOR
		public static bool TryParse_VECTOR(string s, out Vector3 vec)
		{
			vec = Vector3.zero;
			string[] ss = s.Split(new string[1] {","}, System.StringSplitOptions.None);
			if (ss.Length == 0)
				return false;
			if (ss.Length == 1)
			{
				string strx = ss[0];
				float x = 0;
				if (!float.TryParse(strx, floatStyle , NumberFormatInfo.CurrentInfo, out x))
					return false;
				vec.x = x;
				return true;
			}
			if (ss.Length == 2)
			{
				string strx = ss[0];
				float x = 0;
				if (!float.TryParse(strx, floatStyle , NumberFormatInfo.CurrentInfo, out x))
					return false;
				vec.x = x;
				string stry = ss[1];
				float y = 0;
				if (!float.TryParse(stry, floatStyle , NumberFormatInfo.CurrentInfo, out y))
					return false;
				vec.y = y;
				return true;
			}
			if (ss.Length == 3)
			{
				string strx = ss[0];
				float x = 0;
				if (!float.TryParse(strx, floatStyle , NumberFormatInfo.CurrentInfo, out x))
					return false;
				vec.x = x;
				string stry = ss[1];
				float y = 0;
				if (!float.TryParse(stry, floatStyle , NumberFormatInfo.CurrentInfo, out y))
					return false;
				vec.y = y;
				string strz = ss[2];
				float z = 0;
				if (!float.TryParse(strz, floatStyle , NumberFormatInfo.CurrentInfo, out z))
					return false;
				vec.z = z;
				return true;
			}
			return false;
		}

		public static string ToString_VECTOR (Vector3 vec)
		{
			return vec.x.ToString("0.###") + ", " + vec.y.ToString("0.###") + ", " + vec.z.ToString("0.###");
		}
		#endregion

		#region RECT
		public static bool TryParse_RECT(string s, out Rect vec)
		{
			vec = new Rect (0,0,0,0);
			string[] ss = s.Split(new string[1] {","}, System.StringSplitOptions.None);
			if (ss.Length == 0)
				return false;
			if (ss.Length == 1)
			{
				string strx = ss[0];
				float x = 0;
				if (!float.TryParse(strx, floatStyle , NumberFormatInfo.CurrentInfo, out x))
					return false;
				vec.x = x;
				return true;
			}
			if (ss.Length == 2)
			{
				string strx = ss[0];
				float x = 0;
				if (!float.TryParse(strx, floatStyle , NumberFormatInfo.CurrentInfo, out x))
					return false;
				vec.x = x;
				string stry = ss[1];
				float y = 0;
				if (!float.TryParse(stry, floatStyle , NumberFormatInfo.CurrentInfo, out y))
					return false;
				vec.y = y;
				return true;
			}
			if (ss.Length == 3)
			{
				string strx = ss[0];
				float x = 0;
				if (!float.TryParse(strx, floatStyle , NumberFormatInfo.CurrentInfo, out x))
					return false;
				vec.x = x;
				string stry = ss[1];
				float y = 0;
				if (!float.TryParse(stry, floatStyle , NumberFormatInfo.CurrentInfo, out y))
					return false;
				vec.y = y;
				string strz = ss[2];
				float z = 0;
				if (!float.TryParse(strz, floatStyle , NumberFormatInfo.CurrentInfo, out z))
					return false;
				vec.width = z;
				return true;
			}
			if (ss.Length == 4)
			{
				string strx = ss[0];
				float x = 0;
				if (!float.TryParse(strx, floatStyle , NumberFormatInfo.CurrentInfo, out x))
					return false;
				vec.x = x;
				string stry = ss[1];
				float y = 0;
				if (!float.TryParse(stry, floatStyle , NumberFormatInfo.CurrentInfo, out y))
					return false;
				vec.y = y;
				string strz = ss[2];
				float z = 0;
				if (!float.TryParse(strz, floatStyle , NumberFormatInfo.CurrentInfo, out z))
					return false;
				vec.width = z;
				string strw = ss[3];
				float w = 0;
				if (!float.TryParse(strw, floatStyle , NumberFormatInfo.CurrentInfo, out w))
					return false;
				vec.height = w;
				return true;
			}
			return false;
		}

		public static string ToString_RECT (Rect vec)
		{
			return vec.x.ToString("0.###") + ", " + vec.y.ToString("0.###") + ", " + vec.width.ToString("0.###") + ", " + vec.height.ToString("0.###");
		}
		#endregion

		#region COLOR
		public static bool TryParse_COLOR(string s, out Color32 vec)
		{
			vec = new Color32 (0,0,0,0);
			int l = s.Length;
			if (l != 9 && l != 7)
				return false;
			if (s[0] != '#')
				return false;

			try
			{
				string str = s.Substring(1, l - 1);
				uint val = Convert.ToUInt32(str, 16);
				vec.b = (byte)(val & 0xff);
				vec.g = (byte)((val >> 8) & 0xff);
				vec.r = (byte)((val >> 16) & 0xff);
				vec.a = (byte)((val >> 24) & 0xff);
				if (l == 7)
					vec.a = 255;
				return true;
			}
			catch
			{
				return false;
			}
		}

		public static string ToString_COLOR (Color32 vec)
		{
			if (vec.a != 255)
				return string.Format("#{0}{1}{2}{3}",
					vec.a.ToString("X").PadLeft(2,'0'),
					vec.r.ToString("X").PadLeft(2,'0'),
					vec.g.ToString("X").PadLeft(2,'0'),
					vec.b.ToString("X").PadLeft(2,'0'));
			else
				return string.Format("#{0}{1}{2}",
					vec.r.ToString("X").PadLeft(2,'0'),
					vec.g.ToString("X").PadLeft(2,'0'),
					vec.b.ToString("X").PadLeft(2,'0'));
		}
		#endregion

		#region RANGE
		public static bool TryParse_RANGE(string s, out RANGE range)
		{
			if (s == "0")
			{
				range = RANGE.nowhere;
				return true;
			}
			else if (s == "-1")
			{
				range = RANGE.anywhere;
				return true;
			}
			else
			{
				range = RANGE.anywhere;
				string[] ss = s.Split(new string[1] {":"}, StringSplitOptions.None);
				if (ss.Length != 3)
					return false;
				ss[0] = ss[0];
				if (ss[0] == "B" || ss[0] == "B'")
				{
					range.type = RANGE.RANGETYPE.Box;
					if (ss[0].Length == 1)
						range.inverse = false;
					else
						range.inverse = true;

					if (!TryParse_VECTOR(ss[1], out range.center))
						return false;
					if (!TryParse_VECTOR(ss[2], out range.extend))
						return false;
					return true;
				}
				else if (ss[0] == "S" || ss[0] == "S'")
				{
					range.type = RANGE.RANGETYPE.Sphere;
					if (ss[0].Length == 1)
						range.inverse = false;
					else
						range.inverse = true;

					if (!TryParse_VECTOR(ss[1], out range.center))
						return false;
					if (!float.TryParse(ss[2], floatStyle , NumberFormatInfo.CurrentInfo, out range.radius))
						return false;
					return true;
				}
				else if (ss[0] == "C" || ss[0] == "C'")
				{
					range.type = RANGE.RANGETYPE.Circle;
					if (ss[0].Length == 1)
						range.inverse = false;
					else
						range.inverse = true;

					if (!TryParse_VECTOR(ss[1], out range.center))
						return false;
					if (!float.TryParse(ss[2], floatStyle , NumberFormatInfo.CurrentInfo, out range.radius))
						return false;
					return true;
				}
				return false;
			}
		}

		public static string ToString_RANGE (RANGE range)
		{
			string formatstr = "";
			switch (range.type)
			{
			case RANGE.RANGETYPE.Anywhere:
				return range.inverse ? "0" : "-1";
			case RANGE.RANGETYPE.Box:
				if (range.inverse)
					formatstr = "B':{0},{1},{2}:{3},{4},{5}";
				else
					formatstr = "B:{0},{1},{2}:{3},{4},{5}";
				return string.Format(formatstr,
					range.center.x, range.center.y, range.center.z, 
					range.extend.x, range.extend.y, range.extend.z);
			case RANGE.RANGETYPE.Sphere:
				if (range.inverse)
					formatstr = "S':{0},{1},{2}:{3}";
				else
					formatstr = "S:{0},{1},{2}:{3}";
				return string.Format(formatstr, range.center.x, range.center.y, range.center.z, range.radius);
			case RANGE.RANGETYPE.Circle:
				if (range.inverse)
					formatstr = "C':{0},{1},{2}:{3}";
				else
					formatstr = "C:{0},{1},{2}:{3}";
				return string.Format(formatstr, range.center.x, range.center.y, range.center.z, range.radius);
			}
			return "0";
		}
		#endregion

		#region DIR_RANGE
		public static bool TryParse_DIRRANGE(string s, out DIRRANGE range)
		{
			if (s == "0")
			{
				range = DIRRANGE.nodir;
				return true;
			}
			else if (s == "-1")
			{
				range = DIRRANGE.anydir;
				return true;
			}
			else
			{
				range = DIRRANGE.anydir;
				string[] ss = s.Split(new string[1] {":"}, StringSplitOptions.None);
				if (ss.Length != 3)
					return false;
				ss[0] = ss[0];
				if (ss[0] == "C" || ss[0] == "C'")
				{
					range.type = DIRRANGE.DIRRANGETYPE.Cone;
					if (ss[0].Length == 1)
						range.inverse = false;
					else
						range.inverse = true;

					Vector3 error = Vector3.zero;
					if (!TryParse_VECTOR(ss[1], out range.directrix))
						return false;
					if (!TryParse_VECTOR(ss[2], out error))
						return false;
					range.error.x = error.x;
					range.error.y = error.y;

					return true;
				}
				else if (ss[0] == "F" || ss[0] == "F'")
				{
					range.type = DIRRANGE.DIRRANGETYPE.Fan;
					if (ss[0].Length == 1)
						range.inverse = false;
					else
						range.inverse = true;

					Vector3 error = Vector3.zero;
					if (!TryParse_VECTOR(ss[1], out range.directrix))
						return false;
					if (!TryParse_VECTOR(ss[2], out error))
						return false;
					range.error.x = error.x;
					range.error.y = error.y;

					return true;
				}
				return false;
			}
		}

		public static string ToString_DIRRANGE (DIRRANGE range)
		{
			string formatstr = "";
			switch (range.type)
			{
			case DIRRANGE.DIRRANGETYPE.Anydirection:
				return range.inverse ? "0" : "-1";
			case DIRRANGE.DIRRANGETYPE.Cone:
				if (range.inverse)
					formatstr = "C':{0},{1},{2}:{3}";
				else
					formatstr = "C:{0},{1},{2}:{3}";
				return string.Format(formatstr, range.directrix.x, range.directrix.y, range.directrix.z, range.error.x);
			case DIRRANGE.DIRRANGETYPE.Fan:
				if (range.inverse)
					formatstr = "F':{0},{1},{2}:{3},{4}";
				else
					formatstr = "F:{0},{1},{2}:{3},{4}";
				return string.Format(formatstr, range.directrix.x, range.directrix.y, range.directrix.z, range.error.x, range.error.y);
			}
			return "0";
		}
		#endregion

		#region OBJECT
		public static bool TryParse_OBJECT(string s, out OBJECT obj)
		{
			obj = new OBJECT ();
			if (s == "-1")
			{
				obj.type = OBJECT.OBJECTTYPE.AnyObject;
				obj.Group = -1;
				obj.Id = -1;
				return true;
			}
			string[] ss = s.Split(new string[1] {"/"}, System.StringSplitOptions.None);
			if (ss.Length == 3)
			{
				if (ss[0] == "P:")
					obj.type = OBJECT.OBJECTTYPE.Player;
				else if (ss[0] == "W:")
					obj.type = OBJECT.OBJECTTYPE.WorldObject;
				else if (ss[0] == "I:")
					obj.type = OBJECT.OBJECTTYPE.ItemProto;
				else if (ss[0] == "M:")
					obj.type = OBJECT.OBJECTTYPE.MonsterProto;
				else
					return false;

				if (!int.TryParse(ss[1], out obj.Group))
					return false;
				if (!int.TryParse(ss[2], out obj.Id))
					return false;

				if (obj.type == OBJECT.OBJECTTYPE.Player)
				{
					if (obj.Id == 0 && obj.Group == -1)
						return true;
					else if (obj.Id == -1 && obj.Group == -1)
						return true;
					else if (obj.Id == -2 && obj.Group == -1)
						return true;
					else if (obj.Id == -1 && obj.Group == -2)
						return true;
					else if (obj.Id == -1 && obj.Group == -3)
						return true;
					else if (obj.Id == -1 && obj.Group == -4)
						return true;
					else if (obj.Id >= 0 && obj.Group == 0)
						return true;
					else if (obj.Id == -1 && obj.Group >= 0)
						return true;
					else
						return false;
				}
				else if (obj.type == OBJECT.OBJECTTYPE.WorldObject)
				{
					if (obj.Group == -1 && obj.Id == -1)
						return true;
					else if (obj.Group >= 0 && obj.Id == -1)
						return true;
					else if (obj.Group >= 0 && obj.Id > 0)
						return true;
					else
						return false;
				}
				else if (obj.type == OBJECT.OBJECTTYPE.ItemProto || obj.type == OBJECT.OBJECTTYPE.MonsterProto)
				{
					if (obj.Group == -1 && obj.Id == -1)
						return true;
					else if (obj.Group >= 0 && obj.Id == -1)
						return true;
					else if (obj.Id > 0)
						return true;
					else
						return false;
				}

				return true;
			}
			return false;
		}

		public static string ToString_OBJECT (OBJECT obj)
		{
			if (obj.type == OBJECT.OBJECTTYPE.AnyObject)
				return "-1";
			string head = "";
			if (obj.type == OBJECT.OBJECTTYPE.Player)
				head = "P:/";
			else if (obj.type == OBJECT.OBJECTTYPE.WorldObject)
				head = "W:/";
			else if (obj.type == OBJECT.OBJECTTYPE.ItemProto)
				head = "I:/";
			else if (obj.type == OBJECT.OBJECTTYPE.MonsterProto)
				head = "M:/";
			else
				return "";
			return head + obj.Group.ToString() + "/" + obj.Id.ToString();
		}
		#endregion

	}
}