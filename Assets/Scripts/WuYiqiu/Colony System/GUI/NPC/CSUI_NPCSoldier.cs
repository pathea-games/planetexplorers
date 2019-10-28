using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CSUI_NPCSoldier : MonoBehaviour 
{
	#region UI_WIDGET
	
	[SerializeField] CSUI_SoldierPatrol  m_PatrolInfoUI;
    //[SerializeField] CSUI_SoldierGuard   m_GuardInfoUI; //lz-2016.07.26 取消守卫UI

	[SerializeField] UIPopupList  	m_ModeUI;
	[SerializeField] UISprite		m_PatrolModeUI;
	

	#endregion

	[SerializeField] CSUI_EntityState  m_EntityStatePrefab;

	//private List<CSUI_EntityState>	m_EntitesState = new List<CSUI_EntityState>();

	private CSPersonnel  m_RefNpc;
	public CSPersonnel RefNpc  
	{
		get { return m_RefNpc; }
		set 
		{ 
			m_RefNpc = value;
			UpdateModeUI();
			m_PatrolInfoUI.RefNpc = value;
            //lz-2016.07.25 取消守卫
			//m_GuardInfoUI.RefNpc = value;
		}
	}

	#region ACTIVE_PART
	
	private bool m_Active = true;
	public void Activate(bool active)
	{
		if (m_Active != active)
		{
			m_Active = active;
			_activate();
		}
		else
			m_Active = active;
	}
	
	private void _activate()
	{
		if (!m_Active)
		{
			m_ModeUI.items.Clear();
			if (m_RefNpc != null)
				m_ModeUI.items.Add(CSUtils.GetWorkModeName(m_RefNpc.m_WorkMode));
			else
				m_ModeUI.items.Add("None");
		}
		else
			UpdateModeUI();
	}
	
	#endregion


	public void Init()
	{
		CSPersonnel.RegisterOccupaChangedListener(OnOccupationChange);
	}

	void OnEnable()
	{
		if (m_RefNpc == null)
			return;
		if (m_RefNpc.m_WorkMode == CSConst.pwtGuard||m_RefNpc.m_WorkMode == CSConst.pwtPatrol)
		{
			m_PatrolInfoUI.gameObject.SetActive(true);
		}
	}

	void OnDisable()
	{

	}

	void Awake ()
	{
	}

	void OnDestroy()
	{
		CSPersonnel.UnregisterOccupaChangedListener(OnOccupationChange);
	}

	// Use this for initialization
	void Start () 
	{
		_activate();
	}
	
	// Update is called once per frame
	void Update () 
	{
	
	}

	void UpdateModeUI()
	{
		if (!m_Active)
			return;

		m_ModeUI.items.Clear();

		if (m_RefNpc != null)
		{
			m_ModeUI.items.Add(CSUtils.GetWorkModeName(CSConst.pwtPatrol));
			m_ModeUI.items.Add(CSUtils.GetWorkModeName(CSConst.pwtGuard));

			ShowStatusTips = false;
			m_ModeUI.selection = CSUtils.GetWorkModeName(m_RefNpc.m_WorkMode);
			ShowStatusTips = true;
		}
		else
		{
			m_ModeUI.items.Add("None");
		}

	}
	

	bool ShowStatusTips = true;
	void OnSelectionChange(string item)	
	{
		if (item == CSUtils.GetWorkModeName(CSConst.pwtPatrol))
		{
            if (m_RefNpc != null)
            {
                m_RefNpc.m_WorkMode = CSConst.pwtPatrol;
                if (ShowStatusTips)
                    CSUI_MainWndCtrl.ShowStatusBar(UIMsgBoxInfo.mSoldierForPatrol.GetString(), 6f);
            }
		}
		else if (item == CSUtils.GetWorkModeName(CSConst.pwtGuard))
		{
			if (m_RefNpc != null)
			{
				m_RefNpc.m_WorkMode = CSConst.pwtGuard;
				if (ShowStatusTips)
					CSUI_MainWndCtrl.ShowStatusBar(UIMsgBoxInfo.mSoldierForGuard.GetString(), 6f);
			}

		}
        m_PatrolInfoUI.gameObject.SetActive(true);
	}

	void OnOccupationChange(CSPersonnel person, int prvState)
	{
		if (person != m_RefNpc)
			return;
		
		UpdateModeUI();
	}

	void OnPopupListClick()
	{
		if (!m_Active)
			CSUI_StatusBar.ShowText(UIMsgBoxInfo.mCantHandlePersonnel.GetString(), Color.red, 5.5f);
	}

}
