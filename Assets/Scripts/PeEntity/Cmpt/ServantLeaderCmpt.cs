using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pathea.PeEntityExt;
using System;
using System.Linq;

namespace Pathea
{

    public class ServantLeaderCmpt : PeCmpt
    {
        const int Version_0000 = 0;
        const int Version_Current = Version_0000;

        static ServantLeaderCmpt mInstance;
        public static ServantLeaderCmpt Instance
        {
            get
            {
                //lw2017.1.16:初始化servantLeaderCmpt时玩家Entity可能未被初始化好
                if (mInstance == null && PeCreature.Instance.mainPlayer != null)
                    mInstance = PeCreature.Instance.mainPlayer.GetCmpt<ServantLeaderCmpt>();

                return mInstance;
            }
        }

        public event Action<int> FreeNpcEventHandler;

        Action_Sleep mSleepAction;
        public class ServantChanged : PeEvent.EventArg
        {
			public bool 	isAdd;
			public PeEntity servant;

			public ServantChanged(bool add, PeEntity entity)
			{
				isAdd = add;
				servant = entity;
			}
        }

        PeEvent.Event<ServantChanged> mEventor = new PeEvent.Event<ServantChanged>();

        public PeEvent.Event<ServantChanged> changeEventor
        {
            get
            {
                return mEventor;
            }
        }

        const int MaxFollower = 2;
        static public int mMaxFollower
        {
            get { return MaxFollower; }
        }

        public int RQFollowersIndex
        {
            get
            {
                if (MissionManager.Instance != null && MissionManager.Instance.m_PlayerMission != null && MissionManager.Instance.m_PlayerMission.followers != null)
                    return MissionManager.Instance.m_PlayerMission.followers.Count;
                else
                    return 0;
            }
        }

		public NpcCmpt[] mFollowers; //= new NpcCmpt[MaxFollower];
        public List<NpcCmpt> mForcedFollowers = new List<NpcCmpt>();

        PeEntity mpeEntity;
        public PeEntity peEntity { get { return mpeEntity; } }

        bool ValidateIndex(int index)
        {
            if (index < 0 || index >= mFollowers.Length)
            {
                return false;
            }

            return true;
        }

        public NpcCmpt GetServant(int index)
        {
            if (!ValidateIndex(index))
            {
                Debug.LogError("error follower index:" + index);
                return null;
            }

            return mFollowers[index];
        }

        public NpcCmpt[] GetServants()
        {
            return mFollowers;
        }

		public void SevantLostController(PeTrans leader)
		{
			for(int i=0;i<mFollowers.Length;i++)
			{
				Vector3 fixedPos = PETools.PEUtil.GetEqualPositionToStand(PETools.PEUtil.MainCamTransform.position, -PETools.PEUtil.MainCamTransform.forward,leader.position,-leader.forward,3.0f);
				if(mFollowers[i] != null)
					mFollowers[i].Req_Translate(fixedPos);
			}

		}


        //void DoFollow(FollowCmpt f)
        //{
        //    if (null == f)
        //    {
        //        return;
        //    }

        //    PeTrans trans = Entity.peTrans;

        //    f.Follow(trans);
        //}

        //void DoDefollow(FollowCmpt f)
        //{
        //    if (null == f)
        //    {
        //        return;
        //    }

        //    f.Defollow();
        //}
        public override void Awake()
        {
            base.Awake();
            mInstance = this;
			mFollowers = new NpcCmpt[MaxFollower];
            mpeEntity = GetComponent<PeEntity>();
            FreeNpcEventHandler += PlayerNetwork.RequestDismissNpc;
        }

        public override void Start()
        {
            base.Start();
            Invoke("InitFollower", 0.5f);

            //lw2017.1.16:初始化servantLeaderCmpt时玩家Entity可能未被初始化好
            if (PeCreature.Instance.mainPlayer != null)
                mInstance = PeCreature.Instance.mainPlayer.GetCmpt<ServantLeaderCmpt>();

            if (PeEventGlobal.Instance != null && PeEventGlobal.Instance.DestroyEvent != null)
                PeEventGlobal.Instance.DestroyEvent.AddListener(OnFollowerEntityDestroy);
        }

