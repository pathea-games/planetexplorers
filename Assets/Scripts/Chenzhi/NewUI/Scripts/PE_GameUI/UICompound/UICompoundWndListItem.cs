using UnityEngine;
using System.Collections;
using ItemAsset;
using SkillAsset;
using System;

public class UICompoundWndListItem : MonoBehaviour
{
    // Use this for initialization
    public delegate void ClickFunc(int index);
    public event ClickFunc mItemClick = null;


    public UIAtlas mAtlasUI;
    public UIAtlas mAtlasIcon;

    public UILabel mText;
    public UILabel mItemName;
    public UITexture mContentTexture;
    public UISprite[] mContentSprites;
    public UISlicedSprite mItemLine;

    public UISprite mNewMask;
    public int mItemId;
    public UISlicedSprite mMouseSprite;
    public UISlicedSprite mSelectedSprite;
    public BoxCollider mIcoCollider;
    public BoxCollider mRinghtCollider;
    public GameObject mPanel;
    public GameObject mScrolBox;

    public bool isSelected = false;
    public int mIndex;

    public ListItemType mType;

    private ItemSample mItemSample;
    private bool IsEnableCollider = false;
    private int mPaneHeight;

    public void SetItem(string itemName, int id, bool bNew, string[] strContentSprite, string text, int index, ListItemType itemType)
    {
        mItemName.text = itemName;
        mItemId = id;
        mText.text = text;
        SetCotent(strContentSprite);
        mIndex = index;
        mType = itemType;

        //		Pathea.Replicator r = UIGraphControl.GetReplicator();
        //		if (null != r)
        //		{
        //			Pathea.Replicator.KnownFormula kf = r.GetKnownFormula(skillId);
        //			mNewMask.enabled = kf.flag;
        //		}

        mNewMask.enabled = bNew;

    }


    public void SetTextColor(Color mColor)
    {
        mText.color = mColor;
    }

    private void ActiveBoxClider(bool isActive)
    {
        mIcoCollider.enabled = isActive;
        mRinghtCollider.enabled = isActive;
        mItemName.enabled = isActive;
        mItemLine.enabled = isActive;
    }


    private void SetCotent(string[] sprNameList)
    {
        if (mContentSprites == null || mContentSprites.Length <= 0)
            return;
        for (int i = 0; i < sprNameList.Length; i++)
        {
            if (i < mContentSprites.Length)
            {
                if (sprNameList[i] == "0")
                {
                    mContentSprites[i].gameObject.SetActive(false);
                }
                else
                {
                    mContentSprites[i].spriteName = sprNameList[i];
                    mContentSprites[i].gameObject.SetActive(true);
                }
            }
        }
    }

    private void SetCotent(Texture _contentTexture)
    {
        mContentTexture.mainTexture = _contentTexture;
        mContentTexture.gameObject.SetActive(true);
        if (mContentSprites == null || mContentSprites.Length <= 0)
            return;
        for (int i = 0; i < mContentSprites.Length; i++)
        {
            mContentSprites[i].gameObject.SetActive(false);
        }
        
    }

    void OnMouseOver()
    {
        if (isSelected == false)
            mMouseSprite.enabled = true;
    }

    void OnMoseOut()
    {
        if (isSelected == false)
            mMouseSprite.enabled = false;
    }

    void OnClick()
    {
        if (mItemClick != null)
            mItemClick(mIndex);
        mNewMask.enabled = false;
    }


    public void SetSelectmState(bool isSelected)
    {
        if (isSelected)
        {
            isSelected = true;
            mMouseSprite.enabled = false;
            mSelectedSprite.enabled = true;
        }
        else
        {
            isSelected = false;
            mSelectedSprite.enabled = false;
        }
    }

    void Start()
    {
        UIScrollBox sb = mScrolBox.GetComponent<UIScrollBox>();
        mPaneHeight = sb.m_Height;
        ActiveBoxClider(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (isShowInPanel() == true && IsEnableCollider == false)
        {
            ActiveBoxClider(true);
            IsEnableCollider = true;
        }
        else if (isShowInPanel() == false && IsEnableCollider == true)
        {
            ActiveBoxClider(false);
            IsEnableCollider = false;
        }
    }

    bool isShowInPanel()
    {
        float panelPos_y = mPanel.transform.localPosition.y - 3;
        float ItemPos_y = this.gameObject.transform.localPosition.y;
        float resoult = Math.Abs(ItemPos_y) - panelPos_y;
        if (resoult >= -3 && resoult <= mPaneHeight)
            return true;
        return false;
    }

    ItemObject _itemObj = null;

    void OnTooltip(bool show)
    {

        if (ListItemType.mItem == mType)
        {
            if (show == true && mItemSample == null && mItemId != 0)
                mItemSample = new ItemSample(mItemId);
            else if (show == false)
                mItemSample = null;

            if (mItemSample != null)
            {
                //string tipStr = PELocalization.GetString(mItemSample.protoData.descriptionStringId);

                //string _eName = mItemSample.protoData.englishName;
                //int _itemObjId = ItemProto.Mgr.Instance.GetItemObjProtoId(_eName, mItemSample.protoData.id);
                //if (_itemObjId != -1)
                //{
                //   ItemObject _itemObj =ItemMgr.Instance.CreateItem(_itemObjId);
                //   tipStr = _itemObj.GetTooltip();
                //   ToolTipsMgr.ShowText(tipStr);
                //}
                //else
                //    ToolTipsMgr.ShowText(tipStr);

                _itemObj = ItemMgr.Instance.CreateItem(mItemId);
                string tipStr = _itemObj.GetTooltip();
                ToolTipsMgr.ShowText(tipStr);
            }
            else
            {
                ItemMgr.Instance.DestroyItem(_itemObj);
                ToolTipsMgr.ShowText(null);
            }
        }
    }

}