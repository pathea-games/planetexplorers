using UnityEngine;
using System.Collections;
using ItemAsset;
using System.Collections.Generic;
using CSRecord;
using Pathea;
public class MultiColonyManager
{
    static MultiColonyManager mInstance;
    public static MultiColonyManager Instance
    {
        get
        {
            if (mInstance == null)
            {
                mInstance = new MultiColonyManager();
            } 
            return mInstance; }
    }

    public Dictionary<int, List<ColonyNetwork>> ColonyData;

    public List<ColonyNetwork> GetNetwork(int teamNum)
    {
        if (!ColonyData.ContainsKey(teamNum))
            return null;
        return ColonyData[teamNum];
    }

    public void AddNetworkData(ColonyNetwork colonyNetwork, int teamNum)
    {
        if (!ColonyData.ContainsKey(teamNum))
            ColonyData.Add(teamNum, new List<ColonyNetwork>());
        ColonyData[teamNum].Add(colonyNetwork);
    }

    public void AddNpcToColony(int id,int teamNum,int dwellingId)
    {
        CSMgCreator creator;
        if (teamNum == BaseNetwork.MainPlayer.TeamId)
        {
            creator = CSMain.s_MgCreator;
        }
        else
        {
            creator = CSMain.Instance.MultiGetOtherCreator(teamNum) as CSMgCreator;
        }

        //getnpc

        PeEntity npc = EntityMgr.Instance.Get(id); 
        //add: creator.add();

        creator.AddNpcInMultiMode(npc, dwellingId,true );

        //lw:init colony Npc Force
        //creator.InitForceData(npc, teamNum,playerId);
    }

    public static CSMgCreator GetCreator(int teamId, bool createNewIfNone = true)
    {
        CSMgCreator creator;
        if (teamId == BaseNetwork.MainPlayer.TeamId)
        {
            creator = CSMain.s_MgCreator;
        }
        else
        {
            creator = CSMain.Instance.MultiGetOtherCreator(teamId, createNewIfNone) as CSMgCreator;
        }
        return creator;
    }

    public void AddDataToCreator(ColonyNetwork colonyNetwork,int teamNum)
    {
        CSMgCreator creator;
		if (teamNum == BaseNetwork.MainPlayer.TeamId)
        {
            creator = CSMain.s_MgCreator;
        }
        else
        {
            creator = CSMain.Instance.MultiGetOtherCreator(teamNum) as CSMgCreator;
        }
        creator.teamNum = teamNum;
        CSObjectData objData = colonyNetwork._ColonyObj._RecordData as CSObjectData;
        
        if (objData != null
            //&& objData.ID != CSConst.etID_Farm//--to do:?
            )
        {
            //if (!objData.m_Alive)
            //    return;

            CSEntityAttr attr = new CSEntityAttr();
            attr.m_InstanceId = objData.ID;
            attr.m_Type = objData.dType;
			attr.m_Pos = objData.m_Position;
            attr.m_protoId = objData.ItemID;
            attr.m_Bound = objData.m_Bounds;
            attr.m_ColonyBase = colonyNetwork._ColonyObj;
            CSEntity cse = creator._createEntity(attr);
            colonyNetwork.m_Entity = cse;

            if (objData.dType == CSConst.dtAssembly)
            {
                CSAssembly csa = cse as CSAssembly;
                csa.ChangeState();

                
            }
            else
            {
                CSCommon csc = cse as CSCommon;
                if (creator.Assembly != null)
                    creator.Assembly.AttachCommonEntity(csc);
            

	            if (cse as CSDwellings != null)
	            {
	                cse._Net.RPCServer(EPacketType.PT_CL_DWL_SyncNpc);
	            }

				foreach(CSPersonnel csp in creator.GetNpcs())
				{
					if(csp.Data.m_WorkRoomID == attr.m_InstanceId &&csp.WorkRoom!=csc){
						csp.WorkRoom = csc;
					}
				}
			}
			//init worker



            creator.ExecuteEvent(CSConst.cetAddEntity, cse);
        }

    }

    public bool AssignData(int id, int type, ref CSDefaultData refData, ColonyBase _colony)
    {
        if (_colony != null&&_colony._RecordData!=null)
        {
            refData = _colony._RecordData;
            return false;
        }
        else
        {
            switch (type)
            {
                case CSConst.dtAssembly:
                    refData = new CSAssemblyData();
                    break;
                case CSConst.dtStorage:
                    refData = new CSStorageData();
                    break;
                case CSConst.dtEngineer:
                    refData = new CSEngineerData();
                    break;
                case CSConst.dtEnhance:
                    refData = new CSEnhanceData();
                    break;
                case CSConst.dtRepair:
                    refData = new CSRepairData();
                    break;
                case CSConst.dtRecyle:
                    refData = new CSRecycleData();
                    break;
                case CSConst.dtppCoal:
                    refData = new CSPPCoalData();
                    break;
                case CSConst.dtppSolar:
                    refData = new CSPPSolarData();
                    break;
                case CSConst.dtDwelling:
                    refData = new CSDwellingsData();
                    break;
                case CSConst.dtFarm:
                    refData = new CSFarmData();
                    break;
                case CSConst.dtFactory:
                    refData = new CSFactoryData();
                    break;
                case CSConst.dtProcessing:
                    refData = new CSProcessingData();
                    break;
                case CSConst.dtTrade:
                    refData = new CSTradeData();
                    break;
                case CSConst.dtTrain:
                    refData = new CSTrainData();
                    break;
                case CSConst.dtCheck:
                    refData = new CSCheckData();
                    break;
                case CSConst.dtTreat:
                    refData = new CSTreatData();
                    break;
                case CSConst.dtTent:
                    refData = new CSTentData();
					break;
				case CSConst.dtppFusion:
					refData = new CSPPFusionData();
					break;
				default:
					refData = new CSDefaultData();
					break;
            }
            refData.ID = id;
            return true;
        }
    }
}
