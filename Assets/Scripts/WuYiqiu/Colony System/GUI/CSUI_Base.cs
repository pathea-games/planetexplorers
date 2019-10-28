using UnityEngine;
using System.Collections;

public class CSUI_Base : MonoBehaviour 
{
	public CSEntity		m_Entity;
	
	// Duarbility Part
	[System.Serializable]
	public class DurabilityPart
	{
		public UISlider	m_Slider;
		public UILabel  m_Val;
	}
	
	[SerializeField]
	private DurabilityPart m_Durability;
	
	// Slier Tiem Button Part
	[System.Serializable]
	public class STBPart
	{
		public UISlider  m_Slider;
		public UILabel	 m_TimeLb;
		public N_ImageButton	 m_Button;
	}
	
	[SerializeField]
	private STBPart	m_Repair;
	[SerializeField]
	private STBPart	m_Delete;
	
	[SerializeField]
	private CSUI_QueryMat m_QueryMatUIPrefab;
	
	private CSUI_QueryMat m_QueryMatWnd;
	
	//[SerializeField]
	public Transform m_QueryMatRoot;
	
	#region UPDATE_FUNCTION
	
	void UpdateRepair ()
	{
		if (m_Entity.isRepairing)
		{
			float restTime = m_Entity.BaseData.m_RepairTime - m_Entity.BaseData.m_CurRepairTime;
			float percent  = m_Entity.BaseData.m_CurRepairTime / m_Entity.BaseData.m_RepairTime;
			
			m_Repair.m_Slider.sliderValue = percent;
		
			m_Repair.m_TimeLb.text = CSUtils.GetRealTimeMS( (int) restTime );
		}
		else
		{
			m_Repair.m_Slider.sliderValue = 0F;
			m_Repair.m_TimeLb.text = CSUtils.GetRealTimeMS( 0 );
		}
	}
	
	void UpdateDelete ()
	{
		if (m_Entity.isDeleting)
		{
			float restTime = m_Entity.BaseData.m_DeleteTime - m_Entity.BaseData.m_CurDeleteTime;
			float percent  = m_Entity.BaseData.m_CurDeleteTime / m_Entity.BaseData.m_DeleteTime;
			
			m_Delete.m_Slider.sliderValue = percent;
		
			m_Delete.m_TimeLb.text = CSUtils.GetRealTimeMS( (int) restTime );
		}
		else
		{
			m_Delete.m_Slider.sliderValue = 0F;
			m_Delete.m_TimeLb.text = CSUtils.GetRealTimeMS( 0 );
		}
	}
	
	void UpdateDurability ()
	{
		float percent  =  m_Entity.BaseData.m_Durability / m_Entity.m_Info.m_Durability;
		
		m_Durability.m_Slider.sliderValue = percent;
		
		string str = "";
		str += Mathf.RoundToInt(m_Entity.BaseData.m_Durability).ToString();
		str += " / ";
		str += Mathf.RoundToInt(m_Entity.m_Info.m_Durability).ToString();
		
		m_Durability.m_Val.text = str;
	}
	#endregion
	
	void OnRepairBtn ()
	{
		//m_Entity.StartRepairCounter();
		
		if (m_QueryMatWnd == null)
		{
			m_QueryMatWnd = Instantiate(m_QueryMatUIPrefab) as CSUI_QueryMat;
			m_QueryMatWnd.transform.parent			= m_QueryMatRoot;
			m_QueryMatWnd.transform.localPosition	= new Vector3(0, 0, -30);
			m_QueryMatWnd.transform.localRotation	= Quaternion.identity;
			m_QueryMatWnd.transform.localScale		= Vector3.one;
		
			m_QueryMatWnd.funcType = CSUI_QueryMat.FuncType.Repair;
			m_QueryMatWnd.m_Entity = m_Entity;

            if (CSUI_MainWndCtrl.Instance != null)
                CSUI_MainWndCtrl.Instance.m_ChildWindowOfBed = m_QueryMatWnd.gameObject;
		}

	}
	
	void OnDeleteBtn ()
	{
		//m_Entity.StartDeleteCounter();
		
		if (m_QueryMatWnd == null)
		{
			m_QueryMatWnd = Instantiate(m_QueryMatUIPrefab) as CSUI_QueryMat;
			m_QueryMatWnd.transform.parent			= m_QueryMatRoot;
			m_QueryMatWnd.transform.localPosition	= new Vector3(0, 0, -15);
			m_QueryMatWnd.transform.localRotation	= Quaternion.identity;
			m_QueryMatWnd.transform.localScale		= Vector3.one;
			
			m_QueryMatWnd.funcType = CSUI_QueryMat.FuncType.Delete;
			m_QueryMatWnd.m_Entity = m_Entity;

            if (CSUI_MainWndCtrl.Instance != null)
                CSUI_MainWndCtrl.Instance.m_ChildWindowOfBed = m_QueryMatWnd.gameObject;
		}
	}
	
	// Use this for initialization
	protected void Start () 
	{
	
	}
	
	// Update is called once per frame
	protected void Update () 
	{
		if (m_Entity == null)
			return;
		
		UpdateRepair();
		UpdateDelete();
		UpdateDurability();
		
		// Button Enable Or Disable?
		m_Delete.m_Button.isEnabled = !m_Entity.isDeleting;
		
		if (m_Delete.m_Button.isEnabled)
		{
			if (m_Entity.m_Info.m_Durability <= Mathf.CeilToInt (m_Entity.BaseData.m_Durability))
				m_Repair.m_Button.isEnabled = false;
			else
				m_Repair.m_Button.isEnabled = !m_Entity.isRepairing;
		}
		else
			m_Repair.m_Button.isEnabled = false;
	}
}
