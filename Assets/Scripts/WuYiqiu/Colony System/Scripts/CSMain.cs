/**************************************************************
 *                       [CSMain.cs]
 *
 *    Colony System Main process Class
 *
 *     Main objects, params, data lists, main functions.
 *
 *
 **************************************************************/

//
//  Searching Keywords for CS:
//
//   <CETC> - Common Entity Type Cases, code considers common entity 
//			  type cases.
//

//--------------------------------------------------------------

#define NEW_CLOD_MGR

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CSRecord;
using System;
using Pathea;
using Pathea.PeEntityExt;

//--------------------------------------------------------------

public class CSMain : MonoBehaviour
{
    // Instance
    private static CSMain s_Instance;
    public static CSMain Instance { get { return s_Instance; } }

    // SinglePlayer Creator
    public static CSMgCreator s_MgCreator { 
		get { 
			CSCreator creator = null;
//			if(PeGameMgr.IsMulti)
//			{
//				if(PlayerNetwork.MainPlayer != null)
//					creator = MultiColonyManager.GetCreator(PlayerNetwork.MainPlayer.TeamId);
//			}
//			else
//			{
				creator = GetCreator(CSConst.ciDefMgCamp); 
//			}
			return (creator == null ? null : creator as CSMgCreator);			
		} 
	}
    public static CSNoMgCreator s_NoMgCreator { get { CSCreator creator = GetCreator(CSConst.ciDefNoMgCamp); return (creator == null ? null : creator as CSNoMgCreator); } }
    //multiple
    public Dictionary<int, CSCreator> otherCreators;
    public bool m_DebugGUI = true;

    public delegate void InitEvent();
    public static event InitEvent InitOperatItemEvent = null;

    // Counter Script
    [SerializeField]
    private GameObject m_CSCarrier;

    public List<CounterScript> m_CSList;


    // Create a counter script depend on the Carrier
    public CounterScript CreateCounter(string csName, float curTime, float finalTime)
    {
        CounterScript cs = m_CSCarrier.AddComponent<CounterScript>();

        cs.m_Description = csName;
        cs.Init(curTime, finalTime);

        m_CSList.Add(cs);

        return cs;
    }

    public void DestoryCounter(CounterScript cs)
    {
        if (cs != null)
        {
            m_CSList.Remove(cs);
            MonoBehaviour.Destroy(cs);
        }
    }
    public void RemoveCounter(CounterScript cs)
    {
        if (cs != null)
        {
            m_CSList.Remove(cs);
        }
    }
    // Debug use
    public void EndWithCounters()
    {
        //		foreach(CounterScript cs in m_CSList)
        //			cs.FinalCounter = 1;
    }


    #region CREATORS
    public Dictionary<int, CSCreator> m_Creators;

    public CSCreator CreateCreator(int ID, string desc, CSConst.CreatorType type = CSConst.CreatorType.Managed)
    {
        if (m_Creators.ContainsKey(ID))
        {
            Debug.Log("This ID [" + ID.ToString() + "] is exsit");
            return null;
        }

        CSCreator creator = null;
        if (type == CSConst.CreatorType.Managed)
        {
            GameObject go = new GameObject();
            go.transform.parent = transform;
            go.name = desc;
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;

            CSMgCreator mgCreator = go.AddComponent<CSMgCreator>();
            creator = mgCreator;

            creator.m_DataInst = CSDataMgr.CreateDataInst(ID, type);

            mgCreator.m_Clod = CSClodsMgr.CreateClod(ID);
            m_Creators.Add(ID, creator);
        }
        else if (type == CSConst.CreatorType.NoManaged)
        {
            GameObject go = new GameObject();
            go.transform.parent = transform;
            go.name = desc;
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;

            creator = go.AddComponent<CSNoMgCreator>();

            creator.m_DataInst = CSDataMgr.CreateDataInst(ID, type);

            m_Creators.Add(ID, creator);
        }


        return creator;
    }

