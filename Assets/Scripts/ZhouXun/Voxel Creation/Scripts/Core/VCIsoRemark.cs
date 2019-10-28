using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

public class VCIsoRemark
{
	public VCIsoRemark ()
	{
		m_Attribute = new CreationAttr ();
		m_Error = "";
	}

	public CreationAttr m_Attribute;
	public string m_Error;

	public string xml
	{
		get
		{
			string str = "";
			str += "<REMARKS>\r\n";
			str += "\t<ATTR>\r\n";
			string common = string.Format("<COMMON type=\"{0}\" vol=\"{1}\" weight=\"{2}\" dur=\"{3}\" sp=\"{4}\">", 
			                              (int)(m_Attribute.m_Type), m_Attribute.m_Volume, m_Attribute.m_Weight,
			                              m_Attribute.m_Durability, m_Attribute.m_SellPrice);
			str += "\t\t" + common + "\r\n";
			foreach ( KeyValuePair<int, int> kvp in m_Attribute.m_Cost )
			{
				if ( kvp.Value > 0 )
				{
					string coststr = string.Format("\t\t\t<COST id=\"{0}\" cnt=\"{1}\" />\r\n", kvp.Key, kvp.Value);
					str += coststr;
				}
			}
			str += "\t\t</COMMON>\r\n";
			string prop = string.Format("\t\t<PROP atk=\"{0}\" def=\"{1}\" inc=\"{2}\" fs=\"{3}\" acc=\"{4}\" dc=\"{5}\" fuel=\"{6}\" />\r\n",
			                            m_Attribute.m_Attack, m_Attribute.m_Defense, m_Attribute.m_MuzzleAtkInc, m_Attribute.m_FireSpeed,
			                            m_Attribute.m_Accuracy, m_Attribute.m_DragCoef, m_Attribute.m_MaxFuel);
			str += prop;
			str += "\t</ATTR>\r\n";
			str += "</REMARKS>\r\n";
			return str;
		}
		set
		{
			m_Attribute = new CreationAttr ();
			try
			{
				XmlDocument xmldoc = new XmlDocument ();
				xmldoc.LoadXml(value);
				XmlNode attr_node = xmldoc.DocumentElement["ATTR"];
				XmlNode common_node = attr_node["COMMON"];
				XmlNode prop_node = attr_node["PROP"];
				m_Attribute.m_Type = (ECreation)(XmlConvert.ToInt32(common_node.Attributes["type"].Value));
				m_Attribute.m_Volume = XmlConvert.ToSingle(common_node.Attributes["vol"].Value);
				m_Attribute.m_Weight = XmlConvert.ToSingle(common_node.Attributes["weight"].Value);
				m_Attribute.m_Durability = XmlConvert.ToSingle(common_node.Attributes["dur"].Value);
				m_Attribute.m_SellPrice = XmlConvert.ToSingle(common_node.Attributes["sp"].Value);
				m_Attribute.m_Attack = XmlConvert.ToSingle(prop_node.Attributes["atk"].Value);
				m_Attribute.m_Defense = XmlConvert.ToSingle(prop_node.Attributes["def"].Value);
				m_Attribute.m_MuzzleAtkInc = XmlConvert.ToSingle(prop_node.Attributes["inc"].Value);
				m_Attribute.m_FireSpeed = XmlConvert.ToSingle(prop_node.Attributes["fs"].Value);
				m_Attribute.m_Accuracy = XmlConvert.ToSingle(prop_node.Attributes["acc"].Value);
				m_Attribute.m_DragCoef = XmlConvert.ToSingle(prop_node.Attributes["dc"].Value);
				m_Attribute.m_MaxFuel = XmlConvert.ToSingle(prop_node.Attributes["fuel"].Value);
				foreach ( XmlNode node in common_node.ChildNodes )
				{
					int id = XmlConvert.ToInt32(node.Attributes["id"].Value);
					int cnt = XmlConvert.ToInt32(node.Attributes["cnt"].Value);
					m_Attribute.m_Cost.Add(id, cnt);
				}
			}
			catch (Exception e)
			{
				m_Attribute = null;
				m_Error = "Reading remarks error" + e.ToString();
			}
		}
	}
}
