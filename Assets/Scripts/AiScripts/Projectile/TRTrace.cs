using UnityEngine;
using System.Collections;

public class TRTrace : Trajectory
{
    public float lerpValue;
    public float speed;
    public float angle;
    public float deviation;
    public bool isTrace;

    Vector3 m_CurMoveDir;
    Vector3 m_OldCenter;
    Vector3 m_Deviation;

    //bool m_CanTrace;

    void Start()
    {
        //m_CanTrace = true;
        m_OldCenter = GetTargetCenter();
        m_Deviation = Random.insideUnitSphere.normalized * Random.Range(0.0f, deviation);
        m_CurMoveDir = GetTargetPosition() - transform.position;
        transform.rotation = Quaternion.LookRotation(m_CurMoveDir);

        if (m_Index > 0)
        {
            m_CurMoveDir = Quaternion.AngleAxis(Random.Range(0.0f, 360.0f), transform.forward) * transform.right;
            Vector3 axis = Vector3.Cross(m_CurMoveDir, transform.forward);
            m_CurMoveDir = Quaternion.AngleAxis(Random.Range(0.0f, 90.0f), axis) * m_CurMoveDir;
        }
    }

    Vector3 GetTargetPosition()
    {
        if (isTrace)
            return GetTargetCenter() + m_Deviation;
        else
            return m_OldCenter + m_Deviation;
    }

    public override Vector3 Track(float deltaTime)
    {
        //if (m_CanTrace)
        //{
        //    Vector3 desiredMoveDirection = GetTargetCenter() - transform.position;
        //    if (Vector3.Angle(m_CurMoveDir, desiredMoveDirection) <= angle)
        //        m_CurMoveDir = Vector3.Slerp(m_CurMoveDir, desiredMoveDirection, lerpValue * deltaTime);
        //    else
        //        m_CanTrace = false;
        //}

        Vector3 desiredMoveDirection = GetTargetPosition() - transform.position;
        if (Vector3.Angle(m_CurMoveDir, desiredMoveDirection) <= angle)
            m_CurMoveDir = Vector3.Slerp(m_CurMoveDir, desiredMoveDirection, lerpValue * deltaTime);

        return m_CurMoveDir.normalized * speed * deltaTime;
    }

    public override Quaternion Rotate(float deltaTime)
    {
        return Quaternion.LookRotation(m_CurMoveDir);
    }
}
