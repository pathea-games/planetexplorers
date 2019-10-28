using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Pathea;
using Pathea.PeEntityExt;
public class CSUI_Dwellings : MonoBehaviour 
{
	[SerializeField]
	private Transform m_QuaryMatWndRoot;
	
	[System.Serializable]
	public class SubDwellingsPart
	{
		public UIGrid	m_Root;
		public CSUI_SubDwellings	m_Prefab;
		public Transform  m_IconRadio;
	}
	
	[SerializeField]
	private SubDwellingsPart  m_SubDwellings;
	
	// Sub Dwellings windows
	private List<CSUI_SubDwellings>	m_SubDwellingsList = new List<CSUI_SubDwellings>();
	
	// Page
	[System.Serializable]
	public class PagePart
	{
		public UILabel 	 m_ValueLb;
		public UIButton	 m_UpBtn;
		public UIButton  m_DownBtn;
		
		public int  m_PageCount;
		[HideInInspector]
		public int  m_Index;
		[HideInInspector]
		public int  m_MaxPageCount;
	}
	
	[SerializeField]
	private PagePart	m_Page;
	
	// Npc HandleBtn
	[System.Serializable]
	public class NPCHandleBtns
	{
		public N_ImageButton		m_Rest;
		public N_ImageButton		m_Work;
		public N_ImageButton		m_FollowMe;
		public N_ImageButton		m_Idle;
	}
	
	[SerializeField]
	private NPCHandleBtns	m_NPCHandleBtns;
	
	private CSPersonnel		m_ActiveNpc;
	
	public void PageTurning(int pageIndex)
	{
		if (m_Page.m_MaxPageCount < pageIndex && pageIndex >= 0 )
			return;
		
		m_Page.m_Index = pageIndex;
		
		int startIndex = m_Page.m_PageCount * pageIndex;
		int endIndex   = startIndex + m_Page.m_PageCount;
		for (int i = 0; i < m_SubDwellingsList.Count; i++)
		{
			if (i < startIndex || i >= endIndex)
				m_SubDwellingsList[i].gameObject.SetActive(false);
			else
				m_SubDwellingsList[i].gameObject.SetActive(true);
		}
		
		m_SubDwellings.m_Root.repositionNow = true;
	}
	
	public bool IsEmpty ()
	{
		return m_SubDwellingsList.Count == 0;
	}


	public void SetEntityList(List<CSEntity> entityList)
	{
		for (int i=0 ;i<entityList.Count;i++)
		{
			if (!m_SubDwellingsList.Exists(item0 => item0.m_Entity ==  entityList[i] ) )
			    AddDwellings (entityList[i] as CSDwellings);
		}
		for (int i=0; i<m_SubDwellingsList.Count;i++)
		{
			if ( !entityList.Exists(item0 => item0 ==  m_SubDwellingsList[i].m_Entity  ) )
				RomveDwellings(m_SubDwellingsList[i].m_Dwellings );
		}

	}

	// Add new Sub Dwellings windows
	public void AddDwellings (CSDwellings dwellings)
	{
		if ( !m_SubDwellingsList.Exists(item0 => item0.m_Dwellings == dwellings) )
		{
			// Create a Sub Dwellings Window
			CSUI_SubDwellings	sd = Instantiate(m_SubDwellings.m_Prefab) as CSUI_SubDwellings;
			sd.transform.parent  		= m_SubDwellings.m_Root.transform;
			sd.transform.localPosition	= Vector3.zero;
			sd.transform.localRotation  = Quaternion.identity;
			sd.transform.localScale		= Vector3.one;
			sd.m_IconRadioRoot			= m_SubDwellings.m_IconRadio;
			sd.m_QueryMatRoot			= m_QuaryMatWndRoot;
			sd.m_Entity					= dwellings;
			sd.m_Dwellings				= dwellings;
			sd.gameObject.name			= m_SubDwellingsList.Count.ToString();
			sd.m_DwelingsUI				= this;
			sd.gameObject.SetActive(true);

			m_SubDwellingsList.Add (sd);
			
			int totalPage = m_SubDwellingsList.Count / m_Page.m_PageCount;
			m_Page.m_MaxPageCount =  (m_SubDwellingsList.Count % m_Page.m_PageCount) == 0 ? totalPage : totalPage + 1;
			
			if (m_Page.m_Index + m_Page.m_PageCount < m_SubDwellingsList.Count)
				sd.gameObject.SetActive(false);
			else
				m_SubDwellings.m_Root.repositionNow = true;
				
		}
		else
			Debug.LogWarning("The Dwellings that you want to add into UI is areadly exsts!");
	}
	
