using UnityEngine;
using System.Collections;
using Pathea;

public class CSUI_TrainLearnPageCtrl : MonoBehaviour
{

    [SerializeField]
    GameObject mLearnSkillGrid;
    [SerializeField]
    GameObject mUpgradeGrid;
    [SerializeField]
    UICheckbox m_LearnSkillCk;
    [SerializeField]
    UICheckbox m_UpgradeCk;

    [SerializeField]
    UITexture mInstructorFace;
    [SerializeField]
    UITexture mTraineeFace;

    [SerializeField]
    ShowToolTipItem_N mTipTrainee;
    [SerializeField]
    ShowToolTipItem_N mTipInstructor;

    [SerializeField]
    UILabel mAttributeItemNameLabel;
    [SerializeField]
    UILabel mAttributeItemPlusLabel;
    [SerializeField]
    UILabel mAttributeItemContentLabel;

    [SerializeField]
    UILabel mUpgradeTimesLabel;
    [SerializeField]
    UILabel mCannotUpgradeLabel;

    //lz-2016.11.06 设置教师和学院用
    [SerializeField]
    N_ImageButton m_InstructorSetBtn;
    [SerializeField]
    UILabel m_InstructorBtnLbl;
    [SerializeField]
    N_ImageButton m_TraineeSetBtn;
    [SerializeField]
    UILabel m_TraineeSetBtnLbl;



    public N_ImageButton mStartBtn;//开始按钮
    public GameObject mStopBtn;//停止按钮

    private ETrainingType mTrainingType = ETrainingType.Skill;
    public ETrainingType TrainingType
    {
        get { return mTrainingType; }
        set { mTrainingType = value; }
    }


    private CSPersonnel m_InsNpc;
    public CSPersonnel InsNpc
    {
        get { return m_InsNpc; }
        set
        {
            if (GameUI.Instance.mCSUI_MainWndCtrl.TrainUI.mTrainingLock)
                return;

            m_InsNpc = value;

            UpdateStatBtnState();

            if (value != null)
            {
                mInstructorFace.mainTexture = value.RandomNpcFace;
                mInstructorFace.enabled = true;
				mTipInstructor.mTipContent = value.FullName;
                if (TraineeNpc != null)
                    ShowAttributeItem(true);

                m_InstructorBtnLbl.text = PELocalization.GetString(8000897);
            }
            else
            {
                mInstructorFace.mainTexture = null;
                mInstructorFace.enabled = false;
                mTipInstructor.mTipContent = "";
                ShowAttributeItem(false);

                m_InstructorBtnLbl.text = PELocalization.GetString(82230010);
            }

            GameUI.Instance.mCSUI_MainWndCtrl.TrainUI.UpdateInstructorSkillsShow(m_InsNpc);
            GameUI.Instance.mCSUI_MainWndCtrl.TrainUI.ClearStudyList();

        }
    }

    private CSPersonnel m_TraineeNpc;
    public CSPersonnel TraineeNpc
    {
        get { return m_TraineeNpc; }
        set
        {
            if (GameUI.Instance.mCSUI_MainWndCtrl.TrainUI.mTrainingLock)
                return;

            m_TraineeNpc = value;

            UpdateStatBtnState();

            if (value != null)
            {
                mTraineeFace.mainTexture = value.RandomNpcFace;
                mTraineeFace.enabled = true;
				mTipTrainee.mTipContent = value.FullName;

                if (InsNpc != null)
                    ShowAttributeItem(true);

                m_TraineeSetBtnLbl.text= PELocalization.GetString(8000897);
            }
            else
            {
                mTraineeFace.mainTexture = null;
                mTraineeFace.enabled = false;
                mTipTrainee.mTipContent = "";

                ShowAttributeItem(false);

                m_TraineeSetBtnLbl.text = PELocalization.GetString(82230010);
            }

            GameUI.Instance.mCSUI_MainWndCtrl.TrainUI.ClearStudyList();
        }
    }

    void OnInstructorSetBtn()
    {
        if (null != CSUI_TrainMgr.Instance)
        {
            InsNpc=CSUI_TrainMgr.Instance.RefNpc;
        }
    }

    void OnTraineeSetBtn()
    {
        if (null != CSUI_TrainMgr.Instance)
        {
            TraineeNpc = CSUI_TrainMgr.Instance.RefNpc;
        }
    }

    public void UpdateSetBtnsState(CSUI_TrainMgr.TypeEnu type)
    {
        switch (type)
        {
            case CSUI_TrainMgr.TypeEnu.Instructor:
                m_InstructorSetBtn.isEnabled = mStartBtn.gameObject.activeSelf;
                m_TraineeSetBtn.isEnabled = false;
                break;
            case CSUI_TrainMgr.TypeEnu.Trainee:
                m_InstructorSetBtn.isEnabled = false;
                m_TraineeSetBtn.isEnabled = mStartBtn.gameObject.activeSelf;
                break;
        }
    }

    public void UpdateStatBtnState()
    {
        if (mStartBtn.gameObject.activeSelf)
        {
            mStartBtn.isEnabled = (null != m_InsNpc && null != m_TraineeNpc);
        }
        if (null != CSUI_TrainMgr.Instance)
            UpdateSetBtnsState(CSUI_TrainMgr.Instance.Type);
    }

