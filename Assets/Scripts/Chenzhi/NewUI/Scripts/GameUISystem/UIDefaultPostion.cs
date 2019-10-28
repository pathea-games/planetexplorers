using UnityEngine;
using System.Collections;

public class UIDefaultPostion : MonoBehaviour
{
	static UIDefaultPostion 			mInstance;
	public static UIDefaultPostion 		Instance{ get{ return mInstance; } }

	public Vector3 						pos_PlayerInfo = Vector3.zero;
	public Vector3 						pos_ItemPackge = Vector3.zero;
	public Vector3 						pos_NpcStorage = Vector3.zero;
	public Vector3 						pos_Compound = Vector3.zero;
	public Vector3 						pos_Servant = Vector3.zero;
	public Vector3 						pos_Npc = Vector3.zero;
	public Vector3 						pos_Mission = Vector3.zero;
	public Vector3 						pos_MissionTruck = Vector3.zero;
	public Vector3 						pos_ItemGet = Vector3.zero;
	public Vector3 						pos_ItemOp = Vector3.zero;
	public Vector3 						pos_Shop = Vector3.zero;
	public Vector3 						pos_ItemBox = Vector3.zero;
	public Vector3 						pos_Repair = Vector3.zero;
	public Vector3 						pos_PowerPlant = Vector3.zero;
	public Vector3 						pos_Revive = Vector3.zero;
	public Vector3 						pos_WareHouse = Vector3.zero;
	public Vector3 						pos_Colony = Vector3.zero;
	public Vector3 						pos_Phone = Vector3.zero;
	public Vector3 						pos_Skill = Vector3.zero;
	public Vector3 						pos_WorkShop = Vector3.zero; 

	void Awake()
	{
		mInstance = this;
	}
}
