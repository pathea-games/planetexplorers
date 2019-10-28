using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Timers;
using System.IO;
using System.Linq;
using System.Text;
using ItemAsset;
using CSRecord;
public class ColonyIDInfo
{
	public const int COLONY_FAMILY 				= 4007;
	public const int COLONY_ASSEMBLY			= ProtoTypeId.ASSEMBLY_CORE;
	public const int COLONY_PPCOAL				= ProtoTypeId.PPCoal;
	public const int COLONY_STORAGE			= ProtoTypeId.STORAGE;
	public const int COLONY_REPAIR				= ProtoTypeId.REPAIR_MACHINE;
	public const int COLONY_DWELLINGS 		= ProtoTypeId.DWELLING_BED;
	public const int COLONY_ENHANCE			= ProtoTypeId.ENHANCE_MACHINE;
	public const int COLONY_RECYCLE			= ProtoTypeId.RECYCLE_MACHINE;
	public const int COLONY_FARM					= ProtoTypeId.FARM;
	public const int COLONY_FACTORY 			= ProtoTypeId.FACTORY_REPLICATOR;
	public const int COLONY_PROCESSING = ProtoTypeId.PROCESSING;
	public const int COLONY_TRADE = ProtoTypeId.TRADE_POST;
    public const int COLONY_TRAIN = ProtoTypeId.TRAINING_CENTER;
    public const int COLONY_CHECK = ProtoTypeId.MEDICAL_CHECK;
    public const int COLONY_TREAT = ProtoTypeId.MEDICAL_TREAT;
	public const int COLONY_TENT = ProtoTypeId.MEDICAL_TENT;
	public const int COLONY_FUSION = ProtoTypeId.PPFusion;
}
public class ColonyMgr
{
	static Dictionary<int ,Dictionary<int,List<ColonyBase>>> ColonyMap = new Dictionary<int ,Dictionary<int,List<ColonyBase>>>();

	public static void AddColonyItem(ColonyBase item)
	{
		if (item == null || item._Network == null)
			return;
		if (item == null || item._Network == null)
			return;
		if (!ColonyMap.ContainsKey(item._Network.TeamId))
			ColonyMap[item._Network.TeamId] = new Dictionary<int, List<ColonyBase>>();

		if (!ColonyMap[item._Network.TeamId].ContainsKey(item._Network.ExternId))
			ColonyMap[item._Network.TeamId][item._Network.ExternId] = new List<ColonyBase>();

		if (!ColonyMap[item._Network.TeamId][item._Network.ExternId].Contains(item))
			ColonyMap[item._Network.TeamId][item._Network.ExternId].Add(item);
	}
	public static void RemoveColonyItem(ColonyBase item)
	{
		if (item == null || item._Network == null)
			return;
		if (ColonyMap.ContainsKey(item._Network.TeamId) && ColonyMap[item._Network.TeamId].ContainsKey(item._Network.ExternId))
			ColonyMap[item._Network.TeamId][item._Network.ExternId].Remove(item);
	}

	public static int GetColonyItemAmount(int teamNum,int colonyType)
	{
		if(!ColonyMap.ContainsKey(teamNum) || !ColonyMap[teamNum].ContainsKey(colonyType))
			return 0;
		return ColonyMap[teamNum][colonyType].Count;
	}

	public bool HavePower(int teamNum)
	{
		if(!ColonyMap.ContainsKey(teamNum)  || !ColonyMap[teamNum].ContainsKey(ColonyIDInfo.COLONY_PPCOAL))
			return false;
		
		for( int i = 0 ; i < ColonyMap[teamNum][ColonyIDInfo.COLONY_PPCOAL].Count; i ++)
		{
			if(((ColonyPPCoal)(ColonyMap[teamNum][ColonyIDInfo.COLONY_PPCOAL][i])).IsWorking())
				return true;
		}
		return false;
	}

	public static ColonyBase GetColonyItemByObjId(int objId)
	{
		foreach (KeyValuePair<int ,Dictionary<int,List<ColonyBase>>> kvp in ColonyMap)
		{
			foreach( KeyValuePair<int,List<ColonyBase>> kvp1 in kvp.Value)
			{
				foreach(ColonyBase item in kvp1.Value)
				{
					if( item._Network.Id == objId)
						return item;
				}
			}
		} 
		return null;
	}




}
public class ColonyBase
{
	internal ColonyNetwork _Network;
	public CSObjectData _RecordData;

	public ColonyBase()
	{

	}
	public void SetNetwork(ColonyNetwork network)
	{
		if (network == null)
			return;
		_Network = network;
		ColonyMgr.AddColonyItem (this);
	}
	
	public void RecycleItems()
	{
		_Network.RPCServer (EPacketType.PT_CL_Recycle);
	}
	public void Repair()
	{
		_Network.RPCServer (EPacketType.PT_CL_Repair);
	}

}

