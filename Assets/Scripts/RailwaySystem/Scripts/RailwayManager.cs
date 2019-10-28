using System.IO;
using System.Collections.Generic;
using UnityEngine;

namespace Railway
{
    public class Manager : Pathea.ArchivableSingleton<Manager>
    {
        public class PointChanged : PeEvent.EventArg
        {
            public bool bAdd;
            public Point point;
        }

        public class RouteChanged : PeEvent.EventArg
        {
            public bool bAdd;
            public Route route;
        }

        public const int Version1 = 1;
        public const int Version2 = Version1 + 1;
        public const int Version3 = Version2 + 1;
        public const int Version4 = Version3 + 1;
        public const int CurrentVersion = Version4;

        public const int InvalId = -1;

        const float RailwayRadius = 3f;
        public const float JointMinDistance = 5f;
        public const float JointMaxDistance = 80f;

        public static float DefaultStayTime = 10 * GameTime.NormalTimeSpeed;
        public static float TrainSteerSpeed = 40f / GameTime.NormalTimeSpeed; 

        Dictionary<int, Railway.Point> mPointDic = new Dictionary<int, Railway.Point>();

        List<Railway.Route> mRouteList = new List<Railway.Route>();
        
        public int saveVersion;

        PeEvent.Event<PointChanged> mPointChangedEventor;
        public PeEvent.Event<PointChanged> pointChangedEventor
        {
            get
            {
                if (null == mPointChangedEventor)
                {
                    mPointChangedEventor = new PeEvent.Event<PointChanged>(this);
                }

                return mPointChangedEventor;
            }
        }

        PeEvent.Event<RouteChanged> mRouteChangedEventor;
        public PeEvent.Event<RouteChanged> routeChangedEventor
        {
            get
            {
                if (null == mRouteChangedEventor)
                {
                    mRouteChangedEventor = new PeEvent.Event<RouteChanged>(this);
                }

                return mRouteChangedEventor;
            }
        }


        public void UpdateTrain(float deltaTime)
        {
            foreach (Railway.Route route in mRouteList)
            {
                route.Update(deltaTime);
            }
        }

        public bool IsRouteNameExist(string newName)
        {
            foreach (Railway.Route route in mRouteList)
            {
                if (route.name == newName)
                {
                    return true;
                }
            }
            return false;
        }

        public bool IsPointNameExist(string newName)
        {
            foreach (Railway.Point point in mPointDic.Values)
            {
                if (point.name == newName)
                {
                    return true;
                }
            }
            return false;
        }

        public Railway.Point AddPoint(Vector3 pos
            , int prePointId
            , Point.EType type = Point.EType.Joint
            , int pointId = -1)
        {
            if (GetPoint(pointId) != null)
            {
                return null;
            }

            if (pointId == InvalId)
            {
                pointId = GetValidPointId();
            }

            Railway.Point point = new Railway.Point(pointId, type);
            point.position = pos;

            if (type == Railway.Point.EType.Station)
            {
                point.stayTime = Railway.Manager.DefaultStayTime;
            }
            else if (type == Railway.Point.EType.End)
            {
                point.stayTime = Railway.Manager.DefaultStayTime;// / 2f;
            }
            else
            {
                point.stayTime = 0f;
            }

            mPointDic[point.id] = point;

            point.ChangePrePoint(prePointId);

            pointChangedEventor.Dispatch(new PointChanged()
            {
                bAdd = true,
                point = point
            });

            return point;
        }

        public bool RemovePoint(int id)
        {
            Point point = GetPoint(id);

            if (null == point)
            {
                return false;
            }

            Route route = GetRouteByPointId(id);
            if (route != null)
            {
                return false;
            }

            point.Destroy();

            bool ret = mPointDic.Remove(id);
            
            pointChangedEventor.Dispatch(new PointChanged()
            {
                bAdd = false,
                point = point
            });
            return ret;
        }

        public Railway.Route CreateRoute(string name, int[] pointArray, int id = InvalId)
        {
            if (null != GetRoute(id))
            {
                return null;
            }

            if (id == InvalId)
            {
                id = GetValidRouteId();
            }

            Railway.Route route = new Railway.Route(id);

            if (!Railway.Manager.Instance.IsRouteNameExist(name))
            {
                route.name = name;
            }

            route.SetPoints(pointArray);

            mRouteList.Add(route);

            routeChangedEventor.Dispatch(new RouteChanged()
            {
                bAdd = true,
                route = route
            });
            return route;
        }

        public bool RemoveRoute(int ID)
        {
            Railway.Route route = GetRoute(ID);
            if (null == route)
            {
                return false;
            }

            mRouteList.Remove(route);
            route.Destroy();

            routeChangedEventor.Dispatch(new RouteChanged()
            {
                bAdd = false,
                route = route
            });

            return true;
        }

        int GetValidPointId()
        {
            int i = 0;
            while (true)
            {
                if (!mPointDic.ContainsKey(++i))
                {
                    return i;
                }
            }
        }

