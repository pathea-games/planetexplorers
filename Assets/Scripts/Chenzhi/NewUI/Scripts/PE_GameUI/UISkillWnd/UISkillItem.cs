using UnityEngine;
using System.Collections;

public class UISkillItem : MonoBehaviour 
{
	
	[SerializeField] UISprite mSprContent;
	[SerializeField] UISprite mSprLock;
	[SerializeField] UILabel mLbLevel; 
	[SerializeField] UIButton mBtnAddSkill;
	[SerializeField] GameObject mLeftLine;
	[SerializeField] UISlicedSprite mSelectSprite;

	public Color enableColor = Color.white;
	public Color disableColor = Color.white;

	SkillTreeUnit mData = null;

	public SkillTreeUnit data { get { return mData; } set {mData = value; Refresh();}}

	public int grade {get; private set;}
	public int index {get; private set;}

	public delegate void DNotify (UISkillItem item);
	public event DNotify onClickLernBtn;
	public event DNotify onClickItemBtn;

	public void SetCoord(int _grade , int _index)
	{
		grade = _grade;
		index = _index;
		mLeftLine.SetActive( (_grade != 1) );
	}

	public void SetSelect(bool select)
	{
		mSelectSprite.enabled = select;
	}


	bool isLock;
	public void Refresh()
	{
		if (mData == null)
			return ;

		isLock = (data._state == SkillState.Lock);
		mSprLock.enabled = isLock;
		mSprLock.enabled = false;
		mSprContent.spriteName = mData._sprName; 

		if (isLock)
			SetColor(disableColor);
		else
			SetColor(enableColor);

		if (data._state == SkillState.learnt)
		{
			
			if (data._level < data._maxLevel)
			{
					mLbLevel.text = data._level.ToString() + "/" + data._maxLevel.ToString();
				mBtnAddSkill.gameObject.SetActive(true);
			}
			else 
			{
				mLbLevel.text =  " Max" + data._maxLevel.ToString() ;
				mBtnAddSkill.gameObject.SetActive(false);
			}

		}
		else
		{
			mLbLevel.text = "-/" + data._maxLevel.ToString();
			if (isLock)
				mBtnAddSkill.gameObject.SetActive(false);				
			else 
				mBtnAddSkill.gameObject.SetActive(true);


		}
	}

	public void SetColor (Color color)
	{
		mSprContent.color = color;
		mLbLevel.color = color;
	}


	void OnBtnLern()
	{
//		GameUI.Instance.mSkillWndCtrl._SkillMgr.SKTLearn(data._skillType);

		if (onClickLernBtn != null)
			onClickLernBtn(this);
	}

	void OnClick()
	{
		if (onClickItemBtn != null)
			onClickItemBtn(this);
	}

	void OnTooltip (bool show)
	{
		if (mData == null)
			return ;

		string desc = mData._desc + "\r\n" + PELocalization.GetString(8000160) + "[5CB0FF]" + GameUI.Instance.mSkillWndCtrl._SkillMgr.GetNextExpBySkillType(mData._skillType) + "[-]"; 
		ToolTipsMgr.ShowText(desc);

	}
}