        void InitFollower()
        {
            if (mFollowers == null)
                mFollowers = new NpcCmpt[MaxFollower];

            if (null != initReq)
            {
                for (int i = 0; i < initReq.Count; ++i)
                {
                    InitSleepInfo(initReq[i]);
                }
                initReq.Clear();
                initReq = null;
            }

        }

        

        public void InitSleepInfo(NpcCmpt npc)
        {
            if (mSleepAction == null)
                mSleepAction = Entity.motionMgr.GetAction<Action_Sleep>();

            if (mSleepAction != null)
            {
                mSleepAction.startSleepEvt += npc.OnLeaderSleep;
                mSleepAction.endSleepEvt += npc.OnLeaderEndSleep;
            }
        }

        public override void OnDestroy()
        {
            base.OnDestroy();

            FreeNpcEventHandler -= PlayerNetwork.RequestDismissNpc;
        }

        void OnFollowerEntityDestroy(SkillSystem.SkEntity entity)
        {
            if (entity == null || entity.Equals(null))
                return;

            PeEntity _peEntity = entity.GetComponent<PeEntity>();
            if (_peEntity == null || _peEntity.Equals(null) || _peEntity.NpcCmpt == null || _peEntity.NpcCmpt.Equals(null))
                return;

            if (ContainsServant(_peEntity.NpcCmpt))
                RemoveServant(_peEntity.NpcCmpt);
        }

        public void OnFreeNpc(int id)
        {
            if (null != FreeNpcEventHandler)
                FreeNpcEventHandler(id);
        }

        public bool ContainsServant(NpcCmpt follower)
        {
            return new List<NpcCmpt>(mFollowers).Contains(follower);
        }

        public void AddForcedServant(NpcCmpt forcedServant, bool isMove = false)
        {
            PeEntity npc = forcedServant.GetComponent<PeEntity>();
            if (mForcedFollowers.Contains(forcedServant) || npc == null)
                return;
            mForcedFollowers.Add(forcedServant);
            if (isMove == true)
                StroyManager.Instance.Translate(npc, EntityCreateMgr.Instance.GetPlayerTrans().position);
            //StroyManager.Instance.FollowTarget(npc, EntityCreateMgr.Instance.GetPlayerTrans());
            StroyManager.Instance.FollowTarget(npc, PeCreature.Instance.mainPlayer.Id,Vector3.zero,0,0.0f);
        }

        public void RemoveForcedServant(NpcCmpt forcedServant)
        {
            if (mForcedFollowers.Contains(forcedServant))
                mForcedFollowers.Remove(forcedServant);
            else
                return;
            NpcMissionData missionData = forcedServant.Follwerentity.GetUserData() as NpcMissionData;
            missionData.mInFollowMission = false;
            if (forcedServant.GetComponent<PeEntity>() == null)
                return;
            if (!MissionManager.Instance.m_PlayerMission.followers.Contains(forcedServant))
                StroyManager.Instance.RemoveReq(forcedServant.GetComponent<PeEntity>(), EReqType.FollowTarget);

        }



