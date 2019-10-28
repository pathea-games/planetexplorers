using System.Globalization;
using UnityEngine;
using PeCustom;

namespace ScenarioRTL
{
	public static class Utility
	{
		public static bool Compare (bool lhs, bool rhs, ECompare comp)
		{
			int l = lhs ? 1 : 0;
			int r = rhs ? 1 : 0;
			switch (comp)
			{
			case ECompare.Greater: return l > r;
			case ECompare.GreaterEqual: return l >= r;
			case ECompare.Equal: return l == r;
			case ECompare.NotEqual: return l != r;
			case ECompare.LessEqual: return l <= r;
			case ECompare.Less: return l < r;
			default: return false;
			}
		}
		public static bool Compare (int lhs, int rhs, ECompare comp)
		{
			switch (comp)
			{
			case ECompare.Greater: return lhs > rhs;
			case ECompare.GreaterEqual: return lhs >= rhs;
			case ECompare.Equal: return lhs == rhs;
			case ECompare.NotEqual: return lhs != rhs;
			case ECompare.LessEqual: return lhs <= rhs;
			case ECompare.Less: return lhs < rhs;
			default: return false;
			}
		}
		public static bool Compare (float lhs, int rhs, ECompare comp)
		{
			switch (comp)
			{
			case ECompare.Greater: return lhs > rhs;
			case ECompare.GreaterEqual: return lhs >= rhs;
			case ECompare.Equal: return lhs == rhs;
			case ECompare.NotEqual: return lhs != rhs;
			case ECompare.LessEqual: return lhs <= rhs;
			case ECompare.Less: return lhs < rhs;
			default: return false;
			}
		}
		public static bool Compare (int lhs, float rhs, ECompare comp)
		{
			switch (comp)
			{
			case ECompare.Greater: return lhs > rhs;
			case ECompare.GreaterEqual: return lhs >= rhs;
			case ECompare.Equal: return lhs == rhs;
			case ECompare.NotEqual: return lhs != rhs;
			case ECompare.LessEqual: return lhs <= rhs;
			case ECompare.Less: return lhs < rhs;
			default: return false;
			}
		}
		public static bool Compare (float lhs, float rhs, ECompare comp)
		{
			switch (comp)
			{
			case ECompare.Greater: return lhs > rhs;
			case ECompare.GreaterEqual: return lhs >= rhs;
			case ECompare.Equal: return lhs == rhs;
			case ECompare.NotEqual: return lhs != rhs;
			case ECompare.LessEqual: return lhs <= rhs;
			case ECompare.Less: return lhs < rhs;
			default: return false;
			}
		}
		public static bool Compare (double lhs, float rhs, ECompare comp)
		{
			switch (comp)
			{
			case ECompare.Greater: return lhs > rhs;
			case ECompare.GreaterEqual: return lhs >= rhs;
			case ECompare.Equal: return lhs == rhs;
			case ECompare.NotEqual: return lhs != rhs;
			case ECompare.LessEqual: return lhs <= rhs;
			case ECompare.Less: return lhs < rhs;
			default: return false;
			}
		}
		public static bool Compare (float lhs, double rhs, ECompare comp)
		{
			switch (comp)
			{
			case ECompare.Greater: return lhs > rhs;
			case ECompare.GreaterEqual: return lhs >= rhs;
			case ECompare.Equal: return lhs == rhs;
			case ECompare.NotEqual: return lhs != rhs;
			case ECompare.LessEqual: return lhs <= rhs;
			case ECompare.Less: return lhs < rhs;
			default: return false;
			}
		}
		public static bool Compare (double lhs, double rhs, ECompare comp)
		{
			switch (comp)
			{
			case ECompare.Greater: return lhs > rhs;
			case ECompare.GreaterEqual: return lhs >= rhs;
			case ECompare.Equal: return lhs == rhs;
			case ECompare.NotEqual: return lhs != rhs;
			case ECompare.LessEqual: return lhs <= rhs;
			case ECompare.Less: return lhs < rhs;
			default: return false;
			}
		}
		public static bool Compare (int lhs, double rhs, ECompare comp)
		{
			switch (comp)
			{
			case ECompare.Greater: return lhs > rhs;
			case ECompare.GreaterEqual: return lhs >= rhs;
			case ECompare.Equal: return lhs == rhs;
			case ECompare.NotEqual: return lhs != rhs;
			case ECompare.LessEqual: return lhs <= rhs;
			case ECompare.Less: return lhs < rhs;
			default: return false;
			}
		}
		public static bool Compare (double lhs, int rhs, ECompare comp)
		{
			switch (comp)
			{
			case ECompare.Greater: return lhs > rhs;
			case ECompare.GreaterEqual: return lhs >= rhs;
			case ECompare.Equal: return lhs == rhs;
			case ECompare.NotEqual: return lhs != rhs;
			case ECompare.LessEqual: return lhs <= rhs;
			case ECompare.Less: return lhs < rhs;
			default: return false;
			}
		}
		public static bool Compare (string lhs, string rhs, ECompare comp)
		{
			switch (comp)
			{
			case ECompare.Greater: return string.Compare(lhs, rhs) > 0;
			case ECompare.GreaterEqual: return string.Compare(lhs, rhs) >= 0;
			case ECompare.Equal: return string.Compare(lhs, rhs) == 0;
			case ECompare.NotEqual: return string.Compare(lhs, rhs) != 0;
			case ECompare.LessEqual: return string.Compare(lhs, rhs) <= 0;
			case ECompare.Less: return string.Compare(lhs, rhs) < 0;
			default: return false;
			}
		}
		public static bool CompareVar (Var lhs, Var rhs, ECompare comp)
		{
			if (lhs.isInteger && rhs.isInteger)
				return Compare((int)lhs, (int)rhs, comp);
			if (lhs.isNumber && rhs.isNumber)
				return Compare((double)lhs, (double)rhs, comp);
			if (lhs.isString && rhs.isString)
				return Compare((string)lhs, (string)rhs, comp);
			if (lhs.isBool && rhs.isBool)
				return Compare((bool)lhs, (bool)rhs, comp);
			if (lhs.isBool && rhs.isInteger)
				return Compare((bool)lhs, ((int)rhs != 0), comp);
			if (lhs.isInteger && rhs.isBool)
				return Compare(((int)lhs != 0), (bool)rhs, comp);
			return lhs;
		}