    public static CSCreator GetCreator(int ID)
    {
        if (s_Instance == null)
            return null;
        if (s_Instance.m_Creators.ContainsKey(ID))
        {
            return s_Instance.m_Creators[ID];
        }
        else
        {
            Debug.Log("No Creator [" + ID.ToString() + "]");
            return null;
        }
    }
    public CSCreator MultiCreateCreator(int TeamNum)
    {
        if (otherCreators.ContainsKey(TeamNum))
        {
            Debug.Log("This TeamNum [" + TeamNum.ToString() + "] is exsit");
            return null;
        }

        CSCreator creator = null;

        GameObject go = new GameObject();
        go.transform.parent = transform;
        go.name = "Team " + TeamNum + " Managed Creator";
        go.transform.localPosition = Vector3.zero;
        go.transform.localRotation = Quaternion.identity;
        go.transform.localScale = Vector3.one;

        CSMgCreator mgCreator = go.AddComponent<CSMgCreator>();
        creator = mgCreator;
        CSConst.CreatorType type = CSConst.CreatorType.Managed;
        creator.m_DataInst = CSDataMgr.CreateDataInst(TeamNum, type);

        mgCreator.m_Clod = CSClodsMgr.CreateClod(TeamNum);
        mgCreator.teamNum = TeamNum;
        otherCreators.Add(TeamNum, creator);
        return creator;
    }
    public CSCreator MultiGetOtherCreator(int TeamNum, bool createNewIfNone = true)
    {
        if (s_Instance == null)
            return null;
        if (s_Instance.otherCreators.ContainsKey(TeamNum))
        {
            return s_Instance.otherCreators[TeamNum];
        }
        else
        {
            //Debug.LogError("No Creator [" + TeamNum.ToString() + "]");
            if (createNewIfNone)
            {
                return MultiCreateCreator(TeamNum);
            }
            else
            {
                return null;
            }
        }
    }

    #endregion

    /// <summary>
    /// Global Static Function and Members. 
    /// </summary>
    #region STATIC_FUNCS_AND_MEMBERS
    // Layer
    public static int CSEntityLayerIndex = 0;

    #endregion

    #region SOME_CALLBACK

    // CSClodMgr
    void OnDirtyVoxel(Vector3 pos, byte terrainType)
    {

#if NEW_CLOD_MGR

        foreach (KeyValuePair<int, CSCreator> kvp in m_Creators)
        {
            CSMgCreator mgCreator = kvp.Value as CSMgCreator;
            if (mgCreator == null)
                continue;

			if (mgCreator.Assembly != null && mgCreator.Assembly.InLargestRange(pos) && mgCreator.m_Clod != null)
            {
                RaycastHit rch;
                Vector3 realPos = Vector3.zero;
                if (Physics.Raycast(pos + new Vector3(0, 1, 0), Vector3.down, out rch, 2, 1 << Pathea.Layer.VFVoxelTerrain))
                    realPos = rch.point;
                else
                    realPos = pos;

                if (!FarmManager.Instance.mPlantHelpMap.ContainsKey(new IntVec3(pos)))
                {
                    mgCreator.m_Clod.AddClod(realPos, false);
                }
                else
                    mgCreator.m_Clod.AddClod(realPos, true);
            }
        }

#else
		CSMgCreator creator = GetCreator(CSConst.ciDefMgCamp) as CSMgCreator;
		if (creator.Assembly != null && creator.Assembly.InRange(pos))
		{
			RaycastHit rch;
			Vector3 realPos = Vector3.zero;
			if ( Physics.Raycast(pos + new Vector3(0, 1, 0), Vector3.down, out rch, 2, 1 << Pathea.Layer.VFVoxelTerrain) )
				realPos = rch.point;
			else
				realPos = pos;

			if ( !FarmManager.mPlantHelpMap.ContainsKey(new IntVec3(pos)) )
			{
				CSClodMgr.AddClod(realPos, false);
			}
			else
				CSClodMgr.AddClod(realPos, true);
		}
#endif

    }

    void OnCreatePlant(FarmPlantLogic plant)
    {
#if NEW_CLOD_MGR
        foreach (KeyValuePair<int, CSCreator> kvp in m_Creators)
        {
            CSMgCreator mgCreator = kvp.Value as CSMgCreator;
            if (mgCreator == null)
                continue;

            mgCreator.m_Clod.DirtyTheClod(plant.mPos, true);
        }
#else
		CSClodMgr.DirtyTheClod(plant.mPos, true);
#endif
    }

    void OnRemovePlant(FarmPlantLogic plant)
    {
#if NEW_CLOD_MGR
        foreach (KeyValuePair<int, CSCreator> kvp in m_Creators)
        {
            CSMgCreator mgCreator = kvp.Value as CSMgCreator;
            if (mgCreator == null)
                continue;

            mgCreator.m_Clod.DirtyTheClod(plant.mPos, false);
        }
#else
		CSClodMgr.DirtyTheClod(plant.mPos, false);
#endif

    }

