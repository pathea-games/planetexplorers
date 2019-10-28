using UnityEngine;
using System.Collections;

public class CSUI_ProcessorItem : MonoBehaviour {

	// Use this for initialization
	[SerializeField] UILabel mTimeLb;
	[SerializeField] GameObject mSelcet;
	[SerializeField] UILabel mProcessorNumLb;

	void Awake()
	{
		//GetComponent<UICheckbox>().radioButtonRoot = transform.parent;
		m_ProcessorNum = System.Convert.ToInt32(mProcessorNumLb.text);
        UIEventListener.Get(this.gameObject).onDoubleClick = (go) => this.OnDoubleClickEvent(go);
	}

	void Start () 
	{
	
	}
	
	// Update is called once per frame
	void Update () 
	{
	
	}

	ProcessorInfo m_info;
	public ProcessorInfo Info
	{
		set
		{
			m_info = value;

			if(m_info != null)
			  InitWnd();
		}
	}

	int m_ProcessorNum;
	public int ProcessorNum
	{
		get
		{
			return m_ProcessorNum;
		}
		set
		{
			m_ProcessorNum = value;
		}
	}


	private UICheckbox m_CheckBox;
	public bool isChecked { 
		get 
		{
			if (m_CheckBox == null)
				m_CheckBox = GetComponent<UICheckbox>();
			return  m_CheckBox.isChecked;
		}
		set 
		{
			if (m_CheckBox == null)
				m_CheckBox = GetComponent<UICheckbox>();
			m_CheckBox.isChecked = value;
		}
	}

	void InitWnd()
	{

		SetTime(m_info.Times);
		JionProcrssor(m_info.Jioned);
	}

	public void SetTime(float mTimes)
	{
		int Times = (int)mTimes;
		
		int hours = ((Times/60)/60)%24;
		int minute = (Times/60)%60;
		int second = Times%60;
		mTimeLb.gameObject.SetActive(true);
		mTimeLb.text = hours.ToString() +":" + minute.ToString() +":" +second.ToString();
	}

	public void JionProcrssor(bool jion)
	{
		mSelcet.gameObject.SetActive(jion);
	}

	public void SetProcessorNum(int index)
	{
		mProcessorNumLb.gameObject.SetActive(true);
		mProcessorNumLb.text = index.ToString();
        m_ProcessorNum = index;
	}


	public delegate void JionEvent(object sender,int index);
	public event JionEvent e_JionEvent = null;
	void OnJionActivate(bool active)
	{
		if(active)
		{
			if(e_JionEvent != null)
			{
				e_JionEvent(this,m_ProcessorNum);
			}
		}

	}

    public delegate void DoubleClickEvent(GameObject go,int index);
    public event DoubleClickEvent e_DoubleClickEvent=null;
    void OnDoubleClickEvent(GameObject go)
    {
        if(null!=e_DoubleClickEvent)
        {
            e_DoubleClickEvent(go,m_ProcessorNum);
        }
    }
}