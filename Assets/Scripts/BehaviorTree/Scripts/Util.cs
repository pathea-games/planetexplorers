using UnityEngine;
using System;
using System.Xml;
using System.Collections;
using System.Collections.Generic;

namespace Behave.Runtime
{
    public class StringUtil
    {
        public static string[] ToArrayString(string str, char c)
        {
            return str.Split(new char[] { c });
        }

        public static int[] ToArrayInt32(string str, char c)
        {
            string[] strs = ToArrayString(str, c);
            List<int> tmpList = new List<int>();
            foreach (string s in strs)
            {
                tmpList.Add(System.Convert.ToInt32(s));
            }

            return tmpList.ToArray();
        }

        public static byte[] ToArrayByte(string str, char c)
        {
            string[] strs = ToArrayString(str, c);
            List<byte> tmpList = new List<byte>();
            foreach (string s in strs)
            {
                tmpList.Add(System.Convert.ToByte(s));
            }

            return tmpList.ToArray();
        }

        public static float[] ToArraySingle(string str, char c)
        {
            string[] strs = ToArrayString(str, c);
            List<float> tmpList = new List<float>();
            foreach (string s in strs)
            {
                tmpList.Add(System.Convert.ToSingle(s));
            }

            return tmpList.ToArray();
        }

        public static Vector3 ToVector3(string str, char c)
        {
            string[] strs = ToArrayString(str, c);

            if (strs.Length != 3)
                return Vector3.zero;

            List<float> tmpList = new List<float>();
            foreach (string s in strs)
            {
                tmpList.Add(System.Convert.ToSingle(s));
            }

            return new Vector3(tmpList[0], tmpList[1], tmpList[2]);
        }

        public static Vector3[] ToArrayVector3(string str, char s, char c)
        {
            string[] strs = ToArrayString(str, s);

            List<Vector3> tmpList = new List<Vector3>();
            foreach (string item in strs)
            {
                tmpList.Add(ToVector3(item, c));
            }

            return tmpList.ToArray();
        }

        public static Color32 ToColor32(string data, char c)
        {
            byte[] datas = ToArrayByte(data, c);
            if (datas.Length != 4)
                return new Color32(0, 0, 0, 0);
            else
                return new Color32(datas[0], datas[1], datas[2], datas[3]);
        }
    }

    public class XmlUtil
    {
        public static string GetAttributeString(XmlElement e, string name)
        {
            return e.GetAttribute(name);
        }

        public static string[] GetAttributeStringArray(XmlElement e, string name)
        {
            return StringUtil.ToArrayString(e.GetAttribute(name), ',');
        }

        public static int GetAttributeInt32(XmlElement e, string name)
        {
            return Convert.ToInt32(e.GetAttribute(name));
        }

        public static int[] GetAttributeInt32Array(XmlElement e, string name)
        {
            return StringUtil.ToArrayInt32(e.GetAttribute(name), ',');
        }

        public static bool GetAttributeBool(XmlElement e, string name)
        {
            return Convert.ToBoolean(e.GetAttribute(name));
        }

        public static float GetAttributeFloat(XmlElement e, string name)
        {
            return Convert.ToSingle(e.GetAttribute(name));
        }

        public static float[] GetAttributeFloatArray(XmlElement e, string name)
        {
            return StringUtil.ToArraySingle(e.GetAttribute(name), ',');
        }

        public static Color32 GetAttributeColor(XmlElement e, string name)
        {
            return StringUtil.ToColor32(e.GetAttribute(name), ',');
        }

        public static Vector3 GetAttributeVector3(XmlElement e, string name)
        {
            return StringUtil.ToVector3(e.GetAttribute(name), ',');
        }

        public static Vector3[] GetAttributeVector3Array(XmlElement e, string name)
        {
            return StringUtil.ToArrayVector3(e.GetAttribute(name), '|', ',');
        }
    }
}