    void OnDigTerrain(IntVector3 pos)
    {
#if NEW_CLOD_MGR
        foreach (KeyValuePair<int, CSCreator> kvp in m_Creators)
        {
            CSMgCreator mgCreator = kvp.Value as CSMgCreator;
            if (mgCreator == null)
                continue;

            mgCreator.m_Clod.DeleteClod(pos);
            VFVoxel voxel = VFVoxelTerrain.self.Voxels.SafeRead(pos.x, pos.y - 1, pos.z);
            if ((voxel.Type == PlantConst.DIRTY_TYPE0 || voxel.Type == PlantConst.DIRTY_TYPE1) && voxel.Volume > 128)
            {
                RaycastHit rch;
                if (Physics.Raycast(pos, Vector3.down, out rch, 2, 1 << Pathea.Layer.VFVoxelTerrain))
                    mgCreator.m_Clod.AddClod(rch.point, false);
                else
                    mgCreator.m_Clod.AddClod(new Vector3(pos.x, pos.y - 0.7f, pos.z), false);
            }
        }

#else
		CSClodMgr.DeleteClod(pos);

		VFVoxel voxel = VFVoxelTerrain.self.Voxels.SafeRead(pos.x, pos.y - 1, pos.z);
		if (voxel.Type == CSClodMgr.TerrainType)
		{
			RaycastHit rch;
			if ( Physics.Raycast(pos, Vector3.down, out rch, 2, 1 << Pathea.Layer.VFVoxelTerrain) )
				CSClodMgr.AddClod(rch.point, false);
			else
				CSClodMgr.AddClod(new Vector3(pos.x, pos.y - 0.7f, pos.z), false);
		}

#endif
    }

    #endregion

    #region UNITY_INTERNAL

    void OnDestroy()
    {
        CSDataMgr.Clear();
//        CSSimulatorDataMgr.ClearDataMgr();
        DigTerrainManager.onDirtyVoxel -= OnDirtyVoxel;
        FarmManager.Instance.RemovePlantEvent -= OnRemovePlant;
        FarmManager.Instance.CreatePlantEvent -= OnCreatePlant;
        DigTerrainManager.onDigTerrain -= OnDigTerrain;
    }

    void Awake()
    {

        if (s_Instance != null)
            Debug.LogError("CSMain must be only one!");
        else
            s_Instance = this;

        CSEntityLayerIndex = Pathea.Layer.Building;

        m_Creators = new Dictionary<int, CSCreator>();
        if (GameConfig.IsMultiMode)
        {
            otherCreators = new Dictionary<int, CSCreator>();
        }
        CSClodMgr.Init();
        CSClodsMgr.Init();



        CSCreator defMgCreator = CreateCreator(CSConst.ciDefMgCamp, "Default Managed Creator");
        if (GameConfig.IsMultiMode)
        {
            defMgCreator.teamNum = BaseNetwork.MainPlayer.TeamId;
            Debug.Log("Main Creator team: " + defMgCreator.teamNum);
        }
        CreateCreator(CSConst.ciDefNoMgCamp, "Default Non-Managed Creator", CSConst.CreatorType.NoManaged);

        DigTerrainManager.onDirtyVoxel += OnDirtyVoxel;
        FarmManager.Instance.CreatePlantEvent += OnCreatePlant;
        FarmManager.Instance.RemovePlantEvent += OnRemovePlant;
        DigTerrainManager.onDigTerrain += OnDigTerrain;

        // DefmgCreator UI
        //if ( CSUI_Main.Instance != null)
        //{
        //    CSUI_Main.Instance.Creator = defMgCreator;
        //}

        if (CSUI_MainWndCtrl.Instance != null)
        {
            CSUI_MainWndCtrl.Instance.Creator = defMgCreator;
        }
        if (InitOperatItemEvent != null)
        {
            InitOperatItemEvent();
            InitOperatItemEvent = null;
        }
    }

    void OnGUI()
    {
        //		if (GUI.Button(new Rect(400, 100, 100, 30), " Test"))
        //		{
        //			foreach ( CSSimulator csf in s_MgCreator.SimulatorMgr.Simulators.Values)
        //			{
        //				csf.Hp -= 100;
        //			}
        //		}
    }

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
	
	double lastCycle=-9999;
	int counter =0;
    void Update()
    { 
		if(PeGameMgr.IsSingle){
			counter++;
			if(counter>240){
				double currentCycle=GameTime.Timer.CycleInDay;
				if(lastCycle>-2&&lastCycle<-0.25&&currentCycle>=-0.25)
						RefreshColonyMoney();

				lastCycle=GameTime.Timer.CycleInDay;
				counter =0;
			}
	   }
    }

    #endregion

