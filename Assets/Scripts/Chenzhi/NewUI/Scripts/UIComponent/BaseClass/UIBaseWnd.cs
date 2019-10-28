using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class UIBaseWnd : UIBaseWidget
{
    [SerializeField]
    UIWndActivate mActivate = null;
    [SerializeField]
    //float testAlpha = 1;
    public UIAnchor mAnchor;

    public Rect rect
    {
        get
        {
            if (mActivate == null)
                return new Rect(0, 0, 0, 0);

            Transform bgTs = mActivate.mCollider.gameObject.transform;
            float width = bgTs.localScale.x - 6;
            float height = bgTs.localScale.y - 6;

            Vector3 parent_pos = mAnchor == null ? Vector3.zero : mAnchor.transform.localPosition;

            float minx = transform.localPosition.x + mActivate.BgLocalPos.x - width / 2 + parent_pos.x;
            float miny = transform.localPosition.y + mActivate.BgLocalPos.y - height / 2 + parent_pos.y;
            float maxx = minx + width;
            float maxy = miny + height;
            return new Rect(minx, miny, maxx, maxy);
        }
    }

    public bool IsCoverForTopsWnd(List<Rect> rectList)
    {
        foreach (Rect r in rectList)
        {
            if (CrossAlgorithm(r, rect))
                return true;
        }
        return false;
    }

    protected override void InitWindow()
    {
        if (UIRecentDataMgr.Instance != null)
        {
            base.InitWindow();
            ResetWndPostion();
        }
    }


    public override void Show()
    {
        base.Show();
        TopMostWnd();
        UIStateMgr.RecordOpenHistory(this);
    }

    protected override void OnHide()
    {
        base.OnHide();
        SaveWndPostion();
        UIStateMgr.RemoveOpenHistory(this);
    }


    public override void OnCreate()
    {
        base.OnCreate();
    }

    protected override void OnClose()
    {
        base.OnClose();
        UIStateMgr.RemoveOpenHistory(this);
    }

    public virtual void DeActiveWnd()
    {
        // do active effect
        if (!deActive)
        {
            //testAlpha = 0.58f;
            deActive = true;
            if (mGroups != null)
            {
                foreach (UIAlphaGroup group in mGroups)
                    group.State = 1;
            }
        }

        if (mActivate != null)
            mActivate.Deactivate();
    }
    public virtual void ActiveWnd()
    {
        if (!isShow && !GameUI.Instance.mNPCTalk.isShow)
            Show();
        // do active effect
        if (deActive)
        {
            deActive = false;
            //testAlpha = 1;
            if (mGroups != null)
            {
                foreach (UIAlphaGroup group in mGroups)
                    group.State = 0;
            }
        }

        if (mActivate != null)
            mActivate.Activate();
    }

    protected virtual void LateUpdate()
    {
        //		if ( (testAlpha != mAlpha)  && isShow)
        //		{
        //			mAlpha = Mathf.Lerp(mAlpha,testAlpha,2 * Time.deltaTime);
        //			if (Mathf.Abs(mAlpha - testAlpha) < 0.02)
        //				mAlpha = testAlpha;
        //		}
    }

    bool deActive = false;
    public bool Active { get { return !deActive; } }

    public virtual void RepostionDepth()
    {
        transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, mDepth);
    }

    public virtual void TopMostWnd() // TopMost
    {
        if (null != GameUI.Instance)
        {
            UIBaseWnd topWnd = UIStateMgr.Instance.GetTopWnd();
            if (topWnd == this)
                return;
            if (null != topWnd)
                topWnd.RepostionDepth();
        }
        transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, UIStateMgr.BaseWndTopDepth);
    }



    void ResetWndPostion()
    {
        if (this.gameObject == null || UIRecentDataMgr.Instance == null)
            return;
        Vector3 wndPos = gameObject.transform.localPosition;
        wndPos = new Vector3((int)wndPos.x, (int)wndPos.y, (int)((mDepth == 0) ? wndPos.z : mDepth)); // qu zheng
        gameObject.transform.localPosition = UIRecentDataMgr.Instance.GetVector3Value(gameObject.name, wndPos);
    }

    public void SaveWndPostion()
    {
        if (this.gameObject == null || UIRecentDataMgr.Instance == null)
            return;
        Vector3 wndPos = gameObject.transform.localPosition;
        wndPos = new Vector3((int)wndPos.x, (int)wndPos.y, (int)wndPos.z); // qu zheng
        UIRecentDataMgr.Instance.SetVector3Value(gameObject.name, wndPos);
        UIRecentDataMgr.Instance.SaveUIRecentData();
    }

    bool CrossAlgorithm(Rect r1, Rect r2)
    {
        float nMaxLeft = 0;
        float nMaxTop = 0;
        float nMinRight = 0;
        float nMinBottom = 0;

        nMaxLeft = (r1.x >= r2.x) ? r1.x : r2.x;
        nMaxTop = (r1.y >= r2.y) ? r1.y : r2.y;
        nMinRight = (r1.width <= r2.width) ? r1.width : r2.width;
        nMinBottom = (r1.height <= r2.height) ? r1.height : r2.height;

        if (nMaxLeft > nMinRight || nMaxTop > nMinBottom)
            return false;
        else
            return true;
    }
}
