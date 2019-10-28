using UnityEngine;
using System.Collections;
using Pathea;
using Pathea.PeEntityExt;
public class CSUI_NPCGrid : MonoBehaviour
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

    // Show State
    [SerializeField]
    ShowToolTipItem_N m_ShowToolTipItem;

    // Delete Button
    [SerializeField]
    UIButton m_DeleteButton;

    #endregion

    public bool m_UseDeletebutton = true;

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

    public delegate void NpcGridEvent(CSUI_NPCGrid plantGrid);
    public NpcGridEvent OnDestroySelf;



    public void OnActivate(bool active)
    {
        if (m_UseDeletebutton && m_Npc != null)
            m_DeleteButton.gameObject.SetActive(active);
        else
            m_DeleteButton.gameObject.SetActive(false);

        if (active)
            m_CheckMark.color = new Color(1f, 1f, 1f, 0f);
        else
            m_CheckMark.color = new Color(1f, 1f, 1f, 1f);

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

		if (m_Npc != null&&m_Npc.m_Npc!=null)
        {
            //if (m_IconTex.mainTexture != m_Npc.m_N)
            m_EmptyIcon.enabled = false;


            //--to do: wait
            //lz-2016.06.30 这里不能通过判断m_Npc.RandomNpcFace是不是空的来决定使用Texture还是Sprite显示(1.随机npc一定是照相的图像，用Texture赋值 2.主线npc一定是图集里面的)
            if (m_Npc.IsRandomNpc)
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
                int workTipID = -1;
                switch (m_Npc.State)
                {
                    case CSConst.pstPrepare:
                        m_PrepareStateSprite.enabled = true;
                        workTipID = 8000575;
                        
                        break;
                    case CSConst.pstIdle:
                        m_IdleStateSprite.enabled = true;
                        workTipID = 8000576;
                        break;
                    case CSConst.pstRest:
                        m_RestStateSprite.enabled = true;
                        workTipID = 8000577;
                        break;
                    case CSConst.pstWork:
                        m_WorkStateSprite.enabled = true;
                        workTipID = 8000578;
                        break;
                    case CSConst.pstFollow:
                        m_FollowStateSprite.enabled = true;
                        workTipID = 8000579;
                        break;
                    case CSConst.pstDead:
                        m_DeadStateSprite.enabled = true;
                        workTipID = 8000580;
                        break;
                    case CSConst.pstAtk:
                        m_AttackStateSprite.enabled = true;
                        workTipID = 8000581;
                        break;
                    case CSConst.pstPatrol:
                        m_PatrolStateSprite.enabled = true;
                        workTipID = 8000582;
                        break;
                    case CSConst.pstPlant:
                        m_PlantStateSprite.enabled = true;
                        workTipID = 8000583;
                        break;
                    case CSConst.pstWatering:
                        m_WateringStateSprite.enabled = true;
                        workTipID = 8000584;
                        break;
                    case CSConst.pstWeeding:
                        m_CleaningStateSprite.enabled = true;
                        workTipID = 8000585;
                        break;
                    case CSConst.pstGain:
                        m_GainStateSprite.enabled = true;
                        workTipID = 8000586;
                        break;
                    default:
                        break;
                }
                lastState = m_Npc.State;
                if (null!= m_ShowToolTipItem && workTipID != -1)
                {
                    m_ShowToolTipItem.mTipContent = PELocalization.GetString(workTipID);
                }
            }
        }
        else
        {
            SetAllStateFalse();

            m_EmptyIcon.enabled = true;


            m_IconTex.enabled = false;
            m_IconSprite.enabled = false;
            m_IconTex.mainTexture = null;

            //m_Flag = false;
        }
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
        if(null!=m_ShowToolTipItem) m_ShowToolTipItem.mTipContent = "";
    }



    //************************WangCan*****************************
    public delegate void ClickDel(CSPersonnel csp);
    public event ClickDel GetInstructorSkill;
    public event ClickDel ShowFace;
    public event ClickDel ShowInstructorSkill;




}
