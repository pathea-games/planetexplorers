using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pathea;

/*
public class CSUI_Main : GUIWindowBase 
{
    private PeEntity player;
    private PeTrans playerTrans;
	private CSCreator m_Creator;
	public CSCreator Creator
	{
		get {
			return m_Creator;
		}

		set {
			if (m_Creator != null)
			{
				ClearAllMenueItems();
				m_Creator.UnregisterListener(OnCreatorEventListener);
				m_Creator.UnregisterPeronnelListener(OnCreatorEventListenerForPersonnel);
			}

			m_Creator = value;
			if (m_Creator != null)
			{
				AddBuildingItem(value.Assembly);

				Dictionary<int, CSCommon> entities = m_Creator.GetCommonEntities();
				foreach (CSCommon entity in entities.Values)
					AddBuildingItem(entity);

				m_Creator.RegisterListener(OnCreatorEventListener);
				m_Creator.RegisterPersonnelListener(OnCreatorEventListenerForPersonnel);
			}
		}
	}
	#region CREATOR_EVENT
	
	void OnCreatorEventListener (int event_type, CSEntity entity)
	{
		if (event_type == CSConst.cetAddEntity)
		{
			AddBuildingItem(entity);

			if (entity.m_Type == CSConst.etAssembly)
			{
				CSAssembly assem = entity as CSAssembly;
				MapMaskData data = new MapMaskData();
				data.mDescription = "Colony";
				data.mId = -1;
				data.mIconId = 15;
				data.mPosition = new Vector3 (entity.Position.x, entity.Position.y + 4f, entity.Position.z);
				data.mRadius = assem.Radius;
				//GameUI.Instance.mWorldMapGui.AddMapMask(data, true);
			}
		}
		else if (event_type == CSConst.cetRemoveEntity)
		{
			if (entity.m_Type == CSConst.etAssembly)
			{
				//GameUI.Instance.mWorldMapGui.RemoveBase();
			}

			RemoveBuildingItem(entity);
		}
	}

	void OnCreatorEventListenerForPersonnel (int event_type, CSPersonnel p)
	{
		if (event_type == CSConst.cetAddPersonnel)
		{
//			m_Windows.m_PersonnelUI.AddPersonnel(p);
			m_Windows.m_PersonnelUI.OnCreatorAddPersennel(p);
		}
		else if (event_type == CSConst.cetRemovePersonnel)
		{
//			m_Windows.m_PersonnelUI.RemovePersonnel(p);
			m_Windows.m_PersonnelUI.OnCreatorRemovePersennel(p);
		}

	}
	
	#endregion

	static private CSUI_Main m_Instance ;
	static public CSUI_Main Instance
	{
		get {
			return m_Instance;
		}	
	}

	static public UIAtlas MainAtlas	{ get { return m_Instance == null ? null : m_Instance.m_MainAtlas; } }
	static public UIFont  Font12	{ get { return m_Instance == null ? null : m_Instance.m_Font12; } }

	// Atlas
	public UIAtlas m_MainAtlas;
	public UIFont	m_Font12;
	
	//  Menu : Building & Personnel
	[System.Serializable]
	public class MenuPart
	{
		public UITable		 m_Root;
		 
		public UIGrid		 m_BuildingRoot; 	 
		public UICheckbox	 m_BuildingCB;
		public CSUI_MenuItem m_MenuItemPrefab;
		
		public UIGrid		 m_PersonnelRoot;
		public UICheckbox    m_PersonnelCB;
	}
	
	[SerializeField]
	private	MenuPart	m_Menu;
	
	private List<CSUI_MenuItem> m_BuildingItems = new List<CSUI_MenuItem>();
	// Storage Menu Item
	private CSUI_MenuItem 		  m_StorageMI;
	private List<CSUI_MenuItem>   m_DumpedSotageMIs = new List<CSUI_MenuItem>();

	// Dwellings bed Menu Item
	private CSUI_MenuItem		  m_DwellingBedMI;

	// Engineering MenuItem
	private CSUI_MenuItem		  m_EngineeringMI;
	private List<CSUI_MenuItem>	  m_DumpedEngineeringMIs = new List<CSUI_MenuItem>();

	// MainLine Npc Menu Item
	private CSUI_MenuItem	m_MainLineNpcMI;
	// Others Npc Menu Items
	private CSUI_MenuItem	m_OthersNpcMI;

	public CSUI_MenuItem m_ActiveMI;
	
	// Windows 
	[System.Serializable]
	public class WindowPart
	{
		public CSUI_Assembly	m_AssemblyUI;
		public CSUI_PPCoal		m_PPCoalUI;
		public CSUI_Storage		m_StorageUI;
		public CSUI_Engineering m_EngineeringUI;
		public CSUI_Dwellings	m_DwellingsUI;
		public CSUI_Farm		m_FarmUI;
		public CSUI_Factory		m_FactoryUI;
		public CSUI_Personnel	m_PersonnelUI;
	}

	[SerializeField] WindowPart	m_Windows;
	
	public CSUI_Assembly 	AssemblyUI		{ get { return m_Windows.m_AssemblyUI;}  }
	public CSUI_PPCoal		PPCoalUI		{ get { return m_Windows.m_PPCoalUI;} }
	public CSUI_Storage		StorageUI		{ get { return m_Windows.m_StorageUI;} }
	public CSUI_Engineering EngineeringUI	{ get { return m_Windows.m_EngineeringUI;} }
	public CSUI_Dwellings	DwellingsUI		{ get { return m_Windows.m_DwellingsUI;} }
	public CSUI_Farm		FarmUI			{ get { return m_Windows.m_FarmUI;}}
	public CSUI_Factory		FactoryUI		{ get { return m_Windows.m_FactoryUI;}}
	public CSUI_Personnel	PersonnelUI		{ get { return m_Windows.m_PersonnelUI;} }

	// Popup hint
	[SerializeField] CSUI_PopupHint m_PopupHintPrefab;

	CSUI_AlphaTween m_AlphaTween;
	[SerializeField] CSUI_AlphaTween m_AlphaTweenPrefab;

	public static CSUI_PopupHint CreatePopupHint (Vector3 pos, Transform parent, Vector3 offset, string text, bool bGreen = true)
	{
		if (m_Instance == null)
			return null;

		CSUI_PopupHint ph = GameObject.Instantiate(m_Instance.m_PopupHintPrefab) as CSUI_PopupHint;
		ph.transform.parent 	= parent;
		ph.transform.position	= pos;
		ph.transform.localScale = Vector3.one;
		ph.transform.localPosition = new Vector3(ph.transform.localPosition.x + offset.x, ph.transform.localPosition.y + offset.y, offset.z);
		ph.m_Pos = ph.transform.position;

		ph.Text = text;
		ph.bGreen = bGreen;

		ph.Tween();
		return ph;
	}

	public float WorkDistance = 100; 

	public enum EWorkType
	{
		Working,
		OutOfDistance,
		NoAssembly,
		UnKnown
	}
	private EWorkType m_WorkMode;
	
	public static EWorkType WorkType			{ get { if (m_Instance == null) return EWorkType.UnKnown; return m_Instance.m_WorkMode;}}

//	public static bool IsWorking (bool bShowMsg = true)
//	{
//		if (m_Instance == null)
//			return false;
//
//		if (m_Instance.m_WorkMode == EWorkType.Working)
//			return true;
//
//		if (m_Instance.m_WorkMode == EWorkType.OutOfDistance)
//		{
//			if (bShowMsg)
////				ShowStatusBar("You need to use this item within the static field of a settlement!!", Color.red);
//				CSUI_MainWndCtrl.ShowStatusBar(UIMsgBoxInfo.mNeedStaticField.GetString(), Color.red); 
//			return false;
//		}
//		else if (m_Instance.m_WorkMode == EWorkType.NoAssembly)
//		{
//			if (bShowMsg)
////				ShowStatusBar("You need to place an assembly core to make it work!", Color.red);
//				CSUI_MainWndCtrl.ShowStatusBar(UIMsgBoxInfo.mNeedAssembly.GetString(), Color.red); 
//			return false;
//		}
//		else 
//			return false;
//	}
	
	public void ChangeWindow(CSUI_MenuItem menuItem, bool active)
	{
		if (menuItem == null)
			return;

		if (menuItem.m_Type == CSConst.etAssembly)
		{
			m_Windows.m_AssemblyUI.gameObject.SetActive(active);
			if (active)
			{
				m_Windows.m_AssemblyUI.SetEntity(menuItem.m_Entity);
			}
		}
		else if (menuItem.m_Type == CSConst.etPowerPlant)
		{
			CSPowerPlant cspp = menuItem.m_Entity as CSPowerPlant;
			if (cspp.m_PPType == CSConst.etppCoal)
			{
				m_Windows.m_PPCoalUI.gameObject.SetActive(active);
				if (active)
					m_Windows.m_PPCoalUI.SetEntity(menuItem.m_Entity);
			}
		}
		else if (menuItem.m_Type == CSConst.etStorage)
		{
			if (menuItem.m_Value != null)
				m_Windows.m_StorageUI.Replace(menuItem.m_Value as List<CSStorage>);
			m_Windows.m_StorageUI.gameObject.SetActive(active);
		}
		else if (menuItem.m_Type == CSConst.etEnhance || menuItem.m_Type == CSConst.etRepair
				 || menuItem.m_Type == CSConst.etRecyle)
		{
			if (menuItem.m_Value != null)
			{
				//m_Windows.m_EngineeringUI.Replace(menuItem.m_Value as List<CSCommon>);
			}
			m_Windows.m_EngineeringUI.gameObject.SetActive(active);
		}
		else if (menuItem.m_Type == CSConst.etDwelling)
		{
			m_Windows.m_DwellingsUI.gameObject.SetActive(active);
		}
		else if (menuItem.m_Type == CSConst.etFarm)
		{
			m_Windows.m_FarmUI.gameObject.SetActive(active);
			m_Windows.m_FarmUI.SetFarm(menuItem.m_Entity as CSFarm);
		}
		else if (menuItem.m_Type == CSConst.etFactory)
		{
			NGUITools.SetActive(m_Windows.m_FactoryUI.gameObject, active);
			m_Windows.m_FactoryUI.SetEntity(menuItem.m_Entity);
		}
		else if (menuItem == m_MainLineNpcMI)
		{
			m_Windows.m_PersonnelUI.gameObject.SetActive(active);
			if (active)
				m_Windows.m_PersonnelUI.Type = CSUI_Personnel.TypeEnu.MainLine;
		}
		else if (menuItem == m_OthersNpcMI)
		{
			m_Windows.m_PersonnelUI.gameObject.SetActive(active);
			if (active)
				m_Windows.m_PersonnelUI.Type = CSUI_Personnel.TypeEnu.Other; 
		}
	}
	
	#region ADD_OR_REMOVE_BUILDING_ITEM

	public void AddBuildingItem (CSEntity entity)
	{
		if (entity == null)
		{
			Debug.LogWarning("reference building is NULL");
			return;
		}

		entity.AddEventListener(OnEntityHurt);
			
		if (entity.m_Type == CSConst.etStorage)
		{
			CSStorage storage = entity as CSStorage;

			if (storage.Assembly == null)
				_addDumpedStorageMenuItems(entity);
			else
			{
				_addValidMenuItems(entity);
			}
		}
		else if (entity.m_Type == CSConst.etDwelling)
		{
			if (m_DwellingBedMI == null)
			{
				m_DwellingBedMI = _addBuildingItem(entity);
				m_DwellingBedMI.m_Entity = null;
				m_DwellingBedMI.gameObject.name = "_" + CSUtils.GetEntityEnlishName(CSConst.etDwelling);
			}
			
			m_Windows.m_DwellingsUI.AddDwellings(entity as CSDwellings);
		}
		else if (entity.m_Type == CSConst.etEnhance || entity.m_Type == CSConst.etRepair
				 || entity.m_Type == CSConst.etRecyle)
		{
			CSCommon common = entity as CSCommon;

			if (common.Assembly == null)
				_addDumpedStorageMenuItems(entity);
			else
			{
				_addValidMenuItems(entity);
			}

		}
//		else if (!m_BuildingItems.Exists(buildingItem0 => buildingItem0.m_Entity == entity))
		else 
		{
			CSCommon common = entity as CSCommon;


			if (common == null || common.Assembly != null)
			{
				_addValidMenuItems(entity);
			}
			else if ( common.Assembly == null )
				_addDumpedStorageMenuItems(entity);

		}
			
		
		m_Menu.m_BuildingRoot.Reposition();
		m_Menu.m_Root.Reposition();
		m_Menu.m_BuildingRoot.repositionNow = true;
		
	}
	
	CSUI_MenuItem _addBuildingItem(CSEntity entity)
	{
		CSUI_MenuItem item    			= Instantiate(m_Menu.m_MenuItemPrefab) as CSUI_MenuItem;
		item.transform.parent 			= m_Menu.m_BuildingRoot.transform;
		item.transform.localRotation	= Quaternion.identity;
		item.transform.localScale		= Vector3.one;
		item.transform.localPosition    = Vector3.zero;
		
		item.m_Type		 = entity.m_Type;
		item.m_Entity	 = entity;
		item.Description = CSUtils.GetEntityName(item.m_Type);
		
		UICheckbox cb = item.gameObject.GetComponent<UICheckbox>();
		cb.radioButtonRoot = m_Menu.m_Root.transform;
		
		return item;		
	}
	
	public void RemoveBuildingItem (CSEntity entity)
	{
		if (entity == null)
		{
			Debug.LogWarning("reference building is NULL");
			return;
		}

		entity.RemoveEventListener(OnEntityHurt);
		
		if (entity.m_Type == CSConst.etStorage)
		{
			CSStorage storage = entity as CSStorage;

			if (storage.Assembly == null)
			{
				CSUI_MenuItem mi = _removeDumpedStorageMenuItems(entity);
				if (mi == m_ActiveMI && mi != null)
				{
					m_Windows.m_StorageUI.RemoveStorage(storage);
					ChangeWindow(m_ActiveMI, false);
				}
			}
			else
			{
				if (m_StorageMI == null)
					return;
				List<CSStorage>  list_storage = m_StorageMI.m_Value as List<CSStorage>;
				if ( list_storage.Remove(storage))
				{
					if (list_storage.Count == 0)
					{
						ChangeWindow(m_StorageMI, false);
						GameObject.DestroyImmediate(m_StorageMI.gameObject);
					}

					if (m_StorageMI == m_ActiveMI && m_StorageMI != null)
					{
						m_Windows.m_StorageUI.RemoveStorage(storage);
						ChangeWindow(m_ActiveMI, false);
					}
				}
			}
		}
		else if (entity.m_Type == CSConst.etDwelling)
		{
			m_Windows.m_DwellingsUI.RomveDwellings(entity as CSDwellings);
			
			if (m_DwellingBedMI != null && m_Windows.m_DwellingsUI.IsEmpty())
			{
				ChangeWindow(m_DwellingBedMI, false);
				GameObject.DestroyImmediate(m_DwellingBedMI.gameObject);
			}
		}
		else if (entity.m_Type == CSConst.etEnhance || entity.m_Type == CSConst.etRepair
			 	|| entity.m_Type == CSConst.etRecyle)
		{
			CSCommon csc = entity as CSCommon;
			if (csc.Assembly == null)
			{
				CSUI_MenuItem mi = _removeDumpedStorageMenuItems(entity);
				if (mi == m_ActiveMI && mi != null)
				{
					m_Windows.m_EngineeringUI.RemoveMachine(csc);
					ChangeWindow(m_ActiveMI, false);
				}
			}
			else
			{
				if (m_EngineeringMI == null)
					return;

				List<CSCommon>  list_common = m_EngineeringMI.m_Value as List<CSCommon>;
				if ( list_common.Remove(csc))
				{
					if (list_common.Count == 0)
					{
						ChangeWindow(m_EngineeringMI, false);
						GameObject.DestroyImmediate(m_EngineeringMI.gameObject);
					}
					
					if (m_EngineeringMI == m_ActiveMI && m_EngineeringMI != null)
					{
						m_Windows.m_EngineeringUI.RemoveMachine(csc);
						ChangeWindow(m_ActiveMI, false);
					}
				}
			}
		}
		else 
		{
			CSUI_MenuItem item = m_BuildingItems.Find( item0 => item0.m_Entity == entity);
			if (item != null)
			{
				// farm
				if (item.m_Entity.m_Type == CSConst.etAssembly)
				{
					CSAssembly assem = item.m_Entity as CSAssembly;
					CSUI_MenuItem farmItem = m_BuildingItems.Find( item0 => item0.m_Entity == assem.Farm);
					if (farmItem != null)
					{
						m_BuildingItems.Remove(farmItem);
						ChangeWindow(farmItem, false);
						GameObject.DestroyImmediate(farmItem.gameObject);
					}
				}

				m_BuildingItems.Remove(item);
				ChangeWindow(item, false);
				GameObject.DestroyImmediate(item.gameObject);
			}
		}
		
		m_Menu.m_BuildingRoot.Reposition();
		m_Menu.m_Root.Reposition();
		m_Menu.m_BuildingRoot.repositionNow = true;
		m_Menu.m_Root.repositionNow = true;
	}

	public void ClearAllMenueItems()
	{

		foreach (CSUI_MenuItem mi in m_BuildingItems)
		{
			GameObject.DestroyImmediate(mi.gameObject);
		}

		m_Windows.m_AssemblyUI.gameObject.SetActive(false);
		m_Windows.m_DwellingsUI.gameObject.SetActive(false);
		m_Windows.m_EngineeringUI.gameObject.SetActive(false);
		m_Windows.m_PPCoalUI.gameObject.SetActive(false);
		m_Windows.m_StorageUI.gameObject.SetActive(false);
		m_Windows.m_PersonnelUI.ResetUI();
		m_Windows.m_PersonnelUI.gameObject.SetActive(false);
	}

	private CSUI_MenuItem _addValidMenuItems (CSEntity entity)
	{
		CSUI_MenuItem item = null;

		switch(entity.m_Type)
		{
		case CSConst.etStorage:
		{
			if (m_StorageMI == null)
			{
				m_StorageMI = _addBuildingItem(entity);
				m_StorageMI.m_Entity = null;
				m_StorageMI.m_Value = new List<CSStorage>();
			}
			
			m_StorageMI.gameObject.name = "_" + CSUtils.GetEntityEnlishName(CSConst.etStorage);
			
			List<CSStorage> storages =  m_StorageMI.m_Value as List<CSStorage>;
			
			if (!storages.Contains(entity as CSStorage))
				storages.Add(entity as CSStorage);
			
			item = m_StorageMI;
		}break;
		case CSConst.etEnhance:
		case CSConst.etRepair:
		case CSConst.etRecyle:
		{
			if (m_EngineeringMI == null)
			{
				m_EngineeringMI = _addBuildingItem(entity);
				m_EngineeringMI.m_Entity = null;
				m_EngineeringMI.m_Value = new List<CSCommon>();
				m_EngineeringMI.Description = PELocalization.GetString(82210009);

			}

			m_EngineeringMI.gameObject.name = "_" +  CSUtils.GetEntityEnlishName(CSConst.etEngineer);

			List<CSCommon> commons =  m_EngineeringMI.m_Value as List<CSCommon>;
			
			if (!commons.Contains(entity as CSCommon))
				commons.Add(entity as CSCommon);

			item = m_EngineeringMI;
//			
//			m_Windows.m_EngineeringUI.AddMachine(entity);
		}break;
		default:
		{
			item = m_BuildingItems.Find(buildingItem0 => buildingItem0.m_Entity == entity);
			if (item == null)
			{
				item  = _addBuildingItem(entity);
				m_BuildingItems.Add(item);
			}

			item.gameObject.name = "_" + CSUtils.GetEntityEnlishName(entity.m_Type);
		}break;
		}
//		if (entity.m_Type == CSConst.etStorage)
//		{
//			if (m_StorageMI == null)
//			{
//				m_StorageMI = _addBuildingItem(entity);
//				m_StorageMI.m_Entity = null;
//				m_StorageMI.m_Value = new List<CSStorage>();
//			}
//
//			m_StorageMI.gameObject.name = "_" + CSUtils.GetEntityEnlishName(CSConst.etStorage);
//
//			List<CSStorage> storages =  m_StorageMI.m_Value as List<CSStorage>;
//
//			if (!storages.Contains(entity as CSStorage))
//				storages.Add(entity as CSStorage);
//
//			item = m_StorageMI;
//		}
//		else
//		{
//			item = m_BuildingItems.Find(buildingItem0 => buildingItem0.m_Entity == entity);
//			if (item == null)
//			{
//				item  = _addBuildingItem(entity);
//				m_BuildingItems.Add(item);
//			}
//
//			item.gameObject.name = "_" + CSUtils.GetEntityEnlishName(entity.m_Type);
//
//		}

		return item;
	}

	private void _addDumpedStorageMenuItems(CSEntity entity)
	{
		switch (entity.m_Type)
		{
		case CSConst.etStorage:
		{
			CSUI_MenuItem mi = m_DumpedSotageMIs.Find ( item0 =>
			                                           {
				List<CSStorage> storages = item0.m_Value as List<CSStorage>;
				
				if (storages == null)
				{
					Debug.Log("The menuItem is error");
					return false;
				}
				
				CSStorage storage = storages.Find(item1 => item1 == entity);
				if (storage == null && storages.Count < 8)
					return true;
				
				return false;
			});
			
			if (mi == null)
			{
				mi = _addBuildingItem(entity);
				mi.m_Entity = null;
				mi.m_Value = new List<CSStorage>();
				mi.m_Dumped = true;
				mi.Description = CSUtils.GetEntityName(CSConst.etStorage);
				m_DumpedSotageMIs.Add(mi);
			}
			
			mi.gameObject.name = CSUtils.GetEntityEnlishName(CSConst.etStorage);
			
			List<CSStorage> values = mi.m_Value as List<CSStorage>;
			if (values == null)
			{
				Debug.Log("The menuItem is error");
				return;
			}
			
			CSStorage css = entity as CSStorage;
			values.Add(css);
		}break;
		case CSConst.etEnhance:
		case CSConst.etRepair:
		case CSConst.etRecyle:
		{
			CSUI_MenuItem mi = m_DumpedEngineeringMIs.Find( item0 =>
			{
				List<CSCommon> commoms =  item0.m_Value  as List<CSCommon>;

				if (commoms == null)
				{
					Debug.Log("The menuItem is error");
					return false;
				}

				for (int i = 0; i < commoms.Count; i++)
				{
//					if (commoms[i] == entity )
//						already_have = true;

					if ( commoms[i].m_Type == entity.m_Type)
						return false;
				}

				return true;
			});

			if (mi == null)
			{
				mi = _addBuildingItem(entity);
				mi.m_Entity = null;
				mi.m_Value = new List<CSCommon>();
				mi.m_Dumped = true;
				mi.Description = CSUtils.GetEntityName(CSConst.etEngineer);
				m_DumpedEngineeringMIs.Add(mi);
			}

			mi.gameObject.name = CSUtils.GetEntityEnlishName(CSConst.etEngineer);

			List<CSCommon> values = mi.m_Value as List<CSCommon>;
			if (values == null)
			{
				Debug.Log("The menuItem is error");
				return;
			}
			
			CSCommon csc = entity as CSCommon;
			values.Add(csc);
		}break;
		default:
		{
			CSUI_MenuItem mi = m_BuildingItems.Find(buildingItem0 => buildingItem0.m_Entity == entity);
			
			if (mi == null)
			{
				mi = _addBuildingItem(entity);
				mi.m_Dumped = true;
				m_BuildingItems.Add(mi);
			}
			mi.gameObject.name = CSUtils.GetEntityEnlishName(entity.m_Type);
		}break;
		}

//		if (entity.m_Type == CSConst.etStorage)
//		{
//			CSUI_MenuItem mi = m_DumpedSotageMIs.Find ( item0 =>
//			{
//				List<CSStorage> storages = item0.m_Value as List<CSStorage>;
//
//				if (storages == null)
//				{
//					Debug.Log("The menuItem is error");
//					return false;
//				}
//
//			 	CSStorage storage = storages.Find(item1 => item1 == entity);
//				if (storage == null && storages.Count < 8)
//					return true;
//
//				return false;
//			});
//
//			if (mi == null)
//			{
//				mi = _addBuildingItem(entity);
//				mi.m_Entity = null;
//				mi.m_Value = new List<CSStorage>();
//				mi.m_Dumped = true;
//				m_DumpedSotageMIs.Add(mi);
//			}
//
//			mi.gameObject.name = CSUtils.GetEntityEnlishName(CSConst.etStorage);
//
//			List<CSStorage> values = mi.m_Value as List<CSStorage>;
//			if (values == null)
//			{
//				Debug.Log("The menuItem is error");
//				return;
//			}
//
//			CSStorage css = entity as CSStorage;
//			values.Add(css);
//		}
//
//		else
//		{
//			CSUI_MenuItem mi = m_BuildingItems.Find(buildingItem0 => buildingItem0.m_Entity == entity);
//
//			if (mi == null)
//			{
//				mi = _addBuildingItem(entity);
//				mi.m_Dumped = true;
//				m_BuildingItems.Add(mi);
//			}
//			mi.gameObject.name = CSUtils.GetEntityEnlishName(entity.m_Type);
//			
//		}
	}

	private CSUI_MenuItem _removeDumpedStorageMenuItems(CSEntity entity)
	{
//		if (entity.m_Type == CSConst.etStorage)
//		{
//			for (int i = 0; i < m_DumpedSotageMIs.Count;)
//			{
//				List<CSStorage>  list_storage = m_DumpedSotageMIs[i].m_Value as List<CSStorage>;
//				if ( list_storage.Remove(entity as CSStorage))
//				{
//					CSUI_MenuItem mi = m_DumpedSotageMIs[i];
//					if (list_storage.Count == 0)
//					{
//						GameObject.DestroyImmediate(m_DumpedSotageMIs[i].gameObject);
//						m_DumpedSotageMIs.RemoveAt(i);
//					}
//
//					return mi;
//				}
//				else
//					i++;
//			}
//		}

		switch (entity.m_Type)
		{
		case CSConst.etStorage:
		{
			for (int i = 0; i < m_DumpedSotageMIs.Count;)
			{
				List<CSStorage>  list_storage = m_DumpedSotageMIs[i].m_Value as List<CSStorage>;
				if ( list_storage.Remove(entity as CSStorage))
				{
					CSUI_MenuItem mi = m_DumpedSotageMIs[i];
					if (list_storage.Count == 0)
					{
						GameObject.Destroy(m_DumpedSotageMIs[i].gameObject);
						m_DumpedSotageMIs.RemoveAt(i);
					}
					
					return mi;
				}
				else
					i++;
			}
		}break;
		case CSConst.etEnhance:
		case CSConst.etRepair:
		case CSConst.etRecyle:
		{
			for (int i = 0; i < m_DumpedEngineeringMIs.Count;)
			{
				List<CSCommon> list_common = m_DumpedEngineeringMIs[i].m_Value as List<CSCommon>;
				if (list_common.Remove(entity as CSCommon))
				{
					CSUI_MenuItem mi = m_DumpedEngineeringMIs[i];
					if (list_common.Count == 0)
					{
						GameObject.Destroy(m_DumpedEngineeringMIs[i].gameObject);
						m_DumpedEngineeringMIs.RemoveAt(i);
					}

					return mi;
				}
				else
					i++;
			}
		}break;
		}

		return null;
	}
	
	#endregion
	

	CSUI_MenuItem _addNPCMenuItem(string desc)
	{
		CSUI_MenuItem item    			= Instantiate(m_Menu.m_MenuItemPrefab) as CSUI_MenuItem;
		item.transform.parent 			= m_Menu.m_PersonnelRoot.transform;
		item.transform.localRotation	= Quaternion.identity;
		item.transform.localScale		= Vector3.one;
		item.transform.localPosition    = Vector3.zero;
		
		item.Description = desc;
		item.m_Type		 = CSConst.etUnknow;
		
		UICheckbox cb = item.gameObject.GetComponent<UICheckbox>();
		cb.radioButtonRoot = m_Menu.m_Root.transform;
		cb.startsChecked = false;
		
		return item;
	}

	private CSEntity m_ActiveEnti;

	public bool SetSubWindowActive (CSEntity enti)
	{
		m_ActiveEnti = enti;

		return true; 
	}

	#region NGUI_CALLBACK
	
	new void OnClose ()
	{
		SelectItem_N.Instance.SetItem(null);
		base.OnClose ();
	}

	public void PlayTween ()
	{
		if (m_AlphaTween == null)
		{
			m_AlphaTween = Instantiate(m_AlphaTweenPrefab) as CSUI_AlphaTween;
			m_AlphaTween.transform.parent = transform.parent;
			m_AlphaTween.transform.localPosition = new Vector3(0, 300, 0);
			m_AlphaTween.transform.localRotation = Quaternion.identity;
			m_AlphaTween.transform.localScale = Vector3.one;
		}
		
		m_AlphaTween.Play(3);
	}

	void OnEntityHurt (int event_type, CSEntity cse, object arg)
	{
		if (event_type == CSConst.eetHurt)
		{
			if (m_AlphaTween == null)
			{
				m_AlphaTween = Instantiate(m_AlphaTweenPrefab) as CSUI_AlphaTween;
				m_AlphaTween.transform.parent = transform.parent;
				m_AlphaTween.transform.localPosition = new Vector3(0, 300, 0);
				m_AlphaTween.transform.localRotation = Quaternion.identity;
				m_AlphaTween.transform.localScale = Vector3.one;
			}

			m_AlphaTween.Play(3);

		}
		else if (event_type == CSConst.eetCommon_ChangeAssembly)
		{
			CSAssembly assem = arg as CSAssembly;
//			if (cse.m_Type == CSConst.etStorage)
//			{
//				CSStorage storage = cse as CSStorage;
//
//				if (assem == null)
//				{
//					List<CSStorage> storages =  m_StorageMI.m_Value as List<CSStorage>;
//					
//					if (storages.Remove(cse as CSStorage))
//					{
//						if (storages.Count == 0)
//						{
//							GameObject.DestroyImmediate(m_StorageMI.gameObject);
//						}
//						
//						if (m_ActiveMI == m_StorageMI && m_ActiveMI != null)
//							m_Windows.m_StorageUI.Replace(storages);
//						
//						_addDumpedStorageMenuItems(cse);
//					}
//				}
//				else
//				{
//					CSUI_MenuItem mi = _removeDumpedStorageMenuItems(cse);
//					
//					if (mi == m_ActiveMI && mi != null)
//						m_Windows.m_StorageUI.Replace(mi.m_Value as List<CSStorage>);
//
//					mi = _addValidMenuItems(cse);
//					List<CSStorage> storages =  mi.m_Value as List<CSStorage>;
//					
//					if (m_StorageMI == m_ActiveMI && m_StorageMI != null)
//						m_Windows.m_StorageUI.Replace(storages);
//				}
//			}
//			else 
//			{
//				CSUI_MenuItem mi = m_BuildingItems.Find(item0 => item0.m_Entity == cse);
//
//				if (mi != null)
//				{
//					if (assem == null)
//					{
//						mi.m_Dumped = true;
//						mi.gameObject.name = CSUtils.GetEntityEnlishName(cse.m_Type);
//					}
//					else
//					{
//						mi.m_Dumped = false;
//						mi.gameObject.name = "_" + CSUtils.GetEntityEnlishName(cse.m_Type);
//					}
//
//					m_Menu.m_BuildingRoot.repositionNow = true;
//					m_Menu.m_Root.repositionNow = true;
//				}
//			}

			switch(cse.m_Type)
			{
			case CSConst.etStorage:
			{
//				CSStorage storage = cse as CSStorage;
				
				if (assem == null)
				{
					List<CSStorage> storages =  m_StorageMI.m_Value as List<CSStorage>;
					
					if (storages.Remove(cse as CSStorage))
					{
						if (storages.Count == 0)
						{
							GameObject.DestroyImmediate(m_StorageMI.gameObject);
						}
						
						if (m_ActiveMI == m_StorageMI && m_ActiveMI != null)
							m_Windows.m_StorageUI.Replace(storages);
						
						_addDumpedStorageMenuItems(cse);
					}
				}
				else
				{
					CSUI_MenuItem mi = _removeDumpedStorageMenuItems(cse);
					
					if (mi == m_ActiveMI && mi != null)
						m_Windows.m_StorageUI.Replace(mi.m_Value as List<CSStorage>);
					
					mi = _addValidMenuItems(cse);
					List<CSStorage> storages =  mi.m_Value as List<CSStorage>;
					
					if (m_StorageMI == m_ActiveMI && m_StorageMI != null)
						m_Windows.m_StorageUI.Replace(storages);
				}
			}break;
			case CSConst.etEnhance:
			case CSConst.etRepair:
			case CSConst.etRecyle:
			{
				if (assem == null)
				{
					List<CSCommon> commons =  m_EngineeringMI.m_Value as List<CSCommon>;
					
					if (commons.Remove(cse as CSCommon))
					{
						if (commons.Count == 0)
						{
							GameObject.DestroyImmediate(m_EngineeringMI.gameObject);
						}
						
						if (m_ActiveMI == m_EngineeringMI && m_ActiveMI != null)
							//m_Windows.m_EngineeringUI.Replace(commons);
						
						_addDumpedStorageMenuItems(cse);
					}
				}
				else
				{
					CSUI_MenuItem mi = _removeDumpedStorageMenuItems(cse);
					
					//if (mi == m_ActiveMI && mi != null)
						//m_Windows.m_EngineeringUI.Replace(mi.m_Value as List<CSCommon>);
					
					mi = _addValidMenuItems(cse);
//					List<CSCommon> commons =  mi.m_Value as List<CSCommon>;
					
					//if (m_EngineeringMI == m_ActiveMI && m_EngineeringMI != null)
						//m_Windows.m_EngineeringUI.Replace(commons);
				}
			}break;
			default:
			{
				CSUI_MenuItem mi = m_BuildingItems.Find(item0 => item0.m_Entity == cse);
				
				if (mi != null)
				{
					if (assem == null)
					{
						mi.m_Dumped = true;
						mi.gameObject.name = CSUtils.GetEntityEnlishName(cse.m_Type);
					}
					else
					{
						mi.m_Dumped = false;
						mi.gameObject.name = "_" + CSUtils.GetEntityEnlishName(cse.m_Type);
					}
					
					m_Menu.m_BuildingRoot.repositionNow = true;
					m_Menu.m_Root.repositionNow = true;
				}
			}break;
			}

		}


	}

	void OnCommonAssemblyChanged (int event_type, CSEntity entity, object arg)
	{


	}
//	IEnumerator PlayColoyBtnTween()
//	{
//		m_ColoyBtnTween.enabled = true;
//		yield return new WaitForSeconds(2.5f);
//		m_ColoyBtnTween.enabled = false;
//		m_ColoyBtnTween.color = m_ColoyBtnTween.to;
//	}
	
	#endregion


	public static void ShowStatusBar (string text, float time = 4.5F)
	{
		if (m_Instance == null)
			return;

		CSUI_StatusBar.ShowText(text, new Color(0.0f, 0.2f, 1.0f, 0.0f), time);
	}

	public static void ShowStatusBar (string text, Color col, float time = 4.5F)
	{
		if (m_Instance == null)
			return;

		CSUI_StatusBar.ShowText(text, col, time);
	}
	#region UNITY_INNER_FUNC
	
	public override void InitWindow ()
	{
//		base.InitWindow();
//		m_Instance = this;
//		
//		if (m_MainLineNpcMI == null)
//			m_MainLineNpcMI = _addNPCMenuItem(PELocalization.GetString(82230011));
//		if (m_OthersNpcMI == null)
//			m_OthersNpcMI  = _addNPCMenuItem(PELocalization.GetString(82230012));
//
//		m_Windows.m_StorageUI.Init();
//		m_Windows.m_FarmUI.Init();
	}
	
	public override void AwakeWindow ()
	{
		base.AwakeWindow();
		//MainRightGui_N.Instance.ColoyUpdate(false);
	}
	
	void OnEnable ()
	{
		m_Menu.m_BuildingCB.isChecked = false;
		m_Menu.m_PersonnelCB.isChecked = false;

		ShowStatusBar(UIMsgBoxInfo.mOpenColonyTips.GetString());

	}
	
	
	// Use this for initialization
	void Start () 
	{
	}
	
	// Update is called once per frame
	void Update () 
	{
        if (player == null || playerTrans == null || player != PeCreature.Instance.mainPlayer)
        {
            player = PeCreature.Instance.mainPlayer;
            playerTrans = player.peTrans;
        }
		m_Menu.m_Root.repositionNow = true;

		if (m_ActiveEnti != null)
		{
			if (m_ActiveEnti.m_Type == CSConst.etStorage)
			{
				CSUI_MenuItem mi = null;
				List<CSStorage> storage_list = m_StorageMI.m_Value as List<CSStorage>;
				CSStorage storage = storage_list.Find(item0 => item0 == m_ActiveEnti);
				if (storage == null)
				{
					for (int i = 0; i < m_DumpedSotageMIs.Count; i++)
					{
						storage_list = m_DumpedSotageMIs[i].m_Value as List<CSStorage>;
						storage = storage_list.Find(item0 => item0 == m_ActiveEnti);
						if (storage != null)
						{
							mi = m_DumpedSotageMIs[i];
							break;
						}
					}
				}
				else
					mi = m_StorageMI;

				if (mi != null)
				{
					UICheckbox cb = mi.gameObject.GetComponent<UICheckbox>();
					cb.isChecked = true;
				}

				if (storage != null)
					m_Windows.m_StorageUI.m_SetActiveStorage = storage;
			}
			else if (m_ActiveEnti.m_Type == CSConst.etEnhance 
			         || m_ActiveEnti.m_Type == CSConst.etRepair
			         || m_ActiveEnti.m_Type == CSConst.etRecyle)
			{

				CSUI_MenuItem mi = null;
				List<CSCommon> commons_list = m_EngineeringMI.m_Value as List<CSCommon>;
				CSCommon common = commons_list.Find(item0 => item0 == m_ActiveEnti);
				if (common == null)
				{
					for (int i = 0; i < m_DumpedEngineeringMIs.Count; i++)
					{
						commons_list = m_DumpedEngineeringMIs[i].m_Value as List<CSCommon>;
						common = commons_list.Find(item0 => item0 == m_ActiveEnti);
						if (common != null)
						{
							mi = m_DumpedEngineeringMIs[i];
							break;
						}
					}
				}
				else
					mi = m_EngineeringMI;

				if (mi != null)
				{
					UICheckbox cb = mi.gameObject.GetComponent<UICheckbox>();
					cb.isChecked = true;
				}

				if (common != null)
					m_Windows.m_EngineeringUI.m_ActiveMachine = m_ActiveEnti;
//				UICheckbox cb = m_EngineeringMI.gameObject.GetComponent<UICheckbox>();
//				cb.isChecked = true;
//				m_Windows.m_EngineeringUI.m_ActiveMachine = m_ActiveEnti;
			}
			else if (m_ActiveEnti.m_Type == CSConst.etDwelling)
			{
				UICheckbox cb = m_DwellingBedMI.gameObject.GetComponent<UICheckbox>();
				cb.isChecked = true;
			}
			else 
			{
				CSUI_MenuItem mi = m_BuildingItems.Find(item0 => item0.m_Entity == m_ActiveEnti);
				if (mi != null)
				{
					UICheckbox cb = mi.gameObject.GetComponent<UICheckbox>();
					cb.isChecked = true;
				} 
			}
		}
		
		m_ActiveEnti = null;
	
		if (PeCreature.Instance.mainPlayer == null || m_Creator == null)
			m_WorkMode = EWorkType.UnKnown;
		else if (m_Creator.Assembly == null)
			m_WorkMode = EWorkType.NoAssembly;
		else
		{
			Vector3 playerPos = playerTrans.position;
			Vector3 assmPos = m_Creator.Assembly.Position;
			if ((playerPos - assmPos).sqrMagnitude < WorkDistance * WorkDistance)
				m_WorkMode = EWorkType.Working;
			else
				m_WorkMode = EWorkType.OutOfDistance;
		}
	}
	
	#endregion
}*/
