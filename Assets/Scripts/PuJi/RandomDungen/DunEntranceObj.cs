//------------------------------------------------------------------------------
// by Pugee
//------------------------------------------------------------------------------
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Pathea;
using System.IO;


public class DunEntranceObj:ISceneObjAgent
{
	int id;
	int dungeonId;
	int level;
	string path;
	UnityEngine.Object prefab;
	GameObject gameObj;
	Vector3 position;
	Quaternion rotation;
	bool needToActivate=false;
	bool tstYOnActivate=false;
	bool isShowOnMap = false;
	bool showEnterOrNot = false;
	public bool ShowEnterOrNot{
		set{showEnterOrNot = value;
			if(gameObj!=null)
				gameObj.GetComponent<RandomDungenEntrance>().isShow = value;}
	}
	public DunEntranceObj(UnityEngine.Object entrancePrefab, Vector3 pos){
		prefab = entrancePrefab;
		position = pos;
		if(PeMap.MaskTile.Mgr.Instance.GetIsKnowByPos(pos)){
			new DungeonEntranceLabel(pos);
			isShowOnMap = true;
		}
	}


	public int Id
	{
		get
		{
			return id;
		}
		set
		{
			id = value ;
		}
	}

	public int Level{
		get{return level;}
		set{level=value;}
	}

	public int DungeonId{
		get{return dungeonId;}
		set{dungeonId = value;}
	}

	public int ScenarioId { get; set; }
	public GameObject Go    	{	get { return gameObj; 			}  	}
	public Vector3 Pos	    	{	get { return position; 			}  	}
	public IBoundInScene Bound	{	get	{ return null;				}	}
	public bool NeedToActivate	{	get { return needToActivate ; 	}	}
	public bool TstYOnActivate 	{	get { return tstYOnActivate; 	}  	}
	public void OnActivate()
	{
		Rigidbody rb = gameObj.GetComponent<Rigidbody>();
		if (rb != null)
			rb.useGravity = true;
	}
	
	public void OnDeactivate()
	{
		Rigidbody rb = gameObj.GetComponent<Rigidbody>();
		if (rb != null)
			rb.useGravity = false;
	}
	
	public void OnConstruct()
	{
		if (null != prefab)
		{
			gameObj = Object.Instantiate(prefab) as GameObject;
//			if(RandomDungenMgr.Instance!=null&&RandomDungenMgr.Instance.gameObject!=null)
//				gameObj.transform.SetParent(RandomDungenMgr.Instance.gameObject.transform);
			gameObj.transform.position = position;
			gameObj.transform.rotation = rotation;
			RandomDungenEntrance rde = gameObj.GetComponent<RandomDungenEntrance>();
			rde.isShow = showEnterOrNot;
			rde.SetLevel(level);
			rde.SetDungeonId(dungeonId);
			Rigidbody rb = gameObj.GetComponent<Rigidbody>();
			if (rb != null)
				rb.useGravity = false;
			if(!isShowOnMap){
				new DungeonEntranceLabel(position);
				isShowOnMap = true;
			}
		}
		else
		{
			Debug.LogError("entrance prefab not found");
		}
	}
	
	public void OnDestruct()
	{
		if (gameObj != null)
		{
			position = gameObj.transform.position;
			rotation = gameObj.transform.rotation;
			GameObject.Destroy(gameObj);
		}
	}

	public void DestroySelf(){
		if (gameObj != null)
			GameObject.Destroy(gameObj);
		SceneMan.RemoveSceneObj(this);
		DungeonEntranceLabel.Remove(position);
	}
}

