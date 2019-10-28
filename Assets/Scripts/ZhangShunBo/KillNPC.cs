using UnityEngine;
using System.Collections;
using Mono.Data.SqliteClient;
using System.Collections.Generic;
using Pathea;
using System.IO;

public class KillNPC
{
    public static List<PeEntity> NPCBeKilled(int npcNum)
    {
        List<PeEntity> npcs = new List<PeEntity>();
        if (npcNum == 25001 && ServantLeaderCmpt.Instance.GetServant(0) != null)
            npcs.Add(ServantLeaderCmpt.Instance.GetServant(0).GetComponent<PeEntity>());
        else if (npcNum == 25002 && ServantLeaderCmpt.Instance.GetServant(1) != null)
            npcs.Add(ServantLeaderCmpt.Instance.GetServant(1).GetComponent<PeEntity>());
        else if (npcNum == 25010)
        {
            if (ServantLeaderCmpt.Instance.GetServant(0) != null)
                npcs.Add(ServantLeaderCmpt.Instance.GetServant(0).GetComponent<PeEntity>());
            if (ServantLeaderCmpt.Instance.GetServant(1) != null)
                npcs.Add(ServantLeaderCmpt.Instance.GetServant(1).GetComponent<PeEntity>());
        }
        else
            npcs.Add(EntityMgr.Instance.Get(npcNum));
        foreach (var item in npcs)
        {
            if (item == null)
                continue;
            CSMain.RemoveNpc(item);
            item.SetAttribute(AttribType.Hp, 0, false);
            List<int> failureMission = new List<int>();
            foreach (var item1 in MissionManager.Instance.m_PlayerMission.m_MissionInfo.Keys)
            {
                MissionCommonData data = MissionManager.GetMissionCommonData(item1);
                if (data.m_iReplyNpc == item.Id)
                    failureMission.Add(item1);
            }
            foreach (var item2 in failureMission)
                MissionManager.Instance.FailureMission(item2);
            if (!MissionManager.Instance.m_PlayerMission.recordNpcName.ContainsKey(item.Id))
                MissionManager.Instance.m_PlayerMission.recordNpcName.Add(item.Id, item.name.Substring(0, item.name.Length - 1 - System.Convert.ToString(item.Id).Length));
        }
        return npcs;
    }

    public static void NPCaddItem(List<PeEntity> npcs, int itemProtoID, int itemNum)
    {
        foreach (var item in npcs)
        {
            if (item == null)
                continue;
            if (PeGameMgr.IsMultiStory)
            {
                PlayerNetwork.mainPlayer.CreateSceneItem("ash_ball", item.position, "1339,1", item.Id);
				if (item.aliveEntity != null && item.aliveEntity._net != null && (item.aliveEntity.IsController() || !item.aliveEntity._net.hasAuth))
                    item.aliveEntity._net.RPCServer(EPacketType.PT_NPC_Destroy);
            }
            else
            {
                //ItemDropPeEntity itemNPC;
                //if (item.GetComponent<ItemDropPeEntity>() == null)
                //{
                //    itemNPC = item.gameObject.AddComponent<ItemDropPeEntity>();
                //}
                //else
                //    itemNPC = item.GetComponent<ItemDropPeEntity>();

                //if (itemNPC.GetCountByProtoId(1339) <= 0)
                //    itemNPC.AddItem(itemProtoID, itemNum);
                NPCaddItem(item.Id, itemProtoID, itemNum);
            }
        }
    }

    public static void NPCaddItem(int npcid,int itemProtoID,int itemNum) 
    {
        PeEntity npc = EntityMgr.Instance.Get(npcid);
        if (npc == null)
            return;
        
        ClickPEentityLootItem clickPEentity;
        clickPEentity = npc.gameObject.AddComponent<ClickPEentityLootItem>();
        clickPEentity.AddItem(itemProtoID, itemNum);
        clickPEentity.onClick += RemoveRecordItem;
        if(MissionManager.Instance.m_PlayerMission.recordKillNpcItem.Find(ite => ite[0] == npcid) == null)
            MissionManager.Instance.m_PlayerMission.recordKillNpcItem.Add(new int[] { npc.Id, itemProtoID, itemNum });
    }

    static void RemoveRecordItem(int npcId) 
    {
        int[] tmp = MissionManager.Instance.m_PlayerMission.recordKillNpcItem.Find(ite => ite[0] == npcId);
        if (tmp != null)
            MissionManager.Instance.m_PlayerMission.recordKillNpcItem.Remove(tmp);
    }

    public static int ashBox_inScene; //存的是场景里骨灰盒数量
    public static bool isHaveAsh_inScene()
    {
        if (ashBox_inScene == 0)
            return false;
        else
        {
            if(go == null)
                go = DragArticleAgent.Root.gameObject;
            return true;
        }
    }

