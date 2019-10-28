using UnityEngine;
using System.Collections;

using Pathea;

namespace Pathea
{
    namespace PeEntityExt
    {
        public static class PeEntityExt
        {
            #region Util
            public static void SetViewModelPath(this PeEntity entity, string value)
            {
                if (null == entity)
                {
                    return;
                }

				BiologyViewCmpt c = entity.biologyViewCmpt;
                if (null == c)
                {
                    return;
                }

                c.SetViewPath(value);
            }

            public static void SetAvatarNpcModelPath(this PeEntity entity, string value)
            {
                if (null == entity)
                {
                    return;
                }

                AvatarCmpt avatar = entity.GetCmpt<AvatarCmpt>();
                if (null == avatar)
                {
                    return;
                }

                CustomCharactor.AvatarData nudeAvatarData = new CustomCharactor.AvatarData();

                nudeAvatarData.SetPart(CustomCharactor.AvatarData.ESlot.HairF, value);

                avatar.SetData(new AppearBlendShape.AppearData(), nudeAvatarData);
            }

            public static void ExtSetSex(this PeEntity entity, PeSex sex)
            {
                CommonCmpt info = entity.GetCmpt<CommonCmpt>();
                if (null == info)
                {
                    return;
                }

                info.sex = sex;
            }

            public static void ExtSetVoiceType(this PeEntity entity,int voiceType)
            {
                NpcCmpt info = entity.GetCmpt<NpcCmpt>();
                 if (null == info)
                     return;

                 info.voiceType = voiceType;
            }

            public static PeSex ExtGetSex(this PeEntity entity)
            {
                CommonCmpt info = entity.GetCmpt<CommonCmpt>();
                if (null == info)
                {
                    return PeSex.Max;
                }

                return info.sex;
            }

            public static void ExtSetFaceIcon(this PeEntity entity, string icon)
            {
                EntityInfoCmpt info = entity.GetCmpt<EntityInfoCmpt>();

                if (null == info)
                {
                    return;
                }

                info.faceIcon = icon;
            }

            public static string ExtGetFaceIcon(this PeEntity entity)
            {
                EntityInfoCmpt info = entity.GetCmpt<EntityInfoCmpt>();

                if (null == info)
                {
                    return null;
                }

				return string.IsNullOrEmpty(info.faceIcon) ? "npc_big_Unknown" : info.faceIcon;
            }

            public static void ExtSetFaceIconBig(this PeEntity entity, string icon)
            {
                EntityInfoCmpt info = entity.GetCmpt<EntityInfoCmpt>();

                if (null == info)
                {
                    return;
                }

                info.faceIconBig = icon;
            }

            public static string ExtGetFaceIconBig(this PeEntity entity)
            {
                EntityInfoCmpt info = entity.GetCmpt<EntityInfoCmpt>();

                if (null == info)
                {
                    return null;
                }

                return string.IsNullOrEmpty(info.faceIconBig) ? "npc_big_Unknown" : info.faceIconBig;
            }

            //public static void ExtSetMapIcon(this PeEntity entity, int icon)
            //{
            //    EntityInfoCmpt info = entity.GetCmpt<EntityInfoCmpt>();

            //    if (null == info)
            //    {
            //        return;
            //    }

            //    info.mapIcon = icon;
            //}

            //public static int ExtGetMapIcon(this PeEntity entity)
            //{
            //    EntityInfoCmpt info = entity.GetCmpt<EntityInfoCmpt>();

            //    if (null == info)
            //    {
            //        return PeMap.MapIcon.FlagIcon;
            //    }

            //    return info.mapIcon;
            //}

            public static void ExtSetFaceTex(this PeEntity entity, Texture icon)
            {
                EntityInfoCmpt info = entity.GetCmpt<EntityInfoCmpt>();

                if (null == info)
                {
                    return;
                }

                info.faceTex = icon;
            }

            public static Texture ExtGetFaceTex(this PeEntity entity)
            {
                EntityInfoCmpt info = entity.GetCmpt<EntityInfoCmpt>();

                if (null == info)
                {
                    return null;
                }

                return info.faceTex;
            }

            public static void SetShopIcon(this PeEntity entity, string icon)
            {
                EntityInfoCmpt info = entity.GetCmpt<EntityInfoCmpt>();

                if (null == info)
                {
                    return;
                }

                info.shopIcon = icon;
            }

            public static void ExtSetName(this PeEntity entity, CharacterName name)
            {
                EntityInfoCmpt info = entity.GetCmpt<EntityInfoCmpt>();

                if (null == info)
                {
                    return;
                }

                info.characterName = name;
            }

