using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ItemAsset;
using System.IO;
using Pathea;
using Pathea.PeEntityExt;
using Pathea.PeEntityExtTrans;
using Pathea.PeEntityExtMotion_Move;
using Pathea.PeEntityExtNpcPackage;
using ItemAsset.PackageHelper;
using Pathea.PeEntityExtPlayerPackage;
using System;
using WhiteCat.UnityExtension;

public class stShopData
{
	public int ItemObjID;
	public double CreateTime;
	public stShopData(){}
	public stShopData(int itemObjId,double createTime){
		ItemObjID = itemObjId;
		CreateTime = createTime;
	}
}

public class stShopInfo
{
    public Dictionary<int, stShopData> ShopList;

    public stShopInfo()
    {
        ShopList = new Dictionary<int, stShopData>();
    }
}

public class PassengerInfo 
{
    public enum Course
    {
        before,
        on,
        latter
    }

    public int npcID;
    public int startRouteID, startIndexID;
    public int endRouteID, endIndexID;
    public Vector3 dest;
    public Course type = Course.before;

    public byte[] Export()
    {
        return PETools.Serialize.Export((w) =>
        {
            w.Write(npcID);

            w.Write(startRouteID);
            w.Write(startIndexID);

            w.Write(endRouteID);
            w.Write(endIndexID);

            w.Write(dest.x);
            w.Write(dest.y);
            w.Write(dest.z);

            w.Write((int)type);
        });
    }

    public void Import(byte[] data)
    {
        PETools.Serialize.Import(data, (r) =>
        {
            npcID = r.ReadInt32();

            startRouteID = r.ReadInt32();
            startIndexID = r.ReadInt32();
            endRouteID = r.ReadInt32();
            endIndexID = r.ReadInt32();

            dest = new Vector3(r.ReadSingle(),r.ReadSingle(),r.ReadSingle());

            type = (Course)r.ReadInt32();

            //if (type == Course.before)
            //{
            //    StroyManager.Instance.MoveTo(EntityMgr.Instance.Get(npcID), start.position, 4, true, SpeedState.Run);
            //    StroyManager.Instance.StartCoroutine(StroyManager.Instance.WaitingNpcRailStart(start, EntityMgr.Instance.Get(npcID), end, dest));
            //}
            //else if (type == Course.on)
            //{
            //    StroyManager.Instance.StartCoroutine(StroyManager.Instance.WaitingNpcRailEnd(end, EntityMgr.Instance.Get(npcID), dest));
            //}
            //else if (type == Course.latter)
            //{
                
            //}
        });
    }
}

public class StroyManager : MonoBehaviour
{
    public const int SpeMissionID = -10000;
    Dictionary<Vector2, List<Vector3>> m_CreatedNpcList;
    public struct PathInfo
    {
        public Vector3 pos;
        public bool isFinish;
    }

    public class RandScenarios 
    {
        public int id;
        public float curTime;
        public List<int> scenarioIds;
        public float cd;
    }

    public class CurPathInfo
    {
        public int idx;
        public Vector3 curPos;

        public CurPathInfo()
        {
            curPos = Vector3.zero;
        }
    }

    public static Dictionary<int, PassengerInfo> m_Passengers = new Dictionary<int, PassengerInfo>();
    public Dictionary<Vector3, TentScript> m_TentList = new Dictionary<Vector3, TentScript>();
    public Dictionary<int, float> m_MisDelay = new Dictionary<int, float>();
    public Dictionary<int, bool> m_StoryList;
    public List<int> m_AdStoryList;
    public Dictionary<int, int> m_AdStoryListNpc = new Dictionary<int, int>();
    public List<string> m_DriveNpc = new List<string>();
    public List<int> m_iDriveNpc = new List<int>();
    private int m_CurMissionID = 0;
    private Dictionary<int, CurPathInfo> m_iCurPathMap;
    //private int m_CurPathIdx = 0;
    public bool npcLoaded = false;

    public int maxCount;
    public static int m_SpeAdNpcId = -1;
    Transform mFollowCameraTarget;
    public CamMode mCamMode;
    //bool m_bCamModing = false;
    //bool m_bCamMoveModing = false;
    //bool m_bCamRotModing = false;
    int m_CamIdx = 0;
    List<int> delayPlotID = new List<int>();

    public List<int> m_NeedCampTalk = new List<int>();
    PeTrans m_PlayerTrans = null;
    public PeTrans PlayerTrans
    {
        get { return m_PlayerTrans; }
    }

    private List<Bounds> colliderBoundList;

    public List<Bounds> ColliderBoundList
    {
        get { return colliderBoundList; }
    }

    static StroyManager mInstance;
    public static StroyManager Instance
    {
        get
        {
            return mInstance;
        }
    }

    public Dictionary<int, stShopInfo> m_BuyInfo = new Dictionary<int, stShopInfo>();
    public Dictionary<int, List<ItemObject>> m_SellInfo = new Dictionary<int, List<ItemObject>>();
    public Dictionary<int, KillMons> m_RecordKillMons = new Dictionary<int, KillMons>();
    public List<MoveMons> m_RecordMoveMons = new List<MoveMons>();
    public Dictionary<int, Dictionary<PeEntity, bool>> m_RecordIsReachPoitn = new Dictionary<int, Dictionary<PeEntity, bool>>();

    void Start() 
    {
        StartCoroutine(WaitNpcRail(StroyManager.m_Passengers));
    }

    IEnumerator WaitNpcRail(Dictionary<int,PassengerInfo> tmp)
    {
        while (StroyManager.Instance == null || Railway.Manager.Instance == null)
        {
            yield return new WaitForSeconds(0.1f);
        }
        foreach (var item in tmp.Values)
        {
			if(item == null)
				continue;

            Railway.Route route = Railway.Manager.Instance.GetRoute(item.startRouteID);
            if (route == null)
                continue;
            Railway.Point start = route.GetPointByIndex(item.startIndexID);
			if(null == start)
				continue;

            route = Railway.Manager.Instance.GetRoute(item.endRouteID);
            if (route == null)
                continue;
            Railway.Point end = route.GetPointByIndex(item.endIndexID);
			if(null == end)
				continue;
            if (item.type == PassengerInfo.Course.before)
                StroyManager.Instance.StartCoroutine(WaitingNpcRailStart(start, EntityMgr.Instance.Get(item.npcID), end, item.dest));
            else if (item.type == PassengerInfo.Course.on)
                StroyManager.Instance.StartCoroutine(WaitingNpcRailEnd(end, EntityMgr.Instance.Get(item.npcID), item.dest));
        }
        //int npcid, int startRouteID, int startIndexID, int endRouteID, int endIndexID, Vector3 dest, int type
        yield break;
    }

    void Awake()
    {
        mInstance = this;
        m_CreatedNpcList = new Dictionary<Vector2, List<Vector3>>();
        m_StoryList = new Dictionary<int, bool>();
        m_AdStoryList = new List<int>();
        m_iCurPathMap = new Dictionary<int, CurPathInfo>();
        colliderBoundList = new List<Bounds>();

        GameObject obj = new GameObject("mission camera follow");
        obj.transform.parent = this.transform;
        mFollowCameraTarget = obj.transform;

        if (GameConfig.IsMultiMode)
            mInstance = this;
    }

    #region ShopInfo
    public static int PRICE_ID = 229;
    private int mMoney;
    public int m_Money
    {
        get
        {
            if (PeGameMgr.IsMultiAdventure)
                return mMoney;
            else
            //                return PeCreature.Instance.mainPlayer.GetPkgItemCount(PRICE_ID);
            {
                PackageCmpt cmpt = PeCreature.Instance.mainPlayer.GetCmpt<PackageCmpt>();
                return cmpt.money.current;
            }
        }

    }

    public void InitBuyInfo(StoreData npc, int npcid)
    {
        m_BuyInfo[npcid] = new stShopInfo();

        List<int> shopList = npc.itemList;
        for (int i = 0; i < shopList.Count; i++)
        {
            ShopData data = ShopRespository.GetShopData(shopList[i]);
            ItemObject itemObj = ItemAsset.ItemMgr.Instance.CreateItem(data.m_ItemID); // single
            itemObj.stackCount = data.m_LimitNum;
            //itemObj.RemoveProperty(ItemProperty.NewFlagTime);
            //if (GameConfig.IsMultiMode)
            //{
            //    RPC("RPC_C2S_RemoveNewFlag", itemObj.mObjectID);
            //}
			stShopData ssd = new stShopData(itemObj.instanceId,GameTime.Timer.Second);
            m_BuyInfo[npcid].ShopList[data.m_ID] = ssd;
        }
    }

    public bool BuyItem(ItemObject itemObj, int num, int shopID, int npcId, bool bReduce)
    {
        ShopData data = null;
        int price;
        if (bReduce)
        {
            data = ShopRespository.GetShopData(shopID);
            if (data == null)
            {
                PeTipMsg.Register(PELocalization.GetString(8000496), PeTipMsg.EMsgLevel.Warning);
                return false;
            }

            price = data.m_Price;
        }
        else
        {
            price = itemObj.GetSellPrice();
        }

        if (m_Money < price * num)
        {
            PeTipMsg.Register(PELocalization.GetString(Money.Digital? 8000092:8000073), PeTipMsg.EMsgLevel.Warning);
            return false;
        }

        if (null != itemObj)
        {
            ItemObject addItem = null;

            if (itemObj.protoData.maxStackNum == 1)
            {
                int NUM = num;
                for (int i = 0; i < NUM; i++)
                {

                    num = 1;


                    if (num < itemObj.GetCount())
                    {
                        addItem = ItemMgr.Instance.CreateItem(itemObj.protoId); // single
                        addItem.IncreaseStackCount(num - 1);
                    }
                    else
                    {
                        addItem = itemObj;
                        //lz-2016.10.09 会出现字典不包含npcID的情况
                        if (!m_BuyInfo.ContainsKey(npcId))
                        {
                            PeTipMsg.Register(PELocalization.GetString(8000496), PeTipMsg.EMsgLevel.Warning);
                            return false;
                        }
                        if (m_BuyInfo[npcId].ShopList.ContainsKey(shopID))
                        {
                            m_BuyInfo[npcId].ShopList[shopID].ItemObjID = 0;
                            m_BuyInfo[npcId].ShopList[shopID].CreateTime = GameTime.Timer.Second;
                        }
                    }

                    PlayerPackageCmpt pkg = PeCreature.Instance.mainPlayer.GetCmpt<PlayerPackageCmpt>();
                    if (null == pkg)
                    {
                        PeTipMsg.Register(PELocalization.GetString(8000496), PeTipMsg.EMsgLevel.Warning);
                        return false;
                    }


                    if (ItemPackage.InvalidIndex != pkg.package.AddItem(addItem))// Remove meeat only when additem success
                        ReduceMoney(price * num);
                    else
                    {
                        PeTipMsg.Register(PELocalization.GetString(9500312), PeTipMsg.EMsgLevel.Warning);
                        return false;
                    }

                    GameUI.Instance.mItemPackageCtrl.ResetItem();
                }
            }
            else
            {
                if (num < itemObj.GetCount())
                {
                    addItem = ItemMgr.Instance.CreateItem(itemObj.protoId); // single
                    addItem.IncreaseStackCount(num - 1);
                }
                else
                {
                    addItem = itemObj;
                    if (!m_BuyInfo.ContainsKey(npcId))
                    {
                        PeTipMsg.Register(PELocalization.GetString(8000496), PeTipMsg.EMsgLevel.Warning);
                        return false;
                    }
                    //lz-2016.10.09 会出现字典不包含npcID的情况
                    if (m_BuyInfo[npcId].ShopList.ContainsKey(shopID))
                    {
                        m_BuyInfo[npcId].ShopList[shopID].ItemObjID = 0;
                        m_BuyInfo[npcId].ShopList[shopID].CreateTime = GameTime.Timer.Second;
                    }
                }

                PlayerPackageCmpt pkg = PeCreature.Instance.mainPlayer.GetCmpt<PlayerPackageCmpt>();
                if (null == pkg)
                {
                    PeTipMsg.Register(PELocalization.GetString(8000496), PeTipMsg.EMsgLevel.Warning);
                    return false;
                }


				if (pkg.package.CanAdd(addItem.protoId,addItem.GetCount())){// Remove meeat only when additem success
					pkg.package.Add(addItem.protoId,addItem.GetCount());
					ReduceMoney(price * num);
				}
				else
				{
					PeTipMsg.Register(PELocalization.GetString(9500312), PeTipMsg.EMsgLevel.Warning);
					return false;
				}

                GameUI.Instance.mItemPackageCtrl.ResetItem();
            }

        }

        return true;
    }

    public bool AddMoney(int num)
    {
        if (PeGameMgr.IsMultiAdventure)
            mMoney += num;
        else
            return PeCreature.Instance.mainPlayer.AddToPkg(PRICE_ID, num);

        return true;
    }

    public void ReduceMoney(int num)
    {
        if (PeGameMgr.IsMultiAdventure)
            mMoney -= num;
        else
        //            PeCreature.Instance.mainPlayer.RemoveFromPkg(PRICE_ID, num);
        {
            PlayerPackageCmpt pkg = PeCreature.Instance.mainPlayer.GetCmpt<PlayerPackageCmpt>();
            if (pkg != null)
                pkg.money.current -= num;
        }
    }
    #endregion


    public void InitMission(int npcId = -1)
    {
        PeEntity npc;

        foreach (KeyValuePair<int, MissionInit> ite in MisInitRepository.m_MisInitMap)
        {
            MissionInit data = ite.Value;
            if (data == null)
                continue;
			if(PeGameMgr.IsMulti && data.m_NpcAct.Count > 0 && npcId == -1)
				continue;
            bool haveNpc = false;
            for (int i = 0; i < data.m_NpcAct.Count; i++)
            {
                if (npcId == data.m_NpcAct[i].npcid && npcId != -1)
                {
                    haveNpc = true;
                    break;
                }
            }
            if (!haveNpc && npcId != -1)
                continue;
            bool bpass = true;

            for (int i = 0; i < data.m_ComMisID.Count; i++)
            {
                if (data.m_ComMisID[i] > 999 && data.m_ComMisID[i] < 10000)
                {
                    if (!MissionManager.Instance.HadCompleteTarget(data.m_ComMisID[i]))
                    {
                        bpass = false;
                        break;
                    }
                }
                else if (data.m_ComMisID[i] > 0)
                {

                    if (!MissionManager.Instance.HadCompleteMission(data.m_ComMisID[i]))
                    {
                        bpass = false;
                        break;
                    }
                }
            }

            if (!bpass)
                continue;

            if (data.m_NComMisID > 999 && data.m_NComMisID < 10000)
            {
                if (MissionManager.Instance.HadCompleteTarget(data.m_NComMisID))
                    continue;
            }
            else if (data.m_NComMisID > 0)
            {
                if (MissionManager.Instance.HadCompleteMission(data.m_NComMisID))
                    continue;
            }

            //npc animation
            for (int i = 0; i < data.m_NpcAct.Count; i++)
            {
                NpcAct na = data.m_NpcAct[i];

                npc = EntityMgr.Instance.Get(na.npcid);
                if (npc == null)
                    continue;

                if (na.animation == "SpiderWeb")
                    npc.TrapInSpiderWeb(na.btrue, 1f);
                else if (na.animation == "AddItem")
                    AddNpcItem(npc.Id, 90002421);
                else if (na.animation == "CarryUp")
                {
                    CarryUp(npc, 9008, true);
                    //PeEntity gerdy = EntityMgr.Instance.Get(9008);
                    //npc.CarryUp(gerdy);
                }
                else if (na.animation == "PutDown")
                {
                    PeEntity gerdy = EntityMgr.Instance.Get(9008);
                    MotionMgrCmpt mmc = gerdy.GetCmpt<MotionMgrCmpt>();
                    if (mmc == null)
                        return;

                    mmc.FreezePhyState(GetType(), true);
                    SetIdle(gerdy, "InjuredRest");
                }
                else if (na.animation == "Cure")
                {
                    //PeEntity gerdy = EntityMgr.Instance.Get(9008);
                    RemoveReq(npc, EReqType.Idle);
                    //SetIdle(gerdy, "InjuredRest");
                    //gerdy.Cure(na.btrue);
                    MotionMgrCmpt mmc = npc.GetCmpt<MotionMgrCmpt>();
                    if (mmc == null)
                        return;

                    mmc.FreezePhyState(GetType(), false);
                }
                else if (na.animation == "InjuredSit")
                {
                    //(npc.NpcCmpt.Req_GetRequest(EReqType.Idle) as RQIdle).state == "BeCarry"
                    if (!(npc.Id == 9008 &&
                        npc.NpcCmpt.Req_GetRequest(EReqType.Idle) != null &&
                        (npc.NpcCmpt.Req_GetRequest(EReqType.Idle) as RQIdle).state == "BeCarry"))
                        SetIdle(npc, na.animation);
                } else if(na.animation == "Lie")
                {
                    SetIdle(npc, na.animation);
                }
                else if (na.animation == "npcidle")
                    SetIdle(npc, "Idle");
                else if (na.animation == "npcdidle")
                    RemoveReq(npc, EReqType.Idle);
                else if (na.animation != "InjuredLevel")
                    npc.CmdPlayAnimation(na.animation, na.btrue);
            }

            if (npcId != -1)
                return;

            if (data.m_triggerPlot.Count > 0)
                StroyManager.Instance.PushStoryList(data.m_triggerPlot);

            //npc face
            for (int i = 0; i < data.m_NpcFace.Count; i++)
            {
                NpcFace nf = data.m_NpcFace[i];

                npc = EntityMgr.Instance.Get(nf.npcid);
                if (npc == null)
                    continue;

                if (nf.angle == -2)
                    npc.CmdFaceToPoint(PeCreature.Instance.mainPlayer.ExtGetPos());
                else if (nf.angle != -1)
                    SetRotation(npc, Quaternion.AngleAxis(nf.angle, Vector3.up));
                else
                {
                    PeEntity npcother = EntityMgr.Instance.Get(nf.otherid);
                    if (npcother != null)
                        npc.CmdFaceToPoint(npcother.ExtGetPos());
                }

                if (!nf.bmove)
                    npc.CmdStartIdle();
            }

            //npc camp
            for (int i = 0; i < data.m_NpcCamp.Count; i++)
            {
                NpcCamp nc = data.m_NpcCamp[i];

                npc = EntityMgr.Instance.Get(nc.npcid);

                if (npc == null)
                    continue;

                npc.SetCamp(nc.camp);
            }

            //delete monster
            for (int i = 0; i < data.m_DeleteMonster.Count; i++)
            {
				SceneEntityCreatorArchiver.Instance.SetFixedSpawnPointActive(data.m_DeleteMonster[i], false);
            }

            for (int i = 0; i < data.m_MonsterCampList.Count; i++)
            {
                AiAsset.AiDataBlock.SetCamp(data.m_MonsterCampList[i].id, data.m_MonsterCampList[i].value);
                //AiManager.Manager.SetCampFromDataID(data.m_MonsterCampList[i].id, data.m_MonsterCampList[i].value);
            }

            for (int i = 0; i < data.m_MonsterHarmList.Count; i++)
            {
                AiAsset.AiDataBlock.SetHarm(data.m_MonsterHarmList[i].id, data.m_MonsterHarmList[i].value);
                //AiManager.Manager.SetHarmFromDataID(data.m_MonsterHarmList[i].id, data.m_MonsterHarmList[i].value);
            }

            //delete npc
            for (int i = 0; i < data.m_iDeleteNpc.Count; i++)
            {
                npc = EntityMgr.Instance.Get(data.m_iDeleteNpc[i]);

                if (npc == null)
                    continue;

                //NpcManager.Instance.DestroyNpc(npc.Id);
            }

            for (int i = 0; i < data.m_TransNpc.Count; i++)
            {
                NpcStyle ns = data.m_TransNpc[i];
                if (ns == null)
                    continue;

                npc = EntityMgr.Instance.Get(ns.npcid);

                if (npc == null)
                    continue;
                if (PeGameMgr.IsMultiStory)
                {
                    if (npc.Id == 9033)
                    {
                        float d = Vector3.Distance(ns.pos, new Vector3(9856f, 251.5f, 9575f));
                        if (d < 0.5f)
                            ns.pos = new Vector3(9876f, 251.5f, 9597f);
                    }

                }
                StroyManager.Instance.Translate(npc, ns.pos);

                NpcCmpt nc = npc.NpcCmpt;
                if (null != nc)
                    nc.FixedPointPos = ns.pos;
                else
                    Debug.LogError("Failed to set fixed point.");
            }

            for (int i = 0; i < data.m_FollowPlayerList.Count; i++)
            {
                NpcOpen no = data.m_FollowPlayerList[i];

                npc = EntityMgr.Instance.Get(no.npcid);
                if (npc == null)
                    continue;
                NpcCmpt nc = npc.GetComponent<NpcCmpt>();
                if (nc == null)
                    continue;


                if (no.bopen)
                {
                    if(PeGameMgr.IsSingle)
                    {
                        ServantLeaderCmpt leader = PeCreature.Instance.mainPlayer.GetCmpt<ServantLeaderCmpt>();
                        leader.AddForcedServant(nc, true);
                    }                    
                }
                else
                {
                    ServantLeaderCmpt leader = PeCreature.Instance.mainPlayer.GetCmpt<ServantLeaderCmpt>();
                    leader.RemoveForcedServant(nc);
                }
            }

            for (int i = 0; i < data.m_iColonyNoOrderNpcList.Count; i++)
            {
                npc = EntityMgr.Instance.Get(data.m_iColonyNoOrderNpcList[i]);
                if (npc == null)
                    continue;
                npc.NpcCmpt.FixedPointPos = npc.position;
                npc.NpcCmpt.BaseNpcOutMission = true;
            }

            ProcessSpecial(data.m_Special);

            if (data.m_plotMissionTrigger.Count > 0)
                StartCoroutine(WaitPlotMissionTrigger(data.m_plotMissionTrigger));

            int excuteNum = data.m_monsterHatredList.Count / 5;
            List<int> singleHandle = new List<int>();
            for (int i = 0; i < excuteNum; i++)
            {
                singleHandle = data.m_monsterHatredList.GetRange((5 * i), 5);
                SpecialHatred.MonsterHatredAdd(singleHandle);
            }

            excuteNum = data.m_npcHatredList.Count / 4;
            for (int i = 0; i < excuteNum; i++)
            {
                singleHandle = data.m_npcHatredList.GetRange((4 * i), 4);
                SpecialHatred.NpcHatredAdd(singleHandle);
            }

            excuteNum = data.m_harmList.Count / 3;
            for (int i = 0; i < excuteNum; i++)
            {
                singleHandle = data.m_harmList.GetRange((3 * i), 3);
                SpecialHatred.HarmAdd(singleHandle);
            }

            excuteNum = data.m_doodadHarmList.Count / 3;
            for (int i = 0; i < excuteNum; i++)
            {
                singleHandle = data.m_doodadHarmList.GetRange((3 * i), 3);
                PeEntity[] doodads;
                if (singleHandle[1] != 0)
                    doodads = EntityMgr.Instance.GetDoodadEntities(singleHandle[1]);
                else
                {
                    doodads = EntityMgr.Instance.GetDoodadEntitiesByProtoId(singleHandle[2]);
                }
                for (int j = 0; j < doodads.Length; j++)
                {
                    if (doodads[j].GetCmpt<SceneDoodadLodCmpt>() == null)
                        continue;
                    if (singleHandle[0] == 0)
                        doodads[j].GetCmpt<SceneDoodadLodCmpt>().IsDamagable = true;
                    else
                        doodads[j].GetCmpt<SceneDoodadLodCmpt>().IsDamagable = false;
                }
            }

            foreach (var item in data.m_doodadEffectList)
            {
                PeEntity[] doodads = EntityMgr.Instance.GetDoodadEntities(item.id);
                if (doodads == null || doodads.Length == 0)
                    break;
                doodads[0].transform.TraverseHierarchy(
                (trans, d) =>
                {
                    if (item.names.Contains(trans.gameObject.name))
                        trans.gameObject.SetActive(item.openOrClose);
                });
            }

            for (int i = 0; i < data.cantReviveNpc.Count; i++)
            {
                //int npcTypeID;
                //if (NpcMissionDataRepository.dicMissionData.ContainsKey(data.cantReviveNpc[i]))
                //    npcTypeID = NpcMissionDataRepository.dicMissionData[data.cantReviveNpc[i]].m_Rnpc_ID;
                //else
                //    continue;
                //RandomNpcDb.Item n = Pathea.RandomNpcDb.Get(npcTypeID);
                //if(n != null)
                //    n.reviveTime = -1;

                npc = EntityMgr.Instance.Get(data.cantReviveNpc[i]);
                if (npc != null)
                {
                    if (npc.NpcCmpt != null)
                        npc.NpcCmpt.ReviveTime = -1;
                }
            }

            foreach (var item in data.m_npcsBattle)
            {
                ENpcBattle type = (ENpcBattle)(item.type - 1);
                foreach (var item1 in item.npcId)
                {
                    npc = EntityMgr.Instance.Get(item1);
                    if (npc == null)
                        continue;
                    npc.GetComponent<NpcCmpt>().Battle = type;
                }
            }


            List<PeEntity> monsters = new List<PeEntity>(EntityMgr.Instance.All);
            foreach (var item in data.m_killMons)
            {
                if (m_RecordKillMons.Count == 0)
                    MonsterEntityCreator.commonCreateEvent += KillMonster;
                if (!m_RecordKillMons.ContainsKey(item.id))
                    m_RecordKillMons.Add(item.id, item);
                if (item.type == KillMons.Type.protoTypeId)
                {
                    monsters = monsters.FindAll(delegate(PeEntity mon)
                    {
                        if (mon == null)
                            return false;
                        if (mon.proto == EEntityProto.Monster && Vector3.Distance(mon.position, item.center) <= item.radius
                                && (item.monId == -999 ? true : mon.entityProto.protoId == item.monId))
                            return true;
                        return false;
                    });
                }
                else if (item.type == KillMons.Type.fixedId)
                {
                    PeEntity fixMon = SceneEntityCreatorArchiver.Instance.GetEntityByFixedSpId(item.monId);
                    if (fixMon != null)
                        monsters.Add(fixMon);
                }

                foreach (var mon in monsters)
                {
                    if(mon != null && mon.GetComponent<PESkEntity>() != null)
                        mon.GetComponent<PESkEntity>().SetAttribute(AttribType.Hp, 0, false);
                }
            }

            if (data.m_npcType.Count > 0)
            {
                NpcCmpt nc;
                foreach (var item in data.m_npcType)
                {
                    item.npcs.ForEach(delegate(int n)
                    {
                        npc = EntityMgr.Instance.Get(n);
                        if (npc != null && (nc = npc.GetComponent<NpcCmpt>()))
                            nc.NpcControlCmdId = item.type;
                    });
                }
            }

            excuteNum = data.m_killNpcList.Count / 3;
            singleHandle = new List<int>();
            //List<PeEntity> tmp1 = new List<PeEntity>();
            for (int i = 0; i < excuteNum; i++)
            {
                singleHandle = data.m_killNpcList.GetRange((3 * i), 3);
                KillNPC.NPCBeKilled(singleHandle[0]);
                //KillNPC.NPCaddItem(tmp1, singleHandle[1], singleHandle[2]);
            }

        }
    }