    void ShowAttributeItem(bool _show)
    {
        if (_show)
        {

            if (!m_TraineeNpc.m_Npc.GetCmpt<NpcCmpt>().CanAttributeUp())
            {
                mCannotUpgradeLabel.enabled = true;
                mUpgradeTimesLabel.text = "";
                mUpgradeTimesLabel.transform.parent.gameObject.SetActive(false);
                mAttributeItemNameLabel.text = "";
                mAttributeItemPlusLabel.text = "";
                mAttributeItemContentLabel.text = "";
                return;
            }

            mCannotUpgradeLabel.enabled = false;
            mUpgradeTimesLabel.text = "[00bbff]" + m_TraineeNpc.m_Npc.GetCmpt<NpcCmpt>().curAttributeUpTimes.ToString() + "/" + AttPlusNPCData.GetPlusCount(m_TraineeNpc.m_Npc.entityProto.protoId).ToString() + "[-]";
            mUpgradeTimesLabel.transform.parent.gameObject.SetActive(true);
            AttribType mType = AttPlusNPCData.GetRandMaxAttribute(m_InsNpc.m_Npc.entityProto.protoId, m_InsNpc.m_Npc.GetCmpt<SkAliveEntity>());
            if (mType == AttribType.Max)
            {
                Debug.Log(m_InsNpc.m_Npc.entityProto.protoId);

                mAttributeItemNameLabel.text = "";
                mAttributeItemPlusLabel.text = "";
                mAttributeItemContentLabel.text = "";
                mUpgradeTimesLabel.text = "";
                mUpgradeTimesLabel.transform.parent.gameObject.SetActive(false);
                mCannotUpgradeLabel.enabled = false;

                return;

            }
            
            float baseVal = m_TraineeNpc.m_Npc.GetAttribute(mType);
            AttPlusNPCData.AttrPlus.RandomInt randomInt = new AttPlusNPCData.AttrPlus.RandomInt();
            if (AttPlusNPCData.GetRandom(m_InsNpc.m_Npc.entityProto.protoId, mType, out randomInt))
            {
                mAttributeItemNameLabel.text = AtToString(mType) + ":";
                mAttributeItemPlusLabel.text = baseVal + "" + "+";
                mAttributeItemContentLabel.text = "[00ff00]" + randomInt.m_Min + "~" + randomInt.m_Max + "[-]";
            }
            else
            {
                Debug.Log("没有获取到属性");
            }
        }
        else
        {
            mAttributeItemNameLabel.text = "";
            mAttributeItemPlusLabel.text = "";
            mAttributeItemContentLabel.text = "";
            mUpgradeTimesLabel.text = "";
            mUpgradeTimesLabel.transform.parent.gameObject.SetActive(false);
            mCannotUpgradeLabel.enabled = false;
        }
    }

    string AtToString(AttribType _type)
    {
        string realname = "";
        switch (_type)
        {
            case AttribType.HpMax:
                realname = PELocalization.GetString(10066);
                break;
            case AttribType.StaminaMax:
                realname = PELocalization.GetString(10067);
                break;
            case AttribType.ComfortMax:
                realname = PELocalization.GetString(8000202);
                break;
            case AttribType.OxygenMax:
                realname = PELocalization.GetString(10068);
                break;
            case AttribType.HungerMax:
                realname = PELocalization.GetString(10071);
                break;
            case AttribType.EnergyMax:
                realname = PELocalization.GetString(10070);
                break;
            case AttribType.ShieldMax:
                realname = PELocalization.GetString(2000014);
                break;
            case AttribType.Atk:
                realname = PELocalization.GetString(10077);
                break;
            case AttribType.Def:
                realname = PELocalization.GetString(10078);
                break;
        }

        return realname;
    }

    void OnSkillPage(bool active)
    {
        mLearnSkillGrid.SetActive(active);
        if (active)
            TrainingType = ETrainingType.Skill;
    }

    void OnAttributePade(bool active)
    {
        mUpgradeGrid.SetActive(active);
        if (active)
            TrainingType = ETrainingType.Attribute;
    }

    // Use this for initialization
    void Start()
    {
		if(m_InsNpc!=null&&mInstructorFace.mainTexture==null){
			mInstructorFace.mainTexture= m_InsNpc.RandomNpcFace;
		   	mInstructorFace.enabled = true;
			mTipInstructor.mTipContent = m_InsNpc.FullName;
		}

		if(m_TraineeNpc!=null&&mTraineeFace.mainTexture==null){
			mTraineeFace.mainTexture= m_TraineeNpc.RandomNpcFace;
			mTraineeFace.enabled = true;
			mTipTrainee.mTipContent = m_TraineeNpc.FullName;
		}
    }

    // Update is called once per frame
    void Update()
    {
        if (TraineeNpc != null)
        {
            mTraineeFace.mainTexture = TraineeNpc.RandomNpcFace;
        }

        if (InsNpc != null)
        {
            mInstructorFace.mainTexture = InsNpc.RandomNpcFace;
        }
    }
}
