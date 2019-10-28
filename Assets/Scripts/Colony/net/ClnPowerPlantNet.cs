using UnityEngine;
using System.Collections;
using CSRecord;
using ItemAsset;
using System.Collections.Generic;
using Pathea;
public partial class ColonyNetwork 
{

	void RPC_S2C_InitDataPowerPlanet(uLink.BitStream stream,uLink.NetworkMessageInfo info)
	{
		CSPowerPlanetData reocrdData = (CSPowerPlanetData)_ColonyObj._RecordData;
		reocrdData.m_CurDeleteTime = stream.Read <float>();
		reocrdData.m_CurRepairTime = stream.Read<float> ();
		reocrdData.m_DeleteTime = stream.Read<float> ();
		reocrdData.m_Durability = stream.Read<float> ();
		reocrdData.m_RepairTime = stream.Read<float> ();
		reocrdData.m_RepairValue = stream.Read<float> ();
		int [] keys = stream.Read<int[]> ();
		int [] values = stream.Read<int[]> ();
		
		for(int i = 0; i < keys.Length; i++)
		{
			reocrdData.m_ChargingItems[keys[i]] = values[i];
		}
	}
	public void POW_RemoveChargItem(int objId)
	{
        RPCServer(EPacketType.PT_CL_POW_GetChargItem, objId);
	}

	void RPC_S2C_POW_AddChargItem(uLink.BitStream stream,uLink.NetworkMessageInfo info)
	{
        int index = stream.Read <int>();
		int itemObjId = stream.Read <int>();
		bool success = stream.Read<bool>();
		if(m_Entity==null){
			Debug.LogError("entity not ready");
			return;
		}
        //1.data
        if (success)
        {
            ((CSPowerPlant)m_Entity).m_ChargingItems[index] = ItemMgr.Instance.Get(itemObjId).GetCmpt<Energy>();
            ((CSPowerPlanetData)_ColonyObj._RecordData).m_ChargingItems[index] = itemObjId;
        }
        //2.ui
        if (m_Entity is CSPPCoal)
        {
            CSUI_MainWndCtrl.Instance.PPCoalUI.AddChargeItemResult(success, index, itemObjId, (CSPPCoal)m_Entity);
        }
	}

	void RPC_S2C_POW_GetChargItem(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int itemObjId = stream.Read<int>();
		bool success = stream.Read<bool>();
		if(m_Entity==null){
			Debug.LogError("entity not ready");
			return;
		}
        //1.data
        if (success)
        {
            Energy[] entityData = ((CSPowerPlant)m_Entity).m_ChargingItems;
            Dictionary<int,int> recordData = ((CSPowerPlanetData)_ColonyObj._RecordData).m_ChargingItems;
            for (int i = 0; i < entityData.Length; i++)
            {
                if (entityData[i] != null && entityData[i].itemObj.instanceId == itemObjId)
                {
                    entityData[i] = null;
                    recordData.Remove(i);
                    break;
                }
            }
        }
        //2.ui
        if (m_Entity is CSPPCoal)
        {
            CSUI_MainWndCtrl.Instance.PPCoalUI.GetChargItemResult(success, itemObjId, (CSPPCoal)m_Entity);
        }
	}

	public void POW_AddChargItem(int index,ItemObject item)
	{
		if(item == null)
			return;

        //´íÎó #7042 Crash bug
        if (null == PeCreature.Instance || null == PeCreature.Instance.mainPlayer) return;

        PlayerPackageCmpt packageCmpt = PeCreature.Instance.mainPlayer.GetCmpt<PlayerPackageCmpt>();

        if (null == packageCmpt || null == packageCmpt.package || !packageCmpt.package.HasItemObj(item)) return;

        EquipmentCmpt equipCmpt = PeCreature.Instance.mainPlayer.GetCmpt<EquipmentCmpt>();

        if (null == equipCmpt || null == equipCmpt._ItemList || equipCmpt._ItemList.Contains(item)) return;

        RPCServer(EPacketType.PT_CL_POW_AddChargItem, index, item.instanceId);
	}
}









