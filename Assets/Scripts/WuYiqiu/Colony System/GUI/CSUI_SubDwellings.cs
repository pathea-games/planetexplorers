using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CSUI_SubDwellings : CSUI_Base 
{
	public CSDwellings		m_Dwellings;
	
	public CSUI_Dwellings	m_DwelingsUI;
	
	[System.Serializable]
	public class NPCPart
	{
		public UIGrid 			m_Root;
		public CSUI_NPCGrid		m_Prefab;
	}
	
	[SerializeField]
	private NPCPart		m_NpcPart;
	
	private List<CSUI_NPCGrid>	m_NpcGirds = new List<CSUI_NPCGrid>();
	
	[SerializeField]
	private UILabel		m_NameUI;
	
	public string Description
	{
		get {
			return m_NameUI.text;
		}
		set {
			m_NameUI.text = value;
		}
	}
	
	public Transform  m_IconRadioRoot;

	
	#region UNITY_INNER
	
	void Awake ()
	{

	}
	
	// Use this for initialization
	new void Start () 
	{
		base.Start();
		
		// Create Npc Icon grid
		for (int i = 0; i < 4; i++)
		{
		 	CSUI_NPCGrid npcGrid 			= Instantiate(m_NpcPart.m_Prefab) as CSUI_NPCGrid;
			npcGrid.transform.parent		= m_NpcPart.m_Root.transform;
			npcGrid.transform.localPosition	= Vector3.zero;
			npcGrid.transform.localRotation = Quaternion.identity;
			npcGrid.transform.localScale	= Vector3.one;
			npcGrid.NpcIconRadio			= m_IconRadioRoot;
			npcGrid.m_UseDeletebutton = false;
			UIEventListener.Get(npcGrid.gameObject).onActivate = m_DwelingsUI.OnNPCGridClick;
			m_NpcGirds.Add(npcGrid);
			
		}
		
		m_NpcPart.m_Root.repositionNow = true;
		
		// Set Npc
		for (int i = 0; i < m_NpcGirds.Count; i++)
		{
			if (i < m_Dwellings.m_NPCS.Length)
				m_NpcGirds[i].m_Npc = m_Dwellings.m_NPCS[i];
			else
				m_NpcGirds[i].m_Npc = null;
		}
	}
	
	// Update is called once per frame
	new void Update () 
	{
		base.Update();
		
		for (int i = 0; i < m_NpcGirds.Count; i++)
		{
			if (i < m_Dwellings.m_NPCS.Length)
				m_NpcGirds[i].m_Npc = m_Dwellings.m_NPCS[i];
			else
				m_NpcGirds[i].m_Npc = null;
		}
	}
	
	#endregion
}
