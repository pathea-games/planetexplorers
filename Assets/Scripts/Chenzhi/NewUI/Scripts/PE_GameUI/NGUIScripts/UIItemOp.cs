using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ItemAsset;
using SkillAsset;
using Pathea.Operate;

public class UIItemOp : UIBaseWnd
{
    public UISlicedSprite mBg;
    public ItemOpBtn_N mCloseBtn;
    public ItemOpBtn_N mPrefab;

    public GameObject mRefillWnd;
    public UILabel mNumText;
    public UILabel mNum;
    public int AmmoNum { get { return (int)mCurrentNum; } }
    public GameObject mSleepWnd;
    public UIScrollBar mSleepSlider;
    public UILabel mSleepTime;
    public UILabel mMaxSleepTime;
    public UILabel mMinSleepTime;
    public GameObject mMainWnd;
    public UISlider mSlider;

    private bool mAddBtnPress = false;
    private bool mSubBtnPress = false;
    private float mCurrentNum;
    private float mOpDurNum;
    private float mOpStarTime;
    private bool mIsAmmoTower;
    private int m_AmmoCount = 0;
    private int m_AmmoMaxCount = 0;
    private int m_AmmoMaxHave = 0;
    [SerializeField]
    private WhiteCat.SleepingUI sleepingUI;
    private const float mGetItemTime = 2f;
    private UTimer mTimer;
    private List<ItemOpBtn_N> mBottons = new List<ItemOpBtn_N>();

	MonoBehaviour m_Operater;
    Pathea.PeEntity mEntity = null;

    CmdList mCmdList = null;
    System.Action mClose;
    System.Action mOpen;
    System.Action mDoGet;

    //System.Action<float> mDoSleep = null;
    private System.Func<float> m_SpeepEvent;
    PESleep m_PeSleep = null;
    

    #region mono methods

    void Update()
    {
        if (null == m_Operater && null == m_SpeepEvent)
        {
            Hide();
            return;
        }

        if (mSlider.gameObject.activeSelf)
        {
            if (mTimer == null)
                return;

            mTimer.Update(Time.deltaTime);
            if (mTimer.Second > mGetItemTime)
            {
                mTimer.Second = mGetItemTime;
                if (mDoGet != null)
                {
                    mDoGet();
                    mSlider.gameObject.SetActive(false);
                    gameObject.SetActive(false);
                }
            }
            mSlider.sliderValue = (float)mTimer.Second / mGetItemTime;
        }


        mSleepTime.text = string.Format(PELocalization.GetString(8000251),GetCurSleepTime());

        if (mAddBtnPress)
        {
            float dT = Time.time - mOpStarTime;
            if (dT < 0.2f)
                mOpDurNum = 1;
            else if (dT < 1f)
                mOpDurNum += 2 * Time.deltaTime;
            else if (dT < 2f)
                mOpDurNum += 4 * Time.deltaTime;
            else if (dT < 3f)
                mOpDurNum += 7 * Time.deltaTime;
            else if (dT < 4f)
                mOpDurNum += 11 * Time.deltaTime;
            else if (dT < 5f)
                mOpDurNum += 16 * Time.deltaTime;
            else
                mOpDurNum += 20 * Time.deltaTime;

            int addNum = (int)(mOpDurNum + mCurrentNum);
            int farNum = this.m_AmmoMaxCount - this.m_AmmoCount;
            mCurrentNum = Mathf.Clamp((addNum > farNum) ? farNum : addNum, 0, this.m_AmmoMaxHave);
            mNum.text = mCurrentNum.ToString();
        }
        else if (mSubBtnPress)
        {
            float dT = Time.time - mOpStarTime;
            if (dT < 0.5f)
                mOpDurNum = -1;
            else if (dT < 1f)
                mOpDurNum -= 2 * Time.deltaTime;
            else if (dT < 2f)
                mOpDurNum -= 4 * Time.deltaTime;
            else if (dT < 3f)
                mOpDurNum -= 7 * Time.deltaTime;
            else if (dT < 4f)
                mOpDurNum -= 11 * Time.deltaTime;
            else if (dT < 5f)
                mOpDurNum -= 16 * Time.deltaTime;
            else
                mOpDurNum -= 20 * Time.deltaTime;

            mCurrentNum = int.Parse(mNum.text);
            int SubNum = (int)(mOpDurNum + mCurrentNum);
            mCurrentNum = (SubNum < 0) ? 0 : SubNum;
            mNum.text = mCurrentNum.ToString();
        }

		// 超过 30 米取消睡觉操作
		if (mSleepWnd.activeSelf)
		{
			if (null == mEntity || null == m_PeSleep
				|| Vector3.Distance(mEntity.position, m_PeSleep.transform.position) > 30)
			{
				OnCancelBtn();
			}
		}

	}
    #endregion
    #region private methods
    //log:lz-2016.04.13 更新炮塔中炮弹的数量比
    private void UpdateAmmoLabel(int ammoCount, int ammoMaxCount)
    {
        mNumText.text = ammoCount + "/" + ammoMaxCount;
    }

