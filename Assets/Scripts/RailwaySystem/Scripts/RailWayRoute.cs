using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Railway
{
    public class Route
    {
        public class Stats
        {
            public int stationNum;
            public int jointNum;
            public float totalDis;

            public int totalIntDis
            {
                get
                {
                    return (int)totalDis;
                }
            }
        }

        public class RunState
        {
            float[] mScheduleForward;
            float[] mScheduleBackward;

            public int moveDir;
            int mNextPointIndex;
            public float timeToLeavePoint;

            void ToggleTrainDirect()
            {
                moveDir *= -1;
            }

            float[] currentTable
            {
                get
                {
                    return (moveDir == 1) ? mScheduleForward : mScheduleBackward;
                }
            }

			public void SetCurrentStation(int index)
			{
				if (index >= currentTable.Length - 1) 
				{
					mNextPointIndex = 0;
					timeToLeavePoint = 30f;
					ToggleTrainDirect ();
				}
				else
				{
					mNextPointIndex = index;
					timeToLeavePoint = 30f;
				}
			}

            public void Reset()
            {
                moveDir = 1;
                mNextPointIndex = 0;
                timeToLeavePoint = 0f;
            }

            public void Update(float deltaTime)
            {
                timeToLeavePoint -= deltaTime;
                if(timeToLeavePoint < 0f)
                {
                    mNextPointIndex++;

                    if (mNextPointIndex >= currentTable.Length)
                    {
                        timeToLeavePoint = 0;
                        mNextPointIndex = 0;

                        ToggleTrainDirect();
                    }
                    else
                    {
                        timeToLeavePoint += currentTable[mNextPointIndex] - currentTable[mNextPointIndex-1];
                    }
                }
            }

            public void SyncRunState(int direction, int nextIndex, float timeToLeave)
            {
                if (moveDir != direction || Mathf.Abs(nextIndex - mNextPointIndex) > 1
                    || Mathf.Abs(timeToLeave - timeToLeavePoint) > 5f)
                {
                    moveDir = direction;
                    mNextPointIndex = nextIndex;
                    timeToLeavePoint = timeToLeave;
                }
            }

            public double singleTripTime
            {
                get
                {
                    return mScheduleForward[mScheduleForward.Length - 1];
                }
            }

            public int nextPointIndex
            {
                get
                {
                    return (moveDir == 1) ? mNextPointIndex : (currentTable.Length - 1 - mNextPointIndex);
                }
            }

            public float timeToLeaveCurPoint
            {
                get
                {
                    return timeToLeavePoint;
                }
            }

            public void SetSchedule(float[] forwardSchedule, float[] backwardSchedule)
            {
                mScheduleForward = forwardSchedule;
                mScheduleBackward = backwardSchedule;
            }

            public float GetLeaveTime(int pointIndex)
            {
                if (moveDir == 1)
                {
                    if (pointIndex >= mNextPointIndex)
                    {
                        return mScheduleForward[pointIndex] - mScheduleForward[mNextPointIndex] + timeToLeaveCurPoint;
                    }
                    else
                    {
                        return mScheduleForward[mScheduleForward.Length - 1] - mScheduleForward[mNextPointIndex] + timeToLeaveCurPoint
                            + mScheduleBackward[mScheduleBackward.Length - 1 - pointIndex];
                    }
                }
                else
                {
                    pointIndex = mScheduleBackward.Length - 1 - pointIndex;

                    if (pointIndex >= mNextPointIndex)
                    {
                        return mScheduleBackward[pointIndex] - mScheduleBackward[mNextPointIndex] + timeToLeaveCurPoint;
                    }
                    else
                    {
                        return mScheduleBackward[mScheduleBackward.Length - 1] - mScheduleBackward[mNextPointIndex] + timeToLeaveCurPoint
                            + mScheduleForward[mScheduleForward.Length - 1 - pointIndex];
                    }
                }
            }

            public byte[] Export()
            {
                return PETools.Serialize.Export((w) =>
                {
                    w.Write(moveDir);
                    w.Write(mNextPointIndex);
                    w.Write(timeToLeavePoint);
                });
            }

            public void Import(byte[] data)
            {
                PETools.Serialize.Import(data, (r) =>
                {
                    moveDir = r.ReadInt32();
                    mNextPointIndex = r.ReadInt32();
                    timeToLeavePoint = r.ReadSingle();
                });
            }
        }

        int mId;
        public int id
        {
            get
            {
                return mId;
            }
        }

        string mName;
        public string name
        {
            get
            {
                if (string.IsNullOrEmpty(mName))
                {
                    return "Line:" + id;
                }
                return mName;
            }
            set
            {
                mName = value;
            }
        }

        int[] mPointIdList;
        Railway.Point[] pointList;
        public int pointCount
        {
            get
            {
                if (null == mPointIdList)
                {
                    return 0;
                }

                return mPointIdList.Length;
            }
        }

        public Point GetPointByIndex(int index)
        {
			if (pointList == null || pointList.Length <= index)
            {
                return null;
            }

            return pointList[index];
        }

        public Point[] GetPointList() 
        {
            return pointList;
        }

		int mTrainId = Pathea.IdGenerator.Invalid;
        public RunState mRunState = new RunState();

        public int moveDir
        {
            set
            {
                mRunState.moveDir = value;
            }
            get
            {
                return mRunState.moveDir;
            }
        }

        public float TimeToLeavePoint
        {
            set
            {
                mRunState.timeToLeavePoint = value;
            }
            get
            {
                return mRunState.timeToLeavePoint;
            }
        }

        public double singleTripTime
        {
            get
            {
                return mRunState.singleTripTime;
            }
        }

        public int trainId
        {
            get
            {
                return mTrainId;
            }
        }

		public Point stayStation{ get; set; }

		public void SetTrainToStation(int pointID)
		{
			if(null != mRunState)
			{
				int index = GetPointIndex (pointID);
				if (-1 != index) 
				{
					mRunState.SetCurrentStation(index);
				}
			}
		}

        public void SetTrain(int trainItemId)
        {
            DestroyTrain();

            mTrainId = trainItemId;

            if (mTrainId != Pathea.IdGenerator.Invalid)
            {
                mTrain = Railway.Manager.GetTrain(mTrainId);
				if(null != mTrain)
					mTrain.mRoute = this;
				else
					mTrainId = Pathea.IdGenerator.Invalid;
            }

            Reset();
        }

        void Reset()
        {
            mRunState.Reset();
        }

        public bool trainRunning
        {
            get
            {
                return mTrainId != Pathea.IdGenerator.Invalid;
            }
        }

        public Stats GetStats()
        {
            Stats stats = new Stats();

            Railway.Point lastPoint = null;
            foreach (Railway.Point point in pointList)
            {
                if (null != lastPoint)
                {
                    stats.totalDis += Vector3.Distance(point.position, lastPoint.position);
                }
                
                lastPoint = point;

                if (point.pointType != Railway.Point.EType.Joint)
                {
                    stats.stationNum++;
                }
                else
                {
                    stats.jointNum++;
                }
            }

            return stats;
        }

        RailwayTrain mTrain;
        public RailwayTrain train
        {
            get
            {
                return mTrain;
            }
        }

        Route()
        {
        
        }

        public Route(int id)
        {
            mId = id;
        }

        public bool SetStayTime(int pointId, float time)
        {
            if (trainRunning)
            {
                return false;
            }

            Railway.Point point = Railway.Manager.Instance.GetPoint(pointId);
            if (point == null)
            {
                return false;
            }

            if (point.routeId != id)
            {
                return false;
            }

            point.stayTime = time;

            UpdateTimeSchedule();

            return true;
        }

        void UpdateTimeSchedule()
        {
            float[] mTimeForward = new float[pointList.Length];

            mTimeForward[0] = pointList[0].stayTime;

            for (int i = 1; i < pointList.Length; i++)
            {
                float dis = Vector3.Distance(pointList[i].position, pointList[i - 1].position);

                float needTime = dis / Railway.Manager.TrainSteerSpeed;

                mTimeForward[i] = mTimeForward[i - 1] + needTime + pointList[i].stayTime;
            }

            float[] mTimeBackward = new float[pointList.Length];

            mTimeBackward[0] = pointList[pointList.Length - 1].stayTime;
            int count = 1;

            for (int i = pointList.Length - 2; i >= 0; i--, count++)
            {
                float needTime = Vector3.Distance(pointList[i].position, pointList[i + 1].position) / Railway.Manager.TrainSteerSpeed;

                mTimeBackward[count] = mTimeBackward[count - 1] + needTime + pointList[i].stayTime;
            }

            //Debug.Log("<color=red>forward:" + mTimeBackward[mTimeBackward.Length - 1] + "backward:" + mTimeBackward[mTimeBackward.Length-1]+"</color>");
            mRunState.SetSchedule(mTimeForward, mTimeBackward);
        }

        public void SetPoints(int[] pointIdList)
        {
            mPointIdList = pointIdList;

            pointList = new Point[mPointIdList.Length];

            for (int i = 0; i < mPointIdList.Length; i++)
            {
                pointList[i] = Railway.Manager.Instance.GetPoint(mPointIdList[i]);
                pointList[i].routeId = id;
            }

            UpdateTimeSchedule();
        }

        void DestroyTrain()
        {
            if (null == mTrain)
            {
                return;
            }

            mTrain.ClearPassenger();
            GameObject.Destroy(mTrain.gameObject);
            mTrain = null;
        }

        public void Destroy()
        {
            foreach (Railway.Point point in pointList)
            {
                point.routeId = Railway.Manager.InvalId;
            }

            DestroyTrain();
        }

        public void Update(float deltaTime)
        {
            if (!trainRunning)
            {
                return;
            }

            mRunState.Update(deltaTime);

            UpdateTrain();
        }

        const float AngleLerpF = 0.025f;
        float mLerpF = 0;
        void UpdateTrain()
        {
			if (train == null)
            {
                return;
            }

            Vector3 forward;
            Vector3 up;

            Vector3 trainPos = GetTrainPosition(out forward, out up);
			train.transform.position = trainPos;
            Vector3 currentForwad = mTrain.transform.rotation * Vector3.forward;
            float angleLerp = Mathf.Min(Vector3.Angle(currentForwad, forward) * AngleLerpF, 1f);
            mLerpF = Mathf.Lerp(mLerpF, angleLerp, 4f * Time.deltaTime);
            forward = Vector3.Lerp(currentForwad, forward, mLerpF);
			train.transform.rotation = Quaternion.LookRotation(forward, up);
        }

        int GetNextPointIndex(out double timeToLeave)
        {
            timeToLeave = mRunState.timeToLeaveCurPoint;
            return mRunState.nextPointIndex;
        }

        public Vector3 GetTrainPosition(out Vector3 forward, out Vector3 up)
        {
            double timeToLeave;
            int targetIndex = GetNextPointIndex(out timeToLeave);
            Railway.Point targetPoint = Railway.Manager.Instance.GetPoint(pointList[targetIndex].id);
            if (pointList[targetIndex].pointType != Railway.Point.EType.Joint && timeToLeave <= pointList[targetIndex].stayTime + PETools.PEMath.Epsilon)
            {
                forward = targetPoint.GetJointRotation() * Vector3.forward;
                if (targetIndex == pointCount - 1)
                    forward = -1 * forward;
                up = Quaternion.Euler(targetPoint.rotation) * Vector3.up;
				stayStation = Railway.Manager.Instance.GetPoint(pointList[targetIndex].id);
				return stayStation.GetJointPosition();
            }
			stayStation = null;
            float timeToArrive = (float)(timeToLeave);
            if (pointList[targetIndex].pointType != Railway.Point.EType.Joint)
                timeToArrive -= pointList[targetIndex].stayTime;
            Vector3 targetPos = targetPoint.GetJointPosition();
            Railway.Point prePoint = Railway.Manager.Instance.GetPoint(pointList[targetIndex - moveDir].id);
            Vector3 prePos = prePoint.GetJointPosition();
            forward = moveDir * (targetPos - prePos).normalized;
            up = Vector3.Lerp(Quaternion.Euler(targetPoint.rotation) * Vector3.up
                              , Quaternion.Euler(prePoint.rotation) * Vector3.up
                              , timeToArrive * Railway.Manager.TrainSteerSpeed / (targetPos - prePos).magnitude);
            return targetPos + (prePos - targetPos).normalized * Railway.Manager.TrainSteerSpeed * timeToArrive;
        }

        public float GetArriveTime(int pointId)
        {
            if (!trainRunning)
            {
                return 0f;
            }

			int pointIndex = GetPointIndex(pointId);

            if (pointIndex == -1)
            {
                return 0f;
            }

            return mRunState.GetLeaveTime(pointIndex) - pointList[pointIndex].stayTime;
        }

		int GetPointIndex(int pointID)
		{
			int pointIndex = -1;
			for (int i = 0; i < pointCount; i++)
			{
				if (pointID == mPointIdList[i])
				{
					pointIndex = i;
					break;
				}
			}

			return pointIndex;
		}
		
		public byte[] Export()
		{
			return PETools.Serialize.Export((w) =>
			                                {
                w.Write(mId);
                PETools.Serialize.WriteNullableString(w, mName);
                w.Write(mTrainId);

                w.Write(mPointIdList.Length);
                for (int i = 0; i < mPointIdList.Length; i++)
                {
                    w.Write(mPointIdList[i]);
                }

                PETools.Serialize.WriteBytes(mRunState.Export(), w);
            });
        }

        public void Import(byte[] data)
        {
            PETools.Serialize.Import(data, (r) =>
            {
                mId = r.ReadInt32();
                mName = PETools.Serialize.ReadNullableString(r);
                int trainId = r.ReadInt32();
                SetTrain(trainId);

                int listCount = r.ReadInt32();
                    
                int[] idList = new int[listCount];
                for (int j = 0; j < listCount; j++)
                {
                    idList[j] = r.ReadInt32();
                }

                SetPoints(idList);

                byte[] buff = PETools.Serialize.ReadBytes(r);

                mRunState.Import(buff);

                Reset();
            });
        }

        public bool HasPassenger()
        {
            if (train == null)
            {
                return false;
            }

            if (!train.HasPassenger())
            {
                return false;
            }

            return true;
        }

        public bool AddPassenger(Railway.IPassenger passenger)
        {
            if (train == null)
            {
                return false;
            }

            return train.AddPassenger(passenger);
        }

        public bool RemovePassenger(Railway.IPassenger passenger)
        {
            if (train == null)
            {
                return false;
            }

            return train.RemovePassenger(passenger);
        }

        public bool RemovePassenger(Railway.IPassenger passenger, Vector3 getOffPos)
        {
            if (train == null)
            {
                return false;
            }

            return train.RemovePassenger(passenger, getOffPos);
        }

        public static Route Deserialize(byte[] data)
        {
            Route route = new Route();
            route.Import(data);
            return route;
        }
    }
}