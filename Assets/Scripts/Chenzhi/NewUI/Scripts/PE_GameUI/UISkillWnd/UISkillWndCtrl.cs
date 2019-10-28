using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pathea;
using System;
using HelpExtension;

public class UISkillWndCtrl : UIBaseWnd 
{
	[SerializeField] GameObject mSkillTypePrefab = null;
	[SerializeField] Transform mContent;
	[SerializeField] UIScrollBar mScrollBar;
	[SerializeField] UILabel mLbExp;
	[SerializeField] UISlider mSliderExp; 

	[SerializeField] UISkillTypeCtrl mSkillTypeCtrl;
	[SerializeField] UILabel mInfo;

	List<UISkillType> mSkillTypeList = new List<UISkillType>();
	List<UISkillType.SkillTypeData> mSkillTypeDatas = new List<UISkillType.SkillTypeData>();

	UISkillType mActiveSkillType = null;

	public delegate void DNotify (UISkillWndCtrl uiSkill);
	public event DNotify onRefreshTypeData;

	enum TableState
	{
		tb_None = 0,
		tb_Work = 1,
		tb_Live = 2,
		tb_Fight = 3,

	}
	TableState mTableState = TableState.tb_None;
	//bool tableChange = false;
	SkillTreeUnitMgr skillMgr = null;
	SkAliveEntity skEntiyt = null;

	public SkillTreeUnitMgr _SkillMgr 
    {
        get{
            if(skillMgr == null) //lz-2016.11.18 Cursh bug 错误 #6705
                skillMgr = (null==PeCreature.Instance||null==PeCreature.Instance.mainPlayer) ? null:PeCreature.Instance.mainPlayer.GetCmpt<SkillTreeUnitMgr>();
            return skillMgr;
        }
    }

//	int _SkillExp  { get {if (skEntiyt == null) return 0; return Convert.ToInt32( skEntiyt.GetAttribute(AttribType.Exp) );} } 
//	int _SkillExpMax  { get {if (skEntiyt == null) return 0; return Convert.ToInt32( skEntiyt.GetAttribute(AttribType.ExpMax) );} } 
	int _SkillExp  { get {if (skEntiyt == null) return 0; return (int) skEntiyt.GetAttribute(AttribType.Exp);} } 
	int _SkillExpMax  { get {if (skEntiyt == null) return 0; return (int) skEntiyt.GetAttribute(AttribType.ExpMax);} } 

	// Tip CD
 	float _TipCooldown = 3;
	float _curTipCooldown = 1.0f;

	protected override void InitWindow ()
	{
		base.InitWindow ();
		skillMgr = GameUI.Instance.mMainPlayer.GetCmpt<SkillTreeUnitMgr>();
		skEntiyt = GameUI.Instance.mMainPlayer.GetCmpt<SkAliveEntity>();
		Refresh(TableState.tb_Work);
		SkillTreeInfo.SetUICallBack(RefreshTypeData);
		Invoke( "ResetScrollValue" ,0.2f);

		mSkillTypeCtrl.onSetBtnActive += OnSkillTypeBtnActive;

		mSkillTypeCtrl.SetActiveBtn(0);

	}

	void Refresh(TableState state)
	{
		if (mTableState != state)
		{
			mTableState = state;

			if (SkillTreeInfo.SkillMainTypeInfo.ContainsKey(pageIndex))
			{
				mSkillTypeDatas.Clear();
//				int count = 0;
				for (int i = 0; i < SkillTreeInfo.SkillMainTypeInfo[pageIndex].Count; i++)
				{
					int main_type = SkillTreeInfo.SkillMainTypeInfo[pageIndex][i]._mainType;
					SkillMainType mainType = SkillTreeInfo.SkillMainTypeInfo[pageIndex].Find( itr => itr._mainType ==  main_type);
					UISkillType.SkillTypeData skillTypeData = new UISkillType.SkillTypeData( SkillTreeInfo.GetUIShowList(mainType._mainType,skillMgr), mainType );
					mSkillTypeDatas.Add(skillTypeData);
				}


				mSkillTypeCtrl.SetContent(SkillTreeInfo.SkillMainTypeInfo[pageIndex].Count, OnSetSkillTypeBtnContent);

				mSkillTypeCtrl.SetActiveBtn(0);
			}
		}

		UpdateSkillTypePos();
	}