    void ClearOpBtn()
    {
        for (int i = mBottons.Count - 1; i >= 0; i--)
            Destroy(mBottons[i].gameObject);
        mBottons.Clear();
    }

    void AddOpBtn(string buttonName)
    {
        ItemOpBtn_N addItem = Instantiate(mPrefab) as ItemOpBtn_N;
        addItem.gameObject.name = buttonName;
        addItem.InitButton(buttonName, gameObject);
        addItem.transform.parent = mMainWnd.transform;
        addItem.transform.localRotation = Quaternion.identity;
        addItem.transform.localScale = Vector3.one;
        mBottons.Add(addItem);
    }

    void ResetWnd()
    {
        if (mIsAmmoTower)
        {
            mBg.transform.localScale = new Vector3(172, 50f + (mBottons.Count + 1) * 40f + 50f, 1f);
            mRefillWnd.SetActive(true);
            mOpDurNum = 0;
            //log：lz-2016.04.13 这里当前的子弹数量在窗口打开的时候应该清空
            mCurrentNum = 0;
            Vector3 RefillWndPos = mRefillWnd.transform.localPosition;
            RefillWndPos.y = mBg.transform.localScale.y / 2f - 52f;
            RefillWndPos.z = -2f;
            mRefillWnd.transform.localPosition = RefillWndPos;
        }
        else
        {
            mBg.transform.localScale = new Vector3(172, 50f + (mBottons.Count + 1) * 40f, 1f);
            mRefillWnd.SetActive(false);
        }

        float Offset = mBottons.Count * 14;
        for (int i = 0; i < mBottons.Count; i++)
        {
            if (mIsAmmoTower)
                mBottons[i].transform.localPosition = new Vector3(0, -i * 40f + Offset - 32f, 0);
            else
                mBottons[i].transform.localPosition = new Vector3(0, -i * 40f + Offset, 0);
        }
        if (mIsAmmoTower)
            mCloseBtn.transform.localPosition = new Vector3(0, -mBottons.Count * 40f + Offset - 32f, 0);
        else
            mCloseBtn.transform.localPosition = new Vector3(0, -mBottons.Count * 40f + Offset, 0);
    }

    void ResetDefaultState()
    {
        m_Operater = null;
        mEntity = null;
        mCmdList = null;
        mOpen=null;
        mDoGet=null;
        //mDoSleep = null;
        m_SpeepEvent=null;
        m_PeSleep = null;

        mSleepWnd.SetActive(false);
		mMainWnd.SetActive(true);
        mSlider.gameObject.SetActive(false);
    }

