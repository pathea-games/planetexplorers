using UnityEngine;
using System.Collections;
using Pathea;
using SkillSystem;

public class UIfootManItem : MonoBehaviour
{
    private UINPCfootManMgr.FootmanInfo mFootmanInfo = null;
    private const int m_RevivePrtroId = 937;
    private PlayerPackageCmpt m_PlayerPackage;

    public UINPCfootManMgr.FootmanInfo FootmanInfo
    {
        get { return mFootmanInfo; }
        set
        {
            mFootmanInfo = value;

            if (mFootmanInfo == null)
            {
                npcCmpt = null;
                gameObject.SetActive(false);
            }
            else
                gameObject.SetActive(true);

            if (mFootmanInfo != null)
            {

                if (npcCmpt != null && npcCmpt != mFootmanInfo.mNpCmpt)
                {
                    npcCmpt = mFootmanInfo.mNpCmpt;
                    if (npcCmpt.FollowerCurReviveTime == 0f)
                        InitReviveTime();
                }
                else if (npcCmpt == null)
                {
                    npcCmpt = mFootmanInfo.mNpCmpt;
                    if (npcCmpt.FollowerCurReviveTime == 0f)
                        InitReviveTime();
                }

                SetNpcHeadTextre(mFootmanInfo.mTexture);
            }
        }
    }

    SkAliveEntity mSkEntity = null;
    public SkAliveEntity SkEntity
    {
        get { return mSkEntity; }
        set
        {
            mSkEntity = value;
            if (mSkEntity != null)
            {
                FollowerId = mSkEntity.Entity.Id;
            }
        }
    }


    [SerializeField]
    GameObject mAttackBtn;
    [SerializeField]
    GameObject mDefenceBtn;
    [SerializeField]
    GameObject mRestBtn;
    [SerializeField]
    GameObject mStayBtn;
    [SerializeField]
    GameObject mChoseList;


    //[SerializeField]
    //GameObject mAttackshow;
    //[SerializeField]
    //GameObject mDefenceshow;
    //[SerializeField]
    //GameObject mRestshow;
    //[SerializeField]
    //GameObject mStayShow;

    [SerializeField]
    UIFilledSprite mNpcBlood;

    [SerializeField]
    UITexture mNPCTexture;
    [SerializeField]
    GameObject mNpcDead;

    [SerializeField]
    UICheckbox mAttCk;
    [SerializeField]
    UICheckbox mDefCk;
    [SerializeField]
    UICheckbox mResCk;
    [SerializeField]
    UICheckbox mStayCk;

    [SerializeField]
    BoxCollider mAttCol;
    [SerializeField]
    BoxCollider mDefCol;
    [SerializeField]
    BoxCollider mResCol;
    [SerializeField]
    BoxCollider mStayCol;
    [SerializeField]
    UISprite m_WorkSpr;

    public UICheckbox mBtnCk;
    public int FollowerId;

    

    [HideInInspector]
    public int mIndex;

    
    bool IsDead = false;
    bool init = false;
    NpcCmpt m_Cmpt;
    private ShowToolTipItem_N m_ShowToolTipItem;

    public NpcCmpt npcCmpt
    {
        get
        {
            return m_Cmpt;
        }
        set
        {
            this.m_Cmpt = value;
            if (null != this.m_Cmpt)
            {
                this.UpdateShowToolTip();
                this.UpdateWorkState();
                this.m_Cmpt.FollowerWorkStateChangeEvent -= UpdateWorkState;
                this.m_Cmpt.FollowerWorkStateChangeEvent += UpdateWorkState;
            }
        }
    }

    ENpcBattle mBattlType = ENpcBattle.Evasion;
    ENpcBattle mOldBattlType = ENpcBattle.Evasion;

    void Start()
    {
        //mAttackshow.SetActive(false);
        //mDefenceshow.SetActive(true);
        //mRestshow.SetActive(false);
        this.UpdateWorkState();
    }

    float mReviveTimer;

    public float ReviveTimer
    {
        // get { return mReviveTimer; }
        get
        {
            return m_Cmpt != null ? m_Cmpt.FollowerCurReviveTime : 100000;
        }
        set
        {
            if (m_Cmpt != null)
                m_Cmpt.FollowerCurReviveTime = value;
        }

    }

