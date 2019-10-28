using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PEBuilding : MonoBehaviour 
{

	public Transform[] mTrans;
	public GameObject[] mObjs;

	public Transform[] mDoorPos;

	BuildingMap m_Buildingmap;
	public CampFoodTimer mFoodTimer;
	// Use this for initialization
	void Start ()
	{
		m_Buildingmap = new BuildingMap();
		m_Buildingmap.LoadIn(mTrans);
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public Transform Occupy(int entityId)
	{
		if(m_Buildingmap == null)
		{
			m_Buildingmap = new BuildingMap();
			m_Buildingmap.LoadIn(mTrans);
		}
		return  m_Buildingmap.OccupyTran(entityId);
	}

	public void Release(int entityId)
	{
		m_Buildingmap.ReleaseTran(entityId);
	}

	public bool SetFoodShowSlots(List<Pathea.CheckSlot> slots)
	{
		if(mFoodTimer == null)
			return false;

		mFoodTimer.SetSlots(slots);
		return true;
	}
}

public class BuildingInfo
{
	public Transform tran;
    public int occupyId;
	public bool IsOccupy;

	public BuildingInfo(int Id,Transform _tran)
	{
		tran = _tran;
		occupyId = Id;
		IsOccupy = false;
	}
}

public class BuildingMap
{
	public  List<BuildingInfo> s_Data;

	public BuildingMap()
	{
		s_Data = new List<BuildingInfo>();
	}

	public  Transform OccupyTran(int entityId)
	{
		for(int i=0;i<s_Data.Count;i++)
		{
			if(s_Data[i].IsOccupy)
			{
				if(s_Data[i].occupyId == entityId)
					return s_Data[i].tran;
			}
		}

		List<int> infos = GetEmptyTran();
		if(infos.Count >0)
		{
			int index = UnityEngine.Random.Range(0,infos.Count);
			index = infos[index];
			s_Data[index].occupyId = entityId;
			s_Data[index].IsOccupy = true;
			return s_Data[index].tran;

//			for(int i=0;i<s_Data.Count;i++)
//			{
//				if(!s_Data[i].IsOccupy)
//				{
//					s_Data[i].occupyId = entityId;
//					s_Data[i].IsOccupy = true;
//					return s_Data[i].tran;
//				}
//			}
		}


		return null;
	}

	List<int> GetEmptyTran()
	{
		List<int>  lists = new List<int>();
		for(int i=0;i<s_Data.Count;i++)
		{
			if(!s_Data[i].IsOccupy)
				lists.Add(i);
		}
		return lists;
	}

	public  void ReleaseTran(int entityId)
	{
		for(int i=0;i<s_Data.Count;i++)
		{
			if(s_Data[i].IsOccupy && s_Data[i].occupyId == entityId)
			{
				s_Data[i].IsOccupy = false;
				s_Data[i].occupyId = 0;
			}
		}
	}

	public  void LoadIn(Transform[] trans)
	{
		for(int i=0;i<trans.Length;i++)
		{
			s_Data.Add(new BuildingInfo(0,trans[i]));
		}

	}
}
