using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ItemAsset;

using Pathea.PeEntityExtTrans;
using Pathea.PeEntityExt;
using System;

namespace Pathea
{
    public class MapCmpt : PeCmpt, IPeMsg, PeMap.ILabel
    {
        PeTrans mTrans;
        EntityInfoCmpt mEntityInfo;
        //NpcCmpt mServant;
        CommonCmpt mCommon;
        public CommonCmpt Common { get { return mCommon; } }

        public override void Start()
        {
            base.Start();

            mTrans = Entity.peTrans;
            mEntityInfo = Entity.GetCmpt<EntityInfoCmpt>();
            //mServant = Entity.GetCmpt<NpcCmpt>();
            mCommon = Entity.GetCmpt<CommonCmpt>();
        }

        void IPeMsg.OnMsg(EMsg msg, params object[] args)
        {
            if (PeGameMgr.IsMulti && null != mCommon && mCommon.Identity == EIdentity.Player)
            {
                switch (msg)
                {
                    case EMsg.Net_Instantiate:
                        {
                            PeMap.LabelMgr.Instance.Add(this);
                        }
                        break;

                    case EMsg.Net_Destroy:
                        {
                            PeMap.LabelMgr.Instance.Remove(this);
                        }
                        break;
                }
            }
            else
            {
                if (msg == EMsg.Lod_Collider_Created)
                {
                    PeMap.LabelMgr.Instance.Add(this);
                }
                else if (msg == EMsg.View_Prefab_Destroy)
                {
                    PeMap.LabelMgr.Instance.Remove(this);
                }
            }
        }

        int PeMap.ILabel.GetIcon()
        {
            if (mCommon != null)
            {
                switch (mCommon.Race)
                {
                    case ERace.None:
                        break;
                    case ERace.Mankind:
                        if (GameConfig.IsMultiMode)
                        {
                            if (mCommon.Identity == EIdentity.Player)
                            {
                                EntityInfoCmpt c = Entity.GetCmpt<EntityInfoCmpt>();
                                if (null != c)
                                    return c.mapIcon;
                            }
                            else if (mCommon.Identity == EIdentity.Npc)
                            {
                                return PeMap.MapIcon.Npc;
                            }
                            else
                            {
                                //lz-2016.09.29 如果属于人类，不是npc和玩家，就是人形怪（尹犇说人形怪用怪物的图标）
                                return PeMap.MapIcon.Monster;
                            }
                        }
                        else
                        {
                            if (mCommon.Identity == EIdentity.Npc)
                                return PeMap.MapIcon.Npc;
                            else
                                //lz-2016.09.29 单人模式属于人类的有：玩家自己，NPC，人形怪,因为玩家自己不属于ILabel显示，所以这里else返回人形怪的图标
                                return PeMap.MapIcon.Monster;
                        }
                        break;
                    case ERace.Monster:
                        if (!mCommon.IsBoss)
                            return PeMap.MapIcon.Monster;
                        else
                            return PeMap.MapIcon.MonsterBoss;
                    case ERace.Puja:
                        if (!mCommon.IsBoss)
                            return PeMap.MapIcon.Puja;
                        else
                            return PeMap.MapIcon.PujaBoss;
                    case ERace.Paja:
                        if (!mCommon.IsBoss)
                            return PeMap.MapIcon.Paja;
                        else
                            return PeMap.MapIcon.PajaBoss;
                    case ERace.Alien:
                        return PeMap.MapIcon.Alien;
                    case ERace.Tower:
                        return PeMap.MapIcon.Turret;
                    case ERace.Neutral:
                        break;
                    case ERace.Max:
                        break;
                    default:
                        break;
                }
            }

            return PeMap.MapIcon.Monster;
        }

        Vector3 PeMap.ILabel.GetPos()
        {
            return mTrans.position;
        }

        string PeMap.ILabel.GetText()
        {
            if (mEntityInfo == null)
                return "";
            if (mEntityInfo.characterName == null)
                return "";
            //lz-2016.11.15 多人地图上显示玩家名字
			if (Entity.entityProto.proto == EEntityProto.RandomNpc||Entity.entityProto.proto == EEntityProto.Npc || Entity.entityProto.proto == EEntityProto.Player)
            {
                return mEntityInfo.characterName.fullName;
            }
//            else if (Entity.entityProto.proto == EEntityProto.Npc)
//            {
//                return mEntityInfo.characterName.givenName;
//            }
            return "";
        }

        bool PeMap.ILabel.FastTravel()
        {
            //lz-2018.1.2 队友传送
            if (PeGameMgr.IsSingle) return false;
            else if (mEntityInfo != null && mEntityInfo.mapIcon == PeMap.MapIcon.AllyPlayer && PeGameMgr.IsMulti)
            {
                //lz-2018.01.10 多人所有模式主场景ID都为0，只有主场景可以传送
                bool manPlayerIsMainLand = PlayerNetwork.mainPlayer != null && PlayerNetwork.mainPlayer._curSceneId == 0;
                bool otherPlayerIsMainLand = mEntityInfo.Entity != null && mEntityInfo.Entity.netCmpt != null && mEntityInfo.Entity.netCmpt.network != null && mEntityInfo.Entity.netCmpt.network is PlayerNetwork && (mEntityInfo.Entity.netCmpt.network as PlayerNetwork)._curSceneId == 0;
                return manPlayerIsMainLand && otherPlayerIsMainLand;
            }
            return false;
        }

        PeMap.ELabelType PeMap.ILabel.GetType()
        {
            return PeMap.ELabelType.Npc;
        }

        bool PeMap.ILabel.NeedArrow()
        {
            return false;
        }

        float PeMap.ILabel.GetRadius()
        {
            return -1f;
        }

        PeMap.EShow PeMap.ILabel.GetShow()
        {
            if (mCommon != null)
            {
                switch (mCommon.Race)
                {
                    case ERace.None:
                        break;
                    case ERace.Mankind:
                        if (GameConfig.IsMultiMode)
                        {
                            if (mCommon.Identity == EIdentity.Npc|| mCommon.Identity == EIdentity.Player)
                                return PeMap.EShow.All;
                            else
                                return PeMap.EShow.MinMap;  //lz-2016.09.29 人形怪只在小地图显示
                        }
                        else
                        {
                            if (mCommon.Identity == EIdentity.Npc)
                                return PeMap.EShow.All;
                            else
                                return PeMap.EShow.MinMap; //lz-2016.09.29 人形怪只在小地图显示
                        }
                    case ERace.Monster:
                        if (!mCommon.IsBoss)
                        {
                            return PeMap.EShow.MinMap;
                        }
                        else
                        {
                            return PeMap.EShow.All;
                        }
                    case ERace.Puja:
                        return PeMap.EShow.MinMap;
                    case ERace.Paja:
                        return PeMap.EShow.MinMap;
                    case ERace.Alien:
                        return PeMap.EShow.MinMap;
                    case ERace.Neutral:
                        break;
                    case ERace.Max:
                        break;
                    default:
                        break;
                }
            }

            return PeMap.EShow.Max;
        }

        #region lz-2016.06.02

        public bool CompareTo(PeMap.ILabel label)
        {
            if (label is MapCmpt)
            {
                MapCmpt mapCmptLabel = (MapCmpt)label;
                if (mapCmptLabel == this)
                    return true;
                return false;
            }
            return false;
        }

        #endregion
    }
}
