using UnityEngine;

namespace Pathea
{
    public class FollowCmpt : PeCmpt
    {
        const float SmoothingTime = 0.5f;

        SmoothFollower mSmoothFollower;
        [SerializeField]
        Transform mTarget;
        Motion_Move mMove;
        PeTrans mTrans;
        MotionMgrCmpt mMotionMgr;

        public float lostDis = float.MaxValue;
        public float setPosDis = float.MaxValue;

        public override void Awake()
        {
			base.Awake ();
            mSmoothFollower = new SmoothFollower(SmoothingTime);
        }

        public override void Start()
        {
            base.Start();
            
            mMove = Entity.GetCmpt<Motion_Move>();
            mTrans = Entity.peTrans;
            mMotionMgr = Entity.GetCmpt<MotionMgrCmpt>();
        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            if (null == mMove)
            {
                return;
            }

            if (null == mTarget)
            {
                return;
            }

            float dis = Vector3.Distance(mTarget.position, mTrans.position);

            if (dis > setPosDis)
            {
                mTrans.position = mTarget.position;
                return;
            }

            if (dis > lostDis)
            {
                mMotionMgr.EndAction(PEActionType.Move);
                return;
            }

            Vector3 pos = mSmoothFollower.Update(mTarget.position, Time.deltaTime);
            
            //Vector3 pos = mTarget.position;

            mMove.MoveTo(pos);
        }

        public void Follow(PeTrans target, float height = 0f)
        {
            if (null == target)
            {
                return;
            }

            Follow(target.trans, height);
        }

        public void Defollow()
        {
            if (null != mTarget)
            {
                FollowPoint.DestroyFollowTrans(mTarget);
            }
        }

        void Follow(Transform target, float height = 0f)
        {
            Defollow();

            if (null != target)
            {
                mTarget = FollowPoint.CreateFollowTrans(target, height);

                mSmoothFollower.Update(mTarget.position, Time.deltaTime, true);
            }
            else
            {
                mTarget = null;
            }
        }
    }

    namespace PeEntityExtFollow
    {
        public static class PeEntityExtFollow
        {            
            public static void ExtFollow(this PeEntity entity, PeEntity target)
            {
                if(null == target || entity == null)
                {
                    return;
                }

                FollowCmpt f = entity.GetCmpt<FollowCmpt>();
                if (null == f)
                {
                    f = entity.Add<FollowCmpt>();
                }

                PeTrans targetTrans = target.peTrans;
                if (null == targetTrans)
                {
                    return;
                }

                f.Follow(targetTrans);
            }

            public static void ExtDefollow(this PeEntity entity)
            {
                FollowCmpt f = entity.GetCmpt<FollowCmpt>();
                if (null == f)
                {
                    return;
                }

                f.Defollow();
            }

            public static void ExtSetLostDis(this PeEntity entity, float dis)
            {
                FollowCmpt f = entity.GetCmpt<FollowCmpt>();
                if (null == f)
                {
                    return;
                }

                f.lostDis = dis;
            }

            public static void ExtSetSetPosDis(this PeEntity entity, float dis)
            {
                FollowCmpt f = entity.GetCmpt<FollowCmpt>();
                if (null == f)
                {
                    return;
                }

                f.setPosDis = dis;
            }
        }
    }
}