        public bool AddServant(NpcCmpt follower)
        {
            if (ContainsServant(follower))
                return false;

            for (int i = 0; i < mFollowers.Length; i++)
            {
                if (null == mFollowers[i])
                {
                    mFollowers[i] = follower;

                    if (UINPCfootManMgr.Instance != null)
                        UINPCfootManMgr.Instance.GetFollowerAlive();
                    //DoFollow(follower.Entity.GetCmpt<FollowCmpt>());
                    follower.SetServantLeader(this);
                    follower.NpcControlCmdId = 19;
                    PeEntityCreator.RecruitRandomNpc(follower.Entity);
                    InitSleepInfo(mFollowers[i]);
                    if (UIMissionMgr.Instance != null)
                        UIMissionMgr.Instance.DeleteMission(follower.Entity);
					changeEventor.Dispatch(new ServantChanged(true, follower.Entity));

                    //lz-2016.08.22 玩家召一个仆从
                    InGameAidData.CheckGetServant(follower.Entity.Id);
                    //lw:完成成就同生共死
                    SteamAchievementsSystem.Instance.OnGameStateChange(Eachievement.Eleven);
                    return true;
                }
            }

            return false;
        }
        public bool AddServant(NpcCmpt follower, int index)
        {
            if (ContainsServant(follower))
                return false;

            bool isFull = true;
			for (int i = 0; i < mFollowers.Length; i++)
			{
				if(mFollowers[i]==null){
					isFull = false;
					break;
				}
			}
			if(isFull)
				return false;

            for (int i = 0; i < mFollowers.Length; i++)
            {
                if (i == index)
				{
					if(mFollowers[i]!=null){
						List<NpcCmpt> fList = mFollowers.Where(a=>a!=null).ToList();
						fList.Insert(i,follower);
						if(fList.Count<MaxFollower)
						{
							for(int j = 0;j<MaxFollower-fList.Count;j++){
								fList.Add(null);
							}
						}
						mFollowers = fList.ToArray();
					}else{
						mFollowers[i]=follower;
					}

                    if (UINPCfootManMgr.Instance != null)
                        UINPCfootManMgr.Instance.GetFollowerAlive();

					
					//log: lz-2016.05.11  这里移除一个仆从的时候进行排序，让前面不要空出位置
					mFollowers = mFollowers.OrderBy(a => a == null).ToArray();
                    //DoFollow(follower.Entity.GetCmpt<FollowCmpt>());
                    follower.SetServantLeader(this);
                    follower.NpcControlCmdId = 19;
                    PeEntityCreator.RecruitRandomNpc(follower.Entity);
					InitSleepInfo(follower);
					changeEventor.Dispatch(new ServantChanged(false, follower.Entity));
                    return true;
                }
            }

            return false;
        }
        public bool RemoveServant(NpcCmpt follower)
        {
            for (int i = 0; i < mFollowers.Length; i++)
            {
                if (follower == mFollowers[i])
                {
                    follower.NpcControlCmdId = 1;
                    follower.SetServantLeader(null);
                    PeEntity npc = follower.GetComponent<PeEntity>();
                    if (UIMissionMgr.Instance != null)
                        UIMissionMgr.Instance.AddMission(npc);
                    MissionManager.Instance.m_PlayerMission.UpdateNpcMissionTex(npc);
                    return RemoveServant(i);
                }
            }

            return false;
        }

        public bool RemoveServant(int index)
        {
            if (ValidateIndex(index))
            {
                //DoDefollow(mFollowers[index].Entity.GetCmpt<FollowCmpt>());

				PeEntity servant = mFollowers[index].Entity;
                mFollowers[index].NpcControlCmdId = 1;
                mSleepAction.startSleepEvt -= mFollowers[index].OnLeaderSleep;
                mSleepAction.endSleepEvt -= mFollowers[index].OnLeaderEndSleep;
                mFollowers[index].RemoveSleepBuff();
                mFollowers[index].SetServantLeader(null);
				if (mFollowers[index].Entity != null && mFollowers[index].Entity.proto == EEntityProto.RandomNpc)
					PeEntityCreator.ExileRandomNpc(mFollowers[index].Entity);

                mFollowers[index] = null;
                //log: lz-2016.05.11  这里移除一个仆从的时候进行排序，让前面不要空出位置
                mFollowers = mFollowers.OrderBy(a => a == null).ToArray();
				changeEventor.Dispatch(new ServantChanged(false, servant));

                if (UINPCfootManMgr.Instance != null)
                    UINPCfootManMgr.Instance.GetFollowerAlive();
                return true;
            }

            return false;
        }

