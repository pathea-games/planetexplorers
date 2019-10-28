using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ItemAsset;
using CSRecord;
using Pathea;
using Pathea.PeEntityExt;

public partial class AiAdNpcNetwork : AiNetwork
{
	void InitClnNpcData()
	{
        RPCServer(EPacketType.PT_CL_CLN_InitData);
	}
	public void SetClnState(int state)
	{
        RPCServer(EPacketType.PT_CL_CLN_SetState, state);
	}
	public void SetClnDwellingsID(int dwellingsID)
	{
        RPCServer(EPacketType.PT_CL_CLN_SetDwellingsID, dwellingsID);
	}
	public void SetClnWorkRoomID(int workRoomID)
	{
        RPCServer(EPacketType.PT_CL_CLN_SetWorkRoomID,  workRoomID);
	}
	public void SetClnOccupation(int occupation)
	{
        RPCServer(EPacketType.PT_CL_CLN_SetOccupation, occupation);
	}
	public void SetClnWorkMode(int workMode)
	{
        RPCServer(EPacketType.PT_CL_CLN_SetWorkMode,  workMode);
	}
	public void PlantGetBack(int objId,int farmId)
	{
        RPCServer(EPacketType.PT_CL_CLN_PlantGetBack, objId, farmId);
	}


	public void PlantPutOut(Vector3 pos,int farmId,byte terrainType)
	{
        RPCServer(EPacketType.PT_CL_CLN_PlantPutOut, pos, farmId, terrainType);
	}
	public void PlantWater(int objID,int farmId)
	{
        RPCServer(EPacketType.PT_CL_CLN_PlantWater, objID, farmId);
	}
	public void PlantClean(int objID,int farmId)
	{
        RPCServer(EPacketType.PT_CL_CLN_PlantClean, objID, farmId);
	}
	public void PlantClear(int objID)
	{
        RPCServer(EPacketType.PT_CL_CLN_PlantClear, objID);
	}

	void RPC_S2C_CLN_InitData(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int dwellingsID = stream.Read<int>();
		Vector3 guardPos = stream.Read<Vector3>();
		int occupation = stream.Read<int>();
		int state = stream.Read<int>();
		int workMode = stream.Read<int>();
		int workRoomID = stream.Read<int>();
        bool isProcessing = stream.Read<bool>();
        int processingIndex = stream.Read<int>();
		int trainerType = stream.Read<int>();
		int trainingType = stream.Read<int>();
		bool isTraining = stream.Read<bool>();

		CSPersonnelData cspd = new CSPersonnelData();
		cspd.ID 			= Id;
		cspd.dType			= CSConst.dtPersonnel;
		cspd.m_State		= state;
        cspd.m_Occupation = occupation;
        cspd.m_WorkMode = workMode;
		cspd.m_DwellingsID	= dwellingsID;//--to do:remove
		cspd.m_GuardPos = guardPos;
		cspd.m_WorkRoomID	= workRoomID;
        cspd.m_IsProcessing = isProcessing;
        cspd.m_ProcessingIndex = processingIndex;
		cspd.m_TrainerType = trainerType;
		cspd.m_TrainingType = trainingType;
		cspd.m_IsTraining = isTraining;
		//--to do: new attribute

        StartCoroutine(InitDataRoutine(cspd));
		
	}