    public static void ClearRecord()
    {
        isPausing = false;
        randScenarios.Clear();
    }
    public static bool isPausing = false;
    static Dictionary<int, RandScenarios> randScenarios = new Dictionary<int, RandScenarios>();
    Material hidingMat;
    private Material HidingMat 
    {
        get 
        {
            if (hidingMat == null)
                hidingMat = Resources.Load("Shaders/HidingWaveMat", typeof(Material)) as Material;
            return hidingMat;
        } 
    }

    void AddChangingMaterial(SkinnedMeshRenderer tmp)
    {
        if (tmp == null)
            return;
        changingAdd.Enqueue(tmp);
    }

    void AddChangingMaterial(Material[] tmp) 
    {
        foreach (var item in tmp)
        {
            if (item == null)
                continue;
            changingMinus.Enqueue(item);
        }
    }

    void UpdateChangingMaterial()
    {
        if (changingAdd != null && changingAdd.Count > 0)
        {
            float n;
            foreach (var item in changingAdd)
            {
                if (item == null)
                    continue;
                if (item.materials == null)
                    continue;
                for (int i = 0; i < item.materials.Length; i++)
                {
                    if (item.materials[i] == null)
                        continue;
                    n = item.materials[i].GetFloat("_shaderChange");
                    item.materials[i].SetFloat("_shaderChange", Mathf.Clamp(n + 0.005f, 0f, 1f));
                }
            }
            SkinnedMeshRenderer renderer = changingAdd.Peek();
			if (renderer != null && renderer.materials != null && record != null)
            {
                if (renderer.materials[0].GetFloat("_shaderChange") >= 1 - PETools.PEMath.Epsilon)
                {
                    SkinnedMeshRenderer smr = changingAdd.Dequeue();
                    if (smr != null)
                        smr.materials = record;
                }
            }
        }
        if (changingMinus != null && changingMinus.Count > 0)
        {
            float n;
            foreach (var item in changingMinus)
            {
                n = item.GetFloat("_shaderChange");
                item.SetFloat("_shaderChange", Mathf.Clamp(n - 0.005f, 0f, 1f));
            }
            Material mat = changingMinus.Peek();
            if (mat != null)
            {
                if (mat.GetFloat("_shaderChange") < PETools.PEMath.Epsilon)
                    changingMinus.Dequeue();
            }
        }
    }

    void UpdateAdvPlot()
    {
        if (m_AdStoryList.Count == 0)
            return;
        AdStoryData addata = StoryRepository.GetAdStroyData(m_AdStoryList[0]);
        if (addata == null)
            return;
        PeEntity npc;

        for (int i = 0; i < addata.m_creNPC.Count; i++)
        {
            Vector3 referToPos;
            switch (addata.m_creNPC[i].referToType)
            {
                case ReferToType.Player:
                    referToPos = PeCreature.Instance.mainPlayer.position;
                    break;
                case ReferToType.Town:
                    VArtifactUtil.GetTownPos(addata.m_creNPC[i].m_referToID, out referToPos);
                    break;
                case ReferToType.Npc:
                    referToPos = EntityMgr.Instance.Get(MissionManager.Instance.m_PlayerMission.adId_entityId[addata.m_creNPC[i].m_referToID]).position;
                    break;
                default:
                    referToPos = PeCreature.Instance.mainPlayer.position;
                    break;
            }
            if (referToPos == Vector3.zero)
                continue;
            Vector2 rand = UnityEngine.Random.insideUnitCircle.normalized * addata.m_creNPC[i].m_radius;
            Vector2 onCircle = new Vector2(referToPos.x + rand.x, referToPos.z + rand.y);
            Vector3 crePos = new Vector3(onCircle.x, VFDataRTGen.GetPosTop(new IntVector2((int)onCircle.x, (int)onCircle.y)), onCircle.y);
            npc = NpcEntityCreator.CreateNpc(addata.m_creNPC[i].m_NPCID, crePos);
            if (npc != null)
            {
                if (VFDataRTGen.IsTownAvailable((int)onCircle.x, (int)onCircle.y))
                    npc.NpcCmpt.FixedPointPos = new Vector3(onCircle.x, VFDataRTGen.GetPosHeightWithTown(new IntVector2((int)onCircle.x, (int)onCircle.y)), onCircle.y);
                else
                    npc.NpcCmpt.FixedPointPos = new Vector3(onCircle.x, VFDataRTGen.GetPosHeight(new IntVector2((int)onCircle.x, (int)onCircle.y)), onCircle.y);
            }
        }

        checkEnterArea.AddRange(addata.m_enterArea);

        for (int i = 0; i < addata.m_npcMove.Count; i++)
        {
            Vector3 referToPos;
            switch (addata.m_npcMove[i].referToType)
            {
                case ReferToType.Player:
                    referToPos = PeCreature.Instance.mainPlayer.position;
                    break;
                case ReferToType.Town:
                    VArtifactUtil.GetTownPos(addata.m_npcMove[i].m_referToID, out referToPos);
                    break;
                case ReferToType.Npc:
                    referToPos = EntityMgr.Instance.Get(MissionManager.Instance.m_PlayerMission.adId_entityId[addata.m_npcMove[i].m_referToID]).position;
                    break;
                default:
                    referToPos = PeCreature.Instance.mainPlayer.position;
                    break;
            }
            if (referToPos == Vector3.zero)
                continue;

            if (!MissionManager.Instance.m_PlayerMission.adId_entityId.ContainsKey(addata.m_npcMove[i].npcID))
                continue;
            npc = EntityMgr.Instance.Get(MissionManager.Instance.m_PlayerMission.adId_entityId[addata.m_npcMove[i].npcID]);
            if (npc == null)
                continue;
            
            MoveTo(npc, referToPos, addata.m_npcMove[i].m_radius, true, SpeedState.Run);
            npc.NpcCmpt.FixedPointPos = referToPos;
        }

        for (int i = 0; i < addata.m_getMissionID.Count; i++)
        {
            int missionid = addata.m_getMissionID[i];
            MissionCommonData missionData = MissionManager.GetMissionCommonData(missionid);
            if (missionData == null)
                continue;

            if (MissionRepository.HaveTalkOP(missionid))
            {
                GameUI.Instance.mNPCTalk.NormalOrSP(0);
                if (GameUI.Instance.mNPCTalk.isPlayingTalk == false)
                {
                    GameUI.Instance.mNPCTalk.UpdateNpcTalkInfo(missionid, 1);
                    GameUI.Instance.mNPCTalk.PreShow();
                }
                else
                    GameUI.Instance.mNPCTalk.AddNpcTalkInfo(missionid, 1);
            }
            else if(MissionManager.Instance.IsGetTakeMission(missionid))
            {
                PeEntity adnpc = EntityMgr.Instance.Get(missionData.m_iNpc);
                if(adnpc == null || adnpc.NpcCmpt == null)
                    adnpc = EntityMgr.Instance.Get(m_AdStoryListNpc[ m_AdStoryList[0]]);
                MissionManager.Instance.SetGetTakeMission(missionid, adnpc, MissionManager.TakeMissionType.TakeMissionType_Get);
            }
        }

        for (int i = 0; i < addata.m_comMissionID.Count; i++)
        {
            MissionCommonData missionData = MissionManager.GetMissionCommonData(addata.m_comMissionID[i]);
            if (missionData != null)
            {
                //PeEntity adnpc = EntityMgr.Instance.Get(missionData.m_iNpc);
                if(PeGameMgr.IsSingle)
                    MissionManager.Instance.CompleteMission(addata.m_comMissionID[i]);
                else
                    MissionManager.Instance.RequestCompleteMission(addata.m_comMissionID[i]);
            }
        }

        if (addata.m_showTip != 0)
        {
            new PeTipMsg(PELocalization.GetString(addata.m_showTip), PeTipMsg.EMsgLevel.Warning, PeTipMsg.EMsgType.Stroy);
        }

        m_AdStoryListNpc.Remove(m_AdStoryList[0]);
        m_AdStoryList.RemoveAt(0);
    }

    void OnDestroy()
    {
        MonsterEntityCreator.commonCreateEvent -= MonsterCreatePatrolSet;
    }

