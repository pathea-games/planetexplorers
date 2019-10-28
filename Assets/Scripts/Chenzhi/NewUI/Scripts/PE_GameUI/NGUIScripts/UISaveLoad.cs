using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO; 

public class UISaveLoad : UIStaticWnd
{
    static UISaveLoad	mInstance;
	
    public static UISaveLoad Instance{get{return mInstance;}}
	
	public UILabel 			mSaveLoadBtn;
	public N_ImageButton	mDealetBtn;
	public N_ImageButton	mSaveBtn;
	
	public UICheckbox		mSaveTabBtn;
	public UICheckbox		mLoadTabBtn;
	
	public UITexture		mSaveTex;
	
    public UIGrid           mInfoGrid;
	public UILabel			mGametypeText;
    public GameObject       mSeedRoot;
	public UILabel			mSeedIDText;
	public UILabel			mSeedTitleText;
	public UILabel			mPlayTimeText;
	public UILabel			mGameTimeText;
	
	public bool  			mIsSave;
	int   	mIndex;
	
	Texture2D				mAutoTex;
	
	public SaveDateItem_N 		mDataPrefab;
	public UIGrid				mDataGrid;
	List<SaveDateItem_N> 		mDataList;
    const int AutoSaveNum = (int)Pathea.ArchiveMgr.ESave.MaxAuto - (int)Pathea.ArchiveMgr.ESave.MinAuto;
    const int CustomDataNum = (int)Pathea.ArchiveMgr.ESave.MaxUser - (int)Pathea.ArchiveMgr.ESave.MinUser;
	
	void Awake ()
	{
		mInstance = this; 
		mIndex = 0;
		mSaveTex.enabled = false;
		mAutoTex = Resources.Load("Texture2d/Tex/AutoSave") as Texture2D;

		mDataList = new List<SaveDateItem_N>();

        int total = AutoSaveNum + CustomDataNum;
        for (int i = 0; i < total; i++)
		{
			SaveDateItem_N addItem = Instantiate(mDataPrefab) as SaveDateItem_N;
			addItem.transform.parent = mDataGrid.transform;
			addItem.transform.localScale = Vector3.one;
			addItem.transform.localPosition = Vector3.back;

            if (i < AutoSaveNum)
            {
                addItem.mIndexTex.text = "Auto" + (i + 1);
            }
            else
            {
                addItem.mIndexTex.text = (i - AutoSaveNum + 1).ToString();
            }

			addItem.mCheckbox.radioButtonRoot = mDataGrid.transform;

            addItem.Init(i, delegate(int index, Pathea.PeGameSummary summary)
            {
				mIndex = index;
                ChangeArvhive(summary);
            });

			mDataList.Add(addItem);
		}
		mDataGrid.Reposition();
	}
	
	void Update()
	{
        if (mWndCenter.activeSelf && Input.GetKeyDown(KeyCode.Escape))
        {
            OnQuitBtn();
        }

        if (mIsSave && mIndex < AutoSaveNum)
		{
			mSaveBtn.isEnabled = false;
			mDealetBtn.isEnabled = false;
		}
		else
		{
            //lz-2016.10.11 ¿ÕµÄ´æµµ½ûÓÃÉ¾³ý²Ù×÷¡¾´íÎó #4234¡¿
            if (mIndex >= 0 && mIndex < mDataList.Count)
            {
                mDealetBtn.isEnabled = (null != mDataList[mIndex].Summary);
            }
            mSaveBtn.isEnabled = true;
        }
	}
	
	public void ToSaveWnd()
	{
		mLoadTabBtn.startsChecked = false;
		mSaveTabBtn.startsChecked = mSaveTabBtn.isChecked = true;
		OnSaveTabBtn();
	}
	
	public void ToLoadWnd()
	{
		mSaveTabBtn.startsChecked = false;
		mLoadTabBtn.startsChecked = mLoadTabBtn.isChecked = true;
		OnLoadTabBtn();
	}
	
	void OnSaveTabBtn()
	{
		mIsSave = true;
        mSaveLoadBtn.text = PELocalization.GetString(2000058);
		mDealetBtn.gameObject.SetActive(true);

		UpdateArchiveList();
	}
	
	void OnLoadTabBtn()
	{
		mIsSave = false;
        mSaveLoadBtn.text = PELocalization.GetString(8000419);
		mDealetBtn.gameObject.SetActive(false);

		UpdateArchiveList();
	}

	public void HideSaveTabBtn()
	{
		mSaveTabBtn.gameObject.SetActive(false);
		mLoadTabBtn.gameObject.SetActive(true);
		mLoadTabBtn.transform.localPosition = mSaveTabBtn.transform.localPosition;
	}
	
	void UpdateArchiveList()
	{
        for (int i = (int)Pathea.ArchiveMgr.ESave.Min; i < (int)Pathea.ArchiveMgr.ESave.Max; i++)
        {
            string dir = Pathea.ArchiveMgr.Instance.Load((Pathea.ArchiveMgr.ESave)i);
            Pathea.PeGameSummary summary = null;

            if(!string.IsNullOrEmpty(dir))
            {
			    summary = Pathea.PeGameSummary.Mgr.Instance.Get();
            }

            mDataList[i - (int)Pathea.ArchiveMgr.ESave.Min].SetArchive(summary);
        }
		ChangeArvhive ( mDataList[mIndex].Summary );
	}	
	
