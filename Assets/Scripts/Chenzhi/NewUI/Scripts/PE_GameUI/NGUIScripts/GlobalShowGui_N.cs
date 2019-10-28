using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ItemAsset;

public class GlobalShowGui_N : MonoBehaviour
{
	static GlobalShowGui_N mInstance;
	public static GlobalShowGui_N Instance { get { return mInstance; } }
	public GlobalShowItem_N mPrefab;
	public UILabel			mReviveTimeLabal;
	
	float mReviveTime = 0;
	
	List<ItemSample> 	mShowList = new List<ItemSample>();
	
	List<string>	mShowStrList = new List<string>();
	
	const float WeightTime = 0.5f;
	
	float mWeightTime = 0.5f;
	
	void Awake()
	{
		mInstance = this;
	}
	
	public static void ShowString(string showString)
	{
		if(null != mInstance)
			mInstance.AddShow(showString);
	}
	
	public void AddShow(ItemSample itemGrid)
	{
        if (itemGrid.protoId < CreationData.ObjectStartID)
			mShowList.Add(itemGrid);
	}
	
	public void AddShow(string des)
	{
		mShowStrList.Add(des);
	}
	
	public void SetReviveTime(float reviveTime)
	{
		mReviveTime = reviveTime;
		if(mReviveTime > 0)
			mReviveTimeLabal.enabled = true;
	}
	
	void Update()
	{
		if(mWeightTime > 0)
		{
			mWeightTime -= Time.deltaTime;
		}
		else
		{
			if(mShowStrList.Count > 0)
			{
				GlobalShowItem_N AddItem = Instantiate(mPrefab) as GlobalShowItem_N;
				AddItem.transform.parent = transform;
				AddItem.transform.localPosition = Vector3.zero;
				AddItem.transform.localRotation = Quaternion.identity;
				AddItem.transform.localScale = Vector3.one;
				AddItem.InitItem(mShowStrList[0]);
				mShowStrList.RemoveAt(0);
				mWeightTime = WeightTime;
			}
			else if(mShowList.Count > 0)
			{
				GlobalShowItem_N AddItem = Instantiate(mPrefab) as GlobalShowItem_N;
				AddItem.transform.parent = transform;
				AddItem.transform.localPosition = Vector3.zero;
				AddItem.transform.localRotation = Quaternion.identity;
				AddItem.transform.localScale = Vector3.one;
				AddItem.InitItem(mShowList[0]);
				mShowList.RemoveAt(0);
				mWeightTime = WeightTime;
			}
		}
		
		if(mReviveTime > 0)
		{
			mReviveTime -= Time.deltaTime;
			if(mReviveTime<=0)
			{
				mReviveTime = 0;
				mReviveTimeLabal.enabled = false;
			}
			mReviveTimeLabal.text = "ReviveTime:" + (int)mReviveTime;
		}
	}
}
