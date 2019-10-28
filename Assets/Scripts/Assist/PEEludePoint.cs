using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using PETools;

public class EludePoint
{
    Vector3 m_Position;
    Vector3 m_Direction;
    Vector3 m_FaceDirection;
    bool m_Dirty;

    public EludePoint(Vector3 argPos, Vector3 argDir, Vector3 argFace)
    {
        m_Position = argPos;
        m_Direction = argDir;
        m_FaceDirection = argFace;
    }

    public Vector3 Position
    {
        get { return m_Position; }
    }

    public Vector3 Direction
    {
        get { return m_Direction; }
    }

    public Vector3 FaceDirection
    {
        get { return m_FaceDirection; }
    }

    public bool Dirty
    {
        get { return m_Dirty; }
        set { m_Dirty = value;}
    }

    public bool Elude(Vector3 pos)
    {
        return PEUtil.SqrMagnitudeH(m_Position, pos) < 1.0f * 1.0f;
    }

    public bool CanElude(Vector3 pos)
    {
        int layer = 1 << Pathea.Layer.Building
                    | 1 << Pathea.Layer.Unwalkable;

        return Physics.Raycast(m_Position, pos - m_Position, Vector3.Distance(pos, m_Position), layer);
    }
}

public class PEEludePoint : MonoBehaviour
{
    static List<EludePoint> s_Points = new List<EludePoint>();

    public static EludePoint GetEludePoint(Vector3 pos, Vector3 targetPos)
    {
        EludePoint point = null;
        float minDis = 128.0f*128.0f;

        for (int i = 0; i < s_Points.Count; i++)
        {
            if (s_Points[i].Dirty) continue;

            if (!s_Points[i].CanElude(targetPos)) continue;

            float curDis = PEUtil.SqrMagnitudeH(pos, s_Points[i].Position);
            if (curDis < minDis)
            {
                minDis = curDis;
                point = s_Points[i];
            }
        }

        return point;
    }

    public static void RegisterPoint(EludePoint point)
    {
        if(!s_Points.Contains(point))
        {
            s_Points.Add(point);
        }
    }

    public static void RemovePoint(EludePoint point)
    {
        if (s_Points.Contains(point))
        {
            s_Points.Remove(point);
        }
    }

    public Transform[] points;
    
    void Awake()
    {
        for (int i = 0; i < points.Length; i++)
        {
            if(points[i] != null)
            {
                RegisterPoint(new EludePoint(points[i].position, points[i].forward, points[i].position-transform.position));
            }
        }
    }    
}
