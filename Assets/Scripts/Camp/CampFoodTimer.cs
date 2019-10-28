using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CampFoodTimer : MonoBehaviour
{
	List<Pathea.CheckSlot> timeSlots;
	bool HasShow = false;
	public GameObject foodObj;
	void Awake()
	{
	}
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	int index;
	void Update () 
	{
		if(foodObj == null || timeSlots == null)
			return ;

		if(timeSlots.Count >0)
		{
			for(int i=0;i<timeSlots.Count;i++)
			{
				if(!HasShow && timeSlots[i].InSlot((float)GameTime.Timer.HourInDay))
				{
					foodObj.SetActive(true);
					index = i;
					HasShow = true;
				}
			}
			
			if(HasShow && index >=0 && !timeSlots[index].InSlot((float)GameTime.Timer.HourInDay))
			{
				foodObj.SetActive(false);
				HasShow = false;
				index = -1;
			}
		}
	}

	public void SetSlots(List<Pathea.CheckSlot> slots)
	{
		if(timeSlots == null)
			timeSlots = new List<Pathea.CheckSlot>();

		timeSlots.Clear();
		timeSlots.AddRange(slots);
	}
}
