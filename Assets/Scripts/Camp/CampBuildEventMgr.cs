using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CampBuildEventMgr : MonoBehaviour 
{

	public GameObject[] objects;

	bool mInit = false;
	bool HasShow = false;
	TimeSlot[] Times;
	int index = -1;
	// Use this for initialization

	void Awake()
	{
		Init();
	}
	void Start () 
	{
	
	}

	void Init()
	{
	  TimeSlot Slot1 = new TimeSlot(12.0f,12.5f);
	  TimeSlot Slot2 = new TimeSlot(19.0f,19.5f);

		Times = new TimeSlot[2];
		Times[0] = Slot1;
		Times[1] = Slot2;
	    mInit = true;
	}

	void CalculateTimes()
	{

		for(int i=0;i<Times.Length;i++)
		{
			if(!HasShow && Times[i].InTheRange((float)GameTime.Timer.HourInDay))
			{
				objects[0].SetActive(true);
				index = i;
				HasShow = true;
			}
		}

		if(HasShow && index >=0 && !Times[index].InTheRange((float)GameTime.Timer.HourInDay))
			{
				objects[0].SetActive(false);
				HasShow = false;
			    index = -1;
			}

	}
	// Update is called once per frame
	void Update () 
	{
		if(!mInit)
			Init();

		if(mInit)
		{
			CalculateTimes();
		}
	}

//	void OnTriggerEnter()
//	{
//		objects[0].SetActive(true);
//	}
//
//
//	void OnTriggerExit() 
//	{
//		objects[0].SetActive(false);
//	}
//	void OnTriggerStay()
//	{
//
//	}
}
