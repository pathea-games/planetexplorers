using UnityEngine;
using System.Collections;
using Pathea;
using Pathea.PeEntityExt;

public class CSUI_NpcGridItem : MonoBehaviour
{

    #region UI_WIDGET

    [SerializeField]
    UISlicedSprite m_CheckMark;
    [SerializeField]
    UITexture m_IconTex;
    [SerializeField]
    UISlicedSprite m_IconSprite;
    [SerializeField]
    UISlicedSprite m_EmptyIcon;

    // State Icon
    [SerializeField]
    UISlicedSprite m_WorkStateSprite;
    [SerializeField]
    UISlicedSprite m_RestStateSprite;
    [SerializeField]
    UISlicedSprite m_IdleStateSprite;
    [SerializeField]
    UISlicedSprite m_FollowStateSprite;
    [SerializeField]
    UISlicedSprite m_PrepareStateSprite;
    [SerializeField]
    UISlicedSprite m_DeadStateSprite;
    [SerializeField]
    UISlicedSprite m_AttackStateSprite;
    [SerializeField]
    UISlicedSprite m_PatrolStateSprite;
    [SerializeField]
    UISlicedSprite m_PlantStateSprite;
    [SerializeField]
    UISlicedSprite m_WateringStateSprite;
    [SerializeField]
    UISlicedSprite m_CleaningStateSprite;
    [SerializeField]
    UISlicedSprite m_GainStateSprite;
    [SerializeField]
    UISlicedSprite m_SickSprite;

    // State Label
    [SerializeField]
    UILabel m_StateLabel;

    // Delete Button
    [SerializeField]
    UIButton m_DeleteButton;
    [SerializeField]
    UILabel m_TimeLabel;

    public UILabel m_NpcNameLabel;


    #endregion

    public bool m_UseDeletebutton = true;
    //public bool m_UseTimeLabel = false;//是否显示时间
    //public bool m_UseNpcNameLabel = false;//是否显示npc的名字

    public CSPersonnel m_Npc;
    private NpcCmpt m_NpcCmpt;

    private int lastState = 0;
    public Transform NpcIconRadio
    {
        get
        {
            return gameObject.GetComponent<UICheckbox>().radioButtonRoot;
        }

        set
        {
            gameObject.GetComponent<UICheckbox>().radioButtonRoot = value;
        }
    }

    public delegate void NpcGridEvent(CSUI_NpcGridItem plantGrid);
    public NpcGridEvent OnDestroySelf;

    //public delegate void TimeDel(CSPersonnel _npc);
    //public event TimeDel TimeEvent;

    void OnActivate(bool active)
    {
        if (m_UseDeletebutton && m_Npc != null)
            m_DeleteButton.gameObject.SetActive(active);
        else
            m_DeleteButton.gameObject.SetActive(false);

        if (active)
            m_CheckMark.color = new Color(1f, 1f, 1f, 0f);
        else
            m_CheckMark.color = new Color(1f, 1f, 1f, 1f);

        if (m_Npc != null)

            if (!active)
                return;
        if (m_Npc == null)
            return;

        if (m_Npc.IsRandomNpc && GameUI.Instance != null)
        {
            //--to do: wait
            //NpcRescue npcRescue = m_Npc.NPC.GetGameObject().GetComponent<NpcRescue>();
            //if (npcRescue != null)
            //{
            //    if (GameUI.Instance.mItemGetGui.UpdateItem(npcRandom))
            //    {
            //        GameUI.Instance.mItemGetGui.Show();
            //    }

            //    if (npcRandom.Recruited)
            //    {
            //        //GameUI.Instance.mReviveGui.ShowServantRevive(npcRandom);
            //    }
            //}
        }
        //**********************WangCan*********************
        if (m_Npc != null && ShowFace != null)
        {
            ShowFace(m_Npc);
        }
        if (m_Npc != null && GetInstructorSkill != null)
            GetInstructorSkill(m_Npc);
        if (m_Npc != null && ShowInstructorSkill != null)
            ShowInstructorSkill(m_Npc);

    }

    void OnDeleteBtn()
    {
        if (OnDestroySelf != null)
            OnDestroySelf(this);
    }

    #region UNITY_INNER

    void Awake()
    {
    }
    // Use this for initialization
    void Start()
    {
        SetAllStateFalse();
    }