    IEnumerator InitDataRoutine(CSPersonnelData cspd)
    {
        PeEntity npc = null;
        while (true)
        {
            npc = EntityMgr.Instance.Get(Id);
            if (npc == null)
                yield return new WaitForSeconds(0.5f);
            else
                break;
        }
        

		CSCreator creator =null;
		while (true)
		{ 
            creator = MultiColonyManager.GetCreator(TeamId);
			if(creator==null)
				yield return new WaitForSeconds(0.5f);
			else
				break;
		}
		((CSMgCreator)creator).AddNpc(npc, cspd, true);
//		if (TeamId == PlayerNetwork.MainPlayer.TeamId)
//        {
//            CSCreator creator = CSMain.GetCreator(CSConst.ciDefMgCamp);
//            while (creator == null)
//            {
//                yield return new WaitForSeconds(0.5f);
//				creator = MultiColonyManager.GetCreator(CSConst.ciDefMgCamp);
//            }
//
//
//            ((CSMgCreator)creator).AddNpc(npc, cspd, true);
//        }
//        else
//        {
//            CSCreator creator = CSMain.GetCreator(TeamId);
//            while (creator == null)
//            {
//                yield return new WaitForSeconds(0.5f);
//                creator = MultiColonyManager.GetCreator(TeamId);
//            }
//
//            ((CSMgCreator)creator).AddNpc(npc, cspd, true);
//        }
        //m_PersonnelDatas.Add(cspd.ID, cspd);
    }
	
	void RPC_S2C_CLN_SetState(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
        //--to do: wait
        //int state = stream.Read<int>();
        //int playID = stream.Read<int>();
        //if(playID != PlayerFactory.mMainPlayer.OwnerView.viewID.id)
        //{
        //    CSCreator creator = CSMain.GetCreator(CSConst.ciDefMgCamp);
        //    if (creator != null)
        //    {
        //        CSPersonnel[] personnels = creator.GetNps();
        //        foreach (CSPersonnel csp in personnels)
        //        {
        //            if(csp != null && csp.m_Npc != null && csp.m_Npc.Netlayer != null && csp.m_Npc.Netlayer is AiAdNpcNetwork)
        //            {
        //                if(objectID == ((AiAdNpcNetwork)csp.m_Npc.Netlayer).objectID)
        //                {
        //                    //csp.State = state;
        //                }
        //            }
        //        }
        //    }
        //}
	}

	void RPC_S2C_CLN_SetDwellingsID(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
        //--to do: wait
        int dwllingsID = stream.Read<int>();
        //int playID = stream.Read<int>();
        //if (playID != PlayerFactory.mMainPlayer.OwnerView.viewID.id)
        //{
        CSMgCreator creator = MultiColonyManager.GetCreator(TeamId,false);
        if (creator != null)
        {
            CSPersonnel[] personnels = creator.GetNpcs();
            foreach (CSPersonnel csp in personnels)
            {
                if (csp != null && csp.m_Npc != null)
                {
                    if (Id == csp.m_Npc.Id)
                    {
                        CSDwellings cd = creator.GetCommonEntity(dwllingsID) as CSDwellings;
                        cd.AddNpcs(csp);
                    }
                }
            }
        }
        //}
	}

