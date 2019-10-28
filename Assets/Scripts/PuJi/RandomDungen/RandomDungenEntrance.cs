using UnityEngine;
using System.Collections;

public class RandomDungenEntrance:MonoBehaviour
{ 
	public int level=1;
	public int dungeonId = 1;

	Vector3 pos{
		get{return transform.position;}
	}
	public bool isShow = false;
	MessageBox_N.Message ob;
//	public delegate void TriggerEvent(GameObject go);
//	public TriggerEvent onEnterTrigger;


	void OnTriggerEnter(Collider target){
		if((Pathea.PeGameMgr.IsSingleAdventure&&Pathea.PeGameMgr.yirdName==Pathea.AdventureScene.MainAdventure.ToString())
		   ||Pathea.PeGameMgr.IsMultiAdventure){
			Debug.Log("enter dungen"); 
			if (null == target.GetComponentInParent<Pathea.MainPlayerCmpt>())
				return;
			if (isShow == true)
				return;
//			
			isShow = true;
			if(level>=DungeonConstants.TASK_LEVEL_START)
				ob = MessageBox_N.ShowYNBox(PELocalization.GetString(DungenMessage.TASK_ENTER_DUNGEN),SceneTranslate,SetFalse);
			else
				ob = MessageBox_N.ShowYNBox(CSUtils.GetNoFormatString(PELocalization.GetString(DungenMessage.ENTER_DUNGEN), level.ToString()),SceneTranslate,SetFalse);
		}
	}
	void OnTriggerExit(Collider target){
		if(ob!=null)
			MessageBox_N.CancelMessage(ob);
		isShow = false;
	}
	public void SceneTranslate()
	{
		RandomDungenMgr.Instance.EnterDungen(transform.position,dungeonId);
		SetFalse();
//		if(onEnterTrigger!=null)
//			onEnterTrigger(gameObject);
	}
	public void SetFalse() { isShow = false; }

	public void SetLevel(int level){
		this.level = level;
	}
	public void SetDungeonId(int id){
		this.dungeonId = id;
	}

	void OnDestroy(){
		if(ob!=null)
			MessageBox_N.CancelMessage(ob);
	}
	void Start(){
	}
	void Awake(){
	}
}