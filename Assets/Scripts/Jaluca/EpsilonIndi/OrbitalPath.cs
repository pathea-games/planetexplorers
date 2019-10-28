using UnityEngine;
using System.Collections;

namespace EpsilonIndi
{
	public class OrbitalPath : MonoBehaviour {
		public float orbitalRadius_km;
		public float periodSpeed;
		public Transform primaryStar;
		public bool clockwise;
		public float startAngle;
		public float equatorAngle;
		public float verticalAngle;
		float w_angle;
		Vector3 vector = Vector3.zero;
		[HideInInspector]public Vector3 m_position;
		public bool test;
		public Vector3 UpdatePosition(float dt)
		{
			if(primaryStar == null)			
				return Vector3.zero;
			w_angle += periodSpeed * dt;
			w_angle = w_angle > 360f ? w_angle - 360f : w_angle;
			vector = Quaternion.AngleAxis((w_angle + startAngle) * (clockwise ? 1f : -1f), Vector3.up) * (Vector3.forward * orbitalRadius_km);
			vector = (Quaternion.AngleAxis(equatorAngle, Vector3.right) * Quaternion.AngleAxis(verticalAngle, Vector3.up)) * vector;
			if(primaryStar.GetComponent<OrbitalPath>() != null)
				m_position = primaryStar.GetComponent<OrbitalPath>().m_position + vector;
			else
				m_position = vector;		
			return m_position;
		}
		public void TestUpdate()
		{
			transform.position = m_position;
		}
	}
}
