using System.Text;
using System.Collections.Generic;
using System.Xml;
using Vector3 = UnityEngine.Vector3;

namespace PatheaScript
{
    public static class Util
    {
		static StringBuilder s_strBuff = new StringBuilder(64);
		public static string GetStrHhMmSs(this UTimer t)
		{
			UTimer.TimeStruct ts = t.TickToTimeStruct(t.Tick);
			int h = ts.Hour;
			int m = ts.Minute;
			int s = ts.Second;
			s_strBuff.Length = 0;
			s_strBuff.Append ((char)('0'+(h/10)));
			s_strBuff.Append ((char)('0'+(h%10)));
			s_strBuff.Append (':');
			s_strBuff.Append ((char)('0'+(m/10)));
			s_strBuff.Append ((char)('0'+(m%10)));
			s_strBuff.Append (':');
			s_strBuff.Append ((char)('0'+(s/10)));
			s_strBuff.Append ((char)('0'+(s%10)));
			return s_strBuff.ToString();
		}
		public static string GetStrHhMm(this UTimer t)
		{
			UTimer.TimeStruct ts = t.TickToTimeStruct(t.Tick);
			int h = ts.Hour;
			int m = ts.Minute;
			s_strBuff.Length = 0;
			s_strBuff.Append ((char)('0'+(h/10)));
			s_strBuff.Append ((char)('0'+(h%10)));
			s_strBuff.Append (':');
			s_strBuff.Append ((char)('0'+(m/10)));
			s_strBuff.Append ((char)('0'+(m%10)));
			return s_strBuff.ToString();
		}
		public static string GetStrPXZ(this Pathea.PeTrans trans)
		{
			s_strBuff.Length = 0;
			s_strBuff.Append ('P');
			s_strBuff.Append (':');
			s_strBuff.Append ((int)trans.position.x);
			s_strBuff.Append (',');
			s_strBuff.Append ((int)trans.position.z);
			return s_strBuff.ToString();
		}
		public static string GetChunkName(IntVector4 cpos)
		{
			s_strBuff.Length = 0;
			s_strBuff.Append ('C');
			s_strBuff.Append ('h');
			s_strBuff.Append ('n');
			s_strBuff.Append ('k');
			s_strBuff.Append ('_');
			s_strBuff.Append (cpos.x);
			s_strBuff.Append ('_');
			s_strBuff.Append (cpos.y);
			s_strBuff.Append ('_');
			s_strBuff.Append (cpos.z);
			return s_strBuff.ToString();
		}
        public static string Unescape(string origin)
        {
            return System.Uri.UnescapeDataString(origin);
        }

        public static string GetStmtName(XmlNode xmlNode)
        {
            if (xmlNode.Name != "STMT")
            {
                throw new System.Exception("not STMT node.");
            }

            return xmlNode.Attributes["stmt"].Value;
        }

        public static int GetEventPriority(XmlNode xmlNode)
        {
            return GetInt(xmlNode, "order");
        }

        public static Variable.EScope GetVarScope(XmlNode xmlNode)
        {
            int type = GetInt(xmlNode, "scope");

            switch (type)
            {
                case 1:
                    return Variable.EScope.Gloabel;
                case 2:
                    return Variable.EScope.Script;
                case 3:
                    return Variable.EScope.Trigger;
                default:
                    return Variable.EScope.Trigger;
            }
        }

        public static PsScript.EResult GetScriptResult(XmlNode xmlNode)
        {
            int type = GetInt(xmlNode, "result");

            switch (type)
            {
                case 1:
                    return PsScript.EResult.Accomplished;
                case 2:
                    return PsScript.EResult.Failed;
                case 3:
                    return PsScript.EResult.Abort;
                default:
                    return PsScript.EResult.Max;
            }
        }

        //variable name is just a string. not bracketed in %
        public static VarRef GetVarRef(XmlNode xmlNode, string name, Trigger trigger)
        {
            string varName = Util.GetString(xmlNode, name);
            return new VarRef(varName, trigger);
        }