        public int GetServantNum()
        {
            int num = 0;
            for (int i = 0; i < mFollowers.Length; i++)
            {
                if (mFollowers[i] != null)
                    num++;
            }
            return num;
        }
        public override void Serialize(System.IO.BinaryWriter w)
        {
            base.Serialize(w);

            if (PeEntity.CURRENT_VERSION >= PeEntity.VERSION_0001)
            {
                w.Write(Version_Current);
                w.Write(mFollowers.Length);
                //w.Write(mForcedFollowers.Count);

                for (int i = 0; i < mFollowers.Length; i++) {
					if (mFollowers [i] == null)
						w.Write (0);
					else
						w.Write (mFollowers [i].Entity.Id);
				}
                //foreach (NpcCmpt npc in mForcedFollowers)
                //{
                //    if (npc == null)
                //        w.Write(0);
                //    else
                //        w.Write(npc.Entity.Id);
                //}
            }
        }

        List<NpcCmpt> initReq;

        public override void Deserialize(System.IO.BinaryReader r)
        {
            base.Deserialize(r);

            if (PeEntity.CURRENT_VERSION >= PeEntity.VERSION_0001)
            {
                /*int version = */
                r.ReadInt32();
                int length = r.ReadInt32();
                //int forcedLength = r.ReadInt32();

                initReq = new List<NpcCmpt>();

                for (int i = 0; i < length; i++)
                {
                    int npcID = r.ReadInt32();
                    PeEntity entity = EntityMgr.Instance.Get(npcID);
                    if (entity != null)
                    {
                        NpcCmpt npc = entity.GetComponent<NpcCmpt>();
                        if (npc != null)
                        {
                            AddServant(npc);
                            //							if(mSenventsInfo == null)
                            //								mSenventsInfo = new AddServrntInfo();
                            //							mSenventsInfo.AddServrnt(npc);
                            initReq.Add(npc);
                        }
                    }
                }

                //for (int i = 0; i < forcedLength; i++)
                //{
                //    int npcID = r.ReadInt32();
                //    PeEntity entity = EntityMgr.Instance.Get(npcID);
                //    if(entity != null)
                //    {
                //        NpcCmpt npc = entity.GetComponent<NpcCmpt>();
                //        if(npc != null)
                //        {
                //            AddForcedServant(npc);
                //        }
                //    }
                //}
            }
        }

        public override string ToString()
        {
            NpcCmpt f1 = GetServant(0);
            NpcCmpt f2 = GetServant(1);

            return string.Format("leader:{0}, follower1:{1}, follower2:{2}"
                , Entity.Id
                , null == f1 ? -11 : f1.Entity.Id
                , null == f2 ? -11 : f2.Entity.Id);
        }
    }

    //namespace PeEntityExtServentLeader
    //{
    //    public static class PeEntityExtServentLeader
    //    {
    //        public static NpcCmpt ExtGetServant(this PeEntity entity, int index)
    //        {
    //            ServantLeaderCmpt c = entity.GetCmpt<ServantLeaderCmpt>();
    //            if (null == c)
    //            {
    //                return null;
    //            }

    //            return c.GetServant(index);
    //        }

    //        public static bool ExtAddServant(this PeEntity entity, PeEntity servantEntity)
    //        {
    //            NpcCmpt servant = servantEntity.GetCmpt<NpcCmpt>();
    //            if (null == servant)
    //            {
    //                return false;
    //            }

    //            ServantLeaderCmpt leader = entity.GetCmpt<ServantLeaderCmpt>();
    //            if (null == leader)
    //            {
    //                return false;
    //            }

    //            return leader.AddServant(servant);
    //        }

    //        public static bool ExtRemoveServant(this PeEntity entity, PeEntity servantEntity)
    //        {
    //            NpcCmpt servant = servantEntity.GetCmpt<NpcCmpt>();
    //            if (null == servant)
    //            {
    //                return false;
    //            }

    //            ServantLeaderCmpt leader = entity.GetCmpt<ServantLeaderCmpt>();
    //            if (null == leader)
    //            {
    //                return false;
    //            }

    //            return leader.RemoveServant(servant);
    //        }

    //        public static bool ExtRemoveServant(this PeEntity entity, int index)
    //        {
    //            ServantLeaderCmpt c = entity.GetCmpt<ServantLeaderCmpt>();
    //            if (null == c)
    //            {
    //                return false;
    //            }

    //            return c.RemoveServant(index);
    //        }
    //    }
    //}
}
