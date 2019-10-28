using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ItemAsset;
using CSRecord;
using CustomData;
using Pathea;
public class CSUI_Storage : MonoBehaviour 
{
	private CSStorage		m_ActiveStorage;
	public CSStorage		ActiveStorage	
	{ 
		get { return m_ActiveStorage; }

		set
		{
			if (m_ActiveStorage != value )
			{
				if (m_ActiveStorage != null)
				{
					m_ActiveStorage.RemoveEventListener(OnStorageEventHandler);

					HistoryStruct[] historys = value.GetHistory();
					for (int i = 0; i < m_SubHistorys.Count; i++)
					{
						GameObject.DestroyImmediate(m_SubHistorys[i].gameObject);
					}
					m_SubHistorys.Clear();

					for (int i = 0; i < historys.Length; i++)
					{ 
						CSUI_SubStorageHistory subHistory = _addHistoryItem(historys[i]);
						subHistory.AddHistory(historys[i].m_Value);
					}

				}

				if (value != null)
				{

					value.AddEventListener(OnStorageEventHandler);
				}
			}


			m_ActiveStorage = value;

		}
	}
	
	private List<CSStorage>	m_Storages	= new List<CSStorage>();
	
	
	// Storage Menu
	[System.Serializable]
	public class StorageMenu
	{
		public UIGrid 		m_Root;
		public GameObject	m_BtnPrefab;
	}
	[SerializeField]
	private StorageMenu	 m_StorageMenu;
	
	private List<GameObject>	m_StorageMenuObjs;
	
	
	// Storage Grids
	[System.Serializable]
	public class StorageMain
	{
		public CSUI_StorageMain m_Main;
		
		public UICheckbox	m_ItemCB;
		public UICheckbox	m_EquipCB;
		public UICheckbox   m_ResourceCB;
        public UICheckbox   m_ArmorCB;

	}
	[SerializeField]
	private StorageMain	m_StorageMain;

	// History
	[SerializeField] UITable	m_HistoryRootUI;
	[SerializeField] UILabel	m_HistoryPrefab;
	[SerializeField] CSUI_SubStorageHistory		m_SubHistoryPrefab;
	//private List<UILabel>		m_HistoryLbs = new List<UILabel>();
	private List<CSUI_SubStorageHistory>  m_SubHistorys = new List<CSUI_SubStorageHistory>();
	
	private int m_CurrentPickTab = 0;
		
	public CSUI_StorageMain StorageMainUI 	{ get{ return m_StorageMain.m_Main; } }

	public CSStorage m_SetActiveStorage;

	public bool IsEmpty ()
	{
		return m_Storages.Count == 0;
	}
	
	// Add Storage to m_Storages
	public void AddStorage (CSStorage storage)
	{
		if ( !m_Storages.Exists( item0 => item0 == storage) )
		{		
			m_Storages.Add(storage);
			UpdateStorageMenu ();
		}
		else
			Debug.LogWarning("The storage that you want to add into UI is areadly exsts!");
		
	}
	
	// Remove Storage to m_Storages
	public void RemoveStorage (CSStorage storage)
	{
		if ( m_Storages.Remove(storage))
		{
			UpdateStorageMenu (); 
			GameObject go = m_StorageMenuObjs.Find(item0 => item0.activeSelf == true ); 
			if (go != null)
			{
				go.GetComponent<UICheckbox>().isChecked = true;
				OnStorageMenuSelect(go); 
			}
		} 
	}

	public void RemoveAll()
	{
		m_Storages.Clear();
		UpdateStorageMenu ();
	}


	public void Replace(List<CSEntity> entityList)
	{
		m_Storages.Clear();
		int len = entityList.Count > 8 ? 8 : entityList.Count;
		for (int i = 0; i < len; i++ )
		{
			m_Storages.Add(entityList[i] as CSStorage);
		}
		
		UpdateStorageMenu ();
	}

	public void Replace(List<CSStorage> storages)
	{
		m_Storages.Clear();
		int len = storages.Count > 8 ? 8 : storages.Count;
		for (int i = 0; i < len; i++ )
		{
			m_Storages.Add(storages[i]);
		}

		UpdateStorageMenu ();
	}
	
