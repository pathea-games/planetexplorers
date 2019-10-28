using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pathea;

public class TRRaycast : Trajectory
{
    public float range;

    Transform emit;

    LineRenderer[] m_LineRenderers;

	
	List<Vector3> m_HitPositions = new List<Vector3>();

    void Start()
	{
		Emit(m_Emitter);

        m_LineRenderers = GetComponentsInChildren<LineRenderer>();
    }

    public void Emit(Transform emit)
    {
        this.emit = emit;
    }

    public override Vector3 Track(float deltaTime)
    {
        if (emit == null)
            return Vector3.zero;

        RaycastHit[] hitsInfos = Physics.RaycastAll(emit.position, emit.forward, range, GameConfig.ProjectileDamageLayer);
        foreach (RaycastHit hitInfo in hitsInfos)
        {
            Collider collider = hitInfo.collider;
			if (collider == null || collider.tag == "WorldCollider" || collider.transform.IsChildOf(transform) || m_Emitter != null && collider.transform.IsChildOf(m_Emitter))
				continue;
			PEDefenceTrigger defencetrigger = collider.GetComponent<PEDefenceTrigger>();
			PECapsuleHitResult result;
			if(null == defencetrigger && collider.isTrigger)
				continue;
			if(null == defencetrigger || !defencetrigger.RayCast(new Ray(emit.position, emit.forward), range, out result) || result.distance < hitInfo.distance)
				m_HitPositions.Add(hitInfo.point);
			else
				m_HitPositions.Add(result.hitPos);
        }

		Vector3 finalPos = emit.position + emit.forward * range;
        float closetDistance = 2 * range * 2 * range;

		for(int i = 0; i < m_HitPositions.Count; ++i)
        {
			float sqrDistance = (m_HitPositions[i] - emit.position).sqrMagnitude;
            if (sqrDistance < closetDistance)
            {
                closetDistance = sqrDistance;
				finalPos = m_HitPositions[i];
            }
        }

		m_HitPositions.Clear();

		return finalPos - transform.position;
    }

	public override Quaternion Rotate (float deltaTime)
	{
		if(m_Emitter != null)
			return Quaternion.FromToRotation(transform.position, m_Emitter.position);
		return transform.rotation;
	}

    public void Update()
    {
        for (int i = 0; i < m_LineRenderers.Length; i++)
        {
            if(emit != null && m_LineRenderers[i] != null)
            {
				if(isActive)
				{
	                m_LineRenderers[i].SetPosition(0, emit.position);
	                m_LineRenderers[i].SetPosition(1, transform.position);
				}
				else
					m_LineRenderers[i].enabled = false;
            }
        }
    }
}
