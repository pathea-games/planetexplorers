using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public interface ISaveDataInScene
{
	void ImportData(byte[] data);
	byte[] ExportData();
}

public interface ISerializable
{
	void Serialize(BinaryWriter bw);
	void Deserialize(BinaryReader br);
}
// For those very far scenobjs if there are a lot, i suppose to treat a group of them as eight sceneObj
// or set 26+1(center) sceneobjs
public interface ISceneObjAgent
{
	int Id{				get;set;}
    int ScenarioId { 	get;set;}	// for multiplay pos sync(one server for multi scenario)
	GameObject Go{		get;	}
	Vector3 Pos{ 		get;	}
	IBoundInScene Bound{get; 	}
	bool NeedToActivate{get; 	}
	bool TstYOnActivate{get;	}
	void OnConstruct();
	void OnActivate();
	void OnDeactivate();
	void OnDestruct();
}

public class SceneBasicObjAgent : ISceneObjAgent
{
	protected int _id;
	protected Vector3 _pos = Vector3.zero;
	protected Vector3 _scl = Vector3.one;
	protected Quaternion _rot = Quaternion.identity;
	protected string _pathPreAsset = "";
	protected string _pathMainAsset = "";
	protected GameObject _go = null;
	protected GameObject _mainGo = null;
	protected bool _bMainAssetLoading = false;
	public bool IsMainAssetLoading{ get{ return _bMainAssetLoading; } }
	
	public SceneBasicObjAgent(){}
	public SceneBasicObjAgent(string pathPreAsset, string pathMainAsset, Vector3 pos, Quaternion rotation, Vector3 scale, int id = SceneMan.InvalidID)
	{
		_id = id;
		_pos = pos;
		_scl = scale;
		_rot = rotation;
		if(pathPreAsset != null)
		{
			_pathPreAsset = pathPreAsset;
			TryLoadPreGo();
		}
		if(pathMainAsset != null)	_pathMainAsset = pathMainAsset;
	}	

	#region ISceneObjAgent
	public virtual int 		Id{				get{	return _id;		}
											set{	_id = value;	}	}
    public int ScenarioId { get; set; }
    public GameObject 		Go{				get{ 	return _go;		}	} 
	public virtual Vector3 	Pos{ 			get{ 	return _pos;	}	}
	public virtual IBoundInScene Bound{		get{ 	return null;	}	}
	public virtual bool 	NeedToActivate{	get{ 	return true;	}	}
	public virtual bool 	TstYOnActivate{	get{ 	return true;	}	}
	public virtual void OnConstruct()
	{
		TryLoadMainGo();
	}
	public virtual void OnDestruct()
	{
		if(_mainGo == null)		return;	
		OnMainGoDestroy ();
		GameObject.Destroy(_mainGo);
		_mainGo = null;
	}
	public virtual void OnActivate(){}
	public virtual void OnDeactivate(){}
	#endregion
	public virtual void OnPreGoLoaded()
	{
		if(SceneMan.self != null)						_go.transform.parent = SceneMan.self.transform;
	}
	public virtual void OnMainGoLoaded()
	{
		if(_mainGo == _go && SceneMan.self != null)		_go.transform.parent = SceneMan.self.transform;
	}
	public virtual void OnMainGoDestroy()
	{
	}

	// 
	protected void TryLoadPreGo()
	{
		if(_pathPreAsset.Length > 0)
		{
			_go = GameObject.Instantiate(Resources.Load(_pathPreAsset), _pos, _rot) as GameObject;
			_go.transform.localScale = _scl;
			OnPreGoLoaded();
		}
	}
	protected void TryLoadMainGo()
	{
		if(_pathMainAsset.Length > 0)
		{
			_bMainAssetLoading = true;
			if(_pathMainAsset.Contains(".unity3d"))
			{
				AssetPRS pos = new AssetPRS(_pos, _rot, _scl);
				AssetReq req = new AssetReq(_pathMainAsset, pos);
				AssetsLoader.Instance.AddReq(req);
				req.ReqFinishHandler += OnAssetLoaded;
			}
			else
			{
				GameObject go = GameObject.Instantiate(Resources.Load(_pathMainAsset), _pos, _rot) as GameObject;
				go.transform.localScale = _scl;
				OnAssetLoaded(go);
			}
		}
	}
	protected void OnAssetLoaded(GameObject go)
	{
		_bMainAssetLoading = false;
		_mainGo = go;
		if(_mainGo == null)		return;	
		
		if(_go != null)
		{
			_mainGo.transform.parent = _go.transform;
		}
		else
		{
			_go = _mainGo;
		}		
		OnMainGoLoaded();
	}
}
