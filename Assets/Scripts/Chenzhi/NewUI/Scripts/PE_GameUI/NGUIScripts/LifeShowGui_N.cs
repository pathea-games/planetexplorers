using UnityEngine;
using System.Collections;
using System.Collections.Generic;


//public class LifeShowGui_N : GUIWindowBase
//{
//	public LifeShowItem_N	mPrefab;
//	
//	public UIGrid			mGrid;
//	
//	List<LifeShowItem_N>	mAiObjList = new List<LifeShowItem_N>();
//	
//	public GameObject mCountWnd;
//	
//	public 	UILabel	mCountTimeLabel;
//	public 	UILabel	mMonsterNumLabel;
//	public 	UILabel	mMonsterLeftNumLabel;
//	
//	public UIButton mStartBtn;
//	public UILabel	mStartTex;
//	
//	public GameObject 	mPujaBaseWnd;
//	public UISlider		mPujaBaseHP;
//	public UISlider		mPujaBaseMsg;
//    //puja_barrack		mPujaBarrack;
//	
//	float 	mCountTime;
//	
////	int mMosterNum = 0;
////	int mMosterNumMax = 0;
//
//    ITowerDefenceData mTD;
//	
//	void Update()
//	{
//		if(mMonsterLeftNumLabel.enabled)
//			mMonsterLeftNumLabel.enabled = false;
//		// Delete autoremove ones
//		for(int i = mAiObjList.Count-1;i>=0;i--)
//		{
//			if(mAiObjList[i] == null)
//			{
//				mAiObjList.RemoveAt(i);
//				mGrid.Reposition();
//			}
//		}
//
//        if (mCountWnd.activeSelf && null != mTD)
//		{
//			float time = mTD.DelayTime;
//			if(time < 0)
//				time = 0;
//			int min = (int)time/60;
//			int sec = (int)time%60;
//			if(time > 0)
//			{
//				if(!mCountTimeLabel.enabled)
//					mCountTimeLabel.enabled = true;
//
//                string label = mTD.Begin ? "NextTime: " : "PrepTime: ";
//                label += min;
//                label += ":";
//                label += sec < 10 ? "0" : "";
//                label += sec;
//                mCountTimeLabel.text = label;
//			}
//			else if(mCountTimeLabel.enabled)
//			{
//				mCountTimeLabel.enabled = false;
//			}
//
//            mMonsterNumLabel.text = "KillNum: " + mTD.KilledCount + "/" + mTD.TotalCount;
//
//
//		}
//        //if(null != mPujaBarrack)
//        //{
//        //    if(!mPujaBaseWnd.activeSelf)
//        //        mPujaBaseWnd.SetActive(true);
//				
//        //    mPujaBaseHP.sliderValue = mPujaBarrack.lifePercent;
//        //    if(mPujaBarrack.dead)
//        //        mPujaBaseMsg.sliderValue = 0;
//        //    else
//        //        mPujaBaseMsg.sliderValue = (mPujaBaseMsg.sliderValue + Time.deltaTime/5f) % 1f;
//        //}
//        //else
//            if(mPujaBaseWnd.activeSelf)
//			mPujaBaseWnd.SetActive(false);
//			
//	}
//	
//	public void AddEnemy(AiObject enemy)
//	{
//		foreach(LifeShowItem_N item in mAiObjList)
//			if(item.mShowObj == enemy)
//				return;
//		LifeShowItem_N AddItem = Instantiate(mPrefab) as LifeShowItem_N;
//		AddItem.transform.parent = mGrid.transform;
//		AddItem.transform.localRotation = Quaternion.identity;
//		AddItem.transform.localScale = Vector3.one;
//		AddItem.InitItem(enemy);
//		mAiObjList.Add(AddItem);
//		mGrid.Reposition();
//	}
//	
////	public void AddEnemy(Player enemy)
////	{
////		foreach(LifeShowItem_N item in mAiObjList)
////			if(item.mShowObj == enemy)
////				return;
////		
////		LifeShowItem_N AddItem = Instantiate(mPrefab) as LifeShowItem_N;
////		AddItem.transform.parent = mGrid.transform;
////		AddItem.transform.localRotation = Quaternion.identity;
////		AddItem.transform.localScale = Vector3.one;
////		AddItem.InitItem(enemy);
////		mAiObjList.Add(AddItem);
////		mGrid.Reposition();
////		
////		if(enemy as Player)
////		{
////			BGManager.Instance.mInBattle = true;
////			BGManager.Instance.mBattleTime = Time.time;
////		}
////	}
//	
//	public void RemoveEnemy(AiObject enemy)
//	{
//		int index = mAiObjList.FindIndex(delegate(LifeShowItem_N obj) {return obj.mShowObj == enemy;});
//		if(index != -1)
//		{
//			if(mAiObjList[index].gameObject != null)
//			{
//				mAiObjList[index].transform.parent = null;
//				Destroy(mAiObjList[index].gameObject);
//			}
//			mAiObjList.RemoveAt(index);
//			mGrid.Reposition();
//		}
//	}
//	
////	public void RemoveEnemy(Player enemy)
////	{
////		int index = mAiObjList.FindIndex(delegate(LifeShowItem_N obj) {return obj.mShowObj == enemy;});
////		if(index != -1)
////		{
////			if(mAiObjList[index].gameObject != null)
////				Destroy(mAiObjList[index].gameObject);
////			mAiObjList.RemoveAt(index);
////			mGrid.Reposition();
////			if(mAiObjList.Count == 0)
////				BGManager.Instance.mInBattle = false;
////		}
////	}
//
//    public void Activate(ITowerDefenceData td,bool open)
//    {
//        mTD = td;
//        mCountWnd.SetActive(open);
//    }
//	
//    //public void SetPujaBase(puja_barrack pB)
//    //{
//    //    mPujaBarrack = pB;
//    //}
//	
//    //void OnStartBtn()
//    //{
//    //    mCountTime = Time.time;
//    //    //if(mCountTime - Time.time > 0)
//    //    {
//    //        if(mAiMonsterBeacon != null)
//    //            mAiMonsterBeacon.delayTime = 0.0f;
//    //    }
//    //}
//	
//}
