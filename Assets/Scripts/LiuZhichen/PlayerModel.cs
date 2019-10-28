using System;
using System.Collections.Generic;
using UnityEngine;
using AppearBlendShape;
using CustomCharactor;


public class PlayerModel :MonoBehaviour//,IPlayerModel
{
	public GameObject	mMode;
	public GameObject	mModeRoot;
	public Transform	mHeadTran;

	[HideInInspector]
	public AppearData mAppearData;
	[HideInInspector]
	public AvatarData mNude; 
	[HideInInspector]
	public AvatarData mClothed;


	IEnumerable<string> CurrentParts
	{
		get
		{
			return AvatarData.GetParts(mClothed, mNude);
		}
	}

	AvatarData CurrentAvatar
	{
		get
		{
			return mClothed;
		}
	}
	
	AvatarData CurrentNudeAvatar
	{
		get
		{
			return mNude;
		}
	}

	void Awake()
	{
//			mAppearData = new AppearData();
//			mClothed = new AvatarData();
//			mNude = new AvatarData();
//			mNude.SetFemaleBody();
	}

	public void BuildModel()
	{
		AppearBuilder.Build(mModeRoot, mAppearData, CurrentParts);
	}

    public void ApplyColor()
    {
        AppearBuilder.ApplyColor(mModeRoot, mAppearData);
    }
}