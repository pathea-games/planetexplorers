using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using ItemAsset;
using System.Linq;
using System.IO;
using Pathea;
using Pathea.PeEntityExt;

public partial class PlayerMission/* : Pathea.ISerializable*/
{
    public bool ImportNetwork(byte[] buffer, int type = 0)
    {
        bool hasData = false;
        if (null == buffer || buffer.Length <= 0)
            return hasData;
        
        using (MemoryStream ms = new MemoryStream(buffer))
        {
            using (BinaryReader _in = new BinaryReader(ms))
            {
                int id, count;
                int iSize = _in.ReadInt32();
                for (int i = 0; i < iSize; i++)
                {
					int rewardIndex = _in.ReadInt32();
                    if (!m_GetRewards.Contains(rewardIndex))
						m_GetRewards.Add(rewardIndex);
                    hasData = true;
                }

                iSize = _in.ReadInt32();
                for (int i = 0; i < iSize; i++)
                {
					int targetKey = _in.ReadInt32();
					int targetValue = _in.ReadInt32();
                    m_MissionTargetState[targetKey] = targetValue;
                    hasData = true;
                }

                iSize = _in.ReadInt32();
                for (int i = 0; i < iSize; i++)
                {
					int stateKey = _in.ReadInt32();
					int stateValue = _in.ReadInt32();
                    m_MissionState[stateKey] = stateValue;
                    hasData = true;
                }

                iSize = _in.ReadInt32();
                for (int i = 0; i < iSize; i++)
                {
                    Dictionary<string, string> tmp = new Dictionary<string, string>();
                    id = _in.ReadInt32();
                    count = _in.ReadInt32();
					for (int m = 0; m < count; m++)
					{
						string tmpkey = _in.ReadString();
						string tmpValue = _in.ReadString();
                        tmp[tmpkey] = tmpValue;
					}

                    m_RecordMisInfo[id] = tmp;
                    hasData = true;
                }

                MissionManager.Instance.m_PlayerMission.LanguegeSkill = _in.ReadInt32();
                if (type == 1)
                {
                    _in.ReadInt32();
                    m_FollowPlayerName = _in.ReadString();                    
                }
                
                _in.Close();
            }

            ms.Close();
        }
        return hasData;
    }


    void MulAdRandMisOperation(TargetType curType, int targetid)
    {
        switch (curType)
        {
            case TargetType.TargetType_KillMonster:
                {
                    //if (!m_Player.m_bHadInitMission)
                    //    m_Player.RPCServer(EPacketType.PT_InGame_InitMission, targetid);
                }
                break;
            case TargetType.TargetType_Follow:
                {
                    TypeFollowData data = MissionManager.GetTypeFollowData(targetid);
                    if (data == null)
                        return;

                    data.m_DistRadius = data.m_AdDistPos.radius2;

                    if (PeCreature.Instance.mainPlayer.ExtGetName() != m_FollowPlayerName && m_FollowPlayerName != null)
                        break;

                    
                    for (int i = 0; i < data.m_iNpcList.Count; i++)
                    {
                        PeEntity npc = EntityMgr.Instance.Get(data.m_iNpcList[i]);
                        if (npc == null)
                            continue;
                    }
                }
                break;
            case TargetType.TargetType_UseItem:
                {
                    TypeUseItemData data = MissionManager.GetTypeUseItemData(targetid);
                    if (data == null)
                        return;

//                    int iMin = data.m_AdDistPos.dist - data.m_AdDistPos.radius;
//                    int iMax = data.m_AdDistPos.dist + data.m_AdDistPos.radius;
                    data.m_Radius = data.m_AdDistPos.radius2;
                }
                break;
        }
    }

    public void RequestCompleteMission(int MissionID, int TargetID = -1,bool bCheck = true)
    {
		PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_CompleteMission, TargetID, MissionID,bCheck);
    }

    //删除任务
    public void ReplyDeleteMission(int nMissionID)
    {
        MissionCommonData data = MissionManager.GetMissionCommonData(nMissionID);
        if (null == data)
            return;

        PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_DeleteMission, nMissionID);
    }
}
