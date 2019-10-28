/*using UnityEngine;
using Mono.Data.SqliteClient;
using System;
using System.Collections;
using System.Collections.Generic;

//public class TutorialData
//{
//	public int 		mID;
//	public int		mType;
//	public string 	mContent;
//	public string	mTexName;
//	public static Dictionary<int,TutorialData> s_tblTutorialData;
//	public static void LoadData()
//	{
//	    SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable("handphone");
//        s_tblTutorialData = new Dictionary<int, TutorialData>();
//		
//        while (reader.Read())
//        {
//			TutorialData addData = new TutorialData();
//			addData.mID = Convert.ToInt32(reader.GetString(reader.GetOrdinal("ID")));
//			addData.mType = Convert.ToInt32(reader.GetString(reader.GetOrdinal("Type")));
//			addData.mContent = reader.GetString(reader.GetOrdinal("Name"));
//			addData.mTexName = reader.GetString(reader.GetOrdinal("Picture"));
//			s_tblTutorialData[addData.mID] = addData;
//		}
//	}
//}



public class PhoneGui_N : GUIWindowBase
{
	static PhoneGui_N		mInstance;
	public static PhoneGui_N Instance{ get { return mInstance; } }
	public UILabel			mTitleCount;
	public UITexture		mHelpTex;
	public UITexture		mMatelScanTex;
	
	public GameObject		mMainWnd;
	public GameObject		mHelpWnd;
	public GameObject		mMetalScanWnd;
	public UIMonoRailCtrl	mRailway;
	
	public UIGrid			mHelpGrid;
	public TutorialItem_N	mPerfab;
	List<TutorialItem_N> 	mTitleList;
	int mTabIndex;
	
	int mCurrentID = -1;
	
	//vars for metalscan
	public GameObject		mScanTextLabel;
	public Transform 		mMetalListRoot;
	public UICheckbox 		AllCheckBox;
	public UICheckbox		mHelpBox;
	public UICheckbox		mMatScanBox;
	public UICheckbox		mRailwayBox;
	public Camera 			mMetalScanCam;
	public UISprite			mMetalTex;
	public UILabel			mMetalDes;
	public UIGrid			mMetalScanGrid;
	public MetalScanItem_N 	mMetalScanItemPerfab;
	List<MetalScanItem_N> 	mMetalScanItemList;

	public GameObject mCkRialBtn;
	
	float mCamViewDis = 250f;
	float ViewDisMax = 400f;
	float ViewDisMin = 50f;
	float mCamDegX = -90f;
	float mCamDegY = 45f;
	
	public override void InitWindow ()
	{
		base.InitWindow ();
		mInstance = this;
		mTabIndex = 0;
		mTitleList = new List<TutorialItem_N>();
		mMetalScanItemList = new List<MetalScanItem_N>();
		mMatelScanTex.mainTexture = mMetalScanCam.targetTexture = new RenderTexture(672,378,16);
		foreach(MetalScanItem_N item in mMetalScanItemList)
			item.transform.localPosition = Vector3.back;
		mRailway.InitWindow();

		InitGameType();
	}


	void InitGameType()
	{
		if (GameConfig.IsMultiMode)
		{
			if (mCkRialBtn != null)
				mCkRialBtn.SetActive(false);
		}
	}

	void Update()
	{
		if(UICamera.hoveredObject == mMatelScanTex.gameObject)
		{
			if(Input.GetMouseButton(0) || Input.GetMouseButton(1))
			{
				mCamDegX += Input.GetAxis("Mouse X") * 15f * (SystemSettingData.Instance.CameraHorizontalInverse ? 1 : -1);
				if(mCamDegX < 0)
					mCamDegX += 360f;
				else if(mCamDegX > 360f)
					mCamDegX -= 360f;
					
				mCamDegY = Mathf.Clamp(mCamDegY + Input.GetAxis("Mouse Y") * 5f * (SystemSettingData.Instance.CameraVerticalInverse ? 1 : -1), -80f, 80f);
			}
			mCamViewDis = Mathf.Clamp(mCamViewDis - Input.GetAxis("Mouse ScrollWheel") * 30.0f, ViewDisMin, ViewDisMax);
		}
		if(null != PlayerFactory.mMainPlayer)
		{
			float degx = mCamDegX/180f * Mathf.PI;
			float degy = mCamDegY/180f * Mathf.PI;
			mMetalScanCam.transform.position = PlayerFactory.mMainPlayer.transform.position 
				+ mCamViewDis * new Vector3(Mathf.Cos(degx) * Mathf.Cos(degy), Mathf.Sin(degy), Mathf.Sin(degx) * Mathf.Cos(degy));
			mMetalScanCam.transform.LookAt(PlayerFactory.mMainPlayer.transform.position, Vector3.up);
		}
		if(mScanTextLabel.activeSelf != MSScan.Instance.bInScanning)
			mScanTextLabel.SetActive(MSScan.Instance.bInScanning);
	}
	
	public override void AwakeWindow ()
	{
		base.AwakeWindow ();
		if(-1 != mCurrentID)
		{
//			OnItemSelected(mCurrentID);
		}
		if(mMainWnd.activeSelf && mMetalScanWnd.activeSelf)
			ResetMetal();
	}
	
	public override bool HideWindow ()
	{
		if(null != mHelpTex.mainTexture)
		{
			Destroy(mHelpTex.mainTexture);
			mHelpTex.mainTexture = null;
		}
		return base.HideWindow ();
	}
	
	void OnClose()
	{
		HideWindow();
	}
	
	public void ResetTutorial()
	{
		List<int> showIDList = new List<int>();
		List<int> allID = PlayerFactory.mMainPlayer.mActiveTutorialID;
		
		foreach(TutorialItem_N item in mTitleList)
		{
			item.transform.parent = null;
			Destroy(item.gameObject);
		}
		mTitleList.Clear();
		
		foreach(int id in allID)
		{
			if(TutorialData.s_tblTutorialData[id].mType == mTabIndex + 1)
				showIDList.Add(id);
		}
		
		for(int i = 0; i < showIDList.Count; i++)
		{
			TutorialItem_N addItem = Instantiate(mPerfab) as TutorialItem_N;
			//addItem.mParent = this;
			addItem.transform.parent = mHelpGrid.transform;
			addItem.transform.localScale = Vector3.one;
			addItem.transform.localPosition = Vector3.zero;
			addItem.mCheckBox.radioButtonRoot = mHelpGrid.transform;
			addItem.SetItem(showIDList[i],TutorialData.s_tblTutorialData[showIDList[i]].mContent);
			mTitleList.Add(addItem);
		}
		mHelpGrid.Reposition();
	}
	
//	public void OnItemSelected(int ID)
//	{
//		if(!TutorialData.s_tblTutorialData.ContainsKey(ID))
//		{
//			mHelpTex.enabled = false;
//			return;
//		}
//		foreach(TutorialItem_N ti in mTitleList)
//		{
//			if(ti.mID == ID)
//			{
//				ti.mCheckBox.isChecked = true;
//				break;
//			}
//		}
//		
//		mHelpTex.enabled = true;
//		mCurrentID = ID;
//		mTitleCount.text = TutorialData.s_tblTutorialData[ID].mContent;
//		
//		if(null != mHelpTex.mainTexture)
//			Destroy(mHelpTex.mainTexture);
//		
//		Texture tex = Resources.Load("GUI/Atlases/Tex/" + TutorialData.s_tblTutorialData[ID].mTexName) as Texture;
//		mHelpTex.mainTexture = Instantiate(tex) as Texture;
//		Resources.UnloadAsset(tex);
//	}
	
	public void ShowRailway()
	{
		if(!IsOpen())
			AwakeWindow();
		mRailwayBox.isChecked = true;
	}
	
//	public bool AddTutorial(int ID)
//	{
//		if(!PlayerFactory.mMainPlayer.mActiveTutorialID.Contains(ID) && TutorialData.s_tblTutorialData.ContainsKey(ID))
//		{
//			PlayerFactory.mMainPlayer.mActiveTutorialID.Add(ID);
//			if(!IsOpen())
//				AwakeWindow();
//			ResetTutorial();
//			OnItemSelected(ID);
//			mHelpBox.isChecked = true;
//            return true;
//		}
//
//        return false;
//	}
	
	public void OnMetalSelected(byte voxelType)
	{
		MetalScanItem msi = MetalScanData.GetItemByVoxelType(voxelType);
		if(null != msi)
		{
			mMetalTex.spriteName = msi.mTexName;
			mMetalTex.MakePixelPerfect();
			mMetalDes.text = msi.mDes;
		}
	}
	
	void OnShowMainWnd()
	{
		mMainWnd.SetActive(!mMainWnd.activeSelf);
	}
	
	void OnOpenMainWnd()
	{
		if(!mMainWnd.activeSelf)
			mMainWnd.SetActive(true);
	}
	
//	void OnHelpBtn(bool selected)
//	{
//		mMainWnd.SetActive(true);
//		mHelpWnd.SetActive(selected);
//		if(selected)
//		{
//			ResetTutorial();
//			if(mTitleList.Count > 0)
//			{
//				if(-1 == mCurrentID )
//					OnItemSelected(mTitleList[0].mID);
//				else
//					OnItemSelected(mCurrentID);
//			}
//			else
//				mTitleCount.text = "";
//		}
//	}
	
	void OnMetalScanBtn(bool selected)
	{
		mMainWnd.SetActive(true);
		mMetalScanWnd.SetActive(selected);
		if(selected)
			ResetMetal();
	}
	
	public void ResetMetal()
	{
		if(null != PlayerFactory.mMainPlayer)
		{
			foreach(MetalScanItem_N item in mMetalScanItemList)
			{
				item.transform.parent = null;
				GameObject.Destroy(item.gameObject);
			}
			mMetalScanItemList.Clear();
			
			for(int i = 0; i < PlayerFactory.mMainPlayer.m_MetalScanList.Count; i++)
			{
				MetalScanItem msi = MetalScanData.GetItemByID(PlayerFactory.mMainPlayer.m_MetalScanList[i]);
				MetalScanItem_N item = Instantiate(mMetalScanItemPerfab) as MetalScanItem_N;
				item.transform.parent = mMetalScanGrid.transform;
				item.transform.localPosition = Vector3.back;
				item.transform.localScale = Vector3.one;
				item.SetItem(msi.mMatName, msi.mColor, msi.mType);
				mMetalScanItemList.Add(item);
			}
			
			mMetalScanGrid.Reposition();
		}
	}
	
	void OnRailBtn(bool selected)
	{
		mMainWnd.SetActive(true);
		if(selected)
			mRailway.Show();
		else
			mRailway.Hide();
	}
	
	void OnScan()
	{
		if(null != PlayerFactory.mMainPlayer)
		{
			List<byte> matList = new List<byte>();
			for(int i = 0; i < mMetalScanItemList.Count; i++)
			{
				if(mMetalScanItemList[i].mCheckBox.isChecked)
					matList.Add(mMetalScanItemList[i].mType);
			}
			if(matList.Count > 0)
				MSScan.Instance.MakeAScan(PlayerFactory.mMainPlayer.transform.position, matList);
		}
	}
	
	void OnCheckAll(bool isSelected)
	{
//		for(int i = 0; i < mMetalScanItemList.Count; i++)
//			mMetalScanItemList[i].mCheckBox.isChecked = isSelected;
	}
}*/
