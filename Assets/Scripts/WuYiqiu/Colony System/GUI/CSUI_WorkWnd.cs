using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CSUI_WorkWnd : MonoBehaviour 
{
	[HideInInspector]
	public CSEntity m_Entity = null;
	public GameObject mWorkerItemPrefab;
	public UIGrid mWorkerGrid;
	

	List<CSUI_WorkItem> mUIWokerList;
	List<CSPersonnel> mPersonnelList;

	void Awake()
	{
		mUIWokerList = new List<CSUI_WorkItem>();
		mPersonnelList= new List<CSPersonnel>();
	}
	// Use this for initialization
	void Start () 
	{
		AddCSUI_WorkeItem(8);
	}

	void AddCSUI_WorkeItem(int count)
	{
		for(int i=0;i<count;i++)
		{
			GameObject obj = GameObject.Instantiate(mWorkerItemPrefab) as GameObject;
			obj.transform.parent = mWorkerGrid.transform;
			obj.transform.localPosition = Vector3.zero;
			obj.transform.localScale = Vector3.one;
			
			CSUI_WorkItem item = obj.GetComponent<CSUI_WorkItem>();
			mUIWokerList.Add(item);
			obj.SetActive(false);
		}
	}

	// Update is called once per frame
	void Update () 
	{
		if (m_Entity == null)
			return;
		if (ChangeEntityWorkers() )
			RefalshWorkersGrid();
	}



	int m_EntityType = -1;
	bool ChangeEntityWorkers()
	{
		if (m_EntityType != m_Entity.m_Type)
		{
			m_EntityType = m_Entity.m_Type;
			return true;
		}
		else
		{
			CSCommon com = m_Entity as CSCommon;
			if (com == null)
				return false;

			if (com.WorkerMaxCount != mPersonnelList.Count)
				return true;

			for (int i=0;i<com.WorkerMaxCount;i++)
			{
				if (com.Worker(i) != mPersonnelList[i])
					return true;
			}
		}
		return false;

	}

	void RefalshWorkersGrid()
	{
		CSCommon com = m_Entity as CSCommon;
        if (com == null)
        {
            return;
        }

		if (com.WorkerCount > mUIWokerList.Count)
			AddCSUI_WorkeItem(com.WorkerCount - mUIWokerList.Count);

		int j = 0;
		for (int i=0;i<com.WorkerMaxCount;i++)
		{
			if (com.Worker(i) != null)
			{
				mUIWokerList[j].SetWorker(com.Worker(i));
				mUIWokerList[j].gameObject.SetActive(true);
				j++;
			}
		}

		for (int i =j;i<mUIWokerList.Count;i++)
			mUIWokerList[i].gameObject.SetActive(false);

		mPersonnelList.Clear();
		for (int i=0;i<com.WorkerMaxCount;i++)
			mPersonnelList.Add(com.Worker(i));

		mWorkerGrid.repositionNow = true;
	}



}
