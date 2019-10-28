using UnityEngine;
using System.Collections;
using ItemAsset;

public class CSUI_PageGrid : MonoBehaviour
{

    public delegate void ChickItem(object sender);
    public event ChickItem e_ItemClick = null;

    // Use this for initialization
    public UISprite[] mContentSprite;
    [SerializeField]
    UILabel mMaxNumLb;
    [SerializeField]
    GameObject mGridSeclect;

    private UICheckbox m_CheckBox;

    GridInfo m_GridInfo;
    public GridInfo mGridInfo
    {
        get
        {
            return m_GridInfo;
        }
        set
        {
            m_GridInfo = value;
            InitGird();

        }
    }

    int m_ProtoId;
    public int ProtoId
    {
        get
        {
            return m_ProtoId;
        }
        set
        {
            m_ProtoId = value;
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
            mType = value;
        }
    }

    int mMaxNum;
    public int MaxNum
    {
        get
        {
            return mMaxNum;
        }
        set
        {
            mMaxNum = value;
            mMaxNumLb.text = mMaxNum.ToString();
        }
    }

    void Awake()
    {
        InitWnd();
    }

    void Start()
    {

    }

    void InitWnd()
    {
        this.m_CheckBox = this.GetComponent<UICheckbox>();
        if (null != this.m_CheckBox)
        {
            this.m_CheckBox.radioButtonRoot = transform.parent;
        }
    }

    void InitGird()
    {
        if (m_GridInfo != null)
        {
            setIcon(m_GridInfo.IconName);
            SetMaxNum(m_GridInfo.MaxNum);

            m_ProtoId = m_GridInfo.mProtoId;
        }
    }

    void SetMaxNum(int max)
    {
        mMaxNum = max;
        mMaxNumLb.text = mMaxNum.ToString();
    }


    public void ShowGridSeclect(bool show)
    {
        if (null == this.m_CheckBox)
        {
            this.mGridSeclect.gameObject.SetActive(show);
        }
        else
        {
            this.m_CheckBox.isChecked = show;
        }
    }

    public void setIcon(string[] icon0)
    {
        if (mContentSprite.Length == 0)
            return;

        mContentSprite[1].spriteName = icon0[0];
        mContentSprite[1].MakePixelPerfect();
    }

    public void SetCotent(string[] ico)
    {

        if (mContentSprite.Length == 0)
            return;

        for (int i = 0; i < ico.Length; i++)
        {
            if (mContentSprite[i] != null)
            {
                if (ico[i] == "0")
                {
                    mContentSprite[i].gameObject.SetActive(false);
                }
                else
                {
                    mContentSprite[i].spriteName = ico[i];
                    mContentSprite[i].gameObject.SetActive(true);
                }
            }
        }
    }
    // Update is called once per frame
    void Update()
    {

    }

    void OnClickItem()
    {
        if (e_ItemClick != null)
        {
            e_ItemClick(this);
        }
    }

    void OnActivate(bool active)
    {
        mGridSeclect.gameObject.SetActive(active);
    }

    ItemObject _itemObj = null;
    void OnTooltip(bool show)
    {
        if (ListItemType.mItem == mType)
        {
            if (show == true && mItemSample == null && m_ProtoId != 0)
                mItemSample = new ItemSample(m_ProtoId);
            else if (show == false)
                mItemSample = null;

            if (mItemSample != null)
            {
                //string  tipStr = PELocalization.GetString(mItemSample.protoData.descriptionStringId);
                //ToolTipsMgr.ShowText(tipStr);
                _itemObj = ItemMgr.Instance.CreateItem(m_ProtoId);
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
