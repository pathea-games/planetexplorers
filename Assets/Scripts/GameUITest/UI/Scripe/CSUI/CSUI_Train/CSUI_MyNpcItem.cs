using UnityEngine;
using System.Collections;

public class CSUI_MyNpcItem : MonoBehaviour
{

    public Texture linshiyong;


    [HideInInspector]
    public CSUIMyNpc m_Npc;


    [SerializeField]
    UISlicedSprite m_EmptyIcon;
    [SerializeField]
    UITexture m_IconTex;
    [SerializeField]
    UISlicedSprite m_IconSprite;
    [SerializeField]
    UIButton m_DeleteButton;

    public bool m_UseDeletebutton = true;

    //public delegate void ClickDel(CSUI_MyNpcItem mni);
    //public event ClickDel ClickEvent;
    void OnActivate(bool active)
    {
        if (m_UseDeletebutton && m_Npc != null)
            m_DeleteButton.gameObject.SetActive(active);
        else
            m_DeleteButton.gameObject.SetActive(false);

    }

    void Update()
    {

        if (m_Npc != null)
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
            if (m_Npc.RandomNpcFace == null)
            {
                m_IconTex.mainTexture = linshiyong;
                m_IconSprite.enabled = false;
                m_IconTex.enabled = true;
            }
            //else
            //{
            //    m_IconSprite.enabled = true;
            //    m_IconTex.enabled = false;
            //    m_IconSprite.spriteName = m_Npc.MainNpcFace;
            //}




            //    if (lastState != m_Npc.State)
            //    {
            //        SetAllStateFalse();
            //        switch (m_Npc.State)
            //        {
            //            case CSConst.pstPrepare:
            //                m_PrepareStateSprite.enabled = true;
            //                m_StateLabel.text = "Going...";
            //                break;
            //            case CSConst.pstIdle:
            //                m_IdleStateSprite.enabled = true;
            //                break;
            //            case CSConst.pstRest:
            //                m_RestStateSprite.enabled = true;
            //                break;
            //            case CSConst.pstWork:
            //                m_WorkStateSprite.enabled = true;
            //                break;
            //            case CSConst.pstFollow:
            //                m_FollowStateSprite.enabled = true;
            //                break;
            //            case CSConst.pstDead:
            //                m_DeadStateSprite.enabled = true;
            //                break;
            //            case CSConst.pstAtk:
            //                m_AttackStateSprite.enabled = true;
            //                break;
            //            case CSConst.pstPatrol:
            //                m_PatrolStateSprite.enabled = true;
            //                break;
            //            case CSConst.pstPlant:
            //                m_PlantStateSprite.enabled = true;
            //                break;
            //            case CSConst.pstWatering:
            //                m_WateringStateSprite.enabled = true;
            //                break;
            //            case CSConst.pstWeeding:
            //                m_CleaningStateSprite.enabled = true;
            //                break;
            //            case CSConst.pstGain:
            //                m_GainStateSprite.enabled = true;
            //                break;
            //            default:
            //                break;
            //        }
            //        lastState = m_Npc.State;
            //    }
            //}
            //else
            //{
            //    SetAllStateFalse();

            //    m_EmptyIcon.enabled = true;


            //    m_IconTex.enabled = false;
            //    m_IconSprite.enabled = false;
            //    m_IconTex.mainTexture = null;

            //    m_Flag = false;
            //}
        }
    }
}
