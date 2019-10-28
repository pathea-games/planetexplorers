using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class UIServerCtrl : MonoBehaviour 
{
	//------------------------- interface -----------------------------------
	public event OnGuiBtnClicked StartFunc = null;

	public event OnGuiBtnClicked BtnStart = null;
	public event OnGuiBtnClicked BtnBack = null;
	public event OnGuiBtnClicked BtnRefresh = null;
	public event OnGuiBtnClicked BtnClose = null;
	public event OnGuiBtnClicked BtnCreate = null;
	public event OnGuiBtnClicked BtnDelete = null;

	public event OnGuiIndexBaseCallBack checkListItem = null;
	public UIPageListCtrl mList;

	public GameObject mRoomWnd;
	public UILabel mSeverName;
	public UIInput mPassWord;
	public UILabel mGameType;
	public UILabel mGameMode;
	public UILabel mMonster;
	public UILabel mMapName;
	public UILabel mTeamNO;
	public UILabel mPlayerNo;
	public UILabel mSeedNo;
	public UILabel mBiome;
	public UILabel mClimate;
	public UICheckbox mCheckPrivate;
	public UICheckbox mCheckProxy ;

	public UISprite mBtnTeamLeftBg;
	public UISprite mBtnTeamRightBg;
	public BoxCollider mBCTeamLeft;
	public BoxCollider mBCTeamRight;
	
	public UIHostCreateCtrl mHostCreateCtrl;

	private int mTeamCount = 1;
	private int mPlayerCount = 1;
	// set right wnd info

	//--------------------------------------------------------------------------

	public void UpdateServerInfo(MyServer ms)
	{
		List<string> strList = ms.ToServerInfo();
		if(strList.Count < 11)
			return;

		mSeverName.text = strList[0];
		mPassWord.text =  strList[1];
		mGameType.text = strList[2];
		mGameMode.text = strList[3];
		mMonster.text = strList[4];
		mMapName.text = strList[5];
		mTeamNO.text = strList[6];
		mPlayerNo.text = strList[7];
		mSeedNo.text = strList[8];
		mBiome.text = strList[9];
		mClimate.text = strList[10];
		
		mCheckPrivate.isChecked = ms.isPrivate;
		mCheckProxy.isChecked = ms.proxyServer;

		if (Pathea.PeGameMgr.IsVS || Pathea.PeGameMgr.IsCustom)
			SetTeamState(true);
		else
			SetTeamState(false);

		int.TryParse(mTeamNO.text, out mTeamCount);
		int.TryParse(mPlayerNo.text, out mPlayerCount);

		
	}



	public void GetMyServerInfo(MyServer ms)
	{
		if(ms != null)
		{
			ms.gamePassword = mPassWord.text;
			ms.teamNum = int.Parse(mTeamNO.text);
			ms.numPerTeam = int.Parse(mPlayerNo.text);
			ms.isPrivate = mCheckPrivate.isChecked;
			ms.proxyServer = mCheckProxy.isChecked;
		}
	}


	void Awake()
	{
		mList.CheckItem += CheckListItem;
	}

	// Use this for initialization
	void Start () 
	{
		if(StartFunc != null)
			StartFunc();
	}

	// Update is called once per frame
	void Update () 
	{

	}


	void SetTeamState(bool IsActive)
	{
		float color = 1;
		if(!IsActive)
		 	color = 0.3f;

		mBtnTeamLeftBg.color = new Color(color,color,color,1);
		mBtnTeamRightBg.color = new Color(color,color,color,1);

		mBCTeamLeft.enabled = IsActive;
		mBCTeamRight.enabled = IsActive;
	}
	
	void BtnBackOnClick()
	{
		if(Input.GetMouseButtonUp(0))
		{
			this.gameObject.SetActive(false);
			mRoomWnd.SetActive(true);

			if(BtnBack != null)
				BtnBack();
		}
	}

	void BtnCreateOnClick()
	{
		if(Input.GetMouseButtonUp(0))
		{
			if(BtnCreate != null)
			BtnCreate();
			mHostCreateCtrl.gameObject.SetActive(true);
			mHostCreateCtrl.InitMapInfo();
		}
	}

	void BtnDeleteOnClick()
	{
		if(Input.GetMouseButtonUp(0))
		{
			if (mList.mSelectedIndex == -1)
				return;
			if (mList.mSelectedIndex >= mList.mItems.Count)
				return;

			string serverName = mList.mItems[mList.mSelectedIndex].mData[0];
			string strText = UIMsgBoxInfo.mCZ_DeleteSrever.GetString() +  "'" + serverName +  "'";
			MessageBox_N.ShowYNBox(strText,OnBtnDelete,null);
		}
	}

	void OnBtnDelete()
	{
		if(BtnDelete !=null)
			BtnDelete();
	}

	void BtnStartOnClick()
	{
		if(Input.GetMouseButtonUp(0))
		{
			if(BtnStart != null)
				BtnStart();
		}
	}
	
	void BtnCloseOnClick()
	{
		if(Input.GetMouseButtonUp(0))
		{
			if(BtnClose != null)
				BtnClose();
		}
	}



	void CheckListItem(int index)
	{
		if(Input.GetMouseButtonUp(0))
		{
			if(checkListItem != null)
				checkListItem(index);
		}
	}


	void BtnRefreshOnClick()
	{
		if(BtnRefresh != null)
			BtnRefresh();
	}


	void OnTeamNumSelectLeft()
	{
		if(Input.GetMouseButtonUp(0))
		{
			if (Pathea.PeGameMgr.IsVS)
			{
				if (mTeamCount >= 3)
				{
					mTeamCount--;
					mPlayerCount = 4;
					mTeamNO.text = mTeamCount.ToString();
					mPlayerNo.text = mPlayerCount.ToString();
				}
			}
		}
	}
	void OnTeamNumSelectRight()
	{
		if(Input.GetMouseButtonUp(0))
		{
			if (Pathea.PeGameMgr.IsVS)
			{
				if (mTeamCount <= 3)
				{
					mTeamCount++;
					mPlayerCount = 4;
					mTeamNO.text = mTeamCount.ToString();
					mPlayerNo.text = mPlayerCount.ToString();
				}
			}
		}
	}
	void OnPlayerNumSelectLeft()
	{
		if(Input.GetMouseButtonUp(0))
		{
			if (!Pathea.PeGameMgr.IsSurvive)
			{
				if (mTeamCount * (mPlayerCount / 2) >= 1)
				{
					mPlayerCount = mPlayerCount / 2;
					mPlayerNo.text = mPlayerCount.ToString();
				}
			}
			else
			{
				mPlayerCount = Mathf.Max(1, mPlayerCount / 2);
				mPlayerNo.text = mPlayerCount.ToString();
			}
		}
	}

	void OnPlayerNumSelectRight()
	{
		if(Input.GetMouseButtonUp(0))
		{
			if (!Pathea.PeGameMgr.IsSurvive)
			{
				if (mTeamCount * (mPlayerCount * 2) <= 32)
				{
					mPlayerCount = mPlayerCount * 2;
					mPlayerNo.text = mPlayerCount.ToString();
				}
			}
			else
			{
				mPlayerCount = Mathf.Min(32, mPlayerCount * 2);
				mPlayerNo.text = mPlayerCount.ToString();
			}
		}
	}
}