		public static bool Function (bool lhs, bool rhs, EFunc func)
		{
			switch (func)
			{
			case EFunc.SetTo: return rhs;
			case EFunc.Plus: return lhs | rhs;
			case EFunc.Minus: return lhs ^ rhs;
			case EFunc.Multiply: return lhs & rhs;
			case EFunc.Divide: return lhs & rhs;
			case EFunc.Mod: return false;
			case EFunc.Power: return lhs;
			case EFunc.XOR: return lhs ^ rhs;
			default: return rhs;
			}
		}
		public static int Function (int lhs, int rhs, EFunc func)
		{
			switch (func)
			{
			case EFunc.SetTo: return rhs;
			case EFunc.Plus: return lhs + rhs;
			case EFunc.Minus: return lhs - rhs;
			case EFunc.Multiply: return lhs * rhs;
			case EFunc.Divide: return rhs == 0 ? 0 : lhs / rhs;
			case EFunc.Mod: return rhs == 0 ? 0 : lhs % rhs;
			case EFunc.Power: return lhs == 0 ? 0 : (int)System.Math.Pow(lhs, rhs);
			case EFunc.XOR: return lhs ^ rhs;
			default: return rhs;
			}
		}
		public static float Function (float lhs, float rhs, EFunc func)
		{
			switch (func)
			{
			case EFunc.SetTo: return rhs;
			case EFunc.Plus: return lhs + rhs;
			case EFunc.Minus: return lhs - rhs;
			case EFunc.Multiply: return lhs * rhs;
			case EFunc.Divide: return rhs == 0 ? 0 : lhs / rhs;
			case EFunc.Mod: return rhs == 0 ? 0 : lhs % rhs;
			case EFunc.Power: return lhs == 0 ? 0 : (float)System.Math.Pow(lhs, rhs);
			case EFunc.XOR: return (int)lhs ^ (int)rhs;
			default: return rhs;
			}
		}
		public static double Function (double lhs, double rhs, EFunc func)
		{
			switch (func)
			{
			case EFunc.SetTo: return rhs;
			case EFunc.Plus: return lhs + rhs;
			case EFunc.Minus: return lhs - rhs;
			case EFunc.Multiply: return lhs * rhs;
			case EFunc.Divide: return rhs == 0 ? 0 : lhs / rhs;
			case EFunc.Mod: return rhs == 0 ? 0 : lhs % rhs;
			case EFunc.Power: return lhs == 0 ? 0 : System.Math.Pow(lhs, rhs);
			case EFunc.XOR: return (int)lhs ^ (int)rhs;
			default: return rhs;
			}
		}
		public static string Function (string lhs, string rhs, EFunc func)
		{
			switch (func)
			{
			case EFunc.SetTo: return rhs;
			case EFunc.Plus: return lhs + rhs;
			default: return rhs;
			}
		}
		public static Var FunctionVar (Var lhs, Var rhs, EFunc func)
		{
			if (func == EFunc.SetTo)
				return rhs;
			if (lhs.isInteger && rhs.isInteger)
				return Function((int)lhs, (int)rhs, func);
			if (lhs.isNumber && rhs.isNumber)
				return Function((double)lhs, (double)rhs, func);
			if (lhs.isString && rhs.isString)
				return Function((string)lhs, (string)rhs, func);
			if (lhs.isBool && rhs.isBool)
				return Function((bool)lhs, (bool)rhs, func);
			if (lhs.isBool && rhs.isInteger)
				return Function((bool)lhs, ((int)rhs != 0), func);
			if (lhs.isInteger && rhs.isBool)
				return Function(((int)lhs != 0), (bool)rhs, func);
			return lhs;
		}

