using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class OperationTip : MonoBehaviour
{
    static OperationTip mInstance = null;
    public static OperationTip Instance { get { return mInstance; } }
    public Camera uiCamera;
    [SerializeField]
    private UISprite mOpSprite;
    [SerializeField]
    private Transform m_OpTipTrans;
    [SerializeField]
    private UILabel m_TipLabel;
    [SerializeField]
    private UISprite m_TipBg;
    [SerializeField]
    private TweenScale m_TipScaleTween;
    [SerializeField]
    private float m_BorderY = 10;
    [SerializeField]
    private int m_ShopTipCount = 5;
    [SerializeField]
    private float m_WaitShowTime = 1f;

    private Dictionary<MouseOpMgr.MouseOpCursor, int> m_ShopTipDic = new Dictionary<MouseOpMgr.MouseOpCursor, int>();
    private bool m_TipShow = false;
    private MouseOpMgr.MouseOpCursor m_WaitBackupType = MouseOpMgr.MouseOpCursor.Null;
    private MouseOpMgr.MouseOpCursor m_ShowBackupType = MouseOpMgr.MouseOpCursor.Null;
    private float m_WaitStartTime;

    Transform mTrans;
    CursorHandler ch;


    //string mMouseHandBig;
    //string mMouseHandSmall;
    string mBag;
    string mAxe;
    string mNouseTalk;
    string mHand;
    string mRide;
    string mDefault;


    bool mIsShow = false;
    //bool mPlayCirculation = false;

    //float mTimer;
    //float mTotalTime;



    //MouseOpMgr.MouseOpCursor mCurState = MouseOpMgr.MouseOpCursor.Null;

    void Start()
    {
        mTrans = transform;
        if (uiCamera == null) uiCamera = NGUITools.FindCameraForLayer(gameObject.layer);
        ch = new CursorHandler();
        ch.Type = CursorState.EType.None;
        //mTimer = 0;
        //mTotalTime = 0.75f;
        m_TipScaleTween.onFinished = TipTweenFinish;

        //mMouseHandBig = "mouse_hand";
        //mMouseHandSmall = "mouse_hand1";
        mBag = "mouse_get";
        mAxe = "mouse_axe";
        mNouseTalk = "mouse_talk";
        mHand = "mouse_help";
        mRide = "mouse_ride";
        mDefault = "sighting_3";
    }

    //void ShowCursor(string IconName, bool ishand)
    //{
    //    mIsShow = true;
    //    mOpSprite.spriteName = IconName;
    //    mOpSprite.MakePixelPerfect();
    //    CursorState.self.SetHandler(ch);
    //    if (ishand)
    //    {
    //        mTimer = 0;
    //        mPlayCirculation = true;
    //    }
    //    else
    //    {
    //        mTimer = 0;
    //        mPlayCirculation = false;
    //    }
    //}


    //void HideCursor()
    //{
    //    mIsShow = false;
    //    mOpSprite.spriteName = "null";
    //    mOpSprite.MakePixelPerfect();
    //    CursorState.self.SetHandler(null);
    //    mPlayCirculation = false;
    //    mTimer = 0;
    //}

    void EnableCursor(string IconName)
    {
        mOpSprite.spriteName = IconName;
        mOpSprite.MakePixelPerfect();
        CursorState.self.SetHandler(ch);
        mIsShow = true;
    }

    void DisableCursor()
    {
        mOpSprite.spriteName = "null";
        mOpSprite.MakePixelPerfect();
        CursorState.self.SetHandler(null);
        mIsShow = false;
        PlayTipHideTween();
    }

    void Update()
    {
        if (!(PeInput.Get(PeInput.LogicFunction.Item_Drag) || UICamera.hoveredObject != null))
            UpdateMouseIcon();
        if ((UICamera.hoveredObject != null || PeInput.Get(PeInput.LogicFunction.Item_Drag)) && mIsShow)
            DisableCursor(); //HideCursor();

        if (PeInput.Get(PeInput.LogicFunction.InteractWithItem) || Input.GetMouseButtonDown(0))
            PlayTipHideTween();

        //更新位置
        if (mOpSprite.atlas != null)
        {
            Vector3 pos = Input.mousePosition;

            if (uiCamera != null)
            {
                // Since the screen can be of different than expected size, we want to convert
                // mouse coordinates to view space, then convert that to world position.
                pos.x = Mathf.Clamp01(pos.x / Screen.width);
                pos.y = Mathf.Clamp01(pos.y / Screen.height);
                mTrans.position = uiCamera.ViewportToWorldPoint(pos);
                // For pixel-perfect results
                if (uiCamera.orthographic)
                {
                    mTrans.localPosition = NGUIMath.ApplyHalfPixelOffset(mTrans.localPosition, mTrans.localScale);
                }
            }
        }
    }

    void LateUpdate()
    {
        //切换图片

        // mTimer += Time.deltaTime;

        //if (mPlayCirculation && ((int)(mTimer / mTotalTime)) % 2 == 0)
        //{
        //    //mOpSprite.spriteName = mMouseHandBig;
        //    mOpSprite.MakePixelPerfect();
        //}
        //else if (mPlayCirculation && ((int)(mTimer / mTotalTime)) % 2 == 1)
        //{
        //    //mOpSprite.spriteName = mMouseHandSmall;
        //    mOpSprite.MakePixelPerfect();
        //}
    }

    void UpdateMouseIcon()
    {
        if (MouseOpMgr.Instance != null)
        {
            switch (MouseOpMgr.Instance.currentState)
            {
                case MouseOpMgr.MouseOpCursor.Gather:
                    EnableCursor(mBag);
                    break;
                case MouseOpMgr.MouseOpCursor.Fell:
                    EnableCursor(mAxe);
                    break;
                case MouseOpMgr.MouseOpCursor.NPCTalk:
                    EnableCursor(mNouseTalk);
                    break;
                case MouseOpMgr.MouseOpCursor.PickUpItem:
                    EnableCursor(mBag);
                    break;
                case MouseOpMgr.MouseOpCursor.WareHouse:
                    EnableCursor(mBag);
                    break;
                case MouseOpMgr.MouseOpCursor.LootCorpse:
                    EnableCursor(mBag);
                    break;
                case MouseOpMgr.MouseOpCursor.Hand:
                    EnableCursor(mHand);
                    break;
                //lz-2016.10.23 游戏中鼠标悬浮在人员名单物品的时候鼠标的状态图标
                case MouseOpMgr.MouseOpCursor.KickStarter:
                    EnableCursor(mNouseTalk);
                    break;
                //lz-2047.02.24 指向坐骑改为坐骑指针状态
                case MouseOpMgr.MouseOpCursor.Ride:
                    EnableCursor(mRide);
                    break;
                case MouseOpMgr.MouseOpCursor.Null:
                    if (SystemSettingData.Instance.FirstPersonCtrl || !SystemSettingData.Instance.mMMOControlType)
                    {
                        if (PeCamera.cursorLocked && UISightingTelescope.Instance.CurType == UISightingTelescope.SightingType.Null)
                            EnableCursor(mDefault);
                        else
                            DisableCursor();
                    }
                    else
                        DisableCursor();
                    break;
            }
            ShowTipLabel(MouseOpMgr.Instance.currentState);
        }

        //if ((MouseOpMgr.Instance != null && MouseOpMgr.Instance.currentState == MouseOpMgr.MouseOpCursor.Gather) && mCurState != MouseOpMgr.MouseOpCursor.Gather /*&& !mIsShow*/)
        //{
        //    ShowCursor(mBag, false);
        //    mCurState = MouseOpMgr.MouseOpCursor.Gather;
        //}
        //if ((MouseOpMgr.Instance != null && MouseOpMgr.Instance.currentState == MouseOpMgr.MouseOpCursor.Fell) && mCurState != MouseOpMgr.MouseOpCursor.Fell /* && !mIsShow */)
        //{
        //    ShowCursor(mAxe, false);
        //    mCurState = MouseOpMgr.MouseOpCursor.Fell;
        //}
        //else if (MouseOpMgr.Instance != null && MouseOpMgr.Instance.currentState == MouseOpMgr.MouseOpCursor.NPCTalk && mCurState != MouseOpMgr.MouseOpCursor.NPCTalk/*&& !mIsShow*/)
        //{
        //    ShowCursor(mNouseTalk, false);
        //    mCurState = MouseOpMgr.MouseOpCursor.NPCTalk;
        //}
        //else if (MouseOpMgr.Instance != null && MouseOpMgr.Instance.currentState == MouseOpMgr.MouseOpCursor.PickUpItem && mCurState != MouseOpMgr.MouseOpCursor.PickUpItem /*&& !mIsShow*/)
        //{
        //    ShowCursor(mBag, false);
        //    mCurState = MouseOpMgr.MouseOpCursor.PickUpItem;
        //}
        //else if (MouseOpMgr.Instance != null && MouseOpMgr.Instance.currentState == MouseOpMgr.MouseOpCursor.WareHouse && mCurState != MouseOpMgr.MouseOpCursor.WareHouse /*&& !mIsShow*/)
        //{
        //    ShowCursor(mBag, false);
        //    mCurState = MouseOpMgr.MouseOpCursor.WareHouse;
        //}
        //else if (MouseOpMgr.Instance != null && MouseOpMgr.Instance.currentState == MouseOpMgr.MouseOpCursor.LootCorpse && mCurState != MouseOpMgr.MouseOpCursor.LootCorpse /*&& !mIsShow*/)
        //{
        //    ShowCursor(mBag, false);
        //    mCurState = MouseOpMgr.MouseOpCursor.LootCorpse;
        //}
        //else if (MouseOpMgr.Instance != null && MouseOpMgr.Instance.currentState == MouseOpMgr.MouseOpCursor.Hand && mCurState != MouseOpMgr.MouseOpCursor.Hand /*&& !mIsShow*/)
        //{
        //    ShowCursor(mHand, false);
        //    mCurState = MouseOpMgr.MouseOpCursor.Hand;
        //}
        //else if ((MouseOpMgr.Instance != null && MouseOpMgr.Instance.currentState == MouseOpMgr.MouseOpCursor.Null ))
        //{
        //    //HideCursor();

        //    //if (SystemSettingData.Instance.FirstPersonCtrl || !SystemSettingData.Instance.mMMOControlType)
        //    //{
        //    //    if (PeCamera.cursorLocked)
        //    //    {
        //    //        //if (!mIsShow)
        //    //            ShowCursor(mDefault, false);
        //    //    }
        //    //    else
        //    //    {
        //    //        //if (mIsShow)
        //    //            HideCursor();
        //    //    }
        //    //    //ShowCursor(mDefault, false);
        //    //}
        //    //else
        //    //{
        //    //    if (mIsShow)
        //    //        HideCursor();
        //    //}

        //    if (SystemSettingData.Instance.FirstPersonCtrl || !SystemSettingData.Instance.mMMOControlType)
        //    {
        //        if (PeCamera.cursorLocked)
        //        {
        //            if (mCurState != MouseOpMgr.MouseOpCursor.Null)
        //            {
        //                mCurState = MouseOpMgr.MouseOpCursor.Null;
        //                ShowCursor(mDefault, false);
        //            }
        //        }
        //        else
        //        {

        //        }
        //    }


        //}
    }


    private void ShowTipLabel(MouseOpMgr.MouseOpCursor opType)
    {
        if (!SystemSettingData.Instance.MouseStateTip)
        {
            PlayTipHideTween();
            return;
        }

        if (opType == MouseOpMgr.MouseOpCursor.Null)
        {
            PlayTipHideTween();
        }
        else
        {
            if (m_WaitBackupType != opType)
            {
                m_WaitStartTime = Time.realtimeSinceStartup;
                m_WaitBackupType = opType;
            }
            else
            {
                if (Time.realtimeSinceStartup - m_WaitStartTime < m_WaitShowTime)
                    return;
                if (m_ShowBackupType != opType)
                {
                    if (!m_ShopTipDic.ContainsKey(opType) || m_ShopTipDic.ContainsKey(opType) && m_ShopTipDic[opType] < m_ShopTipCount)
                    {
                        int contentID = -1;
                        switch (opType)
                        {
                            case MouseOpMgr.MouseOpCursor.Gather:
                                contentID = 8000678;
                                break;
                            case MouseOpMgr.MouseOpCursor.Fell:
                                contentID = 8000679;
                                break;
                            case MouseOpMgr.MouseOpCursor.NPCTalk:
                                contentID = -1;
                                break;
                            case MouseOpMgr.MouseOpCursor.PickUpItem:
                                contentID = 8000678;
                                break;
                            case MouseOpMgr.MouseOpCursor.WareHouse:
                                contentID = 8000678;
                                break;
                            case MouseOpMgr.MouseOpCursor.LootCorpse:
                                contentID = 8000678;
                                break;
                            case MouseOpMgr.MouseOpCursor.Hand:
                                contentID = 8000680;
                                break;
                            case MouseOpMgr.MouseOpCursor.Ride:
                                contentID = 8000983;
                                break;
                        }
                        if (contentID == -1) return;
                        PlayTipShowTween(PELocalization.GetString(contentID));
                        m_ShowBackupType = opType;
                        if (m_ShopTipDic.ContainsKey(opType))
                            m_ShopTipDic[opType]++;
                        else
                            m_ShopTipDic.Add(opType, 1);
                    }
                }
            }
        }
    }

    private void PlayTipShowTween(string content)
    {
        m_TipLabel.text = content;
        m_TipLabel.MakePixelPerfect();
        float tipLblHeight = m_TipLabel.relativeSize.y * m_TipLabel.font.size;
        Vector3 bgScale = m_TipBg.transform.localScale;
        bgScale.y = tipLblHeight + m_BorderY * 2;
        bgScale.x = m_TipLabel.relativeSize.x * m_TipLabel.font.size + 10f;
        m_TipBg.transform.localScale = bgScale;
        if (!m_TipShow)
            m_TipScaleTween.Play(true);
    }

    private void PlayTipHideTween()
    {
        if (m_TipShow)
            m_TipScaleTween.Play(false);
    }

    private void TipTweenFinish(UITweener tween)
    {
        m_TipShow = !m_TipShow;
        if (!m_TipShow)
        {
            m_ShowBackupType = MouseOpMgr.MouseOpCursor.Null;
            m_WaitBackupType = MouseOpMgr.MouseOpCursor.Null;
        }
    }
}