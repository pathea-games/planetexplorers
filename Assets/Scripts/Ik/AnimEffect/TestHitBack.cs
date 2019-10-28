using UnityEngine;
using System.Collections;
//using RootMotion.FinalIK;
using RootMotion.FinalIK.Demos;
using PEIK;
using Pathea;

public class TestHitBack : HitReactionCharacter 
{
	public LayerMask		m_CheckLayer;
	private Motion_Beat		m_Beat;
	public	float			m_HitPower = 100f;

	void Start()
	{
		if(null != m_Beat)
			m_Beat = GetComponentInParent<Motion_Beat>();
	}

	public void SetHandler(Motion_Move move, ViewCmpt view, Motion_Beat beat)
	{
		m_Beat = beat;
	}

	protected override void Update ()
	{
		if(null != cam)
		{
			CheckHit();
		}
	}

	void CheckHit()
	{
		if (null != m_Beat && Input.GetMouseButtonDown(0)) 
		{
			Ray ray = cam.ScreenPointToRay(Input.mousePosition);
			// Raycast to find a ragdoll collider
			RaycastHit hit = new RaycastHit();
			if (Physics.Raycast(ray, out hit, 100f, m_CheckLayer.value)) 
			{
				PEDefenceTrigger defenceTrigger = hit.collider.GetComponent<PEDefenceTrigger>();

				PECapsuleHitResult result;

				if(defenceTrigger.RayCast(ray, 1000f, out result))
				{
					m_Beat.Beat(GetComponentInParent<SkAliveEntity>(), result.hitTrans, ray.direction, m_HitPower);
				}
			}
		}
	}
}