    public static bool IsBurried(Vector3 pos,out Vector3 upPos) 
    {
        upPos = Vector3.zero;
        int num = 0;
        RaycastHit hit = new RaycastHit();
        Vector3 origin, direction;
        //上方判断
        origin = pos + 4 * Vector3.up;
        Physics.Raycast(origin, pos - origin, out hit, 4, LayerMask.GetMask("Unwalkable", "VFVoxelTerrain"));
        if (hit.collider == null || hit.collider.gameObject.layer != Pathea.Layer.VFVoxelTerrain)
            return false;
        upPos = hit.point;
        //前后左右的判断
        for (int i = 0; i < 4; i++)
        {
            direction = i == 0 ? Vector3.forward : i == 1 ? Vector3.back : i == 2 ? Vector3.left : Vector3.right;
            origin = pos + 3 * direction + Vector3.up;
            Physics.Raycast(origin, pos - origin, out hit, 4, LayerMask.GetMask("Unwalkable", "VFVoxelTerrain"));
            if (hit.collider != null)
            {
                if (hit.collider.gameObject.layer == Pathea.Layer.VFVoxelTerrain)
                    num++;
                else
                {
                    origin = pos + 3 * direction + 5 * Vector3.up;
                    Physics.Raycast(origin, Vector3.down, out hit, 5, LayerMask.GetMask("Unwalkable", "VFVoxelTerrain"));
                    if (hit.collider != null)
                    {
                        if (hit.collider.gameObject.layer == Pathea.Layer.VFVoxelTerrain)
                            num++;
                    }
                }
            }
            else
                break;
        }

        if (num != 4)
            return false;
        return true;
    }

    static GameObject go;
    public static int burriedNum = 0;
    public static void UpdateAshBox()
    {
		if (DragArticleAgent.Root != null)
        {
			Transform tRoot = DragArticleAgent.Root;
			foreach(Transform item in tRoot){
				if(item.name == "ash_box"){
                    Vector3 p;
                    if (IsBurried(item.position,out p))
                    {
                        //ItemScript s = item.GetComponent<ItemScript>();
                        //if (s == null || s.MItemObj == null)
                        //    continue;
						
                        //ItemAsset.OwnerData tmp = s.MItemObj.GetCmpt<ItemAsset.OwnerData>();

                        if (MissionManager.Instance.m_PlayerMission.HasMission(MissionManager.m_SpecialMissionID10))
                        {
                            ashBox_inScene--;

                            //int n = tmp.npcID;
                            //if (n == 9214 || n == 9215)
                            //{
                                
                            //}
                            MissionManager.Instance.ProcessUseItemMissionByID(1339, StroyManager.Instance.GetPlayerPos());
                            MissionManager.Instance.m_PlayerMission.ModifyQuestVariable(MissionManager.m_SpecialMissionID10, "ITEM0", 1339, 1);
                            if (PeGameMgr.IsMultiStory)
                            {
                                PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_ModifyQuestVariable, MissionManager.m_SpecialMissionID10, "ITEM0", 1339, 1);
                            }
                            burriedNum++;
                            if (burriedNum == 2)
                            {
                                if (PeGameMgr.IsMulti)
                                    MissionManager.Instance.RequestCompleteMission(MissionManager.m_SpecialMissionID10, -1, false);
                                else
                                    MissionManager.Instance.CompleteMission(MissionManager.m_SpecialMissionID10, -1, false);
                            }
                        }
                        else if (MissionManager.Instance.m_PlayerMission.HasMission(MissionManager.m_SpecialMissionID80))
                        {
                            ashBox_inScene--;

                            MissionManager.Instance.ProcessUseItemMissionByID(1339, StroyManager.Instance.GetPlayerPos());
                            MissionManager.Instance.m_PlayerMission.ModifyQuestVariable(MissionManager.m_SpecialMissionID80, "ITEM0", 1339, 1);
                            if (PeGameMgr.IsMultiStory)
                            {
                                PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_ModifyQuestVariable, MissionManager.m_SpecialMissionID80, "ITEM0", 1339, 1);
                            }
                            burriedNum++;
                            if (burriedNum == 1)
                            {
                                if (PeGameMgr.IsMulti)
                                    MissionManager.Instance.RequestCompleteMission(MissionManager.m_SpecialMissionID80, -1, false);
                                else
                                    MissionManager.Instance.CompleteMission(MissionManager.m_SpecialMissionID80, -1, false);
                            }
                        }
                        item.name = "ash_box_burried";

                        //string npcName = tmp.npcName;
                        //if(!string.IsNullOrEmpty(npcName))
                        //    Debug.Log("The burried ashbox belong to:" + npcName.Substring(0, npcName.Length - 5));
                    }
                }
            }
        }
    }
}

namespace ItemAsset
{
    public class OwnerData : ItemAsset.Cmpt
    {
        public static OwnerData deadNPC;

        public int npcID;
        public string npcName = "";

        public override byte[] Export()
        {
            base.Export();
            return PETools.Serialize.Export((w) =>
            {
                w.Write(npcID);
                w.Write(npcName);
            });
        }

		public override void Export(BinaryWriter w)
		{
			base.Export(w);
			BufferHelper.Serialize(w, npcID);
			BufferHelper.Serialize(w, npcName);
		}

		public override void Import(byte[] buff)
        {
            base.Import(buff);
            PETools.Serialize.Import(buff, (r) =>
            {
                npcID = r.ReadInt32();
                npcName = r.ReadString();
            });
        }

		public override void Import(BinaryReader r)
		{
			base.Import(r);
			npcID = BufferHelper.ReadInt32(r);
			npcName = BufferHelper.ReadString(r);
		}

		public override void Init()
        {
            base.Init();
        }

        public override string ProcessTooltip(string text)
        {
            return base.ProcessTooltip(text);
        }


    }
}