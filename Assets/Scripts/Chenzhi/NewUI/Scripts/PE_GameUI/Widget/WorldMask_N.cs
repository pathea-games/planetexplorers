using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
/*
public class WorldMask_N : MonoBehaviour
{
	public UISprite	mIcon;
	
	public MapMaskData mMaskDate;
	
	public MapMaskType mMaskType;
	
	public int 	mGlobIndex;
	
	public bool	mIsMiniMap = false;
	
	float  mWorldMapConvert = 1f; // Distanse with one pixel

    public bool Openable
    {
        get
        {
            return !(mMaskType == MapMaskType.Npc || mMaskType == MapMaskType.Vehicle
                || mMaskType == MapMaskType.Mark);
        }
    }
	
	public void SetMask(MapMaskData MaskDate, bool autoId = false)
	{
		mMaskDate = MaskDate;
		if(autoId)
			SetAutoIconIndex(0);
		else
		{
			MapIconData icon = MapIconData.s_tblIconInfo.Find(itr => (itr.mId == MaskDate.mIconId));
			mIcon.spriteName = icon.mIconName;
			mIcon.MakePixelPerfect();
			mMaskType = icon.mMaskType;
			if(mMaskType == MapMaskType.Npc)
			{
				GetComponent<Collider>().enabled = false;
				GetComponent<Collider>().isTrigger = true;
			}
		}
	}

	public void SetAutoIconIndex(int index)
	{
		MapIconData icon = MapIconData.s_tblCustomInfo[index];
		mMaskDate.mIconId = icon.mId;
		mIcon.spriteName = icon.mIconName;
		mIcon.MakePixelPerfect();
		mMaskType = MapMaskType.Custom;
	}
	
	public void SetDes(string des)
	{
		mMaskDate.mDescription = des;
	}
	
	void OnHover(bool isOver)
	{
		if(VFVoxelTerrain.RandomMap)
		{
			if(isOver)
				GameUI.Instance.mLimitWorldMapGui.OnMouseOver(this);
			else
				GameUI.Instance.mLimitWorldMapGui.OnMouseLeave();
		}
		else
		{
			if(isOver)
				GameUI.Instance.mWorldMapGui.OnMouseOver(this);
			else
				GameUI.Instance.mWorldMapGui.OnMouseLeave();
		}
	}
	
	void OnClick ()
	{
		if(VFVoxelTerrain.RandomMap)
			GameUI.Instance.mLimitWorldMapGui.OnMouseClick(this);
		else
			GameUI.Instance.mWorldMapGui.OnMouseClick(this);
	}
	
	void Update()
	{
        //if(mIsMiniMap && PlayerFactory.mMainPlayer)
        //{
        //    Vector3 dis = mMaskDate.mPosition - PlayerFactory.mMainPlayer.transform.position;
        //    dis *= mWorldMapConvert;
			
        //    dis.x = Convert.ToInt32(dis.x * GameUI.Instance.mUIMinMapCtrl.mMapScale.x);
        //    dis.y = Convert.ToInt32(dis.y * GameUI.Instance.mUIMinMapCtrl.mMapScale.y);
        //    transform.localPosition = new Vector3(dis.x,dis.z,0);
        //}
	}
//	public void UpDateTexSize()
//	{
//		mIcon.MakePixelPerfect();
//		mName.MakePixelPerfect();
//	}



	List<string> UserIconList = new List<string>();



//	void OnSelfMask0()
//	{
//		mSelIconIndex = 0;
//		mMaskSprite.spriteName = UserIconList[mSelIconIndex].mIconName;
//	}
//	
//	void OnSelfMask1()
//	{
//		mSelIconIndex = 1;
//		mMaskSprite.spriteName = UserIconList[mSelIconIndex].mIconName;
//	}
//	
//	void OnSelfMask2()
//	{
//		mSelIconIndex = 2;
//		mMaskSprite.spriteName = UserIconList[mSelIconIndex].mIconName;
//	}
//	
//	void OnSelfMask3()
//	{
//		mSelIconIndex = 3;
//		mMaskSprite.spriteName = UserIconList[mSelIconIndex].mIconName;
//	}
}*/