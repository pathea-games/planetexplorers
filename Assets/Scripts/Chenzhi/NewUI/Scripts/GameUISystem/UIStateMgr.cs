using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class UIStateMgr : MonoBehaviour
{
    static UIStateMgr mInstance;
    public static UIStateMgr Instance { get { return mInstance; } }
    public bool CurShowGui { get { return m_CurShowGui; } }
    [HideInInspector]
    public List<UIBaseWnd> mBaseWndList;
    [HideInInspector]
    public List<UIStaticWnd> mStaticWndList;
    [HideInInspector]
    public const float BaseWndTopDepth = 20;
    [HideInInspector]
    public const float BaseWndDepth = 10f;
    [HideInInspector]
    private bool m_BackupShowGui = false;
    private bool m_CurShowGui=false;
    private List<UIBaseWnd> m_OpenHistoryList;    //log: lz-2016.05.05 记录打开的窗口列表
    public bool OpAnyGUI
    {
        get
        {
            if (GameUI.Instance.mNPCTalk.isShow)
                return true;

            return UIMouseEvent.opAnyGUI;
        }
    }

	public bool isTalking
	{
		get
		{
			if(null == GameUI.Instance || null == GameUI.Instance.mNPCTalk || null == GameUI.Instance.mNpcWnd || null == GameUI.Instance.mShopWnd)
				return false;
			return GameUI.Instance.mNPCTalk.isShow || GameUI.Instance.mNpcWnd.isShow || GameUI.Instance.mShopWnd.isShow;
		}
	}

    void Awake()
    {
        mInstance = this;
        mBaseWndList = new List<UIBaseWnd>();
        m_OpenHistoryList = new List<UIBaseWnd>();
    }
    // Use this for initialization
    void Start()
    {
        InitBaseWndList();

        mStaticWndList.AddRange(gameObject.GetComponentsInChildren<UIStaticWnd>(true));
    }

    void InitBaseWndList()
    {
        mBaseWndList.AddRange(gameObject.GetComponentsInChildren<UIBaseWnd>(true));
        // set depth for wnd
        float depth = BaseWndTopDepth;
		for(int i = 0; i < mBaseWndList.Count; ++i)
        {
			UIBaseWnd wnd = mBaseWndList[i];
            depth += BaseWndDepth;
            wnd.mDepth = depth;
            wnd.transform.localPosition = new Vector3(wnd.transform.localPosition.x, wnd.transform.localPosition.y, wnd.mDepth);
        }
    }

    void UpdateWndActive()
    {
		for(int i = 0; i < mBaseWndList.Count; ++i)
        {
			UIBaseWnd wnd = mBaseWndList[i];
            if (null != wnd && wnd.isShow)
            {
                if (wnd.IsCoverForTopsWnd(GetTopRects(wnd)))
                {
                    if (wnd.Active)
                        wnd.DeActiveWnd();
                }
                else
                {
                    if (!wnd.Active)
                        wnd.ActiveWnd();
                }

            }
        }
    }

    public void SaveUIPostion()
	{
		for(int i = 0; i < mBaseWndList.Count; ++i)
        {
			mBaseWndList[i].SaveWndPostion();
        }
    }
    // 带入计算 的中间矩形列表
    List<Rect> rectList = new List<Rect>();
    List<Rect> GetTopRects(UIBaseWnd wnd)
    {
		rectList.Clear();
		for(int i = 0; i < mBaseWndList.Count; ++i)
        {
			UIBaseWnd topWnd = mBaseWndList[i];
            if (null != topWnd && topWnd.isShow)
            {
                if (topWnd.transform.localPosition.z < wnd.transform.localPosition.z)
                {
                    rectList.Add(topWnd.rect);
                }
            }
        }
        return rectList;
    }


    bool GameUIIsShow()
    {
        if (GameConfig.IsInVCE)
            return true;

        //log:lz-2016.09.13 如果没有进入持枪状态，并且是NPCTalkWnd打开了，并且是不是自动播放的对话，就返回真，代表UI存在，需要解锁鼠标，如果是持枪状态，并且
        if (null!=UISightingTelescope.Instance&&UISightingTelescope.Instance.CurType == UISightingTelescope.SightingType.Null
            &&null!= GameUI.Instance&&GameUI.Instance.mNPCTalk.isShow&&GameUI.Instance.mNPCTalk.type==UINPCTalk.NormalOrSp.Normal)
           return true;

        if (GameUI.Instance.mSystemMenu.IsOpen()
           || GameUI.Instance.mOption.isShow
            || GameUI.Instance.mSaveLoad.isShow
            || MessageBox_N.Instance.isShow)
            return true;

        if (GameUI.Instance.mUIWorldMap.isShow)
            return true;
		
		for(int i = 0; i < mBaseWndList.Count; ++i)
        {
			UIBaseWnd wnd = mBaseWndList[i];
            if (wnd is UIMissionTrackCtrl || wnd is UIItemsTrackCtrl)
                continue;
            //lz-2016.06.24 因为吴怡秋把wnd.IsOpen检测改为gameObject.activeSelf了，这里要检测窗口是否可见，所检测activeInHierarchy
            if (null != wnd && wnd.gameObject.activeInHierarchy)
            {
                return true;
            }
        }
        return false;
    }

    // Update is called once per frame
    void Update()
    {
        UpdateWndActive();
        this.m_CurShowGui = GameUIIsShow();
        if (this.m_CurShowGui != this.m_BackupShowGui)
        {
			m_BackupShowGui = m_CurShowGui;
            if (GameUI.Instance != null && GameUI.Instance.mMainPlayer != null)
                GameUI.Instance.mMainPlayer.SendMsg(Pathea.EMsg.UI_ShowChange, m_BackupShowGui);
        }
     
    }

    #region public methods

    public static void RecordOpenHistory(UIBaseWnd openWnd)
    {
        if (null == Instance) return;
        RemoveOpenHistory(openWnd);
        Instance.m_OpenHistoryList.Add(openWnd);
    }

    public static void RemoveOpenHistory(UIBaseWnd openWnd)
    {
        if (null == Instance) return;
        if (Instance.m_OpenHistoryList.Contains(openWnd))
        {
            Instance.m_OpenHistoryList.RemoveAll(a => a == openWnd);
        }
    }

    public UIBaseWnd GetTopWnd()
    {
        if (this.m_OpenHistoryList.Count > 0)
        {
            return this.m_OpenHistoryList.FirstOrDefault(a => Mathf.Abs(a.transform.localPosition.z-BaseWndTopDepth)<0.0001f);
        }
        return null;
    }
    #endregion
}