    //bool m_Flag = false;
    //protected NpcRescue m_npcRescue;
    // Update is called once per frame
    void Update()
    {
        ShowNpcName();
        //lz-2016.11.09 Cursh bug 错误 #5621
        if (m_Npc != null&& m_Npc.m_Npc)
        {
            //if (m_IconTex.mainTexture != m_Npc.m_N)
            m_EmptyIcon.enabled = false;


            //--to do: wait
            //if (m_Npc.m_Npc.IsRandomNpc())
            if (m_Npc.RandomNpcFace != null)
            {
                m_IconTex.mainTexture = m_Npc.RandomNpcFace;
                m_IconSprite.enabled = false;
                m_IconTex.enabled = true;
            }
            else
            {
                m_IconSprite.enabled = true;
                m_IconTex.enabled = false;
                m_IconSprite.spriteName = m_Npc.MainNpcFace;
            }

            if (m_NpcCmpt == null)
            {
                m_NpcCmpt = m_Npc.m_Npc.GetCmpt<NpcCmpt>();
            }
            else
            {
                m_SickSprite.enabled = m_NpcCmpt.IsNeedMedicine;
            }

            if (lastState != m_Npc.State)
            {
                SetAllStateFalse();
                switch (m_Npc.State)
                {
                    case CSConst.pstPrepare:
                        m_PrepareStateSprite.enabled = true;
                        m_StateLabel.text = "Going...";
                        break;
                    case CSConst.pstIdle:
                        m_IdleStateSprite.enabled = true;
                        break;
                    case CSConst.pstRest:
                        m_RestStateSprite.enabled = true;
                        break;
                    case CSConst.pstWork:
                        m_WorkStateSprite.enabled = true;
                        break;
                    case CSConst.pstFollow:
                        m_FollowStateSprite.enabled = true;
                        break;
                    case CSConst.pstDead:
                        m_DeadStateSprite.enabled = true;
                        break;
                    case CSConst.pstAtk:
                        m_AttackStateSprite.enabled = true;
                        break;
                    case CSConst.pstPatrol:
                        m_PatrolStateSprite.enabled = true;
                        break;
                    case CSConst.pstPlant:
                        m_PlantStateSprite.enabled = true;
                        break;
                    case CSConst.pstWatering:
                        m_WateringStateSprite.enabled = true;
                        break;
                    case CSConst.pstWeeding:
                        m_CleaningStateSprite.enabled = true;
                        break;
                    case CSConst.pstGain:
                        m_GainStateSprite.enabled = true;
                        break;
                    default:
                        break;
                }
                lastState = m_Npc.State;
            }
        }
        else
        {
            SetAllStateFalse();
            CloseDiseaseState();
            m_EmptyIcon.enabled = true;


            m_IconTex.enabled = false;
            m_IconSprite.enabled = false;
            m_IconTex.mainTexture = null;

            //m_Flag = false;
        }
    }

    void CloseDiseaseState()
    {
        m_NpcCmpt = null;
        m_SickSprite.enabled = false;
    }

    public void ShowNpcName()
    {
        if (m_Npc != null && m_NpcNameLabel != null && m_NpcNameLabel.enabled == true)
            m_NpcNameLabel.text = m_Npc.FullName;
    }

    private int _minute, _second;//时、分、秒
    public void NpcGridTimeShow(float _time)
    {
        if (m_TimeLabel != null)
        {
            _minute = (int)(_time / 60);
            _second = (int)(_time - _minute * 60);

            m_TimeLabel.text = TimeTransition(_minute) + ":" + TimeTransition(_second);
        }
    }

    private string TimeTransition(int _number)
    {
        if (_number < 10)
            return "0" + _number.ToString();
        else
            return _number.ToString();
    }

    #endregion

    private void SetAllStateFalse()
    {
        m_RestStateSprite.enabled = false;
        m_IdleStateSprite.enabled = false;
        m_WorkStateSprite.enabled = false;
        m_FollowStateSprite.enabled = false;
        m_PrepareStateSprite.enabled = false;
        m_DeadStateSprite.enabled = false;
        m_AttackStateSprite.enabled = false;
        m_PatrolStateSprite.enabled = false;
        m_PlantStateSprite.enabled = false;
        m_WateringStateSprite.enabled = false;
        m_CleaningStateSprite.enabled = false;
        m_GainStateSprite.enabled = false;
        m_StateLabel.text = "";
    }



    //************************WangCan*****************************
    public delegate void ClickDel(CSPersonnel csp);
    public event ClickDel GetInstructorSkill;
    public event ClickDel ShowFace;
    public event ClickDel ShowInstructorSkill;


    void OnTooltip(bool show)
    {
        if (!show)
        {
            ToolTipsMgr.ShowText(null);
            return;
        }
        if (m_Npc != null)
			ToolTipsMgr.ShowText(m_Npc.FullName);
        //if (show)
        //    Debug.Log("进来了");
    }
}