    float TotalRestTime
    {
        get
        {
            return m_Cmpt != null ? m_Cmpt.FollowerReviceTime : 100000;
        }
    }

    float mTimer = 0f;

    public void InitReviveTime()
    {
        ReviveTimer = TotalRestTime;
    }

    void Update()
    {
        UpdateHpPersent();

        if (m_Cmpt != null)
            mBattlType = m_Cmpt.Battle;
        if (mOldBattlType != mBattlType)
            ShowBattle();


        if (mSkEntity != null)
            ShowNpcDead(mSkEntity.isDead);

        if (mSkEntity != null && mSkEntity.isDead)
        {
            mTimer += Time.deltaTime;

            if (mTimer >= 1f)
            {
                mTimer = 0f;
                ReviveTimer--;
            }
        }
    }

    void LateUpdate()
    {

        if (mChoseList.gameObject.activeSelf)
        {
            if (Input.GetMouseButton(0) || Input.GetMouseButton(1))
            {
                if (!init)
                {
                    init = true;
                    Ray ray = UICamera.mainCamera.ScreenPointToRay(Input.mousePosition);
                    RaycastHit rayHit;
                    if (mAttCol.Raycast(ray, out rayHit, 300) || mDefCol.Raycast(ray, out rayHit, 300) 
                        || mResCol.Raycast(ray, out rayHit, 300) || mStayCol.Raycast(ray, out rayHit, 300))
                    {
                        StartCoroutine(BtnCkStateChange(0.2f));
                    }
                    else
                    {
                        StartCoroutine(BtnCkStateChange(0.1f));
                    }
                }
            }
        }

    }

    void UpdateHpPersent()
    {
        if (mSkEntity != null)
        {
            mNpcBlood.fillAmount = mSkEntity.HPPercent;
        }
    }

    void ShowNpcDead(bool Show)
    {
        mNpcDead.SetActive(Show);
        mNPCTexture.gameObject.SetActive(!Show);
        IsDead = Show;

    }

    void ChangeBattle(ENpcBattle type)
    {
        if (m_Cmpt == null)
            return;
        m_Cmpt.Battle = type;
        this.UpdateShowToolTip();
    }

    //log:lz-2016.05.04 增加mBtnCk上的悬浮提示
    void UpdateShowToolTip()
    {
        if (null==this.m_ShowToolTipItem)
        {
            this.m_ShowToolTipItem=this.mBtnCk.gameObject.AddComponent<ShowToolTipItem_N>();
        }
        int strID=0;
        switch (m_Cmpt.Battle)
        {
            case ENpcBattle.Attack:
                strID = 10077;
                break;
            case ENpcBattle.Defence:
                strID = 10078;
                break;
            case ENpcBattle.Passive:
                strID = 10079;
                break;
            case ENpcBattle.Evasion:
                break;
            case ENpcBattle.Stay:
                strID = 10076;
                break;
        }
        if (0 != strID)
        {
            this.m_ShowToolTipItem.mStrID = strID;
        }
    }


