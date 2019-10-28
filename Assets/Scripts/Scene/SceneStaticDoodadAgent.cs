using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

#if false
public class SceneStaticDoodadAgent : SceneSerializableObjAgent
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
		
		Mono.Data.SqliteClient.SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable("PrototypeDoodad");
		while (reader.Read())
		{
			PrototypeDesc rand = new PrototypeDesc();
			
			rand._pid = System.Convert.ToInt32(reader.GetString(reader.GetOrdinal("id")));
			rand._strAssetbundle = reader.GetString(reader.GetOrdinal("asset_path"));
			rand._strPrefab = null;
			
			_protoDescs.Add(rand);
		}
	}
	
	public SceneStaticDoodadAgent(){}	// for Activator
	private SceneStaticDoodadAgent(string pathPrefab, string pathAssetbundle, Vector3 pos, Quaternion rotation, Vector3 scale, int id = SceneMan.InvalidID)
	: base(pathPrefab, pathAssetbundle, pos, rotation, scale, id){}
	
	public static SceneStaticDoodadAgent Create(int protoId, Vector3 pos, Quaternion rotation, Vector3 scale, int id = SceneMan.InvalidID)
	{
		if (null == _protoDescs)
		{
			LoadData();
		}
		
		PrototypeDesc desc = _protoDescs.Find(it=>it._pid == protoId);
		if(desc != null)
		{
			SceneStaticDoodadAgent ret = new SceneStaticDoodadAgent(desc._strPrefab, desc._strAssetbundle, pos, rotation, scale, id);
			return ret;
		}
		return null;
	}

	public override bool IsPosFixed{ 		get{	return true;			}	}
	public override bool NeedToActivate{	get{ 	return false;	}	}
}
#endif
