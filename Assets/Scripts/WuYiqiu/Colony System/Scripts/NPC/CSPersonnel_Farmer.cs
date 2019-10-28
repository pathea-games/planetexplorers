
#define NEW_CLOD_MGR

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ItemAsset;
using ItemAsset.PackageHelper;

//
//  Schedule For farmer. when the personnel is a famrer,
//		
//   	The most function in this partial class will be invoked.
//
public class FarmWorkInfo
{
    public FarmPlantLogic m_Plant;
    public Vector3 m_Pos;
    public ClodChunk m_ClodChunk;
    public FarmWorkInfo(FarmPlantLogic plant)
    {
        if (plant == null)
        {
            Debug.LogError("Giving plant must be not null.");
            Debug.DebugBreak();
        }

        m_Plant = plant;
        m_Pos = plant.mPos;
    }
    public FarmWorkInfo(ClodChunk clodChunk, Vector3 pos)
    {
        m_ClodChunk = clodChunk;
        m_Pos = pos;
    }
}
public partial class CSPersonnel : PersonnelBase 
{
	//  Work for farm
	public enum EFarmWorkType
	{
		None,
		Watering,
		Cleaning,
		Harvesting,
		Planting,
	}
	protected class FarmWorkInfo
	{
		public FarmPlantLogic m_Plant;
		public Vector3  m_Pos;
		public ClodChunk m_ClodChunk;
		public FarmWorkInfo (FarmPlantLogic plant)
		{
			if (plant == null)
			{
				Debug.LogError("Giving plant must be not null.");
				Debug.DebugBreak();
			}
			
			m_Plant = plant;
			m_Pos = plant.mPos;
		}
		public FarmWorkInfo (ClodChunk clodChunk, Vector3 pos)
		{
			m_ClodChunk = clodChunk;
			m_Pos = pos;
		}
	}
	protected Dictionary<EFarmWorkType, FarmWorkInfo> m_FarmWorkMap = new Dictionary<EFarmWorkType, FarmWorkInfo>();
	
	public void ClearFarmWorks ()
	{
		if (WorkRoom == null)
			return;
		CSFarm farm = WorkRoom as CSFarm;
		if (farm == null)
		{
			m_FarmWorkMap.Clear();
			return;
		}
		
		foreach (KeyValuePair<EFarmWorkType, FarmWorkInfo> kvp in m_FarmWorkMap)
		{
			switch (kvp.Key)
			{
			case EFarmWorkType.Watering:
				farm.RestoreWateringPlant(kvp.Value.m_Plant);
				break;
			case EFarmWorkType.Cleaning:
				farm.RestoreCleaningPlant(kvp.Value.m_Plant);
				break;
			case EFarmWorkType.Harvesting:
				farm.RestoreRipePlant(kvp.Value.m_Plant);
				break;
			case EFarmWorkType.Planting:
#if NEW_CLOD_MGR
				CSMgCreator mgCreator = m_Creator as CSMgCreator;
				if (mgCreator == null)
				{
					Debug.Log(" CSCreator is error");
					break;
				}

				mgCreator.m_Clod.DirtyTheChunk(kvp.Value.m_ClodChunk.m_ChunkIndex, false);

#else
				CSClodMgr.DirtyTheChunk(kvp.Value.m_ClodChunk.m_ChunkIndex, false);
#endif
				break;
			}
		}
		
		m_FarmWorkMap.Clear();
	}

	/// <summary>
	/// The main farmer function, invoke in npc think
	/// </summary>
	private void FarmerTick()
	{


        //--to do: new
        //if (m_State == CSConst.pstAtk )
        //{
        //    //--to do: wait enemy
        //    //if (m_Npc.enemy == null)
        //    //    SetTargetMonster(null);
        //    //else
        //    //    return;
        //}
        //else if (m_State == CSConst.pstPrepare)
        //    return;
		
        //if (WorkRoom as CSFarm == null)
        //{
        //    CSMgCreator mgCreator = m_Creator as CSMgCreator;
        //    if (mgCreator != null && mgCreator.Assembly != null)
        //    {
        //        if (mgCreator.Assembly.m_BelongObjectsMap[CSConst.ObjectType.Farm].Count != 0)
        //            SetWorkRoom(mgCreator.Assembly.m_BelongObjectsMap[CSConst.ObjectType.Farm][0]);
        //    }
			
        //    if (WorkRoom as CSFarm == null)
        //    {
        //        DwellerTick();
        //        return;
        //    }
        //}
		
		
        //// Calculate Time
        //int hour = (int) (GameTime.Timer.HourInDay);
		
        //if (m_ScheduleMap.ContainsKey(hour))
        //{
        //    if (m_ScheduleMap[hour] == EScheduleType.Rest)
        //    {
        //        if (m_State != CSConst.pstRest)
        //            Rest();
        //    }
        //    else if (m_ScheduleMap[hour] == EScheduleType.Work)
        //    {
        //        if (Stamina < 20)
        //        {
        //            if (m_State != CSConst.pstRest)
        //                Rest();
        //        }
        //        else if (Stamina > 300)
        //        {
        //            _farmWorkStyle();
        //        }
        //        else
        //        {
        //            if (m_State != CSConst.pstRest )
        //                _farmWorkStyle();
        //        }
        //    }
        //}
	}
	