    void ShowBattle()
    {
        mOldBattlType = mBattlType;
        switch (m_Cmpt.Battle)
        {
            case ENpcBattle.Attack:
                {
                    mAttackBtn.SetActive(true);
                    mDefenceBtn.SetActive(false);
                    mRestBtn.SetActive(false);
                    mStayBtn.SetActive(false);

                    //if (mChoseList.activeSelf)
                    //{
                    //    mAttCk.isChecked = true;
                    //    mDefCk.isChecked = false;
                    //    mResCk.isChecked = false;
                    //}
                    mAttCk.isChecked = true;
                    mDefCk.isChecked = false;
                    mResCk.isChecked = false;
                    mStayCk.isChecked = false;
                }
                break;
            case ENpcBattle.Defence:
                {
                    mAttackBtn.SetActive(false);
                    mDefenceBtn.SetActive(true);
                    mRestBtn.SetActive(false);
                    mStayBtn.SetActive(false);

                    //if (mChoseList.activeSelf)
                    //{
                    //    mAttCk.isChecked = false;
                    //    mDefCk.isChecked = true;
                    //    mResCk.isChecked = false;
                    //}
                    mAttCk.isChecked = false;
                    mDefCk.isChecked = true;
                    mResCk.isChecked = false;
                    mStayCk.isChecked = false;
                }
                break;
            case ENpcBattle.Passive:
                {
                    mAttackBtn.SetActive(false);
                    mDefenceBtn.SetActive(false);
                    mRestBtn.SetActive(true);
                    mStayBtn.SetActive(false);

                    //if (mChoseList.activeSelf)
                    //{
                    //    mAttCk.isChecked = false;
                    //    mDefCk.isChecked = false;
                    //    mResCk.isChecked = true;
                    //}
                    mAttCk.isChecked = false;
                    mDefCk.isChecked = false;
                    mResCk.isChecked = true;
                    mStayCk.isChecked = false;

                }
                break;
            case ENpcBattle.Stay:
                {
                    mAttackBtn.SetActive(false);
                    mDefenceBtn.SetActive(false);
                    mRestBtn.SetActive(false);
                    mStayBtn.SetActive(true);

                    //if (mChoseList.activeSelf)
                    //{
                    //    mAttCk.isChecked = false;
                    //    mDefCk.isChecked = false;
                    //    mResCk.isChecked = true;
                    //}
                    mAttCk.isChecked = false;
                    mDefCk.isChecked = false;
                    mResCk.isChecked = false;
                    mStayCk.isChecked = true;
                }
                break;
            default:
                break;
        }

        //if (init)//第一次打开
        //{
        //    StartCoroutine(BtnCkStateChange());
        //}
        //else
        //{
        //    init = true;
        //}
        return;
    }

    IEnumerator BtnCkStateChange(float _waittime)
    {
        yield return new WaitForSeconds(_waittime);
        //mBtnCk.isChecked = !mBtnCk.isChecked;
        mChoseList.SetActive(false);
        mBtnCk.isChecked = false;
        init = false;
    }

    public void SetNPCHpPercent(float HpPercent)
    {
        mNpcBlood.fillAmount = HpPercent;
    }

    public void SetNpcHeadTextre(Texture tex)
    {
        mNPCTexture.mainTexture = tex;
    }



    void OnAttackChosebtn(bool active)
    {
        if (!active)
            return;
        ChangeBattle(ENpcBattle.Attack);
    }

    void OnDefenceChoseBtn(bool active)
    {
        if (!active)
            return;
        ChangeBattle(ENpcBattle.Defence);
    }

    void OnRestChoseBtn(bool active)
    {
        if (!active)
            return;
        ChangeBattle(ENpcBattle.Passive);
    }

    void OnStayChooseBtn(bool active)
    {
        if (!active)
            return;
        ChangeBattle(ENpcBattle.Stay);
    }

    void OnServantRevive()
    {
        if (this.IsDead)
        {
            //log:lz-2016.05.25 唐小力说在复活范围内的时候可以复活，没在就提示没在范围内
            if (this.m_Cmpt.CanRecive)
            {
                GameUI.Instance.mRevive.ShowServantRevive(m_Cmpt);
            }
            else
            {
                MessageBox_N.ShowOkBox(PELocalization.GetString(8000188));
            }
        }
    }

    //log: lz-2016.05.19 是否有复活药
    public bool HasReviveMedicine()
    {
        if (this.m_PlayerPackage == null)
        {
            if (GameUI.Instance != null && GameUI.Instance.mMainPlayer != null)
            {
                this.m_PlayerPackage = GameUI.Instance.mMainPlayer.GetCmpt<PlayerPackageCmpt>();
            }
            if (this.m_PlayerPackage == null)
                return false;
        }
        int itemCount = this.m_PlayerPackage.GetItemCount(m_RevivePrtroId);
        return itemCount > 0;
    }

    void OnServantWndShow()
    {
        GameUI.Instance.mServantWndCtrl.mCurrentIndex = (UIServantWnd.ServantIndex)mIndex;
        GameUI.Instance.mServantWndCtrl.Show();
    }

    void ShowOptions(bool active)
    {
        if (!mChoseList.activeSelf && active)
            mChoseList.SetActive(active);
    }

    //lz-2016.09.12 更新NPC工作状态
    void UpdateWorkState()
    {
        this.m_WorkSpr.enabled = (null==m_Cmpt)?false:m_Cmpt.FollowerWork;
    }

}
