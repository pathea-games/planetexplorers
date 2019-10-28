using UnityEngine;
using System.Collections;

namespace EpsilonIndi
{
	public class SelfRotation : MonoBehaviour {
		public float selfSpeed;
		public bool clockwise;
		public float rotationAngle;
		public float startAngle;
		float sumAngle;
		[HideInInspector] public float s_angle;
		[HideInInspector] public Quaternion m_rotationX;
		[HideInInspector] public Quaternion m_rotationY;

		public Quaternion UpdateRotateY(float dt)
		{
			sumAngle += selfSpeed * dt;
			sumAngle = sumAngle > 360f ? sumAngle - 360f : sumAngle;
			s_angle = (sumAngle + startAngle) * (clockwise ? 1f : -1f);
			m_rotationY = Quaternion.AngleAxis(s_angle, Vector3.up);
			return m_rotationY;
		}
		public void TestUpdate()
		{
			transform.rotation = m_rotationX * m_rotationY;
		}
		void FixedUpdate() 
		{
			m_rotationX = Quaternion.AngleAxis(rotationAngle, Vector3.right);
		}
	}

}
