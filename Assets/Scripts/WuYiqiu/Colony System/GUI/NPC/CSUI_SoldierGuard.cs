using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CSUI_SoldierGuard : MonoBehaviour 
{
    //lz-2016.07.25 
//    #region UI_WIDGET
	
//    [SerializeField] UIGrid 		m_EntityRootUI;
//    [SerializeField] UICheckbox		m_ProtectedAreaCB;

//    [SerializeField] CSUI_EntityState  m_EntityStatePrefab;
	
//    #endregion

//    [SerializeField] CSGuardRangeEffect m_EffectPrefab;
//    private CSGuardRangeEffect m_Effect; 

//    private List<CSUI_EntityState>	m_EntitesState = new List<CSUI_EntityState>();

//    private CSPersonnel  m_RefNpc;
//    public CSPersonnel RefNpc  
//    {
//        get { return m_RefNpc; }
//        set 
//        { 
//            m_RefNpc = value;

//        }
//    }
//    private CSPersonnel m_OldRefNpc;


//    void OnDisable ()
//    {
//        if (m_Effect != null)
//            m_Effect.DelayDestroy(2.5f);

//        m_OldRefNpc = null;
//    }

//    void OnEnable ()
//    {

//        if (m_Effect != null)
//            m_Effect.StopDestroy();

//        if (m_ProtectedAreaCB.isChecked)
//        {
//            CreateEffect ();

//            m_Effect.gameObject.SetActive(true);
//        }
//    }

//    // Use this for initialization
//    void Start () 
//    {

//    }
	
//    // Update is called once per frame
//    void Update () 
//    {
//        if (m_Effect != null)
//        {
//            m_Effect.transform.position = 	m_RefNpc.Data.m_GuardPos;
//            m_Effect.CubeEffectFollower = m_RefNpc.transform;
//        }

//        if (m_OldRefNpc != m_RefNpc)
//        {
//            UpdateProtectedEntity ();
//            m_OldRefNpc = m_RefNpc;
//        }
//    }
	
//    void CreateEffect ()
//    {
//        if (m_Effect == null)
//        {
//            m_Effect = Instantiate(m_EffectPrefab) as CSGuardRangeEffect;
//            m_Effect.name = " NPC Protected area effect";
//            m_Effect.Radius = 27;
//        }

//    }

//    void UpdateProtectedEntity ()
//    {
//        if (m_RefNpc == null)
//            return;

//        List<CSEntity> entities = m_RefNpc.GetProtectedEntities();
//        m_RefNpc.GuardEntities = entities;

//        int i = 0;
//        int old_cnt = m_EntitesState.Count;
//        for (; i < entities.Count; i++)
//        {
//            if ( i < m_EntitesState.Count)
//            {
//                m_EntitesState[i].m_RefCommon = entities[i];
//            }
//            else
//            {
//                CSUI_EntityState es 		= Instantiate(m_EntityStatePrefab) as CSUI_EntityState;
//                es.transform.parent			= m_EntityRootUI.transform;
//                es.transform.localPosition	= Vector3.zero;
//                es.transform.localRotation	= Quaternion.identity;
//                es.transform.localScale		= Vector3.one;

//                es.m_RefCommon = entities[i];
//                m_EntitesState.Add(es);

//                entities[i].AddEventListener(OnEntityEventListener);
//            }
//        }

//        for (int j = i; j < m_EntitesState.Count; )
//        {
//            m_EntitesState[j].m_RefCommon.RemoveEventListener(OnEntityEventListener);
//            GameObject.Destroy(m_EntitesState[j].gameObject);
//            m_EntitesState.RemoveAt(j);
//        }

//        if (old_cnt != m_EntitesState.Count)
//            m_EntityRootUI.repositionNow = true;

//    }

//    #region UI_CALLBACK

//    void OnShowProtectedArea(bool active)
//    {
//        if (active)
//        {
//            CreateEffect ();

//            m_Effect.gameObject.SetActive(true);

//            CSUI_MainWndCtrl.ShowStatusBar(UIMsgBoxInfo.mGuardArea.GetString());
//        }
//        else
//        {
//            if (m_Effect != null)
//                m_Effect.gameObject.SetActive(false);
//        }
//    }

//    void OnGuardPosBtn()
//    {
//#if DEBUG
//        if (m_RefNpc != null)
//        {
//            Debug.LogError("The npc reference is missing, set it first?");
//            Debug.DebugBreak();
//        }
//#endif

//        if (Pathea.PeCreature.Instance.mainPlayer != null)
//        {
//            m_RefNpc.SetGuardAttr(Pathea.PeCreature.Instance.mainPlayer.position);
//            UpdateProtectedEntity ();

//            CSUI_MainWndCtrl.ShowStatusBar(UIMsgBoxInfo.mGuardPos.GetString());
//        }
//    }
//    #endregion

//    void OnEntityEventListener(int event_id, CSEntity entity, object arg)
//    {
//        CSCommon csc = entity as CSCommon;
//        if (csc == null)
//            return;
//        if (event_id == CSConst.eetDestroy)
//        {
//            csc.RemoveEventListener(OnEntityEventListener);
			
//            CSUI_EntityState es = m_EntitesState.Find(item0 => item0.m_RefCommon == csc);
//            if (es != null)
//            {
//                GameObject.Destroy(es.gameObject);
//                m_EntitesState.Remove(es);
//                m_EntityRootUI.repositionNow = true;
//            }
//        }
//    }
}