    Material[] record;  //记录变换成隐身材质之后，需要还原的material
    List<AdEnterArea> checkEnterArea = new List<AdEnterArea>();
    Queue<SkinnedMeshRenderer> changingAdd = new Queue<SkinnedMeshRenderer>();
    Queue<Material> changingMinus = new Queue<Material>();
    Vector3 gerdyPutDownPos = new Vector3(12246.42f, 193.1f, 6528.76f);
    void Update()
    {
        UpdateUIWindow();
        UpdateChangingMaterial();
        if (KillNPC.isHaveAsh_inScene())
            KillNPC.UpdateAshBox();

        if (!GameConfig.IsMultiMode)
            UpdateCamp();

        if (randScenarios.Count > 0) 
        {
            foreach (var item in randScenarios.Keys)
            {
                if (Time.time - randScenarios[item].curTime > randScenarios[item].cd)
                {
                    randScenarios[item].curTime = Time.time;
                    int r = UnityEngine.Random.Range(0, randScenarios[item].scenarioIds.Count);

                    GameUI.Instance.mNPCTalk.SpTalkSymbol(true);
                    UINPCTalk.NpcTalkInfo talkinfo = new UINPCTalk.NpcTalkInfo();
                    GameUI.Instance.mNPCTalk.GetTalkInfo(randScenarios[item].scenarioIds[r], ref talkinfo, null);
                    GameUI.Instance.mNPCTalk.ParseName(null, ref talkinfo);
                    GameUI.Instance.mNPCTalk.m_NpcTalkList.Add(talkinfo);

                    if (GameUI.Instance.mNPCTalk.isPlayingTalk == false)
                        GameUI.Instance.mNPCTalk.SPTalkClose();
                }
            }
        }
        if(PeGameMgr.IsMulti && PlayerNetwork._missionInited)
        {
            if (MissionManager.Instance.HadCompleteMission(18) && !MissionManager.Instance.HadCompleteMission(27))
            {
                PeEntity gerdy = EntityMgr.Instance.Get(9008);
                PeEntity allen = EntityMgr.Instance.Get(9009);
                if (gerdy != null)
                {
                    //gerdy.NpcCmpt.SetPause(true);
                    if (gerdy.peTrans.rotation.eulerAngles.y != 270)
                    {
                        gerdy.peTrans.rotation = Quaternion.Euler(0, 270, 0);
                        gerdy.netCmpt.network.rot = Quaternion.Euler(0, 270, 0);
                    }
                    if (Vector3.Distance(gerdy.peTrans.position, gerdyPutDownPos) > 0.5f)
                    {
                        gerdy.peTrans.position = gerdyPutDownPos;
                        gerdy.netCmpt.network._pos = gerdyPutDownPos;
                        gerdy.NpcCmpt.MountID = 0;
                        if (allen != null)
                            allen.NpcCmpt.MountID = 0;
                        //gerdy.NpcCmpt.Req_Translate(gerdyPutDownPos);
                    }
                    if (!MissionManager.Instance.HasMission(27))
                    {
                        gerdy.animCmpt.SetBool("BeCarry", false);
                        gerdy.animCmpt.SetBool("InjuredSit", false);
                        gerdy.animCmpt.SetBool("InjuredRest", true);
                    }
                        
                                           
                    
                    //MotionMgrCmpt mmc = gerdy.GetCmpt<MotionMgrCmpt>();
                    //if (mmc == null)
                    //    return;

                    //mmc.FreezePhyState(GetType(), true);
                    //SetLie(gerdy, "Lie");
                    //SetIdle(gerdy, "InjuredRest");
                }
            }
            if (MissionManager.Instance.HadCompleteMission(27) && !MissionManager.Instance.HadCompleteMission(61))
            {
                PeEntity gerdy = EntityMgr.Instance.Get(9008);
                if (gerdy != null)
                {
                    gerdy.animCmpt.SetBool("InjuredRest", false);                    
                }                    
            }
            //else
            //{
            //    PeEntity gerdy = EntityMgr.Instance.Get(9008);
            //    if (gerdy != null)
            //    {
            //        gerdy.NpcCmpt.SetPause(false);
            //    }
            //}
        }
        
        if (PeGameMgr.IsMulti && !PlayerNetwork._missionInited && PeCreature.Instance.mainPlayer == null)
            return;

        
        int count = checkEnterArea.Count;
        for (int i = count - 1; i >= 0; i--)
        {
            Vector3 referToPos;
            bool isbreak = false;
            switch (checkEnterArea[i].referToType)
            {
                case 1:
                    referToPos = EntityMgr.Instance.Get(checkEnterArea[i].m_referToID).position;
                    break;
                case 2:
                    VArtifactUtil.GetTownPos(checkEnterArea[i].m_referToID, out referToPos);
                    break;
                case 3:
                    if (!CSMain.GetAssemblyPos(out referToPos))
                        isbreak = true;
                    break;
                default:
                    referToPos = PeCreature.Instance.mainPlayer.position;
                    break;
            }
            if (isbreak)
                continue;
            if (referToPos == Vector3.zero)
            {
                checkEnterArea.RemoveAt(i);
                continue;
            }

            //lz-2016.11.28 空对象
            if (null != PeCreature.Instance && null != PeCreature.Instance.mainPlayer)
            {
                if (Vector3.Distance(PeCreature.Instance.mainPlayer.position, referToPos) < checkEnterArea[i].m_radius)
                {
                    PushAdStoryList(checkEnterArea[i].m_plotID);
                    checkEnterArea.RemoveAt(i);
                }
            }
            //todo:
        }

        UpdateAdvPlot();

        if (m_StoryList.Count == 0)
            return;

        int id = 0;
        //bool bInit = true;

        foreach (KeyValuePair<int, bool> ite in m_StoryList)
        {
            if (delayPlotID.Find(tmp => tmp == ite.Key) != 0)
                continue;
            id = ite.Key;
            //bInit = ite.Value;
            break;
        }

        StoryData data = StoryRepository.GetStroyData(id);
        if (data == null)
        {
            m_StoryList.Remove(id);
            return;
        }

        if (m_MisDelay.ContainsKey(id))
        {
            if (Time.time - m_MisDelay[id] < data.m_Delay)
            {
                if (!delayPlotID.Contains(id))
                    delayPlotID.Add(id);
                if (delayPlotID.Count == m_StoryList.Count)
                    delayPlotID.Clear();
                return;
            }
        }

        if (data.m_triggerPlot.Count > 0)
            StroyManager.Instance.PushStoryList(data.m_triggerPlot);

        //talk
        if (data.m_TalkList.Count > 0)
        {
            GameUI.Instance.mNPCTalk.UpdateNpcTalkInfo(data.m_TalkList);
            GameUI.Instance.mNPCTalk.PreShow();
        }

        //ScenarioSP
        if (data.m_ServantTalkList.Count > 0)
        {
            GameUI.Instance.mNPCTalk.SpTalkSymbol(true);

            GameUI.Instance.mNPCTalk.UpdateNpcTalkInfo(data.m_ServantTalkList, null, false);
            if (GameUI.Instance.mNPCTalk.isPlayingTalk == false)
                GameUI.Instance.mNPCTalk.SPTalkClose();
        }

        if (data.m_randScenarios.id != 0)
        {
            if (data.m_randScenarios.startOrClose)
            {
                RandScenarios rs = new RandScenarios();
                rs.id = data.m_randScenarios.id;
                rs.scenarioIds = data.m_randScenarios.scenarioIds;
                rs.cd = data.m_randScenarios.cd;
                rs.curTime = Time.time;

                if (!randScenarios.ContainsKey(rs.id))
                    randScenarios.Add(rs.id, rs);
            }
            else if (randScenarios.ContainsKey(data.m_randScenarios.id))
                randScenarios.Remove(data.m_randScenarios.id);
            if(PeGameMgr.IsMulti)
            {
                if(MissionManager.Instance.HadCompleteMission(69))
                {
                    if (randScenarios.ContainsKey(1))
                        randScenarios.Remove(1);
                }
                if (MissionManager.Instance.HadCompleteMission(127))
                {
                    if (randScenarios.ContainsKey(2))
                        randScenarios.Remove(2);
                }
            }
        }

        //create monster
        for (int i = 0; i < data.m_CreateMonster.Count; i++)
        {
            MissionIDNum min = data.m_CreateMonster[i];
            //AISpawnPoint.Activate(min.id, true);
            //AISpawnPoint.SpawnImmediately(min.id);
			SceneEntityCreatorArchiver.Instance.SetFixedSpawnPointActive(min.id, true);
            //SceneEntityCreatorArchiver.Instance.ReqReborn(min.id);
            MissionManager.Instance.RemoveTimerByID(min.id);
        }

        //delete monster
        for (int i = 0; i < data.m_DeleteMonster.Count; i++)
        {
            AISpawnPoint.Activate(data.m_DeleteMonster[i], false);
			SceneEntityCreatorArchiver.Instance.SetFixedSpawnPointActive(data.m_DeleteMonster[i], false);
        }

        for (int i = 0; i < data.m_MonsterCampList.Count; i++)
        {
            AiAsset.AiDataBlock.SetCamp(data.m_MonsterCampList[i].id, data.m_MonsterCampList[i].value);
            //AiManager.Manager.SetCampFromDataID(data.m_MonsterCampList[i].id, data.m_MonsterCampList[i].value);
        }

        for (int i = 0; i < data.m_MonsterHarmList.Count; i++)
        {
            AiAsset.AiDataBlock.SetHarm(data.m_MonsterHarmList[i].id, data.m_MonsterHarmList[i].value);
            //AiManager.Manager.SetHarmFromDataID(data.m_MonsterHarmList[i].id, data.m_MonsterHarmList[i].value);
        }

        PeEntity npc;
        for (int i = 0; i < data.m_MonAct.Count; i++)
        {
            MonAct ma = data.m_MonAct[i];
            PeEntity mon;
            for (int j = 0; j < ma.mons.Count; j++)
            {
                mon = SceneEntityCreatorArchiver.Instance.GetEntityByFixedSpId(ma.mons[j]);
                if (mon == null)
                    continue;
                RequestCmpt rc = mon.GetComponent<RequestCmpt>();
                if (rc == null)
                    continue;
                if (ma.btrue)
                    rc.Register(EReqType.Animation, ma.animation, ma.time);
                else
                    rc.RemoveRequest(EReqType.Animation);
            }
        }

        //npc animation
        for (int i = 0; i < data.m_NpcAct.Count; i++)
        {
            NpcAct na = data.m_NpcAct[i];

            npc = EntityMgr.Instance.Get(na.npcid);
            if (npc == null)
                continue;

            switch(na.animation)
            {
                case "AddItem": AddNpcItem(npc.Id, 90002421);break;
                case "CarryUp": CarryUp(npc, 9008, true);break;
                case "PutDown":
                    {
                        PeEntity gerdy = EntityMgr.Instance.Get(9008);
                        NpcCmpt npcCmpt = gerdy.NpcCmpt;
                        npcCmpt.Req_Remove(EReqType.Salvation);
                        if (PeGameMgr.IsMultiStory)
                        {
                            GameObject goSpe = GameObject.Find("scene_healing_tube(Clone)");
                            if (goSpe != null)
                                goSpe.GetComponent<Collider>().enabled = false;
                            if (npc != null && !npc.NpcCmpt.Net.hasOwnerAuth)
                                break;
                        }
                        CarryUp(npc, 9008, false);

                        BiologyViewCmpt view = gerdy.biologyViewCmpt;
                        if (null != view) view.ActivateInjured(false);

                        MotionMgrCmpt mmc = gerdy.motionMgr;
                        if (mmc == null)
                            break;

                        mmc.FreezePhyState(GetType(), true);
                        SetIdle(gerdy, "InjuredRest");
                    } break;
                case "Cure":
                    {
                        RemoveReq(npc, EReqType.Idle);
                        MotionMgrCmpt mmc = npc.motionMgr;
                        if (mmc == null)
                            break;

                        BiologyViewCmpt view = npc.biologyViewCmpt;
                        if (null != view) view.ActivateInjured(true);

                        mmc.FreezePhyState(GetType(), false);
                    }
                    break;
                case "InjuredSit":
                case "InjuredSitEX":
                case "Lie":
                    {
                        if (na.btrue)
                            SetIdle(npc, na.animation);
                        else
                            RemoveReq(npc, EReqType.Idle);
                    }
                    break;
                case "npcidle": SetIdle(npc, "Idle");break;
                case "npcdidle": RemoveReq(npc, EReqType.Idle);break;
                default:break;
            }
    //        if (na.animation == "AddItem")
    //            AddNpcItem(npc.Id, 90002421);
    //        else if (na.animation == "CarryUp")
    //            CarryUp(npc, 9008, true);
    //        else if (na.animation == "PutDown")
    //        {
				//PeEntity gerdy = EntityMgr.Instance.Get(9008);
				//NpcCmpt npcCmpt = gerdy.NpcCmpt;
    //            npcCmpt.Req_Remove(EReqType.Salvation);
				//if(PeGameMgr.IsMultiStory)
				//{
				//	GameObject goSpe = GameObject.Find("scene_healing_tube(Clone)");
				//	if (goSpe != null)
				//		goSpe.GetComponent<Collider>().enabled = false;
				//	if(npc != null && !npc.NpcCmpt.Net.hasOwnerAuth)
				//		break;
				//}
    //            CarryUp(npc, 9008, false);

				//BiologyViewCmpt view = gerdy.biologyViewCmpt;
    //            if (null != view) view.ActivateInjured(false);
                
    //            MotionMgrCmpt mmc = gerdy.motionMgr;
    //            if (mmc == null)
    //                break;

    //            mmc.FreezePhyState(GetType(), true);
    //            SetIdle(gerdy, "InjuredRest");
    //        }
    //        else if (na.animation == "Cure")
    //        {
    //            RemoveReq(npc, EReqType.Idle);
    //            MotionMgrCmpt mmc = npc.motionMgr;
    //            if (mmc == null)
    //                break;

				//BiologyViewCmpt view = npc.biologyViewCmpt;
    //            if (null != view) view.ActivateInjured(true);

    //            mmc.FreezePhyState(GetType(), false);
    //        }
    //        else if (na.animation == "InjuredSit" || na.animation == "InjuredSitEX" || na.animation == "Lie")
    //        {
    //            if (na.btrue)
    //                SetIdle(npc, na.animation);
    //            else
    //                RemoveReq(npc, EReqType.Idle);
    //        }
    //        else if (na.animation == "npcidle")
    //            SetIdle(npc, "Idle");
    //        else if (na.animation == "npcdidle")
    //            RemoveReq(npc, EReqType.Idle);
        }

        for (int i = 0; i < data.m_NpcReq.Count; i++)
        {
            npc = EntityMgr.Instance.Get(data.m_NpcReq[i].type);
            PEActionType type = (PEActionType)data.m_NpcReq[i].valve;
            //npc.NpcCmpt.Req_SetIdle(type.ToString());
            //npc.NpcCmpt.Req_Remove(EReqType.Idle);
            if(data.m_NpcReq[i].isEffect)
                npc.motionMgr.DoAction(type);
            else
                npc.motionMgr.EndAction(type);
        }

        foreach (var item in data.m_NpcAnimator)
        {
            npc = EntityMgr.Instance.Get(item.npcid);
            if (npc == null)
                continue;
            npc.NpcCmpt.Req_PlayAnimation(item.animation, 0f, item.btrue);
        }

        if (data.m_PlayerAni.Count == 2) 
        {
            AnimatorCmpt ac = MainPlayer.Instance.entity.GetCmpt<AnimatorCmpt>();
            if (ac != null) 
            {
                ac.SetBool(data.m_PlayerAni[0], (data.m_PlayerAni[1].Equals("1") ? true : false));
            }
        }

        foreach (var item in data.m_MotionStyle)
        {
            npc = EntityMgr.Instance.Get(item.id);
            if (npc == null)
                continue;
            NpcCmpt nc = npc.GetCmpt<NpcCmpt>();
            if (nc == null)
                continue;
            nc.MotionStyle = item.type;
        }

        for (int i = 0; i < data.m_TransNpc.Count; i++)
        {
            NpcStyle ns = data.m_TransNpc[i];
            if (ns == null)
                continue;

            npc = EntityMgr.Instance.Get(ns.npcid);
            if (npc == null)
                continue;

            if (PeGameMgr.IsMultiStory)
            {
                if (npc.Id == 9033)
                {
                    float d = Vector3.Distance(ns.pos, new Vector3(9856f, 251.5f, 9575f));
                    if (d < 0.5f)
                        ns.pos = new Vector3(9876f, 251.5f, 9597f);
                }

            }
            StroyManager.Instance.Translate(npc, ns.pos);

            NpcCmpt nc = npc.NpcCmpt;
            if (null != nc)
                nc.FixedPointPos = ns.pos;
            else
                Debug.LogError("Failed to set fixed point.");
        }

        for (int i = 0; i < data.m_FollowPlayerList.Count; i++)
        {
            NpcOpen no = data.m_FollowPlayerList[i];

            npc = EntityMgr.Instance.Get(no.npcid);
            if (npc == null)
                continue;
            NpcCmpt nc = npc.NpcCmpt;
            if (nc == null)
                continue;

            if (no.bopen)
            {
                ServantLeaderCmpt leader = PeCreature.Instance.mainPlayer.GetCmpt<ServantLeaderCmpt>();
                if (PeGameMgr.IsMultiStory)
                {
                    if (nc.Net.hasOwnerAuth)
                        nc.Net.RPCServer(EPacketType.PT_NPC_ForcedServant, true);
                    continue;
                }
                else
                    leader.AddForcedServant(nc);
                //npc.GetComponent<NpcCmpt>().Req_FollowTarget(PeCreature.Instance.mainPlayer.GetComponent<PeTrans>().trans);
            }
            else
            {
                ServantLeaderCmpt leader = PeCreature.Instance.mainPlayer.GetCmpt<ServantLeaderCmpt>();
                if (PeGameMgr.IsMultiStory)
                {
                    if (nc.Net.hasOwnerAuth)
                        nc.Net.RPCServer(EPacketType.PT_NPC_ForcedServant, false);
                }
                else
                    leader.RemoveForcedServant(nc);
                nc.FixedPointPos = npc.position;
            }
        }

        //move npc
        Vector3 targetPos;
        for (int i = 0; i < data.m_MoveNpc.Count; i++)
        {
            bool isColonyPos = false;
            MoveNpcData mnd = data.m_MoveNpc[i];
            if (mnd == null)
                continue;

            SpeedState movespeed;
            if (data.m_MoveType == 0)
                movespeed = SpeedState.Walk;
            else
                movespeed = SpeedState.Run;

            if (mnd.pos != Vector3.zero)
                targetPos = mnd.pos;
            else if (mnd.targetNpc > 0)
            {
                if (mnd.targetNpc == 20000)
                {
                    if (PeGameMgr.IsMultiStory)
                    {
                        PlayerNetwork player = PlayerNetwork.GetNearestPlayer(PlayerNetwork.mainPlayer.PlayerPos);
                        targetPos = player.PlayerPos;
                    }
                        
                    else
                        targetPos = MainPlayer.Instance.entity.position;
                }
                else
                {
                    if (EntityMgr.Instance.Get(mnd.targetNpc) == null)
                        continue;
                    targetPos = GetNpcPos(mnd.targetNpc);
                }
            }
            else
            {
                isColonyPos = true;
                if (!CSMain.GetAssemblyPos(out targetPos))
                {
                    if (mnd.targetNpc == -99)
                        continue;
                    else if (mnd.targetNpc == -98)
                        targetPos = PeCreature.Instance.mainPlayer.position;
                }
            }

            List<Vector3> dests = GetMeetingPosition(targetPos, mnd.npcsId.Count, 2);

            Dictionary<PeEntity, bool> tmp = new Dictionary<PeEntity, bool>();
            for (int j = 0; j < mnd.npcsId.Count; j++)
            {
                npc = EntityMgr.Instance.Get(mnd.npcsId[j]);
                if (npc == null)
                    continue;
                if (targetPos == Vector3.zero)
                    break;

                Vector3 dest;
                if (!isColonyPos)
                    dest = dests[j];
                else
                {
                    if (mnd.targetNpc == -99)
                    {
                        CSBuildingLogic cslogic = CSMain.GetAssemblyLogic();
						if(cslogic == null)
							break;
                        int index = (j + 2) < cslogic.m_NPCTrans.Length ? j + 1 : 0;
                        if (!cslogic.GetNpcPos(index, out dest))
                            break;
                    }
                    else if (mnd.targetNpc == -98)
                        dest = AiUtil.GetRandomPositionInLand(targetPos, 30f, 50f, 10f, LayerMask.GetMask("Default", "VFVoxelTerrain", "SceneStatic"), 30);
                    else
                        break;
                }

                MoveTo(npc, dest, 1, true, movespeed);

                NpcCmpt nc = npc.NpcCmpt;
                if (null != nc)
                    nc.FixedPointPos = dest;
                else
                    Debug.LogError("Failed to set fixed point.");

                if (mnd.missionOrPlot_id != 0)
                    tmp.Add(npc, false);
            }

            if (mnd.missionOrPlot_id != 0 && !m_RecordIsReachPoitn.ContainsKey(mnd.missionOrPlot_id))
            {
                m_RecordIsReachPoitn.Add(mnd.missionOrPlot_id, tmp);
            }
        }

        if (data.m_moveNpc_missionOrPlot_id != 0)
        {
            Dictionary<PeEntity, bool> tmp = new Dictionary<PeEntity, bool>();
            foreach (var item in data.m_MoveNpc)
            {
                foreach (var item1 in item.npcsId)
                {
                    npc = EntityMgr.Instance.Get(item1);
                    if (npc == null)
                        continue;
                    tmp.Add(npc, false);
                }
            }
            if (!m_RecordIsReachPoitn.ContainsKey(data.m_moveNpc_missionOrPlot_id))
                m_RecordIsReachPoitn.Add(data.m_moveNpc_missionOrPlot_id, tmp);
        }

        if (data.m_NpcRail.inpclist.Count > 0 && Railway.Manager.Instance.GetRoutes().Count > 0)
        {
            Vector3 pos = Vector3.zero;
            if (data.m_NpcRail.bplayer)
                pos = PeCreature.Instance.mainPlayer.position;
            else if (data.m_NpcRail.othernpcid > 0)
            {
                npc = EntityMgr.Instance.Get(data.m_NpcRail.othernpcid);
                if (npc != null)
                    pos = npc.position;
            }
            else
                pos = data.m_NpcRail.pos;

            if (pos != Vector3.zero)
            {
                Railway.Point start, end;
                int startIndex, endIndex;
                for (int i = 0; i < data.m_NpcRail.inpclist.Count; i++)
                {
                    npc = EntityMgr.Instance.Get(data.m_NpcRail.inpclist[i]);
                    Railway.Manager.Instance.GetTwoPointClosest(npc.ExtGetPos(), pos,out start,out end,out startIndex,out endIndex);
                    if (npc != null)
                    {
                        PassengerInfo pi = new PassengerInfo();
                        pi.npcID = npc.Id;
                        pi.startRouteID = start.routeId;
                        pi.startIndexID = startIndex;
                        pi.endRouteID = end.routeId;
                        pi.endIndexID = endIndex;
                        pi.dest = pos;
                        pi.type = PassengerInfo.Course.before;
                        if(!m_Passengers.ContainsKey(pi.npcID))
                            m_Passengers.Add(pi.npcID, pi);

                        StartCoroutine(WaitingNpcRailStart(start, npc, end, pos));
                    }
                    //npc.GetOnTrain(start.routeId, false);
                }
                //Railway.Point start, end;
                //Railway.Point rp = Railway.Manager.Instance.GetStation(pos, 200);
                //if (rp != null)
                //{
                //    for (int i = 0; i < data.m_NpcRail.inpclist.Count; i++)
                //    {
                //        npc = EntityMgr.Instance.Get(data.m_NpcRail.inpclist[i]);
                //        if (npc != null)
                //            npc.GetOnTrain(rp.routeId, false);
                //    }

                //    StartCoroutine(WaitingNpcRailEnd(rp, data.m_NpcRail, pos));
                //}
            }
        }

        //npc face
        for (int i = 0; i < data.m_NpcFace.Count; i++)
        {
            NpcFace nf = data.m_NpcFace[i];

            npc = EntityMgr.Instance.Get(nf.npcid);

            if (npc == null)
                continue;

            if (nf.angle == -2)
                npc.CmdFaceToPoint(PeCreature.Instance.mainPlayer.ExtGetPos());
            else if (nf.angle != -1)
                SetRotation(npc, Quaternion.AngleAxis(nf.angle, Vector3.up));
            else
            {
                PeEntity npcother = EntityMgr.Instance.Get(nf.otherid);
                if (npcother != null)
                    npc.CmdFaceToPoint(npcother.ExtGetPos());
            }

            //if (!nf.bmove)
            //    npc.CmdStartIdle();
        }

        //npc camp
        for (int i = 0; i < data.m_NpcCamp.Count; i++)
        {
            NpcCamp nc = data.m_NpcCamp[i];

            npc = EntityMgr.Instance.Get(nc.npcid);
            if (npc == null)
                continue;

            npc.SetCamp(nc.camp);
        }

        //sence face
        for (int i = 0; i < data.m_SenceFace.Count; i++)
        {
            SenceFace sf = data.m_SenceFace[i];

            GameObject obj = GameObject.Find(sf.name);
            if (obj == null)
                continue;

            obj.transform.rotation = Quaternion.AngleAxis(sf.angle, Vector3.up);
        }

        //npc ai
        for (int i = 0; i < data.m_NpcAI.Count; i++)
        {
            NpcOpen no = data.m_NpcAI[i];

            npc = EntityMgr.Instance.Get(no.npcid);
            if (npc == null)
                continue;

            npc.SetAiActive(no.bopen);
        }

        //npc invincible
        for (int i = 0; i < data.m_NpcInvincible.Count; i++)
        {
            NpcOpen no = data.m_NpcInvincible[i];

            npc = EntityMgr.Instance.Get(no.npcid);
            if (npc == null)
                continue;

            npc.SetInvincible(no.bopen);
        }

        //zhangshunboPlot
        for (int i = 0; i < data.m_cantReviveNpc.Count; i++)
        {
            npc = EntityMgr.Instance.Get(data.m_cantReviveNpc[i]);
            if (npc != null)
            {
                if (npc.NpcCmpt != null)
                    npc.NpcCmpt.ReviveTime = -1;
            }
        }

        if (data.m_attractMons.Count >= 3)
        {
            List<int> triggerID = new List<int>();
            int divide = data.m_attractMons.FindIndex(ite => ite == -9999);
            bool missionOrPlot = true;
            for (int i = 0; i < data.m_attractMons.Count; i++)
            {
                if (i == divide)
                    continue;
                else if (i == divide + 1)
                {
                    missionOrPlot = data.m_attractMons[i] == 1 ? true : false;
                    continue;
                }
                else if (i > divide + 1)
                    triggerID.Add(data.m_attractMons[i]);
            }
            StartCoroutine(CheckAttractMons(data.m_attractMons.GetRange(0, divide), missionOrPlot, triggerID));
        }

        int excuteNum = data.m_killNpcList.Count / 3;
        List<int> singleHandle = new List<int>();
        List<PeEntity> tmp1 = new List<PeEntity>();
        for (int i = 0; i < excuteNum; i++)
        {
            singleHandle = data.m_killNpcList.GetRange((3 * i), 3);
            tmp1 = KillNPC.NPCBeKilled(singleHandle[0]);
            KillNPC.NPCaddItem(tmp1, singleHandle[1], singleHandle[2]);
        }

        excuteNum = data.m_monsterHatredList.Count / 5;
        for (int i = 0; i < excuteNum; i++)
        {
            singleHandle = data.m_monsterHatredList.GetRange((5 * i), 5);
            SpecialHatred.MonsterHatredAdd(singleHandle);
        }

        excuteNum = data.m_npcHatredList.Count / 4;
        for (int i = 0; i < excuteNum; i++)
        {
            singleHandle = data.m_npcHatredList.GetRange((4 * i), 4);
            SpecialHatred.NpcHatredAdd(singleHandle);
        }

        excuteNum = data.m_harmList.Count / 3;
        for (int i = 0; i < excuteNum; i++)
        {
            singleHandle = data.m_harmList.GetRange((3 * i), 3);
            SpecialHatred.HarmAdd(singleHandle);
        }

        excuteNum = data.m_doodadHarmList.Count / 3;
        for (int i = 0; i < excuteNum; i++)
        {
            singleHandle = data.m_doodadHarmList.GetRange((3 * i), 3);
            PeEntity[] doodads;
            if (singleHandle[1] != 0)
                doodads = EntityMgr.Instance.GetDoodadEntities(singleHandle[1]);
            else
            {
                doodads = EntityMgr.Instance.GetDoodadEntitiesByProtoId(singleHandle[2]);
            }
            for (int j = 0; j < doodads.Length; j++)
            {
                if (doodads[j].GetCmpt<SceneDoodadLodCmpt>() == null)
                    continue;
                if (singleHandle[0] == 0)
                    doodads[j].GetCmpt<SceneDoodadLodCmpt>().IsDamagable = true;
                else
                    doodads[j].GetCmpt<SceneDoodadLodCmpt>().IsDamagable = false;
            }
        }

        foreach (var item in data.m_doodadEffectList)
        {
            PeEntity[] doodads = EntityMgr.Instance.GetDoodadEntities(item.id);
            if (doodads.Length == 0)
                continue;
            if (doodads[0] == null)
                continue;
            doodads[0].transform.TraverseHierarchy(
                (trans, d) =>
                {
                    if (item.names.Contains(trans.gameObject.name))
                        trans.gameObject.SetActive(item.openOrClose);
                });
        }

        if (data.m_npcType.Count > 0)
        {
            foreach (var item in data.m_npcType)
            {
                item.npcs.ForEach(delegate(int n)
                {
                    npc = EntityMgr.Instance.Get(n);
                    if (npc != null && npc.NpcCmpt != null)
                        npc.NpcCmpt.NpcControlCmdId = item.type;
                });
            }
        }

        if (data.m_checkMons.Count > 0)
            StartCoroutine(CheckMonsExist(data.m_checkMons));

        List<PeEntity> monsters;
        foreach (var item in data.m_killMons)
        {
            if (m_RecordKillMons.Count == 0)
                MonsterEntityCreator.commonCreateEvent += KillMonster;
            if (!m_RecordKillMons.ContainsKey(item.id))
                m_RecordKillMons.Add(item.id, item);
            if (item.type == KillMons.Type.protoTypeId) 
            {
                monsters = new List<PeEntity>(EntityMgr.Instance.All);
                monsters = monsters.FindAll(delegate(PeEntity mon) 
                {
                    if (mon == null)
                        return false;
                    if (mon.proto == EEntityProto.Monster && Vector3.Distance(mon.position, item.center) <= item.radius
                            && (item.monId == -999 ? true : mon.entityProto.protoId == item.monId))
                        return true;
                    return false;
                });
                foreach (var mon in monsters)
                {
                    if (mon == null)
                        continue;
                    mon.GetComponent<PESkEntity>().SetAttribute(AttribType.Hp, 0, false);
                }
            }
            else if (item.type == KillMons.Type.fixedId)
            {
                PeEntity fixMon = SceneEntityCreatorArchiver.Instance.GetEntityByFixedSpId(item.monId);
                if (fixMon != null && fixMon.GetComponent<PESkEntity>() != null)
                    fixMon.GetComponent<PESkEntity>().SetAttribute(AttribType.Hp, 0, false);
            }
        }

        foreach (var item in data.m_stopKillMonsID)
        {
            m_RecordKillMons.Remove(item);
            if (m_RecordKillMons.Count <= 0)
                MonsterEntityCreator.commonCreateEvent -= KillMonster;
        }

        foreach (var item in data.m_monPatrolMode)
        {
            ChangePartrolmode recordData;
            bool record = false;
            recordData.monsId = new List<int>();
            recordData.type = 0;
            recordData.radius = 0;

            foreach (var item1 in item.monsId)
            {
                npc = SceneEntityCreatorArchiver.Instance.GetEntityByFixedSpId(item1);
                if (null != npc) 
                {
                    if (npc is EntityGrp)
                    {
                        EntityGrp eg = npc as EntityGrp;
                        foreach (var item2 in eg.memberAgents)
                        {
                            SceneEntityPosAgent sea = item2 as SceneEntityPosAgent;
                            if (sea == null)
                                continue;
                            if (sea.entity == null)
                                continue;
                            sea.entity.BehaveCmpt.PatrolMode = (BHPatrolMode)item.type;
                            sea.entity.BehaveCmpt.MinPatrolRadius = item.radius;
                            sea.entity.BehaveCmpt.MaxPatrolRadius = item.radius;
                        }
                    }
                    else
                    {
                        npc.BehaveCmpt.PatrolMode = (BHPatrolMode)item.type;
                        npc.BehaveCmpt.MinPatrolRadius = item.radius;
                        npc.BehaveCmpt.MaxPatrolRadius = item.radius;
                    }
                }
                else
                {
                    if (!record)
                    {
                        record = true;
                        recordData.type = item.type;
                        recordData.radius = item.radius;
                    }
                    recordData.monsId.Add(item1);
                }
            }
            if (record)
            {
                patrolModeRecord.Add(recordData);
                if (!recordPatrol)
                {
                    recordPatrol = true;
                    MonsterEntityCreator.commonCreateEvent += MonsterCreatePatrolSet;
                }
            }
        }

        foreach (var item in data.m_moveMons)
        {
            PeEntity fixMon = SceneEntityCreatorArchiver.Instance.GetEntityByFixedSpId(item.fixedId);
            if (fixMon != null)
            {
                MonsterCmpt mc = fixMon.GetComponent<MonsterCmpt>();
                if (mc != null)
                    mc.Req_MoveToPosition(item.dist, 1, true, (SpeedState)item.stepOrRun);
            }
            else
            {
                if(m_RecordMoveMons.Count == 0)
                    MonsterEntityCreator.commonCreateEvent += MoveMonster;
                if (!m_RecordMoveMons.Contains(item))
                    m_RecordMoveMons.Add(item);
            }
            if (item.missionOrPlot_id != 0) 
            {
                if (fixMon != null)
                {
                    Dictionary<PeEntity, bool> tmp = new Dictionary<PeEntity, bool>();
                    tmp.Add(fixMon, false);
                    if (!m_RecordIsReachPoitn.ContainsKey(item.missionOrPlot_id))
                        m_RecordIsReachPoitn.Add(item.missionOrPlot_id, tmp);
                }
            }
        }

        if (data.m_moveMons_missionOrPlot_id != 0)
        {
            Dictionary<PeEntity, bool> tmp = new Dictionary<PeEntity, bool>();
            foreach (var item in data.m_moveMons)
            {
                npc = SceneEntityCreatorArchiver.Instance.GetEntityByFixedSpId(item.fixedId);
                if (npc == null)
                    continue;
                tmp.Add(npc, false);
            }
            m_RecordIsReachPoitn.Add(data.m_moveNpc_missionOrPlot_id, tmp);
        }

        foreach (var item in data.m_npcsBattle)
        {
            ENpcBattle type = (ENpcBattle)(item.type - 1);
            foreach (var item1 in item.npcId)
            {
                npc = EntityMgr.Instance.Get(item1);
                if (npc == null)
                    continue;
                npc.GetComponent<NpcCmpt>().Battle = type;
            }
        }

        for (int i = 0; i < data.m_abnormalInfo.Count; i++)
        {
            AbnormalInfo abInfo = data.m_abnormalInfo[i];
            AbnormalConditionCmpt acc;
            for (int j = 0; j < abInfo.npcs.Count; j++)
            {
                if (abInfo.npcs[j] == 30000)
                {
                    foreach (var item in EntityMgr.Instance.All)
                    {
                        if (item == null)
                            continue;
                        if (item.proto != EEntityProto.Npc && item.proto != EEntityProto.RandomNpc)
                            continue;

                        acc = item.GetCmpt<AbnormalConditionCmpt>();
                        if (acc == null)
                            continue;
                        if (abInfo.setOrRevive)
                            acc.StartAbnormalCondition((PEAbnormalType)abInfo.virusNum);
                        else
                            acc.EndAbnormalCondition((PEAbnormalType)abInfo.virusNum);
                    }
                    break;
                }

                if (abInfo.npcs[j] == 20000)
                    npc = MainPlayer.Instance.entity;
                else if (abInfo.npcs[j] == 0)
                {
                    if (!CSMain.HasCSAssembly())
                        continue;
					if(CSMain.GetCSRandomNpc().Count > 0)
                    	npc = CSMain.GetCSRandomNpc()[UnityEngine.Random.Range(0, CSMain.GetCSRandomNpc().Count)];
					else
						continue;
                }
                else
                    npc = EntityMgr.Instance.Get(abInfo.npcs[j]);
                if (npc == null)
                    continue;
                acc = npc.GetCmpt<AbnormalConditionCmpt>();
                if (acc == null)
                    continue;
                if (abInfo.setOrRevive)
                    acc.StartAbnormalCondition((PEAbnormalType)abInfo.virusNum);
                else
                    acc.EndAbnormalCondition((PEAbnormalType)abInfo.virusNum);
            }
        }

        if (null != PeCreature.Instance.mainPlayer)
        {
            int playerID = Mathf.RoundToInt(PeCreature.Instance.mainPlayer.GetAttribute(AttribType.DefaultPlayerID));

            for (int i = 0; i < data.m_reputationChange.Length; i++)
            {
                if (data.m_reputationChange[i].isEffect == false)
                    continue;
                if (data.m_reputationChange[i].type == 0)
                    ReputationSystem.Instance.SetReputationValue(playerID, i + 5, data.m_reputationChange[i].valve);
                else
                    ReputationSystem.Instance.ChangeReputationValue(playerID, i + 5, data.m_reputationChange[i].type * data.m_reputationChange[i].valve, false);
            }
            if (data.m_nativeAttitude.isEffect)
            {
                if (data.m_nativeAttitude.valve == -1)
                    ReputationSystem.Instance.CancelEXValue(playerID, data.m_nativeAttitude.type + 5);
                else
                    ReputationSystem.Instance.SetEXValue(playerID, data.m_nativeAttitude.type + 5, data.m_nativeAttitude.valve);
            }
        }

        if (data.oldDoodad.Count > 0 || data.newDoodad.Count > 0) 
            ReplaceBuilding(data.oldDoodad, data.newDoodad);

        if (data.m_CameraList.Count > 0)
        {
            if (!PeGameMgr.IsMultiStory)
                PlotLensAnimation.PlotPlay(data.m_CameraList);
            else
            {
				if (!PlotLensAnimation.TooFar(new List<int>(Array.ConvertAll<CameraInfo, int>(data.m_CameraList.ToArray(), tmp => tmp.cameraId))))
					PlotLensAnimation.PlotPlay(data.m_CameraList);
            }
        }

        ProcessSpecial(data.m_Special);

        if (data.m_PausePlayer != 0)
            StroyManager.isPausing = data.m_PausePlayer == 1 ? true : false;

        foreach (PeEntity npc1 in EntityMgr.Instance.All)
        {
            if (npc1 == null)
                continue;

            npc1.SetAiActive(data.m_PauseNPC);
            if (!data.m_PauseNPC)
            {
                //MoveTo(Vector3.zero);
                Translate(npc1, Vector3.zero);
                npc1.PatrolMoveTo(Vector3.zero);
            }
        }

        //PauseMonster
        //AiManager.Manager.Pause(data.m_PauseMons);

        if (data.m_EffectPosList.Count > 0 && data.m_EffectID > 0)
        {
            for (int i = 0; i < data.m_EffectPosList.Count; i++)
                Pathea.Effect.EffectBuilder.Instance.Register(data.m_EffectID, null, data.m_EffectPosList[i], Quaternion.identity);
            //EffectManager.Instance.Instantiate(data.m_EffectID, data.m_EffectPosList[i], Quaternion.identity, null);
        }

        if (data.m_SoundPosList.Count > 0 && data.m_SoundID > 0)
        {
            for (int i = 0; i < data.m_SoundPosList.Count; i++)
            {
                Vector3 pos = data.m_SoundPosList[i].type == 1 ? EntityMgr.Instance.Get(data.m_SoundPosList[i].npcID).position : data.m_SoundPosList[i].pos;
                AudioManager.instance.Create(pos, data.m_SoundID);
            }
        }

        //if (data.m_CameraList.Count > 0)
        //    StartCoroutine(WaitingCamera(data));

        for (int i = 0; i < data.m_iColonyNoOrderNpcList.Count; i++)
        {
            npc = EntityMgr.Instance.Get(data.m_iColonyNoOrderNpcList[i]);
            if (npc == null)
                continue;
            if (npc.NpcCmpt == null)
                continue;
            npc.NpcCmpt.BaseNpcOutMission = true;
        }

        for (int i = 0; i < data.m_iColonyOrderNpcList.Count; i++)
        {
            npc = EntityMgr.Instance.Get(data.m_iColonyOrderNpcList[i]);
            if (npc == null)
                continue;
            if (npc.NpcCmpt == null)
                continue;
            npc.NpcCmpt.BaseNpcOutMission = false;
        }

        if (data.m_pauseSiege != 0)
            MonsterSiege_Base.MonsterSiegeBasePause = data.m_pauseSiege == 1 ? true : false;

        foreach (var item in data.m_campAlert)
        {
            Camp cp = Camp.GetCamp(item.id);
            cp.SetCampNpcAlert(item.isActive);
        }

        foreach (var item in data.m_campActive)
        {
            Camp.SetCampActive(item.id, item.isActive);
        }

        foreach (var item in data.m_comMission)
        {
            if (PeGameMgr.IsMulti)
                MissionManager.Instance.RequestCompleteMission(item);
            else
                MissionManager.Instance.CompleteMission(item);
        }

        for (int i = 0; i < data.m_whackedList.Count; i++)
        {
            ENpcBattleInfo whacked = data.m_whackedList[i];
            for (int j = 0; j < whacked.npcId.Count; j++)
            {
                npc = EntityMgr.Instance.Get(whacked.npcId[j]);
                if (npc == null)
                    continue;
                if (whacked.type == 1)
                    SkillSystem.SkEntity.MountBuff(npc.skEntity, 30200176, new List<int>(), new List<float>());
                else if (whacked.type == 0)
                    npc.skEntity.CancelBuffById(30200176);
            }
        }

        for (int i = 0; i < data.m_getMission.Count; i++)
        {
            int n = data.m_getMission[i];
            if (MissionManager.Instance.m_PlayerMission.IsGetTakeMission(n))
            {
                if (MissionRepository.HaveTalkOP(n))
                {
                    GameUI.Instance.mNPCTalk.NormalOrSP(0);
                    if (GameUI.Instance.mNPCTalk.isPlayingTalk == false)
                    {
                        GameUI.Instance.mNPCTalk.UpdateNpcTalkInfo(n, 1);
                        GameUI.Instance.mNPCTalk.PreShow();
                    }
                    else
                        GameUI.Instance.mNPCTalk.AddNpcTalkInfo(n, 1);
                }
                else
                {
                    MissionCommonData missionData = MissionManager.GetMissionCommonData(n);
                    if (data == null)
                        return;

                    npc = EntityMgr.Instance.Get(missionData.m_iNpc);
                    MissionManager.Instance.SetGetTakeMission(n, npc, MissionManager.TakeMissionType.TakeMissionType_Get);
                }
            }
        }

        if (data.m_showTip != 0)
        {
            new PeTipMsg(PELocalization.GetString(data.m_showTip), PeTipMsg.EMsgLevel.Warning, PeTipMsg.EMsgType.Stroy);
        }

        MissionManager.Instance.m_PlayerMission.LanguegeSkill += data.m_increaseLangSkill;

        if (data.m_plotMissionTrigger.Count > 0)
            StartCoroutine(WaitPlotMissionTrigger(data.m_plotMissionTrigger));

        m_StoryList.Remove(id);
        if (delayPlotID.Count == m_StoryList.Count)
            delayPlotID.Clear();
        if (m_MisDelay.ContainsKey(id))
            m_MisDelay.Remove(id);
        MissionManager.Instance.m_PlayerMission.UpdateAllNpcMisTex();
       
    }

