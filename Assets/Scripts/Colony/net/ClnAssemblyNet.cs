using UnityEngine;
using System.Collections;
using CSRecord;
using System;
public partial class ColonyNetwork 
{
	void OnTeamChange()
	{
		if (null == ForceSetting.Instance|| null == m_Entity)
			return;

        //lz-2018.02.02 基地核心才需要地图图标
        if (m_Entity.m_Type == CSConst.dtAssembly)
        {
            Vector3 travelPos = Vector3.zero;

            CSBuildingLogic csb = m_Entity.gameLogic == null ? null : m_Entity.gameLogic.GetComponent<CSBuildingLogic>();
            //lz-2017.01.17 基地逻辑预制物体加载成功并且有传送点就用配置的传送点的位置
            if (csb != null && csb.travelTrans != null)
            {
                travelPos = csb.travelTrans.position;
            }
            else
            {
                //lz-2017.01.17 否则就用基地核心向上偏移两米的位置
                travelPos = _pos + new Vector3(0, 2, 0);
            }

            if (ForceSetting.Instance.Conflict(TeamId, PlayerNetwork.mainPlayerId))
            {
                ColonyLabel.Remove(travelPos);
            }
            else
            {
                if (!ColonyLabel.ContainsIcon(_pos))
                {
                    new ColonyLabel(travelPos);
                }
            }
        }
	}

	void RPC_S2C_InitDataAssembly(uLink.BitStream stream,uLink.NetworkMessageInfo info) 
	{
		CSAssemblyData reocrdData = (CSAssemblyData)_ColonyObj._RecordData;
		reocrdData.m_CurDeleteTime = stream.Read <float>();
		reocrdData.m_CurRepairTime = stream.Read<float> ();
		reocrdData.m_DeleteTime = stream.Read<float> ();
		reocrdData.m_Durability = stream.Read<float> ();
		reocrdData.m_RepairTime = stream.Read<float> ();
		reocrdData.m_RepairValue = stream.Read<float> ();
		reocrdData.m_Level	 = stream.Read<int>();
		reocrdData.m_TimeTicks = stream.Read<long> ();
		reocrdData.m_UpgradeTime = stream.Read<float> ();
		reocrdData.m_CurUpgradeTime = stream.Read<float> ();
		reocrdData.m_ShowShield =  stream.Read<bool> ();
	}

    //[RPC]
    //void RPC_S2C_AddAssemblyMask(uLink.BitStream stream)
    //{
    //    return;
    //    int _teamNum = stream.Read<int>();
    //    if (this.teamNum == _teamNum)
    //    {
    //        WorldMapManager.Instance.AddColony(transform.position);
    //    }
    //}

    void RPC_S2C_CounterTick(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        float upgradeTime = stream.Read<float>();
        CSAssembly csAssembly = m_Entity as CSAssembly;
        csAssembly.SetCounter(upgradeTime);
    }

    void RPC_S2C_ASB_LevelUp(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
        int level= stream.Read<int>();
		CSAssemblyData recordData = (CSAssemblyData)_ColonyObj._RecordData;
        recordData.m_UpgradeTime = -1;
		if(m_Entity==null){
			Debug.LogError("entity not ready");
			return;
		}
        CSAssembly csAssembly = m_Entity as CSAssembly;
        csAssembly.StopCounter();
        csAssembly.SetLevel(level);
		csAssembly.ChangeState();
		csAssembly.RefreshErodeMap();
        csAssembly.RefreshAssemblyObject();
        csAssembly.ExcuteEvent(CSConst.eetAssembly_Upgraded);
	}
	void RPC_S2C_ASB_QueryTime(uLink.BitStream stream,uLink.NetworkMessageInfo info)
	{
		CSAssemblyData reocrdData = (CSAssemblyData)_ColonyObj._RecordData;
		reocrdData.m_CurUpgradeTime	 = stream.Read<float>();
	}
    void RPC_S2C_ASB_LevelUpStart(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
        CSAssemblyData recordData = (CSAssemblyData)_ColonyObj._RecordData;
        recordData.m_CurUpgradeTime = stream.Read<float>();
        recordData.m_UpgradeTime = stream.Read<float>();
        string roleName = stream.Read<string>();
        bool success = stream.Read<bool>();
        if (m_Entity == null)
        {
            Debug.LogWarning("entity not ready");
            return;
        }

        CSAssembly csAssembly = m_Entity as CSAssembly;
        csAssembly.StartUpgradeCounter(recordData.m_CurUpgradeTime, recordData.m_UpgradeTime);
        if (CSUI_MainWndCtrl.Instance.AssemblyUI != null && success)
        {
            CSUI_MainWndCtrl.Instance.AssemblyUI.UpgradeStartSuccuss(csAssembly, roleName);
        }

    }

    void RPC_S2C_ASB_HideShield(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		bool showShield = stream.Read<bool>();
		if(m_Entity==null){
			Debug.LogError("entity not ready");
			return;
		}
        CSAssembly csAssembly = m_Entity as CSAssembly;
        csAssembly.bShowShield = showShield;
    }
	void RPC_S2C_ASB_ShowTips(uLink.BitStream stream, uLink.NetworkMessageInfo info){
		int eTipType = stream.Read<int>();
		int replaceStrId = stream.Read<int>();
		if(PlayerNetwork.mainPlayer!=null&&CSAutocycleMgr.Instance!=null)
			if(TeamId==BaseNetwork.MainPlayer.TeamId)
					CSAutocycleMgr.Instance.ShowTips((ETipType)eTipType,replaceStrId);
	}

	public void ASB_QueryCurUpTime()
	{
		RPCServer (EPacketType.PT_CL_ASB_QueryTime);
	}
	public void ASB_LevelUp()
	{
		RPCServer (EPacketType.PT_CL_ASB_LevelUp);
	}

    public void ASB_HideShield(bool showShield) 
    {
		RPCServer (EPacketType.PT_CL_ASB_HideShield,showShield);
    }
}