        public Railway.Point GetPoint(int pointID)
        {
            if (mPointDic.ContainsKey(pointID))
            {
                return mPointDic[pointID];
            }

            return null;
        }

        public Railway.Route GetRouteByTrainId(int trainId)
        {
            foreach (Railway.Route route in mRouteList)
            {
                if (route.trainId == trainId)
                {
                    return route;
                }
            }
            return null;
        }

        public Railway.Route GetRouteByPointId(int pointId)
        {
            Railway.Point point = GetPoint(pointId);
            if (null == point)
            {
                return null;
            }

            return GetRoute(point.routeId);
        }

        public Railway.Point GetPoint(string pointName)
        {
            foreach (Railway.Point point in mPointDic.Values)
            {
                if (point.name == pointName)
                    return point;
            }
            return null;
        }

        public List<Railway.Point> GetNearPoint(Railway.Point centerPoint)
        {
            List<Railway.Point> retList = GetNearPoint(centerPoint.position, JointMaxDistance);
            retList.Remove(centerPoint);
            return retList;
        }

        public List<Railway.Point> GetNearPoint(Vector3 pos, float dis = 50f)
        {
            List<Railway.Point> retList = new List<Railway.Point>();
            foreach (Railway.Point point in mPointDic.Values)
            {
                if (Vector3.Distance(point.position, pos) < dis)
                {
                    retList.Add(point);
                }
            }
            return retList;
        }

        int GetValidRouteId()
        {
            int i = 0;
            while (true)
            {
                if (null == GetRoute(++i))
                {
                    return i;
                }
            }
        }

        public Railway.Route GetRoute(int routeId)
        {
            return mRouteList.Find((route) =>
            {
                if (routeId == route.id)
                {
                    return true;
                }
                return false;
            });
        }

        public List<Railway.Route> GetRoutes()
        {
            return mRouteList;
        }

        public List<int> GetIsolatePoint()
        {
            List<int> list = new List<int>(5);
            foreach (KeyValuePair<int, Railway.Point> kv in mPointDic)
            {
                if (kv.Value.routeId == InvalId)
                {
                    list.Add(kv.Key);
                }
            }

            return list;
        }

        public Railway.Point GetStation(Vector3 pos, float range)
        {
            float sqrDis = range * range;
            foreach (Railway.Point point in mPointDic.Values)
            {
                if ((point.pointType == Railway.Point.EType.End || point.pointType == Railway.Point.EType.Station)
                   && (point.position - pos).sqrMagnitude < sqrDis)
                {
                    return point;
                }
            }
            return null;
        }

        public void GetTwoPointClosest(Vector3 origin,Vector3 dest,out Point start,out Point end,out int startIndex,out int endIndex) 
        {
            start = null;
            end = null;
            startIndex = -1;
            endIndex = -1;

			if(null == mRouteList || mRouteList.Count == 0)
				return;
			
			float closetDis = 0;
			float routeDis;
			float minDisToStart = 0;
			float minDisToEnd = 0;
			int indexToStart = 0;
			int indexToEnd = 0;

			for(int i = 0; i < mRouteList.Count; ++i)
			{
				Route route = mRouteList[i];
				if(null == route)
					continue;
				Point[] pointList = route.GetPointList();
				if(null == pointList)
					continue;

				indexToStart = -1;
				indexToEnd = -1;
				minDisToStart = -1;
				minDisToEnd = -1;

				for(int pointIndex = 0; pointIndex < pointList.Length; ++pointIndex)
				{
					Point point = pointList[pointIndex];
					if(null == point || point.pointType == Point.EType.Joint)
						continue;
					float disToStart = Vector3.Distance(origin, point.position);
					if(minDisToStart <= PETools.PEMath.Epsilon || disToStart < minDisToStart)
					{
						minDisToStart = disToStart;
						indexToStart = pointIndex;
					}
					float disToEnd = Vector3.Distance(dest, point.position);
					if(minDisToEnd <= PETools.PEMath.Epsilon || disToEnd < minDisToEnd)
					{
						minDisToEnd = disToEnd;
						indexToEnd = pointIndex;
					}
				}
				
				routeDis = minDisToStart + minDisToEnd;
				
				if (closetDis < PETools.PEMath.Epsilon || routeDis < closetDis)
				{
					closetDis = routeDis;
					start = pointList[indexToStart];
					startIndex = indexToStart;
					end = pointList[indexToEnd];
					endIndex = indexToEnd;
				}
			}
        }

        public Railway.Point GetEndPoint(Vector3 pos, float range)
        {
            float sqrDis = range * range;
            foreach (Railway.Point point in mPointDic.Values)
            {
                if (point.pointType == Railway.Point.EType.End
                    && (point.position - pos).sqrMagnitude < sqrDis)
                {
                    return point;
                }
            }
            return null;
        }

        public Railway.Point GetAnotherEndPoint(Railway.Point endPoint)
        {
            Railway.Route route = GetRoute(endPoint.routeId);
            if (null != route)
            {
                if (route.GetPointByIndex(0) == endPoint)
                {
                    return route.GetPointByIndex(route.pointCount - 1);
                }
                else
                {
                    return route.GetPointByIndex(0);
                }
            }
            return null;
        }