    bool recordPatrol;
    List<ChangePartrolmode> patrolModeRecord = new List<ChangePartrolmode>();
    void MonsterCreatePatrolSet(PeEntity mon)
    {
        if (patrolModeRecord.Count == 0)
            return;
        PeEntity entity;
        foreach (var item in patrolModeRecord)
        {
            foreach (var item1 in item.monsId)
            {
                entity = SceneEntityCreatorArchiver.Instance.GetEntityByFixedSpId(item1);
                if (entity is EntityGrp)
                {
                    EntityGrp eg = entity as EntityGrp;
                    if (eg.memberAgents == null)
                        continue;
                    foreach (var item2 in eg.memberAgents)
                    {
                        SceneEntityPosAgent sep = item2 as SceneEntityPosAgent;
                        if (sep == null || sep.entity == null)
                            continue;
                        if (sep.entity == mon)
                        {
                            mon.BehaveCmpt.PatrolMode = (BHPatrolMode)item.type;
                            mon.BehaveCmpt.MinPatrolRadius = item.radius;
                            mon.BehaveCmpt.MaxPatrolRadius = item.radius;
                        }
                    }
                }
                else if(entity != null && entity == mon)
                {
                    entity.BehaveCmpt.PatrolMode = (BHPatrolMode)item.type;
                    entity.BehaveCmpt.MinPatrolRadius = item.radius;
                    entity.BehaveCmpt.MaxPatrolRadius = item.radius;
                }
            }
        }
    }

    void MoveMonster(PeEntity mon)  //怪物生成事件
    {
        List<MoveMons> removeRecord = new List<MoveMons>();
        foreach (var item in m_RecordMoveMons)
        {
            if (SceneEntityCreatorArchiver.Instance.GetEntityByFixedSpId(item.fixedId) == mon)
            {
                if (!mon.GetCmpt<MonsterCmpt>())
                    continue;
                mon.GetCmpt<MonsterCmpt>().Req_MoveToPosition(item.dist, 0, true, (SpeedState)item.stepOrRun);
                removeRecord.Add(item);
            }
        }
        foreach (var item in removeRecord)
        {
            if (m_RecordMoveMons.Contains(item))
                m_RecordMoveMons.Remove(item);
        }

        if (m_RecordMoveMons.Count == 0)
            MonsterEntityCreator.commonCreateEvent -= MoveMonster;
    }

