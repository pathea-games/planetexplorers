using UnityEngine;
using WhiteCat;
using System;
using System.Collections;

namespace Pathea
{
	public class PassengerCmpt : PeCmpt, Railway.IPassenger, WhiteCat.IVCPassenger
    {
        PeTrans 		mPeTrans;
		PESkEntity 		mSkEntity;
        MotionMgrCmpt 	mMotionMgr;
        int mRailRouteId = Railway.Manager.InvalId;

        bool m_UpperAir;
        float m_UpperAirStartTime;

		WhiteCat.VCPBaseSeat m_Seat;

		WhiteCat.CarrierController m_DrivingController;

		public Action<WhiteCat.CarrierController> onGetOnCarrier;
		public Action<WhiteCat.CarrierController> onGetOffCarrier;
		
		public WhiteCat.VCPBaseSeat seat
		{
		    get
		    {
				return m_Seat;
		    }
		}

		public WhiteCat.CarrierController carrier
		{
			get
			{
				return (m_Seat != null) ? m_Seat.drivingController : null;
			}
		}

		public void UpdateHeadInfo()
		{
			if(mSkEntity != null)
			{
				PeEntity entity = EntityMgr.Instance.Get(mSkEntity.GetId());
				if(entity != null)
				{
					EntityInfoCmpt info1 = entity.GetCmpt<EntityInfoCmpt>();
					if(info1 != null )
					{
						info1.OverHead.UpdateTransform();
					}
				}
			}

		}

		public WhiteCat.CarrierController drivingController
		{
		    get
		    {
				return m_DrivingController;
		    }
		}

        public override void Start()
        {
			base.Start ();
			mPeTrans = Entity.peTrans;
			mSkEntity = Entity.GetComponent<PESkEntity>();
            mMotionMgr = Entity.GetCmpt<MotionMgrCmpt>();

            if (mRailRouteId != Railway.Manager.InvalId)
            {
				if(null != Railway.Manager.Instance.GetRoute(mRailRouteId))
					DoGetOn(mRailRouteId, true);
				else
					mRailRouteId = Railway.Manager.InvalId;
            }
        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            UpdatePlayerUpperAir();
        }

        void UpdatePlayerUpperAir()
        {
            if(!PeGameMgr.IsMulti && Entity == PeCreature.Instance.mainPlayer)
            {
                if (carrier == null || !(carrier is HelicopterController))
                    m_UpperAir = false;
                else
                {
                    float baseHeight = PeMappingMgr.Instance.GetTerrainHeight(Entity.position);

                    bool isHeight = Entity.position.y - baseHeight > 30.0f;
                    bool isRunning = carrier.rigidbody != null && carrier.rigidbody.velocity.sqrMagnitude > 1f * 1f;

                    m_UpperAir = isHeight && isRunning;
                }
            }

            if (!m_UpperAir)
                m_UpperAirStartTime = 0.0f;
            else
            {
                if(m_UpperAirStartTime < PETools.PEMath.Epsilon)
                    m_UpperAirStartTime = Time.time;
            }

            if(m_UpperAir && m_UpperAirStartTime > 0.0f)
            {
                if(Time.time - m_UpperAirStartTime > 10.0f)
                {
                    m_UpperAirStartTime = Time.time;

                    float randomValue = UnityEngine.Random.value;

                    if(randomValue < 0.015f)
                    {
                        Vector3 position = PETools.PEUtil.GetRandomPosition(Entity.position, 25.0f, 35.0f);
                        MonsterEntityCreator.CreateMonster(73, position);
                        Debug.Log("Spawn caelum rex in upper air : " + randomValue + " time : " + Time.time);
                    }
                }
            }
        }

        void Railway.IPassenger.GetOn(string pose)
        {
            mMotionMgr.FreezePhyState(GetType(), true);
			mMotionMgr.SetMaskState(PEActionMask.OnVehicle, true);
			PEActionParamS param = PEActionParamS.param;
			param.str = pose;
			mMotionMgr.DoActionImmediately(PEActionType.GetOnTrain, param);
        }

        void Railway.IPassenger.GetOff(Vector3 pos)
        {
			mMotionMgr.FreezePhyState(GetType(), false);
			mMotionMgr.SetMaskState(PEActionMask.OnVehicle, false);
		}
		
		void Railway.IPassenger.UpdateTrans(UnityEngine.Transform trans)
        {
            mPeTrans.position = trans.position;
            mPeTrans.rotation = trans.rotation;
        }

		bool DoGetOn(int railRouteId, bool checkState = true)
        {
			if(checkState && !mMotionMgr.CanDoAction(PEActionType.GetOnTrain))
			{
				return false;
			}

            Railway.Route route = Railway.Manager.Instance.GetRoute(railRouteId);
			if (route == null)
			{
                Debug.LogError("cant find route to get on, route id:" + railRouteId);
				return false;
			}
			
			if (!route.AddPassenger(this))
			{
                Debug.LogError("get on failed, route id:" + railRouteId);
				return false;
			}
			
			mRailRouteId = railRouteId;
 
            return true;
        }