        //if is variable name, it must be bracketed in %
        public static VarRef GetVarRefOrValue(XmlNode xmlNode, string name, VarValue.EType eType, Trigger trigger)
        {
            VarRef varRef = null;

            string text = Util.GetString(xmlNode, name);

            if (IsVarName(text))
            {
                string varName = GetVarName(text);
                varRef = new VarRef(varName, trigger);
            }
            else
            {
                VarValue varValue = GetVarValue(text, eType);
                varRef = new VarRef(varValue);
            }

            return varRef;
        }

        static bool IsVarName(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return false;
            }

            if (text.Length < 3)
            {
                return false;
            }

            if (!text.StartsWith("%"))
            {
                return false;
            }

            if (!text.EndsWith("%"))
            {
                return false;
            }

            return true;
        }

        static string GetVarName(string text)
        {
            return text.Substring(1, text.Length - 2);
        }

        static VarValue GetVarValue(string text, VarValue.EType eType)
        {
            switch (eType)
            {
                case VarValue.EType.Int:
                    return new VarValue(int.Parse(text));
                case VarValue.EType.Bool:
                    switch (text)
                    {
                        case "0": return false;
                        case "1": return true;
                    }
                    return false;
                //return new VarValue(bool.Parse(text));
                case VarValue.EType.Float:
                    return new VarValue(float.Parse(text));
                case VarValue.EType.Vector3:
                    return new VarValue(GetVector3(text));
                case VarValue.EType.String:
                    return new VarValue(text);
                case VarValue.EType.Var:
                    return new VarValue((object)text);
                default:
                    throw new System.Exception("error value type");
            }
        }

        public static Vector3 GetVector3(string text)
        {
            string[] tmp = text.Split(',');
            float x = float.Parse(tmp[0]);
            float y = float.Parse(tmp[1]);
            float z = float.Parse(tmp[2]);

            return new Vector3(x, y, z);
        }

        public static int GetInt(XmlNode xmlNode, string name)
        {
            return int.Parse(xmlNode.Attributes[name].Value);
        }

        //public static Vector3 GetVector3(XmlNode xmlNode, string name)
        //{
        //    string text = xmlNode.Attributes[name].Value;
        //    return GetVector3(text);
        //}

        //public static float GetFloat(XmlNode xmlNode, string name)
        //{
        //    return float.Parse(xmlNode.Attributes[name].Value);
        //}

        public static bool GetBool(XmlNode xmlNode, string name)
        {
            return bool.Parse(xmlNode.Attributes[name].Value);
        }

        public static string GetString(XmlNode xmlNode, string name)
        {
            return Unescape(xmlNode.Attributes[name].Value);
        }

        static VarValue TryParseVarValue(XmlNode xmlNode, string name)
        {
            string textValue = Util.Unescape(xmlNode.Attributes[name].Value);

            return TryParseVarValue(textValue);
        }

        public static VarValue TryParseVarValue(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return new VarValue();
            }

            int i;
            if (int.TryParse(text, out i))
            {
                return new VarValue(i);
            }

            bool b;
            if (bool.TryParse(text, out b))
            {
                return new VarValue(b);
            }

            float f;
            if (float.TryParse(text, out f))
            {
                return new VarValue(f);
            }

            Vector3 vector;
            if (Util.TryParseVector3(text, out vector))
            {
                return new VarValue(f);
            }

            //text
            return new VarValue(text);
        }

        static bool TryParseVector3(string text, out Vector3 vector)
        {
            vector = Vector3.zero;

            if (string.IsNullOrEmpty(text))
            {
                return false;
            }

            string[] tmp = text.Split(',');
            if (3 != tmp.Length)
            {
                return false;
            }

            float x = 0f;
            if (!float.TryParse(tmp[0], out x))
            {
                return false;
            }

            float y = 0f;
            if (!float.TryParse(tmp[1], out y))
            {
                return false;
            }

            float z = 0f;
            if (!float.TryParse(tmp[2], out z))
            {
                return false;
            }

            vector.Set(x, y, z);
            return true;
        }
    }

    public static class Debug
    {
        //List<object> mLogMsg = new List<object>(10);
        //public List<object> LogMsg
        //{
        //    get
        //    {

        //    }
        //}

        public static void Log(object msg)
        {
            UnityEngine.Debug.Log(msg);
        }

        public static void LogWarning(object msg)
        {
            UnityEngine.Debug.LogWarning(msg);
        }

        public static void LogError(object msg)
        {
            UnityEngine.Debug.LogError(msg);
        }
    }
}