    public IEnumerator SearchVaildClodForAssembly(CSAssembly assem)
    {
        if (assem == null)
            yield break;
		if(assem.isSearchingClod)
			yield break;
		assem.isSearchingClod = true;
        CSMgCreator mgCreator = assem.m_Creator as CSMgCreator;

        mgCreator.m_Clod.Clear();

		int width = Mathf.RoundToInt(assem.LargestRadius);// - 10;
        int length = width;
        int height = width;

        Vector3 int_pos = new Vector3(Mathf.FloorToInt(assem.Position.x), Mathf.FloorToInt(assem.Position.y), Mathf.FloorToInt(assem.Position.z));
        Vector3 min_pos = int_pos - new Vector3(width, length, height);
        //Vector3 max_pos = int_pos + new Vector3(width, length, height);

        Vector3 pos = min_pos;

		float sqrRadius = assem.LargestRadius * assem.LargestRadius;
		
		length *= 2;
        width *= 2;
        height *= 2;

        int raycast_count = 0;
        int break_count = 0;
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < length; j++, break_count++)
            {
                Vector3 prv_pos = Vector3.zero;
                VFVoxel prv_voxel = new VFVoxel();
                for (int k = 0; k < height; k++)
                {
                    pos = min_pos + new Vector3(i, k, j);
                    if ((pos - int_pos).sqrMagnitude > sqrRadius)
                        continue;


                    VFVoxel voxel = VFVoxelTerrain.self.Voxels.SafeRead((int)pos.x, (int)pos.y, (int)pos.z);

                    if (voxel.Volume < 128)
                    {
                        if (prv_voxel.Volume > 128 && (prv_voxel.Type == 19 || prv_voxel.Type == 63))
                        {
                            RaycastHit rch;
                            Vector3 clod_pos = Vector3.zero;
                            if (Physics.Raycast(new Vector3(prv_pos.x, prv_pos.y + 1, prv_pos.z), Vector3.down, out rch, 2, 1 << Pathea.Layer.VFVoxelTerrain))
                            {
                                raycast_count++;
                                clod_pos = rch.point;
                            }
                            else
                                clod_pos = new Vector3(prv_pos.x, prv_pos.y + 0.4f, prv_pos.z);

#if NEW_CLOD_MGR
                            if (!FarmManager.Instance.mPlantHelpMap.ContainsKey(new IntVec3(prv_pos)))
                                mgCreator.m_Clod.AddClod(clod_pos, false);
                            else
                                mgCreator.m_Clod.AddClod(clod_pos, true);
#else
							if ( !FarmManager.mPlantHelpMap.ContainsKey( new IntVec3( prv_pos) ))
								CSClodMgr.AddClod(clod_pos, false);
							else
								CSClodMgr.AddClod(clod_pos, true);

#endif
                        }
                    }

                    prv_pos = pos;
                    prv_voxel = voxel;
                }

                if (break_count >= 30 || raycast_count >= 30)
                {
                    raycast_count = 0;
                    break_count = 0;
					yield return 0; 
					if (assem == null)
						yield break;
                }
            }
        }
		if (assem != null)
			assem.isSearchingClod = false;
		yield return 0;
    }

    public static void SinglePlayerCheckClod()
    {
        if (s_MgCreator == null || s_MgCreator.Assembly == null)
            return;
		if(s_MgCreator.Assembly.isSearchingClod)
			return;
        s_Instance.StartCoroutine(s_Instance.SearchVaildClodForAssembly(s_MgCreator.Assembly));
    }

    #region interface
	public static void RemoveNpc(PeEntity npc){
		CSMgCreator creator = null;
		if (PeGameMgr.IsMulti)
		{
			NetworkInterface npcnet = AiAdNpcNetwork.Get(npc.Id);
			if (npcnet == null)
				return;
			creator = MultiColonyManager.GetCreator(npcnet.TeamId);
		}
		else
		{
			creator = s_MgCreator;
		}
		if(creator!=null)
			creator.RemoveNpc(npc);
	}

    public static List<PeEntity> GetCSNpcs()
    {
        List<PeEntity> allNpcs = new List<PeEntity>();

        foreach (CSPersonnel csp in s_MgCreator.GetNpcs())
        {
            allNpcs.Add(csp.NPC);
        }

        return allNpcs;
    }

	public static List<PeEntity> GetCSNpcs(CSCreator creator){
		List<PeEntity> allNpcs = new List<PeEntity>();
		
		foreach (CSPersonnel csp in creator.GetNpcs())
		{
			allNpcs.Add(csp.NPC);
		}
		
		return allNpcs;
	}

    public static List<PeEntity> GetCSBuildings(CSCreator creator){

		List<PeEntity> allbuildings = new List<PeEntity>();
        CSMgCreator mgCreater = creator as CSMgCreator;
        if (mgCreater != null)
        {
            foreach (CSBuildingLogic csb in mgCreater.allBuildingLogic.Values)
            {
                allbuildings.Add(csb._peEntity);
            }
        }
		
		return allbuildings;
	}

    public static List<PeEntity> GetCSMainNpc()
    {
        List<PeEntity> mainNpcs = new List<PeEntity>();

        foreach (CSPersonnel csp in s_MgCreator.MainNpcs)
        {
            mainNpcs.Add(csp.NPC);
        }

        return mainNpcs;
    }
    public static List<PeEntity> GetCSRandomNpc()
    {
        List<PeEntity> randomNpcs = new List<PeEntity>();

        foreach (CSPersonnel csp in s_MgCreator.RandomNpcs)
        {
            randomNpcs.Add(csp.NPC);
        }

        return randomNpcs;
    }
	public static bool HasBuilding(int protoId,CSCreator creator){
		CSMgCreator mgCreater = creator as CSMgCreator;
			if (mgCreater != null)
		{
			foreach (CSBuildingLogic csb in mgCreater.allBuildingLogic.Values)
			{
				if(csb != null && csb.protoId==protoId){
					return true;
				}
			}
		}
		return false;
	}
	public static bool HasBuilding(int protoId,CSCreator creator,out Vector3 pos){
		CSMgCreator mgCreater = creator as CSMgCreator;
		pos = Vector3.zero;
		if (mgCreater != null)
		{
			foreach (CSBuildingLogic csb in mgCreater.allBuildingLogic.Values)
			{
				if(csb != null && csb.protoId==protoId){
					pos = csb.transform.position;
					return true;
				}
			}
		}
		return false;
	}
	
	public static bool HasCSAssembly()
	{
		if(s_MgCreator==null)
			return false;
        return s_MgCreator.Assembly != null;
    }

    public static bool GetAssemblyPos(out Vector3 pos)
    {
        pos = new Vector3();
        if (HasCSAssembly())
        {
            pos = s_MgCreator.Assembly.Position;
            return true;
        }
        pos = Vector3.zero;
        return false;
    }
	public static CSAssembly GetAssemblyEntity(){
		return s_MgCreator.Assembly;
	}
	public static CSBuildingLogic GetAssemblyLogic(){
		CSAssembly csa = s_MgCreator.Assembly;
		if(csa==null)
			return null;
		if(csa.gameLogic ==null)
			return null;
		return csa.gameLogic.GetComponent<CSBuildingLogic>();
	}
	public bool IsInAssemblyArea(Vector3 pos){
		if(s_MgCreator.Assembly!=null && s_MgCreator.Assembly.InRange(pos))
			return true;
		if(PeGameMgr.IsMulti){
			foreach(CSCreator csc in otherCreators.Values){
				if(csc.Assembly!=null&&csc.Assembly.InRange(pos))
					return true;
			}
		}
		return false;
	}
	public bool IsInOtherAssemblyArea(Vector3 pos){
		if(PeGameMgr.IsMulti){
			foreach(CSCreator csc in otherCreators.Values){
				if(csc.Assembly!=null&&csc.Assembly.InRange(pos))
					return true;
			}
		}
		return false;
	}
	public List<CSAssembly> GetAllAssemblies(){
		List<CSAssembly> resultList = new List<CSAssembly> ();
		if(s_MgCreator.Assembly!=null)
			resultList.Add(s_MgCreator.Assembly);
		if(PeGameMgr.IsMulti){
			foreach(CSCreator csc in otherCreators.Values){
				if(csc.Assembly!=null)
					resultList.Add(csc.Assembly);
			}
		}
		return resultList;
	}


    public static int GetEmptyBedRoom()
    {
        if (!HasCSAssembly())
            return 0;
        List<CSCommon> dwellingList = s_MgCreator.Assembly.Dwellings;
        if (dwellingList == null)
            return 0;
        int sumEmpty = 0;
        foreach (CSCommon dw in dwellingList)
        {
            CSDwellings d = dw as CSDwellings;
            if (d == null)
                continue;
            sumEmpty += d.GetEmptySpace();
        }
        return sumEmpty;
    }

	public static CSPersonnel GetColonyNpc(int id){
		CSMgCreator creator = null;
		if (PeGameMgr.IsMulti)
		{
			NetworkInterface npcnet = AiAdNpcNetwork.Get(id);
			if (npcnet == null)
				return null;
			creator = MultiColonyManager.GetCreator(npcnet.TeamId);
		}
		else
		{
			creator = s_MgCreator;
		}
		return creator.GetNpc(id);
	}
	//called by mainplayer
	public static bool SetNpcFollower(PeEntity npc){
		CSPersonnel csp = GetColonyNpc(npc.Id);
		if(csp!=null){
			if(csp.TrySetOccupation(CSConst.potFollower))
				return npc.SetFollower(true);
			return false;
		}
		return npc.SetFollower(true);
	}
	public static bool IsColonyNpc(int npcId){
		return GetColonyNpc (npcId)!=null;
	}
    #endregion

    #region hospital
    public static CSMedicalCheck FindCheckMachine(PeEntity npc)
    {
        CSMgCreator creator = null;
        if (PeGameMgr.IsMulti)
        {
            NetworkInterface npcnet = AiAdNpcNetwork.Get(npc.Id);
            if (npcnet == null)
                return null;
            creator = MultiColonyManager.GetCreator(npcnet.TeamId);
        }
        else
        {
            creator = s_MgCreator;
        }

        if (creator.Assembly == null)
            return null;
        if (creator.Assembly.MedicalCheck != null && creator.Assembly.MedicalCheck.IsRunning)
            return creator.Assembly.MedicalCheck;
        else
            return null;
    }
    public static CSMedicalTreat FindTreatMachine(PeEntity npc)
    {
        CSMgCreator creator = null;
        if (PeGameMgr.IsMulti)
        {
            NetworkInterface npcnet = AiAdNpcNetwork.Get(npc.Id);
            if (npcnet == null)
                return null;
            creator = MultiColonyManager.GetCreator(npcnet.TeamId);
        }
        else
        {
            creator = s_MgCreator;
        }
        if (creator.Assembly == null)
            return null;
        if (creator.Assembly.MedicalTreat != null && creator.Assembly.MedicalTreat.IsRunning)
            return creator.Assembly.MedicalTreat;
        else
            return null;
    }
    public static CSMedicalTent FindTentMachine(PeEntity npc)
    {
        CSMgCreator creator = null;
        if (PeGameMgr.IsMulti)
        {
            NetworkInterface npcnet = AiAdNpcNetwork.Get(npc.Id);
            if (npcnet == null)
                return null;
            creator = MultiColonyManager.GetCreator(npcnet.TeamId);
        }
        else
        {
            creator = s_MgCreator;
        }
        if (creator.Assembly == null)
            return null;
        if (creator.Assembly.MedicalTent != null && creator.Assembly.MedicalTent.IsRunning)
            return creator.Assembly.MedicalTent;
        else
            return null;
    }
    public static CSMedicalCheck FindMedicalCheck(out bool isReady, PeEntity npc)
    {
        isReady = false;
        if (PeGameMgr.IsMulti)
        {
            NetworkInterface npcnet = AiAdNpcNetwork.Get(npc.Id);
            if (npcnet == null)
                return null;
            CSMgCreator creator = MultiColonyManager.GetCreator(npcnet.TeamId);
            if (creator.Assembly == null)
                return null;
            if (creator.Assembly.MedicalCheck == null)
                return null;
            CSMedicalCheck check = creator.Assembly.MedicalCheck;
            if(!check.IsRunning)
				return null;
			isReady = check.IsReady(npc);
            if (npc.GetCmpt<NpcCmpt>().illAbnormals != null && npc.GetCmpt<NpcCmpt>().illAbnormals.Count > 0)
                check._ColonyObj._Network.RPCServer(EPacketType.PT_CL_CHK_FindMachine, npc.Id);
            return check;
        }
        else
        {
            if (s_MgCreator.Assembly == null)
                return null;
            if (s_MgCreator.Assembly.MedicalCheck == null)
                return null;
            CSMedicalCheck detector = s_MgCreator.Assembly.MedicalCheck;
            if (!detector.IsRunning)
                return null;
            isReady = detector.IsReady(npc);
            detector.AppointCheck(npc);
            return detector;
        }
    }

    public static bool TryGetCheck(PeEntity npc)
    {
        if (PeGameMgr.IsMulti)
        {
            NetworkInterface npcnet = AiAdNpcNetwork.Get(npc.Id);
            if (npcnet == null)
                return false;
            CSMgCreator creator = MultiColonyManager.GetCreator(npcnet.TeamId);
            if (creator.Assembly == null)
                return false;
            if (creator.Assembly.MedicalCheck == null)
                return false;
            CSMedicalCheck detector = creator.Assembly.MedicalCheck;
            if (!detector.IsRunning)
                return false;
            detector._ColonyObj._Network.RPCServer(EPacketType.PT_CL_CHK_TryStart, npc.Id);
        }
        else
        {
            if (s_MgCreator.Assembly == null)
                return false;
            if (s_MgCreator.Assembly.MedicalCheck == null)
                return false;
            CSMedicalCheck detector = s_MgCreator.Assembly.MedicalCheck;
            if (!detector.IsRunning)
                return false;
            //--to do : check npc detector
            return detector.StartCheck(npc);
        }
        return false;
    }

    public static CSMedicalTreat FindMedicalTreat(out bool isReady, PeEntity npc)
    {
        isReady = false;
        if (PeGameMgr.IsMulti)
        {
            NetworkInterface npcnet = AiAdNpcNetwork.Get(npc.Id);
            if (npcnet == null)
                return null;
            CSMgCreator creator = MultiColonyManager.GetCreator(npcnet.TeamId);
            if (creator.Assembly == null)
                return null;
            if (creator.Assembly.MedicalTreat == null)
                return null;
            CSMedicalTreat treat = creator.Assembly.MedicalTreat;
            if (!treat.IsRunning)
                return null;
			isReady = treat.IsReady(npc);
            treat._ColonyObj._Network.RPCServer(EPacketType.PT_CL_TRT_FindMachine, npc.Id);
            return treat;
        }
        else
        {
            if (s_MgCreator.Assembly == null)
                return null;
            if (s_MgCreator.Assembly.MedicalTreat == null)
                return null;
            CSMedicalTreat lab = s_MgCreator.Assembly.MedicalTreat;
            if (!lab.IsRunning)
                return null;
            isReady = lab.IsReady(npc);
            lab.AppointTreat(npc);
            return lab;
        }
        //--to do: inform player need treat
        //return null;
    }
    public static bool TryGetTreat(PeEntity npc)
    {
        if (PeGameMgr.IsMulti)
        {
            NetworkInterface npcnet = AiAdNpcNetwork.Get(npc.Id);
            if (npcnet == null)
                return false;
            CSMgCreator creator = MultiColonyManager.GetCreator(npcnet.TeamId);
            if (creator.Assembly == null)
                return false;
            if (creator.Assembly.MedicalTreat == null)
                return false;
            CSMedicalTreat treat = creator.Assembly.MedicalTreat;
            if (!treat.IsRunning)
                return false;
            treat._ColonyObj._Network.RPCServer(EPacketType.PT_CL_TRT_TryStart, npc.Id);
        }
        else
        {
            if (s_MgCreator.Assembly == null)
                return false;
            if (s_MgCreator.Assembly.MedicalTreat == null)
                return false;
            CSMedicalTreat treat = s_MgCreator.Assembly.MedicalTreat;
            if (!treat.IsRunning)
                return false;
            return treat.StartTreat(npc);
        }
        return false;
    }
    public static CSMedicalTent FindMedicalTent(out bool isReady, PeEntity npc, out Sickbed sickBed)
    {
        isReady = false;
        sickBed = null;
        if (PeGameMgr.IsMulti)
        {
            NetworkInterface npcnet = AiAdNpcNetwork.Get(npc.Id);
            if (npcnet == null)
                return null;
            CSMgCreator creator = MultiColonyManager.GetCreator(npcnet.TeamId);
            if (creator.Assembly == null)
                return null;
            if (creator.Assembly.MedicalTent == null)
                return null;
            CSMedicalTent tent = creator.Assembly.MedicalTent;
            if (!tent.IsRunning)
                return null;
            sickBed = tent.CheckNpcBed(npc);
            if (sickBed == null)
            {
                tent._ColonyObj._Network.RPCServer(EPacketType.PT_CL_TET_FindMachine, npc.Id);
            }
            else
            {
                isReady = true;
            }
            return tent;
        }
        else
        {
            if (s_MgCreator.Assembly == null)
                return null;
            if (s_MgCreator.Assembly.MedicalTent == null)
                return null;
            CSMedicalTent tent = s_MgCreator.Assembly.MedicalTent;
            if (!tent.IsRunning)
                return null;
            isReady = tent.IsReady(npc, out sickBed);
            tent.AppointTent(npc);
            return tent;
        }
    }
    public static bool TryGetTent(PeEntity npc)
    {
        if (PeGameMgr.IsMulti)
        {
            NetworkInterface npcnet = AiAdNpcNetwork.Get(npc.Id);
            if (npcnet == null)
                return false;
            CSMgCreator creator = MultiColonyManager.GetCreator(npcnet.TeamId);
            if (creator.Assembly == null)
                return false;
            if (creator.Assembly.MedicalTent == null)
                return false;
            CSMedicalTent tent = creator.Assembly.MedicalTent;
            if (!tent.IsRunning)
                return false;
            tent._ColonyObj._Network.RPCServer(EPacketType.PT_CL_TET_TryStart, npc.Id);
        }
        else
        {
            if (s_MgCreator.Assembly == null)
                return false;
            if (s_MgCreator.Assembly.MedicalTent == null)
                return false;
            CSMedicalTent tent = s_MgCreator.Assembly.MedicalTent;
            if (!tent.IsRunning)
                return false;
            return tent.StartTent(npc);
        }
        return false;
    }

    public static List<CSTreatment> GetTreatmentList()
    {
        if (PeGameMgr.IsMulti)
        {
            return s_MgCreator.m_TreatmentList;
        }
        else
        {
            return s_MgCreator.m_TreatmentList;
        }
    }

    public static void KickOutFromHospital(PeEntity npc)
    {
        CSMgCreator creator = null;
        if (PeGameMgr.IsMulti)
        {
            if (PeGameMgr.IsCustom)
                return;

            NetworkInterface npcnet = AiAdNpcNetwork.Get(npc.Id);
            if (npcnet == null)
                return;
            creator = MultiColonyManager.GetCreator(npcnet.TeamId);
        }
        else
        {
            creator = s_MgCreator;
        }
        if (creator == null)
            return;
        //find npc
        if (creator.Assembly == null)
            return;

        if (creator.Assembly.MedicalCheck != null && creator.Assembly.MedicalCheck.IsRunning)
            creator.Assembly.MedicalCheck.RemoveDeadPatient(npc.Id);
        if (creator.Assembly.MedicalTreat != null && creator.Assembly.MedicalTreat.IsRunning)
            creator.Assembly.MedicalTreat.RemoveDeadPatient(npc.Id);
        if (creator.Assembly.MedicalTent != null && creator.Assembly.MedicalTent.IsRunning)
            creator.Assembly.MedicalTent.RemoveDeadPatient(npc.Id);
        creator.m_TreatmentList.RemoveAll(it => it.npcId == npc.Id);
    }

	public static void StopTraining(PeEntity npc){
		CSMgCreator creator = null;
		if (PeGameMgr.IsMulti)
		{
			NetworkInterface npcnet = AiAdNpcNetwork.Get(npc.Id);
			if (npcnet == null)
				return;
			creator = MultiColonyManager.GetCreator(npcnet.TeamId);
		}
		else
		{
			creator = s_MgCreator;
		}
		if (creator == null)
			return;
		//find npc
		if (creator.Assembly == null)
			return;
	}
    #endregion

    #region cstrain
    public static List<CSPersonnel> GetInstructorList()
    {   
        CSMgCreator creator = null;
        creator = s_MgCreator;
        List<CSPersonnel> result = new List<CSPersonnel>();
        if (s_MgCreator.Assembly == null)
            return null;
        CSTraining cst = s_MgCreator.Assembly.TrainingCenter;
        if (cst == null)
            return null;
        foreach (int id in cst.InstructorList)
        {
            CSPersonnel csp = creator.GetNpc(id);
            if (csp != null)
                result.Add(csp);
        }
        return result;
    }

    public static List<CSPersonnel> GetTraineeList()
    {
        CSMgCreator creator = null;
        creator = s_MgCreator;
        List<CSPersonnel> result = new List<CSPersonnel>();
        if (s_MgCreator.Assembly == null)
            return null;
        CSTraining cst = s_MgCreator.Assembly.TrainingCenter;
        if (cst == null)
            return null;
        foreach (int id in cst.TraineeList)
        {
            CSPersonnel csp = creator.GetNpc(id);
            if (csp != null)
                result.Add(csp);
        }
        return result;
    }
    public static int GetInstructorCount()
    {
//        CSMgCreator creator = null;
//        creator = s_MgCreator;
        if (s_MgCreator.Assembly == null)
            return 0;
        CSTraining cst = s_MgCreator.Assembly.TrainingCenter;
        if (cst == null)
            return 0;
        return cst.InstructorList.Count;
    }

    public static int GetTraineeCount()
    {
//        CSMgCreator creator = null;
//        creator = s_MgCreator;
        if (s_MgCreator.Assembly == null)
            return 0;
        CSTraining cst = s_MgCreator.Assembly.TrainingCenter;
        if (cst == null)
            return 0;
        return cst.TraineeList.Count;
    }

    public static int GetInstructorMax()
    {
        return CSTraining.MAX_INSTRUCTOR_NUM;
    }
    public static int GetTraineeMax()
    {
        return CSTraining.MAX_TRAINEE_NUM;
    }
	#endregion

	#region TradePost
	//single mode only
	public static void AddTradeNpc(int npcId,List<int> storeIdList){
		if(s_MgCreator.AddedNpcId.Contains(npcId))
		   return;
		s_MgCreator.AddStoreId(storeIdList);
		s_MgCreator.AddedNpcId.Add(npcId);
	}

	public void RefreshColonyMoney(){
		s_MgCreator.RefreshMoney();
	}
    #endregion
}