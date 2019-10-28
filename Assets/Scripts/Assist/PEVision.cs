using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SkillSystem;
using Pathea;

public class PEVision : PEPerception
{
    public float radius = 0.0f;
    public float angle = 0.0f;
    public Vector3 axis = Vector3.forward;

    List<PeEntity> m_Entities;

    ulong m_FrameCount;
    //byte m_CurCount;

    public List<PeEntity> Entities
    {
        get { return m_Entities; }
    }

    public void AddBuff(float value, float time)
    {
        StartCoroutine(Buff(value, time));
    }

    void Start()
    {
        //m_CurCount = (byte)Random.Range(15, 30);
        m_Entities = new List<PeEntity>();
    }

    void Update()
    {
        m_FrameCount++;

        Vision();
    }

    bool IsBlockEye(PeEntity entity)
    {
        if (entity == null)
            return true;

        Ray rayStart = new Ray(transform.position, entity.centerPos - transform.position);
		float dis = Vector3.Distance(transform.position, entity.centerPos);
		return Physics.Raycast(rayStart, dis, blockLayer);
    }

    bool InSight(PeEntity entity)
    {
        if (entity == null)
            return false;

        Vector3 dir1 = entity.centerPos - transform.position;
        Vector3 dir2 = transform.TransformDirection(axis);

        return Vector3.Angle(dir2, dir1) < angle;
    }

    public void Vision()
    {
        if (m_FrameCount % 20 != 0)
            return;

        m_Entities.Clear();

        Vector3 pos = transform.position;
        List<PeEntity> entitiesWithView = EntityMgr.Instance.GetEntitiesWithView();
        int n = entitiesWithView.Count;
        for (int i = 0; i < n; i++)
        {
            PeEntity entity = entitiesWithView[i];
            if (entity != null && entity.hasView)
            {
                float maxRadius = radius + entity.maxRadius;
                float sqrDis = PETools.PEUtil.SqrMagnitudeH(entity.position, pos);
                if (sqrDis <= maxRadius * maxRadius && InSight(entity) && !IsBlockEye(entity) && !entity.IsSnake)
                {
                    m_Entities.Add(entity);
                }
            }
        }
        
    }

    IEnumerator Buff(float value, float time)
    {
        radius += value;

        yield return new WaitForSeconds(time);

        radius -= value;
    }

    public void OnDrawGizmosSelected()
    {
        //int count = 50;

        //Vector3 rotAxis = transform.TransformDirection(axis) * radius;
        //Vector3 newVec = Vector3.ProjectOnPlane(Vector3.forward, rotAxis);

        //float tmpAngle = 360.0f / count;
        //for (int i = 0; i < count; i++)
        //{
        //    Vector3 tmpVector3 = Quaternion.AngleAxis(tmpAngle * i, rotAxis) * newVec;
        //    Vector3 tmpAxis = Vector3.Cross(rotAxis, tmpVector3);
        //    Vector3 tmpDir = Quaternion.AngleAxis(angle, tmpAxis) * rotAxis;

        //    Gizmos.DrawRay(transform.position, tmpDir);

        //    int j = (i+1)%count;
        //    Vector3 tmpVec1 = Quaternion.AngleAxis(tmpAngle * j, rotAxis) * newVec;
        //    Vector3 tmpAxis1 = Vector3.Cross(rotAxis, tmpVec1);
        //    Vector3 tmpDir1 = Quaternion.AngleAxis(angle, tmpAxis1) * rotAxis;

        //    Gizmos.DrawLine(transform.position + tmpDir, transform.position + tmpDir1);
        //}
    }
}
