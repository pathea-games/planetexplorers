using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Xml;

public static class VEObjectAsm
{
    static VEObjectAsm()
	{
		Assembly asm = Assembly.GetAssembly(VEObjectBaseType);
		Type[] alltypes = asm.GetTypes();
		VEObjectTypes = new List<Type> ();
		foreach (Type t in alltypes)
		{
			if (t.IsSubclassOf(VEObjectBaseType))
			{
				if (!t.IsAbstract)
					VEObjectTypes.Add(t);
			}
		}

		TypeDict = new Dictionary<string, XMLMemberCollection> ();
		foreach (Type t in VEObjectTypes)
		{
			XMLMemberCollection mc = new XMLMemberCollection ();
			mc.Name = t.Name.ToUpper();
			TypeDict[t.FullName] = mc;
			object[] xmlobj_attrs = t.GetCustomAttributes(typeof(XMLObjectAttribute), false);
			if (xmlobj_attrs.Length > 0)
				mc.Name = (xmlobj_attrs[0] as XMLObjectAttribute).Name.ToUpper();
			MemberInfo[] mems = t.GetMembers();
			foreach (MemberInfo mem in mems)
			{
				FieldInfo field = mem as FieldInfo;
				PropertyInfo property = mem as PropertyInfo;
				
				if (field != null || property != null)
				{
					object[] xml_attrs = mem.GetCustomAttributes(typeof(XMLIOAttribute), true);
					XMLIOAttribute xmlAttr = (xml_attrs.Length > 0) ? (xml_attrs[0] as XMLIOAttribute) : (null);

					if (xmlAttr != null)
					{
						XMLMemberDesc xmlDesc = new XMLMemberDesc ();
						xmlDesc.Name = mem.Name;
						xmlDesc.Type = (field != null) ? field.FieldType : property.PropertyType;
						xmlDesc.Field = field;
						xmlDesc.Property = property;
						xmlDesc.Order = xmlAttr.Order;
						xmlDesc.Attr = xmlAttr.Attr;
						xmlDesc.Necessary = xmlAttr.Necessary;
						xmlDesc.DefaultValue = xmlAttr.DefaultValue;
						mc.Members.Add(xmlDesc);
					}
				}
			}
			mc.Members.Sort(XMLMemberDesc.Compare);
		}
	}

	public static string ToXML (VEObject obj)
	{
		string typename = obj.GetType().FullName;
		if (!TypeDict.ContainsKey(typename))
		{
			Debug.LogError("Object [" + obj.ID + "] ToXML failed: Unknown Type");
			return "";
		}
		XMLMemberCollection mc = TypeDict[typename];
		string xml = "<" + mc.Name + " ";
		foreach (XMLMemberDesc mem in mc.Members)
			xml += XMLIO.WriteValue(mem.Attr, mem.GetValue(obj), mem.Type, mem.Necessary, mem.DefaultValue);
		xml += "/>\r\n";
		return xml;
	}

	public static void Parse (VEObject obj, XmlElement xml)
	{
		string typename = obj.GetType().FullName;
		if (!TypeDict.ContainsKey(typename))
			throw new Exception("Object [" + obj.ID + "] ToXML failed: Unknown Type");
		XMLMemberCollection mc = TypeDict[typename];
		foreach (XMLMemberDesc mem in mc.Members)
		{
			object value = XMLIO.ReadValue(xml, mem.Attr, mem.Type, mem.Necessary, mem.DefaultValue);
			if (value != null)
				mem.SetValue(obj, value);
		}
	}

	public static Type VEObjectBaseType = typeof(VEObject);
	public static List<Type> VEObjectTypes = null;

	public class XMLMemberDesc
	{
		public string Name;
		public Type Type;
		public FieldInfo Field;
		public PropertyInfo Property;
		public int Order;
		public string Attr;
		public bool Necessary;
		public object DefaultValue;

		public static int Compare (XMLMemberDesc lhs, XMLMemberDesc rhs)
		{
			return lhs.Order - rhs.Order;
		}

		public object GetValue (object obj)
		{
			if (Field != null)
				return Field.GetValue(obj);
			if (Property != null)
				return Property.GetValue(obj, null);
			return null;
		}

		public void SetValue (object obj, object val)
		{
			if (Field != null)
				Field.SetValue(obj, val);
			else if (Property != null)
				Property.SetValue(obj, val, null);
		}
	}
	public class XMLMemberCollection
	{
		public string Name = "";
		public List<XMLMemberDesc> Members = new List<XMLMemberDesc> ();
	}
	public static Dictionary<string, XMLMemberCollection> TypeDict = null;
}
