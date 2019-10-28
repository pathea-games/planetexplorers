using UnityEngine;
using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

// Now not used
public class AssetBundleDesc
{
	[XmlAttribute("PathName")]
	public string pathName
	{ get; set; }
	
	[XmlAttribute("CampName")]
	public string campName
	{ get; set; }
	
	[XmlElement("Pos")]
	public AssetPRS[] pos;
}

public class AssetBundleXml
{
	public bool isItem;
	public bool isTower;
	public bool isGroup;
	public bool isNative;
	public bool isMonster;
	
	static AssetBundleXml _instance;
	public static AssetBundleXml instance
	{
		get
		{
			if(_instance == null)
			{
				SerializeAssetBundleXml(Application.dataPath + "/AssetBundles.xml");
			}
			
			return _instance;
		}
	}
	
	public bool IsEnable(string path)
	{
		string directory = Path.GetDirectoryName(path);
		if(directory.Contains("Monster"))
			return isMonster;
		else if(directory.Contains("Native"))
			return isNative;
		else if(directory.Contains("Group"))
			return isGroup;
		else if(directory.Contains("Tower"))
			return isTower;
		else if(directory.Contains("Item"))
			return isItem;
		
		return true;
	}
	
	static void SerializeAssetBundleXml(string fileName)
	{
		//string fileName = Application.dataPath + "/Resources/UpdateConfig.xml";
		if (File.Exists(fileName))
		{
			using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
			{
				if (null != fs)
				{
					XmlSerializer serialize = new XmlSerializer(typeof(AssetBundleXml));
					_instance = serialize.Deserialize(fs) as AssetBundleXml;
				}
				else
				{
					Debug.LogError("Do not have any correct AssetBundles file!!");
				}
			}
		}
		else
		{
			using (FileStream fs = new FileStream(fileName, FileMode.Create, FileAccess.Write))
			{
				if (fs != null)
				{
					_instance = new AssetBundleXml();
					_instance.isItem = true;
					_instance.isTower = true;
					_instance.isGroup = true;
					_instance.isNative = true;
					_instance.isMonster = true;
					
					XmlSerializer serialize = new XmlSerializer(typeof(AssetBundleXml));
					serialize.Serialize(fs, _instance);
				}
				else
				{
					Debug.LogError("Do not have any correct Config file!!");
				}
			}
		}
	}
}