	// Remove Dwellings and Sub Dwelling windows
	public void RomveDwellings (CSDwellings dwellings)
	{
		int  index = m_SubDwellingsList.FindIndex( item0 => item0.m_Dwellings == dwellings);
		if (index != -1)
		{
			CSUI_SubDwellings sdWnd = m_SubDwellingsList[index];
			m_SubDwellingsList.RemoveAt(index);
			GameObject.DestroyImmediate(sdWnd.gameObject);
			
			// Rename
			for (int i = index; i < m_SubDwellingsList.Count; i ++)
				m_SubDwellingsList[i].gameObject.name = i.ToString();
			
			// Get new Max page
			int totalPage = m_SubDwellingsList.Count / m_Page.m_PageCount;
			m_Page.m_MaxPageCount =  (m_SubDwellingsList.Count % m_Page.m_PageCount) == 0 ? totalPage : totalPage + 1;
			
			
			PageTurning(m_Page.m_Index);
		}
		else
			Debug.LogWarning("The Dwellings that you want to remove is not exsist!");
	}
	
	#region NGUI_CALLBACK
	
	void OnPageUpBtn ()
	{
		PageTurning(m_Page.m_Index - 1);
	}
	
	void OnPageDownBtn ()
	{
		PageTurning(m_Page.m_Index + 1);
	}
	
	
	// CSUI_NPCGrid OnClick Event 
	public void OnNPCGridClick (GameObject go, bool active)
	{
		if (!active)	return;


		CSUI_NPCGrid npcGird = go.GetComponent<CSUI_NPCGrid>();
		m_ActiveNpc = npcGird.m_Npc;

	}
	
    //void OnWorkClick ()
    //{
    //    if (m_ActiveNpc != null)
    //        m_ActiveNpc.WorkNow();
    //    else
    //        Debug.LogWarning("The Active Npc is not exist!");
    //}
	
    //void OnRestClick ()
    //{
    //    if (m_ActiveNpc != null)
    //        m_ActiveNpc.Rest();
    //    else
    //        Debug.LogWarning("The Active Npc is not exist!");
    //}
	
	void OnFollowMeClick ()
	{
		if (m_ActiveNpc != null)
			MessageBox_N.ShowYNBox(PELocalization.GetString(8000096), SetActiveNpcFollow);
		else
			Debug.LogWarning("The Active Npc is not exist!");
	}
	
	void SetActiveNpcFollow()
	{
		m_ActiveNpc.FollowMe(true);
	}
	
	void OnCallClick ()
	{
//		if (m_ActiveNpc != null)
//		{
//			Transform cameraPos = Camera.mainCamera.transform;
//        	Vector3 pos = cameraPos.position - cameraPos.forward * 2;
//        	if (AiUtil.CheckPositionOnGround(ref pos, 150, 50, AiManager.Manager.groundedLayer))
//        	{
//            	mHero.transform.position = pos + Vector3.up * 3;
//        	}
//		}
	}
	
    //void OnIdleClick ()
    //{
    //    if (m_ActiveNpc != null)
    //        m_ActiveNpc.Idle();
    //    else
    //        Debug.LogWarning("The Active Npc is not exist!");
    //}
	
	#endregion
	

    //void OnNpcStateChangedListener (CSPersonnel csp, int prvState)
    //{
    //    if (csp != m_ActiveNpc)
    //        return;

    //    string npcName = csp.Name;
    //    string str = "The " + npcName;
    //    if (csp.State == CSConst.pstIdle)
    //        str += " is wandering aroud.";
    //    else if (csp.State == CSConst.pstPrepare)
    //        str += " is going to destination";
    //    else if (csp.State == CSConst.pstRest)
    //        str += " is resting";
    //    else if (csp.State == CSConst.pstFollow)
    //        str += " is following you.";
    //    else if (csp.State == CSConst.pstDead)
    //        str += " died";
    //    else if (csp.State == CSConst.pstWork)
    //        str += " is working in the " + csp.WorkRoom.Name;
    //    else
    //        str = "";
		
    //    CSUI_MainWndCtrl.ShowStatusBar(str);
    //}


	#region UNITY_INNER_FUNC

	void OnEnable ()
	{

	}

	void OnDisable ()
	{

	}

	// Use this for initialization
	void Start ()  
	{
		
	}
	
