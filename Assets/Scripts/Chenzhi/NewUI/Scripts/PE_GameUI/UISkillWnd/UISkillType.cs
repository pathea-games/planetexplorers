using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UISkillType : MonoBehaviour 
{
	public class SkillTypeData
	{
		public Dictionary<int,List<SkillTreeUnit>> data {get;set;}
		public SkillMainType info {get;set;}

		public SkillTypeData(Dictionary<int,List<SkillTreeUnit>> _data, SkillMainType _info)
		{
			data = _data;
			info = _info;
		}
	}

	SkillTypeData mData = null;
	public SkillTypeData data { get { return mData;}  set { mData = value; Refresh();} }
	public int mainType {get;set;}  // OnCreate Set SkillMainType._mainType 

	[SerializeField] GameObject mSkillItemPrefab;
	[SerializeField] UISkillGrade mGrade_1;
	[SerializeField] UISkillGrade mGrade_2;
	[SerializeField] UISkillGrade mGrade_3;
	[SerializeField] UILabel mLbName;
	[SerializeField] UISprite mSprInfo;
	[SerializeField] UISprite mSprLine;

	public delegate void DNotify (UISkillItem item);
	public event DNotify onSkillItemLernBtn;
	public event DNotify onSelectItemChanged;

	UISkillItem mSelectItem = null;

	public UISkillItem selectItem  
	{
		get {return mSelectItem;} 

		set
		{
			if (value != mSelectItem)
			{
				if (mSelectItem != null)
					mSelectItem.SetSelect(false);

				mSelectItem = value;
				if (mSelectItem != null)
					mSelectItem.SetSelect(true);

				if (onSelectItemChanged != null)
					onSelectItemChanged(mSelectItem);
			}
		}
	} 


	public float height 
	{
		get 
		{
			if (mData == null)
                return 362f;
			int maxCount = 0;
			foreach (List<SkillTreeUnit> list in mData.data.Values)
			{
				if (maxCount < list.Count)
					maxCount = list.Count;
			}

		    //return (maxCount > 3 ) ?  maxCount * UISkillGrade.c_SkillItemSpace * 2 + UISkillGrade.c_SkillItemSpace : 130;
            return Mathf.Max(maxCount * UISkillGrade.c_SkillItemSpace * 2, 362f);

        } 
	}
	
	//bool bCreate = false;

	void Awake ()
	{
		mGrade_1.onSkillItemClick += OnSkillItemClick;
		mGrade_1.onSkillItemLernBtn += OnSkillItemLernBtn;
		mGrade_2.onSkillItemClick += OnSkillItemClick;
		mGrade_2.onSkillItemLernBtn += OnSkillItemLernBtn;
		mGrade_3.onSkillItemClick += OnSkillItemClick;
		mGrade_3.onSkillItemLernBtn += OnSkillItemLernBtn;
	}

	void Refresh()
	{
		if (mData == null)
			return ;

//		if (!bCreate)
//		{
//			if (Create())
//				bCreate = true;
//			else 
//				return;
//		}

		Create();

		RefreshSkillType();
		RefreshData();


		mSprLine.transform.localPosition = new Vector3(0,-height/2,0);
	}

	public void RefreshData()
	{

		mGrade_1.RefreshData(mData);
		mGrade_2.RefreshData(mData);
		mGrade_3.RefreshData(mData);

	}

	void RefreshSkillType()
	{
		if (mData.info == null)
			return ;
		
		mLbName.text = mData.info._desc;
		
		int iconCount = mData.info._icon.Count;
		if (iconCount >0)
			mSprInfo.spriteName = mData.info._icon[0];
		else 
			mSprInfo.enabled = false;
		
		if (iconCount >1)
			mGrade_1.mIcon.spriteName = mData.info._icon[1];
		else 
			mGrade_1.mIcon.enabled = false;
		
		if (iconCount >2)
			mGrade_2.mIcon.spriteName = mData.info._icon[2];
		else 
			mGrade_2.mIcon.enabled = false;
		
		if (iconCount >3)
			mGrade_3.mIcon.spriteName = mData.info._icon[3];
		else 
			mGrade_3.mIcon.enabled = false;
		
		mSprInfo.MakePixelPerfect();
		mGrade_1.mIcon.MakePixelPerfect();
		mGrade_2.mIcon.MakePixelPerfect();
		mGrade_3.mIcon.MakePixelPerfect();
	}

	bool Create()
	{
		if (mData.data == null )
			return false;

		if (mData.data.ContainsKey(1))
		{
			mGrade_1.SetContent(1, mData.data[1]);
		}
		else 
		{
			mGrade_1.transform.gameObject.SetActive(false);
			mGrade_2.transform.gameObject.SetActive(false);
			mGrade_3.transform.gameObject.SetActive(false);
			return true;
		}

		if (mData.data.ContainsKey(2))
		{
			mGrade_2.SetContent(2, mData.data[2]);
		}
		else 
		{
			mGrade_2.transform.gameObject.SetActive(false);
			mGrade_3.transform.gameObject.SetActive(false);
			return true;
		}

		if (mData.data.ContainsKey(3))
		{
			mGrade_3.SetContent(3, mData.data[3]);
		}
		else 
		{
			mGrade_3.transform.gameObject.SetActive(false);
			return true;
		}

		return true;
	}

	void OnSkillItemLernBtn(UISkillItem item)
	{
		if (onSkillItemLernBtn != null)
			onSkillItemLernBtn(item);
	}
		
	void OnSkillItemClick(UISkillItem item)
	{
		selectItem = item;
	}



}
