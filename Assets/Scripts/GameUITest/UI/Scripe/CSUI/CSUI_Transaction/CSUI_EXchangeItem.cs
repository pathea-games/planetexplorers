using UnityEngine;
using System.Collections;
using ItemAsset;

public class CSUI_EXchangeItem : MonoBehaviour {


	[SerializeField] UILabel mMaxNumLb;
	[SerializeField] UISprite mItemSprit;
	[SerializeField] UILabel mPerviteNumLb;

	public UIInput mInputNuLabel;

	//[SerializeField] UILabel mCurrentNumLb;
	// Use this for initialization


	int m_CurmaxNum;
	int m_MaxNum;
	public int MaxNum
	{
		get 
		{
			return m_MaxNum;
		}
		set 
		{
			m_MaxNum = value ;
		}
	}

	public float CurrentNum
	{
		get
		{
			return  mCurrentNum;
		}
	}

	int mprotoId;
	public int ProtoId
	{
		get
		{
			return mprotoId;
		}
		set
		{
			mprotoId = value ;
		}
	}

	private ItemSample mItemSample = null;
	private ListItemType mType;
	public ListItemType Type
	{
		get
		{
			return mType;
		}
		set
		{
			mType = value ;
		}
	}

	int mPackageNum;
	public int PackageNum
	{
		get
		{
			return mPackageNum;
		}
		set
		{
			mPackageNum = value;
			mPerviteNumLb.text = mPackageNum.ToString();
		}
	}

	void Start () 
	{
	
	}

	public void SetIcon (string  icon)
	{
		mItemSprit.spriteName = icon;
		mItemSprit.MakePixelPerfect();
	}

	public void SetMaxNum(int maxNum)
	{
		mMaxNumLb.text  ="X" + maxNum.ToString();
		m_MaxNum = maxNum;

	}

	// Update is called once per frame
	void Update () 
	{
		if(m_MaxNum > mPackageNum)
			m_CurmaxNum = mPackageNum;
		else
			m_CurmaxNum = m_MaxNum;
		
		if(mAddBtnPress)
		{
			float dT = Time.time - mOpStarTime;
			if(dT < 0.2f)
				mOpDurNum = 1;
			else if(dT < 1f)
				mOpDurNum += 2*Time.deltaTime;
			else if(dT < 2f)
				mOpDurNum += 4*Time.deltaTime;
			else if(dT < 3f)
				mOpDurNum += 7*Time.deltaTime;
			else if(dT < 4f)
				mOpDurNum += 11*Time.deltaTime;
			else if(dT < 5f)
				mOpDurNum += 16*Time.deltaTime;
			else
				mOpDurNum += 20*Time.deltaTime;
			

			if(m_CurmaxNum >= mOpDurNum + mCurrentNum)
			{
			  mInputNuLabel.text = ((int)(mOpDurNum + mCurrentNum)).ToString();
			}
			else
			{
				mInputNuLabel.text = m_CurmaxNum.ToString();
			}
		}
		else if(mSubBtnPress)
		{
			float dT = Time.time - mOpStarTime;
			if(dT < 0.5f)
				mOpDurNum = -1;
			else if(dT < 1f)
				mOpDurNum -= 2*Time.deltaTime;
			else if(dT < 2f)
				mOpDurNum -= 4*Time.deltaTime;
			else if(dT < 3f)
				mOpDurNum -= 7*Time.deltaTime;
			else if(dT < 4f)
				mOpDurNum -= 11*Time.deltaTime;
			else if(dT < 5f)
				mOpDurNum -= 16*Time.deltaTime;
			else
				mOpDurNum -= 20*Time.deltaTime;

			if(mOpDurNum + mCurrentNum > 0)
			{
			mInputNuLabel.text = ((int)(mOpDurNum + mCurrentNum)).ToString();
			}
			else 
			{
				mInputNuLabel.text = "0";
			}
		}

		if("" == mInputNuLabel.text)
			mCurrentNum = 0;
		else 
		{
			mCurrentNum = System.Convert.ToInt32(mInputNuLabel.text);
			if(mCurrentNum > m_CurmaxNum)
			{
				mInputNuLabel.text = m_CurmaxNum.ToString();
			}
		}

	}




	public void SetTheCurentNum(float curtent)
	{
		//CurrentNum = curtent;
		//mCurrentNumLb.text = CurrentNum.ToString();
	}

	void OnSubmit()
	{
		//CurrentNum = int.Parse(mCurrentNumLb.text);
	}
	void OnAddBtn()
	{
		//CurrentNum ++;
		//mCurrentNumLb.text = CurrentNum.ToString();
	}

	void OnSubtractBtn ()
	{
		//CurrentNum -- ;
		//mCurrentNumLb.text = CurrentNum.ToString();
	}



	bool mAddBtnPress = false;
	bool mSubBtnPress = false;

	float mOpStarTime;
	float 		mCurrentNum;
	float		mOpDurNum = 0;

	void OnAddBtnPress()
	{
		mAddBtnPress = true;
		mOpStarTime = Time.time;
		mOpDurNum = 0;
	}
	
	
	void OnAddBtnRelease()
	{
		mAddBtnPress = false;
		mCurrentNum = (int)(mCurrentNum + mOpDurNum);
		mOpDurNum = 0;

		if(mCurrentNum > m_CurmaxNum )
		{
			mCurrentNum = m_CurmaxNum;
		}
		mInputNuLabel.text = ((int)(mCurrentNum)).ToString();
		//mTotalLabel.text = (mPrice * (int)(mCurrentNum)).ToString();
	}
	
	void OnSubstructBtnPress()
	{
		mSubBtnPress = true;
		mOpStarTime = Time.time;
		mOpDurNum = 0;
	}
	
	void OnSubstructBtnRelease()
	{
		mSubBtnPress = false;
		mCurrentNum = (int)(mCurrentNum + mOpDurNum);
		mOpDurNum = 0;

		if(mCurrentNum < 0 )
		{
			mCurrentNum = 0;
		}

		mInputNuLabel.text = ((int)(mCurrentNum)).ToString();
		//mTotalLabel.text = (mPrice * (int)(mCurrentNum)).ToString();
	}

	void OnTooltip (bool show)
	{
		if(ListItemType.mItem == mType)
		{
			if(show == true && mItemSample == null  && mprotoId != 0)
				mItemSample = new ItemSample(mprotoId);
			else if(show == false)
				mItemSample = null;

			if(mItemSample != null)
			{
				string  tipStr = PELocalization.GetString(mItemSample.protoData.descriptionStringId);
				ToolTipsMgr.ShowText(tipStr);
			}
			else
			{
				ToolTipsMgr.ShowText(null);
			}
		}

	}
}
