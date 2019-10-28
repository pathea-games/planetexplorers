#define UsingDoodadInsteadOfStaticAsset
using UnityEngine;
using Mono.Data.SqliteClient;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public enum SceneAssetType
{
	StaticAsset,
	OperatableItem,
}

public class SceneStaticAssetAgent : SceneSerializableObjAgent
{
	public SceneStaticAssetAgent(){}	// for saveload
	public SceneStaticAssetAgent(string pathPre, string pathMain, Vector3 pos, Quaternion rotation, Vector3 scale) 
		: base(pathPre, pathMain, pos, rotation, scale)
	{}

	public override void OnMainGoLoaded()
	{
		if(_go == null)
			return;
		_go.transform.parent = SceneAssetsMan.Instance.RootObj.transform;
	}
	public override bool NeedToActivate{	get{ 	return false;			}	}
}

public class SceneAssetDesc
{
	public int _id;
	public ISceneObjAgent _agent;
}

public class SceneAssetsMan : Pathea.MonoLikeSingleton<SceneAssetsMan>
{
	List<SceneAssetDesc> _assets = new List<SceneAssetDesc>();
	GameObject _rootObj = null;
	public GameObject RootObj
	{
		get
		{
			if(_rootObj == null)
			{
				_rootObj = new GameObject("SceneStaticObjs");
			}

			if(_rootObj.transform.parent == null && SceneMan.self != null)
			{
				_rootObj.transform.parent = SceneMan.self.transform;
			}
			return _rootObj;
		}
	}
	public List<SceneAssetDesc> Assets{ get{ return _assets; 	} }

	public void New()
	{
#if !UsingDoodadInsteadOfStaticAsset
		LoadStaticAssetsFromDB();
		Camp.AttachAssetWithCamp(_assets);
#endif
	}
	public void Restore()
	{
#if !UsingDoodadInsteadOfStaticAsset
		if (SceneMan.self != null) 
		{
			SceneMan.self.AddImportedObj(typeof(SceneStaticAssetAgent).Name);
		}
#endif
	}

	void LoadStaticAssetsFromDB()
	{
		List<ISceneObjAgent> objAgents = new List<ISceneObjAgent>();
		_assets.Clear();
		SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable("sceneAssetList");
		while (reader.Read())
		{
			int id = Convert.ToInt32(reader.GetString(0));
			int type = Convert.ToInt32(reader.GetString(1));
			string prePathName = reader.GetString(2);
			string mainPathName = reader.GetString(3);
			string[] strPos = reader.GetString(4).Split(',');
			string[] strRot = reader.GetString(5).Split(',');
			string[] strScl = reader.GetString(6).Split(',');
			Vector3 pos = new Vector3(
				Convert.ToSingle(strPos[0]),
				Convert.ToSingle(strPos[1]),
				Convert.ToSingle(strPos[2]));
			Quaternion rot = new Quaternion(
				Convert.ToSingle(strRot[0]),
				Convert.ToSingle(strRot[1]),
				Convert.ToSingle(strRot[2]),
				Convert.ToSingle(strRot[3]));
			if(rot.w > 2) //Quaternion should be normalized
			{
				rot.eulerAngles = new Vector3(rot.x, rot.y, rot.z);
			}
			Vector3 scl = new Vector3(
				Convert.ToSingle(strScl[0]),
				Convert.ToSingle(strScl[1]),
				Convert.ToSingle(strScl[2]));
			if(prePathName != null && prePathName.Length <= 1)
			{
				prePathName = null;
			}
			switch(type)
			{
			case (int)SceneAssetType.StaticAsset:
				{
				SceneAssetDesc asset = new SceneAssetDesc();
				asset._id = id;
				asset._agent = new SceneStaticAssetAgent(prePathName, mainPathName, pos, rot, scl);
				_assets.Add(asset);
				objAgents.Add(asset._agent);
				}
				break;
			case (int)SceneAssetType.OperatableItem:
				{
				// TODO : code for prePathName
				OperatableItemAgent agent = new OperatableItemAgent(id, pos, mainPathName);
				objAgents.Add(agent);
				}
				break;
			default:
				Debug.LogError("[SceneAssets]:Unrecognizable asset type:"+type);
				break;
			}
		}
		SceneMan.AddSceneObjs(objAgents);
	}

	public void Register(string pathMain, Vector3 position, Quaternion rotation, Vector3 scale, SceneAssetType type = SceneAssetType.StaticAsset)
	{
		/*
		SceneStaticAssetAgent objAgent = new SceneStaticAssetAgent(null, pathMain, position, rotation, scale);
		SceneMan.AddSceneObj(objAgent);
		*/
	}
}