	void OnSetSkillTypeBtnContent(UISkillTypeBtn btn)
	{
		btn.spriteName = mSkillTypeDatas[btn.index].info._icon[0];
	}

	void RefreshTypeData(int maintype)
	{

		int index = mSkillTypeDatas.FindIndex(itr=>itr.info._mainType == maintype); 
		if (index != -1)
		{
			mSkillTypeDatas[index].data = SkillTreeInfo.GetUIShowList (maintype, skillMgr);
		}


		if (mActiveSkillType != null)
		{
			SkillMainType mainType = SkillTreeInfo.SkillMainTypeInfo[pageIndex].Find( itr => itr._mainType ==  mActiveSkillType.data.info._mainType);

			if (mainType != null)
			{
				UISkillType.SkillTypeData skillTypeData = new UISkillType.SkillTypeData( SkillTreeInfo.GetUIShowList(mainType._mainType,skillMgr), mainType );
				mActiveSkillType.data = skillTypeData;
			}
		}

		if (onRefreshTypeData != null)
			onRefreshTypeData(this);
	}


	int pageIndex {get {return (int)mTableState;}}

	void CreateSkillTypes()
	{
		if (!SkillTreeInfo.SkillMainTypeInfo.ContainsKey(pageIndex))
			return;
		ClearSkillTypes();

		for (int i=0 ; i<SkillTreeInfo.SkillMainTypeInfo[pageIndex].Count;i++ )
		{
			UISkillType type = InstantiateSkillType();
			type.mainType = SkillTreeInfo.SkillMainTypeInfo[pageIndex][i]._mainType;

			mSkillTypeList.Add(type);
		}
		ResetScrollValue();
	}
	

	void ResetScrollValue()
	{
		mScrollBar.scrollValue = 0;
	}

	void UpdateSkillTypePos()
	{
//		float height = 0;
//		float lastHeight = 0;
//		foreach (UISkillType uiType in mSkillTypeList)
//		{
//			height -= (uiType.height + lastHeight) / 2;
//			uiType.transform.localPosition = new Vector3(0,height,0);
//			lastHeight = uiType.height;
//		}
//		if (mActiveSkillType == null)
//			return;
//		float height = (mActiveSkillType.height) / 2;
//		mActiveSkillType.transform.localPosition = new Vector3(0,height,0);
		if (mActiveSkillType != null)
		{
			Vector3 pos = mActiveSkillType.transform.localPosition;
			pos.y = -mActiveSkillType.height /2;
			mActiveSkillType.transform.localPosition = pos;
		}
	}

	void ClearSkillTypes()
	{
		foreach (UISkillType type in mSkillTypeList)
		{
			GameObject.Destroy(type.gameObject);
			type.transform.parent = null;
		}
		mSkillTypeList.Clear();
	}



	UISkillType InstantiateSkillType()
	{
		GameObject obj = GameObject.Instantiate(mSkillTypePrefab) as GameObject;
		obj.transform.parent = mContent;
		obj.transform.localPosition = Vector3.zero;
		obj.transform.localScale = Vector3.one;
		UISkillType type = obj.GetComponent<UISkillType>();
		type.onSkillItemLernBtn += OnSkillItemLernClick;
		return type;
	}

	

