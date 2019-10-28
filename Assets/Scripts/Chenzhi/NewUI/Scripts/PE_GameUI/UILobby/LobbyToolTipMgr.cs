using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class LobbyToolTipInfo
{

	public string  mRoomNumber;
	public string  mGameMode;
	public string  mMapName;
	public string  mTeamNo;
	public string  mSeedNo;
	public string  mAlienCamp;
	public string  mTown;
	public string  mMapSize;
	public string  mElevation;

	public LobbyToolTipInfo(ServerRegistered _server)
	{
		mRoomNumber = _server.ServerID.ToString();

		Pathea.PeGameMgr.ESceneMode mode = (Pathea.PeGameMgr.ESceneMode)_server.GameMode;
		switch (mode)
		{
			case Pathea.PeGameMgr.ESceneMode.Adventure:
				mGameMode = "Adventure";
				break;
			case Pathea.PeGameMgr.ESceneMode.Build:
				mGameMode = "Build";
				break;
			case Pathea.PeGameMgr.ESceneMode.Story:
				mGameMode = "Story";
				break;
			case Pathea.PeGameMgr.ESceneMode.Custom:
				mGameMode = "Custom";
				break;
			default:
				mGameMode = "Adventure";
				break;
		}
		mMapName = "";
		mTeamNo = "";
		mSeedNo = "";
		mAlienCamp = "";
		mTown = "";
		mMapSize = "";
		mElevation = "";
	}




	public List<string> ToList()
	{
		List<string> strList = new List<string>();
		strList.Add(mRoomNumber);
		strList.Add(mGameMode);
		strList.Add(mMapName);
		strList.Add(mTeamNo);
		strList.Add(mSeedNo);
		strList.Add(mAlienCamp);
		strList.Add(mTown);
		strList.Add(mMapSize);
		strList.Add(mElevation);
		return strList;
	}
}

public class LobbyToolTipMgr : MonoBehaviour
{

	static LobbyToolTipMgr 		mInstance;
	public GameObject 			mContent;

	public UILabel              mLbRoomNumber;
	public UILabel              mLbGameMode;
	public UILabel              mLbMapName;
	public UILabel              mLbTeamNo;
	public UILabel              mLbSeedNo;
	public UILabel              mLbAlienCamp;
	public UILabel              mLbTown;
	public UILabel              mLbMapSize;
	public UILabel              mLbElevation;

	void Awake () 
	{
		mInstance = this; 
	}
	
	void SetText( LobbyToolTipInfo info)
	{
		if(null != info)
		{
			List<string> strList = info.ToList();

			mContent.SetActive(true);

			mLbRoomNumber.text =  strList[0].Length > 0 ? strList[0] : "N/A";
			mLbGameMode.text   =  strList[1].Length > 0 ? strList[0] : "N/A";
			mLbMapName.text    =  strList[2].Length > 0 ? strList[0] : "N/A";
			mLbTeamNo.text     =  strList[3].Length > 0 ? strList[0] : "N/A";
			mLbSeedNo.text     =  strList[4].Length > 0 ? strList[0] : "N/A";
			mLbAlienCamp.text  =  strList[5].Length > 0 ? strList[0] : "N/A";
			mLbTown.text       =  strList[6].Length > 0 ? strList[0] : "N/A";
			mLbMapSize.text    =  strList[7].Length > 0 ? strList[0] : "N/A";
			mLbElevation.text  =  strList[8].Length > 0 ? strList[0] : "N/A";

		}
		else
		{
			mContent.SetActive(false);
		}
	}
	
	static public void ShowText (LobbyToolTipInfo info)
	{
		if(mInstance)
			mInstance.SetText(info);
	}
}
