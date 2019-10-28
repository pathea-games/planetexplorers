using UnityEngine;
using System;

namespace Pathea
{
	namespace Maths
	{
		public static class Phy
		{
			public static bool Raycast(Vector3 origin, Vector3 direction, float distance, int maskLayer)
			{
					RaycastHit[] fhitInfos = Physics.RaycastAll(origin, direction, distance, maskLayer);
					float minDist = distance;
					bool bCast = false;
					foreach (RaycastHit rch in fhitInfos)
					{
						if (rch.collider.isTrigger)
							continue;
						float dist = (rch.point - origin).magnitude;
						if (dist < minDist)
						{
							minDist = dist;
							bCast = true;
						}
					}
					return bCast;
			}

			public static bool Raycast(Ray ray, ref RaycastHit hit, float distance, int maskLayer)
			{
				RaycastHit[] fhitInfos = Physics.RaycastAll(ray, distance, maskLayer);
				float minDist = distance;
				bool bCast = false;
				foreach (RaycastHit rch in fhitInfos)
				{
					if (rch.collider.isTrigger)
						continue;
					float dist = (rch.point - ray.origin).magnitude;
					if (dist < minDist)
					{
						minDist = dist;
						hit = rch;
						bCast = true;
					}
				}
				return bCast;
			}

			public static bool Raycast(Ray ray, out RaycastHit hitInfo, float distance, int maskLayer, Transform ignoreTrans)
			{
				hitInfo = new RaycastHit();
				RaycastHit[] fhitInfos = Physics.RaycastAll(ray, distance, maskLayer);
				float minDist = distance;
				bool bCast = false;
				foreach (RaycastHit rch in fhitInfos)
				{
					if (rch.collider.isTrigger || rch.collider.transform.IsChildOf(ignoreTrans))
						continue;
					float dist = (rch.point - ray.origin).magnitude;
					if (dist < minDist)
					{
						minDist = dist;
						hitInfo = rch;
						bCast = true;
					}
				}
				return bCast;
			}
		}
	}
}
