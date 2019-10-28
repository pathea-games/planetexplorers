using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class UITalkBoxCtrl : MonoBehaviour
{
    public UISlicedSprite mWndBg;

    public BoxCollider mTopClider;
    public BoxCollider mRightClider;
    public BoxCollider mDrgPanelClider;

    public UISprite mTopTuodong;
    public UISprite mRightTuodong;

    public GameObject mBtnDown;
    public GameObject mBtnUp;
    public GameObject mBtnClear;
    public GameObject mBtnLock;
    public GameObject mBtnUnLock;

    public CommonChatItem_N m_ChatItemPrefab;
    public UITable m_ContentTable;
    public UIScrollBar mScrollBar;

    bool IsLock = true;

    bool IsOnDrapTop = false;
    bool IsOnDrapRight = false;


    public float mMinWidth;
    public float mMinHeight;
    public float mMoveHeight;
    public int FixedTimeCount = 40;
    public int LineWidth = 890;

    private int mMaxMsgCount = 300;
    private int mDeleteMsgCount = 100;
    private List<CommonChatItem_N> m_CurChatItems = new List<CommonChatItem_N>();
    private Queue<CommonChatItem_N> m_ChatItemsPool = new Queue<CommonChatItem_N>();
    private int m_RepositionCount = 0;

    public const string LANGE_CN = "[lang:cn]";
    public const string LANGE_OTHER = "[lang:other]";

    bool isScroll = false;

    public void AddMsg(string userName, string strMsg, string strColor)
    {
        //lz-2016.12.12 去除语言标记符号
        bool isChinese = strMsg.Contains(LANGE_CN);
        strMsg = strMsg.Replace(LANGE_CN, string.Empty);
        strMsg = strMsg.Replace(LANGE_OTHER, string.Empty);

        string text = "[" + strColor + "]" + userName + "[-]" + ":" + strMsg;

        CommonChatItem_N item = GetNewChatItem();
         if (null != item)
        {
            item.SetLineWidth(LineWidth);
            item.UpdateText(isChinese,text);
            m_CurChatItems.Add(item);
        }

        if (m_CurChatItems.Count > mMaxMsgCount)
        {
            RecoveryItems(mDeleteMsgCount);
        }

        m_RepositionCount = 3;
    }


    public void ClearInputBox()
    {
        RecoveryItems(m_CurChatItems.Count);
    }

    // Use this for initialization
    void Start()
    {
        //mScrollBar.onChange = OnScrollValueChage;


        //-------------------- text code ----------------------------------------------
        //		for (int i=0;i<100;i++)
        //		{
        //			AddMsg("User Name","just for text!","ffffff");
        //		}
        //------------------------------------------------------------------------------

    }

    void OnScrollValueChage(UIScrollBar bar)
    {
        if ((Mathf.Abs(Input.GetAxis("Mouse ScrollWheel")) > PETools.PEMath.Epsilon) || Input.GetMouseButtonDown(0))
        {
            isScroll = true;
            tempCount = 0;
        }
    }

    // Update is called once per frame
    void Update()
    {

        if (IsOnDrapTop)
            OnDrapTop();
        if (IsOnDrapRight)
            OnDrapRight();

        UpdateCliderTrans();

        if (IsWndMove)
            UpdateWndMove();

        UpdateScollValue();

        if (m_RepositionCount > 0)
        {
            m_ContentTable.Reposition();
            mScrollBar.scrollValue = 1;
            --m_RepositionCount;
        }
    }


    int tempCount = 0;
    void UpdateScollValue()
    {
        if (mScrollBar.scrollValue != 1 && isScroll == true)
        {
            tempCount++;
            if (tempCount >= 300)
            {
                isScroll = false;
                tempCount = 0;
                mScrollBar.scrollValue = 1;
            }

        }
    }


    private void UpdateCliderTrans()
    {
        Vector3 bgScale = mWndBg.gameObject.transform.localScale;

        if (mTopClider.size.y != bgScale.y)
        {
            mRightClider.gameObject.transform.localPosition = new Vector3(bgScale.x, 0, 0);
            mRightClider.center = new Vector3(0, bgScale.y / 2, -1);
            mRightClider.size = new Vector3(20, bgScale.y, 1);
        }

        if (mRightClider.size.x != bgScale.x)
        {
            mTopClider.gameObject.transform.localPosition = new Vector3(0, bgScale.y, -1);
            mTopClider.center = new Vector3(bgScale.x / 2, 0, -1);
            mTopClider.size = new Vector3(bgScale.x, 20, 1);
        }

    }


    void OnDrapTop()
    {

        float posx = Input.mousePosition.x - gameObject.transform.localPosition.x;
        Vector3 topPos = mTopTuodong.transform.localPosition;
        mTopTuodong.transform.localPosition = new Vector3(posx, topPos.y, topPos.z);

        if (Input.GetMouseButton(0))
        {
            float posy = Input.mousePosition.y - gameObject.transform.localPosition.y;
            Vector3 scale = mWndBg.gameObject.transform.localScale;
            if (posy > mMinHeight && posy < (Screen.height - 100))
                mWndBg.gameObject.transform.localScale = new Vector3(scale.x, posy, scale.z);
        }
    }


    void OnDrapRight()
    {
        float posy = Input.mousePosition.y - gameObject.transform.localPosition.y;
        Vector3 rightPos = mRightTuodong.transform.localPosition;
        mRightTuodong.transform.localPosition = new Vector3(rightPos.x, posy, rightPos.z);

        if (Input.GetMouseButton(0))
        {
            float posx = Input.mousePosition.x - gameObject.transform.localPosition.x;
            Vector3 scale = mWndBg.gameObject.transform.localScale;
            if (posx > mMinWidth && posx < (Screen.width - 10))
            {
                mWndBg.gameObject.transform.localScale = new Vector3(posx, scale.y, scale.z);
                LineWidth = Convert.ToInt32(posx - 70);
                SetCurItemsLineWidth(LineWidth);
            }
        }
    }

    void RightColiderOnMouseOver()
    {
        if (IsOnDrapRight == false && IsLock == false)
        {
            Cursor.visible = false;
            float posy = Input.mousePosition.y - gameObject.transform.localPosition.y;
            Vector3 rightPos = mRightTuodong.transform.localPosition;
            mRightTuodong.transform.localPosition = new Vector3(rightPos.x, posy, rightPos.z);
            mRightTuodong.enabled = true;
            IsOnDrapRight = true;
        }
    }


    void RightColiderOnMouseOut()
    {
        if (IsOnDrapRight == true && IsLock == false)
        {
            Cursor.visible = true;
            mRightTuodong.enabled = false;
            IsOnDrapRight = false;
            m_RepositionCount = 3;
        }
    }

    void TopColiderOnMouseOver()
    {
        if (IsOnDrapTop == false && IsLock == false)
        {
            Cursor.visible = false;
            float posx = Input.mousePosition.x - gameObject.transform.localPosition.x;
            Vector3 topPos = mTopTuodong.transform.localPosition;
            mTopTuodong.transform.localPosition = new Vector3(posx, topPos.y, topPos.z);
            mTopTuodong.enabled = true;
            IsOnDrapTop = true;

            Debug.Log("TopColiderOnMouseOver");
        }
    }

    void TopColiderOnMouseOut()
    {
        if (IsOnDrapTop == true && IsLock == false)
        {
            Cursor.visible = true;
            mTopTuodong.enabled = false;
            IsOnDrapTop = false;
            m_RepositionCount = 3;
            Debug.Log("TopColiderOnMouseOut");
        }
    }




    bool IsHide = false;
    bool IsWndMove = false;
    float mWndBgHeight = 0;
    //int Movetemp = 0;
    void BtnUpOnClick()
    {
        if (IsHide == true && IsWndMove == false && mWndBgHeight != 0)
        {

            IsHide = false;
            IsWndMove = true;
        }

    }

    void BtnDownOnClick()
    {
        if (IsHide == false && IsWndMove == false)
        {
            mWndBgHeight = mWndBg.gameObject.transform.localScale.y;
            //Movetemp = Convert.ToInt32((mWndBgHeight - mMoveHeight) / FixedTimeCount);
            IsHide = true;
            IsWndMove = true;

            mBtnDown.SetActive(false);
            mBtnUp.SetActive(true);
            mBtnLock.SetActive(false);
            mBtnUnLock.SetActive(false);
            mBtnClear.SetActive(false);

            mDrgPanelClider.enabled = false;

            m_RepositionCount = 3;
            mScrollBar.gameObject.SetActive(!IsHide);
        }
    }


    void UpdateWndMove()
    {

        if (IsHide)
        {
            Vector3 bgScale = mWndBg.gameObject.transform.localScale;
            float newHeight = Mathf.Lerp(mWndBg.gameObject.transform.localScale.y, mMoveHeight, Time.deltaTime * 10);

            if (newHeight - mMoveHeight < 1)
                newHeight = mMoveHeight;

            if (newHeight > mMoveHeight)
                mWndBg.gameObject.transform.localScale = new Vector3(bgScale.x, newHeight, bgScale.z);
            else
            {
                IsWndMove = false;
                mWndBg.gameObject.transform.localScale = new Vector3(bgScale.x, mMoveHeight, bgScale.z);
            }

        }
        else
        {

            Vector3 bgScale = mWndBg.gameObject.transform.localScale;
            float newHeight = Mathf.Lerp(mWndBg.gameObject.transform.localScale.y, mWndBgHeight, Time.deltaTime * 10);

            if (mWndBgHeight - newHeight < 1)
                newHeight = mWndBgHeight;


            if (newHeight < mWndBgHeight)
                mWndBg.gameObject.transform.localScale = new Vector3(bgScale.x, newHeight, bgScale.z);
            else
            {
                IsWndMove = false;
                mWndBg.gameObject.transform.localScale = new Vector3(bgScale.x, mWndBgHeight, bgScale.z);

                mBtnDown.SetActive(true);
                mBtnUp.SetActive(false);
                mBtnLock.SetActive(IsLock);
                mBtnUnLock.SetActive(!IsLock);
                mBtnClear.SetActive(true);

                mDrgPanelClider.enabled = true;

                mScrollBar.gameObject.SetActive(!IsHide);
            }
        }
    }


    void BtnLockOnClick()
    {
        if (IsLock == true)
        {
            IsLock = false;
            mTopClider.enabled = true;
            mRightClider.enabled = true;
        }
    }


    void BtnUnLockOnClick()
    {
        if (IsLock == false)
        {
            IsLock = true;
            mTopClider.enabled = false;
            mRightClider.enabled = false;
        }

    }

    void BtnClearOnClick()
    {
        ClearInputBox();

        //-------------------- text code ----------------------------------------------
        //		for (int i=0;i<100;i++)
        //		{
        //			AddMesg("User Name","just for text! " + i.ToString(),"ffffff");
        //		}
        //------------------------------------------------------------------------------
    }

    void SetCurItemsLineWidth(int lineWidth)
    {
        if (null != m_CurChatItems && m_CurChatItems.Count > 0)
        {
            for (int i = 0; i < m_CurChatItems.Count; i++)
            {
                m_CurChatItems[i].SetLineWidth(lineWidth);
            }
        }
    }

    CommonChatItem_N GetNewChatItem()
    {
        CommonChatItem_N item;
        if (m_ChatItemsPool.Count > 0)
        {
            item = m_ChatItemsPool.Dequeue();
            item.gameObject.SetActive(true);
        }
        else
        {
            GameObject go = Instantiate(m_ChatItemPrefab.gameObject);
            go.transform.parent = m_ContentTable.transform;
            go.transform.localPosition = Vector3.zero;
            go.transform.localScale = Vector3.one;
            go.transform.localRotation = Quaternion.identity;
            item= go.GetComponent<CommonChatItem_N>();
        }
        return item;
    }

    void RecoveryItems(int count)
    {
        if (null!=m_CurChatItems&&m_CurChatItems.Count >= count)
        {
            CommonChatItem_N recoveryItem;
            for (int i = 0; i < count; i++)
            {
                recoveryItem = m_CurChatItems[i];
                recoveryItem.ResetItem();
                recoveryItem.gameObject.SetActive(false);
                m_ChatItemsPool.Enqueue(recoveryItem);
            }
            m_CurChatItems.RemoveRange(0, count);
        }
    }
}
