using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class SceneStaticEffectAgent : SceneSerializableObjAgent
{
	private class PrototypeDesc
	{
		public int _pid;
		public string _strPrefab;
		public string _strAssetbundle;
	}

	static List<PrototypeDesc> _protoDescs = null;

	public static void LoadData()
	{
		_protoDescs = new List<PrototypeDesc>();
		
		Mono.Data.SqliteClient.SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable("PrototypeEffect");
		while (reader.Read())
		{
			PrototypeDesc rand = new PrototypeDesc();
			
			rand._pid = System.Convert.ToInt32(reader.GetString(reader.GetOrdinal("id")));
			rand._strAssetbundle = null;
			rand._strPrefab = reader.GetString(reader.GetOrdinal("prefab_path"));
			_protoDescs.Add(rand);
		}
	}

	public SceneStaticEffectAgent(){}	// for Activator
	private SceneStaticEffectAgent(string pathPrefab, string pathAssetbundle, Vector3 pos, Quaternion rotation, Vector3 scale, int id = SceneMan.InvalidID)
	: base(pathPrefab, pathAssetbundle, pos, rotation, scale, id){}

	public static SceneStaticEffectAgent Create(int protoId, Vector3 pos, Quaternion rotation, Vector3 scale, int id = SceneMan.InvalidID)
	{
		if (null == _protoDescs)
		{
			LoadData();
		}
		
		PrototypeDesc desc = _protoDescs.Find(it=>it._pid == protoId);
		if(desc != null)
		{
			SceneStaticEffectAgent ret = new SceneStaticEffectAgent(desc._strPrefab, desc._strAssetbundle, pos, rotation, scale, id);
			return ret;
		}
		return null;
	}

	public override bool NeedToActivate{	get{ 	return false;	}	}

}
