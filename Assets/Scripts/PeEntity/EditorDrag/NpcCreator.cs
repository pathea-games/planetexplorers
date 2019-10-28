using UnityEngine;
using System.Collections;
using Pathea;
using Pathea.PeEntityExt;

public class NpcCreator : MonoBehaviour
{
    //[SerializeField]
    //string m_skinName = "Ava_Azniv-Ava_Azniv";

    [SerializeField]
    int m_npcProtoId = 11;//Ava_Azniv

	void Start ()
    {
        int id = WorldInfoMgr.Instance.FetchNonRecordAutoId();
        //CreateNpc(id, m_skinName);
        CreateWithProto(id, m_npcProtoId);

        //Destroy(gameObject);
    }

    void CreateWithProto(int id, int protoId)
    {
		PeEntity entity = PeEntityCreator.Instance.CreateNpc(id, protoId, transform.position, Quaternion.identity, Vector3.one);

        PeMousePickCmpt pmp = entity.GetCmpt<PeMousePickCmpt>();
        if (null != pmp)
        {
            pmp.mousePick.eventor.Subscribe(NpcMouseEventHandler);
        }

        NpcMissionData useData = new NpcMissionData();
        useData.m_bRandomNpc = true;
        useData.m_Rnpc_ID = protoId;
        useData.m_QCID = 1;
        useData.m_MissionList.Add(191);
        entity.SetUserData(useData);
    }

    public Vector3 GetPlayerPos()
    {
        PeTrans playerTrans = null;

        if (playerTrans == null)
            playerTrans = PeCreature.Instance.mainPlayer.peTrans;

        if (playerTrans == null)
            return Vector3.zero;

        return playerTrans.position;
    }

    public bool IsRandomNpc(PeEntity npc)
    {
        if (npc == null)
            return false;

        NpcMissionData missionData = npc.GetUserData() as NpcMissionData;
        if (missionData == null)
            return false;

        return missionData.m_bRandomNpc;
    }

    public void NpcMouseEventHandler(object sender, MousePickable.RMouseClickEvent e)
    {
        Pathea.PeEntity npc = e.mousePickable.GetComponent<Pathea.PeEntity>();

        if (npc == null)
            return;

        float dist = Vector3.Distance(npc.position, GetPlayerPos());
        if (dist > 7)
            return;

        if (IsRandomNpc(npc) && npc.IsDead())
        {
            if (npc.Id == 9203 || npc.Id == 9204)
                return;

            if (npc.Id == 9214 || npc.Id == 9215)
            {
                if (!MissionManager.Instance.HasMission(MissionManager.m_SpecialMissionID10))
                    return;
            }

            if (GameConfig.IsMultiMode)
            {
                //if (null != PlayerFactory.mMainPlayer)
                //    PlayerFactory.mMainPlayer.RequestDeadObjItem(npc.OwnerView);
            }
            else
            {
                //if (GameUI.Instance.mItemGetGui.UpdateItem(npc))
                //{
                //    GameUI.Instance.mItemGetGui.Show();
                //}
            }

            if (npc.IsRecruited())
            {
                GameUI.Instance.mRevive.ShowServantRevive(npc);
            }

            return;
        }


        if (IsRandomNpc(npc) && npc.IsFollower())
            return;

        if (!npc.GetTalkEnable())
            return;

        if (IsRandomNpc(npc) && !npc.IsDead())
        {
            NpcMissionData missionData = npc.GetUserData() as NpcMissionData;
            if (null != missionData)
            {
                if (!MissionManager.Instance.HasMission(missionData.m_RandomMission))
                {
                    if (PeGameMgr.IsStory)
                        RMRepository.CreateRandomMission(missionData.m_RandomMission);
                    else if (PeGameMgr.IsMultiAdventure || PeGameMgr.IsMultiBuild)
                        PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_NewMission, missionData.m_RandomMission,npc.Id);
                    else
                        AdRMRepository.CreateRandomMission(missionData.m_RandomMission);
                }
            }
        }

        GameUI.Instance.mNpcWnd.SetCurSelNpc(npc);
        GameUI.Instance.mNpcWnd.Show();
    }

//    void CreateNpc(int id, string skinName)
//    {
//        PeEntity entity = EntityMgr.Instance.Create(id, PeCreature.NpcPrefabPath);

//        if (!string.IsNullOrEmpty(skinName))
//        {
//            SetModelData(entity, skinName);
//        }

//        EntityInfoCmpt info = entity.GetCmpt<EntityInfoCmpt>();
//        if (null != info)
//        {
//            //info.Name = characterName;
//            //info.FaceIcon = npcIcon;
//            info.mapIcon = PeMap.MapIcon.FlagIcon;
//        }

//        PeTrans trans = entity.peTrans;
//        if (null != trans)
//        {
//            trans.position = transform.position;
//        }

//        EquipmentCmpt equip = entity.GetCmpt<EquipmentCmpt>();
//        if (null != equip)
//        {
//            equip._ItemList.Add(ItemAsset.ItemMgr.Instance.CreateItem(1));
////            equip.PutOnEquipment(ItemAsset.ItemMgr.Instance.CreateItem(1));
//        }
//    }

//    void SetModelData(PeEntity entity, string name)
//    {
//        AvatarCmpt avatar = entity.GetCmpt<AvatarCmpt>();

//        CustomCharactor.AvatarData nudeAvatarData = new CustomCharactor.AvatarData();

//        nudeAvatarData.SetPart(CustomCharactor.AvatarData.ESlot.HairF, "Model/Npc/" + name);

//        avatar.SetData(new AppearBlendShape.AppearData(), nudeAvatarData);
//    }
}