	// Storage Menu
	void UpdateStorageMenu ()
	{
		if (m_StorageMenuObjs == null)
			return;
		
		for (int i = 0; i < 8; i++)
		{
			CSUI_CommonIcon commonIcon = m_StorageMenuObjs[i].GetComponent<CSUI_CommonIcon>();
			if (i < m_Storages.Count)
			{
				m_StorageMenuObjs[i].SetActive(true);
				UIEventListener listener = UIEventListener.Get(m_StorageMenuObjs[i]);
				listener.onClick = OnStorageMenuSelect;
				bool ischecked = m_StorageMenuObjs[i].GetComponent<UICheckbox>().isChecked;
				if (ischecked)
					OnStorageMenuSelect(m_StorageMenuObjs[i]);
				commonIcon.Common = m_Storages[i] as CSCommon;
			}
			else
			{
				m_StorageMenuObjs[i].SetActive(false);
				commonIcon.Common = null;
			}
		}
		
		m_StorageMenu.m_Root.repositionNow = true;
	}
	
	public void SetStorageType (int type, int pageIndex)
	{
		switch (type)
		{
		case 0:
			m_StorageMain.m_ItemCB.isChecked = true;
			break;
		case 1:
			m_StorageMain.m_EquipCB.isChecked = true;
			break;
		case 2:
			m_StorageMain.m_ResourceCB.isChecked = true;
			break;
        case 3:
            m_StorageMain.m_ArmorCB.isChecked = true;
            break;
		}
		
		m_CurrentPickTab = type;
		
		m_StorageMain.m_Main.SetType(m_CurrentPickTab, pageIndex);
	}


	private void SetActiveStorage()
	{
		if (m_SetActiveStorage != null)
		{
			int index = m_Storages.FindIndex(item0 => item0 == m_SetActiveStorage);
			if (index != -1)
			{
				m_StorageMenuObjs[index].GetComponent<UICheckbox>().isChecked = true;
				OnStorageMenuSelect(m_StorageMenuObjs[index]);
			}

			m_SetActiveStorage = null;
		}

	}
	

	private CSUI_SubStorageHistory _addHistoryItem(HistoryStruct history)
	{
		if (m_SubHistorys.Count != 0)
		{
			CSUI_SubStorageHistory curSub = m_SubHistorys[m_SubHistorys.Count - 1];
			if ( curSub.Day == history.m_Day)
				return curSub;
		}
		CSUI_SubStorageHistory curSubHistory = Instantiate(m_SubHistoryPrefab) as CSUI_SubStorageHistory;
		curSubHistory.transform.parent = m_HistoryRootUI.transform;
		curSubHistory.transform.localPosition = Vector3.zero;
		curSubHistory.transform.localRotation = Quaternion.identity;
		curSubHistory.transform.localScale = Vector3.one;
		curSubHistory.Day = history.m_Day;
		curSubHistory.onReposition = OnSubHistoryReposition;

		m_SubHistorys.Add(curSubHistory);
		return curSubHistory;
	}

	private void OnSubHistoryReposition()
	{
		m_HistoryRootUI.repositionNow = true;
	}
	
		
	#region NGUI_CALLBACK
	
	void OnStorageMenuSelect (GameObject go)
	{
		if (Input.GetMouseButtonUp(1)) return;
		
		int index = m_StorageMenuObjs.FindIndex(item0 => item0 == go);
		
		ActiveStorage = m_Storages[index];
		CSUI_MainWndCtrl.Instance.mSelectedEnntity = ActiveStorage;
		m_StorageMain.m_ItemCB.isChecked = true;
		OnStorageTypeSelect(m_StorageMain.m_ItemCB.gameObject);
	}
	
	void OnStorageTypeSelect (GameObject go)
	{
		if (Input.GetMouseButtonUp(1)) return;

        if (go == m_StorageMain.m_ItemCB.gameObject)
            m_CurrentPickTab = 0;
        else if (go == m_StorageMain.m_EquipCB.gameObject)
            m_CurrentPickTab = 1;
        else if (go == m_StorageMain.m_ResourceCB.gameObject)
            m_CurrentPickTab = 2;
        else if (go == m_StorageMain.m_ArmorCB.gameObject)
            m_CurrentPickTab = 3;
        if (PeGameMgr.IsMulti) 
            m_StorageMain.m_Main.SetPackage(ActiveStorage.m_Package, m_CurrentPickTab,ActiveStorage);
		else
		    m_StorageMain.m_Main.SetPackage(ActiveStorage.m_Package, m_CurrentPickTab);
		
		GameUI.Instance.mItemPackageCtrl.ResetItem(m_CurrentPickTab, m_StorageMain.m_Main.PageIndex);
		GameUI.Instance.mWarehouse.ResetItem(m_CurrentPickTab, m_StorageMain.m_Main.PageIndex);
	}
	
	#endregion

	#region  STORAGESTATUS

