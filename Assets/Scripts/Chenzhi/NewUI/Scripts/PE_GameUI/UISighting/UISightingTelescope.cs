using UnityEngine;
using System.Collections;
using System;

public class UISightingTelescope : UIBaseWidget 
{
	static UISightingTelescope mInstance = null;
	public static UISightingTelescope Instance {get {return mInstance;}}

	[SerializeField] UIDefaultSighting mDefault;
	[SerializeField] UIShotGunSighting mShotGun;
	[SerializeField] UIBowSighting mBow;
	[SerializeField] SightingType currentType;
    [SerializeField]
    float mValue = 0;
    //[SerializeField]
    //bool bShoot = false;
    [SerializeField]
    GameObject m_OrthoAimGoDefault;
    [SerializeField]
    GameObject m_OrthoAimGoShotGun;
    [SerializeField]
    GameObject m_OrthoAimGoBow;

    Vector2 viewPos { get { return PeCamera.cursorPos; } }
    private GameObject m_OrthoAimGo;    //log:2016.05.12 正交准心点
    private SightingType m_CurOrthoAimType;

    public float Scale { set { mValue = Mathf.Clamp01(value); } }
    public int MaxOrthoAimAlpha=1;
    public int MinOrthoAimAlpha = 0;
    public int MaxDistance = 30;
    public int MinDistance = 0;
	public SightingType CurType { get { return currentType; } }

	public enum SightingType
	{
		Default,
		ShotGun,
		Bow,
		Null
	}

    #region public methods
    public override void OnCreate ()
	{
		base.OnCreate ();
		mInstance = this;
		currentType = SightingType.Null;
	}

	public void Show(SightingType type)
	{
		currentType = type;
        UpdateType();
        m_UpdateUIWnd = true;

        if (GameUI.Instance.mMissionTrackWnd.isShow)
        {
            //lz-2016.09.12 进入射击模式的时候，鼠标会锁定，所以需要禁用Drag，不然鼠标状态不会被释放了
            GameUI.Instance.mMissionTrackWnd.EnableWndDrag(false);
        }

        if (GameUI.Instance.mItemsTrackWnd.isShow)
        {
            GameUI.Instance.mItemsTrackWnd.EnableWndDrag(false);
        }
    }

	public void ExitShootMode ()
	{
		EnableOrthoAimPoint(false);
		currentType = SightingType.Null;
		UpdateType();
        //		Hide();
        m_UpdateUIWnd = true;

        if (GameUI.Instance.mMissionTrackWnd.isShow)
        {
            GameUI.Instance.mMissionTrackWnd.EnableWndDrag(true);
        }

        if (GameUI.Instance.mItemsTrackWnd.isShow)
        {
            GameUI.Instance.mItemsTrackWnd.EnableWndDrag(true);
        }
    }

    bool m_UpdateUIWnd = false;
	public void UpdateType ()
	{
		mDefault.gameObject.SetActive(currentType == SightingType.Default);
		mShotGun.gameObject.SetActive(currentType == SightingType.ShotGun);
		mBow.gameObject.SetActive(currentType == SightingType.Bow);

       
    }

    //log:lz-2016.05.12 激活正交准心
    public void EnableOrthoAimPoint(bool enabel)
    {
        if(this.currentType!= SightingType.Null)
        {
            if(enabel)
            {
                this.ShowOrthoAimByCurType();
            }
            else
            {
                this.HideOrthoAim();
            }
        }
    }

    //log:lz-2016.05.12 更新正交准心位置
    public void SetOrthoAimPointPos(Vector3 pointPos)
    {
        if (this.currentType != SightingType.Null)
        {
            if (this.m_CurOrthoAimType != this.currentType || null==this.m_OrthoAimGo)
            {
                this.ShowOrthoAimByCurType();
            }
			this.m_OrthoAimGo.transform.localPosition = pointPos - new Vector3(0.5f * Screen.width, 0.5f * Screen.height, 0);
            this.ChangeAlphaByDistance();
        }
    }