	void RefreshData()
	{
		if (!SkillTreeInfo.SkillMainTypeInfo.ContainsKey(pageIndex) || skillMgr == null )
			return;

		foreach (UISkillType uiType in mSkillTypeList)
		{
			SkillMainType mainType = SkillTreeInfo.SkillMainTypeInfo[pageIndex].Find( itr => itr._mainType ==  uiType.mainType);
			if (mainType == null)
				continue;

			UISkillType.SkillTypeData skillTypeData = new UISkillType.SkillTypeData( SkillTreeInfo.GetUIShowList(mainType._mainType,skillMgr), mainType );
			uiType.data = skillTypeData;
		}
	}
	

	#region UI_CALLBACK
	void BtnLiveOnClick()
	{
		Refresh(TableState.tb_Live);
	}

	void BtnFightOnClick()
	{
		Refresh(TableState.tb_Fight);
	}

	void BtnWorkOnClick()
	{
		Refresh(TableState.tb_Work);
	}
	


	UISkillItem _prevLernItem;
	void OnSkillItemLernClick(UISkillItem item)
	{
		SKTLearnResult r = _SkillMgr.SKTLearn(item.data._skillType);
		if (r == SKTLearnResult.SKTLearnResult_DontHaveEnoughExp)
		{
			if (_curTipCooldown >= _TipCooldown)
			{
				new PeTipMsg(PELocalization.GetString(8000159), PeTipMsg.EMsgLevel.Warning, PeTipMsg.EMsgType.Misc);
				_curTipCooldown = 0;
			}

		}
	}

	void OnSkillItemSelectChanged (UISkillItem item)
	{
		if (item != null)
		{
			try
			{
				//Parse desc
				string desc = item.data._desc.Substring(item.data._desc.LastIndexOf("\\n") + 2) 
					+ "\r\n" + PELocalization.GetString(8000160) + "[5CB0FF]" + GameUI.Instance.mSkillWndCtrl._SkillMgr.GetNextExpBySkillType(item.data._skillType) + "[-]";
				mSkillTypeCtrl.desc = desc;
			}
			catch
			{
				mSkillTypeCtrl.desc = "";
			}
		}
		else
			mSkillTypeCtrl.desc = "";
	}	

	#endregion

	#region OTHER_CALLBACK

	void OnSkillTypeBtnActive (UISkillTypeBtn btn)
	{
		if (mActiveSkillType == null)
		{
			GameObject go = mSkillTypePrefab.CreateNew(mContent);
			UISkillType type = go.GetComponent<UISkillType>();
			type.onSkillItemLernBtn += OnSkillItemLernClick;
			type.onSelectItemChanged += OnSkillItemSelectChanged;
			mActiveSkillType = type;
		}

		
		mActiveSkillType.data = mSkillTypeDatas[btn.index];
		mSkillTypeCtrl.desc = mActiveSkillType.data.info._desc;

		// Set Position
		UpdateSkillTypePos();
	


		if (mActiveSkillType != null)
			mActiveSkillType.selectItem = null;
	}

	#endregion

	#region UNITY_INNER_FUNC

	void Update()
	{
		mLbExp.text = _SkillExp.ToString() + "/" + _SkillExpMax.ToString();
		mSliderExp.sliderValue = (_SkillExpMax == 0) ? 0 :  Convert.ToSingle(_SkillExp) / Convert.ToSingle(_SkillExpMax);

		if (_curTipCooldown < _TipCooldown)
			_curTipCooldown += Time.deltaTime;
	}

	void OnGUI()
	{
		if (Application.isEditor)
		{
			if (GUI.Button(new Rect(150, 50, 80, 50), "Max Exp"))
			{
				skEntiyt.SetAttribute(AttribType.Exp, _SkillExpMax);
			}
		}
	}
    #endregion

    //lz-2016.11.04 跳转到帮助界面,选中技能树第一个帮助
    private void OnHelpBtnClick()
    {
        if (null != GameUI.Instance && null != GameUI.Instance.mPhoneWnd)
        {
            GameUI.Instance.mPhoneWnd.Show(UIPhoneWnd.PageSelect.Page_Help);
            GameUI.Instance.mPhoneWnd.mUIHelp.ChangeSelect(TutorialData.SkillIDs[0]);
        }
    }
}