	// Update is called once per frame
	void Update () 
	{
		// Page Update
		if (m_Page.m_Index == 0)
			m_Page.m_UpBtn.gameObject.SetActive(false);
		else
			m_Page.m_UpBtn.gameObject.SetActive(true);
		
		if (m_Page.m_Index == m_Page.m_MaxPageCount - 1)
			m_Page.m_DownBtn.gameObject.SetActive(false);
		else
			m_Page.m_DownBtn.gameObject.SetActive(true);
		
		m_Page.m_ValueLb.text = (m_Page.m_Index + 1).ToString() + " / " + m_Page.m_MaxPageCount.ToString();
		
		// NPC Handle button
		if (m_ActiveNpc != null)
		{
            //if (m_ActiveNpc.WorkRoom == null)
            //{
            //    m_NPCHandleBtns.m_Work.isEnabled = false;
            //    if (m_ActiveNpc.State == CSConst.pstIdle)
            //    {
            //        m_NPCHandleBtns.m_Rest.isEnabled = true;
            //        m_NPCHandleBtns.m_FollowMe.isEnabled = true;
            //        m_NPCHandleBtns.m_Idle.isEnabled  = false;
            //    }
            //    else if (m_ActiveNpc.State == CSConst.pstRest)
            //    {
            //        m_NPCHandleBtns.m_Rest.isEnabled = false;
            //        m_NPCHandleBtns.m_FollowMe.isEnabled = true;
            //        m_NPCHandleBtns.m_Idle.isEnabled  = true;
            //    }
            //    else if (m_ActiveNpc.State == CSConst.pstFollow)
            //    {
            //        m_NPCHandleBtns.m_FollowMe.isEnabled = false;
            //        m_NPCHandleBtns.m_Rest.isEnabled = true;
            //        m_NPCHandleBtns.m_Idle.isEnabled  = true;
            //    }
            //    else if (m_ActiveNpc.State == CSConst.pstPrepare)
            //    {
            //        m_NPCHandleBtns.m_FollowMe.isEnabled = true;
            //        m_NPCHandleBtns.m_Rest.isEnabled = true;
            //        m_NPCHandleBtns.m_Idle.isEnabled  = true;
            //    }
            //}
            //else
            //{
                //if (m_ActiveNpc.State == CSConst.pstIdle)
                //{
                //    m_NPCHandleBtns.m_Rest.isEnabled = true;
                //    m_NPCHandleBtns.m_FollowMe.isEnabled = true;
                //    m_NPCHandleBtns.m_Work.isEnabled = true;
                //    m_NPCHandleBtns.m_Idle.isEnabled  = false;
                //}
                //else if (m_ActiveNpc.State == CSConst.pstRest)
                //{
                //    m_NPCHandleBtns.m_Rest.isEnabled = false;
                //    m_NPCHandleBtns.m_FollowMe.isEnabled = true;
                //    m_NPCHandleBtns.m_Work.isEnabled = true;
                //    m_NPCHandleBtns.m_Idle.isEnabled  = true;
                //}
                //else if (m_ActiveNpc.State == CSConst.pstWork)
                //{
                //    m_NPCHandleBtns.m_Rest.isEnabled = true;
                //    m_NPCHandleBtns.m_FollowMe.isEnabled = true;
                //    m_NPCHandleBtns.m_Work.isEnabled = false;
                //    m_NPCHandleBtns.m_Idle.isEnabled  = true;
                //}
                //else if (m_ActiveNpc.State == CSConst.pstFollow)
                //{
                //    m_NPCHandleBtns.m_Rest.isEnabled = true;
                //    m_NPCHandleBtns.m_FollowMe.isEnabled = false;
                //    m_NPCHandleBtns.m_Work.isEnabled = true;
                //    m_NPCHandleBtns.m_Idle.isEnabled  = true;
                //}
                //else if (m_ActiveNpc.State == CSConst.pstPrepare)
                //{
					m_NPCHandleBtns.m_Rest.isEnabled = true;
					m_NPCHandleBtns.m_FollowMe.isEnabled = true;
					m_NPCHandleBtns.m_Work.isEnabled = true;
					m_NPCHandleBtns.m_Idle.isEnabled  = true;
                //}
            //}

		}
		else
		{
			m_NPCHandleBtns.m_FollowMe.isEnabled = false;
			m_NPCHandleBtns.m_Rest.isEnabled = false;
			m_NPCHandleBtns.m_Work.isEnabled = false;
		}
	}
	
	#endregion
}
