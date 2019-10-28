using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ItemAsset;

//public class MissionNoticeGui_N : GUIWindowBase
//{
//	public MissionNoticeItem_N	mPrefab;
//	
//	MissionNoticeItem_N			mCurrentItem;
//	
//	MissionNoticeItem_N			mLastItem;
//	
//	public void AddNotice(int type, string name)
//	{
//		if(mLastItem != null)
//			mLastItem.Hide();
//		if(mCurrentItem != null)
//		{
//			mCurrentItem.Hide();
//			mLastItem = mCurrentItem;
//		}
//
//		mCurrentItem = Instantiate(mPrefab) as MissionNoticeItem_N;
//		mCurrentItem.transform.parent = this.transform;
//		mCurrentItem.transform.localPosition = Vector3.zero;
//		mCurrentItem.transform.localRotation = Quaternion.identity;
//		mCurrentItem.InitItem(type,name);
//	}
//}
