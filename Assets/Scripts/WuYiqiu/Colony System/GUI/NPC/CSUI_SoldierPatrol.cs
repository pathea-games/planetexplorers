using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CSUI_SoldierPatrol : MonoBehaviour 
{
	#region UI_WIDGET

	[SerializeField] UITable  m_EntityRootUI;
    [SerializeField] UIScrollBar m_ScrollBar;

    #endregion

    [SerializeField] CSUI_EntityState  m_EntityStatePrefab;

	private List<CSUI_EntityState>	m_EntitesState = new List<CSUI_EntityState>();

	private CSPersonnel  m_RefNpc;
	public CSPersonnel RefNpc  
	{
		get { return m_RefNpc; }
		set 
		{ 
			m_RefNpc = value;
		}
	}

	private CSPersonnel m_OldRefNpc;

	void OnEnable()
	{

	}

	void OnDisable()
	{
        ClearEntites();
        m_OldRefNpc = null;
	}

    void ClearEntites()
    {
        foreach (CSUI_EntityState es in m_EntitesState)
        {
            GameObject.Destroy(es.gameObject);
            es.m_RefCommon.RemoveEventListener(OnEntityEventListener);
        }
        m_EntitesState.Clear();
    }

	// Use this for initialization
	void Start () 
	{
	
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (m_OldRefNpc != m_RefNpc)
		{
            //lz-2016.10.10 每次更新要清除以前的Entities
            ClearEntites();
//            CSCreator creator = CSUI_MainWndCtrl.Instance.Creator;
            List<CSEntity> entities = m_RefNpc.GetProtectedEntities();
            m_RefNpc.GuardEntities = entities;
			if (entities != null)
			{
				foreach (CSEntity cse in entities)
				{
					CSUI_EntityState es 		= Instantiate(m_EntityStatePrefab) as CSUI_EntityState;
					es.transform.parent			= m_EntityRootUI.transform;
					es.transform.localPosition	= Vector3.zero;
					es.transform.localRotation	= Quaternion.identity;
					es.transform.localScale		= Vector3.one;
					
					es.m_RefCommon		= cse;
					
					m_EntitesState.Add(es);
					
					cse.AddEventListener(OnEntityEventListener);
				}
				
				m_EntityRootUI.repositionNow = true;
                m_ScrollBar.scrollValue = 0;
            }

			m_OldRefNpc = m_RefNpc;
		}
	}

	void OnEntityEventListener(int event_id, CSEntity entity, object arg)
	{
		CSCommon csc = entity as CSCommon;
		if (csc == null)
			return;
		if (event_id == CSConst.eetDestroy)
		{
			csc.RemoveEventListener(OnEntityEventListener);
			
			CSUI_EntityState es = m_EntitesState.Find(item0 => item0.m_RefCommon == csc);
			if (es != null)
			{
				GameObject.Destroy(es.gameObject);
				m_EntitesState.Remove(es);
				m_EntityRootUI.repositionNow = true;
			}
		}
	}
}
