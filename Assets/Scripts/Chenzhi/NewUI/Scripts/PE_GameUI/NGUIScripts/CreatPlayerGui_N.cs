using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using ItemAsset;
using CustomData;

public class CreatPlayerGui_N : UIStaticWnd
{
//    public bool actionOk = true;
//
//	GameObject[] mModeObject;
//	PlayerAppearance[] mPlayerAppearance;
//	PlayerModel mCurrentPlayer;
//	
//	float[][] mFaceValue;
//	float[][] mBodyValue;
//	
//	public UIInput 		mNameInput;
//	public Palette_N mPalette;
//	
//	public CustomSettingItem_N			mSettingPerfab;
//	public UIGrid						mSettingGrid;
//	public List<CustomSettingItem_N> 	mSettingList;
//	
//	public CustomStyleItem_N 		mStylePerfab;
//	public UIGrid					mStyleGrid;
//	public List<CustomStyleItem_N>	mStyleList;
//	
//
//	int mSex = 2; 
//	
//	float mRotSpeed = 90f;
//	bool mRotLeft = false;
//	bool mRotRight = false;
//	
//	int	mTabIndex = 0;
//	int mSettingPage = 0;
//	const int PageSettingNum = 8;
//	
//	Vector3	mBodyCamPos;
//	
//	public Vector3 mTargetCamPos;
//	
//	bool mInitEnd;
//	
////	Dictionary<int, AssetBundleReq> mModelReq = new Dictionary<int, AssetBundleReq>();
//	
//	void Awake()
//	{
//		mBodyCamPos = Camera.main.transform.position;
//		InitGrid();
//		InitPlayer();
//		InitCustomValue();
//		OnBodyBtn();
//	}
//	
//	void Update()
//	{
//		if(mInitEnd)
//		{
//			if(!mCurrentPlayer.gameObject.activeSelf)
//			{
//				mCurrentPlayer.gameObject.SetActive(true);
//				mCurrentPlayer.rebuildMesh = true;
//			}
//			
//			if(mRotLeft)
//				mCurrentPlayer.transform.rotation *= Quaternion.Euler(0, mRotSpeed * Time.deltaTime, 0);
//			if(mRotRight)
//				mCurrentPlayer.transform.rotation *= Quaternion.Euler(0, -mRotSpeed * Time.deltaTime, 0);
//			
//			Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position, mTargetCamPos, 10f * Time.deltaTime);
//		}
//	}
//	
//	void InitGrid()
//	{
//		mSettingList = new List<CustomSettingItem_N>();
//		mSettingList.Add(mSettingPerfab);
//		for(int i = 0; i < 7; i++)
//		{
//			CustomSettingItem_N newItem = Instantiate(mSettingPerfab) as CustomSettingItem_N;
//			newItem.transform.parent = mSettingGrid.transform;
//			newItem.transform.localScale = Vector3.one;
//			mSettingList.Add(newItem);
//		}
//		mSettingGrid.Reposition();
//		
//		mStyleList = new List<CustomStyleItem_N>();
//		mStyleList.Add(mStylePerfab);
//		for(int i = 0; i < 27; i++)
//		{
//			CustomStyleItem_N newItem = Instantiate(mStylePerfab) as CustomStyleItem_N;
//			newItem.transform.parent = mStyleGrid.transform;
//			newItem.transform.localScale = Vector3.one;
//			mStyleList.Add(newItem);
//		}
//		mStyleGrid.Reposition();
//	}
//	
//	
//	void Release()
//	{
//		mFaceValue = null;
//		mBodyValue = null;
//		mModeObject = null;
//		mPlayerAppearance = null;
//		mCurrentPlayer = null;
//	}
//	
//	void InitPlayer()
//	{
//		mInitEnd = false;
//		mModeObject = new GameObject[2];
//		mPlayerAppearance = new PlayerAppearance[2];
//		
//		GameObject fgo = GameObject.Find("VirtualObjManager");
//		string fPathName = CharacterData.GetFBXPath(1);
//		GameObject go = Instantiate(Resources.Load(fPathName)) as GameObject;
//		go.transform.position = fgo.transform.position;
//		go.transform.rotation = Quaternion.identity;
//		CreatePlayerMode(go, 0);
////		AssetBundleReq req = AssetBundlesMan.Instance.AddReq(fPathName, fgo.transform.position, Quaternion.identity);
////		req.ReqFinishWithReqHandler += OnSpawned;
////		mModelReq[0] = req;
//		
//		fPathName = CharacterData.GetFBXPath(2);
//		go = Instantiate(Resources.Load(fPathName)) as GameObject;
//		go.transform.position = fgo.transform.position;
//		go.transform.rotation = Quaternion.identity;
//		CreatePlayerMode(go, 1);
////		req = AssetBundlesMan.Instance.AddReq(fPathName, fgo.transform.position, Quaternion.identity);
////		req.ReqFinishWithReqHandler += OnSpawned;
////		mModelReq[1] = req;
//	}
//	
//	void InitCustomValue()
//	{
//		mFaceValue = new float[2][];
//		mFaceValue[0] = new float[8];
//		mFaceValue[1] = new float[8];
//		for (int i = 0; i < 8; i++)
//        {
//            mFaceValue[0][i] = 0.5f;
//			mFaceValue[1][i] = 0.5f;
//        }
//		mBodyValue = new float[2][];
//		mBodyValue[0] = new float[AppearanceData.mBodilyElements.Length];
//		mBodyValue[1] = new float[AppearanceData.mBodilyElements.Length];
//		for (int i = 0; i < AppearanceData.mBodilyElements.Length; i++)
//        {
//            mBodyValue[0][i] = 0.5f;
//			mBodyValue[1][i] = 0.5f;
//        }
//	}
//	
//	void CreatePlayer()
//	{
//		if(mNameInput.text == "" || mNameInput.text == "NickName")
//		{
//			MessageBox_N.ShowOkBox(PELocalization.GetString(8000051));
//		}
//		else if(mInitEnd)
//		{			
//			GameConfig.Account = mNameInput.text;
//			GameConfig.IsNewGame = true;
//			GameConfig.PlayerSex = mSex;
//			mCurrentPlayer.mPlayerAppearance.mAppearanceData.mSex = mSex;
//			
//			if (!GameConfig.IsMultiMode)
//			{
//				if(GameConfig.GameModeNoMask == GameConfig.EGameMode.Singleplayer_Story)
//				{
//					LogoGui_N.Instance.StartMovie();
//				}
//				else
//				{
//					if((GameConfig.GameMode & GameConfig.EGameMode.RandomMap) != 0)
//					{
//						SeedSetGui_N.Instance.AwakeWindow();
//						return;
//					}
//					LogoGui_N.Instance.GotoLoad();
//				}
//			}
//			else
//			{
//				CustomAppearanceData data = new CustomAppearanceData(mNameInput.text, GameClientLobby.Self.SelfAccount, mCurrentPlayer.mPlayerAppearance.mAppearanceData);
//				//AppearanceData appearance = mCurrentPlayer.mPlayerAppearance.mAppearanceData;
//				//data.customStyle = appearance.mCustomStyle;
//				//Array.Copy(appearance.mCurrentBodyCustomData, data.bodydata, appearance.mCurrentBodyCustomData.Length);
//				//data.skincolor = mCurrentPlayer.mPlayerAppearance.mAppearanceData.mSkinColor;
//				//data.eyecolor = mCurrentPlayer.mPlayerAppearance.mAppearanceData.mEyeColor;
//				//data.haircolor = mCurrentPlayer.mPlayerAppearance.mAppearanceData.mHairColor;
//				//data.height = mCurrentPlayer.mPlayerAppearance.mAppearanceData.mHeight;
//				//data.width = mCurrentPlayer.mPlayerAppearance.mAppearanceData.mWith;
//				//data.name = mNameInput.text;
//				//data.account = GameClientLobby.Self.SelfAccount;
//                if (actionOk) 
//                { 
//				    GameClientLobby.Self.LobbyRPC("RPC_C2L_RoleCreate", mNameInput.text, (byte)(mSex), data);
//                    actionOk = false;
//                    Invoke("ResetActionOK", 2.0f);
//                }
//			}
//		}
//	}
//
//    public void ResetActionOK() {
//        actionOk = true;
//    }
//
//	void OnOkBtn()
//	{
//		// Only do samething when left mouse click
//		if (Input.GetMouseButtonUp(0))
//			CreatePlayer();
//	}
//	
//	
//	void OnInputNameSubmit(string text)
//	{
//		CreatePlayer();
//	}
//		
//	void OnBackBtn()
//	{
//		MessageBox_N.ShowYNBox(PELocalization.GetString(8000052), LogoGui_N.Instance.GotoTitle);
//	}
//	
//	void OnFemaleBtn()
//	{
//		if(1 != mSex)
//		{
//			mSex = 1;
//			mModeObject[1].SetActive(false);
//			mModeObject[0].SetActive(true);
//			mCurrentPlayer = mModeObject[0].GetComponent<PlayerModel>();
//			mCurrentPlayer.rebuildMesh = true;
//			switch(mTabIndex)
//			{
//			case 0:
//				OnFaceBtn();
//				break;
//			case 1:
//				OnHairBtn();
//				break;
//			case 2:
//				OnBodyBtn();
//				break;
//			case 3:
//				OnOtherBtn();
//				break;
//			}
//		}
//	}
//	
//	void OnMaleBtn()
//	{
//		if(2 != mSex)
//		{
//			mSex = 2;
//			mModeObject[1].SetActive(true);
//			mModeObject[0].SetActive(false);
//			mCurrentPlayer = mModeObject[1].GetComponent<PlayerModel>();
//			mCurrentPlayer.rebuildMesh = true;
//			switch(mTabIndex)
//			{
//			case 0:
//				OnFaceBtn();
//				break;
//			case 1:
//				OnHairBtn();
//				break;
//			case 2:
//				OnBodyBtn();
//				break;
//			case 3:
//				OnOtherBtn();
//				break;
//			}
//		}
//	}
//	
//	void OnRotLeftBtnDown()
//	{
//		mRotLeft = true;
//	}
//	void OnRotLeftBtnUp()
//	{
//		mRotLeft = false;
//	}
//	
//	void OnRotRightBtnDown()
//	{
//		mRotRight = true;
//	}
//	void OnRotRightBtncUp()
//	{
//		mRotRight = false;
//	}
//	Vector3 OffSetPos = new Vector3(0,0.12f,0.6f);
//	
//	void OnFaceBtn()
//	{
//		mTabIndex = 0;
//		mSettingPage = 0;
//		ResetFaceWnd();
//		mTargetCamPos = mCurrentPlayer.mHeadTran.position
//			 				+ OffSetPos;
//	}
//	
//	void OnHairBtn()
//	{
//		mTabIndex = 1;
//		mSettingPage = 0;
//		ResetHairWnd();
//		mTargetCamPos = mCurrentPlayer.mHeadTran.position
//			 				+ OffSetPos;
//	}
//	
//	void OnBodyBtn()
//	{
//		mTabIndex = 2;
//		mSettingPage = 0;
//		ResetBodyWnd();
//		mTargetCamPos = mBodyCamPos;
//	}
//	
//	void OnOtherBtn()
//	{
//		mTabIndex = 3;
//		mSettingPage = 0;
//		ResetOtherWnd();
//	}
//	
//	void ResetFaceWnd()
//	{
//		int faceCount = mCurrentPlayer.mPlayerAppearance.mAppearanceData.mBodyData[3].GetCount();
//		int start = mSettingPage * PageSettingNum;
//		for(int i = 0; i < mSettingList.Count; i++)
//		{
//			if(i + start < faceCount)
//				mSettingList[i].SetItemInfo(mCurrentPlayer.mPlayerAppearance.mAppearanceData.mBodyData[3].GetIconName(i + start),i + start);
//			else
//				mSettingList[i].SetItemInfo("Null",i + start);
//		}
//	}
//	
//	void ResetHairWnd()
//	{
//		int hairCount = mCurrentPlayer.mPlayerAppearance.mAppearanceData.mBodyData[0].GetCount();
//		int start = mSettingPage * PageSettingNum;
//		for(int i = 0; i < mSettingList.Count; i++)
//		{
//			if(i + start < hairCount)
//				mSettingList[i].SetItemInfo(mCurrentPlayer.mPlayerAppearance.mAppearanceData.mBodyData[0].GetIconName(i + start),i + start);
//			else
//				mSettingList[i].SetItemInfo("Null",i + start);
//		}
//	}
//	
//	void ResetBodyWnd()
//	{
//		for(int i = 0; i < mSettingList.Count; i++)
//			mSettingList[i].SetItemInfo("Null",i);
//	}
//	
//	
//	void ResetOtherWnd()
//	{
//		for(int i = 0; i < mSettingList.Count; i++)
//			mSettingList[i].SetItemInfo("Null",i);
//	}
//	
//	void OnRandomBtn()
//	{
//		switch(mTabIndex)
//		{
//		case 0:
//			int faceCount = mCurrentPlayer.mPlayerAppearance.mAppearanceData.mBodyData[3].GetCount();
//			int faceIndex = UnityEngine.Random.Range(0,faceCount);
//			mCurrentPlayer.mPlayerAppearance.mAppearanceData.mCustomStyle[3] = faceIndex;
//	        mCurrentPlayer.mPlayerAppearance.mAppearanceData.mEyeColor 
//				= new Color(UnityEngine.Random.Range(0f,1f),UnityEngine.Random.Range(0f,1f),UnityEngine.Random.Range(0f,1f),1f);
//			mCurrentPlayer.ResetCustomSetting();
//			break;
//		case 1:
//			int hairCount = mCurrentPlayer.mPlayerAppearance.mAppearanceData.mBodyData[0].GetCount();
//			int hairIndex = UnityEngine.Random.Range(0,hairCount);
//			mCurrentPlayer.mPlayerAppearance.mAppearanceData.mCustomStyle[0] = hairIndex;
//			mCurrentPlayer.mPlayerAppearance.mAppearanceData.mCustomStyle[1] = hairIndex;
//			mCurrentPlayer.mPlayerAppearance.mAppearanceData.mCustomStyle[2] = hairIndex;
//	        mCurrentPlayer.mPlayerAppearance.mAppearanceData.mHairColor 
//				= new Color(UnityEngine.Random.Range(0f,1f),UnityEngine.Random.Range(0f,1f),UnityEngine.Random.Range(0f,1f),1f);
//			mCurrentPlayer.ResetCustomSetting();
//			break;
//		case 2:
//			mCurrentPlayer.mPlayerAppearance.mAppearanceData.mSkinColor 
//				= new Color(UnityEngine.Random.Range(0f,1f),UnityEngine.Random.Range(0f,1f),UnityEngine.Random.Range(0f,1f),1f);
//			mCurrentPlayer.UpDateSkinColor();
//			break;
//		}
//	}
//	
//	void OnResetBtn()
//	{
//		switch(mTabIndex)
//		{
//		case 0:
//			mCurrentPlayer.mPlayerAppearance.mAppearanceData.mCustomStyle[3] = 0;
//	        mCurrentPlayer.mPlayerAppearance.mAppearanceData.mEyeColor = Color.white;
//			mCurrentPlayer.ResetCustomSetting();
//			break;
//		case 1:
//			mCurrentPlayer.mPlayerAppearance.mAppearanceData.mCustomStyle[0] = 0;
//			mCurrentPlayer.mPlayerAppearance.mAppearanceData.mCustomStyle[1] = 0;
//			mCurrentPlayer.mPlayerAppearance.mAppearanceData.mCustomStyle[2] = 0;
//	        mCurrentPlayer.mPlayerAppearance.mAppearanceData.mHairColor = Color.white;
//			mCurrentPlayer.ResetCustomSetting();
//			break;
//		case 2:
//			mCurrentPlayer.mPlayerAppearance.mAppearanceData.mSkinColor = Color.white;
//			mCurrentPlayer.UpDateSkinColor();
//			break;
//		}
//	}
//	
//	void OnSaveBtn()
//	{
//		
//	}
//	
//	void OnSettingLPageBtn()
//	{
//		int pageMax = 0;
//		switch(mTabIndex)
//		{
//		case 0:
//			int hairCount = mCurrentPlayer.mPlayerAppearance.mAppearanceData.mBodyData[0].GetCount();
//			pageMax = (hairCount - 1) / PageSettingNum + 1;
//			mSettingPage = Mathf.Clamp(mSettingPage - 1, 0, pageMax);
//			ResetFaceWnd();
//			break;
//		case 1:
//			int faceCount = mCurrentPlayer.mPlayerAppearance.mAppearanceData.mBodyData[3].GetCount();
//			pageMax = (faceCount - 1) / PageSettingNum + 1;
//			mSettingPage = Mathf.Clamp(mSettingPage - 1, 0, pageMax);
//			ResetHairWnd();
//			break;
//		}
//	}
//	
//	void OnSettingRPageBtn()
//	{
//		
//		int pageMax = 0;
//		switch(mTabIndex)
//		{
//		case 0:
//			int faceCount = mCurrentPlayer.mPlayerAppearance.mAppearanceData.mBodyData[3].GetCount();
//			pageMax = (faceCount - 1) / PageSettingNum + 1;
//			mSettingPage = Mathf.Clamp(mSettingPage + 1, 0, pageMax);
//			ResetFaceWnd();
//			break;
//		case 1:
//			int hairCount = mCurrentPlayer.mPlayerAppearance.mAppearanceData.mBodyData[0].GetCount();
//			pageMax = (hairCount - 1) / PageSettingNum + 1;
//			mSettingPage = Mathf.Clamp(mSettingPage + 1, 0, pageMax);
//			ResetHairWnd();
//			break;
//		}
//	}
//	
//	public void OnSettingItemClicked(int index)
//	{
//		switch(mTabIndex)
//		{
//		case 0: //Face
//			mCurrentPlayer.mPlayerAppearance.mAppearanceData.mCustomStyle[3] = index;
//			mCurrentPlayer.ResetCustomSetting();
//			break;
//		case 1: //Hair
//			mCurrentPlayer.mPlayerAppearance.mAppearanceData.mCustomStyle[0] = index;
//			mCurrentPlayer.mPlayerAppearance.mAppearanceData.mCustomStyle[1] = index;
//			mCurrentPlayer.mPlayerAppearance.mAppearanceData.mCustomStyle[2] = index;
//			mCurrentPlayer.ResetCustomSetting();
//			break;
//		case 2: //Body
//			break;
//		case 3: //Other
//			break;
//		}
//	}
//	
//	public void OnColorChanged(Color col)
//	{
//		switch(mTabIndex)
//		{
//		case 0:
//			mCurrentPlayer.ChangeEyeColor(col);
//			break;
//		case 1:
//			mCurrentPlayer.ChangeHairColor(col);
//			break;
//		case 2:
//			mCurrentPlayer.ChangeSkinColor(col);
//			break;
//		}
//	}
//	
//	void CreatePlayerMode(GameObject go, int index)
//	{
//		mModeObject[index] = go;
//        mModeObject[index].name = (index==0)?"Female":"Male";
//		mModeObject[index].tag = "Player";
//		mPlayerAppearance[index] = new PlayerAppearance(CharacterData.GetAppearanceData(index + 1));
//        mPlayerAppearance[index].InitAppearance(mModeObject[index]);
//		mCurrentPlayer = mModeObject[index].GetComponent<PlayerModel>();
//		mCurrentPlayer.mPlayerAppearance = mPlayerAppearance[index];
//		mCurrentPlayer.ResetCustomSetting();
//		mModeObject[index].SetActive(false);
//		
//		if(null != mModeObject[0] && null != mModeObject[1])
//			mInitEnd = true;
//	}
	
	
//	public void OnSpawned(GameObject go, AssetBundleReq req)
//	{
//		if(mModelReq[0] == req)
//			CreatePlayerMode(go, 0);
//		else
//			CreatePlayerMode(go, 1);
//	}
}