	private void _farmWorkStyle()
	{
		CSFarm farm = WorkRoom as CSFarm;
		if (farm == null)
			return;
		
		FarmPlantLogic plant = null;

		CSMgCreator mgCreator = m_Creator as CSMgCreator;

		// Only watering and weeding 
		if (m_WorkMode == CSConst.pwtFarmForMag)
		{
			if (m_FarmWorkMap.Count != 0)
				return;
			
			ItemObject waterItem = farm.GetPlantTool(0);
			ItemObject weedingItem = farm.GetPlantTool(1);
			
			// Watering
			plant = waterItem == null ? null : farm.AssignOutWateringPlant();
			if (plant != null)
			{
				FarmWorkInfo fwi = new FarmWorkInfo(plant);
				m_FarmWorkMap.Add(EFarmWorkType.Watering, fwi);
				
				//_sendToWorkOnFarm(fwi.m_Pos);


			}  
			else
			{
				// Weeding
				plant = weedingItem == null ? null : farm.AssignOutCleaningPlant();
				
				if (plant != null)
				{
					FarmWorkInfo fwi = new FarmWorkInfo(plant);
					m_FarmWorkMap.Add(EFarmWorkType.Cleaning, fwi);
					
					//_sendToWorkOnFarm(fwi.m_Pos);
				}
                //else
                //    Idle(0.0f, false);
			}
		}
		else if (m_WorkMode == CSConst.pwtFarmForHarvest)
		{
			if (m_FarmWorkMap.Count != 0)
				return;
			
			CSStorage storage = null;
            
			foreach(CSStorage css in farm.Assembly.m_BelongObjectsMap[CSConst.ObjectType.Storage])
			{
                SlotList slotList = css.m_Package.GetSlotList();
                if(slotList.GetVacancyCount() >= 2)
				//if (css.m_Package.GetEmptyGridCount() >= 2)
				{
					storage = css;
					break;
				}
			}
			
			if (storage != null )
			{
				plant = farm.AssignOutRipePlant();
				
				if (plant != null)
				{
					FarmWorkInfo fwi = new FarmWorkInfo(plant);
					m_FarmWorkMap.Add(EFarmWorkType.Harvesting, fwi);
					//_sendToWorkOnFarm(fwi.m_Pos);
				}
                //else
                //    Idle(0.0f, false);
			}
            //else
            //    Idle(0.0f, false);
		}
		else if (m_WorkMode == CSConst.pwtFarmForPlant)
		{
			// Planting
			if (m_FarmWorkMap.Count == 0)
			{
#if NEW_CLOD_MGR
				ClodChunk cc = mgCreator.m_Clod.FindCleanChunk(farm.Assembly.Position, farm.Assembly.Radius);
#else
				ClodChunk cc = CSClodMgr.FindCleanChunk(farm.Assembly.Position, farm.Assembly.Radius);
#endif

				if ( farm.HasPlantSeed() && cc != null)
				{
					Vector3 pos;
                    bool flag= cc.FindCleanClod(out pos);
					if (flag)
					{
#if NEW_CLOD_MGR
						mgCreator.m_Clod.DirtyTheChunk(cc.m_ChunkIndex, true);
#else
						CSClodMgr.DirtyTheChunk(cc.m_ChunkIndex, true);
#endif
						FarmWorkInfo fwi = new FarmWorkInfo(cc, pos);
						m_FarmWorkMap.Add(EFarmWorkType.Planting, fwi);

                        _sendToWorkOnFarm(fwi.m_Pos);
					}
				}
                //else
                //    Idle(0.0f, false);
				
			}
			else  if (m_FarmWorkMap.ContainsKey(EFarmWorkType.Planting)
			          && m_FarmWorkMap[EFarmWorkType.Planting].m_Pos == Vector3.zero)
			{
				
				if (farm.HasPlantSeed())
				{
					FarmWorkInfo fwi = m_FarmWorkMap[EFarmWorkType.Planting];
                    bool flag = fwi.m_ClodChunk.FindCleanClod(out fwi.m_Pos);

                    if (flag)
                    {
                        //_sendToWorkOnFarm(fwi.m_Pos);
                    }
                    else
                        m_FarmWorkMap.Remove(EFarmWorkType.Planting);
				}
				else
				{
					FarmWorkInfo fwi = m_FarmWorkMap[EFarmWorkType.Planting];
#if NEW_CLOD_MGR
					mgCreator.m_Clod.DirtyTheChunk(fwi.m_ClodChunk.m_ChunkIndex, false);
#else
					CSClodMgr.DirtyTheChunk(fwi.m_ClodChunk.m_ChunkIndex, false);
#endif
					m_FarmWorkMap.Remove(EFarmWorkType.Planting);
				}
				
			}

            if (!m_FarmWorkMap.ContainsKey(EFarmWorkType.Planting))
            {
                //Idle(0.0f, false);
            }
		}
	}

