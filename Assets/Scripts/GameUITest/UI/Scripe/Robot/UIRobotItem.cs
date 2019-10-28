using UnityEngine;
using System.Collections;
using ItemAsset;
using Pathea;
using WhiteCat;

public class UIRobotItem : MonoBehaviour
{

    [SerializeField]
    GameObject mAttackBtn;
    [SerializeField]
    GameObject mDefenceBtn;
    [SerializeField]
    GameObject mCureBtn;
    [SerializeField]
    GameObject mRestBtn;
    [SerializeField]
    GameObject mChoseList;

    [SerializeField]
    UICheckbox mAttCk;
    [SerializeField]
    UICheckbox mDefCk;
    [SerializeField]
    UICheckbox mCureCk;
    [SerializeField]
    UICheckbox mResCk;
    [SerializeField]
    UICheckbox mBtnCk;

    [SerializeField]
    BoxCollider mAttCol;
    [SerializeField]
    BoxCollider mDefCol;
    [SerializeField]
    BoxCollider mCureCol;
    [SerializeField]
    BoxCollider mResCol;

    [SerializeField]
    UISlider mHealth;
    [SerializeField]
    UISlider mEnergy;
    [SerializeField]
    UITexture mHeadTex;

    [SerializeField]
    TweenAlpha mAnimation;
    [SerializeField]
    UISlicedSprite mForeground;

    [HideInInspector]
    public ItemObject mItemObj = null;
    [HideInInspector]
    public GameObject mGameobj = null;


    AIMode mBattlType = AIMode.Passive;
    AIMode mOldBattlType = AIMode.Passive;

    bool init = false;

    LifeLimit mLifeCmpt;
    Energy mEnergyCmpt;

    void Update()
    {
        RefreshInfo();

        if (RobotController.playerFollower != null)
            mBattlType = RobotController.playerFollower.aiMode;
        if (mOldBattlType != mBattlType)
            ShowBattle();
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
                    if (mAttCol.Raycast(ray, out rayHit, 300) || mDefCol.Raycast(ray, out rayHit, 300) || mCureCol.Raycast(ray, out rayHit, 300) || mResCol.Raycast(ray, out rayHit, 300))
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

    IEnumerator BtnCkStateChange(float _waittime)
    {
        yield return new WaitForSeconds(_waittime);
        mChoseList.SetActive(false);
        mBtnCk.isChecked = false;
        init = false;
    }

    void ShowBattle()
    {
        mOldBattlType = mBattlType;
        switch (RobotController.playerFollower.aiMode)
        {
            case AIMode.Attack:
                {
                    mAttackBtn.SetActive(true);
                    mDefenceBtn.SetActive(false);
                    mCureBtn.SetActive(false);
                    mRestBtn.SetActive(false);

                    mAttCk.isChecked = true;
                    mDefCk.isChecked = false;
                    mCureCk.isChecked = false;
                    mResCk.isChecked = false;
                }
                break;
            case AIMode.Defence:
                {
                    mAttackBtn.SetActive(false);
                    mDefenceBtn.SetActive(true);
                    mCureBtn.SetActive(false);
                    mRestBtn.SetActive(false);

                    mAttCk.isChecked = false;
                    mDefCk.isChecked = true;
                    mCureCk.isChecked = false;
                    mResCk.isChecked = false;
                }
                break;
            case AIMode.Cure:
                {
                    mAttackBtn.SetActive(false);
                    mDefenceBtn.SetActive(false);
                    mCureBtn.SetActive(true);
                    mRestBtn.SetActive(false);

                    mAttCk.isChecked = false;
                    mDefCk.isChecked = false;
                    mCureCk.isChecked = true;
                    mResCk.isChecked = false;
                }
                break;
            case AIMode.Passive:
                {
                    mAttackBtn.SetActive(false);
                    mDefenceBtn.SetActive(false);
                    mCureBtn.SetActive(false);
                    mRestBtn.SetActive(true);

                    mAttCk.isChecked = false;
                    mDefCk.isChecked = false;
                    mCureCk.isChecked = false;
                    mResCk.isChecked = true;
                }
                break;
            default:
                break;
        }
        return;
    }

    void RefreshInfo()
    {
        if (mItemObj == null)
            return;
        mHeadTex.mainTexture = mItemObj.iconTex;
        mLifeCmpt = mItemObj.GetCmpt<LifeLimit>();
        mEnergyCmpt = mItemObj.GetCmpt<Energy>();
        mHealth.sliderValue = mLifeCmpt.floatValue.percent;
        mEnergy.sliderValue = mEnergyCmpt.floatValue.percent;

        if (mEnergyCmpt.floatValue.percent < 0.30f)
        {
            mAnimation.enabled = true;
        }
        else
        {
            mAnimation.enabled = false;
            mForeground.alpha = 1f;
        }
    }

    void OnBtnClick()
    {
        if (mGameobj == null)
            return;
        DragItemMousePickRobot _cmpt = mGameobj.GetComponent<DragItemMousePickRobot>();
        if (_cmpt != null)
            _cmpt.DoGetItem();
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }

    public bool IsShow { get { return gameObject.activeSelf; } }

    void ShowOptions(bool active)
    {
        if (!mChoseList.activeSelf && active)
            mChoseList.SetActive(active);
    }

    void OnAttackChosebtn(bool active)
    {
        if (!active)
            return;
        ChangeBattle(AIMode.Attack);
    }

    void OnDefenceChoseBtn(bool active)
    {
        if (!active)
            return;
        ChangeBattle(AIMode.Defence);
    }

    void OnRestChoseBtn(bool active)
    {
        if (!active)
            return;
        ChangeBattle(AIMode.Passive);
    }

    void OnCureChoseBtn(bool active)
    {
        if (!active)
            return;
        ChangeBattle(AIMode.Cure);
    }

    void ChangeBattle(AIMode type)
    {
        if (RobotController.playerFollower == null)
            return;
        RobotController.playerFollower.aiMode = type;
    }
}
