using UnityEngine;
using System.Collections;

public class CSUI_WorkItem : MonoBehaviour 
{
	[SerializeField] UILabel mLbName;
	[SerializeField] CSUI_NPCGrid mNpcGrid;
	// Use this for initialization

	public void SetWorker(CSPersonnel personnel)
	{
		mLbName.text = personnel.m_Name;
		mNpcGrid.m_Npc = personnel;
	}
}