        public static RailwayTrain GetTrain(int itemInstanceId)
        {
            ItemAsset.ItemObject trainItem = ItemAsset.ItemMgr.Instance.Get(itemInstanceId);
            if (null == trainItem)
            {
                return null;
            }

            ItemAsset.Instantiate train = trainItem.GetCmpt<ItemAsset.Instantiate>();
            if (null == train)
            {
                return null;
            }

            GameObject obj = train.CreateViewGameObj(null);

            if (null == obj)
            {
                return null;
            }

            obj.transform.parent = railRoot;

            return obj.GetComponent<RailwayTrain>();
        }

        static Transform sRailRoot = null;
        public static Transform railRoot
        {
            get
            {
                if (sRailRoot == null)
                {
                    sRailRoot = new GameObject("RailRoot").transform;
                }

                return sRailRoot;
            }
        }

        public static bool CheckLinkState(Vector3 linkPos1, Vector3 linkPos2, Transform trans1, Transform trans2)
        {
			int ignoreLayer = (1 << Pathea.Layer.Player)
					| (1 << Pathea.Layer.AIPlayer)
					| (1 << Pathea.Layer.Ragdoll)
					| (1 << Pathea.Layer.GIEProductLayer)
					| (1 << Pathea.Layer.EnergyShield)
					| (1 << Pathea.Layer.TreeStatic)
					| (1 << Pathea.Layer.NearTreePhysics);

            Vector3 dir = linkPos1 - linkPos2;

            //Debug.DrawRay(linkPos2, dir, Color.green);

            Vector3 ajustedPos = linkPos2 + RailwayRadius * Vector3.up;

            //Debug.DrawRay(ajustedPos, dir, Color.cyan);

            Ray ray = new Ray(ajustedPos, dir.normalized);

            RaycastHit[] hitInfos = Physics.SphereCastAll(ray, RailwayRadius, dir.magnitude, -1 & ~ignoreLayer, QueryTriggerInteraction.Ignore);

            foreach (RaycastHit info in hitInfos)
            {
                if (Vector3.Distance(info.point, linkPos1) > RailwayRadius
                    && Vector3.Distance(info.point, linkPos2) > RailwayRadius
                    && !info.transform.IsChildOf(trans1)
                    && !info.transform.IsChildOf(trans2)
                    )
                {
                    return false;
                }
            }

            return true;
        }

        public static bool CheckLinkState(Point point1, Point point2)
        {
            if (point1 == null || point2 == null)
            {
                return false;
            }

            return CheckLinkState(point1.GetLinkPosition(), point2.GetLinkPosition(), point1.GetTrans(), point2.GetTrans());
        }

        #region Serialize
        void Export(BinaryWriter w)
        {
            w.Write(CurrentVersion);

            w.Write(mPointDic.Count);
            foreach (Railway.Point point in mPointDic.Values)
            {
                PETools.Serialize.WriteBytes(point.Export(), w);
            }

            w.Write(mRouteList.Count);
            foreach (Railway.Route route in mRouteList)
            {
                PETools.Serialize.WriteBytes(route.Export(), w);
            }
            //-----------
            w.Write(StroyManager.m_Passengers.Count);
            foreach (var item in StroyManager.m_Passengers)
                PETools.Serialize.WriteBytes(StroyManager.m_Passengers[item.Key].Export(), w);
        }

        //network need it to resore
        public void Import(byte[] data)
        {
            PETools.Serialize.Import(data, (r) =>
            {
                saveVersion = r.ReadInt32();

                int count = r.ReadInt32();
                for (int i = 0; i < count; i++)
                {
                    Railway.Point point = Point.Deserialize(PETools.Serialize.ReadBytes(r));

                    mPointDic[point.id] = point;
                }

                count = r.ReadInt32();
                for (int i = 0; i < count; i++)
                {
                    Railway.Route route = Railway.Route.Deserialize(PETools.Serialize.ReadBytes(r));

                    mRouteList.Add(route);
                }
                //-----------
                if (saveVersion >= Version4)
                {
                    StroyManager.m_Passengers.Clear();
                    int n = r.ReadInt32();
                    byte[] buff;
                    for (int i = 0; i < n; i++)
                    {
                        buff = PETools.Serialize.ReadBytes(r);
                        PassengerInfo pi = new PassengerInfo();
                        pi.Import(buff);

                        StroyManager.m_Passengers.Add(pi.npcID, pi);
                    }
                }
            });

            foreach (Point point in mPointDic.Values)
            {
                point.UpdateLinkTarget();
            }
        }

        protected override void WriteData(BinaryWriter bw)
        {
            Export(bw);
        }

        protected override void SetData(byte[] data)
        {
            Import(data);
        }

        protected override string GetArchiveKey()
        {
            return "ArchiveKeyRailwaySystem";
        }
        #endregion
    }
}