	void RPC_S2C_CLN_SetWorkRoomID(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
        //--to do: wait
        int workRoomID = stream.Read<int>();
        //int playID = stream.Read<int>();
        //if (playID != PlayerFactory.mMainPlayer.OwnerView.viewID.id)
        //{
        CSCreator creator = MultiColonyManager.GetCreator(TeamId,false);
        if (creator != null)
        {
            CSPersonnel[] personnels = creator.GetNpcs();
            foreach (CSPersonnel csp in personnels)
            {
                if (csp != null && csp.m_Npc != null)
                {
                    if (Id == csp.m_Npc.Id)
                    {
                        if (workRoomID == 0)
                        {
                            csp.WorkRoom = null;
                            return;
                        }
                        Dictionary<int, CSCommon> commons = creator.GetCommonEntities();
                        foreach (KeyValuePair<int, CSCommon> kvp in commons)
                        {
                            if (kvp.Value.Assembly != null && kvp.Value.WorkerMaxCount > 0 && kvp.Value.m_Type != CSConst.etFarm)
                            {
                                if (kvp.Value.ID == workRoomID)
                                {
                                    csp.WorkRoom=kvp.Value;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }
        //}
	}

	void RPC_S2C_CLN_SetOccupation(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
        //--to do: wait
        int occupation = stream.Read<int>();
        int workmode = stream.Read<int>();
        int workroomId = stream.Read<int>();
        //int playID = stream.Read<int>();
        //if(playID != PlayerFactory.mMainPlayer.OwnerView.viewID.id)
        //{
        CSCreator creator = MultiColonyManager.GetCreator(TeamId,false);
            if (creator != null)
            {
                CSPersonnel[] personnels = creator.GetNpcs();
                foreach (CSPersonnel csp in personnels)
                {
                    if(csp != null && csp.m_Npc != null)
                    {
                        if(Id == csp.m_Npc.Id)
                        {
                            csp.m_Occupation = occupation;
                            csp.m_WorkMode = workmode;
                            csp.WorkRoom = creator.GetCommonEntity(workroomId);
                        }
                    }
                }
            }
        //}
	}

	void RPC_S2C_CLN_SetWorkMode(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
        //--to do: wait
        int workMode = stream.Read<int>();
        //int playID = stream.Read<int>();
        //if(playID != PlayerFactory.mMainPlayer.OwnerView.viewID.id)
		//{
		CSCreator creator = MultiColonyManager.GetCreator(TeamId,false);
            if (creator != null)
            {
                CSPersonnel[] personnels = creator.GetNpcs();
                foreach (CSPersonnel csp in personnels)
                {
                    if (csp != null && csp.m_Npc != null)
                    {
                        if (Id == csp.m_Npc.Id)
                        {
                            csp.m_WorkMode = workMode;
                        }
                    }
                }
            }
        //}
	}

//	void RPC_S2C_CLN_SetGuardPos(uLink.BitStream stream, uLink.NetworkMessageInfo info)
//	{
//        //--to do: wait
//        Vector3 guardPos = stream.Read<Vector3>();
//        //int playID = stream.Read<int>();
//        //if (playID == PlayerFactory.mMainPlayer.OwnerView.viewID.id)
//        //{
//            CSCreator creator = CSMain.GetCreator(CSConst.ciDefMgCamp);
//            if (creator != null)
//            {
//                CSPersonnel[] personnels = creator.GetNpcs();
//                foreach (CSPersonnel csp in personnels)
//                {
//                    if (csp != null && csp.m_Npc != null)
//                    {
//                        if (Id == csp.m_Npc.Id)
//                        {
//                            csp.SetGuardAttr(guardPos);
//                        }
//                    }
//                }
//            }
//        //}
//	}

	void RPC_S2C_CLN_PlantGetBack(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int objId = stream.Read<int> ();
        if (objId != -1)
        {
            FarmManager.Instance.RemovePlant(objId);
            DragArticleAgent.Destory(objId);
            ItemMgr.Instance.DestroyItem(objId);
        }
        else
        {
            FarmPlantLogic plant = stream.Read<FarmPlantLogic>();

            CSMgCreator creator = MultiColonyManager.GetCreator(TeamId);
            if (creator == null || creator.Assembly == null)
            {
                return;
            }
            CSFarm farm = creator.Assembly.Farm;
            if (farm == null)
                return;
            farm.RestoreWateringPlant(plant);
        }
	}

	void RPC_S2C_CLN_RemoveNpc(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
        //int playID = stream.Read<int>();
        //if(playID == PlayerFactory.mMainPlayer.OwnerView.viewID.id)
        //{
        //}

        CSCreator mCreator = MultiColonyManager.GetCreator(TeamId);
        PeEntity npcEntity = EntityMgr.Instance.Get(Id);
        mCreator.RemoveNpc(npcEntity);
        npcEntity.Dismiss();
	}
	
	void RPC_S2C_CLN_PlantPutOut(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		Vector3 pos = stream.Read<Vector3>();
		Quaternion rot = stream.Read<Quaternion>();
		int objID = stream.Read<int>();
		/*byte type = */stream.Read<byte>();
		
        //FarmPlantLogic mPlant = FarmManager.Instance.GetPlantByItemObjID (objID);
        //if(null == mPlant)
        //{
        //    mPlant = FarmManager.Instance.CreatePlant(objID, PlantInfo.GetPlantInfoByItemId(m_ItemID).mTypeID, pos);
        //    mPlant.mTerrianType = type;
        //    mPlant.UpdateGrowRate(0);
        //}
        //mPlant.mPos = pos;

        //DragArticleAgent item = DragArticleAgent.PutItemByProroId(objID, pos, transform.rotation);

        ItemObject itemobj = ItemMgr.Instance.Get(objID);

        DragArticleAgent dragItem = DragArticleAgent.Create(itemobj.GetCmpt<Drag>(), pos, Vector3.one, rot, objID);

        FarmPlantLogic plant = dragItem.itemLogic as FarmPlantLogic;
        plant.InitInMultiMode();
        stream.Read<FarmPlantLogic>();
        plant.UpdateInMultiMode();

	}

	void RPC_S2C_CLN_PlantUpdateInfo(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int objId = stream.Read<int> ();
		double life = stream.Read<double> ();
		double water = stream.Read<double> ();
		double clean = stream.Read<double> ();
		FarmPlantLogic plant = FarmManager.Instance.GetPlantByItemObjID (objId);
		if (plant != null)
		{
			plant.mLife = life;
			plant.mWater = water;
			plant.mClean = clean;
		}
	}

	void RPC_S2C_CLN_PlantClear(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int objId = stream.Read<int> ();
		FarmPlantLogic plant = FarmManager.Instance.GetPlantByItemObjID (objId);
		if (plant != null)
		{
			FarmManager.Instance.RemovePlant(objId);
            DragArticleAgent.Destory(objId);
			ItemMgr.Instance.DestroyItem(objId);
		}
	}

    void RPC_S2C_CLN_PlantWater(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        /*int farmId = */stream.Read<int>();
        FarmPlantLogic plant = stream.Read<FarmPlantLogic>();

        CSMgCreator creator = MultiColonyManager.GetCreator(TeamId);
        if (creator == null || creator.Assembly == null)
        {
            return;
        }
        CSFarm farm = creator.Assembly.Farm;
        farm.RestoreWateringPlant(plant);
    }
    void RPC_S2C_CLN_PlantClean(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        /*int farmId = */stream.Read<int>();
        FarmPlantLogic plant = stream.Read<FarmPlantLogic>();

        CSMgCreator creator = MultiColonyManager.GetCreator(TeamId);
        if (creator == null || creator.Assembly == null)
        {
            return;
        }
        CSFarm farm = creator.Assembly.Farm;
        farm.RestoreCleaningPlant(plant);
    }

    void RPC_S2C_CLN_SetProcessingIndex(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        int index = stream.Read<int>();
        CSMgCreator creator = MultiColonyManager.GetCreator(TeamId,false);

        if (creator != null)
        {
            CSPersonnel[] personnels = creator.GetNpcs();
            foreach (CSPersonnel csp in personnels)
            {
                if (csp != null && csp.m_Npc != null)
                {
                    if (Id == csp.m_Npc.Id)
                    {
                        csp.ProcessingIndex = index;
                    }
                }
            }
        }
    }

    void RPC_S2C_CLN_SetIsProcessing(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        bool isProcessing = stream.Read<bool>();
        CSMgCreator creator = MultiColonyManager.GetCreator(TeamId, false);

        if (creator != null)
        {
            CSPersonnel[] personnels = creator.GetNpcs();
            foreach (CSPersonnel csp in personnels)
            {
                if (csp != null && csp.m_Npc != null)
                {
                    if (Id == csp.m_Npc.Id)
                    {
                        csp.IsProcessing = isProcessing;
                    }
                }
            }
        }
    }
}

