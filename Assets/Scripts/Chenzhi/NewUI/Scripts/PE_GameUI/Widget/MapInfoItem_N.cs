using UnityEngine;
using System.Collections;
using AiAsset;
using System;

public class MapInfoItem_N : MonoBehaviour
{
	bool 	mIsMissionTrack;
	
	public int		mMissionId;
	//Vector3			mMissionPos;
	
	public MonoBehaviour 	mAiObj;
	
	
	
	public UISprite mIcon;
	
	float			mWorldMapConvert = 1f; // Distanse with one pixel
	
	float			mViewRadius = 128;
	
	float			mMissionRadius = 32;
	
	bool			mIsBigMap = false;
	
	public void SetAiObj(MonoBehaviour aiObj)
	{
		mIcon.MakePixelPerfect();
		Update();
	}
	
	public void SetMissionTrack(int missionId,Vector3 pos,bool worldMap = false)
	{
		mIsBigMap = worldMap;
		mIsMissionTrack = true;
		mMissionId = missionId;
		//mMissionPos = pos;
		if(worldMap)
		{
			mIcon.spriteName = "MissionTrack";
			mIcon.MakePixelPerfect();
		}
		Update();
	}
	
	public void SetMissionRadius(float radius)
	{
		mMissionRadius = Mathf.Clamp(radius * mWorldMapConvert,32f,128f);
	}
	
	public void SetMapInfo(float convert,float viewRadius)
	{
		mWorldMapConvert = 1f;//convert;
		mViewRadius = viewRadius;
		Update();
	}
	
	void Update()
	{
        //if(PlayerFactory.mMainPlayer == null)
        //    return;
		if(mIsMissionTrack)
		{
			Vector3 dis = Vector3.zero;
			if(mIsBigMap)
				return;//dis = mMissionPos - PlayerFactory.mMainPlayer.transform.position; // HalfWorldSize
            //else
            //    dis = mMissionPos - PlayerFactory.mMainPlayer.transform.position;
			dis.y = dis.z;
			dis.z = 0;
			dis *= mWorldMapConvert;
			if(dis.magnitude + 30 > mViewRadius+mMissionRadius)
			{
				mIcon.spriteName = "MissionTrackDirArrow";
				dis = (mViewRadius - 10) * dis.normalized;
				mIcon.MakePixelPerfect();
				transform.localScale = new Vector3(11,16,1);
				transform.rotation = Quaternion.FromToRotation(Vector3.up,dis);
			}
			else
			{
				mIcon.spriteName = "MissionTrack";
				transform.localScale = new Vector3(mMissionRadius,mMissionRadius,1);
				transform.rotation = Quaternion.identity;
			}
			dis.x = Convert.ToInt32(Mathf.Round(dis.x) * GameUI.Instance.mUIMinMapCtrl.mMapScale.x);
			dis.y = Convert.ToInt32(Mathf.Round(dis.y) * GameUI.Instance.mUIMinMapCtrl.mMapScale.y);
			transform.localPosition = dis;
		}
		else
		{
			if(mAiObj == null)
			{
				Destroy(gameObject);
				return;
			}

            //NpcRandom hero = mAiObj as NpcRandom;
            
            //if(hero && hero.Recruited)
            //{
            //    if(hero.dead)
            //        mIcon.spriteName = "ServantDead";
            //    else
            //        mIcon.spriteName = "map_Servant";
            //    mIcon.MakePixelPerfect();
            //}
            Vector3 dis = Vector3.zero; //((MonoBehaviour)mAiObj).transform.position - PlayerFactory.mMainPlayer.transform.position;
			dis *= mWorldMapConvert;

			dis.x = (int)dis.x;
			dis.y = (int)dis.y;
			if (!mIsBigMap)
			{
				dis.x = Convert.ToInt32(dis.x * GameUI.Instance.mUIMinMapCtrl.mMapScale.x);
				dis.y = Convert.ToInt32(dis.y * GameUI.Instance.mUIMinMapCtrl.mMapScale.y);
			}

			transform.localPosition = new Vector3(dis.x,dis.z,0);
		}
	}
}