	void OnSaveLoadBtn()
	{
		if (mIndex >= mDataList.Count)												return;
        
		if(mIsSave)
		{
			if(mIndex < AutoSaveNum)												return;
            SaveData();
		}
		else
		{
			if (mDataList [mIndex] == null || mDataList [mIndex].Summary == null)	return;
            LoadData();
		}
	}
	
	void SaveData()
	{
        if (Pathea.PeGameMgr.playerType == Pathea.PeGameMgr.EPlayerType.Tutorial||(Pathea.PeGameMgr.IsSingleAdventure&&Pathea.PeGameMgr.yirdName ==Pathea.AdventureScene.Dungen.ToString()))
        {
            //lz-2016.10.31 You can't save file in this area!
            new PeTipMsg("[C8C800]" + PELocalization.GetString(8000852), PeTipMsg.EMsgLevel.Norm, PeTipMsg.EMsgType.Stroy);
            return;
        }
        Pathea.ArchiveMgr.Instance.Save((Pathea.ArchiveMgr.ESave)mIndex);
		MessageBox_N.ShowOkBox(PELocalization.GetString(8000029));

		UpdateArchiveList();
		//mDataList[mIndex].OnActivate(true);
	}
	
	void OnDeletBtn()
	{
        Pathea.ArchiveMgr.Instance.Delete((Pathea.ArchiveMgr.ESave)mIndex);

        MessageBox_N.ShowOkBox(PELocalization.GetString(8000030));

		UpdateArchiveList();
	}
		
	void OnQuitBtn()
	{
		Hide();
		if(null != GameUI.Instance)
			Hide();
		else
			TitleMenuGui_N.Instance.Show();
	}

    void ChangeArvhive(Pathea.PeGameSummary summary)
	{
		if(summary != null)
		{
            switch (summary.sceneMode)
			{
			    case Pathea.PeGameMgr.ESceneMode.Story:
                    mGametypeText.text = PELocalization.GetString(10007);
                    //mSeedIDText.text = "Unknown";
                    //mSeedTitleText.text = "Area:";
                    mSeedRoot.SetActive(false);
                    break;
                case Pathea.PeGameMgr.ESceneMode.Adventure:
                    mGametypeText.text = PELocalization.GetString(10008);
                    mSeedIDText.text = summary.seed;
				    mSeedTitleText.text = PELocalization.GetString(8000361)+":";
                    mSeedRoot.SetActive(true);
                    break;
                case Pathea.PeGameMgr.ESceneMode.Build:
                    mGametypeText.text = PELocalization.GetString(10009);
				    mSeedIDText.text = summary.seed;
                    mSeedTitleText.text = PELocalization.GetString(8000361) + ":";
                    mSeedRoot.SetActive(true);
                    break;
                case Pathea.PeGameMgr.ESceneMode.Custom:
                    mGametypeText.text = PELocalization.GetString(10222);
                    mSeedIDText.text = PELocalization.GetString(8000558);
                    mSeedTitleText.text = PELocalization.GetString(8000557);
                    mSeedRoot.SetActive(false);
                    break;
                default:
                    break;
			}
            mInfoGrid.repositionNow = true;

			UTimer tmp_playtimer = new UTimer ();
			tmp_playtimer.Second = summary.playTime;
			if ( tmp_playtimer.Day < 1 )
				mPlayTimeText.text = tmp_playtimer.FormatString("hh:mm:ss");
			else
				mPlayTimeText.text = tmp_playtimer.FormatString("D days hh:mm:ss");

			PETimer tmp_gametimer = PETimerUtil.GetTmpTimer();
			tmp_gametimer.Second = summary.gameTime;
			mGameTimeText.text = tmp_gametimer.FormatString("hh:mm:ss AP");
			mSaveTex.enabled = true;
			mSaveTex.mainTexture = (summary.screenshot != null) ?  summary.screenshot : mAutoTex;
		}
		else
		{
			mGametypeText.text = "";
			mSeedIDText.text = "";
//			mBorimText.text = "";
			mPlayTimeText.text = "";
			mGameTimeText.text = "";
			mSaveTex.enabled = false;
			mSaveTex.mainTexture = null;
		}
	}
	
	void LoadData()
	{ 
		Pathea.ArchiveMgr.ESave eSave = (Pathea.ArchiveMgr.ESave)mIndex;

		if(string.IsNullOrEmpty( Pathea.ArchiveMgr.Instance.Load(eSave)) )
			return;

        Pathea.PeGameMgr.playerType = Pathea.PeGameMgr.EPlayerType.Single;
        Pathea.PeGameMgr.loadArchive = eSave;

        Pathea.PeFlowMgr.Instance.LoadScene(Pathea.PeFlowMgr.EPeScene.GameScene, eSave != Pathea.ArchiveMgr.ESave.Auto1);
	}
}
