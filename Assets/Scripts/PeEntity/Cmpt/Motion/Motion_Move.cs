using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using AnimFollow;
using Steer3D;

namespace Pathea
{
    public enum SpeedState
    {
        None,
        Walk,
        Run,
		Sprint,
        Retreat
    }

    public enum MovementState
    {
        None,
        Ground,
        Water,
        WaterSurface,
        Air
    }

    public enum MovementField
    {
        None,
        Land,
        water,
        Sky,
        Amphibian,
        All,
    }

	public enum MoveStyle
	{
		Normal = 0,
		Sword,
		Rifle,
		HandGun,
		Bow,
		Shotgun,
		Mine,
		Carry,
		BeCarry,
		Grenade,
		Gloves,
		HeavyEquipment,
		Drill,
		Abnormal,
		Crouch
	}
	
	public enum MoveMode
	{
		ForwardOnly = 0,
		EightAaxis,
	}
	
	public class NetTranInfo
	{
		public Vector3 pos;
		public Vector3 rot;
		public SpeedState speed;
		public double contrllerTime;
	}

	public abstract class Motion_Move : PeCmpt
	{
		protected SpeedState m_SpeedState;

		protected MovementState m_State;

		protected MoveStyle	m_Style;

		protected MoveMode	m_Mode;

		protected static Stack<NetTranInfo> g_NetTranInfos = new Stack<NetTranInfo>();

		protected NetTranInfo GetNetTransInfo()
		{
			NetTranInfo retInfo = null;
			if(g_NetTranInfos.Count > 0)
				retInfo = g_NetTranInfos.Pop();
			if(null == retInfo)
				retInfo = new NetTranInfo();
			return retInfo;
		}

		protected void RecycleNetTranInfo(NetTranInfo info)
		{
			g_NetTranInfos.Push(info);
		}

		public abstract void Move(Vector3 dir, SpeedState state = SpeedState.Walk);

        public abstract void SetSpeed(float Speed);

        public abstract void MoveTo(Vector3 targetPos, SpeedState state = SpeedState.Walk,bool avoid = true);

		public abstract void NetMoveTo(Vector3 position, Vector3 moveVelocity, bool immediately = false);

		public abstract void NetRotateTo(Vector3 eulerAngle);

        public abstract void Jump();

		public virtual void Dodge(Vector3 dir) { }

		public virtual SpeedState speed { set { m_SpeedState = value; } get{ return m_SpeedState; } }

		public virtual MovementState state{ set{ m_State = value; } get{ return m_State; } }

		public virtual MoveStyle baseMoveStyle { get; set; }

		public virtual MoveStyle style{ set{ m_Style = value; } get{ return m_Style; } }

		public virtual MoveMode mode{ set{ m_Mode = value; } get{ return m_Mode; } }

        public virtual void RotateTo(Vector3 targetDir) { }

		public virtual Vector3 velocity { get { return Vector3.zero; } set{}}

		public virtual Vector3 movement { get { return Vector3.zero; } }

		public virtual void ApplyForce(Vector3 power, ForceMode mode) {}

        public virtual bool Stucking(float time) { return false; }

        public virtual bool grounded { get { return false; } }

        public virtual float gravity { get { return 0.0f; } set { } }

        public virtual void Stop() { }



        //Steer3D
        protected SteerAgent m_Steer;
        public Seek AlterSeekBehaviour(Vector3 target, float slowingRadius, float arriveRadius, float weight = 1f)
        {
            return m_Steer != null ? m_Steer.AlterSeekBehaviour(target, slowingRadius, arriveRadius, weight) : null;
        }

        public Seek AlterSeekBehaviour(Transform target, float slowingRadius, float arriveRadius, float weight = 1f)
        {
            return m_Steer != null ? m_Steer.AlterSeekBehaviour(target, slowingRadius, arriveRadius, weight) : null;
        }
        //net move
        protected List<NetTranInfo> mNetTransInfos = new List<NetTranInfo>();
        public void AddNetTransInfo(Vector3 pos, Vector3 rot, SpeedState speed, double controllerTime)
        {
            NetTranInfo netTransInfo = GetNetTransInfo();
            netTransInfo.pos = pos;
            netTransInfo.rot = rot;
            netTransInfo.speed = speed;
            netTransInfo.contrllerTime = controllerTime;
            mNetTransInfos.Add(netTransInfo);
        }

    }

    namespace PeEntityExtMotion_Move
    {
        public static class PeEntityExtMotion_Move
        {
            #region Move
            public static float GetWalkSpeed(this PeEntity entity)
            {
                return 1f;
            }

            public static void SetWalkSpeed(this PeEntity entity, float value)
            {

            }

            public static float GetRunSpeed(this PeEntity entity)
            {
                return 1f;
            }

            public static void SetRunSpeed(this PeEntity entity, float value)
            {

            }

            public static void MoveTo(this PeEntity entity, Vector3 dest, float stopDistance, float speed)
            {
                Motion_Move mm = entity.GetCmpt<Motion_Move>();
                if (mm == null)
                    return;

                mm.SetSpeed(speed);
                mm.MoveTo(dest);
            }

            public static void MoveTo(this PeEntity entity, Vector3 dst)
            {
                Motion_Move mm = entity.GetCmpt<Motion_Move>();
                if (mm == null)
                    return;

                mm.MoveTo(dst);
            }

            public static void PatrolMoveTo(this PeEntity entity, Vector3 dst)
            {

            }

            public static bool DisableMoveCheck(this PeEntity entity)
            {
                return false;
            }

            public static bool EnableMoveCheck(this PeEntity entity)
            {
                return false;
            }

            public static void CmdFaceToPoint(this PeEntity entity, Vector3 point)
            {

            }

            public static void CmdFaceToDirect(this PeEntity entity, Vector3 direct)
            {

            }

            public static void CmdFollow(this PeEntity entity, Transform dst)
            {

            }

            public static Transform GetFollowing(this PeEntity entity)
            {
                return null;
            }

            public static void StopMove(this PeEntity entity) { }

            #endregion
        }
    }
}
