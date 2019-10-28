using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Railway
{
    public class Point
    {
        public enum EType
        {
            Joint = 0,
            Station,
            End,
        }

        EType mType;
        public EType pointType
        {
            get
            {
                return mType;
            }
        }

        int mId;
        public int id
        {
            get
            {
                return mId;
            }

            set
            {
                mId = value;
                station.pointId = value;
            }
        }
        
        Vector3 mPosition;
        public Vector3 position
        {
            get
            {
                return mPosition;
            }
            set
            {
                mPosition = value;
                station.SetPos(mPosition);

                UpdatePrePointLink();
            }
        }
        string mName;
        public string name
        {
            get
            {
                if (string.IsNullOrEmpty(mName))
                {
                    return pointType.ToString() + id;
                }

                return mName;
            }
            set
            {
                mName = value;
            }
        }

        int mPrePointId;
        public int prePointId
        {
            get
            {
                return mPrePointId;
            }

            private set
            {
                mPrePointId = value;
            }
        }

        int mNextPointId;
        public int nextPointId
        {
            get
            {
                return mNextPointId;
            }

            private set
            {
                mNextPointId = value;

                UpdateLinkTarget();
            }
        }

        public int routeId = Manager.InvalId;
        Vector3 mRotation;
        public Vector3 rotation
        {
            get
            {
                return mRotation;
            }
            set
            {
                mRotation = value;

                station.SetRot(mRotation);

                UpdatePrePointLink();
            }
        }

        public float stayTime;
        public float realStayTime
        {
            get
            {
                //if (pointType == Railway.Point.EType.End)
                //{
                //    return 2 * stayTime;
                //}
                return stayTime;
            }
        }

        public int itemInstanceId;

        RailwayStation mStation = null;
        public RailwayStation station
        {
            get
            {
                if (mStation == null)
                {
                    mStation = CreateModel(pointType);
                }

                return mStation;
            }
        }

        Point()
        {
        }

        public Point(int identifier, EType type)
        {
            mType = type;
            id = identifier;
            prePointId = Railway.Manager.InvalId;
            nextPointId = Railway.Manager.InvalId;
            routeId = Railway.Manager.InvalId;
            rotation = Vector3.zero;
        }

        public void ChangePrePoint(int preId)
        {
            if (preId == prePointId)
            {
                return;
            }

            Railway.Point oldPrePoint = GetPrePoint();

            prePointId = preId;

            if (null != oldPrePoint)
            {
                oldPrePoint.ChangeNextPoint(Railway.Manager.InvalId);
            }

            Railway.Point newPrePoint = Railway.Manager.Instance.GetPoint(preId);
            if (null != newPrePoint)
            {
                newPrePoint.ChangeNextPoint(id);
            }

            UpdateRotation();
        }

        public void ChangeNextPoint(int nextID)
        {
            if (nextID == nextPointId)
            {
                return;
            }

            Railway.Point oldNextPoint = GetNextPoint();

            nextPointId = nextID;

            if (null != oldNextPoint)
            {
                oldNextPoint.ChangePrePoint(Railway.Manager.InvalId);
            }

            Railway.Point newNextPoint = Railway.Manager.Instance.GetPoint(nextID);
            if (null != newNextPoint)
            {
                newNextPoint.ChangePrePoint(id);
            }

            UpdateRotation();
        }

        void UpdateRotation()
        {
            Vector3 forwrd = Vector3.zero;
            Railway.Point prePoint = GetPrePoint();
            Railway.Point nextpoint = GetNextPoint();

            Vector3 upDir = Quaternion.Euler(rotation) * Vector3.up;

            if (pointType != Railway.Point.EType.Joint)
            {
                upDir = Vector3.up;
            }

            if (null != prePoint)
            {
                forwrd = (position - prePoint.position).normalized;
            }

            if (null != nextpoint)
            {
                forwrd = (forwrd.normalized + (nextpoint.position - position).normalized).normalized;
            }
            else
            {
                forwrd = -forwrd;
            }

            if (forwrd != Vector3.zero)
            {
                rotation = Quaternion.LookRotation(forwrd, upDir).eulerAngles;
            }
        }

        public void RotUpDir(float angle)
        {
            Quaternion tranRot = Quaternion.Euler(rotation);
            Vector3 foward = tranRot * Vector3.forward;

            tranRot = Quaternion.AngleAxis(angle, foward) * tranRot;
            rotation = tranRot.eulerAngles;
        }

        void UpdatePrePointLink()
        {
            Point prePoint = GetPrePoint();
            if (prePoint == null)
            {
                return;
            }

            prePoint.station.UpdateLink();
        }

        public void UpdateLinkTarget()
        {
            RailwayStation nextStation = null;

            Point nextPoint = GetNextPoint();
            if (nextPoint != null)
            {
                nextStation = nextPoint.station;
            }

            station.LinkTo(nextStation);
        }

        public Railway.Point GetNextPoint()
        {
            return Railway.Manager.Instance.GetPoint(nextPointId);
        }

        public Railway.Point GetPrePoint()
        {
            return Railway.Manager.Instance.GetPoint(prePointId);
        }

        public Vector3 GetLinkPosition()
        {
            return station.mLinkPoint.position;
        }

        public Vector3 GetJointPosition()
        {
            return station.mJointPoint.position;
        }

        public Quaternion GetJointRotation()
        {
            if (null != station)
            {
                return station.mJointPoint.rotation;
            }
            return Quaternion.Euler(rotation);
        }

        public void Destroy()
        {
            Railway.Point getPoint = GetPrePoint();
            if (null != getPoint)
            {
                getPoint.ChangeNextPoint(Railway.Manager.InvalId);
            }

            getPoint = GetNextPoint();

            if (null != getPoint)
            {
                getPoint.ChangePrePoint(Railway.Manager.InvalId);
            }

            if (null != station)
            {
                GameObject.Destroy(station.gameObject);
            }
        }

        static RailwayStation CreateModel(Railway.Point.EType type)
        {
            string path = "TaskJoint";

            switch (type)
            {
                case Railway.Point.EType.Joint:
                    path = "TaskJoint";
                    break;
                case Railway.Point.EType.Station:
                    path = "TaskStation";
                    break;
                case Railway.Point.EType.End:
                default:
                    path = "TaskEnd";
                    break;
            }


            GameObject go = Object.Instantiate(Resources.Load<GameObject>(path));

            if (null == go)
            {
                return null;
            }

            go.transform.parent = Railway.Manager.railRoot;
            go.transform.localScale = Vector3.one;

            return go.GetComponent<RailwayStation>();
        }

        public Transform GetTrans()
        {
            return station.transform;
        }

        public float GetArriveTime()
        {
            Railway.Route route = Railway.Manager.Instance.GetRoute(routeId);
            if (route == null)
            {
                return 0f;
            }

            return route.GetArriveTime(id);
        }

        public byte[] Export()
        {
            return PETools.Serialize.Export((w) =>
            {
                w.Write(itemInstanceId);
                PETools.Serialize.WriteNullableString(w, mName);
                w.Write(stayTime);
                w.Write((int)mType);

                w.Write(id);
                PETools.Serialize.WriteVector3(w, position);
                PETools.Serialize.WriteVector3(w, rotation);

                w.Write(prePointId);
                w.Write(nextPointId);
            });
        }

        public void Import(byte[] data)
        {
            PETools.Serialize.Import(data, (r) =>
            {
                itemInstanceId = r.ReadInt32();
                mName = PETools.Serialize.ReadNullableString(r);
                stayTime = r.ReadSingle();
                mType = (Railway.Point.EType)r.ReadInt32();

                id = r.ReadInt32();
                position = PETools.Serialize.ReadVector3(r);
                rotation = PETools.Serialize.ReadVector3(r);

                prePointId = r.ReadInt32();
                nextPointId = r.ReadInt32();                
            });
        }

        public static Railway.Point GetHeader(Railway.Point point)
        {
            if (point == null)
            {
                return null;
            }

            while (true)
            {
                Railway.Point prePoint = point.GetPrePoint();
                if (prePoint == null)
                {
                    return point;
                }
                else
                {
                    point = prePoint;
                }
            }
        }

        public static void Travel(Railway.Point point, System.Action<Railway.Point> action)
        {
            if (point == null)
            {
                return;
            }

            while (true)
            {
                action(point);

                Railway.Point nextPoint = point.GetNextPoint();

                if (nextPoint == null)
                {
                    break;
                }
                else
                {
                    point = nextPoint;
                }
            }
        }

        public static Railway.Point GetTail(Railway.Point point)
        {
            if (point == null)
            {
                return null;
            }

            while (true)
            {
                Railway.Point nextPoint = point.GetNextPoint();
                if (nextPoint == null)
                {
                    return point;
                }
                else
                {
                    point = nextPoint;
                }
            }
        }

        public static void ReverseNext(Point point)
        {
            if (point == null)
            {
                return;
            }

            List<Railway.Point> pointList = new List<Railway.Point>();
            while (point != null)
            {
                pointList.Add(point);

                point = point.GetNextPoint();
            }

            pointList[pointList.Count - 1].ChangePrePoint(Railway.Manager.InvalId);

            for (int i = 0; i < pointList.Count - 1; i++)
            {
                pointList[i].ChangePrePoint(pointList[i + 1].id);
            }
        }

        public static Point Deserialize(byte[] data)
        {
            Point point = new Point();
            point.Import(data);
            return point;
        }
    }
}