    void OnOkBtn()
    {
        //		if (DoSleep)//mDoSleep != null
        //        {		
        //			mDoSleep((int)(((int)11 * mSleepSlider.scrollValue) + 1));
        //			ShowSleepWnd(false);
        //		}
        //		else
        //			Hide();
        if (m_PeSleep == null || mEntity == null)
        {
            return;
        }

        if (!m_PeSleep.CanOperateMask(Pathea.Operate.EOperationMask.Sleep))
        {
            return;
        }

        Pathea.MotionMgrCmpt mmc = mEntity.GetCmpt<Pathea.MotionMgrCmpt>();

        if (null != mmc && (mmc.IsActionRunning(Pathea.PEActionType.Sleep) || !mmc.CanDoAction(Pathea.PEActionType.Sleep)))
        {
            return;
        }

        SleepController.StartSleep(m_PeSleep, mEntity, GetCurSleepTime());
        ShowSleepWnd(false);
    }

    void OnCancelBtn()
    {
        ShowSleepWnd(false);
        this.gameObject.SetActive(false);
    }

    void OnMinBtn()
    {
        //if(mousePick)
        //{
        mCurrentNum = 0;
        mOpDurNum = 0;
        mNum.text = "0";
        //}
        //else
        //    Hide();
    }

    void OnMaxBtn()
    {
        //if(mousePick)
        //{

        int maxNum = Mathf.Clamp(this.m_AmmoMaxCount - this.m_AmmoCount, 0, this.m_AmmoMaxHave);
        mCurrentNum = maxNum;
        mNum.text = maxNum.ToString();
        //AiTowerAmmo tower = mOpItemSc.GetComponent<AiTowerAmmo>();
        //int playerNum = 0;//PlayerFactory.mMainPlayer.GetItemNum(tower.mAmmoID);
        //int needNum = 0;//(int)(mOpItemSc.mItemObj.GetProperty(ItemProperty.AmmoMax) - mOpItemSc.mItemObj.GetProperty(ItemProperty.AmmoNum));
        //int maxNum = (needNum > playerNum)?playerNum:needNum;
        //mCurrentNum = maxNum;
        //mOpDurNum = 0;
        //mNum.text = maxNum.ToString();
        //}
        //else
        //    Hide();
    }

    void OnAddBtnPress()
    {
        mAddBtnPress = true;
        mOpStarTime = Time.time;
        mOpDurNum = 0;
    }

    void OnAddBtnRelease()
    {
        mAddBtnPress = false;
        //mCurrentNum=0;

        //log：lz-2016.04.13 这里释放按钮的时候mCurrentNum就不用再加了，因为在Update中已经加过了
        //int AddNum = int.Parse(mNum.text);
        //mCurrentNum = Mathf.Clamp(ammoCount + AddNum, 0, ammoMaxHave);

    }

    void OnSubstructBtnPress()
    {
        mSubBtnPress = true;
        mOpStarTime = Time.time;
        mOpDurNum = 0;
    }

    void OnSubstructBtnRelease()
    {
        mSubBtnPress = false;
        //mCurrentNum += mOpDurNum;
    }

    int GetCurSleepTime()
    {
        return (int)((SleepTime.MaxHours - SleepTime.MinHours) * mSleepSlider.scrollValue + SleepTime.MinHours);
    }

    void InitSleepTime()
    {
        mMaxSleepTime.text = SleepTime.MaxHours.ToString();
        mMinSleepTime.text = SleepTime.MinHours.ToString();
        mSleepSlider.scrollValue = ((float)SleepTime.NormalHours) / SleepTime.MaxHours;
        mSleepTime.text = string.Format(PELocalization.GetString(8000251), GetCurSleepTime());
    }

