using UnityEngine;
using System;
using System.Collections;

public class SaveDateItem_N : MonoBehaviour
{
    public delegate void OnSelected(int index, Pathea.PeGameSummary summary);

	public UICheckbox	mCheckbox;
	public UILabel 	mCharacterName;
	public UILabel	mGameType;
	public UILabel	mSaveTime;
	public UILabel	mIndexTex;

    int mIndex;
    public OnSelected mOnSelected;
	public Pathea.PeGameSummary Summary{ get {return mSummary;} }


    Pathea.PeGameSummary mSummary;

	public void Init(int index, OnSelected onSelected)
	{
        mIndex = index;
        mOnSelected = onSelected;
    }

    public void SetArchive(Pathea.PeGameSummary summary)
    {        
        mSummary = summary;

        if (mSummary == null)
        {
            ClearInfo();
            return;
        }

        mCharacterName.text = summary.playerName;
        mSaveTime.text = summary.saveTime.ToString();

        switch (summary.sceneMode)
		{
            case Pathea.PeGameMgr.ESceneMode.Story:
			    mGameType.text = "Story";
			    break;
            case Pathea.PeGameMgr.ESceneMode.Adventure:
			    mGameType.text = "Adventure";
			    break;
            case Pathea.PeGameMgr.ESceneMode.Build:
			    mGameType.text = "Build";
			    break;
			case Pathea.PeGameMgr.ESceneMode.Custom:
				mGameType.text = "Custom";
				break;
		}
	}
	
	void ClearInfo()
	{
		mCharacterName.text = "";
		mGameType.text = "";
		mSaveTime.text = "";
	}
	
	void OnActivate(bool active)
	{
        if (active && null != mOnSelected)
        {
            mOnSelected(mIndex, mSummary);
        }
	}
}
