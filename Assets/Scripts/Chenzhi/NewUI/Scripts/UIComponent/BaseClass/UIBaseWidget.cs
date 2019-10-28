using UnityEngine;
using System.Collections;


public abstract class UIBaseWidget : MonoBehaviour 
{
	[HideInInspector]
	public bool mInit = false;
	[HideInInspector]
	public float mDepth = 0;
	//[HideInInspector] Rect rect;
	[HideInInspector] 
	public bool bMouseMoveIn;

 	public float mAlpha = 1;

	public delegate void WndEvent(UIBaseWidget widget = null);
	public WndEvent e_OnInit = null;
	public WndEvent e_OnShow = null;
	public WndEvent e_OnHide = null;
    public WndEvent e_OnShowFinish = null;


	protected UIAlphaGroup[] mGroups = null;
    private bool m_IsRefresh = false;

    private UIEnum.WndType m_SelfWndType = UIEnum.WndType.Null;
    public UIEnum.WndType SelfWndType { get { return m_SelfWndType; } protected set { m_SelfWndType = value; }}

    [SerializeField]  TweenScale mTweener;
	[SerializeField]  UITweenBufferAlpha mAlphaTweener;

	public virtual void OnCreate()
	{
		mTweener = gameObject.GetComponent<TweenScale>();
		if (mTweener != null)
			mTweener.onFinished = OnTweenFinished;

		mAlphaTweener = gameObject.GetComponent<UITweenBufferAlpha>();
		if (mAlphaTweener != null)
			mAlphaTweener.onFinished = OnTweenFinished;

	}


	public virtual void OnDelete()
	{

	}

	public virtual bool isShow
	{
		get
		{
			return gameObject.activeSelf;
		}
		set
		{
			if (gameObject != null)
			{
				if (gameObject.activeSelf != value)
				{
					if (value)
						Show();
					else 
						Hide();
				}
			}
		}
	}
	
	public virtual void Show()
	{
        if (!CanOpenWnd())
            return;

        if (!mInit)
            InitWindow();

        UpdateTween();
		PlayShowTween();
        PlayOpenSoundEffect();
		
		if (mInit)
		{

			if (gameObject != null )
				gameObject.SetActive(true);
			if (e_OnShow != null)
				e_OnShow(this);
		}
	}

    //lz-2016.07.04 系统菜单打开的时候禁止打开其他的界面
    protected bool CanOpenWnd()
    {
        if (null!=GameUI.Instance
            &&GameUI.Instance.SystemUIIsOpen
            && this != GameUI.Instance.mSystemMenu
            && this != GameUI.Instance.mOption
            && this != GameUI.Instance.mSaveLoad
            && this != MessageBox_N.Instance
            && this!=GameUI.Instance.mRevive)
        {
            return false;
        }
        return true;
    }

	protected void PlayShowTween ()
	{
        this.m_IsRefresh = false;
		if (mTweener != null)
			mTweener.Play(true);
		
		if (mAlphaTweener != null)
			mAlphaTweener.Play(true);

        //lz-2016.08.31 检测是否触发引导
        CheckOpenUI();
    }

	bool PlayerHideTween ()
	{
		bool r = false;

		if (mTweener != null)
		{
			if (gameObject.activeInHierarchy)
			{
				mTweener.Play(false);
				r = true;
			}
			else
				r = false;

		}

		if (mAlphaTweener != null)
		{
			if (gameObject.activeInHierarchy) 
			{
				mAlphaTweener.Play(false);
				r = true;
			}
			else
				r = false;
		}

		return r;
	}

	protected void UpdateTween()
	{
		if (mAlphaTweener != null)
			mAlphaTweener.refreshWidget = true;
	}

	public bool IsOpen()
	{
		return isShow;
	}

	public void Hide()
	{
		if (!PlayerHideTween())
			OnHide();

	}

	void OnTweenFinished (UITweener tween)
	{
        if (tween.direction == AnimationOrTween.Direction.Reverse)
        {
            if (gameObject.activeSelf)
                OnHide();
        }
        else if(tween.direction == AnimationOrTween.Direction.Forward)
        {
            if (!this.m_IsRefresh)
            {
                OnShowFinish();
                this.m_IsRefresh=true;
            }
        }
	}
	
	protected virtual void OnHide ()
	{
		if (gameObject != null )
			gameObject.SetActive(false);
		if (e_OnHide != null)
			e_OnHide(this);
	}

    //log:lz-2016.05.25 这个方法提供给有些界面在Tween播放完后出现图片渲染问题自己实现刷新用
    protected virtual void OnShowFinish()
    {
        //log:lz-2016.05.25 不要尝试在这里刷新根物体，会闪烁严重的，并且导致其他情况
        if (e_OnShowFinish != null)
            e_OnShowFinish();
    }

	protected virtual void OnClose()
	{
		Hide();
	}
	

	protected virtual void InitWindow()
	{
		mInit = true;
		mGroups = GetComponentsInChildren<UIAlphaGroup>(true);
		if (e_OnInit != null)
			e_OnInit(this);
	}

	public virtual void ChangeWindowShowState()
	{
		if (isShow)
			Hide();
		else
			Show();
	}

    //lz-2016.06.15 支持 #2397 添加UI音效
    protected virtual void PlayOpenSoundEffect()
    {
        if (null != GameUI.Instance)
        {
            GameUI.Instance.PlayWndOpenAudioEffect(this);
        }
    }

    //lz-2016.08.31 引导检测打开UI
    protected virtual void CheckOpenUI()
    {
        //lz-2016.08.31 只有配置了类型的面板才会进行检测,优化不必要的检测调用
        if (SelfWndType != UIEnum.WndType.Null)
        {
            InGameAidData.CheckOpenUI(SelfWndType);
        }
    }

}