    void ShowOrthoAimByCurType()
    {
        if (this.m_CurOrthoAimType != this.currentType || null == this.m_OrthoAimGo)
        {
            this.HideAllOrthoAim();
            switch (this.currentType)
            {
                case SightingType.Default:
                    this.m_OrthoAimGo = this.m_OrthoAimGoDefault;
                    break;
                case SightingType.ShotGun:
                    this.m_OrthoAimGo = this.m_OrthoAimGoShotGun;
                    break;
                case SightingType.Bow:
                    this.m_OrthoAimGo = this.m_OrthoAimGoBow;
                    break;
                case SightingType.Null:
                    this.m_OrthoAimGo = null;
                    break;
            }
            this.m_CurOrthoAimType = this.currentType;
        }
        if (this.m_OrthoAimGo != null)
        {
            this.m_OrthoAimGo.transform.localPosition = Vector3.zero;
            this.m_OrthoAimGo.SetActive(true);
        }
    }

    void HideOrthoAim()
    {
        if (this.m_CurOrthoAimType == this.currentType && null != this.m_OrthoAimGo)
        {
            this.m_OrthoAimGo.SetActive(false);
        }
        else
        {
            this.HideAllOrthoAim();
        }
    }

    void HideAllOrthoAim()
    {
        if (this.m_OrthoAimGoDefault.activeSelf)
        {
            this.m_OrthoAimGoDefault.SetActive(false);
        }
        if (this.m_OrthoAimGoShotGun.activeSelf)
        {
            this.m_OrthoAimGoShotGun.SetActive(false);
        }
        if (this.m_OrthoAimGoBow.activeSelf)
        {
            this.m_OrthoAimGoBow.SetActive(false);
        }
    }

    public void OnShoot()
    {
        if (this.currentType != SightingType.Null)
            this.GetCurSightingByCurType().OnShoot();
    }

    #endregion

    #region private methods
    private void ChangeAlphaByDistance()
    {
        if(null!=this.m_OrthoAimGo&&this.currentType != SightingType.Null)
        {
            UISprite[] sprites = this.m_OrthoAimGo.GetComponentsInChildren<UISprite>();
            if(sprites.Length>0)
            {
                Vector2 sightingPos=this.GetCurSightingByCurType().transform.localPosition;
                Vector2 orthoAimPos=this.m_OrthoAimGo.transform.localPosition;
                float distance = (orthoAimPos - sightingPos).magnitude;
                distance=Mathf.Clamp(distance,this.MinDistance,this.MaxDistance);
                float alpha = Mathf.Lerp(this.MinOrthoAimAlpha, this.MaxOrthoAimAlpha, distance / (this.MaxDistance - this.MinDistance));
                for (int i = 0; i < sprites.Length; i++)
                {
                    sprites[i].alpha = alpha;
                }
            }
        }
    }

    private void InstantiateCurSighting()
    {
        GameObject curSightingGo=  this.GetCurSightingByCurType().gameObject;
        this.m_OrthoAimGo = GameObject.Instantiate(curSightingGo);
        this.m_OrthoAimGo.GetComponent<UIBaseSighting>().Value = 0f;
        this.m_OrthoAimGo.transform.parent = transform;
        this.m_OrthoAimGo.name = "OrthoAimGo";
        this.m_OrthoAimGo.transform.localPosition = Vector3.zero;
        this.m_OrthoAimGo.transform.localScale = Vector3.one;
        this.m_OrthoAimGo.SetActive(true);
        this.m_CurOrthoAimType = this.currentType;
    }

    private UIBaseSighting GetCurSightingByCurType()
    {
        switch (currentType)
        {
            case SightingType.Default:
                return mDefault;
            case SightingType.ShotGun:
                return mShotGun;
            case SightingType.Bow:
                return mBow;
        }
        return null;
    }

    #endregion

    #region mono methods
    void Update()
	{
        if (this.currentType != SightingType.Null)
		{
			Vector2 vpos = viewPos;
			try{
				int x = Convert.ToInt32( (vpos.x - 0.5f) * Screen.width) ;
				int y = Convert.ToInt32( (vpos.y - 0.5f) * Screen.height);
				GetCurSightingByCurType().transform.localPosition = new Vector3(x,y,0);
				GetCurSightingByCurType().Value = mValue;
			} catch{}
		}
	} 

    void LateUpdate ()
    {
        if (m_UpdateUIWnd)
        {
            if (currentType != SightingType.Null)
            {
                //GameUI.Instance.Invoke("HideGameWnd", 0.1f);
                GameUI.Instance.HideGameWnd();
            }
            else
                GameUI.Instance.ShowGameWndAll();
                //GameUI.Instance.Invoke("ShowGameWnd", 0.1f);

            m_UpdateUIWnd = false;
        }
    }
    #endregion
}
