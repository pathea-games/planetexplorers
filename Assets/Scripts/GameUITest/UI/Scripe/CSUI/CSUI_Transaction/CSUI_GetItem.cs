using UnityEngine;
using System.Collections;
using ItemAsset;

public class CSUI_GetItem : MonoBehaviour {

	// Use this for initialization
	[SerializeField] UISprite mItemSprite;
	[SerializeField] UILabel mPerviteNumLb;

	public UIInput mInputNuLabel;

	float 		mCurrentNum;

	public float CurrentNum
	{
		get 
		{
			return mCurrentNum;
		}
		set 
		{
			mCurrentNum = value ;
			mInputNuLabel.text = mCurrentNum.ToString();
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

	public void SetCurrentNum(int MaxNum)
	{
		mCurrentNum = MaxNum;
		mInputNuLabel.text = mCurrentNum.ToString();
	}


	public void SetTexture (string  icon)
	{
		mItemSprite.spriteName = icon;
		mItemSprite.MakePixelPerfect();
	}

	// Update is called once per frame



	void Update () 
	{   
		if("" == mInputNuLabel.text)
			mCurrentNum = 0;
		else
			mCurrentNum = System.Convert.ToInt32(mInputNuLabel.text);
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