    public void EntityReach(PeEntity entity,bool trigger,bool fromNet = false) 
    {
        List<int> recordRemove = new List<int>();

        List<int> tmpInt = new List<int>(m_RecordIsReachPoitn.Keys);
        foreach (var item in tmpInt)
        {
            List<PeEntity> tmpEntity = new List<PeEntity>(m_RecordIsReachPoitn[item].Keys);
            foreach (var item1 in tmpEntity)
            {
                if (item1 == entity)
                {
                    m_RecordIsReachPoitn[item][entity] = true;
                    if (new List<bool>(m_RecordIsReachPoitn[item].Values).FindAll(ite => ite == true).Count == m_RecordIsReachPoitn[item].Count)
                    {
                        if (trigger)
                        {
                            if(PeGameMgr.IsMulti)
                            {
                                if(PlayerNetwork.mainPlayer != null || !fromNet)
                                {
                                    PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_EntityReach, item, PlayerNetwork.mainPlayerId);
                                }
                            }
                            GetMissionOrPlotById(item);
                        }
                           
                        recordRemove.Add(item);
                    }
                }
            }
        }
        foreach (var item in recordRemove)
        {
            if (m_RecordIsReachPoitn.ContainsKey(item))
                m_RecordIsReachPoitn.Remove(item);
        }
    }

    public void GetMissionOrPlotById(int id) 
    {
        int n = id / 10000;
        if (n == 1) 
        {
            n = id % 10000;
            if (MissionManager.Instance.m_PlayerMission.IsGetTakeMission(n))
            {
                if (MissionRepository.HaveTalkOP(n))
                {
                    GameUI.Instance.mNPCTalk.NormalOrSP(0);
                    if (GameUI.Instance.mNPCTalk.isPlayingTalk == false)
                    {
                        GameUI.Instance.mNPCTalk.UpdateNpcTalkInfo(n, 1);
                        GameUI.Instance.mNPCTalk.PreShow();
                    }
                    else
                        GameUI.Instance.mNPCTalk.AddNpcTalkInfo(n, 1);
                }
                else
                {
                    MissionCommonData data = MissionManager.GetMissionCommonData(n);
                    if (data == null)
                        return;

                    PeEntity npc = EntityMgr.Instance.Get(data.m_iNpc);
                    MissionManager.Instance.SetGetTakeMission(n, npc, MissionManager.TakeMissionType.TakeMissionType_Get);
                }
            }
        }
        else if (n == 2)
        {
            n = id % 10000;
            List<int> tmp = new List<int>();
            tmp.Add(n);
            StroyManager.Instance.PushStoryList(tmp);
        }
    }

    void KillMonster(PeEntity mon)  //怪物生成事件
    {
        foreach (var item in m_RecordKillMons.Values)
        {
            if (item.type == KillMons.Type.fixedId && SceneEntityCreatorArchiver.Instance.GetEntityByFixedSpId(item.monId) == mon)
            {
                if (mon.GetComponent<PESkEntity>() != null)
                    mon.GetComponent<PESkEntity>().SetAttribute(AttribType.Hp, 0, false);
                break;
            }
            else if (item.type == KillMons.Type.protoTypeId)
            {
                if ((item.monId == -999 ? true : mon.entityProto.protoId == item.monId) && Vector3.Distance(mon.position, item.center) <= item.radius
                    && mon.GetComponent<PESkEntity>() != null)
                    mon.GetComponent<PESkEntity>().SetAttribute(AttribType.Hp, 0, false);
                break;
            }
        }
    }

    void ReplaceBuilding(List<int> oldBuilding,List<int> newBuilding) 
    {
        try
        {
            SceneDoodadLodCmpt doodad;
            foreach (var item in oldBuilding)
            {
                doodad = EntityMgr.Instance.GetDoodadEntities(item)[0].GetComponent<SceneDoodadLodCmpt>();
                if (doodad == null)
                    return;
                doodad.IsShown = false;
            }
            foreach (var item in newBuilding)
            {
                doodad = EntityMgr.Instance.GetDoodadEntities(item)[0].GetComponent<SceneDoodadLodCmpt>();
                if (doodad == null)
                    return;
                doodad.IsShown = true;
            }
        }
        catch
        {
            Debug.LogError("Replacing buildingDoodad failed.");
        }
    }


    void DestroyRailWay()
    {
        MapMaskData mmd = MapMaskData.s_tblMaskData.Find(ret => ret.mId == 29);
        if (mmd == null)
            return;
        List<int> routeID = new List<int>();
        List<int> routeNpcId = new List<int>();
        foreach (var item in Railway.Manager.Instance.GetRoutes())
        {
            for (int i = 0; i < item.pointCount; i++)
            {
                Railway.Point point = item.GetPointByIndex(i);
                if (Vector3.Distance(point.position, mmd.mPosition) < 200)
                {
                    if (!routeID.Contains(point.routeId))
                    {
                        routeID.Add(point.routeId);
						if(null != item.train)
	                        item.train.ClearPassenger();
                        foreach (var passenger in m_Passengers)
                        {
                            if (passenger.Value.startRouteID == item.id)
                                routeNpcId.Add(passenger.Key);
                        }
                        foreach (var rpassenger in routeNpcId)
                        {
                            Translate(EntityMgr.Instance.Get(rpassenger), m_Passengers[rpassenger].dest);
                            m_Passengers.Remove(rpassenger);
                        }
                    }
                    point.Destroy();
                }
            }
        }
        foreach (var item in routeID)
            RailwayOperate.Instance.RequestDeleteRoute(item);
    }

    void TakeMissionOrPlot(bool missionOrPlot, List<int> trigerId) 
    {
        if (missionOrPlot)
        {
            for (int i = 0; i < trigerId.Count; i++)
            {
                if (!MissionManager.Instance.IsGetTakeMission(trigerId[i]))
                    continue;
                MissionCommonData info = MissionManager.GetMissionCommonData(trigerId[i]);
                if (info == null)
                    continue;
                if (MissionRepository.HaveTalkOP(trigerId[i]))
                {
                    GameUI.Instance.mNPCTalk.NormalOrSP(0);
                    if (GameUI.Instance.mNPCTalk.isPlayingTalk == false)
                    {
                        GameUI.Instance.mNPCTalk.UpdateNpcTalkInfo(trigerId[i], 1);
                        GameUI.Instance.mNPCTalk.PreShow();
                    }
                    else
                    {
                        int count = 0;
                        List<int> talkList = GameUI.Instance.mNPCTalk.m_NpcTalkList.ConvertAll<int>(ite => ite.talkid);
                        int minCount = talkList.Count < info.m_TalkOP.Count ? talkList.Count : info.m_TalkOP.Count;
                        for (int j = 0; j < minCount; j++)
                        {
                            if (talkList[j] == info.m_TalkOP[j])
                                count++;
                        }
                        if (count != minCount)
                            GameUI.Instance.mNPCTalk.AddNpcTalkInfo(trigerId[i], 1);
                    }
                }
                else
                {
                    PeEntity npc = EntityMgr.Instance.Get(info.m_iNpc);
                    MissionManager.Instance.SetGetTakeMission(trigerId[i], npc, MissionManager.TakeMissionType.TakeMissionType_Get, false);
                }
            }
        }
        else
            StroyManager.Instance.PushStoryList(trigerId);
    }

    IEnumerator CheckAttractMons(List<int> monsId, bool missionOrPlot, List<int> ids)
    {
        //SceneEntityCreatorArchiver.Instance.GetEntityByFixedSpId(data.m_attractMons[i]);
        List<PeEntity> mons = new List<PeEntity>();
        PeEntity player = PeCreature.Instance.mainPlayer;
        PeEntity mon;
        bool canPass = false;
        while (true)
        {
            if (mons.Count < monsId.Count)
            {
                foreach (var item in monsId)
                {
                    mon = SceneEntityCreatorArchiver.Instance.GetEntityByFixedSpId(item);
                    if (mon == null)
                        break;
                    mons.Add(mon);
                }
                if (mons.Count < monsId.Count)
                {
                    mons.Clear();
                    yield return new WaitForSeconds(2f);
                    continue;
                }
            }
            foreach (var item in mons)
            {
                if (item.attackEnemy == null)
                    continue;
                if (item.attackEnemy.entityTarget == player) 
                {
                    canPass = true;
                    break;
                }
            }
            if (canPass)
                break;
            yield return new WaitForSeconds(2f);
        }
        TakeMissionOrPlot(missionOrPlot, ids);
    }

    IEnumerator CheckMonsExist(List<CheckMons> data) 
    {
        CheckMons cm;
        bool[] n = new bool[data.Count - 1];
        while (true)
        {
            int num;
            for (int i = 0; i < data.Count - 1; i++)
            {
                cm = data[i];
                Vector3 center;
                if (cm.npcid != 0)
                {
                    if (cm.npcid == 20000)
                        center = PeCreature.Instance.mainPlayer.position;
                    else
                        center = EntityMgr.Instance.Get(cm.npcid).position;
                }
                else { center = cm.center; }
                num = EntityMgr.Instance.GetHatredEntities(center, cm.radius, cm.protoTypeid).Length;
                if ((cm.existOrNot && num > 0) || (!cm.existOrNot && num <= 0))
                    n[i] = true;
            }
            if (!Array.Exists<bool>(n, ite => !ite))
                break;
            yield return new WaitForSeconds(2f);
        }
        cm = data[data.Count - 1];
        TakeMissionOrPlot(cm.missionOrPlot, cm.trigerId);
    }

    IEnumerator WaitDayuLowHP() 
    {
        PeEntity dayu = SceneEntityCreatorArchiver.Instance.GetEntityByFixedSpId(94);
        if (null == dayu)
            yield break;
        while (dayu.GetAttribute(AttribType.Hp) > 800)
            yield return new WaitForSeconds(0.5f);
        List<int> tmp = new List<int>();
        tmp.Add(282);
        StroyManager.Instance.PushStoryList(tmp);
    }

    IEnumerator WaitPlotMissionTrigger(List<string> triggerData)
    {
        int enterOrExit = Convert.ToInt32(triggerData[0]);

        List<string> tmp = new List<string>(triggerData[2].Split(','));
        Vector3 position;
        if (tmp.Count == 3)
            position = new Vector3(Convert.ToSingle(tmp[0]), Convert.ToSingle(tmp[1]), Convert.ToSingle(tmp[2]));
        else
        {
            if (EntityMgr.Instance.Get(Convert.ToInt32(tmp[0])) != null)
                position = EntityMgr.Instance.Get(Convert.ToInt32(tmp[0])).position;
            else
                yield break;
        }
        if (position == new Vector3(-255, -255, -255))
        {
            if (CSMain.HasCSAssembly())
                CSMain.GetAssemblyPos(out position);
            else
                yield break;
        }
        int radius = Convert.ToInt32(triggerData[3]);
        int plotOrMission = Convert.ToInt32(triggerData[4]);
        List<int> triggerID = new List<int>();

        tmp = new List<string>(triggerData[5].Split(','));
        for (int i = 0; i < tmp.Count; i++)
        {
            triggerID.Add(Convert.ToInt32(tmp[i]));
        }

        List<PeEntity> entities = new List<PeEntity>();
        tmp = new List<string>(triggerData[1].Split(','));
        for (int i = 0; i < tmp.Count; i++)
        {
            if (tmp[i] == "20000")
                entities.Add(PeCreature.Instance.mainPlayer);
            else if (EntityMgr.Instance.Get(Convert.ToInt32(tmp[i])) != null)
                entities.Add(EntityMgr.Instance.Get(Convert.ToInt32(tmp[i])));
        }
        while (true)
        {
            if ((enterOrExit == 1 && entities.FindAll(ite => Vector3.Distance(position, ite.position) <= radius).Count == entities.Count)
                || (enterOrExit == 2 && entities.FindAll(ite => Vector3.Distance(position, ite.position) >= radius).Count == entities.Count))
                break;
            yield return new WaitForSeconds(1f);
        }
        TakeMissionOrPlot(plotOrMission == 1 ? false : true, triggerID);
    }

    public IEnumerator WaitingNpcRailStart(Railway.Point start, PeEntity npc,Railway.Point dest,Vector3 pos)
    {
        yield return 0;
        if (npc.NpcCmpt != null && npc.NpcCmpt.Req_Contains(EReqType.FollowTarget))
            yield break;

        MoveTo(npc, start.position, 6, true, SpeedState.Run);
        while (true)
        {
            if (start.GetArriveTime() < 0.3f && Vector3.Distance(npc.position, start.position) < 10)
            {
                if (m_Passengers.ContainsKey(npc.Id))
                    m_Passengers[npc.Id].type = PassengerInfo.Course.on;

                npc.GetOnTrain(start.routeId, false);
                StartCoroutine(WaitingNpcRailEnd(dest, npc, pos));

                yield break;
            }
            yield return new WaitForSeconds(0.2f);
        }
    }

    public IEnumerator WaitingNpcRailEnd(Railway.Point dest, PeEntity npc, Vector3 pos)
    {
        yield return 0;
        Vector3 rpos;
        while (true)
        {
            if (dest.GetArriveTime() < 0.3f)
            {
                if (npc == null)
                    continue;

                if (m_Passengers.ContainsKey(npc.Id))
                    m_Passengers[npc.Id].type = PassengerInfo.Course.latter;
                rpos = AiUtil.GetRandomPosition(dest.position, 4, 6, 100.0f, AiUtil.groundedLayer, 10);
                npc.GetOffTrain(rpos);
                MoveTo(npc, pos, 1, true, SpeedState.Run);
                m_Passengers.Remove(npc.Id);

                yield break;
            }

            yield return new WaitForSeconds(0.2f);
        }
    }

    //IEnumerator WaitingCamera(StoryData data)
    //{
    //    m_CamIdx = 0;

    //    while (true)
    //    {
    //        if (!m_bCamModing && !m_bCamMoveModing && !m_bCamRotModing)
    //        {
    //            if (m_CamIdx >= data.m_CameraList.Count)
    //                yield return 0;
    //            else
    //            {
    //                CameraPlot cpData = CameraRepository.GetCameraPlotData(data.m_CameraList[m_CamIdx]);
    //                ProcessCamera(cpData);
    //            }

    //            m_CamIdx++;
    //        }

    //        yield return new WaitForSeconds(0.1f);
    //    }
    //}

    void ProcessCamera(CameraPlot cpData)
    {
        if (cpData == null)
            return;

        //m_bCamModing = true;
        PeEntity npc;
//        int outinfo;

        Vector3 distPos = Vector3.zero;
        Vector3 dir = Vector3.zero;
//        int dirType = 0;

        if (cpData.m_CamMove.npcid > 0)
        {
            npc = EntityMgr.Instance.Get(cpData.m_CamMove.npcid);
            if (npc == null)
                return;

            distPos = npc.position;

            if (cpData.m_CamMove.dirType == 1)
                distPos = distPos + npc.GetForward() * 5 + Vector3.up * 5;
            else if (cpData.m_CamMove.dirType == 2)
                distPos = distPos + npc.GetForward() * -5 + Vector3.up * 5;
        }
        else if (cpData.m_CamMove.pos != Vector3.zero)
            distPos = cpData.m_CamMove.pos;


        if (cpData.m_CamMove.type == 1)
        {
            if (m_CamIdx == 0)
            {
				mFollowCameraTarget.position = PETools.PEUtil.MainCamTransform.position;
				mFollowCameraTarget.rotation = PETools.PEUtil.MainCamTransform.rotation;
            }

            StartCoroutine(CamFollowMove(distPos, cpData.m_Delay));
            //StartCoroutine(CamFollow(cpData.m_CamMove.name, 0, 1, 0, Vector3.zero, distPos, cpData.m_Delay, 0));

            if (mCamMode == null || PECameraMan.Instance.m_Controller.currentMode != mCamMode)
                mCamMode = PECameraMan.Instance.EnterFollow(mFollowCameraTarget);

            //return;
        }
        else if (cpData.m_CamMove.type == 2)
        {
            mFollowCameraTarget.position = distPos;
            if (m_CamIdx == 0)
				mFollowCameraTarget.rotation = PETools.PEUtil.MainCamTransform.rotation;

            if (mCamMode == null || PECameraMan.Instance.m_Controller.currentMode != mCamMode)
                mCamMode = PECameraMan.Instance.EnterFollow(mFollowCameraTarget);
        }

        if (cpData.m_CamRot.angleY != 0
            || cpData.m_CamRot.angleX != 0)
        {
            float trueangleY = 0;
            float trueangleX = 0;
            //dirType = cpData.m_CamRot.dirType == 1 ? 1 : -1;

            if (cpData.m_CamRot.type1 == 1)
            {
                dir = Vector3.up;
                if (cpData.m_CamRot.dirType == 1)
                    trueangleY = cpData.m_CamRot.angleY;
                else
                    trueangleY = cpData.m_CamRot.angleY - 360;
            }
            else if (cpData.m_CamRot.type1 == 2)
            {
                dir = Vector3.right;
                if (cpData.m_CamRot.dirType == 1)
                    trueangleY = cpData.m_CamRot.angleY;
                else
                    trueangleY = cpData.m_CamRot.angleY - 360;
            }
            else if (cpData.m_CamRot.type1 == 3)
            {
                dir = Vector3.forward;
                if (cpData.m_CamRot.dirType == 1)
                {
                    trueangleY = cpData.m_CamRot.angleY;
                    trueangleX = cpData.m_CamRot.angleX;
                }
                else
                {
                    trueangleY = cpData.m_CamRot.angleY - 360;
                    trueangleX = cpData.m_CamRot.angleX - 360;
                }
            }

            trueangleX = trueangleX < -360 ? trueangleX + 360 : trueangleX;
            trueangleY = trueangleY < -360 ? trueangleY + 360 : trueangleY;

            if (m_CamIdx == 0)
				mFollowCameraTarget.position = PETools.PEUtil.MainCamTransform.position;

            if (cpData.m_CamRot.type == 1)
            {
                StartCoroutine(CamFollow(0, 0, 2, trueangleY, dir, Vector3.zero, cpData.m_Delay, trueangleX));

                if (mCamMode == null || PECameraMan.Instance.m_Controller.currentMode != mCamMode)
                    mCamMode = PECameraMan.Instance.EnterFollow(mFollowCameraTarget);

                //return;
            }
            else if (cpData.m_CamRot.type == 2)
            {
                Quaternion qua = new Quaternion();
                if (dir == Vector3.up)
                    qua = Quaternion.Euler(new Vector3(0, trueangleY, 0));
                else if (dir == Vector3.right)
                    qua = Quaternion.Euler(new Vector3(-trueangleY, 0, 0));
                else if (dir == Vector3.forward)
                    qua = Quaternion.Euler(new Vector3(-trueangleX, trueangleY, 0));

                mFollowCameraTarget.rotation = qua;
            }

            if (mCamMode == null || PECameraMan.Instance.m_Controller.currentMode != mCamMode)
                mCamMode = PECameraMan.Instance.EnterFollow(mFollowCameraTarget);
        }

        if (cpData.m_CamTrack.npcid > 0)
        {
            if (m_CamIdx == 0)
            {
				mFollowCameraTarget.position = PETools.PEUtil.MainCamTransform.position;
				mFollowCameraTarget.rotation = PETools.PEUtil.MainCamTransform.rotation;
            }


            if (mCamMode == null || PECameraMan.Instance.m_Controller.currentMode != mCamMode)
                mCamMode = PECameraMan.Instance.EnterFollow(mFollowCameraTarget);

            StartCoroutine(CamFollow(cpData.m_CamTrack.npcid, cpData.m_CamTrack.type, 3, 0, Vector3.zero, Vector3.zero, cpData.m_Delay, 0));
            //return;
        }

        if (cpData.m_CamToPlayer)
        {
            //StartCoroutine(CamFollow(0, 0, -1, 0, Vector3.zero, PeCreature.Instance.mainPlayer.CameraTarget.CustomCameraTarget.position, 0, 0));
            //return;
        }

        //m_CamIdx++;
        //m_bCamModing = false;
    }

    IEnumerator CamFollowMove(Vector3 distpos, int delay)
    {
        bool bfinish = false;
        float dist;
        //m_bCamMoveModing = true;

        while (!bfinish)
        {
            mFollowCameraTarget.position = Vector3.Lerp(mFollowCameraTarget.position, distpos, 0.03f);
            dist = Vector3.Distance(mFollowCameraTarget.position, distpos);
            if (dist < 1)
                bfinish = true;

            yield return new WaitForSeconds(0.01f);
        }

        yield return new WaitForSeconds(delay);
        //m_bCamMoveModing = false;
    }

    IEnumerator CamFollow(int npcid, int type, int CameraType, float angle, Vector3 dir, Vector3 distpos, int delay, float angle1)
    {
        PeEntity npc = null;
        float dist;
        Vector3 velocity = Vector3.zero;
        Quaternion endQua = new Quaternion();
        Quaternion endQua1 = new Quaternion();
        bool bfinish = false;
        bool bdouble = false;
        float trueangle = angle;
        float trueangle1 = angle1;
        float rotateSpeed = 30;
        float t;
        //m_bCamRotModing = true;

        if (CameraType == 2)
        {
            if (angle > 180 || angle < -180)
            {
                bdouble = true;
                trueangle = angle / 2;
            }

            if (angle1 > 180 || angle1 < -180)
            {
                bdouble = true;
                trueangle1 = angle1 / 2;
            }

            if (dir == Vector3.up)
            {
                endQua = Quaternion.Euler(new Vector3(0, trueangle, 0));
                endQua1 = Quaternion.Euler(new Vector3(0, angle, 0));
            }
            else if (dir == Vector3.right)
            {
                endQua = Quaternion.Euler(new Vector3(-trueangle, 0, 0));
                endQua1 = Quaternion.Euler(new Vector3(-angle, 0, 0));
            }
            else if (dir == Vector3.forward)
            {
                endQua = Quaternion.Euler(new Vector3(-trueangle1, trueangle, 0));
                endQua1 = Quaternion.Euler(new Vector3(-angle1, angle, 0));
            }
        }
        else if (CameraType == 3)
        {
            npc = EntityMgr.Instance.Get(npcid);
            if (npc == null)
                bfinish = true;
        }

        while (!bfinish)
        {
            //if (CameraType == 1)
            //{
            //    mFollowCameraTarget.position = Vector3.Lerp(mFollowCameraTarget.position, distpos, 0.03f);
            //    dist = Vector3.Distance(mFollowCameraTarget.position, distpos);
            //    if (dist < 1)
            //        bfinish = true;
            //}
            /*else */
            if (CameraType == 2)
            {
                t = rotateSpeed / Quaternion.Angle(mFollowCameraTarget.rotation, endQua) * Time.deltaTime;

                mFollowCameraTarget.rotation = Quaternion.Slerp(mFollowCameraTarget.rotation, endQua, t);

                dist = Quaternion.Angle(mFollowCameraTarget.rotation, endQua);
                if (dist < 1)
                {
                    if (bdouble)
                    {
                        bdouble = false;
                        endQua = endQua1;
                    }
                    else
                        bfinish = true;
                }
            }
            else if (CameraType == 3)
            {
                if (type == 1)
                {
                    Vector3 endpos = npc.position + Vector3.up * 5 + npc.GetForward() * -16;
                    mFollowCameraTarget.position = Vector3.SmoothDamp(mFollowCameraTarget.position, endpos, ref velocity, 0.3f);
                }
                else if (type == 2)
                    mFollowCameraTarget.LookAt(npc.GetGameObject().transform);
                else if (type == 3)
                {
                    Vector3 endpos = npc.position + Vector3.up * 5 + npc.GetForward() * -16;
                    mFollowCameraTarget.position = Vector3.SmoothDamp(mFollowCameraTarget.position, endpos, ref velocity, 0.1f);
                    Vector3 relativePos = endpos - mFollowCameraTarget.position;
                    Quaternion rotation = Quaternion.LookRotation(relativePos);
                    mFollowCameraTarget.rotation = rotation;
                }
            }
            else
            {
                dist = Vector3.Distance(distpos, mFollowCameraTarget.position);
                dist = dist / 30;
                mFollowCameraTarget.position = Vector3.SmoothDamp(mFollowCameraTarget.position, distpos, ref velocity, dist);
                dist = Vector3.Distance(mFollowCameraTarget.position, distpos);
                if (dist < 1)
                {
                    bfinish = true;
                    Quaternion rotation = Quaternion.LookRotation(PeCreature.Instance.mainPlayer.GetGameObject().transform.forward);
                    mFollowCameraTarget.rotation = rotation;
                    PECameraMan.Instance.RemoveCamMode(mCamMode);
                }
            }

            yield return new WaitForSeconds(0.01f);
        }

        yield return new WaitForSeconds(delay);
        //m_CamIdx++;
        //m_bCamModing = false;
        //m_bCamRotModing = false;
    }


    public void PushStoryList(List<int> idlist)
    {
        for (int i = 0; i < idlist.Count; i++)
        {
            int id = idlist[i];

            if (id == -1)
                continue;

            //bool initmis = MissionManager.Instance.m_bHadInitMission;

            //if (!initmis)
            //{
            StoryData data = StoryRepository.GetStroyData(id);
            if (data == null)
                continue;

            if (data.m_Delay > PETools.PEMath.Epsilon)
            {
                if (!m_MisDelay.ContainsKey(id))
                    m_MisDelay.Add(id, Time.time);
            }

            if (!m_StoryList.ContainsKey(id))
            {
                if (PeGameMgr.IsMulti)
                    PlayerNetwork._storyPlot.Add(id);
                m_StoryList.Add(id, MissionManager.Instance.m_bHadInitMission);
            }
                
        }
    }

    public void PushAdStoryList(List<int> idlist,int npcid = -1)
    {
        for (int i = 0; i < idlist.Count; i++)
        {
            int id = idlist[i];

            AdStoryData data = StoryRepository.GetAdStroyData(id);
            if (data == null)
                continue;

            if (!m_AdStoryList.Contains(id))
            {
                m_AdStoryList.Add(id);
                m_AdStoryListNpc.Add(id, npcid);
            }                
        }
    }

    Vector3 Str2V3(string param)
    {
        string[] pos = param.Split(',');
        if (pos.Length < 3)
        {
            return Vector3.zero;
        }
        float x = System.Convert.ToSingle(pos[0]);
        float y = System.Convert.ToSingle(pos[1]);
        float z = System.Convert.ToSingle(pos[2]);
        return new Vector3(x, y, z);
    }
    static UnityEngine.Object samplePosObj;
    static UnityEngine.Object SamplePosObj
    {
        get
        {
            if(samplePosObj == null)
                samplePosObj = Resources.Load("Prefab/Item/Other/TextSamplePos");
            return samplePosObj;
        }
    }
    static UnityEngine.Object sampleObj;
    static UnityEngine.Object SampleObj
    {
        get
        {
            if(sampleObj == null)
                sampleObj = Resources.Load("Prefab/Item/Other/language_sample_canUse");
            return sampleObj;
        }
    }

    public static void CreateAndHeraNest(int index)
    {
        if(PeGameMgr.IsSingle)
        {
            switch (index)
            {
                case 0:
                    CreateAndHeraNest_index(index, "Prefab/Item/Other/coelodonta_rhino_bone", new Vector3(5164.348f, 480.4204f, 12205.1f), new Vector3(340, 68, 3), 1569);
                    break;
                case 1:
                    CreateAndHeraNest_index(index, "Prefab/Item/Other/coelodonta_rhino_bone", new Vector3(5159.757f, 478.46f, 12206.4f), new Vector3(), 1569);
                    break;
                case 2:
                    CreateAndHeraNest_index(index, "Prefab/Item/Other/lepus_hare_bone", new Vector3(5159.303f, 478.9189f, 12192.89f), new Vector3(), 1568);
                    break;
                case 3:
                    CreateAndHeraNest_index(index, "Prefab/Item/Other/andhera_queen_egg", new Vector3(5159.537f, 479.3898f, 12186.93f), new Vector3(), 1570);
                    break;
                case 4:
                    CreateAndHeraNest_index(index, "Prefab/Item/Other/andhera_queen_egg", new Vector3(5158.81f, 478.58f, 12185.42f), new Vector3(9, 2, 20), 1570);
                    break;
                case 5:
                    CreateAndHeraNest_index(index, "Prefab/Item/Other/andhera_queen_egg", new Vector3(5161.023f, 480.3488f, 12184.96f), new Vector3(0, 0, 30), 1570);
                    break;
                case 6:
                    CreateAndHeraNest_index(index, "Prefab/Item/Other/andhera_queen_egg", new Vector3(5159.489f, 478.992f, 12183f), new Vector3(15, 2, 5), 1570);
                    break;
                case 7:
                    CreateAndHeraNest_index(index, "Prefab/Item/Other/andhera_queen_egg", new Vector3(5157.84f, 478.05f, 12184.52f), new Vector3(350, 358, 7), 1570);
                    break;
                default:
                    break;
            }
        }
        else
        {
            switch (index)
            {
                case 0:
                    PlayerNetwork.mainPlayer.CreateSceneItem("coelodonta_rhino_bone0", new Vector3(5164.348f, 480.4204f, 12205.1f), "1569,1",-1,true);
                    break;
                case 1:
                    PlayerNetwork.mainPlayer.CreateSceneItem("coelodonta_rhino_bone1", new Vector3(5159.757f, 478.46f, 12206.4f), "1569,1", -1, true);
                    break;
                case 2:
                    PlayerNetwork.mainPlayer.CreateSceneItem("lepus_hare_bone2", new Vector3(5159.303f, 478.9189f, 12192.89f), "1568,1", -1, true);
                    break;
                case 3:
                    PlayerNetwork.mainPlayer.CreateSceneItem("andhera_queen_egg3", new Vector3(5159.537f, 479.3898f, 12186.93f), "1570,1", -1, true);
                    break;
                case 4:
                    PlayerNetwork.mainPlayer.CreateSceneItem("andhera_queen_egg4", new Vector3(5158.81f, 478.58f, 12185.42f), "1570,1", -1, true);
                    break;
                case 5:
                    PlayerNetwork.mainPlayer.CreateSceneItem("andhera_queen_egg5", new Vector3(5161.023f, 480.3488f, 12184.96f), "1570,1", -1, true);
                    break;
                case 6:
                    PlayerNetwork.mainPlayer.CreateSceneItem("andhera_queen_egg6", new Vector3(5159.489f, 478.992f, 12183f), "1570,1", -1, true);
                    break;
                case 7:
                    PlayerNetwork.mainPlayer.CreateSceneItem("andhera_queen_egg7", new Vector3(5157.84f, 478.05f, 12184.52f), "1570,1", -1, true);
                    break;
                default:
                    break;
            }
        }
    }

    static void CreateAndHeraNest_index(int index, string path, Vector3 pos, Vector3 euler, int protoID)
    {
        UnityEngine.Object o = Resources.Load(path);
        GameObject go = Instantiate(o) as GameObject;
        go.transform.position = pos;
        go.transform.eulerAngles = euler;
        ItemDrop itemf = go.GetComponent<ItemDrop>();
        if (itemf != null)
        {
            itemf.AddItem(protoID, 1);
            MissionManager.Instance.m_PlayerMission.recordAndHer.Add(index);
            itemf.fetchItem += delegate()
            {
                GameObject.Destroy(go);
                MissionManager.Instance.m_PlayerMission.recordAndHer.Remove(index);
            };
        }
    }

    static public ItemDrop CreateAndHeraNest_indexNet(string objName, Vector3 objPos, List<int> itemObjId = null, MapObjNetwork net = null)
    {
        int index = System.Convert.ToInt32( objName.Substring(objName.Length - 1, 1));
        string path = "";
        Vector3 ag = new Vector3();
        switch (index)
        {
            case 0:
                path = "Prefab/Item/Other/coelodonta_rhino_bone";
                ag = new Vector3(340, 68, 3);
                break;
            case 1:
                path = "Prefab/Item/Other/coelodonta_rhino_bone";
                ag = new Vector3();
                break;
            case 2:
                path = "Prefab/Item/Other/lepus_hare_bone";
                ag = new Vector3();
                break;
            case 3:
                path = "Prefab/Item/Other/andhera_queen_egg";
                ag = new Vector3();
                break;
            case 4:
                path = "Prefab/Item/Other/andhera_queen_egg";
                ag = new Vector3(9, 2, 20);
                break;
            case 5:
                path = "Prefab/Item/Other/andhera_queen_egg";
                ag = new Vector3(0, 0, 30);
                break;
            case 6:
                path = "Prefab/Item/Other/andhera_queen_egg";
                ag = new Vector3(15, 2, 5);
                break;
            case 7:
                path = "Prefab/Item/Other/andhera_queen_egg";
                ag = new Vector3(350, 358, 7);
                break;
            default:
                break;
        }


        UnityEngine.Object o = Resources.Load(path);
        GameObject go = Instantiate(o) as GameObject;
        go.transform.position = objPos;
        go.transform.eulerAngles = ag;
        ItemDrop itemf = go.GetComponent<ItemDrop>();
        if (itemf != null)
        {
            itemf.SetNet(net);
            foreach (int iter in itemObjId)
            {
                if (ItemMgr.Instance.Get(iter) != null)
                    itemf.AddItem(ItemMgr.Instance.Get(iter));
                else
                    Debug.LogError(objName + " item is null");
            }
            MissionManager.Instance.m_PlayerMission.recordAndHer.Add(index);
            itemf.fetchItem += delegate ()
            {
                GameObject.Destroy(go);
                MissionManager.Instance.m_PlayerMission.recordAndHer.Remove(index);
            };
        }
        return itemf;
    }

    public static void CreateLanguageSample_byIndex(int index)
    {
        if (index < 0 || index > 19)
            return;
        GameObject samplePos = Instantiate(SamplePosObj) as GameObject;
        Transform[] transes = samplePos.GetComponentsInChildren<Transform>();
        GameObject sample = Instantiate(SampleObj) as GameObject;
        System.Text.StringBuilder s = new System.Text.StringBuilder(sample.name);
        s.AppendFormat(":{0}", (index));
        sample.name = s.ToString();
        sample.transform.position = transes[index + 1].position;
        sample.transform.eulerAngles = transes[index + 1].eulerAngles;
        ItemDrop itemf = sample.GetComponent<ItemDrop>();
        if (itemf != null)
            itemf.AddItem(1541, 1);

        Destroy(samplePos);
    }

    public static void CreateLanguageSample()
    {
        if (MissionManager.Instance.m_PlayerMission.textSamples.Count > 0)
            return;

        GameObject samplePos = Instantiate(SamplePosObj) as GameObject;
        Transform[] transes = samplePos.GetComponentsInChildren<Transform>();
        GameObject sample;
        for (int i = 1; i < transes.Length; i++)
        {
            if(PeGameMgr.IsSingle)
            {
                sample = Instantiate(SampleObj) as GameObject;
                System.Text.StringBuilder s = new System.Text.StringBuilder(sample.name);
                s.AppendFormat(":{0}", (i - 1));
                sample.name = s.ToString();
                sample.transform.position = transes[i].position;
                sample.transform.eulerAngles = transes[i].eulerAngles;
                ItemDrop itemf = sample.GetComponent<ItemDrop>();
                if (itemf != null)
                    itemf.AddItem(1541, 1);
                MissionManager.Instance.m_PlayerMission.textSamples.Add(i - 1, transes[i].position);
            }
            else
            {
                string name = "language_sample_canUse(Clone):" + (i - 1);
                PlayerNetwork.mainPlayer.CreateSceneItem(name, transes[i].position, "1541,1");
            }
        }
        Destroy(samplePos);
    }

    public static ItemDrop CreateLanguageSampleNet(string objName, Vector3 objPos, List<int> itemObjId = null, MapObjNetwork net = null)
    {
        GameObject sample = Instantiate(SampleObj) as GameObject;
        //System.Text.StringBuilder s = new System.Text.StringBuilder(sample.name);
        sample.name = objName;
        sample.transform.position = objPos;
        ItemDrop itemf = sample.GetComponent<ItemDrop>();
        if (itemf != null)
        {
            itemf.SetNet(net);
            foreach (int iter in itemObjId)
            {
                if (ItemMgr.Instance.Get(iter) != null)
                    itemf.AddItem(ItemMgr.Instance.Get(iter));
                else
                    Debug.LogError(objName+" item is null");
            }
        }
        string temp = objName.Replace("language_sample_canUse(Clone):", "");
        int index = System.Convert.ToInt32(temp);
        if(!MissionManager.Instance.m_PlayerMission.textSamples.ContainsKey(index))
            MissionManager.Instance.m_PlayerMission.textSamples.Add(index, objPos);
        return itemf;
    }

    public static ItemDrop CreateBackpack(Vector3 objPos, List<int> itemObjId = null, MapObjNetwork net = null)
    {
        UnityEngine.Object obj = Resources.Load("Prefab/Item/Other/backpack");
        if (null != obj)
        {
            GameObject backpack = Instantiate(obj) as GameObject;
            backpack.transform.position = objPos;
            backpack.name = "backpack";

            ItemDrop itemf = backpack.GetComponent<ItemDrop>();
            if (itemf != null)
            {
                if (PeGameMgr.IsSingleStory)
                    itemf.AddItem(1332, 1);
                else
                {
                    itemf.SetNet(net);
                    foreach (int iter in itemObjId)
                    {
                        if (ItemMgr.Instance.Get(iter) != null)
                            itemf.AddItem(ItemMgr.Instance.Get(iter));
                        else
                            Debug.LogError(backpack.name + " item is null");
                    }
                }
                return itemf;
            }
        }
        return null;
    }

	public static ItemDrop CreatePajaLanguage(Vector3 objPos, List<int> itemObjId = null, MapObjNetwork net = null) 
	{
        UnityEngine.Object obj = Resources.Load("Prefab/Item/Other/Paja_language_sample");
        if (null != obj)
        {
            GameObject pajaLanguage = Instantiate(obj) as GameObject;
            pajaLanguage.transform.position = objPos;
			pajaLanguage.name = "pajaLanguage";
			
			ItemDrop itemf = pajaLanguage.GetComponent<ItemDrop>();
            if (itemf == null)
                return null;
            if (PeGameMgr.IsSingle)
            {                
                itemf.AddItem(1508, 1);
            }
			else
			{
				itemf.SetNet(net);
				foreach (int iter in itemObjId)
				{
					if (ItemMgr.Instance.Get(iter) != null)
						itemf.AddItem(ItemMgr.Instance.Get(iter));
					else
						Debug.LogError("Paja_language item is null");
				}
			}
            languages.Add(itemf);
            return itemf;
		}
		return null;
	}
	
	public static ItemDrop CreateProbe(Vector3 objPos, List<int> itemObjId = null, MapObjNetwork net = null)
	{
		UnityEngine.Object obj = Resources.Load("Prefab/Item/Other/probe_michelson02");
        if (null != obj)
        {
            GameObject probe = Instantiate(obj) as GameObject;
            probe.transform.position = objPos;
            probe.name = "probe";

            ItemDrop itemf = probe.GetComponentInChildren<ItemDrop>();
            if (itemf != null)
            {
                if (PeGameMgr.IsSingleStory)
                    itemf.AddItem(1340, 1);
                else
                {
                    itemf.SetNet(net);
                    foreach (int iter in itemObjId)
                    {
                        if (ItemMgr.Instance.Get(iter) != null)
                            itemf.AddItem(ItemMgr.Instance.Get(iter));
                        else
                            Debug.LogError("probe item is null");
                    }
                }
                return itemf;
            }
        }
        return null;
    }
    public static ItemDrop CreateHugefish_bone(Vector3 objPos, List<int> itemObjId = null, MapObjNetwork net = null)
    {
        UnityEngine.Object obj = Resources.Load("Prefab/Item/Other/hugefish_bone");
        if (null == obj)
        {
            return null;
        }
        GameObject hugefish_bone = Instantiate(obj) as GameObject;
        hugefish_bone.transform.position = objPos;
        hugefish_bone.name = "hugefish_bone";

        ItemDrop itemf = hugefish_bone.GetComponentInChildren<ItemDrop>();
        if (itemf != null)
        {
            if (PeGameMgr.IsSingleStory)
                itemf.AddItem(1342, 1);
            else
            {
                itemf.SetNet(net);
                foreach (int iter in itemObjId)
                {
                    if (ItemMgr.Instance.Get(iter) != null)
                        itemf.AddItem(ItemMgr.Instance.Get(iter));
                    else
                        Debug.LogError("Hugefish_bone item is null");
                }
            }
            return itemf;
        }
        return null;
    }

    public static ItemDrop Createlarve_Q425(Vector3 objPos)
    {
        UnityEngine.Object obj = Resources.Load("Prefab/Item/Other/larve_Q425");
        if (null == obj)
            return null;

        GameObject larve_Q425 = Instantiate(obj) as GameObject;
        larve_Q425.transform.position = objPos;
        return null;
    }
    public static ItemDrop CreateFruitpack(Vector3 objPos)
    {
        UnityEngine.Object obj = Resources.Load("Prefab/Item/Other/iron_bed");
        if (obj != null)
        {
            GameObject fruitpack = Instantiate(obj) as GameObject;
            fruitpack.name = "fruitpack";
            fruitpack.transform.position = objPos;
        }
        return null;
    }
    public static ItemDrop CreateAsh_box(Vector3 objPos, int entityId, List<int> itemObjId = null, MapObjNetwork net = null)
    {
		PeEntity item = EntityMgr.Instance.Get(entityId); ;
        UnityEngine.Object obj = Resources.Load("Prefab/Item/Other/DropItem");
        if (null != obj)
        {
            ItemDrop itemf;
            List<int> failureMission = new List<int>();
            foreach (var item1 in MissionManager.Instance.m_PlayerMission.m_MissionInfo.Keys)
            {
                MissionCommonData data = MissionManager.GetMissionCommonData(item1);
                if (data.m_iReplyNpc == item.Id)
                    failureMission.Add(item1);
            }
            foreach (var item2 in failureMission)
                MissionManager.Instance.FailureMission(item2);

            if (!PeGameMgr.IsSingleStory)
            {
				GameObject ash_box = Instantiate(obj, objPos, Quaternion.identity) as GameObject;
                ash_box.name = "ashBox_Sphere";
                itemf = ash_box.GetComponent<ItemDrop>();
				if(itemf == null)
					itemf = ash_box.AddComponent<ItemDrop>();
                if (itemf != null) 
                {
                    itemf.SetNet(net);
//                     for (int i = 0; i < item.GetComponent<EquipmentCmpt>()._ItemList.Count; i++)
//                     {
//                         itemf.AddItem(item.GetComponent<EquipmentCmpt>()._ItemList[i]);
//                     }
                    foreach (int iter in itemObjId)
                    {
                        if (ItemMgr.Instance.Get(iter) != null)
                            itemf.AddItem(ItemMgr.Instance.Get(iter));
                        else
                            Debug.LogError("Ash_box item is null");
                    }
					if (item != null &&item.skEntity != null && item.skEntity._net != null && item.skEntity.IsController())
                        item.skEntity._net.RPCServer(EPacketType.PT_NPC_Destroy);
                }
            }
            else
            {
				if (item == null)
					return null;
                ItemBox iBox = ItemBoxMgr.Instance.AddItemSinglePlay(objPos);
                iBox.AddItem(ItemAsset.ItemMgr.Instance.CreateItem(1339));
                for (int i = 0; i < item.GetComponent<EquipmentCmpt>()._ItemList.Count; i++)
                    iBox.AddItem(item.GetComponent<EquipmentCmpt>()._ItemList[i]);

                //ItemDropMousePick itemNPC;
                //if (ash_box.GetComponent<ItemDropMousePick>() == null)
                //{
                //    itemNPC = ash_box.gameObject.AddComponent<ItemDropMousePick>();
                //}
                //else
                //    itemNPC = ash_box.GetComponent<ItemDropMousePick>();
                //for (int i = 0; i < item.GetComponent<EquipmentCmpt>()._ItemList.Count; i++)
                //{
                //    itemNPC.AddItem(item.GetComponent<EquipmentCmpt>()._ItemList[i].protoId, 1);
                //}
                //if (itemNPC.GetCountByProtoId(1339) <= 0)
                //    itemNPC.AddItem(1339, 1);
                itemf = null;
            }
			if(item != null)
                PeLogicGlobal.Instance.DestroyEntity(item.skEntity);
            return itemf;
        }
        return null;
    }

    public static ItemDrop CreateAsh_ball(Vector3 objPos, List<int> itemObjId = null, MapObjNetwork net = null)
    {
        UnityEngine.Object obj = Resources.Load("Prefab/Other/ItemBox");
        if (null == obj)
        {
            return null;
        }
        GameObject ash_ball = Instantiate(obj) as GameObject;
        ash_ball.transform.position = objPos;
        ItemDrop itemf = ash_ball.GetComponentInChildren<ItemDrop>();
        if (itemf == null)
        {
            itemf = ash_ball.gameObject.AddComponent<ItemDrop>();
        }
        if (itemf != null)
        {
            if (PeGameMgr.IsSingleStory)
			{
				// Do nothing
			}
            else
            {
                itemf.SetNet(net);
                foreach (int iter in itemObjId)
                {
                    if (ItemMgr.Instance.Get(iter) != null)
                        itemf.AddItem(ItemMgr.Instance.Get(iter));
                    else
                        Debug.LogError("Ash_ball item is null");
                }
            }
            return itemf;
        }
        return null;
    }

    GameObject mcTalk;
    GameObject Mctalk 
    {
        get 
        {
            if(mcTalk == null)
                mcTalk = GameObject.Find("McTalk");
            return mcTalk;
        } 
    }
    IEnumerator GameEnd() 
    {
        if (Mctalk != null) 
        {

            Vector3 startPos = Mctalk.transform.position;
            CutsceneClip clip = Mctalk.GetComponentInChildren<CutsceneClip>();
            if (clip != null)
                clip.testClip = true;
            clip.transform.SetParent(null, true);

            Transform[] trans = Mctalk.GetComponentsInChildren<Transform>(true);
            trans[1].SetParent(null, true);

            //lz-2018.01.31 保证最后飞船每个部件都打开了
            for (int i = 0; i < Mctalk.transform.childCount; i++)
            {
                Mctalk.transform.GetChild(i).gameObject.SetActive(true);
            }

            float dis = 0;
            if(PeGameMgr.IsSingle)
                PeCreature.Instance.mainPlayer.biologyViewCmpt.Fadeout();
            bool beginFoward = false;
            //int[] target = new int[] { 3, 5, 7, 11,13 };
            while (dis < 30)
            {
                if (!beginFoward)
                {
                    mcTalk.transform.position += Vector3.up * Time.deltaTime * 4f;
                    clip.transform.position += Vector3.up * Time.deltaTime * 4f;
                    //mcTalk.transform.eulerAngles = Vector3.Lerp(mcTalk.transform.eulerAngles, Vector3.forward, 0.1f);
                    //Quaternion q = Quaternion.Slerp(mcTalk.transform.rotation, new Quaternion(0, 0, 0, -1), 0.1f);
                    //mcTalk.transform.rotation = q;
                    if (dis > 20f)
                        beginFoward = true;
                }
                else
                {
                    mcTalk.transform.position += -mcTalk.transform.forward * Time.deltaTime * dis;
                }
                dis += Time.deltaTime;

                yield return 0;
            }

            mcTalk.gameObject.SetActive(false);

            if (PeGameMgr.IsMulti)
            {

                PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_GameEnd);
                //PlayerNetwork.mainPlayer.DestroySceneItem(startPos);
                MessageBox_N.ShowYNBox(PELocalization.GetString(8000621), null, PeSceneCtrl.Instance.GotoLobbyScene);

            }
            else
            {
                DestroyObject(mcTalk.gameObject);
                Application.LoadLevel("GameED");
            }
        }
    }

    public static List<string> deadNpcsName = new List<string>();
    public static List<ItemDrop> languages = new List<ItemDrop>();
    public bool enableBook = false;
    public bool moveVploas = false; //外星人NPC
    bool returnZjComplete = false;
    void ProcessSpecial(string str)
    {
        if (string.IsNullOrEmpty(str))
            return;
        string[] strArray = str.Split(';');
        if (strArray.Length == 0)
            return;
        string spe = strArray[0];
        if (spe == "Vploas_1")
        {
            PeEntity npc = EntityMgr.Instance.Get(9056);
            if (!CSMain.HasCSAssembly())
                return;
            if (CSMain.GetAssemblyLogic() != null)
                Translate(npc, CSMain.GetAssemblyLogic().m_NPCTrans[19].position);
            moveVploas = true;
        }
        else if (spe == "RecordMission")
        {
            MissionManager.Instance.m_PlayerMission.isRecordCreation = true;
        }
        else if (spe == "EnableBook")
            enableBook = true;
        else if (spe == "Mons")
        {
            string[] tmp = strArray[1].Split(',');
            foreach (var item in tmp)
            {
                string[] tmp1 = item.Split('_');
                if (tmp1.Length != 2)
                    continue;
                int protoid = Convert.ToInt32(tmp1[0]) == 0 ? Convert.ToInt32(tmp1[1]) : Convert.ToInt32(tmp1[1]) | Pathea.EntityProto.IdGrpMask;

                Vector3 v = Vector3.zero;
                if (CSMain.HasCSAssembly())
                    CSMain.GetAssemblyPos(out v);
                else if (Pathea.PeCreature.Instance.mainPlayer != null)
                    v = Pathea.PeCreature.Instance.mainPlayer.peTrans.position;
                v = AiUtil.GetRandomPositionInLand(v, 30f, 50f, 10f, LayerMask.GetMask("Default", "VFVoxelTerrain", "SceneStatic"), 30);
                MonsterEntityCreator.CreateMonster(protoid, v);
            }
        }
        else if (spe == "gameend")
        {
            StartCoroutine(GameEnd());
        }
        else if (spe == "McTalk")
        {
            string[] tmp = strArray[1].Split(',');
            if (Mctalk == null)
                return;
            foreach (var item in tmp)
            {
                int n = Convert.ToInt32(item);
                if (n >= 0 && n < Mctalk.transform.childCount)
                {
                    Mctalk.transform.GetChild(n).gameObject.SetActive(true);
                }
            }
        }
        else if (spe == "backpack")
        {
            Pathea.PlayerPackageCmpt pkg = Pathea.PeCreature.Instance.mainPlayer.GetCmpt<Pathea.PlayerPackageCmpt>();

            if (pkg.package.GetCount(1332) == 0 && strArray.Length > 1)
            {
                Vector3 objPos = Str2V3(strArray[1]);
                if (objPos == Vector3.zero)
                {
                    Debug.LogError("backpack pos invalid.");
                }
                if (PeGameMgr.IsMultiStory)
                {
                    PlayerNetwork.mainPlayer.CreateSceneItem("backpack", objPos, "1332,1");
                }
                else
                {
                    CreateBackpack(objPos);
                }
            }
        }
        else if (spe == "TextSample")
        {
            CreateLanguageSample();
        }
        else if (spe == "PujaTrade")
        {
            DetectedTownMgr.Instance.AddStoryCampByMission(TradeCampId.MISSION_NATIVE);
        }
        else if (spe == "EnableGerdy")
        {
            PeEntity npc = EntityMgr.Instance.Get(9008);
            npc.biologyViewCmpt.ActivateCollider(true);
            npc.motionMgr.FreezePhyState(GetType(), false);
        }
        else if (spe == "pajaLanguage")
        {
            //Pathea.PlayerPackageCmpt pkg = Pathea.PeCreature.Instance.mainPlayer.GetCmpt<Pathea.PlayerPackageCmpt>();

            if (strArray.Length > 1)
            {
                Vector3 objPos = Str2V3(strArray[1]);
                if (objPos == Vector3.zero)
                {
                    Debug.LogError("pajaLanguage pos invalid.");
                }
                else
                {
                    PlayerMission mp = MissionManager.Instance.m_PlayerMission;
                    if (mp.pajaLanguageBePickup == 0 ||
                        (SingleGameStory.curType == SingleGameStory.StoryScene.PajaShip && objPos.z < 9000 && mp.pajaLanguageBePickup != 1 && mp.pajaLanguageBePickup != 3) ||
                        (SingleGameStory.curType == SingleGameStory.StoryScene.LaunchCenter && objPos.z > 9000 && mp.pajaLanguageBePickup != 2 && mp.pajaLanguageBePickup != 3))
                    {
                        if (PeGameMgr.IsMultiStory)
                        {
                            Vector3 sppos = objPos;
                            sppos.y = sppos.y - 500;
                            //PlayerNetwork.MainPlayer.CreateSceneItem("pajaLanguage", objPos, "1508,1");
                            PlayerNetwork.mainPlayer.CreateSceneItem("pajaLanguage", sppos, "1508,1");
                        }
                        else
                        {
                            languages.Add(CreatePajaLanguage(objPos));
                        }
                    }
                }
            }
        }
        else if (spe == "probe")
        {
            Pathea.PlayerPackageCmpt pkg = Pathea.PeCreature.Instance.mainPlayer.GetCmpt<Pathea.PlayerPackageCmpt>();
            if (pkg.package.GetCount(1340) == 0 && strArray.Length > 1)
            {
                Vector3 objPos = Str2V3(strArray[1]);
                if (objPos == Vector3.zero)
                {
                    Debug.LogError("probe pos invalid.");
                }
                if (PeGameMgr.IsMultiStory)
                {
                    PlayerNetwork.mainPlayer.CreateSceneItem("probe", objPos, "1340,1");
                }
                else
                {
                    CreateProbe(objPos);
                }
            }
        }
        else if (spe == "hugefish_bone" && strArray.Length > 1)
        {
            Vector3 objPos = Str2V3(strArray[1]);
            if (objPos == Vector3.zero)
            {
                Debug.LogError("hugefish_bone pos invalid.");
            }
            if (PeGameMgr.IsMultiStory)
            {
                PlayerNetwork.mainPlayer.CreateSceneItem("hugefish_bone", objPos, "1342,1");
            }
            else
            {
                CreateHugefish_bone(objPos);
            }
        }
        else if (spe == "1_larve_Q425" && strArray.Length > 1)
        {
            Vector3 objPos = Str2V3(strArray[1]);
            if (objPos == Vector3.zero)
            {
                Debug.LogError("1_larve_Q425 pos invalid.");
            }
            if (PeGameMgr.IsMultiStory)
            {
                PlayerNetwork.mainPlayer.CreateSceneItem("1_larve_Q425", objPos, "");
            }
            else
            {
                Createlarve_Q425(objPos);
            }

        }
        else if (spe == "0_larve_Q425")
        {
            GameObject goSpe = GameObject.Find("larve_Q425(Clone)");
            if (goSpe == null)
                return;
            if (PeGameMgr.IsSingleStory)
            {
                Destroy(goSpe);
            }
            else
            {
                PlayerNetwork.mainPlayer.DestroySceneItem(goSpe.transform.position);
            }
        }
        else if (spe == "earthCamp")
        {
            CSCreator creator = CSMain.GetCreator(CSConst.ciDefMgCamp);
            if (creator == null)
                return;

            MapMaskData mmd = MapMaskData.s_tblMaskData.Find(ret => ret.mId == 22);
            if (mmd == null)
                return;

            foreach (PeEntity npc in EntityMgr.Instance.All)
            {
                if (npc == null)
                    continue;

                if (npc.IsRecruited())
                    continue;

                if (Vector3.Distance(mmd.mPosition, npc.position) > mmd.mRadius)
                    continue;

                if (npc.proto != EEntityProto.Npc && npc.proto != EEntityProto.RandomNpc)
                    continue;

                if (PeGameMgr.IsMulti)
                    MissionManager.Instance.SetGetTakeMission(MissionManager.m_SpecialMissionID16, npc, MissionManager.TakeMissionType.TakeMissionType_Get);
                else
                    creator.AddNpc(npc, true);
            }
        }
        else if (spe == "DrawCir_1")
        {
            GameObject gomc = GameObject.Find("MissionCylinder");
            if (gomc == null)
                return;

            GameObject.Destroy(gomc);
        }
        else if (spe == "npcdrive")
        {
            NpcDrive();
        }
        else if (spe == "playerlie")
        {
            MotionMgrCmpt mmc = PeCreature.Instance.mainPlayer.motionMgr;
            if (mmc == null)
                return;
            mmc.EndAction(PEActionType.Lie);
        }
        else if (spe == "npctobed")
        {
            if (CSMain.HasCSAssembly())
            {

            }
            //UnityEngine.Object obj = Resources.Load("Prefab/Item/Furniture/iron_bed");         //床的模型
        }
        else if (spe == "Fruit_pack" && strArray.Length > 1)
        {
            Vector3 objPos = Str2V3(strArray[1]);
            if (objPos == Vector3.zero)
            {
                Debug.LogError("fruitpack pos invalid.");
            }
            if (PeGameMgr.IsMultiStory)
            {
                PlayerNetwork.mainPlayer.CreateSceneItem("Fruit_pack", objPos, "");
            }
            else
            {
                CreateFruitpack(objPos);
            }
        }
        else if (spe == "Fruit_pack_1")
        {
            GameObject go = GameObject.Find("fruitpack");
            if (go == null)
                return;
            if (PeGameMgr.IsSingleStory)
            {
                GameObject.Destroy(go);
            }
            else
            {
                PlayerNetwork.mainPlayer.DestroySceneItem(go.transform.position);
            }
        }
        else if (spe == "closeviyus_22,23")
        {
            for (int i = 0; i < 4; i++)
            {
                DienManager.DoorClose(DienManager.doors[i]);
            }
            DienManager.doorsCanTrigger = false;
        }
        else if (spe == "openviyus_22,23")
        {
            DienManager.DoorOpen(DienManager.doors[4]);
            DienManager.doorsCanTrigger = true;
        }
        else if (spe == "openL1")
        {
            GameObject o = GameObject.Find("Epiphany_L1Outside195");
            if (o != null)
                o.SetActive(false);
        }
        else if (spe == "DestroyRail")
            DestroyRailWay();
        else if (spe == "GetVirus")
        {

        }
        else if (spe == "GetThruster")
        {

        }
        else if (spe == "dayuLowHP")
            StartCoroutine(WaitDayuLowHP());
        else if (spe == "dayuRunaway")
        {
            PeEntity dayu = SceneEntityCreatorArchiver.Instance.GetEntityByFixedSpId(94);
            if (null != dayu)
            {
                AnimatorCmpt dayuAni = dayu.GetComponent<AnimatorCmpt>();
                dayuAni.SetBool("jump", true);
                Invoke("DestroyDayu", 3f);
            }
        }
        else if (spe == "EnableReputation")
            ReputationSystem.Instance.ActiveReputation((int)PeCreature.Instance.mainPlayer.GetAttribute(AttribType.DefaultPlayerID));
        else if (spe == "Cartercycle")
        {
            VCEditor.MakeCreation("Isos/Mission/Cartercycle");
        }
        else if (spe == "copyisozj")
        {
            for (int i = 0; i < 3; i++)
                VCEditor.CopyCretion(ECreation.Aircraft);
        }
        else if (spe == "movezj")
        {
            if (PeGameMgr.IsSingle)
            {
                GameObject zj;
                for (int i = 0; i < MissionManager.Instance.m_PlayerMission.recordCreationName.Count; i++)
                {
                    zj = GameObject.Find(MissionManager.Instance.m_PlayerMission.recordCreationName[i]);
                    if (zj == null)
                        continue;
                    MissionManager.Instance.m_PlayerMission.recordCretionPos.Add(zj.transform.position);
                    UnityEngine.Object o = Resources.Load("Cutscene Clips/PathClip" + (i + 1));
                    if (o == null)
                        continue;
                    GameObject go = GameObject.Instantiate(o) as GameObject;
                    MoveByPath mbp = zj.AddComponent<MoveByPath>();
                    mbp.SetDurationDelay(15f, 0);
                    mbp.StartMove(go, WhiteCat.RotationMode.ConstantUp, WhiteCat.TweenMethod.Linear);
                    zj.GetComponent<Rigidbody>().useGravity = false;
                }
            }
            else
            {
                PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_MissionMoveAircraft, true);
            }
        }
        else if (spe == "returnzj")
        {
            if (PeGameMgr.IsSingle)
            {
                for (int i = 0; i < MissionManager.Instance.m_PlayerMission.recordCreationName.Count; i++)
                {
                    GameObject zj = GameObject.Find(MissionManager.Instance.m_PlayerMission.recordCreationName[i]);
                    if (zj == null)
                        continue;
                    UnityEngine.Object o = Resources.Load("Cutscene Clips/PathClip" + (i + 5));
                    if (o == null)
                        continue;
                    GameObject go = GameObject.Instantiate(o) as GameObject;
                    MoveByPath mbp = zj.AddComponent<MoveByPath>();
                    mbp.SetDurationDelay(15f, 0);
                    if (i >= MissionManager.Instance.m_PlayerMission.recordCretionPos.Count)
                        continue;
                    Vector3 recordPos = MissionManager.Instance.m_PlayerMission.recordCretionPos[i] + (Vector3.up * 20);
                    mbp.AddEndListener(delegate ()
                    {
                        returnZjComplete = true;
                        if (zj != null)
                        {
                            zj.transform.position = recordPos;
                            Rigidbody rigi = zj.GetComponent<Rigidbody>();
                            if(rigi != null)
                                rigi.useGravity = true;
                        }
                    });
                    GameUI.Instance.mUIWorldMap.CurMap.onTravel += delegate ()
                    {
                        if (zj != null && !returnZjComplete)
                        {
                            zj.transform.position = recordPos;
                            Rigidbody rigi = zj.GetComponent<Rigidbody>();
                            if(rigi != null)
                                rigi.useGravity = true;
                        }
                    };
                    mbp.StartMove(go, WhiteCat.RotationMode.ConstantUp, WhiteCat.TweenMethod.Linear);
                }
            }
            else
            {
                PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_MissionMoveAircraft, false);
            }
        }
        else if (spe == "MeatToMoney")
        {
            if (PeGameMgr.IsMulti)
            {
                PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_ChangeCurrency, CustomData.EMoneyType.Digital);
                return;
            }
            foreach (var item in EntityMgr.Instance.All)
            {
                if (item == null || item.gameObject == null)
                    continue;
                NpcPackageCmpt npcpc = item.GetCmpt<NpcPackageCmpt>();
                if (npcpc == null)
                    continue;
                npcpc.money.SetCur(npcpc.money.current * 4);
            }
            int meatCount = GameUI.Instance.playerMoney;
            Money.Digital = true;
            //lz-2018.1.16 肉数量转化为
            GameUI.Instance.playerMoney = meatCount * 4;
            GameUI.Instance.mShopWnd.mMeatSprite.gameObject.SetActive(false);
            GameUI.Instance.mShopWnd.mMoneySprite.gameObject.SetActive(true);
            GameUI.Instance.mItemPackageCtrl.nMoneyRoot.SetActive(true);

        }
        else if (spe.Length >= 6)
        {
            if (spe.Substring(0, 6) == "gotoCS")
            {
                Vector3 csPos;
                if (CSMain.GetAssemblyPos(out csPos))
                {
                    string laterPart = spe.Substring(7, (spe.Length - 7));
                    List<int> npcNum = new List<int>(Array.ConvertAll<string, int>(laterPart.Split(','), s => int.Parse(s)));
                    for (int i = 0; i < npcNum.Count; i++)
                    {
                        CSCreator creator = CSMain.GetCreator(CSConst.ciDefMgCamp);
                        if (creator == null)
                            return;

                        PeEntity npc = EntityMgr.Instance.Get(npcNum[i]);
                        if (npc != null)
                        {
                            npc.NpcCmpt.FixedPointPos = PETools.PEUtil.GetRandomPosition(csPos, 10, 30, true);
                            if (PeGameMgr.IsMulti)
                                MissionManager.Instance.SetGetTakeMission(MissionManager.m_SpecialMissionID16, npc, MissionManager.TakeMissionType.TakeMissionType_Get);
                            else
                                creator.AddNpc(npc, true);
                        }
                    }
                }
            }
            else if (spe.Substring(0, 7) == "moncall")
            {
                if (spe.Length > 7)
                {
                    string laterPart = spe.Substring(8, (spe.Length - 8));
                    List<string> monCallData = new List<string>(laterPart.Split('_', ';'));
                    int excuteNum = monCallData.Count / 3;
                    //                    List<int> tmp;
                    for (int i = 0; i < excuteNum; i++)
                    {
                        /*tmp = */
                        monCallData.GetRange(i * 3, 3).ConvertAll<int>(delegate (string n)
                                                                   {
                                                                       return Convert.ToInt32(n);
                                                                   });
                    }
                }

            }
            else if (spe.Substring(0, 7) == "npcdead")
            {
                StroyManager.deadNpcsName.Clear();
                string laterPart = spe.Substring(8, (spe.Length - 8));
                if (laterPart.Length <= 2)
                {
                    int deadNum = Convert.ToInt32(laterPart);
                    List<int> npcsIndex = new List<int>();
                    List<PeEntity> npcs = CSMain.GetCSRandomNpc();
                    if (npcs.Count != 0)
                    {
                        ServantLeaderCmpt servantCmpt = PeCreature.Instance.mainPlayer.GetComponent<ServantLeaderCmpt>();
                        NpcCmpt[] servants = servantCmpt.GetServants();

                        for (int j = 0; j < servants.Length; j++)
                        {
                            for (int i = 0; i < npcs.Count; i++)
                            {
                                if (servants[j] == npcs[i].NpcCmpt)
                                {
                                    npcs.RemoveAt(i);
                                    break;
                                }
                            }
                        }
                        
                        if (npcs.Count <= deadNum)
                        {
                            for (int i = 0; i < npcs.Count; i++)
                            {
                                bool pass = true;
                                for (int j = 0; j < servants.Length; j++)
                                {
                                    if (servants[j] == npcs[i].NpcCmpt)
                                    {
                                        pass = false;
                                        break;
                                    }
                                }
                                if(pass)
                                    npcsIndex.Add(i);
                            }
                        }
                        else
                        {
                            while (npcsIndex.Count < deadNum)
                            {
                                int num = UnityEngine.Random.Range(0, npcs.Count);
                                if (!npcsIndex.Contains(num))
                                    npcsIndex.Add(num);
                            }
                        }

                        for (int i = 0; i < npcsIndex.Count; i++)
                        {
                            if (!StroyManager.deadNpcsName.Contains(npcs[npcsIndex[i]].name))
                                StroyManager.deadNpcsName.Add(npcs[npcsIndex[i]].name.Substring(0, npcs[npcsIndex[i]].name.Length - 5));
                            if (npcs[npcsIndex[i]] != null)
                                CSMain.RemoveNpc(npcs[npcsIndex[i]]);

                            if (PeGameMgr.IsMultiStory)
                                PlayerNetwork.mainPlayer.CreateSceneItem("ash_box", npcs[npcsIndex[i]].position, "1339,1", npcs[npcsIndex[i]].Id);
                            else
                                CreateAsh_box(npcs[npcsIndex[i]].position, npcs[npcsIndex[i]].Id);
                        }
                    }
                }
                else
                {
                    List<int> npcNum = new List<int>(Array.ConvertAll<string, int>(laterPart.Split(','), s => int.Parse(s)));
                    PeEntity npc;
                    for (int i = 0; i < npcNum.Count; i++)
                    {
                        npc = EntityMgr.Instance.Get(npcNum[i]);
                        if (npc == null)
                            continue;
                        if (PeGameMgr.IsMultiStory)
                            PlayerNetwork.mainPlayer.CreateSceneItem("ash_box", npc.position, "1339,1", npc.Id);
                        else
                            CreateAsh_box(npc.position, npc.Id);

                        if (!StroyManager.deadNpcsName.Contains(npc.name))
                            StroyManager.deadNpcsName.Add(npc.name.Substring(0, npc.name.Length - 5));
                        CSMain.RemoveNpc(npc);
                    }
                }
            }
            if (spe.Length >= 10)
            {
                if (spe.Substring(0, 8) == "special_")
                {
                    string lastPart = spe.Substring(8, spe.Length - 8);
                    string[] tmp = lastPart.Split('_');
                    if (tmp.Length == 2)
                    {
                        int[][] info = new int[][] { new int[] { 603, 604, 605 }, new int[] { 926, 927, 928 }, new int[] { 937, 938, 939 } };
                        int missionNum = Convert.ToInt32(tmp[0]);
                        int bedNum = Convert.ToInt32(tmp[1]);
                        int index = !CSMain.HasCSAssembly() ? 0 : bedNum > CSMain.GetEmptyBedRoom() ? 1 : 2;
                        int missionID = info[missionNum - 1][index];

                        MissionCommonData data = MissionManager.GetMissionCommonData(missionID);
                        if (data == null)
                            return;
                        PeEntity npc = EntityMgr.Instance.Get(data.m_iNpc);
                        if (MissionRepository.HaveTalkOP(missionID))
                        {
                            GameUI.Instance.mNPCTalk.UpdateNpcTalkInfo(missionID, 1);
                            GameUI.Instance.mNPCTalk.NormalOrSP(0);
                            GameUI.Instance.mNPCTalk.PreShow();
                        }
                        else
                        {
                            MissionManager.Instance.SetGetTakeMission(missionID, npc, MissionManager.TakeMissionType.TakeMissionType_Get);
                        }
                    }
                }
                else if (spe.Substring(0, 10) == "SpiderWeb_")
                {
                    string laterPart = spe.Substring(10, (spe.Length - 10));
                    List<string> tmp = new List<string>(laterPart.Split('_'));
                    List<int> npcNum = new List<int>(Array.ConvertAll<string, int>(tmp[0].Split(','), s => Convert.ToInt32(s)));
                    if (tmp.Count == 2 && Convert.ToInt32(tmp[1]) == 1)
                    {
                        UnityEngine.Object obj = Resources.Load("Prefab/Item/Other/SpiderWeb");
                        if (null != obj)
                        {
                            for (int i = 0; i < npcNum.Count; i++)
                            {
                                if (npcNum[i] == 20000)
                                {
                                    if (PeCreature.Instance.mainPlayer.transform.FindChild("DummyTransform/spiderWeb") == null)
                                    {
                                        GameObject spiderWeb = Instantiate(obj) as GameObject;
                                        spiderWeb.transform.parent = PETools.PEUtil.GetChild(PeCreature.Instance.mainPlayer.transform, "DummyTransform");
                                        spiderWeb.transform.localPosition = Vector3.zero;
                                        spiderWeb.name = "spiderWeb";
                                    }
                                }
                                else
                                {
                                    if (EntityMgr.Instance.Get(npcNum[i]).gameObject.transform.FindChild("DummyTransform/spiderWeb") == null)
                                    {
                                        GameObject spiderWeb = Instantiate(obj) as GameObject;
                                        spiderWeb.transform.parent = PETools.PEUtil.GetChild(EntityMgr.Instance.Get(npcNum[i]).transform, "DummyTransform");
                                        spiderWeb.transform.localPosition = Vector3.zero;
                                        spiderWeb.name = "spiderWeb";
                                    }
                                }
                            }
                        }
                    }
                    if (tmp.Count == 2 && Convert.ToInt32(tmp[1]) == 0)
                    {
                        for (int i = 0; i < npcNum.Count; i++)
                        {
                            if (npcNum[i] == 20000)
                            {
                                Transform trans = PeCreature.Instance.mainPlayer.transform.FindChild("DummyTransform/spiderWeb");
                                GameObject o;
                                if (trans != null)
                                {
                                    o = trans.gameObject;
                                    if (o != null)
                                        GameObject.Destroy(o);
                                }
                            }
                            else
                            {
                                Transform trans = EntityMgr.Instance.Get(npcNum[i]).gameObject.transform.FindChild("DummyTransform/spiderWeb");
                                GameObject o;
                                if (trans != null)
                                {
                                    o = trans.gameObject;
                                    if (o != null)
                                        GameObject.Destroy(o);
                                }

                            }
                        }
                    }
                }
                else if (spe.Substring(0, 11) == "AndheraNest")
                {
                    for (int i = 0; i < 8; i++)
                        CreateAndHeraNest(i);
                }
                else if (spe.Substring(0, 12) == "changeshader")
                {
                    string laterPart = spe.Substring(12, (spe.Length - 12));
                    string[] tmp = laterPart.Split('_');
                    if (tmp.Length == 2)
                    {
                        bool active = Convert.ToInt32(tmp[0]) == 1 ? true : false;
                        PeEntity npc = EntityMgr.Instance.Get(Convert.ToInt32(tmp[1]));
                        if (null != npc)
                        {
                            if (active)
                            {
                                SkinnedMeshRenderer[] n = EntityMgr.Instance.Get(9029).GetComponentsInChildren<SkinnedMeshRenderer>();
                                if (n.Length < 2)
                                    return;
                                if (n[1].materials[0].name != "HidingWaveMat(Clone) (Instance)")
                                {
                                    record = n[1].materials;

                                    Material[] newMats = new Material[n[1].materials.Length];
                                    for (int j = 0; j < n[1].materials.Length; j++)
                                    {
                                        Texture t = n[1].materials[j].GetTexture(0);
                                        newMats[j] = Material.Instantiate(HidingMat);
                                        newMats[j].SetTexture("_SrcMap", t);
                                    }
                                    n[1].materials = newMats;
                                    AddChangingMaterial(n[1].materials);
                                }
                            }
                            else
                            {
                                SkinnedMeshRenderer[] n = EntityMgr.Instance.Get(9029).GetComponentsInChildren<SkinnedMeshRenderer>();
                                if (n.Length < 2)
                                    return;
                                AddChangingMaterial(n[1]);
                            }
                        }
                    }
                }
            }
        }
    }

    void DestroyDayu() 
    {
        PeEntity dayu = SceneEntityCreatorArchiver.Instance.GetEntityByFixedSpId(94);
        PeLogicGlobal.Instance.DestroyEntity(dayu.skEntity, 0f, 1f);
    }

    void MonCall(List<int> monCallData)
    {
        if (monCallData.Count != 3)
            return;
        //处理三位数据  定点/种类_ID（可多个）_0（恢复）/1（设置只威慑）
    }

    public void TestMeetingPos(int num)
    {
        foreach (var item in GetMeetingPosition(PeCreature.Instance.mainPlayer.position, num, 3.0f))
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.transform.position = item;
            GameObject.Destroy(go.GetComponent<SphereCollider>());
            go.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            go.name = "MeetingPos";
        }
    }

    public List<Vector3> GetMeetingPosition(Vector3 center, int num, float radius)
    {
        List<Vector3> result = new List<Vector3>();
        if (num == 1)
        {
            result.Add(center);
            return result;
        }
        float closeDistance = Mathf.Sin(Mathf.PI / num) * radius * 2;
        int n = 0;
        while (result.Count < num)
        {
            n++;
            Vector3 tmp = GetHorizontalDir();
            bool isClose = false;
            foreach (var item in result)
            {
                if (Vector3.Distance(item, center + Vector3.up + tmp * radius) < closeDistance * 0.5f)
                {
                    isClose = true;
                    break;
                }
            }
            if (isClose)
                continue;
            if (!Physics.Raycast(center + Vector3.up, tmp + Vector3.up * 0.5f, radius))
            {
                if (Physics.Raycast(center + (Vector3.up * radius / 2) + tmp * radius, Vector3.down, 3))
                    result.Add(center + Vector3.up + tmp * radius);
            }
            if (n >= 100)
                break;
        }
        while (result.Count < num)
        {
            n++;
            for (int i = 0; i < result.Count; i++)
            {
                result.Add((center + Vector3.up + result[i]) / 2);
                if (result.Count >= num)
                    break;
            }
            if (n >= 105)
                break;
        }
        while (result.Count < num)
        {
            Vector3 tmp = GetHorizontalDir();
            bool isClose = false;
            foreach (var item in result)
            {
                if (Vector3.Distance(item, center + Vector3.up + tmp * radius) < closeDistance * 0.5f)
                {
                    isClose = true;
                    break;
                }
            }
            if (isClose)
                continue;
            result.Add(center + Vector3.up + tmp * radius);
        }
        return result;
    }

    Vector3 GetHorizontalDir()
    {
        float r = UnityEngine.Random.Range(0f, 2 * Mathf.PI);
        return new Vector3(Mathf.Sin(r), 0f, Mathf.Cos(r));
    }

    #region PathInfo

    public void ResetPathIdx()
    {
        //m_CurPathIdx = 0;
        m_iCurPathMap.Clear();
    }

    public int GetPathIdx(List<Vector3> pathList)
    {
        if (pathList.Count == 0)
            return 0;

        Vector2 v1 = new Vector2(PeCreature.Instance.mainPlayer.position.x, PeCreature.Instance.mainPlayer.position.z);
        Vector2 v2 = new Vector2(pathList[0].x, pathList[0].z);
        float dis = Vector2.Distance(v1, v2);
        float disTmp = dis;
        int idx = 0;
        //�ҳ������Ҿ�����С�ĵ�
        for (int i = 1; i < pathList.Count; i++)
        {
            v2 = new Vector2(pathList[i].x, pathList[i].z);
            dis = Vector2.Distance(v1, v2);
            if (dis < disTmp)
            {
                disTmp = dis;
                idx = i;
            }
        }

        //�ٱȽ��������õ����յ��ľ���
        if (idx >= pathList.Count - 1)
            return idx;

        v2 = new Vector2(pathList[idx].x, pathList[idx].z);
        Vector2 v3 = new Vector2(pathList[pathList.Count - 1].x, pathList[pathList.Count - 1].z);
        dis = Vector2.Distance(v1, v3);
        disTmp = Vector2.Distance(v2, v3);

        if (dis <= disTmp)
            return idx + 1;

        return idx;
    }

    public PathInfo GetPath(int id, bool bChangePos, PeEntity npc)
    {
        PathInfo pathInfo;
        pathInfo.pos = Vector3.zero;
        pathInfo.isFinish = true;

        if (id == m_CurMissionID && !bChangePos && m_iCurPathMap.ContainsKey(npc.Id))
        {
            pathInfo.isFinish = false;
            pathInfo.pos = m_iCurPathMap[npc.Id].curPos;
            return pathInfo;
        }

        //int pathidx = 0;

        //if (npc != null && m_iCurPathMap.ContainsKey(npc.Id))
        //    pathidx = m_iCurPathMap[npc.Id].idx;
        //else
        //    pathidx = m_CurPathIdx;

        //if (pathidx > m_CurPathList.Count - 1)
            return pathInfo;

//        m_CurMissionID = id;
        //pathInfo.isFinish = false;
        ////pathInfo.pos = m_CurPathList[pathidx];
        ////curPathPos = pathInfo.pos;
        //pathidx++;

        //CurPathInfo curPath = new CurPathInfo();
        //curPath.idx = pathidx;
        //curPath.curPos = pathInfo.pos;

        //if (m_iCurPathMap.ContainsKey(npc.Id))
        //    m_iCurPathMap[npc.Id] = curPath;
        //else
        //    m_iCurPathMap.Add(npc.Id, curPath);

        //return pathInfo;
    }

    #endregion

    #region NpcInfo
    public int CreateMissionRandomNpc(Vector3 pos, int num)
    {
        //Vector3 center = Vector3.zero;

        //Vector2 idxPos = GetIdxPos(pos, out center);
        //if(m_CreatedNpcList.ContainsKey(idxPos))
        //{
        //    if(m_CreatedNpcList[idxPos].Count >= CreateNpcNum)
        //        return m_CreatedNpcList[idxPos][0];
        //}

        //List<Vector3> listPos = new List<Vector3>();
        //pos = GetPatrolPoint(pos, true);
        //listPos.Add(pos);
        //List<int> npcList = CreateAdRandomNpc(listPos, num);
        //if(m_CreatedNpcList.ContainsKey(idxPos))
        //{
        //    for(int i=0; i<npcList.Count; i++)
        //        m_CreatedNpcList[idxPos].Add(npcList[i]);
        //}
        //else
        //{
        //    m_CreatedNpcList.Add(idxPos, npcList);
        //}

        //if(npcList.Count > 1)
        //{
        //    int idx = UnityEngine.Random.Range(0, npcList.Count);
        //    return npcList[idx];
        //}

        //return npcList[0];
        return 0;
    }

    //public PeEntity CreateRandomNpc(int npcid, Vector3 pos)
    //{
    //    if (!NpcMissionDataRepository.m_AdRandMisNpcData.ContainsKey(npcid))
    //        return null;

    //    AdNpcData data = NpcMissionDataRepository.m_AdRandMisNpcData[npcid];

    //    PeEntity entity = PeCreature.Instance.CreateRandomNpc(data.mRnpc_ID, data.mID);

    //    if (null == entity)
    //        return null;

    //    NpcMissionData useData = new NpcMissionData();
    //    useData.m_bRandomNpc = true;
    //    useData.m_Rnpc_ID = data.mRnpc_ID;
    //    useData.m_QCID = data.mQC_ID;
    //    PeTrans view = entity.peTrans;
    //    if (null == view)
    //    {
    //        Debug.LogError("entity has no ViewCmpt");
    //        return null;
    //    }

    //    view.position = pos;
    //    entity.SetBirthPos(pos);//delete npc need
    //    int misid = GetRandomMission(data.mQC_ID, useData.m_CurMissionGroup);
    //    if (misid != 0)
    //        useData.m_RandomMission = misid;
    //    //useData.m_RandomMission = 9009;

    //    for (int i = 0; i < data.m_CSRecruitMissionList.Count; i++)
    //        useData.m_CSRecruitMissionList.Add(data.m_CSRecruitMissionList[i]);

    //    NpcMissionDataRepository.AddMissionData(data.mID, useData);

    //    entity.SetUserData(useData);
    //    PeMousePickCmpt pmp = entity.GetCmpt<PeMousePickCmpt>();
    //    if (pmp == null)
    //        return null;

    //    pmp.mousePick.eventor.Subscribe(EntityCreateMgr.Instance.NpcMouseEventHandler);

    //    return entity;
    //}

    public void NpcTakeMission(int Rnpcid, int Qcid, Vector3 pos, /*NpcRandom npcRandom,*/ List<int> csrecruit = null)
    {
        //if(npcRandom == null)
        //    return ;

        //NpcMissionData useData = new NpcMissionData();
        //useData.m_Rnpc_ID = Rnpcid;
        //useData.m_QCID = Qcid;
        //npcRandom.SetBirthPos(pos);
        //int misid = GetRandomMission(Qcid, useData.m_CurMissionGroup);
        //if(misid != 0)
        //    useData.m_RandomMission = misid;

        //if (csrecruit != null)
        //{
        //    for (int i = 0; i < csrecruit.Count; i++)
        //        useData.m_CSRecruitMissionList.Add(csrecruit[i]);
        //}

        //NpcMissionDataRepository.AddMissionData(npcRandom.mNpcId, useData);

        ////npcRandom.MouseCtrl.MouseEvent.SubscribeEvent(NpcMouseEventHandler);
        //npcRandom.UserData = useData;
        ////SetNpcShopIcon(npcRandom);
    }

    //public int GetRandomMission(int qcid, int groupidx)
    //{
    //    AdRandomGroup agi = AdRMRepository.GetAdRandomGroup(qcid);
    //    if(agi == null)
    //        return 0;

    //    if(!agi.m_GroupList.ContainsKey(groupidx))
    //        return 0;

    //    List<GroupInfo> giList = agi.m_GroupList[groupidx];

    //    if (giList.Count == 0)
    //        return 0;

    //    int count = giList[giList.Count - 1].radius;

    //    int idx = UnityEngine.Random.Range(0, count);

    //    for(int i=0; i<giList.Count; i++)
    //    {
    //        GroupInfo gi = giList[i];
    //        if(idx < gi.radius)
    //            return gi.id;
    //    }

    //    return 0;
    //}

    //public int GetRandomMission(NpcMissionData missionData)
    //{
    //    AdRandomGroup agi = AdRMRepository.GetAdRandomGroup(missionData.m_QCID);
    //    if(agi == null)
    //        return 0;
    //    else
    //    {
    //        if(missionData.m_CurMissionGroup > 11)
    //        {
    //            missionData.m_CurGroupTimes++;
    //            if(!agi.m_GroupList.ContainsKey(missionData.m_CurMissionGroup))
    //                missionData.m_CurMissionGroup = -1;

    //            if(agi.m_FinishTimes == 0)
    //                missionData.m_CurMissionGroup = -1;

    //            if(agi.m_FinishTimes == -1)
    //                missionData.m_CurMissionGroup = 1;

    //            if(agi.m_FinishTimes > 0)
    //            {
    //                if(agi.m_FinishTimes > missionData.m_CurGroupTimes)
    //                    missionData.m_CurMissionGroup = 1;
    //                else
    //                    missionData.m_CurMissionGroup = -1;
    //            }
    //        }

    //        if(missionData.m_CurMissionGroup != -1)
    //            return GetRandomMission(missionData.m_QCID, missionData.m_CurMissionGroup);

    //        return 0;
    //    }
    //}

    //void InitRandomNpc()
    //{
    //    foreach (KeyValuePair<int, NpcMissionData> iter in NpcMissionDataRepository.dicMissionData)
    //    {
    //        if (iter.Value.m_Rnpc_ID == -1)
    //            continue;

    //        PeEntity entity = PeCreature.Instance.CreateRandomNpc(iter.Value.m_Rnpc_ID, iter.Key);
    //        if (null == entity)
    //        {
    //            Debug.LogError("create monster with path:" + PeCreature.MonsterPrefabPath);
    //            return;
    //        }

    //        PeTrans view = entity.peTrans;

    //        if (null == view)
    //        {
    //            Debug.LogError("entity has no ViewCmpt");
    //            return;
    //        }

    //        PeTrans playerView = PeCreature.Instance.mainPlayer.peTrans;
    //        view.position = iter.Value.m_Pos;

    //        //view.position = playerView.position + Vector3.left * 5 + Vector3.up * 3;
    //        PeMousePickCmpt pmp = entity.GetCmpt<PeMousePickCmpt>();
    //        if (pmp == null)
    //            continue;

    //        pmp.mousePick.eventor.Subscribe(NpcMouseEventHandler);
    //        entity.SetUserData(iter.Value);
    //        SetNpcShopIcon(entity);

    //        //NpcRandom npcRandom = NpcManager.Instance.CreateRandomNpc(iter.Key, iter.Value.m_Rnpc_ID, iter.Value.m_Pos);

    //        //if (null != npcRandom)
    //        //{
    //        //    npcRandom.MouseCtrl.MouseEvent.SubscribeEvent(NpcMouseEventHandler);
    //        //    npcRandom.UserData = iter.Value;
    //        //    SetNpcShopIcon(npcRandom);
    //        //}
    //    }
    //}

    //void InitDefaultNpc()
    //{
    //    Mono.Data.SqliteClient.SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable("NPC");
    //    while (reader.Read())
    //    {
    //        int id = Convert.ToInt32(reader.GetString(reader.GetOrdinal("NPC_ID")));

    //        PeEntity entity = PeCreature.Instance.CreateNpc(id, 1);
    //        if (entity == null)
    //            continue;

    //        InitNpcWithDb(entity);
    //        PeMousePickCmpt pmp = entity.GetCmpt<PeMousePickCmpt>();
    //        if (pmp == null)
    //            continue;

    //        pmp.mousePick.eventor.Subscribe(NpcMouseEventHandler);
    //        NpcMissionData nmd = NpcMissionDataRepository.GetMissionData(entity.Id);
    //        entity.SetUserData(nmd);
    //        SetNpcShopIcon(entity);
    //    }
    //}

    //bool InitNpcWithDb(PeEntity entity)
    //{
    //    int id = entity.Id;

    //    Mono.Data.SqliteClient.SqliteDataReader reader = LocalDatabase.Instance.SelectWhereSingle("npc", "*", "NPC_ID", " = ", "'" + id + "'");

    //    if (!reader.Read())
    //    {
    //        Debug.LogError("no npc[" + id + "] found in db");
    //        return false;
    //    }

    //    string npcName = reader.GetString(reader.GetOrdinal("NPC_name"));
    //    string showName = reader.GetString(reader.GetOrdinal("NPC_showname"));
    //    CharacterName characterName = new CharacterName(npcName, showName, CharacterName.DefaultFamilyName);
    //    string npcIcon = reader.GetString(reader.GetOrdinal("NPC_Icon"));
    //    string npcmode = reader.GetString(reader.GetOrdinal("NPC_Model"));

    //    EntityInfoCmpt info = entity.GetCmpt<EntityInfoCmpt>();
    //    if (null != info)
    //    {
    //        info.Name = characterName;
    //        info.FaceIcon = npcIcon;
    //    }

    //    string strTemp = reader.GetString(reader.GetOrdinal("startpoint"));
    //    string[] pos = strTemp.Split(',');
    //    if (pos.Length < 3)
    //    {
    //        Debug.LogError("Npc's StartPoint is Error");
    //    }
    //    else
    //    {
    //        float x = System.Convert.ToSingle(pos[0]);
    //        float y = System.Convert.ToSingle(pos[1]);
    //        float z = System.Convert.ToSingle(pos[2]);

    //        PeTrans view = entity.peTrans;
    //        if (null != view)
    //        {
    //            view.position = new Vector3(x, y, z);
    //        }
    //    }

    //    SetModelData(entity, npcmode);

    //    SetNpcMoney(entity, reader.GetString(reader.GetOrdinal("money")));

    //    return true;
    //}

    //public struct Min_Max_Int
    //{
    //    public int m_Min;
    //    public int m_Max;

    //    public int Random()
    //    {
    //        return UnityEngine.Random.Range(m_Min, m_Max);
    //    }
    //}

    //public void SetNpcMoney(PeEntity entity, string text)
    //{
    //    NpcPackageCmpt pkg = entity.GetCmpt<NpcPackageCmpt>();

    //    string[] groupStrArray = text.Split(';');
    //    if (groupStrArray.Length != 3)
    //    {
    //        return;
    //    }

    //    string[] strArray = groupStrArray[0].Split(',');
    //    if (strArray.Length != 2)
    //    {
    //        return;
    //    }

    //    Min_Max_Int initValue;
    //    if (!int.TryParse(strArray[0], out initValue.m_Min))
    //    {
    //        return;
    //    }
    //    if (!int.TryParse(strArray[1], out initValue.m_Max))
    //    {
    //        return;
    //    }

    //    strArray = groupStrArray[1].Split(',');
    //    if (strArray.Length != 2)
    //    {
    //        return;
    //    }

    //    Min_Max_Int incValue;
    //    if (!int.TryParse(strArray[0], out incValue.m_Min))
    //    {
    //        return;
    //    }
    //    if (!int.TryParse(strArray[1], out incValue.m_Max))
    //    {
    //        return;
    //    }

    //    int max = 0;
    //    if (!int.TryParse(groupStrArray[2], out max))
    //    {
    //        return;
    //    }

    //    pkg.InitAutoIncreaseMoney(max, incValue.Random());

    //    pkg.money.current = initValue.Random();
    //}

    //void SetModelData(PeEntity entity, string name)
    //{
    //    AvatarCmpt avatar = entity.GetCmpt<AvatarCmpt>();

    //    CustomCharactor.AvatarData nudeAvatarData = new CustomCharactor.AvatarData();

    //    nudeAvatarData.SetPart(CustomCharactor.AvatarData.ESlot.HairF, "Model/Npc/" + name);

    //    avatar.SetData(new AppearBlendShape.AppearData(), nudeAvatarData);
    //}

    //public void SetNpcShopIcon(PeEntity npc)
    //{
    //    string icon = StoreRepository.GetStoreNpcIcon(npc.Id);
    //    if (icon == "0")
    //        return;

    //    npc.SetShopIcon(icon);
    //}

    void SetRandomNpcInitState()
    {
        PeEntity npcRandom = EntityMgr.Instance.Get(3);
        if (npcRandom != null)
        {
            npcRandom.SetInjuredLevel(0.9f);
        }

        npcRandom = EntityMgr.Instance.Get(13);
        if (npcRandom != null)
        {
            npcRandom.SetInjuredLevel(1f);
        }

        npcRandom = EntityMgr.Instance.Get(14);
        if (npcRandom != null)
        {
            npcRandom.SetAiActive(false);
        }

        npcRandom = EntityMgr.Instance.Get(15);
        if (npcRandom != null)
        {
            npcRandom.SetInvincible(false);
            npcRandom.ApplyDamage(1000000f);
        }

        npcRandom = EntityMgr.Instance.Get(16);
        if (npcRandom != null)
        {
            npcRandom.SetInvincible(false);
            npcRandom.ApplyDamage(1000000f);
        }
    }

    void AddNpcItem(int npcid, int itemid)
    {
        if (GameConfig.IsMultiMode)
            return;

        PeEntity npc = EntityMgr.Instance.Get(npcid);
        if (npc == null)
            return;

        if (!EntityCreateMgr.Instance.IsRandomNpc(npc))
            return;

        ItemObject item;
        int count = npc.GetBagItemCount();
        if (count > 0)
        {
            for (int i = 0; i < count; i++)
            {
                item = npc.GetBagItem(i);
                if (item == null)
                    continue;

                if (item.protoId == itemid)
                    break;

                item = ItemMgr.Instance.CreateItem(itemid); // unknown
                npc.AddToBag(item);
                break;
            }
        }
        else
        {
            item = ItemMgr.Instance.CreateItem(itemid); // unknown
            npc.AddToBag(item);
        }
    }

    public Vector3 GetNpcPos(int npcid)
    {
        PeEntity npc = EntityMgr.Instance.Get(npcid);

        if (npc == null)
            return Vector3.zero;

        return npc.position;
    }

    public Vector3 GetPlayerPos()
    {
        if (m_PlayerTrans == null)
            m_PlayerTrans = PeCreature.Instance.mainPlayer.peTrans;

        if (m_PlayerTrans == null)
            return Vector3.zero;

        return m_PlayerTrans.position;
    }

    public PeEntity GetNpc(int npcid)
    {
        PeEntity npc = EntityMgr.Instance.Get(npcid);

        return npc;
    }

    public bool IsZeroPoint(Vector3 npcPos)
    {
        Vector2 pos = new Vector2(npcPos.x, npcPos.z);
        Vector2 pos1 = new Vector2(0, 0);
        if (Vector2.Distance(pos, pos1) < 5)
            return true;

        return false;
    }

    //public bool IsRandomNpc(PeEntity npc)
    //{
    //    if (npc == null)
    //        return false;

    //    NpcMissionData missionData = npc.GetUserData() as NpcMissionData;
    //    if (missionData == null)
    //        return false;

    //    return missionData.m_bRandomNpc;
    //}

    #endregion
    public void PauseAll(PeEntity npc) 
    {
        NpcCmpt npccmpt = npc.NpcCmpt;
        if (npccmpt == null)
            return;

        npccmpt.Req_PauseAll();
    }

    public void MoveTo(PeEntity npc, Vector3 pos, float radius = 1, bool bForce = true, SpeedState ss = SpeedState.Walk)
    {
		if(npc == null)
			return;
        NpcCmpt npccmpt = npc.NpcCmpt;
        if (npccmpt == null)
            return;

        npccmpt.Req_MoveToPosition(pos, radius, bForce, ss);
    }

    public void TalkMoveTo(PeEntity npc, Vector3 pos, float radius = 1, bool bForce = true ,SpeedState ss = SpeedState.Walk) 
    {
        if (npc == null)
            return;
        NpcCmpt npccmpt = npc.NpcCmpt;
        if (npccmpt == null)
            return;

        npccmpt.Req_TalkMoveToPosition(pos, radius, bForce, ss);
    }

    public void MoveToByPath(PeEntity npc, Vector3[] posList, SpeedState state = SpeedState.Run)
    {
        NpcCmpt npccmpt = npc.NpcCmpt;
        if (npccmpt == null)
            return;

		if(npc.target != null && npc.target.GetAttackEnemy() != null)
			npc.target.ClearEnemy();

        npccmpt.Req_FollowPath(posList, false, state);
    }

    public void Translate(PeEntity npc, Vector3 position)
    {
        NpcCmpt npccmpt = npc.NpcCmpt;
        if (npccmpt == null)
            return;

        npccmpt.Req_Translate(position,false);
    }

    public void FollowTarget(PeEntity npc, Transform trans)
    {
        NpcCmpt npccmpt = npc.NpcCmpt;
        if (npccmpt == null)
            return;


		if(npc.target != null && npc.target.GetAttackEnemy() != null)
			npc.target.ClearEnemy();

        npccmpt.Req_FollowTarget(npc.Id,Vector3.zero,0,0.0f);
    }

    public void FollowTarget(PeEntity npc, int targetId, Vector3 pos, int dirTargetid, float radius, bool send = true)
    {
        NpcCmpt npccmpt = npc.NpcCmpt;
        if (npccmpt == null)
            return;

        if (npc.target != null && npc.target.GetAttackEnemy() != null)
            npc.target.ClearEnemy();

        npccmpt.Req_FollowTarget(targetId,pos,dirTargetid,radius, false, send);
    }

    public void FollowTarget(List<int> npcs, int targetId)
    {
        foreach(var id in npcs)
        {
            PeEntity npc = EntityMgr.Instance.Get(id);
            NpcCmpt npccmpt = npc.NpcCmpt;
            if (npccmpt == null)
                return;

            if (npc.target != null && npc.target.GetAttackEnemy() != null)
                npc.target.ClearEnemy();
        }
        if(PeGameMgr.IsMulti)
        {
            PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_NPC_RequestAiOp, (int)EReqType.FollowTarget, targetId, npcs.ToArray());
        }
        
    }

    public void CarryUp(PeEntity npc, int carryedNpc, bool bCarryUp)
    {
        NpcCmpt npccmpt = npc.NpcCmpt;
        if (npccmpt == null)
            return;

        npccmpt.Req_Salvation(carryedNpc, bCarryUp);
    }

    public void RemoveReq(PeEntity npc, EReqType type)
    {
        NpcCmpt npccmpt = npc.NpcCmpt;
        if (npccmpt == null)
            return;

        npccmpt.Req_Remove(type);
    }

    public void SetIdle(PeEntity npc, string anim)
    {
        NpcCmpt npccmpt = npc.NpcCmpt;
        if (npccmpt == null)
            return;

        npccmpt.Req_SetIdle(anim);
    }


    public void SetRotation(PeEntity npc, Quaternion qua)
    {
        NpcCmpt npccmpt = npc.NpcCmpt;
        if (npccmpt == null)
            return;

        Motion_Move mm = npc.motionMove;
        if (mm == null)
            return;

        mm.Stop();

        npccmpt.Req_Rotation(qua);
    }

    public void SetTalking(PeEntity npc, string RqAction = "", object npcidOrVecter3 = null)
    {
        NpcCmpt npccmpt = npc.NpcCmpt;
        if (npccmpt == null)
            return;

        if (npcidOrVecter3 != null)
        {
            if (npcidOrVecter3 is int) 
            {
                if ((int)npcidOrVecter3 == 0 || EntityMgr.Instance.Get((int)npcidOrVecter3) == null)
                    npccmpt.Req_Dialogue(RqAction, null);
                else
                {
                    PeTrans trans = EntityMgr.Instance.Get((int)npcidOrVecter3).peTrans;
                    if (trans != null)
                        npccmpt.Req_Dialogue(RqAction, trans);
                }
            }
            else if (npcidOrVecter3 is Vector3)
            {
                npccmpt.Req_Dialogue(RqAction, (Vector3)npcidOrVecter3);
            }
        }
        else
            npccmpt.Req_Dialogue(RqAction);
    }

    public AudioController PlaySound(PeEntity npc, int audioid)
    {
        if (npc == null)
            return null;

        return AudioManager.instance.Create(npc.ExtGetPos(), audioid);
    }

    void NpcDrive()
    {
        Vector3 conoly;
        if (!CSMain.GetAssemblyPos(out conoly))
            return;

        PeEntity npc = EntityMgr.Instance.Get(9001);
        if (npc == null)
            return;

        PeEntity npc1 = EntityMgr.Instance.Get(9021);
        if (npc1 == null)
            return;

        PeEntity npc2 = EntityMgr.Instance.Get(9033);
        if (npc2 == null)
            return;

        PeMap.StaticPoint camp = PeMap.StaticPoint.Mgr.Instance.Find(e => e.campId == 7 ? true : false);
        if (camp == null)
            return;

        Vector3[] driveposlist = new Vector3[4];
        for (int i = 0; i < 3; i++)
            driveposlist[i] = AiUtil.GetRandomPosition(conoly, 20, 50, 100.0f, AiUtil.groundedLayer, 10);

        driveposlist[3] = camp.position;

        MoveToByPath(npc, driveposlist, SpeedState.Walk);

        Vector3[] offset1 = new Vector3[4];
        Vector3[] offset2 = new Vector3[4];
        Vector3 offset;
        for (int i = 0; i < driveposlist.Length - 1; i++)
        {
            offset = Vector3.Cross(driveposlist[i + 1] - driveposlist[i], Vector3.up).normalized;
            offset1[i] = driveposlist[i] + offset;
            offset2[i] = driveposlist[i] - offset;
        }
        offset1[3] = camp.position;
        offset2[3] = camp.position;

        MoveToByPath(npc1, offset1, SpeedState.Walk);
        MoveToByPath(npc2, offset2, SpeedState.Walk);
    }

    public int GetMgCampNpcCount()
    {
        int count = 0;
        CSCreator creator = CSMain.GetCreator(CSConst.ciDefMgCamp);
        if (creator == null)
            return 0;

        MapMaskData mmd = MapMaskData.s_tblMaskData.Find(ret => ret.mId == 22);
        if (mmd == null)
            return 0;

        foreach (PeEntity npc in EntityMgr.Instance.All)
        {
            if (npc == null)
                continue;

            if (npc.IsRecruited())
                continue;

            if (npc.proto != EEntityProto.Npc && npc.proto != EEntityProto.RandomNpc)
                continue;

            if (Vector3.Distance(mmd.mPosition, npc.position) > mmd.mRadius)
                continue;

            count++;
        }

        return count;
    }

    public int ParseIDByColony(int id)
    {
        MissionCommonData data = MissionManager.GetMissionCommonData(id);
        if (data == null)
            return 0;

        if (data.m_ColonyMis[0] == 0)
            return id;

        if (data.m_iColonyNpcList.Count == 0)
            return id;

        for (int i = 0; i < data.m_iColonyNpcList.Count; i++)
        {
            PeEntity npc = EntityMgr.Instance.Get(data.m_iColonyNpcList[i]);
            if (npc == null)
                continue;

//            CSPersonnelObject cspo = npc.GetGameObject().GetComponent<CSPersonnelObject>();
//            if (cspo == null)
			if(!CSMain.IsColonyNpc(npc.Id))
                return data.m_ColonyMis[1];
        }

        return data.m_ColonyMis[0];
    }

    public void PlayerGetOnTrain(int index)
    {
        MissionCommonData data = MissionManager.GetMissionCommonData(MissionManager.m_SpecialMissionID68);

        if (data == null)
            return;

        PeEntity npc;
        for (int i = 0; i < data.m_TargetIDList.Count; i++)
        {
            TypeFollowData folData = MissionManager.GetTypeFollowData(data.m_TargetIDList[i]);
            if (folData == null)
                continue;

            for (int m = 0; m < folData.m_iNpcList.Count; m++)
            {
                npc = EntityMgr.Instance.Get(folData.m_iNpcList[m]);
                npc.GetOnTrain(index, false);
            }
        }
    }

    public void PlayerGetOffTrain(Vector3 pos)
    {
        MissionCommonData data = MissionManager.GetMissionCommonData(MissionManager.m_SpecialMissionID68);

        if (data == null)
            return;

        PeEntity npc;
        for (int i = 0; i < data.m_TargetIDList.Count; i++)
        {
            TypeFollowData folData = MissionManager.GetTypeFollowData(data.m_TargetIDList[i]);
            if (folData == null)
                continue;

            for (int m = 0; m < folData.m_iNpcList.Count; m++)
            {
                npc = EntityMgr.Instance.Get(folData.m_iNpcList[m]);
                npc.GetOffTrain(Vector3.zero);
            }
        }

        if (MissionManager.Instance.HadCompleteMission(MissionManager.m_SpecialMissionID68))
            GlobalEvent.OnPlayerGetOffTrain -= PlayerGetOffTrain;
    }


    #region EntityEvent
    public void InitPlayerEvent()
    {
        PlayerPackageCmpt pkg = PeCreature.Instance.mainPlayer.GetCmpt<PlayerPackageCmpt>();
        if (pkg == null)
            return;

        pkg.package._playerPak.changeEventor.Subscribe(PlayerItemEventHandler);
        PlayerPackage._missionPak.changeEventor.Subscribe(PlayerItemEventHandler);

        DraggingMgr.Instance.eventor.Subscribe(PlayerDragItemEventHandler);
        SkEntitySubTerrain.Instance.AddListener(TreeCutDownListen);

        UseItemCmpt uic = PeCreature.Instance.mainPlayer.GetCmpt<UseItemCmpt>();
        if (uic == null)
            return;

        uic.eventor.Subscribe(PlayerUseItemEventHandler);
        PEBuildingMan.Self.onVoxelMotify += new PEBuildingMan.DVoxelModifyNotify(PlayerBuildingEventHandler);
        MainPlayerCmpt.gMainPlayer.onEquipmentAttack += new Action<int>(PlayerUseEquipmentHandler);
    }

    void TreeCutDownListen(SkillSystem.SkEntity skEntity, GlobalTreeInfo tree) 
    {
        PeEntity entity = skEntity.GetComponent<PeEntity>();
        if (entity == null)
            return;
        if (Pathea.PeGameMgr.IsStory)
			TreeCutDown(entity.position, tree._treeInfo, tree.WorldPos);
        else if (Pathea.PeGameMgr.IsAdventure)
			TreeCutDown(entity.position, tree._treeInfo, tree._treeInfo.m_pos);
    }

    public void TreeCutDown(Vector3 casterPosition, TreeInfo tree, Vector3 worldPos)
    {
        if (!tree.IsTree())
            return;

        GameObject o = null;
        if (Pathea.PeGameMgr.IsStory)
            o = LSubTerrainMgr.Instance.GlobalPrototypePrefabList[tree.m_protoTypeIdx];
        else if (Pathea.PeGameMgr.IsAdventure)
            o = RSubTerrainMgr.Instance.GlobalPrototypePrefabList[tree.m_protoTypeIdx];
        if (null == o)
            return;

        o = Instantiate(o);
        o.name = "CutDownTree";
        o.transform.position = worldPos;
        o.transform.rotation = Quaternion.identity;
        o.transform.localScale = new Vector3(tree.m_widthScale, tree.m_heightScale, tree.m_widthScale);

        Collider[] cols = o.GetComponents<Collider>();
        if (cols != null)
        {
            foreach (var item in cols)
                GameObject.Destroy(item);
        }

        Bounds bound;
        if (Pathea.PeGameMgr.IsStory)
            bound = LSubTerrainMgr.Instance.GlobalPrototypeBounds[tree.m_protoTypeIdx];
        else if (Pathea.PeGameMgr.IsAdventure)
            bound = RSubTerrainMgr.Instance.GlobalPrototypeBounds[tree.m_protoTypeIdx];

        Vector3[] footsPos;
        if (tree.IsDoubleFoot(out footsPos, worldPos, o.transform.localScale))
			o.AddComponent<TreeCutDown>().SetDirection(casterPosition, bound.center.y + bound.extents.y, bound.extents.x, footsPos);
        else
			o.AddComponent<TreeCutDown>().SetDirection(casterPosition, bound.center.y + bound.extents.y, bound.extents.x);

    }

    //public void NpcMouseEventHandler(object sender, MousePickablePeEntity.RMouseClickEvent e)
    //{
    //    PeEntity npc = e.entity;
    //    if (npc == null)
    //        return;

    //    float dist = Vector3.Distance(npc.position, GetPlayerPos());
    //    if (dist > 7)
    //        return;

    //    if (StroyManager.Instance.IsRandomNpc(npc) && npc.IsDead())
    //    {
    //        if (npc.Id == 9203 || npc.Id == 9204)
    //            return;

    //        if (npc.Id == 9214 || npc.Id == 9215)
    //        {
    //            if (!MissionManager.Instance.HasMission(MissionManager.m_SpecialMissionID10))
    //                return;
    //        }

    //        if (GameConfig.IsMultiMode)
    //        {
    //            //if (null != PlayerFactory.mMainPlayer)
    //            //    PlayerFactory.mMainPlayer.RequestDeadObjItem(npc.OwnerView);
    //        }
    //        else
    //        {
    //            //if (GameUI.Instance.mItemGetGui.UpdateItem(npc))
    //            //{
    //            //    GameUI.Instance.mItemGetGui.Show();
    //            //}
    //        }

    //        if (npc.IsRecruited())
    //        {
    //            GameUI.Instance.mReviveGui.ShowServantRevive(npc);
    //        }

    //        return;
    //    }


    //    if (StroyManager.Instance.IsRandomNpc(npc) && npc.IsFollower())
    //        return;

    //    if (!npc.GetTalkEnable())
    //        return;

    //    if (StroyManager.Instance.IsRandomNpc(npc) && !npc.IsDead())
    //    {
    //        //������������
    //        NpcMissionData missionData = npc.GetUserData() as NpcMissionData;
    //        if (null != missionData)
    //        {
    //            if (!MissionManager.Instance.HasMission(missionData.m_RandomMission))
    //            {
    //                if (PeGameMgr.IsStory)
    //                    RMRepository.CreateRandomMission(missionData.m_RandomMission);
    //                //else if (GameConfig.GameModeNoMask == GameConfig.EGameMode.Multiplayer_Adventure
    //                //     || GameConfig.GameModeNoMask == GameConfig.EGameMode.Multiplayer_Build)
    //                //    PlayerFactory.mMainPlayer.RPCServer(EPacketType.PT_InGame_NewMission, missionData.m_RandomMission);
    //                else
    //                    AdRMRepository.CreateRandomMission(missionData.m_RandomMission);
    //            }
    //        }
    //        else
    //        {
    //            Debug.LogError("û�� NpcMissionData");
    //        }
    //    }

    //    GameUI.Instance.mNPCTalk.SetCurSelNpc(npc);
    //    GameUI.Instance.mNPCTalk.Show();
    //}
    public int ItemClassIdtoProtoId(int itemClassId) 
    {
        int result = 0;
        WhiteCat.CreationItemClass type = WhiteCat.CreationHelper.GetCreationItemClass(itemClassId);
        switch (type)
        {
            case WhiteCat.CreationItemClass.None:
                break;
            case WhiteCat.CreationItemClass.Sword:
                result = 1322;
                break;
            case WhiteCat.CreationItemClass.Bow:
                break;
            case WhiteCat.CreationItemClass.Axe:
                break;
            case WhiteCat.CreationItemClass.Shield:
                break;
            case WhiteCat.CreationItemClass.HandGun:
                result = 1323;
                break;
            case WhiteCat.CreationItemClass.Rifle:
                result = 1324;
                break;
            case WhiteCat.CreationItemClass.Vehicle:
                result = 1328;
                break;
            case WhiteCat.CreationItemClass.Aircraft:
                result = 1330;
                break;
            case WhiteCat.CreationItemClass.Boat:
                break;
            case WhiteCat.CreationItemClass.SimpleObject:
                result = 1542;
                break;
            case WhiteCat.CreationItemClass.Armor:
                break;
            case WhiteCat.CreationItemClass.Robot:
                break;
            case WhiteCat.CreationItemClass.AITurret:
                break;
            default:
                break;
        }
        return result;
    }

    public void PlayerItemEventHandler(object sender, ItemPackage.EventArg e)
    {
        ItemPackage.EventArg.Op type = e.op;
        PlayerPackageCmpt pkg = PeCreature.Instance.mainPlayer.GetCmpt<Pathea.PlayerPackageCmpt>();
        if (pkg == null)
            return;

        ItemObject io = e.itemObj;
        if (io == null)
            return;

        if (e.itemObj.protoId == 1508)
        {
            PlayerMission pm = MissionManager.Instance.m_PlayerMission;
            if (SingleGameStory.curType == SingleGameStory.StoryScene.PajaShip)
                pm.pajaLanguageBePickup = pm.pajaLanguageBePickup == 0 ? 1 : pm.pajaLanguageBePickup == 2 ? 3 : pm.pajaLanguageBePickup;
            else if (SingleGameStory.curType == SingleGameStory.StoryScene.LaunchCenter)
                pm.pajaLanguageBePickup = pm.pajaLanguageBePickup == 0 ? 2 : pm.pajaLanguageBePickup == 1 ? 3 : pm.pajaLanguageBePickup; ;
        }

        if (io.protoId == 1541)
        {
            int index = -1;
            foreach (var item in MissionManager.Instance.m_PlayerMission.textSamples)
            {
                if (Vector3.Distance(PeCreature.Instance.mainPlayer.position, item.Value) < 50)
                {
                    index = item.Key;
                    break;
                }
            }
            if (index != -1)
            {                
                GameObject go = GameObject.Find("language_sample_canUse(Clone):" + index.ToString());
                if (go != null)
                {
                    if (PeGameMgr.IsSingle)
                    {
                        MissionManager.Instance.m_PlayerMission.textSamples.Remove(index);
                        Destroy(go);
                    }
                    else
                    {
                        PlayerNetwork.mainPlayer.DestroySceneItem(go.transform.position);
                    }      
                }
            }
        }

        if (io.protoId == 1332)
        {
            
            GameObject go = GameObject.Find("backpack");
            if (go != null)
            {
                if (PeGameMgr.IsSingle)
                {
                    Destroy(go);
                    if (GameUI.Instance.mItemGet != null)
                        GameUI.Instance.mItemGet.Hide();
                }
                else
                {
                    PlayerNetwork.mainPlayer.DestroySceneItem(go.transform.position);
                }
            }            
        }

        if (io.protoId == 1508)
        {            
            if (PeGameMgr.IsSingle)
            {
                //lz-2016.06.29 Find 会存在返回null的情况，不可以直接访问.gameObject;
                //GameObject go = StroyManager.languages.Find(delegate (ItemDrop item)
                //{
                //    if (item == null)
                //        return false;
                //    float dist = Vector3.Distance(item.gameObject.transform.position, PeCreature.Instance.mainPlayer.position);
                //    if (dist < 10)
                //        return true;
                //    return false;
                //}).gameObject;

                ItemDrop itemDrop = StroyManager.languages.Find((item)=>
                {
                    if (item == null)
                        return false;
                    float dist = Vector3.Distance(item.gameObject.transform.position, PeCreature.Instance.mainPlayer.position);
                    if (dist < 10)
                        return true;
                    return false;
                });

                if (null!=itemDrop && null!=itemDrop.gameObject)
                {
                    Destroy(itemDrop.gameObject);
                    if (GameUI.Instance.mItemGet != null)
                        GameUI.Instance.mItemGet.Hide();
                }
            }
            else
            {
                //PlayerNetwork.MainPlayer.DestroySceneItem(go.transform.position);
            }
        }

        if (io.protoId == 1340)
        {
            GameObject go = GameObject.Find("probe");
            if (go != null)
            {
                Destroy(go);
                if (GameUI.Instance.mItemGet != null)
                    GameUI.Instance.mItemGet.Hide();
            }
        }

        if (io.protoId == 1342)
        {
            GameObject go = GameObject.Find("hugefish_bone");
            if (go != null)
            {
                Destroy(go);
                if (GameUI.Instance.mItemGet != null)
                    GameUI.Instance.mItemGet.Hide();
            }
        }

        if (type == ItemPackage.EventArg.Op.Put)
        {
            if (io.protoData.itemClassId > 0) 
            {
                int n = ItemClassIdtoProtoId(io.protoData.itemClassId);
                if (n == 0)
                    MissionManager.Instance.ProcessCollectMissionByID(io.protoId);
                else
                    MissionManager.Instance.ProcessCollectMissionByID(n);
            }
            else
                MissionManager.Instance.ProcessCollectMissionByID(io.protoId);

            if (io.protoId == 1339 && KillNPC.ashBox_inScene > 0)
                KillNPC.ashBox_inScene--;
        }
    }

    public void PlayerBuildingEventHandler(IntVector3[] indexes, BSVoxel[] voxels, BSVoxel[] oldvoxels, EBSBrushMode mode, IBSDataSource d)
    {
        if (d != BuildingMan.Blocks)
            return;
        if (mode == EBSBrushMode.Add)
        {
            for (int i = 0; i < indexes.Length; i++)
            {
                Vector3 v = new Vector3(indexes[i].x * d.Scale, indexes[i].y * d.Scale, indexes[i].z * d.Scale);
                MissionManager.Instance.ProcessUseItemMissionByID(PEBuildingMan.GetBlockItemProtoID(voxels[i].materialType), v);
            }
        }
        else
        {
            for (int i = 0; i < indexes.Length; i++)
            {
                Vector3 v = new Vector3(indexes[i].x * d.Scale, indexes[i].y * d.Scale, indexes[i].z * d.Scale);
                MissionManager.Instance.ProcessUseItemMissionByID(PEBuildingMan.GetBlockItemProtoID(oldvoxels[i].materialType), v, -1);
            }
        }
    }

    public void PlayerDragItemEventHandler(object sender, DraggingMgr.EventArg e)
    {
        ItemObjDragging items = e.dragable as ItemObjDragging;
        if (items == null)
            return;

        PlayerPackageCmpt pkg = PeCreature.Instance.mainPlayer.GetCmpt<Pathea.PlayerPackageCmpt>();
        if (pkg == null)
            return;
        if (!PeGameMgr.IsMulti)
            MissionManager.Instance.ProcessUseItemMissionByID(items.GetItemProtoId(), items.GetPos(), 1, items.GetItemDrag().itemObj);        
    }

    public void PlayerUseItemEventHandler(object sender, UseItemCmpt.EventArg e)
    {
        ItemObject items = e.itemObj;
        if (items == null)
            return;

        MissionManager.Instance.ProcessUseItemMissionByID(items.protoId, GetPlayerPos());
        MissionManager.Instance.ProcessCollectMissionByID(items.protoId);
    }

    public void PlayerUseEquipmentHandler(int protoId) 
    {
        MissionManager.Instance.ProcessUseItemMissionByID(protoId, GetPlayerPos());
        MissionManager.Instance.ProcessCollectMissionByID(protoId);
    }
    #endregion

    #region EntityManager
    //public PeEntity CreateMonster(IntVec3 pos)
    //{
    //    int mapid = PeMappingMgr.Instance.GetAiSpawn(new Vector2(pos.x, pos.z));

    //    int id = WorldInfoMgr.Instance.FetchNonRecordAutoId();

    //    PeEntity entity = PeEntityCreator.Instance.CreateMonster(id, 1);
    //    if (null == entity)
    //    {
    //        Debug.LogError("create monster error");
    //        return null;
    //    }

    //    PeTrans view = entity.peTrans;

    //    if (null == view)
    //    {
    //        Debug.LogError("entity has no ViewCmpt");
    //        return null;
    //    }

    //    view.position = new Vector3(pos.x, pos.y, pos.z);
    //    return entity;
    //}

    //public bool GetMapCreateCenterPosList(int MaxEntityNum, int Radius, ref List<IntVec2> indexPos)
    //{
    //    if (PeCreature.Instance == null) return false;
    //    if (PeCreature.Instance.mainPlayer == null) return false;

    //    Vector3 playerpos = PeCreature.Instance.mainPlayer.ExtGetPos();
    //    int centerX, centerZ, oldX, oldZ, off;
    //    if (playerpos.x > 0)
    //        off = Radius;
    //    else
    //        off = Radius * -1;
    //    centerX = (int)(playerpos.x + off) / (Radius * 2);

    //    if (playerpos.z > 0)
    //        off = Radius;
    //    else
    //        off = Radius * -1;
    //    centerZ = (int)(playerpos.z + off) / (Radius * 2);

    //    oldX = centerX;
    //    oldZ = centerZ;

    //    for (int i = -1; i < 2; i++)
    //    {
    //        centerX = oldX + i;
    //        for (int j = -1; j < 2; j++)
    //        {
    //            centerZ = oldZ + j;
    //            IntVec2 idxPos = new IntVec2(centerX, centerZ);
    //            Vector3 centeridxpos = new Vector3(centerX * (Radius * 2), 0, centerZ * (Radius * 2));

    //            if (m_StoryEntityMgr.ContainsKey(idxPos))
    //                continue;

    //            StoryEntityMgr sem = new StoryEntityMgr();
    //            sem.m_IdxPos = idxPos;
    //            for (int m = 0; m < MaxEntityNum; m++)
    //            {
    //                IntVec3 targetpos = GetPatrolPoint(centeridxpos, false);
    //                sem.m_RandomCreatePosMap.Add(targetpos);
    //            }

    //            m_StoryEntityMgr.Add(idxPos, sem);
    //            if (!indexPos.Contains(idxPos))
    //                indexPos.Add(idxPos);
    //        }
    //    }

    //    return indexPos.Count > 0;
    //}

    //void CreateMonsterMgr(IntVec3 pos, StoryEntityMgr sem)
    //{
    //    if (NetworkInterface.IsClient)
    //    {
    //        PlayerNetwork.MainPlayer.RequestCreateAi(1, new IntVector2(sem.m_IdxPos.x, sem.m_IdxPos.y), new IntVector3(pos.x, pos.y, pos.z));
    //        return;
    //    }

    //    PeEntity entity = CreateMonster(pos);
    //    if (entity == null)
    //        return;

    //    if (sem.m_CreatedEntityMap.ContainsKey(EntityType.EntityType_Monster))
    //    {
    //        sem.m_CreatedEntityMap[EntityType.EntityType_Monster].Add(pos, entity.Id);
    //    }
    //    else
    //    {
    //        Dictionary<IntVec3, int> tmpDic = new Dictionary<IntVec3, int>();
    //        tmpDic.Add(pos, entity.Id);
    //    }
    //}

    //void CreateAdRandomNpcMgr(int adnpcid, IntVec3 npcPos, StoryEntityMgr sem)
    //{
    //    AdNpcData data = NpcMissionDataRepository.GetAdNpcData(adnpcid);
    //    if (data == null)
    //        return;

    //    if (NetworkInterface.IsClient)
    //    {
    //        PlayerNetwork.MainPlayer.RequestCreateNpc(adnpcid, new IntVector2(sem.m_IdxPos.x, sem.m_IdxPos.y), new IntVector3(npcPos.x, npcPos.y, npcPos.z));
    //        return;
    //    }

    //    PeEntity entity = CreateRandomNpc(data.mID, new Vector3(npcPos.x, npcPos.y, npcPos.z));
    //    if (entity == null)
    //        return;

    //    if (sem.m_CreatedEntityMap.ContainsKey(EntityType.EntityType_Npc))
    //    {
    //        sem.m_CreatedEntityMap[EntityType.EntityType_Npc].Add(npcPos, entity.Id);
    //    }
    //    else
    //    {
    //        Dictionary<IntVec3, int> tmpDic = new Dictionary<IntVec3, int>();
    //        tmpDic.Add(npcPos, entity.Id);
    //    }
    //}

    //public void CreateEntity(int NpcNum, int monsterNum, int Radius)
    //{
    //    List<IntVec2> idxPos = new List<IntVec2>();
    //    bool havePoint = GetMapCreateCenterPosList(NpcNum + monsterNum, Radius, ref idxPos);
    //    if (!havePoint)
    //        return;

    //    if (NetworkInterface.IsClient && null != PlayerNetwork.MainPlayer)
    //    {
    //        byte[] binPos = PETools.Serialize.Export((w) =>
    //            {
    //                w.Write(NpcNum);
    //                w.Write(monsterNum);
    //                w.Write(idxPos.Count);

    //                foreach (IntVec2 pos in idxPos)
    //                {
    //                    w.Write(pos.x);
    //                    w.Write(pos.y);
    //                }
    //            });

    //        PlayerNetwork.MainPlayer.SyncSpawnPos(binPos);
    //    }

    //    for (int i = 0; i < idxPos.Count; i++)
    //    {
    //        if (!m_StoryEntityMgr.ContainsKey(idxPos[i]))
    //            continue;

    //        StoryEntityMgr sem = m_StoryEntityMgr[idxPos[i]];
    //        if (sem == null)
    //            continue;

    //        List<IntVec3> createPos = new List<IntVec3>();

    //        for (int j = 0; j < sem.m_RandomCreatePosMap.Count; j++)
    //        {
    //            createPos.Add(sem.m_RandomCreatePosMap[j]);
    //        }

    //        List<int> adnpcList = NpcMissionDataRepository.GetAdRandListByWild(1);
    //        for (int j = 0; j < NpcNum; j++)
    //        {
    //            int idx = UnityEngine.Random.Range(1, adnpcList.Count);
    //            int idxpos = UnityEngine.Random.Range(0, createPos.Count);
    //            CreateAdRandomNpcMgr(adnpcList[idx], createPos[idxpos], sem);

    //            createPos.RemoveAt(idxpos);
    //        }

    //        for (int j = 0; j < monsterNum; j++)
    //        {
    //            int idxpos = UnityEngine.Random.Range(0, createPos.Count);
    //            CreateMonsterMgr(createPos[idxpos], sem);
    //            createPos.RemoveAt(idxpos);
    //        }
    //    }
    //}

    public int GetPosEntityCount(Vector3 pos, int Radius)
    {
        int count = 0;
        int centerX, centerZ, off;
        if (pos.x > 0)
            off = Radius;
        else
            off = Radius * -1;
        centerX = (int)(pos.x + off) / (Radius * 2);

        if (pos.z > 0)
            off = Radius;
        else
            off = Radius * -1;
        centerZ = (int)(pos.z + off) / (Radius * 2);
        Vector2 idxPos = new Vector2(centerX, centerZ);
        if (!m_CreatedNpcList.ContainsKey(idxPos))
            return 0;

//        List<Vector3> mapPosList = m_CreatedNpcList[idxPos];


        return count;
    }

    //public IntVec3 GetPatrolPoint(Vector3 center, bool bCheck = true)
    //{
    //    Vector2 v2 = UnityEngine.Random.insideUnitCircle;

    //    while (true)
    //    {
    //        v2 = v2.normalized * UnityEngine.Random.Range(-400, 400);
    //        Vector3 pos = center + new Vector3(v2.x, 0.0f, v2.y);

    //        IntVector2 iv = new IntVector2((int)pos.x, (int)pos.z);
    //        bool bTmp = false;
    //        pos.y = VFDataRTGen.GetPosTop(iv, out bTmp);

    //        if (pos.y > -1.01f && pos.y < -0.99f)
    //            continue;

    //        if (!bTmp)
    //            continue;


    //        return new IntVec3(pos.x, pos.y, pos.z);
    //    }

    //}

    public Vector3 GetPatrolPoint(Vector3 center, int iMin, int iMax, bool bCheck = true)
    {
        Vector2 v2 = UnityEngine.Random.insideUnitCircle;

        while (true)
        {
            v2 = v2.normalized * UnityEngine.Random.Range(iMin, iMax);
            Vector3 pos = center + new Vector3(v2.x, 0.0f, v2.y);

            IntVector2 iv = new IntVector2((int)pos.x, (int)pos.z);
            bool bTmp = false;
            pos.y = VFDataRTGen.GetPosTop(iv, out bTmp);

            if (pos.y > -1.01f && pos.y < -0.99f)
                continue;

            return pos;
        }
    }

    #endregion

    void UpdateCamp()
    {
        CampPatrolData cpData;
        bool bPass = true;
        for (int i = 0; i < m_NeedCampTalk.Count; i++)
        {
            cpData = CampPatrolData.GetCampData(m_NeedCampTalk[i]);
            if (cpData == null)
                continue;

            float dist = Vector3.Distance(GetPlayerPos(), cpData.mPosition);
            if (dist > cpData.mRadius)
                continue;

            for (int j = 0; j < cpData.m_PreLimit.Count; j++)
            {
                if (cpData.m_PreLimit[j] == 0)
                    continue;

                if (cpData.m_PreLimit[j] < 1000)
                {
                    if (!MissionManager.Instance.HadCompleteMission(cpData.m_PreLimit[j]))
                    {
                        bPass = false;
                        break;
                    }
                }
                else
                    if (!MissionManager.Instance.HadCompleteTarget(cpData.m_PreLimit[j]))
                    {
                        bPass = false;
                        break;
                    }
            }

            if (!bPass)
                continue;

            for (int j = 0; j < cpData.m_TalkList.Count; j++)
                GameUI.Instance.mServantTalk.AddTalk(cpData.m_TalkList[j]);

            m_NeedCampTalk.Remove(m_NeedCampTalk[i]);
        }
    }

    void UpdateUIWindow()
    {
        if (PeCreature.Instance.mainPlayer == null || GameUI.Instance == null)
            return;

        if (GameUI.Instance.mNpcWnd.isShow && GameUI.Instance.mNpcWnd.m_CurSelNpc != null)
        {
            float dis = Vector3.Distance(EntityCreateMgr.Instance.GetPlayerPos(), GameUI.Instance.mNpcWnd.m_CurSelNpc.ExtGetPos());
            if (dis < 8)
                return;

            GameUI.Instance.mNpcWnd.Hide();
        }
        else if (GameUI.Instance.mShopWnd.isShow && GameUI.Instance.mNpcWnd.m_CurSelNpc != null)
        {
            float dis = Vector3.Distance(EntityCreateMgr.Instance.GetPlayerPos(), GameUI.Instance.mNpcWnd.m_CurSelNpc.ExtGetPos());
            if (dis < 8)
                return;

            GameUI.Instance.mShopWnd.Hide();
        }
    }
}
