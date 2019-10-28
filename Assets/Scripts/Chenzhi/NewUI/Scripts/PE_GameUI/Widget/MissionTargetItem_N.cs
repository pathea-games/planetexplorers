using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class MissionTargetItem_N : MonoBehaviour 
{
	public UILabel 	mContent;
	public UISprite	mTargetTex;
	public UIGrid	mGrid;


	public UIMissionMgr.TargetShow targetShow = null;

	public void SetTarget(UIMissionMgr.TargetShow target) 
	{
		targetShow = target;

		UpdateContent();
        if (target.mIconName.Count == 0)
            mTargetTex.gameObject.SetActive(false);
        else
            mTargetTex.spriteName = target.mIconName[0];

        TypeMonsterData tmd = MissionManager.GetTypeMonsterData(target.mID);
        if (tmd != null && tmd.type == 2)
            mTargetTex.spriteName = "Null";
		
		mTargetTex.MakePixelPerfect();
		
		for(int i = 1;i< target.mIconName.Count;i++)
		{
			GameObject addSpr = Instantiate(mTargetTex.gameObject) as GameObject;
			addSpr.transform.parent = mGrid.transform;
            if (tmd != null && tmd.type == 2)
                addSpr.GetComponent<UISprite>().spriteName = "Null";
            else
			    addSpr.GetComponent<UISprite>().spriteName = target.mIconName[i];
			addSpr.GetComponent<UISprite>().MakePixelPerfect();
		}
		mGrid.Reposition();
	}
	

	void Update()
	{
		Vector3 pos = gameObject.transform.localPosition;
		gameObject.transform.localPosition = new Vector3( Convert.ToInt32(pos.x),Convert.ToInt32 (pos.y),Convert.ToInt32(pos.z ) );

		UpdateContent();
	}

	void UpdateContent()
	{
		if (targetShow == null)
			return; 
		string countStr =  targetShow.mMaxCount > 0 ? targetShow.mCount.ToString() + "/" + targetShow.mMaxCount.ToString() :  "";
		mContent.text = targetShow.mContent + " " +  countStr;
	}
}
