using UnityEngine;
using System.Collections;
using System;

public class UIBuildWndItem : MonoBehaviour 
{
	#region enum
	const string AtlasType_button = "Button";
	const string AtlasType_icon = "Icon";
	
	public enum ItemType
	{
		mNull,
		mBlockMat,
		mBlockPattern,
		mVoxelMat,
		mVoxelType,
		mIso,
		mCost,
		mMenu
	}
	#endregion
	
	public delegate void EventFunc(ItemType _ItemType,int _Index);

	public event EventFunc BeginDrag = null;
	public event EventFunc Drag = null;
	public event EventFunc Drop = null;
	public event EventFunc ClickItem = null;
	public event EventFunc OnGetDrag = null;

	public delegate void ToolToipFunc(bool isShow,ItemType _ItemType,int _Index);
	public event ToolToipFunc ToolTip = null;


	public int mIndex = -1;
	public ItemType mItemType = ItemType.mNull;
	public int mTargetIndex = -1;
	public int mSubsetIndex = -1;
	public ItemType mTargetItemType = ItemType.mNull;


	public bool mCanDrag = false;
	public bool mCanGetDrag = false;
	
	public UIAtlas mAtlasIcon;
	
	public UIFilledSprite mSkillCoold;
	public UISprite mSpriteSelect;
	public UILabel mNumber;
	public UILabel mText;
	public UILabel mTextIndex;
	public UISprite mBgSprite;
	public UISprite mIndexSprite;
	public UISprite mDeActiveSprite;
    [SerializeField]
    private UISprite m_QuickBarSprite;
    
	

	public UISprite mContentSprite;
	public UITexture mContentTexture;

	[SerializeField] GameObject gridEffectPrefab; 

	private int mItemId = 0;
	public int ItemId  { get { return mItemId; }}

	public string atlas = "";

	private bool IsOnDrag = false;

	// Aactive 
	private bool mActive = true;

	public bool IsActive
	{
		get { return mActive; }

		set
		{
			if (mDeActiveSprite != null)
				mDeActiveSprite.enabled = !value;
			mActive = value;
		}
	}


	public void SetItemID(int _ItemID)
	{
		mItemId = _ItemID;
	}

	public void GetDrag(ItemType _targetItemType, int _targetIndex, string _sprName, string strAtlas = "Button")
	{
		if(mCanGetDrag == false)
			return;
		SetCotent( _sprName, strAtlas);
		mTargetItemType = _targetItemType;
		mTargetIndex = _targetIndex;
		
		if(OnGetDrag != null)
			OnGetDrag(mItemType,mIndex);
	}

	public void GetDrag(ItemType _targetItemType, int _targetIndex,Texture _contentTexture)
	{
		if(mCanGetDrag == false)
			return;
		SetCotent(_contentTexture);
		mTargetItemType = _targetItemType;
		mTargetIndex = _targetIndex;
		
		if(OnGetDrag != null)
			OnGetDrag(mItemType,mIndex);
	}

	public void SetNullContent()
	{
		mTargetIndex = -1;
		mTargetItemType  = ItemType.mNull;

		SetCotent("Null","Button");
	}
	

	public void SetText(string _text)
	{
		if(mText != null)
			mText.text = _text;
	}

	public void SetNumber(string _number)
	{
		if(mNumber != null)
			mNumber.text = _number; 
	}


	public void SetTextIndex(string _index)
	{
		if(mTextIndex != null)
			mTextIndex.text = _index; 
	}
	public void SetSpriteIndex (string _index)
	{
		if (mIndexSprite != null)
		{
			mIndexSprite.spriteName = "num_" + _index.ToString();
		}
        //lz-2016.08.12 设置快捷键显示
        if(null!=this.m_QuickBarSprite)
        {
            this.m_QuickBarSprite.spriteName = "QuickKey_"+_index.ToString();
        }
	}

	public void SetSelect(bool _IsSelect)
	{
		if(_IsSelect)
		{
			mSpriteSelect.gameObject.SetActive(true);
			mBgSprite.color = new Color(0, 1, 0, 1);
		}
		else
		{
			mSpriteSelect.gameObject.SetActive(false);
			mBgSprite.color = new Color(255,255,255,255);
		}
	}



	public void InitItem(ItemType _type, int _index)
	{
		mItemType = _type;
		mIndex = _index;
	}