            public static string ExtGetName(this PeEntity entity)
            {
                EntityInfoCmpt info = entity.GetCmpt<EntityInfoCmpt>();

                if (null == info)
                {
                    return null;
                }

                return info.characterName.fullName;
            }
//			public static string ExtGetFullName(this PeEntity entity)
//			{
//				EntityInfoCmpt info = entity.GetCmpt<EntityInfoCmpt>();
//				
//				if (null == info)
//				{
//					return null;
//				}
//				
//				return info.characterName.fullName;
//			}
			public static void SetAvatarData(this PeEntity entity, AppearBlendShape.AppearData appearData, CustomCharactor.AvatarData nudeAvatarData)
            {
                AvatarCmpt v = entity.GetCmpt<AvatarCmpt>();
                if (null != v)
                {
                    v.SetData(appearData, nudeAvatarData);
                }
            }

            public static void TrapInSpiderWeb(this PeEntity entity, bool btrue, float delayTime) { }

            public static void SetAiActive(this PeEntity entity, bool value) { }

            public static void SetCamp(this PeEntity entity, int iCamp) { }

            public static void SetState(this PeEntity entity, NpcMissionState state)
            {
                EntityInfoCmpt info = entity.GetCmpt<EntityInfoCmpt>();
                if (info != null)
                {
                    info.SetMissionState(state);
                }
            }

            public static void SetInjuredLevel(this PeEntity entity, float injuredLevel) { }

            #endregion

            #region Die
            public static void ApplyDamage(this PeEntity entity, float damage)
            {

            }

            public static bool IsInvincible(this PeEntity entity)
            {
                CommonCmpt c = entity.GetCmpt<CommonCmpt>();
                if (c == null)
                {
                    return false;
                }

                return c.invincible;
            }

            public static void SetInvincible(this PeEntity entity, bool value)
            {
                CommonCmpt c = entity.GetCmpt<CommonCmpt>();
                if (c == null)
                {
                    return;
                }

                c.invincible = value;
            }

            public static bool IsDead(this PeEntity entity)
            {
				SkAliveEntity alive = entity.GetCmpt<SkAliveEntity>();
				if (alive == null)
					return false;

				return alive.isDead;
            }

            #endregion

            #region Carrier
            public static bool IsOnCarrier(this PeEntity entity)
            {
                PassengerCmpt p = entity.GetCmpt<PassengerCmpt>();
                if (p == null)
                {
                    return false;
                }

                return p.IsOnCarrier();
            }

            public static void GetOffCarrier(this PeEntity entity)
            {
                PassengerCmpt p = entity.GetCmpt<PassengerCmpt>();
                if (p == null)
                {
                    return;
                }

                p.GetOffCarrier();
            }
            #endregion

            #region State
            public static bool IsRandomNpc(this PeEntity entity)
            {
				if(entity==null)
					return false;
                CommonCmpt c = entity.GetCmpt<CommonCmpt>();
                if (c == null)
                {
                    return false;
                }

                return c.entityProto.proto == EEntityProto.RandomNpc;
            }

            public static bool IsRecruited(this PeEntity entity)
            {
                if (CSMain.IsColonyNpc(entity.Id))
                    return true;
                return false;
            }

            public static bool Recruit(this PeEntity entity)
            {
                return false;
            }

            public static bool Dismiss(this PeEntity entity)
            {
                return false;
            }

            public static EAttackMode GetAttackMode(this PeEntity entity)
            {
                return EAttackMode.Max;
            }

            public static void SetAttackMode(this PeEntity entity, EAttackMode mode)
            {
            }

            #endregion

            #region UserData
            public static object GetUserData(this PeEntity entity)
            {
				if(entity == null)
				{
					return null;
				}
                CommonCmpt c = entity.GetCmpt<CommonCmpt>();
                if (c == null)
                {
                    return null;
                }

                return c.userData;
            }

            public static void SetUserData(this PeEntity entity, object obj)
            {
				if(entity == null)
				{
					return ;
				}
                CommonCmpt c = entity.GetCmpt<CommonCmpt>();
                if (c == null)
                {
                    return;
                }
               

                c.userData = obj;
                CompatibleMissionData(entity);

            }