    private void _sendToWorkOnFarm(Vector3 pos)
    {
        //m_State = CSConst.pstWork;
        //CmdToWorkWithoutObstacle(pos - new Vector3(0.5f, -1, 0.5f), Quaternion.identity, CSWorkBParam.EWorkType.Farm_Watering, 2.0f + Random.Range(0.5f, 2.5f));
    }
	
	// Invoke by OnWorkToDest in CSPersonnel_Cmd.cs
//    protected void _onWorkFarmToDest()
//    {
//        List<EFarmWorkType> reomve_type_list = new List<EFarmWorkType>();
//        CSFarm farm = WorkRoom as CSFarm;
//        foreach (KeyValuePair<EFarmWorkType, FarmWorkInfo> kvp in m_FarmWorkMap)
//        {
//            switch (kvp.Key)
//            {
//                case EFarmWorkType.Watering:
//                    {
//                        if (!kvp.Value.m_Plant.NeedWater)
//                            break;

//                        int needNum = kvp.Value.m_Plant.GetWaterItemCount();
//                        ItemObject water = farm.GetPlantTool(0);
//                        if (water != null)
//                        {
//                            int haveNum = water.GetCount();
//                            if (haveNum >= needNum)
//                            {
//                                int num = Mathf.Min(haveNum, needNum);
//                                kvp.Value.m_Plant.Watering(num);
//                                water.DecreaseStackCount(num);

//                                if (water.GetCount() <= 0)
//                                {
//                                    ItemMgr.Instance.DestroyItem(water.instanceId);
//                                    farm.SetPlantTool(0, null);
//                                }

//                                // consume Stamina
//                                Stamina = Mathf.Max(0, Stamina - STAMINA_CONSUMPTION_WATERING);
//                            }
//                            else
//                                farm.RestoreWateringPlant(kvp.Value.m_Plant);
//                        }
//                        else
//                            farm.RestoreWateringPlant(kvp.Value.m_Plant);

//                        reomve_type_list.Add(EFarmWorkType.Watering);
//                    } break;
//                case EFarmWorkType.Cleaning:
//                    {
//                        if (!kvp.Value.m_Plant.NeedClean)
//                            break;

//                        int needNum = kvp.Value.m_Plant.GetWeedingItemCount();
//                        ItemObject weed = farm.GetPlantTool(1);

//                        if (weed != null)
//                        {
//                            int haveNum = weed.GetCount();
//                            if (haveNum >= needNum)
//                            {
//                                int num = Mathf.Min(haveNum, needNum);
//                                kvp.Value.m_Plant.Weeding(num);
//                                weed.DecreaseStackCount(num);

//                                if (weed.GetCount() <= 0)
//                                {
//                                    ItemMgr.Instance.DestroyItem(weed.instanceId);
//                                    farm.SetPlantTool(1, null);
//                                }

//                                // consume Stamina
//                                Stamina = Mathf.Max(0, Stamina - STAMINA_CONSUMPTION_WEEDING);

//                            }
//                            else
//                                farm.RestoreCleaningPlant(kvp.Value.m_Plant);
//                        }
//                        else
//                            farm.RestoreCleaningPlant(kvp.Value.m_Plant);

//                        reomve_type_list.Add(EFarmWorkType.Cleaning);
//                    } break;
//                case EFarmWorkType.Harvesting:
//                    {
//                        int itemGetNum = kvp.Value.m_Plant.GetHarvestItemNum();
//                        if (farm.Assembly != null && farm.Plants.ContainsKey(kvp.Value.m_Plant.mInstanceId))
//                        {
//                            CSStorage storage = null;
//                            foreach (CSStorage css in farm.Assembly.m_BelongObjectsMap[CSConst.ObjectType.Storage])
//                            {
//                                SlotList slotList = css.m_Package.GetSlotList();

//                                if (slotList.GetVacancyCount() >= itemGetNum)
//                                {
//                                    storage = css;
//                                    break;
//                                }
//                            }

//                            if (storage != null)
//                            {
//                                Dictionary<int, int> retItems = kvp.Value.m_Plant.GetHarvestItemIds(itemGetNum);

//                                string item_str = "";
//                                ItemPackage accessor = storage.m_Package;

//                                foreach (int itemid in retItems.Keys)
//                                {
//                                    ItemSample addItem = new ItemSample(itemid, retItems[itemid]);

//                                    accessor.Add(itemid, retItems[itemid]);

//                                    item_str += addItem.protoData.GetName() + " x " + retItems[itemid].ToString() + ", ";
//                                }

//                                // [STORAGE_HISTORY]
//                                //						string str = CSStorage.c_TimeColorStr + GameTime.Timer.FormatString("hh : mm ") + UIMsgBoxInfo.mStorageHistory_1.GetString();
//                                //						storage.AddHistory(CSUtils.GetNoFormatString(str,  CSStorage.c_NameColorStr + m_Npc.NpcName + "[FFFFFF]", item_str));
//                                CSStorageHistoryAttr cssha = new CSStorageHistoryAttr();
//                                cssha.m_Day = (int)GameTime.Timer.Day;
//                                cssha.m_TimeStr = GameTime.Timer.FormatString("hh : mm ");
//                                cssha.m_NpcName = Name;
//                                cssha.m_ItemStr = item_str;
//                                cssha.m_Type = CSStorageHistoryAttr.EType.NpcAddSth;
//                                storage.AddHistory(cssha);

//                                FarmManager.Instance.RemovePlant(kvp.Value.m_Plant.mInstanceId);
//                                DragItem.Destory(kvp.Value.m_Plant.mInstanceId);
//                                //ItemMgr.Instance.DestroyItem(kvp.Value.m_Plant.mInstanceId);

//#if NEW_CLOD_MGR
//                                CSMgCreator mgCreator = m_Creator as CSMgCreator;
//                                mgCreator.m_Clod.AddClod(kvp.Value.m_Pos, false);
//#else
//                        CSClodMgr.AddClod(kvp.Value.m_Pos, false);
//#endif

//                                // consume Stamina
//                                Stamina = Mathf.Max(0, Stamina - STAMINA_CONSUMPTION_HARVEST);
//                            }
//                            else
//                            {
//                                farm.RestoreCleaningPlant(kvp.Value.m_Plant);
//                            }
//                        }

//                        reomve_type_list.Add(EFarmWorkType.Harvesting);
//                    } break;
//                case EFarmWorkType.Planting:
//                    {
//                        Vector3 pos = kvp.Value.m_ClodChunk.FindCleanClod();

//                        if (pos == kvp.Value.m_Pos)
//                        {
//                            farm.PlantTo(pos);
//                            kvp.Value.m_ClodChunk.DirtyTheClod(pos, true);
//                            kvp.Value.m_Pos = Vector3.zero;

//                            if (kvp.Value.m_ClodChunk.m_IdleClods.Count == 0)
//                            {
//                                reomve_type_list.Add(EFarmWorkType.Planting);
//                            }

//                            // consume Stamina
//                            Stamina = Mathf.Max(0, Stamina - STAMINA_CONSUMPTION_PLANT);
//                        }
//                        else
//                        {
//                            kvp.Value.m_Pos = Vector3.zero;
//                        }

//                    } break;
//                default:
//                    break;
//            }

//        }

//        //m_FarmWorkMap.Clear();
//        foreach (EFarmWorkType fwt in reomve_type_list)
//        {
//            m_FarmWorkMap.Remove(fwt);
//        }

//        ClearAllBehave();

//        //--to do: wait cmdidle
//        //m_Npc.CmdIdle = true;
//    }

}