	public void InitItem(ItemType _type, string _sprName, string strAtlas, int _index)
	{
        mIndex = _index;
        mItemType = _type;
        //lz-2016.08.30 设置图片从这个接口走，快捷键提示状态
        SetCotent(_sprName, strAtlas);
		if (mDeActiveSprite != null)
			mDeActiveSprite.enabled = !mActive;
	}

	public void InitItem(ItemType _type, Texture _contentTexture,int _index)
	{
		mItemType = _type;
		mContentSprite.gameObject.SetActive(false);
		mContentTexture.mainTexture = _contentTexture;
		mIndex = _index;

		if (mDeActiveSprite != null)
			mDeActiveSprite.enabled = !mActive;

        //lz-2016.08.30 显示和隐藏快捷键提示
        if (null != m_QuickBarSprite)
        {
            m_QuickBarSprite.enabled = (null!=_contentTexture);
        }
    }


	public void SetCotent(string _sprName, string strAtlas)
	{
		SetAtlas(strAtlas);
		mContentSprite.spriteName = _sprName;
		mContentSprite.gameObject.SetActive(true);
		mContentTexture.gameObject.SetActive(false);

		if (mDeActiveSprite != null)
		{
			if ("Null" == _sprName)
				mDeActiveSprite.enabled = false;
			else
				mDeActiveSprite.enabled = !mActive;
		}

        //lz-2016.08.12 显示和隐藏快捷键提示
        if (null != m_QuickBarSprite)
        {
            m_QuickBarSprite.enabled= (!_sprName.Equals("Null"));
        }
	}
	
	private void SetCotent(Texture _contentTexture)
	{
		mContentTexture.mainTexture = _contentTexture;
		mContentTexture.gameObject.SetActive(true);
		mContentSprite.gameObject.SetActive(false);

		if (mDeActiveSprite != null)
		{
			mDeActiveSprite.enabled = !mActive;
		}
	}


	private void SetAtlas(string type)
	{
//		if(atlas == type)
//			return;
		mContentSprite.atlas = mAtlasIcon ;
	}


	#region Grid_Effect
	
	PeUIEffect.UIGridEffect effect;
    //bool moveIn = false;
    //float moveInTime = 0;
    private GameObject _effectGo = null;
	
	public void PlayGridEffect()
	{
		if (gridEffectPrefab == null)
			return;
		if (_effectGo != null)
		{
			Destroy(_effectGo);
			effect = null;
		}
		
		_effectGo = GameObject.Instantiate(gridEffectPrefab) as GameObject;
		_effectGo.transform.parent = this.transform.parent;
		_effectGo.transform.localPosition = new Vector3(transform.localPosition.x,transform.localPosition.y, -5);
		_effectGo.transform.localScale = new Vector3(48,48,1);
		effect = _effectGo.GetComponentInChildren<PeUIEffect.UIGridEffect>();
		if (effect != null)
			effect.e_OnEnd += EffectEnd;
	}
	
	void EffectEnd(PeUIEffect.UIEffect _effect)
	{
		effect = null;
	}
	
	#endregion

	
	void Awake()
	{
	}

	// Use this for initialization
	void Start () 
	{

	}
	
	// Update is called once per frame
	void Update () 
	{
		if(IsOnDrag == true && Input.GetMouseButtonUp(0))
			OnDrop();
	}


	void ItemOnClick()
	{
		if( ClickItem != null)
			ClickItem(mItemType, mIndex);
	}



	void OnBeginDrag()
	{
		if(BeginDrag != null)
			BeginDrag(mItemType,mIndex);

		mSkillCoold.fillAmount = 1;

		IsOnDrag = true ;
	}

	void OnDrag()
	{
		if(Input.GetMouseButton(0))
		{
			if(mCanDrag == false)
				return;

			if(IsOnDrag == false)
			{
				OnBeginDrag();
			}

			if(Drag != null)
				Drag(mItemType,mIndex);
		}
	}

	void OnDrop()
	{
		mSkillCoold.fillAmount = 0;

		if(Drop != null)
			Drop(mItemType,mIndex);

		IsOnDrag = false;

	}



	void OnTooltip (bool show)
	{
		if (ToolTip != null)
			ToolTip(show,mItemType,mIndex);
	}

}