        public bool IsOnRail
        {
            get
            {
                if (mRailRouteId == Railway.Manager.InvalId)
                {
                    return false;
                }

                return true;
            }
        }

        public int railRouteId
        {
            get
            {
                return mRailRouteId;
            }
            //set
            //{
            //    mRailRouteId = value;
            //}
        }

        public bool GetOn(int railRouteId, bool checkState = true)
        {
            if (railRouteId == Railway.Manager.InvalId)
            {
                return false;
            }

			DoGetOn(railRouteId, checkState);

            return true;
        }

        public bool GetOff(Vector3 getOffPos)
        {
            Railway.Route route = Railway.Manager.Instance.GetRoute(mRailRouteId);
            if (route == null)
            {
                Debug.LogError("cant find route to get off, route id:" + mRailRouteId);
                return false;
            }
			mMotionMgr.EndAction(PEActionType.GetOnTrain);
            mRailRouteId = Railway.Manager.InvalId;
            return route.RemovePassenger(this, getOffPos);
        }

        public bool GetOff()
        {
            Railway.Route route = Railway.Manager.Instance.GetRoute(mRailRouteId);
            if (route == null)
            {
                Debug.LogError("cant find route to get off, route id:" + mRailRouteId);
                return false;
            }
			
			mMotionMgr.EndAction(PEActionType.GetOnTrain);
            mRailRouteId = Railway.Manager.InvalId;
            return route.RemovePassenger(this);
        }

        public override void Deserialize(System.IO.BinaryReader r)
        {
            base.Deserialize(r);

            mRailRouteId = r.ReadInt32();
        }

        public override void Serialize(System.IO.BinaryWriter w)
        {
            base.Serialize(w);

            w.Write(mRailRouteId);
        }

		void OnGetOnSucceed(WhiteCat.CarrierController controller, int seatIndex)
		{			
			m_DrivingController = controller;
			mIsOnVCCarrier = true;

			if(null != onGetOnCarrier)
				onGetOnCarrier(carrier);
		}
		
		public void GetOn(WhiteCat.CarrierController controller, int seatIndex, bool checkState)
		{
			if (null != mMotionMgr)
			{
				PEActionParamDrive param = PEActionParamDrive.param;
				param.controller = controller;
				param.seatIndex = seatIndex;
				if(!checkState)
				{
					mMotionMgr.DoActionImmediately(PEActionType.Drive, param);
					OnGetOnSucceed(controller, seatIndex);
				}
				else if(mMotionMgr.DoAction(PEActionType.Drive, param))
				{
					OnGetOnSucceed(controller, seatIndex);
				}
			}
		}

        public void GetOffCarrier()
        {
			if(null == seat)
				return;

			Vector3 getOffPos;
			if(seat.FindGetOffPosition(out getOffPos))
			{
				if (GameConfig.IsMultiMode)
					mSkEntity._net.RPCServer(EPacketType.PT_InGame_GetOffVehicle, getOffPos);
				else
				{
					if(mMotionMgr.EndAction(PEActionType.Drive))
						mPeTrans.position = getOffPos;
				}
			}
        }

		#region IVCPassenger implementation
        bool mIsOnVCCarrier = false;
        void WhiteCat.IVCPassenger.GetOn(string sitAnimName, VCPBaseSeat seat)
		{
			m_Seat = seat;
            if (null != mMotionMgr)
            {
				Action_Drive drive = mMotionMgr.GetAction<Action_Drive>();
				if(null != drive)
					drive.SetSeat(sitAnimName, seat);
            }
		}

		public void GetOffCarrier(Vector3 pos)
		{
			m_DrivingController = null;
			m_Seat = null;
			mIsOnVCCarrier = false;			
			if(null != onGetOffCarrier && null != carrier)
				onGetOffCarrier(carrier);
		}

        void WhiteCat.IVCPassenger.GetOff()
        {
            if (null != mMotionMgr)
            {
                //mMotionMgr.DoAction(PEActionType.GetOnVehicle, sitAnimName, seat);
				m_DrivingController = null;
				m_Seat = null;
                mIsOnVCCarrier = false;

				if(null != onGetOffCarrier)
					onGetOffCarrier(carrier);
            }
        }

        void WhiteCat.IVCPassenger.Sync(Vector3 position, Quaternion rotation)
		{
			if(null != mPeTrans)
			{
				mPeTrans.position = position;
				mPeTrans.rotation = rotation;
			}
		}
        void WhiteCat.IVCPassenger.SetHands(Transform left, Transform right)
		{
			if (null != mMotionMgr)
			{
				Action_Drive drive = mMotionMgr.GetAction<Action_Drive>();
				if(null != drive)
					drive.SetHand(left, right);
			}
		}
		#endregion

        public bool IsOnCarrier()
        {
            return mIsOnVCCarrier || IsOnRail;
        }

        public bool IsOnVCCarrier
        {
            get { return mIsOnVCCarrier; }
        }
	}
}