    #endregion
    #region protected methods
    protected override void InitWindow()
    {
        if (mInit) return;
        base.InitWindow();
        mTimer = new UTimer();
        mTimer.ElapseSpeed = 1;
        mTimer.Second = 0;
        mInit = true;
    }
	public override void Show()
	{
		ResetDefaultState();
		base.Show();
	}
	protected override void OnClose()
    {
        base.OnClose();

        if (mClose != null)
        {
            mClose();
        }

        //// Wuyiqiu
        //if (mousePick != null && mousePick as DragItemMousePickColony != null)
        //{
        //    DragItemMousePickColony isc = mousePick as DragItemMousePickColony;

        //    isc.OnItemOpGUIHide();
        //}
    }
	protected override void OnHide()
    {
        ResetDefaultState();
        base.OnHide();
    }
    #endregion
    #region public method
    public void ShowSleepingUI(System.Func<float> time01)
    {
        InitWindow();
        isShow = true;
        m_SpeepEvent = time01;
        sleepingUI.Show(time01);
    }

    public void HideSleepingUI()
    {
        isShow = false;
        m_SpeepEvent = null;
        sleepingUI.Hide();
    }

    public void SetRefill(int currentCount, int maxCount, int maxHave)
    {
        this.UpdateAmmoCount(currentCount, maxCount, maxHave);
        this.UpdateAmmoLabel(this.m_AmmoCount, this.m_AmmoMaxCount);
        mNum.text = "0";
        mCurrentNum = 0;
        mOpDurNum = 0;

        mIsAmmoTower = true;
        ResetWnd();
    }

    //log:lz-2016.04.13 这个方法用来实时更新数量
    public void UpdateAmmoCount(int currentCount, int maxCount, int maxHave)
    {
        this.m_AmmoCount = currentCount;
        this.m_AmmoMaxCount = maxCount;
        this.m_AmmoMaxHave = maxHave;
        this.UpdateAmmoLabel(this.m_AmmoCount, this.m_AmmoMaxCount);
    }
   
    public void ListenEvent(System.Action close, System.Action open)
    {
        if (mClose != null)
        {
            mClose();
        }

        mClose = close;
        mOpen = open;
    }

	//lz-2016.09.10 设置当前的操作对象
	private void SetOperater(MonoBehaviour mono)
	{
		m_Operater = mono;
    }

    public void SetCmdList(MonoBehaviour mono, CmdList cmdList)
    {
		Show();
		SetOperater(mono);
        if (mOpen != null)
        {
            mOpen();
        }

        mIsAmmoTower = false;

        ClearOpBtn();

        mCmdList = cmdList;

        foreach (string funName in cmdList)
        {
            AddOpBtn(funName);
        }

        ResetWnd();
    }
    
    public void ShowSleepWnd(bool show,MonoBehaviour operater=null, PESleep peSleep = null, Pathea.PeEntity character = null, System.Action<float> sleep = null)
    {
        //lz-2016.06.01 不经过菜单直接打开SleepWnd的时候要先打开ItemOP
        if (!isShow)
            Show();
        if (show&& operater!=null)
        {
            if (!mInit)
                InitWindow();
            mSleepWnd.SetActive(true);
            mMainWnd.SetActive(false);
            InitSleepTime();
            m_PeSleep = peSleep;
            mEntity = character;
            //mDoSleep = sleep;
			SetOperater(operater);
        }
        else
            mSleepWnd.SetActive(false);

    }

    public void SleepImmediately(PESleep peSleep, Pathea.PeEntity character)
    {
        if (!mInit) InitWindow();

        mMainWnd.SetActive(false);
        SleepController.StartSleep(peSleep, Pathea.MainPlayer.Instance.entity,SleepTime.NormalHours);
    }

    public void GetItem(System.Action doGet, MonoBehaviour mono)
    {
		if (doGet == null)
        {
            return;
        }

        if (!isShow)
        {
            Show();
        }
		
		m_Operater = mono;
		mDoGet = doGet;

        mMainWnd.SetActive(false);
        mSlider.gameObject.SetActive(true);
        mTimer.Second = 0;
    }

    public void CallFunction(string funcName)
    {
		if (null == mCmdList || null == m_Operater)
        {
            return;
        }

        mCmdList.ExecuteCmd(funcName);
    }

    #endregion
}