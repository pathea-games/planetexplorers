using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Xml;

namespace PETools
{
    public class XmlUtil
    {
        public static string GetAttributeString(XmlElement e, string name)
        {
            if (e != null && !string.IsNullOrEmpty(name) && e.HasAttribute(name))
            {
                return e.GetAttribute(name);
            }

            return "";
        }

        public static int GetAttributeInt32(XmlElement e, string name)
        {
            if (e != null && !string.IsNullOrEmpty(name) && e.HasAttribute(name))
            {
                try
                {
                    return Convert.ToInt32(e.GetAttribute(name));
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning(ex);
                }
            }

            return 0;
        }

        public static bool GetAttributeBool(XmlElement e, string name)
        {
            if (e != null && !string.IsNullOrEmpty(name) && e.HasAttribute(name))
            {
                try
                {
                    return Convert.ToBoolean(e.GetAttribute(name));
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning(ex);
                }
            }

            return false;
        }

        public static float GetAttributeFloat(XmlElement e, string name)
        {
            if (e != null && !string.IsNullOrEmpty(name) && e.HasAttribute(name))
            {
                try
                {
                    return Convert.ToSingle(e.GetAttribute(name));
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning(ex);
                }
            }

            return 0.0f;
        }

        public static Color32 GetAttributeColor(XmlElement e, string name)
        {
            if (e != null && !string.IsNullOrEmpty(name) && e.HasAttribute(name))
            {
                try
                {
                    return PEUtil.ToColor32(e.GetAttribute(name), ',');
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning(ex);
                }
            }

            return new Color32(0,0,0,0);
        }

        public static string GetNodeString(XmlElement e, string name)
        {
            if (e != null && !string.IsNullOrEmpty(name))
            {
                XmlNode child = e.SelectSingleNode(name) as XmlNode;
                if (child != null)
                {
                    return child.InnerText;
                }
            }

            return "";
        }

        public static int GetNodeInt32(XmlElement e, string name)
        {
            if (e != null && !string.IsNullOrEmpty(name))
            {
                XmlNode child = e.SelectSingleNode(name) as XmlNode;
                if (child != null)
                {
                    try
                    {
                        return Convert.ToInt32(child.InnerText);
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogWarning(ex);
                    }
                }
            }

            return 0;
        }

        public static float GetNodeFloat(XmlElement e, string name)
        {
            if (e != null && !string.IsNullOrEmpty(name))
            {
                XmlNode child = e.SelectSingleNode(name) as XmlNode;
                if (child != null)
                {
                    try
                    {
                        return Convert.ToSingle(child.InnerText);
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogWarning(ex);
                    }
                }
            }

            return 0.0f;
        }

        public static bool GetNodeBool(XmlElement e, string name)
        {
            if (e != null && !string.IsNullOrEmpty(name))
            {
                XmlNode child = e.SelectSingleNode(name) as XmlNode;
                if (child != null)
                {
                    try
                    {
                        return Convert.ToBoolean(child.InnerText);
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogWarning(ex);
                    }
                }
            }

            return false;
        }

        public static Color32 GetNodeColor32(XmlElement e, string name)
        {
            if (e != null && !string.IsNullOrEmpty(name))
            {
                XmlNode child = e.SelectSingleNode(name) as XmlNode;
                if (child != null)
                {
                    try
                    {
                        return PEUtil.ToColor32(child.InnerText, ',');
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogWarning(ex);
                    }
                }
            }

            return new Color32(0,0,0,0);
        }

        public static List<string> GetNodeStringList(XmlElement e, string tag)
        {
            List<string> tmpList = new List<string>();

            if (e != null && !string.IsNullOrEmpty(tag))
            {
                XmlNodeList list = e.GetElementsByTagName(tag);
                foreach (XmlNode node in list)
                {
                    tmpList.Add(node.InnerText);
                }
            }

            return tmpList;
        }

        public static List<int> GetNodeInt32List(XmlElement e, string tag)
        {
            List<int> tmpList = new List<int>();

            if (e != null && !string.IsNullOrEmpty(tag))
            {
                XmlNodeList list = e.GetElementsByTagName(tag);
                foreach (XmlNode node in list)
                {
                    try
                    {
                        tmpList.Add(Convert.ToInt32(node.InnerText));
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning(ex);
                    }
                }
            }

            return tmpList;
        }

        public static List<float> GetNodeFloatList(XmlElement e, string tag)
        {
            List<float> tmpList = new List<float>();

            if (e != null && !string.IsNullOrEmpty(tag))
            {
                XmlNodeList list = e.GetElementsByTagName(tag);
                foreach (XmlNode node in list)
                {
                    try
                    {
                        tmpList.Add(Convert.ToSingle(node.InnerText));
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning(ex);
                    }
                }
            }

            return tmpList;
        }
    }
}