	void OnStorageMainOpStateEvent (CSUI_StorageMain.EEventType type, object obj1, object obj2)
	{
		if (type == CSUI_StorageMain.EEventType.CantWork)
			CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString( UIMsgBoxInfo.mCantWorkWithoutElectricity.GetString(), (string)obj1), Color.red); 
		else if (type == CSUI_StorageMain.EEventType.PutItemInto)
			CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(UIMsgBoxInfo.mPutIntoMachine.GetString(), (string)obj1, (string)obj2));
		else if (type == CSUI_StorageMain.EEventType.DeleteItem)
			CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(UIMsgBoxInfo.mDeleteItem.GetString(), (string)obj1, (string)obj2));
		else if (type == CSUI_StorageMain.EEventType.TakeAwayItem)
			CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(UIMsgBoxInfo.mTakeAwayFromMachine.GetString(), (string)obj1, (string)obj2));
		else if (type == CSUI_StorageMain.EEventType.ResortItem)
			CSUI_MainWndCtrl.ShowStatusBar(UIMsgBoxInfo.mResortTheItems.GetString());
		else if (type == CSUI_StorageMain.EEventType.SplitItem)
			CSUI_MainWndCtrl.ShowStatusBar(CSUtils.GetNoFormatString(UIMsgBoxInfo.mSplitItems.GetString(), (string)obj1, (string)obj2));
	}
	

	void OnStorageEventHandler (int event_id, CSEntity entity, object arg)
	{
		switch (event_id)
		{
		case CSConst.eetStorage_HistoryEnqueue:
		{
			HistoryStruct hs = arg as HistoryStruct;
			CSUI_SubStorageHistory ssh = _addHistoryItem(hs);
			if (ssh != null)
				ssh.AddHistory(hs.m_Value);
		}break;
		case CSConst.eetStorage_HistoryDequeue:
		{
//			HistoryStruct hs = arg as HistoryStruct;
			if (m_SubHistorys.Count != 0)
			{
				m_SubHistorys[0].PopImmediate();

				if (m_SubHistorys[0].IsEmpty)
				{
					Destroy(m_SubHistorys[0].gameObject);
					m_SubHistorys.RemoveAt(0);
				}
			}
		}break;
		case CSConst.eetStorage_PackageRemoveItem:
		{
			StorageMainUI.RestItems();
		}break;
		}
	}
	#endregion

	public void Init ()
	{
		m_StorageMenuObjs = new List<GameObject>();
		
		// Create storage Menu 
		for (int i = 0; i < 8; i++)
		{
			GameObject go 				= Instantiate(m_StorageMenu.m_BtnPrefab) as GameObject;
			go.transform.parent			= m_StorageMenu.m_Root.transform;
			go.transform.localPosition	= Vector3.zero;
			go.transform.localRotation  = Quaternion.identity;
			go.transform.localScale		= Vector3.one;
			go.name	= i.ToString() + " Storage Menu";
			
			UICheckbox cb = go.GetComponent<UICheckbox>();
			cb.radioButtonRoot = m_StorageMenu.m_Root.transform;
			if (i == 0)	
			{
				cb.startsChecked = true;
				cb.isChecked = true;
			}
			else
			{
				cb.startsChecked = false;
				cb.isChecked = false;
			}
			
			m_StorageMenuObjs.Add(go);
		}
		
		UIEventListener listener = UIEventListener.Get( m_StorageMain.m_ItemCB.gameObject );
		listener.onClick 	= OnStorageTypeSelect;
		listener = UIEventListener.Get( m_StorageMain.m_EquipCB.gameObject );
		listener.onClick	= OnStorageTypeSelect;
		listener = UIEventListener.Get( m_StorageMain.m_ResourceCB.gameObject );
		listener.onClick	= OnStorageTypeSelect;
        listener = UIEventListener.Get(m_StorageMain.m_ArmorCB.gameObject);
        listener.onClick    = OnStorageTypeSelect;

		StorageMainUI.OpStatusEvent += OnStorageMainOpStateEvent;
	}
	
	
	#region UNITY_INNER_FUNC

	void OnDestroy()
	{
		StorageMainUI.OpStatusEvent -= OnStorageMainOpStateEvent;
	}
	
	void OnEnable()
	{
		//UpdateStorageMenu();

	}
	
	// Use this for initialization
	void Start () 
	{
		UpdateStorageMenu ();

	}
	
	// Update is called once per frame
	void Update () 
	{
		if (ActiveStorage == null && m_Storages.Count > 0)
			ActiveStorage = m_Storages[0];

		if (ActiveStorage != null)
			m_StorageMain.m_Main.SetWork(ActiveStorage.IsRunning);
		else
			m_StorageMain.m_Main.SetWork(false);

		Transform trans = m_HistoryRootUI.transform;
		trans.localPosition = new Vector3(trans.localPosition.x, -m_HistoryRootUI.mVariableHeight, trans.localPosition.z);

		SetActiveStorage();
	}
	#endregion
}