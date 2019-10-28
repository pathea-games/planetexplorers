using UnityEngine;
using System.Collections;
using System.Collections.Generic;



public class ProcessorInfo
{
	public float Times;
	public bool Jioned;
	public int ProcessorNum;
}
public class CSUI_Processor : MonoBehaviour {

	// Use this for initialization

	[SerializeField] CSUI_ProcessorItem[] m_ProItems;

	[SerializeField] CSUI_ProcessorItem m_ProItemsNull;

	//List<CSUI_ProcessorItem> Processors = new List<CSUI_ProcessorItem>();

	private CSPersonnel  m_RefNpc;
	public CSPersonnel RefNpc  
	{
		get
		{
			return m_RefNpc;
		}
		set
		{
			m_RefNpc =value;

			if(m_RefNpc !=null)
				InitProcessChose(m_RefNpc.ProcessingIndex);
		}
	}
	
	void Start () 
	{
	
	}
	
	// Update is called once per frame
	void Update () 
	{
		UpdateTime();
		UpdateProcessingIndex();
	}


	public void Init()
	{
		InitEvent();
	}

	void InitProcessChose(int index)
	{
		if(index == -1)
		{
			m_ProItemsNull.isChecked = true;
			return ;
		}
		else
		{
			m_ProItems[index].isChecked = true;
			return ;
		}
	}
	void UpdateProcessingIndex()
	{
		if(m_RefNpc !=null)
			InitProcessChose(m_RefNpc.ProcessingIndex);
	}

	void OnDestroy()
	{
		CSPersonnel.UnregisterOccupaChangedListener(OnOccupationChange);
	}


	 void UpdateTime()
	{
		if(CSUI_MainWndCtrl.Instance == null)
			return ;

		if(CSUI_MainWndCtrl.Instance.CollectUI == null)
			return ;

		if(CSUI_MainWndCtrl.Instance.CollectUI.m_Processes == null)
			return ;

		for(int index = 0;index<m_ProItems.Length;index++)
		{
			m_ProItems[index].SetTime(CSUI_MainWndCtrl.Instance.CollectUI.m_Processes[index].Times);
		}

	}

	void InitEvent()
	{
		CSPersonnel.RegisterOccupaChangedListener(OnOccupationChange);

		for(int i=0;i<m_ProItems.Length;i++)
		{
			m_ProItems[i].e_JionEvent += OnJionin;
            m_ProItems[i].e_DoubleClickEvent += OnDoubleClick;
		}
	}

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
		if(m_Active)
		{

		}
	}

	void OnJionin(object sender,int index)
	{
        if (m_RefNpc != null)
        {
            m_RefNpc.TrySetProcessingIndex(index);
            //lz-2016.06.27 单击如果：npc工作正常并且这个进程没有材料，就提示需要加材料
            if (m_RefNpc.CanProcess && !CheckProcessHasMaterial(index))
            {
                CSUI_MainWndCtrl.ShowStatusBar(PELocalization.GetString(82209012));
            } 
        }
	}

    void OnDoubleClick(GameObject go, int index)
    {
        //lz-2016.06.27 双击如果：npc工作正常并且这个进程没有材料，就跳转到采集场的这个进程
        if (null!=m_RefNpc&&m_RefNpc.CanProcess /*&& !CheckProcessHasMaterial(index)*/)
        {
            if (CSUI_MainWndCtrl.Instance)
            {
                CSUI_MainWndCtrl.Instance.GoToCollectWnd(index);
            }
        }
    }

    //lz-2016.06.27 检测这个进程是否有材料
    bool CheckProcessHasMaterial(int index)
    {
        if(CSUI_MainWndCtrl.Instance)
        {
            if (CSUI_MainWndCtrl.Instance.CollectUI.mProlists.ContainsKey(index))
            {
                return CSUI_MainWndCtrl.Instance.CollectUI.mProlists[index].Count > 0;
            }
        }
        return false;
    }

	void OnOccupationChange (CSPersonnel person, int prvState)
	{
		if (person != m_RefNpc)
			return;
		
		if(m_RefNpc !=null)
			InitProcessChose(m_RefNpc.ProcessingIndex);
	}
	


}
