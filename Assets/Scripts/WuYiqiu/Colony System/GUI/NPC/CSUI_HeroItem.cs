using UnityEngine;
using System.Collections;
using Pathea;
using Pathea.PeEntityExt;

public class CSUI_HeroItem : MonoBehaviour
{
    #region UI_WIDGET

    [SerializeField]
    UILabel m_NameLbUI;
    [SerializeField]
    UIButton m_BtnUI;
    [SerializeField]
    UILabel m_BtnLbUI;
    [SerializeField]
    ServantShowItem_N m_ServantShowingUI;

    #endregion

    public int m_HeroIndex = 0;

    public CSPersonnel HandlePerson = null;

    private PeEntity m_CurNpc;

    private bool m_Cancel = false;
    private bool m_Replace = false;

    private bool m_Runing = false;

    //lz-2016.06.15 仆从死后，基地仆从按钮不可操作
    private bool m_Active { get {
        if (m_Cancel)
        {
            //Replace： 要设置的仆从位置不为空，并且没有死
            return m_Runing && (null != m_CurNpc &&!m_CurNpc.IsDead());     
        }
        else if (m_Replace)
        {
            //Replace： 要设置的仆从位置不为空，并且没有死,选中的npc不为空，并且没有死
            return m_Runing && (null != m_CurNpc && !m_CurNpc.IsDead()) && (null != HandlePerson && null != HandlePerson.m_Npc && !HandlePerson.m_Npc.IsDead());
        }
        else
        {
            //Set： 要设置的仆从位置为空,选中的npc不为空，并且没有死
            return m_Runing && null == m_CurNpc && (null != HandlePerson && null != HandlePerson.m_Npc && !HandlePerson.m_Npc.IsDead()); 
        }
        
    } }

    public void Activate(bool running)
    {
        m_Runing = running;
    }

    // Use this for initialization
    void Start()
    {
        m_NameLbUI.text = "";
        m_BtnLbUI.text = "Set";
        m_BtnUI.isEnabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        NpcCmpt npcCmpt = PeCreature.Instance.mainPlayer.GetCmpt<ServantLeaderCmpt>().GetServant(m_HeroIndex);
        if (npcCmpt != null)
        {
            PeEntity npcRd = npcCmpt.Entity;
            if (npcRd != m_CurNpc)
            {
                //m_NameLbUI.text = npcCmpt.name;
                m_NameLbUI.text = npcCmpt.Entity.enityInfoCmpt.characterName.ToString();
                m_ServantShowingUI.SetNpc(npcRd);
                m_CurNpc = npcRd;
            }

            m_Cancel = false;
            m_Replace = false;
            if (HandlePerson != null)
            {

                if (HandlePerson.m_Npc == m_CurNpc)
                {
                    m_BtnLbUI.text = "Cancel";
                    m_Cancel = true;
                    m_Replace = false;
                    m_BtnUI.isEnabled = m_Active;
                }
                else if (HandlePerson.NPC.IsFollower())
                {
                    m_BtnUI.isEnabled = false;
                    m_BtnLbUI.text = "Replace";
                }
                else
                {
                    m_BtnLbUI.text = "Replace";
                    m_Replace = true;
                    m_BtnUI.isEnabled = m_Active;
                }
            }
            else
                m_BtnUI.isEnabled = m_Active;
        }
        else
        {
            m_CurNpc = null;
            m_NameLbUI.text = "";
            m_ServantShowingUI.SetNpc(null);
            m_BtnLbUI.text = "Set";
            m_Cancel = false;

            if (HandlePerson != null)
            {
                if (HandlePerson.NPC.IsFollower())
                {
                    m_BtnUI.isEnabled = false;
                }
                else
                {
                    //lz-2016.06.01 这里不能直接设置为为true，还要考虑基地是否存在和有电
                    m_BtnUI.isEnabled = m_Active;
                }
            }
        }
    }

    #region NGUI_CALLBACK

    void OnSetBtn()
    {
        if (!m_Cancel)
        {// add/replace follower
            if (HandlePerson != null)
            {
                if (m_Replace)
                {   // follower
                    if (PeGameMgr.IsMulti)
                    {
                        if (m_CurNpc != null)
                        {
                            ServantLeaderCmpt masterCmpt = PeCreature.Instance.mainPlayer.GetCmpt<ServantLeaderCmpt>();
                            NpcCmpt npcCmpt = m_CurNpc.GetComponent<NpcCmpt>();
                            if (masterCmpt == npcCmpt.Master)
                            {
                                if (null != PlayerNetwork.mainPlayer)
                                {
                                    PlayerNetwork.RequestDismissNpc(m_CurNpc.Id);
                                    PlayerNetwork.RequestNpcRecruit(HandlePerson.ID);
                                }
                            }
                            else
                            {
                                //--to do: log
                            }
                        }
                    }
                    else
                    {
                        if (m_CurNpc != null)
                            m_CurNpc.SetFollower(false);
                        HandlePerson.FollowMe(m_HeroIndex);
                    }
                }
                else
                {
                    //check if can add person
                    if (PeGameMgr.IsMulti)
                    {
                        if (null != PlayerNetwork.mainPlayer)
                            PlayerNetwork.RequestNpcRecruit(HandlePerson.ID);
                    }
                    else
                    {
                        HandlePerson.FollowMe(m_HeroIndex);
                    }
                }
            }
        }
        else
        {// remove follower
            if (m_CurNpc != null)
            {
                if (PeGameMgr.IsMulti)
                {
                    if (null != PlayerNetwork.mainPlayer)
                        PlayerNetwork.RequestDismissNpc(m_CurNpc.Id);
                }
                else
                {
                    m_CurNpc.SetFollower(false);
                }
            }
        }
    }

    #endregion
}