            /// <summary>
            /// 兼容任务数据
            /// </summary>
            private static void CompatibleMissionData(PeEntity entity)
            {
                if (entity == null || !entity.commonCmpt)
                    return;
               
                //lz-2017.03.06 找andy任务兼容以前存档 : 接任务列表
                NpcMissionData missionData = entity.commonCmpt.userData as NpcMissionData;
                if (null != missionData)
                {
                    if (entity.Id == 9007)
                    {
                        if(!missionData.m_MissionList.Contains(10054))
                           missionData.m_MissionList.Add(10054);

                        if (!missionData.m_MissionList.Contains(10055))
                            missionData.m_MissionList.Add(10055);

                    }
                    if (entity.Id == 9041 )
                    {
                        //andy接任务列表兼容
                        if(!missionData.m_MissionList.Contains(10053))
                           missionData.m_MissionList.Add(10053);

                        //交任务列表兼容
                        if (!missionData.m_MissionListReply.Contains(10053))
                            missionData.m_MissionListReply.Add(10053);
                    }
                    //10055,10060,10061,10066,10056,10057,10058,10059
                    if (entity.Id == 9037)
                    {
                        if(!missionData.m_MissionList.Contains(10055))
                            missionData.m_MissionList.Add(10055);

                        if (!missionData.m_MissionList.Contains(10056))
                            missionData.m_MissionList.Add(10056);

                        if (!missionData.m_MissionList.Contains(10057))
                            missionData.m_MissionList.Add(10057);

                        if (!missionData.m_MissionList.Contains(10058))
                            missionData.m_MissionList.Add(10058);

                        if (!missionData.m_MissionList.Contains(10059))
                            missionData.m_MissionList.Add(10059);

                        if (!missionData.m_MissionList.Contains(10060))
                            missionData.m_MissionList.Add(10060);

                        if (!missionData.m_MissionList.Contains(10061))
                            missionData.m_MissionList.Add(10061);

                        if (!missionData.m_MissionList.Contains(10066))
                            missionData.m_MissionList.Add(10066); 
                    }

                  
                   
                }
            }
            #endregion

            #region Cmd

            public static void CmdStartIdle(this PeEntity entity)
            {

            }

            public static void CmdStopIdle(this PeEntity entity)
            {

            }

            public static bool GetTalkEnable(this PeEntity entity)
            {
                return true;
            }

            public static void CmdStartTalk(this PeEntity entity)
            {

            }

            public static void CmdStopTalk(this PeEntity entity)
            {

            }

            public static bool IsTalking(this PeEntity entity)
            {
                return false;
            }

            public static bool CmdPlayAnimation(this PeEntity entity, string parameter, float autoReturnTime) { return false; }

            public static bool CmdPlayAnimation(this PeEntity entity, string parameter, bool flag) { return false; }

            #endregion

            #region Follower
            public static bool IsFollower(this PeEntity entity)
            {
                if (entity.NpcCmpt == null)
                    return false;
                if(PeGameMgr.IsMulti)
                {
                    if ((entity.GetComponent<NpcCmpt>().Net as AiAdNpcNetwork).bForcedServant)
                        return false;
                }
                return entity.GetComponent<NpcCmpt>().IsServant;
            }

            public static bool SetFollower(this PeEntity entity, bool bFlag, int index = -1)
            {
                ServantLeaderCmpt masterCmpt = PeCreature.Instance.mainPlayer.GetCmpt<ServantLeaderCmpt>();
                NpcCmpt npcCmpt = entity.NpcCmpt;
                if (!bFlag)
                {
                    return masterCmpt.RemoveServant(npcCmpt);
                }
                else
                {
                    //set floower succeed talk
                    entity.NpcCmpt.AddTalkInfo(ENpcTalkType.Conscribe_succeed, ENpcSpeakType.TopHead);
                    if (index == -1)
                    {
                        return masterCmpt.AddServant(npcCmpt);
                    }
                    return masterCmpt.AddServant(npcCmpt, index);
                }
            }
            #endregion

            #region train
			public static void GetOnTrain(this PeEntity entity, int id, bool checkState = true)
            {
                PassengerCmpt p = entity.GetCmpt<PassengerCmpt>();
                if (p == null)
                {
                    return;
                }

				p.GetOn(id, checkState);
            }

            public static void GetOffTrain(this PeEntity entity, Vector3 pos)
            {
                PassengerCmpt p = entity.GetCmpt<PassengerCmpt>();
                if (p == null)
                {
                    return;
                }

                p.GetOff(pos);
            }

            public static bool IsOnTrain(this PeEntity entity)
            {
                PassengerCmpt p = entity.GetCmpt<PassengerCmpt>();
                if (p == null)
                {
                    return false;
                }

                return p.IsOnRail;
            }
            #endregion

            #region Sound
            public static void SayHiRandom(this PeEntity entity) { }

            public static void SayByeRandom(this PeEntity entity) { }
            #endregion
        }
    }
}