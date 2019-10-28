using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SkillSystem;
using Pathea;

public class PEHearing : PEPerception
{
    public float radius;
    public bool isIgnoreBlock;

    List<PeEntity> m_Entities;

    int layer;

    ulong m_FrameCount;

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
        m_Entities = new List<PeEntity>();
    }

    void Update()
    {
        m_FrameCount++;

        Hearing();
    }

    bool IsBlockEye(PeEntity entity)
    {
        if (entity == null)
            return true;

        if (isIgnoreBlock)
            return false;

        Ray rayStart = new Ray(transform.position, entity.centerPos - transform.position);
        float dis = Vector3.Distance(transform.position, entity.centerPos);

        return Physics.Raycast(rayStart, dis, blockLayer);
    }

    bool IgnoreCollider(Collider col)
    {
        if (col.isTrigger || col.transform.IsChildOf(transform))
            return true;

        Vector3 pos = transform.position;
        Vector3 dir = col.transform.position - pos;
        //float dis = Vector3.Distance(pos, col.transform.position);
        if (Physics.Raycast(transform.position, dir, PEConfig.BlockLayer))
            return false;

        return m_Entities.Find(ret => ret != null && col.transform.IsChildOf(ret.transform)) != null;
    }

    public void Hearing()
    {
        if ((m_FrameCount & 0x0f) != 0)
            return;

        Profiler.BeginSample("Hearing");
        m_Entities.Clear();

        Vector3 pos = transform.position;
		List<PeEntity> entitiesWithView = EntityMgr.Instance.GetEntitiesWithView ();
		int n = entitiesWithView.Count;
		for (int i = 0; i < n; i++)
		{
			PeEntity entity = entitiesWithView[i];
			if(entity != null && entity.hasView){
				float maxRadius = radius + entity.maxRadius;
				if(PETools.PEUtil.SqrMagnitudeH(entity.position, pos) <= maxRadius*maxRadius &&
				   !IsBlockEye(entity)){
					m_Entities.Add(entity);
				}
			}
		}
        Profiler.EndSample();
    }

    IEnumerator Buff(float value, float time)
    {
        radius += value;

        yield return new WaitForSeconds(time);

        radius -= value;
    }

    public void OnDrawGizmosSelected()
    {
        //Gizmos.DrawWireSphere(transform.position, radius);
    }
}
