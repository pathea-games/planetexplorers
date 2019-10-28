using UnityEngine;
using System.Collections;
using Pathfinding;

public class PointPath
{
    int m_Index;
    bool m_IsLoop;
    bool m_IsGravity;
    float m_PickLength;
    Vector3[] m_Points;

    public PointPath(Vector3[] points, float pickLength = 0.25f, bool loop = false, bool isGravity = true)
    {
        m_Index = -1;
        m_IsLoop = loop;
        m_IsGravity = isGravity;
        m_PickLength = pickLength;
        m_Points = points;
    }

    float GetSqrDistance(Vector3 p1, Vector3 p2)
    {
        if (m_IsGravity)
            return PETools.PEUtil.SqrMagnitudeH(p1, p2);
        else
            return PETools.PEUtil.SqrMagnitude(p1, p2);
    }

    int GetClosestIndex(Vector3 pos)
    {
        if (m_Points == null || m_Points.Length <= 0)
            return -1;

        int idx = 0;
        float sqrDis = GetSqrDistance(pos, m_Points[0]);

        for (int i = 1; i < m_Points.Length; i++)
        {
            float tmpDis = GetSqrDistance(pos, m_Points[i]);
            if (tmpDis < sqrDis)
            {
                idx = i;
                sqrDis = tmpDis;
            }
        }

        return idx;
    }

    public Vector3 GetNextPoint(Vector3 pos)
    {
        if (m_Index == -1)
            m_Index = GetClosestIndex(pos);

        float sqrDis = GetSqrDistance(pos, m_Points[m_Index]);
        if(sqrDis < m_PickLength * m_PickLength)
        {
            m_Index++;

            if(m_Index >= m_Points.Length)
            {
                if (m_IsLoop)
                    m_Index = 0;
                else
                    m_Index = m_Points.Length - 1;
            }
        }

        if (m_Index >= 0 && m_Index < m_Points.Length)
            return m_Points[m_Index];
        else
            return Vector3.zero;
    }
}

public class Bezier
{
    float tangentLengths;

    Vector3[] points;

    float time;

    public Bezier(float argTangentLengths = 5.0f)
    {
        time = 0.0f;
        points = new Vector3[0];
        tangentLengths = argTangentLengths;
    }

    public Bezier(Vector3[] argPoints, float argTangentLengths = 5.0f)
    {
        time = 0.0f;
        points = argPoints;
        tangentLengths = argTangentLengths;
    }

    Vector3 CubicBezier(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        t = Mathf.Clamp01(t);
        float t2 = 1 - t;
        return t2 * t2 * t2 * p0 + 3 * t2 * t2 * t * p1 + 3 * t2 * t * t * p2 + t * t * t * p3;
    }

    public void SetLength(float length)
    {
        tangentLengths = length;
    }

    public void Insert(Vector3 v)
    {
        System.Array.Resize(ref points, points.Length + 1);
        points[points.Length-1] = v;
    }

    public void Refill(Vector3[] ps)
    {
        points = ps;
    }

    public float GetTime(Vector3 pos, float speed)
    {
        float mn = time;
        float mx = time + 1;

        while (mx - mn > 0.0001f)
        {
            float mid = (mn + mx) / 2;

            Vector3 p = Plot(mid);
            if ((p - pos).sqrMagnitude > (speed * Time.deltaTime) * (speed * Time.deltaTime))
                mx = mid;
            else
                mn = mid;
        }

        time = (mn + mx) / 2;

        return time;
    }

    public Vector3 Plot(float t)
    {
        Vector3 inTang, outTang;

        int c = points.Length;
        int pt = Mathf.FloorToInt(t);

        inTang = ((points[(pt + 1) % c] - points[(pt + 0) % c]).normalized - (points[(pt - 1 + c) % c] - points[(pt + 0) % c]).normalized).normalized;

        outTang = ((points[(pt + 2) % c] - points[(pt + 1) % c]).normalized - (points[(pt - 0 + c) % c] - points[(pt + 1) % c]).normalized).normalized;

        return CubicBezier(points[pt % c], points[pt % c] + inTang * tangentLengths, points[(pt + 1) % c] - outTang * tangentLengths, points[(pt + 1) % c], t - pt);
    }

    public void OnDrawGizmos()
    {
        if (points.Length >= 3)
        {
            for (int pt = 0; pt < points.Length; pt++)
            {
                int c = points.Length;
                Vector3 inTang = ((points[(pt + 1) % c] - points[pt + 0]).normalized - (points[(pt - 1 + c) % c] - points[pt + 0]).normalized).normalized;

                Vector3 outTang = ((points[(pt + 2) % c] - points[(pt + 1) % c]).normalized - (points[(pt - 0 + c) % c] - points[(pt + 1) % c]).normalized).normalized;

                Vector3 pp = points[pt];

                for (int i = 1; i <= 100; i++)
                {
                    Vector3 p = CubicBezier(points[pt], points[pt] + inTang * tangentLengths, points[(pt + 1) % c] - outTang * tangentLengths, points[(pt + 1) % c], i / 100.0f);
                    Gizmos.DrawLine(pp, p);
                    pp = p;
                }
            }

        }
    }
}