		// Conversion
		public static string GetVarName (string value)
		{
			int l = value.Length;
			if (l < 3)
				return null;
			if (value[0] == '%' && value[l-1] == '%')
				return value.Substring(1, l-2);
			return null;
		}

		public static int ToEnumInt (string value)
		{
			int retval = 0;
			if (int.TryParse(value, out retval))
				return retval;
			return 0;
		}

		public static int ToInt (VarScope vs, string value)
		{
			int retval = 0;
			if (vs != null)
			{
				string varname = GetVarName(value);
				if (varname != null)
					return (int)(vs[varname]);
			}
			if (int.TryParse(value, out retval))
				return retval;
			return 0;
		}

		public static ECompare ToCompare (string value)
		{
			int retval = 0;
			if (int.TryParse(value, out retval))
				return (ECompare)retval;
			return (ECompare)0;
		}

		public static EFunc ToFunc (string value)
		{
			int retval = 0;
			if (int.TryParse(value, out retval))
				return (EFunc)retval;
			return EFunc.SetTo;
		}

		public static bool ToBool (VarScope vs, string value)
		{
			int retval = 0;
			if (vs != null)
			{
				string varname = GetVarName(value);
				if (varname != null)
					return (bool)(vs[varname]);
			}
			if (int.TryParse(value, out retval))
				return retval != 0;
			if (value.ToLower() == "true")
				return true;
			return false;
		}

		public static float ToSingle (VarScope vs, string value)
		{
			float retval = 0;
			if (vs != null)
			{
				string varname = GetVarName(value);
				if (varname != null)
					return (float)(vs[varname]);
			}
			if (float.TryParse(value, NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite |
				NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint |
				NumberStyles.AllowExponent, NumberFormatInfo.CurrentInfo, out retval))
				return retval;
			return 0;
		}

		public static double ToDouble (VarScope vs, string value)
		{
			double retval = 0;
			if (vs != null)
			{
				string varname = GetVarName(value);
				if (varname != null)
					return (double)(vs[varname]);
			}
			if (double.TryParse(value, NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite |
				NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint |
				NumberStyles.AllowExponent, NumberFormatInfo.CurrentInfo, out retval))
				return retval;
			return 0;
		}

		public static string ToText (VarScope vs, string value)
		{
			if (vs != null)
			{
				string varname = GetVarName(value);
				if (varname != null)
					return (string)vs[varname];
			}
			return value;
		}

		public static Var ToVar (VarScope vs, string value)
		{
			if (vs != null)
			{
				string varname = GetVarName(value);
				if (varname != null)
					return vs[varname];
			}
			return new Var(value);
		}

		public static string ToVarname (string value)
		{
			string varname = GetVarName(value);
			if (varname != null)
				return varname;
			return value;
		}

		public static Vector3 ToVector (VarScope vs, string value)
		{
			Vector3 retval;
			if (vs != null)
			{
				string varname = GetVarName(value);
				if (varname != null)
					value = vs[varname].data;
			}
			if (SEType.TryParse_VECTOR(value, out retval))
				return retval;
			return Vector3.zero;
		}

		public static Rect ToRect (VarScope vs, string value)
		{
			Rect retval;
			if (vs != null)
			{
				string varname = GetVarName(value);
				if (varname != null)
					value = vs[varname].data;
			}

			if (SEType.TryParse_RECT(value, out retval))
				return retval;
			return new Rect(0,0,0,0);
		}

		public static Color ToColor (VarScope vs, string value)
		{
			Color32 retval;
			if (vs != null)
			{
				string varname = GetVarName(value);
				if (varname != null)
					value = vs[varname].data;
			}

			if (SEType.TryParse_COLOR(value, out retval))
				return (Color)retval;
			return Color.white;
		}

		public static RANGE ToRange (VarScope vs, string value)
		{
			RANGE retval;
			if (vs != null)
			{
				string varname = GetVarName(value);
				if (varname != null)
					value = vs[varname].data;
			}

			if (SEType.TryParse_RANGE(value, out retval))
				return retval;
			return RANGE.nowhere;
		}

		public static DIRRANGE ToDirRange (VarScope vs, string value)
		{
			DIRRANGE retval;
			if (vs != null)
			{
				string varname = GetVarName(value);
				if (varname != null)
					value = vs[varname].data;
			}

			if (SEType.TryParse_DIRRANGE(value, out retval))
				return retval;
			return DIRRANGE.nodir;
		}

		public static OBJECT ToObject (string value)
		{
			OBJECT retval;
			if (SEType.TryParse_OBJECT(value, out retval))
				return retval;
			return OBJECT.Null;
		}
	}
}
