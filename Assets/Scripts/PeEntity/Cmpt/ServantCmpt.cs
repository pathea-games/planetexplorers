//using UnityEngine;
//using System.Collections;

//namespace Pathea
//{
//    public class ServantCmpt : PeCmpt
//    {
//        ServantLeaderCmpt mLeader;

//        public ServantLeaderCmpt leader
//        {
//            get
//            {
//                return mLeader;
//            }
//        }

//        public void Defollow()
//        {
//            if (null == mLeader)
//            {
//                return;
//            }

//            //mLeader.RemoveServant(this);
//            mLeader = null;
//        }

//        public bool IsFollowing(ServantLeaderCmpt leader)
//        {
//            if (null == leader)
//            {
//                return false;
//            }

//            if (leader != mLeader)
//            {
//                return false;
//            }

//            return true;
//        }

//        public void Follow(ServantLeaderCmpt leader)
//        {
//            if (null == leader)
//            {
//                return;
//            }

//            if (IsFollowing(leader))
//            {
//                return;
//            }

//            Defollow();

//            mLeader = leader;
//            //mLeader.AddServant(this);
//        }

//        void Follow(PeEntity entity)
//        {
//            ServantLeaderCmpt leader = entity.GetCmpt<ServantLeaderCmpt>();

//            if (null == leader)
//            {
//                return;
//            }

//            Follow(leader);
//        }

//        public override void Serialize(System.IO.BinaryWriter w)
//        {
//            int leaderId = null != mLeader ? mLeader.Entity.Id : -1;

//            w.Write((int)leaderId);
//        }

//        public override void Deserialize(System.IO.BinaryReader r)
//        {
//            int leaderId = r.ReadInt32();
//            if (leaderId != -1)
//            {
//                PeEntity leader = EntityMgr.Instance.Get(leaderId);
//                if (null == leader)
//                {
//                    Debug.Log("cant find leader by id:" + leaderId);
//                }
//                else
//                {
//                    Follow(leader);
//                }
//            }
//        }

//        public override string ToString()
//        {
//            return string.Format("follower:{0}, leader:{1}", Entity.Id, null == leader ? -11 : leader.Entity.Id);
//        }
//    }

//    namespace PeEntityExtServent
//    {
//        public static class PeEntityExtServent
//        {
//            public static ServantLeaderCmpt GetLeader(this PeEntity entity)
//            {
//                ServantCmpt c = entity.GetCmpt<ServantCmpt>();
//                if(null == c)
//                {
//                    return null;
//                }
//                return c.leader;
//            }

//            public static void ExtDefollow(this PeEntity entity)
//            {
//                ServantCmpt c = entity.GetCmpt<ServantCmpt>();
//                if (null == c)
//                {
//                    return;
//                }

//                c.Defollow();
//            }

//            public static bool ExtIsFollowing(this PeEntity entity, ServantLeaderCmpt leader)
//            {
//                ServantCmpt c = entity.GetCmpt<ServantCmpt>();
//                if (null == c)
//                {
//                    return false;
//                }

//                return c.IsFollowing(leader);
//            }

//            public static bool ExtIsFollowing(this PeEntity entity, PeEntity leader)
//            {
//                if (null == leader)
//                {
//                    return false;
//                }

//                return ExtIsFollowing(entity, leader.GetCmpt<ServantLeaderCmpt>());
//            }

//            public static void ExtFollow(this PeEntity entity, ServantLeaderCmpt leader)
//            {
//                ServantCmpt c = entity.GetCmpt<ServantCmpt>();
//                if (null == c)
//                {
//                    return;
//                }

//                c.Follow(leader);
//            }

//            public static void ExtFollow(this PeEntity entity, PeEntity leader)
//            {
//                ServantLeaderCmpt leaderCmpt = leader.GetCmpt<ServantLeaderCmpt>();

//                if (null == leaderCmpt)
//                {
//                    return;
//                }

//                ExtFollow(entity, leaderCmpt);
//            }
//        }
//    }
//}