using UnityEngine;
using System.Collections;


public enum WorkGridItemType
{
	mNull = -1,
	mWorkShop = 0,
	mLocalIcon = 1,
	mUpLoad = 2,
	mUpDown = 3,
	mLocalFloder = 4,
}


public class UIWorkShopGridItem : MonoBehaviour 
{

	public delegate void ClickFunc(WorkGridItemType mType,int index);

	public event ClickFunc mClickItem= null;
	public event ClickFunc mDoubleClickItem= null;
	public event ClickFunc mBtnReloadOnClick= null;

	public event ClickFunc mBtnCaiOnClick= null;
	public event ClickFunc mBtnDingOnClick= null;
	

	public UITexture mTextureContent;
	public UISprite mContentSprite;
	public UISprite mSeletedSprite;

	public UILabel mIsoName;
	public UILabel mIsoCreateName;

	public UILabel mLbDingText;
	public UILabel mLbCaiText;
	public UILabel mLbUpDownText;
	public UISprite mSpUpDown;
	public GameObject mUpDown;
	public Texture2D mUnkonwTexture;

	public GameObject[] mBtns;

	public WorkGridItemType mType = WorkGridItemType.mNull;

	public int index = -1;

    private bool m_DownLoad;    // Log:2016.05.17 已经下载标志
    private bool m_IsActiveLoading;
    //private bool m_ActiveUpDown;
    private string m_IsoFileName="";
    public string IsoFileName{get{return this.m_IsoFileName;}}
	

	public void ActiveUpDown(bool isActive)
	{
        //this.m_ActiveUpDown = isActive;
		mUpDown.SetActive(isActive);
	}

	public void UpdteUpDownInfo(string infoText)
	{
		mLbUpDownText.text = infoText;
	}

	public void InitItem(WorkGridItemType ItemItye,string _isoName)
	{
		mType = ItemItye;
		mIsoName.text = _isoName;
        this.UpdateIsoFileName(_isoName);
		SetUIState();
	}

	public void SetDingText(string strText)
	{
		mLbDingText.text = strText;
	}

	public void SetCaiText(string strText)
	{
		mLbCaiText.text = strText;
	}


	public void ActiveVoteUI(bool isActive)
	{
		for (int i= 0;i < mBtns.Length - 1;i++)// 保留BtnReload
			mBtns[i].SetActive(isActive);
	}
	
	public void SetIsoName(string _isoName)
	{
		mIsoName.text = _isoName;
        this.UpdateIsoFileName(_isoName);
	}

	public void SetAuthor(string _CreatorName)
	{
		if (mType == WorkGridItemType.mWorkShop)
		{
			mIsoCreateName.text = PELocalization.GetString(8000692)+":"+ _CreatorName;
		}
		else if (mType == WorkGridItemType.mUpLoad)
		{
			mIsoCreateName.text = PELocalization.GetString(8000692) + ":" + _CreatorName;
		}
		else if (mType == WorkGridItemType.mLocalIcon)
		{
			mIsoCreateName.text = PELocalization.GetString(8000693) + ":" + _CreatorName;
		}
		else
		{
			mIsoCreateName.enabled = false;
		}
	}

	public void SetIco(Texture2D texture)
	{
		if (texture == null)
			texture = mUnkonwTexture;

		if (mType == WorkGridItemType.mLocalIcon)
		{
			mTextureContent.enabled = true;
			mTextureContent.mainTexture = texture;
			mContentSprite.enabled = false;
		}
		else if ( mType ==WorkGridItemType.mLocalFloder )
		{
			mContentSprite.spriteName = "folder_icon";
			mContentSprite.enabled = true;
			mTextureContent.enabled = false;
		}
		else if (mType ==WorkGridItemType.mWorkShop )
		{
			mTextureContent.enabled = true;
			mTextureContent.mainTexture = texture;
			mContentSprite.enabled = false;
		}
		else if (mType ==WorkGridItemType.mUpLoad )
		{
			mTextureContent.enabled = true;
			mTextureContent.mainTexture = texture;
			mContentSprite.enabled = false;
		}
	}

	public void ActiveLoadingItem(bool isStar)
	{
		if (isStar)
		{
			mContentSprite.spriteName = "icoloading";
			mContentSprite.enabled = true;
			mTextureContent.enabled = false;
            this.SetDownloaded(false);
		}
		else
		{
			mContentSprite.enabled = false;
			mTextureContent.enabled = true;
		}

		m_IsActiveLoading = isStar;

	}
	
	public void SetSelected(bool Selected)
	{
		if(mSeletedSprite.enabled != Selected)
			mSeletedSprite.enabled = Selected;
	}

    public void SetDownloaded(bool download)
    {
        if (download)
        {
            ActiveUpDown(true);
            mSpUpDown.spriteName = "clouddown";
            mLbUpDownText.text = PELocalization.GetString(8000694);
        }
        else
        {
            mLbUpDownText.text = "";
            ActiveUpDown(false);
        }
    }

	void SetUIState()
	{
		if (mType == WorkGridItemType.mWorkShop)
		{
			mIsoCreateName.enabled = true;
			ActiveVoteUI(false);

			mSpUpDown.spriteName = "clouddown";

		}
		else if (mType == WorkGridItemType.mLocalIcon)
		{
			mIsoCreateName.enabled = true;

			for (int i= 0;i < mBtns.Length;i++)
				mBtns[i].SetActive(false);

			mSpUpDown.spriteName = "cloudup";
			
		}
		else if (mType == WorkGridItemType.mUpLoad)
		{
			mIsoCreateName.enabled = true;

			ActiveVoteUI(false);
			mBtns[2].SetActive(true);

			mSpUpDown.spriteName = "clouddown";
		}
		else if (mType == WorkGridItemType.mUpDown)
		{

		}
		else if (mType == WorkGridItemType.mLocalFloder)
		{
			mIsoCreateName.enabled = false;
			for (int i= 0;i < mBtns.Length;i++)
				mBtns[i].SetActive(false);

			ActiveUpDown(false);
		}

	}


	// Use this for initialization
	void Start () 
	{

	}


	void Update()
	{
		if (m_IsActiveLoading)
		{
			Vector3 angl = mContentSprite.transform.localEulerAngles;

			float z = angl.z -  Time.deltaTime * 200;
			mContentSprite.transform.localEulerAngles = new Vector3(angl.x,angl.y,z);
		}
	}

	void GridItemOnClick()
	{
		if (Input.GetMouseButtonUp(0) )
		{
			if (mClickItem != null)
				mClickItem(mType,index);
		}
	}

	void GridItemDoubleClick()
	{
		if (Input.GetMouseButtonUp(0) )
		{
			if (mDoubleClickItem != null)
				mDoubleClickItem(mType,index);
		}
	}

	void BtnReloadOnClick()
	{
        if (m_IsActiveLoading == true)
            return;

		if (Input.GetMouseButtonUp(0) )
		{
			if (mBtnReloadOnClick != null)
				mBtnReloadOnClick(mType,index);
		}
	}
	
	void BtnCaiOnClick()
	{
		if (Input.GetMouseButtonUp(0) )
		{
			if (mBtnCaiOnClick != null)
				mBtnCaiOnClick(mType,index);
		}
	}

	void BtnDingOnClick()
	{
		if (Input.GetMouseButtonUp(0) )
		{
			if (mBtnDingOnClick != null)
				mBtnDingOnClick(mType,index);
		}
	}

    void UpdateIsoFileName(string isoName)
    {
        this.m_IsoFileName = isoName + VCConfig.s_IsoFileExt;
    }

}
