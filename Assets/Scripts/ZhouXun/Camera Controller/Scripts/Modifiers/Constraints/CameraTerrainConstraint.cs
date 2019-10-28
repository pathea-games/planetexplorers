using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CameraTerrainConstraint : CamConstraint
{
	public LayerMask m_TerrainLayers;
	public float m_CastDistance = 80F;
	public override void Do ()
	{
		Transform character = m_Controller.character;
		if ( character == null ) return;

		m_CastDistance = Vector3.Distance(m_TargetCam.transform.position, character.position);

		Vector3 eye = m_TargetCam.transform.position;
		Vector3 forward = m_TargetCam.transform.forward;
		Vector3 target = eye + forward * m_CastDistance;

		Ray outray = new Ray (eye, forward);
		Ray inray = new Ray (target, -forward);

		Debug.DrawLine(outray.origin, outray.GetPoint(m_CastDistance), Color.yellow);

		float npheight = Mathf.Tan(m_TargetCam.fieldOfView*Mathf.Deg2Rad*0.5f) * m_TargetCam.nearClipPlane;
		float npwidth = npheight * m_TargetCam.aspect;
		float minr = Mathf.Sqrt(npheight*npheight + npwidth*npwidth + m_TargetCam.nearClipPlane*m_TargetCam.nearClipPlane);

		List<RaycastHit> outcasts = RaycastAllFix(outray, m_CastDistance, m_TerrainLayers);
		List<RaycastHit> incasts = RaycastAllFix(inray, m_CastDistance, m_TerrainLayers);

		List<RaycastHit> rchs = new List<RaycastHit> ();
		foreach (RaycastHit outc in outcasts)
		{
			rchs.Add(outc);
			Debug.DrawLine(outc.point + Vector3.left*0.1f, outc.point + Vector3.right*0.1f, Color.green);
			Debug.DrawLine(outc.point + Vector3.up*0.1f, outc.point + Vector3.down*0.1f, Color.green);
			Debug.DrawLine(outc.point + Vector3.forward*0.1f, outc.point + Vector3.back*0.1f, Color.green);
		}
		for (int i = 0; i < incasts.Count; ++i)
		{
			RaycastHit iter = incasts[i];
			iter.distance = m_CastDistance - incasts[i].distance;
			incasts[i] = iter;
			rchs.Add(incasts[i]);
			Debug.DrawLine(incasts[i].point + Vector3.left*0.1f, incasts[i].point + Vector3.right*0.1f, Color.red);
			Debug.DrawLine(incasts[i].point + Vector3.up*0.1f, incasts[i].point + Vector3.down*0.1f, Color.red);
			Debug.DrawLine(incasts[i].point + Vector3.forward*0.1f, incasts[i].point + Vector3.back*0.1f, Color.red);
		}
		RaycastHit zero = new RaycastHit ();
		zero.distance = 0;
		zero.point = eye;
		zero.normal = forward;
		rchs.Add(zero);
		rchs.Sort(RaycastHitCompare);
		bool merged = false;
		do
		{
			merged = false;
			for ( int i = 0; i < rchs.Count - 1; )
			{
				bool into0 = (Vector3.Dot(rchs[i].normal, forward) < 0);
				bool into1 = (Vector3.Dot(rchs[i+1].normal, forward) < 0);
				float close = Mathf.Abs(rchs[i].distance - rchs[i+1].distance);
				// Merge check
				if ( close < minr && into0 != into1 || into0 && !into1 )
				{
					merged = true;
					rchs.RemoveAt(i);
					rchs.RemoveAt(i);
				}
				else
				{
					++i;
				}
			}
		}
		while ( merged );
		for ( int i = rchs.Count - 1; i >= 0; --i )
		{
			if ( Vector3.Dot(rchs[i].normal, forward) > 0 )
			{
				if ( rchs[i].distance > 0.001f )
					m_TargetCam.transform.position = outray.GetPoint(rchs[i].distance + 0.5f);
				break;
			}
		}
	}

	private static int RaycastHitCompare (RaycastHit lhs, RaycastHit rhs)
	{
		return Mathf.RoundToInt((lhs.distance - rhs.distance)*10000);
	}

	private List<RaycastHit> RaycastAllFix (Ray ray, float dist, LayerMask lm)
	{
		bool cast = true;
		float currdist = 0;
		List<RaycastHit> retval = new List<RaycastHit> ();
		int n = 0;
		while ( currdist < dist && cast )
		{
			// Avoid endless loop
			if ( n++ > 256 ) break;

			Ray _ray = new Ray (ray.GetPoint(currdist), ray.direction);
			RaycastHit rch;
			if ( dist-currdist-0.01f > 0 )
			{
				cast = Physics.Raycast(_ray, out rch, dist-currdist-0.01f, lm);
				if (cast)
				{
					rch.distance = currdist + rch.distance;
					retval.Add(rch);
					currdist = rch.distance + 0.01f;
				}
			}
			else break;
		}
		return retval;
	}
}
