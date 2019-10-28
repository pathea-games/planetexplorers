using UnityEngine;
using System.Collections;

namespace EpsilonIndi
{
	public class ShipSightRotate : MonoBehaviour {
		public float selfSpeed;
		public bool clockwise;
		public float rotationAngle;
		public float startAngle;
		float sumAngle;
		[HideInInspector] public float rotateY;
		[HideInInspector] public Quaternion m_rotation;
		
		void FixedUpdate()
		{
			sumAngle += selfSpeed * Time.deltaTime;
			sumAngle = sumAngle > 360f ? sumAngle - 360f : sumAngle;
			rotateY = (sumAngle + startAngle) * (clockwise ? 1f : -1f);
			m_rotation = (Quaternion.AngleAxis(rotationAngle, transform.right) * Quaternion.AngleAxis(rotateY, Vector3.up));
			transform.rotation = m_rotation;
		}
	}
	
}
