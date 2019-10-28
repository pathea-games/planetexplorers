using UnityEngine;
using System.Collections;

public class CSUI_NPCFollower : MonoBehaviour 
{

	#region UI_WIDGET

	[SerializeField] 	UIGrid m_HeroItemRootUI;

	#endregion

	[SerializeField] CSUI_HeroItem	HeroItemUIPrefab;

	private CSUI_HeroItem[] m_HeroItems;

	private CSPersonnel  m_RefNpc;
	public CSPersonnel RefNpc  
	{
		get { return m_RefNpc; }
		set 
		{ 
			m_RefNpc = value;

			if (m_HeroItems != null)
			{
				for (int i = 0; i < m_HeroItems.Length; i++)
				{
					m_HeroItems[i].HandlePerson = m_RefNpc;
				}
			}
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
		if (m_HeroItems == null)
			return;
		for (int i = 0; i < m_HeroItems.Length; i++)
		{
			m_HeroItems[i].Activate(m_Active);
		}
	}
	
	#endregion


	public void Init()
	{

	}

	void Awake ()
	{
		m_HeroItems = new CSUI_HeroItem[2];
		for (int i = 0; i < m_HeroItems.Length; i++)
		{
			m_HeroItems[i] = Instantiate(HeroItemUIPrefab) as CSUI_HeroItem;
			m_HeroItems[i].transform.parent = m_HeroItemRootUI.transform;
			CSUtils.ResetLoacalTransform(m_HeroItems[i].transform);
			m_HeroItems[i].m_HeroIndex = i;
			m_HeroItems[i].HandlePerson = m_RefNpc;
		}


		m_HeroItemRootUI.repositionNow = true;
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

	#region CALL_BACK

	void OnPopupListClick()
	{
		if (!m_Active)
			CSUI_StatusBar.ShowText(UIMsgBoxInfo.mCantHandlePersonnel.GetString(), Color.red, 5.5f);
	}

	#endregion
}
