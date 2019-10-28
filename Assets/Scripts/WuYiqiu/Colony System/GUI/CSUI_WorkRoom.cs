using UnityEngine;
using System.Collections;
using ItemAsset;

public class CSUI_WorkRoom : MonoBehaviour 
{
	public CSCommon		m_RefCommon;
	
	public CSUI_Personnel	m_PersonnelUI;

	public CSPersonnel m_RefNpc;
	
	// Icon
	[SerializeField]
	private UISlicedSprite	m_IconSprite;
	
	public string	IconSpriteName			{ get { return m_IconSprite.spriteName; } set {  m_IconSprite.spriteName = value; }}
	
	// Worker Count
	[SerializeField]
	private UILabel		m_NPCCntLb;
	
	[SerializeField]
	private UILabel		m_BuildingNameLb;
	
	[SerializeField]
	private UIButton	m_LivingBtn;
	
	[SerializeField]
	private Color 		m_NormalColor;
	[SerializeField]
	private Color		m_LivingColor;
	

	private bool m_Active = false;
	public void Activate(bool active)
	{
		m_Active = active;
	}

	#region NGUI_CALLBACK
	
	void OnLivingRoomClick()
	{

        if (m_RefNpc == null || !(m_RefNpc.m_Occupation == CSConst.potWorker || m_RefNpc.m_Occupation==CSConst.potDoctor))
			return;
        if (m_RefCommon.WorkerCount >= m_RefCommon.WorkerMaxCount)
            return;
		if (m_RefNpc.WorkRoom != m_RefCommon)
		{
			m_RefNpc.TrySetWorkRoom(m_RefCommon);
            //--to do: wait
            //if(GameConfig.IsMultiMode)
            //{
            //    if(m_RefNpc.m_Npc.Netlayer is AiAdNpcNetwork)
            //        ((AiAdNpcNetwork)m_RefNpc.m_Npc.Netlayer).SetClnWorkRoomID(m_RefNpc.WorkRoom.ID);
            //}
			CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(UIMsgBoxInfo.mSetWorkRoom.GetString(), m_RefNpc.FullName, CSUtils.GetEntityName(m_RefCommon.m_Type)));
		}
	}
	
	#endregion
	
	// Use this for initialization
	void Start () 
	{
		if (m_RefNpc != null)
			Activate(m_RefNpc.Running);
	}
	
	// Update is called once per frame
	void Update () 
	{

		if (m_RefNpc == null)
			return;

		if (m_RefNpc != null)
		{
			if (m_RefNpc.WorkRoom == m_RefCommon)
			{
				m_NPCCntLb.color = m_LivingColor;
				m_BuildingNameLb.color	= m_LivingColor;
			}
			else
			{
				m_NPCCntLb.color = m_NormalColor;
				m_BuildingNameLb.color	= m_NormalColor;
			}
		}
		
		if (m_RefCommon == null)
			return;
		
		int npcCount =  m_RefCommon.WorkerCount;
		int npcMaxCount = m_RefCommon.WorkerMaxCount;
		m_NPCCntLb.text = "[" + m_RefCommon.WorkerCount + "/" + m_RefCommon.WorkerMaxCount + "]";
		ItemProto itemData = ItemProto.GetItemData(m_RefCommon.ItemID);
		if (itemData != null)
		{
			string[] iconStr = ItemProto.GetItemData(m_RefCommon.ItemID).icon;
			if (iconStr.Length != 0)
				m_IconSprite.spriteName = iconStr[0];
			else
				m_IconSprite.spriteName = "";
		}
		m_BuildingNameLb.text  = m_RefCommon.Name;
		
		if (npcCount >= npcMaxCount || !m_Active)
			m_LivingBtn.isEnabled = false;
		else
			m_LivingBtn.isEnabled = true;
	}
}
