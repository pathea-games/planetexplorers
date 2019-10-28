using UnityEngine;
using System.Collections;

namespace EpsilonIndi
{
	public class CloudMotion : MonoBehaviour 
	{
		public float relativeSpeed;
		public float offsetAngle;
		public float selfRotateSpeed;
		float deltaW;
		float selfAngle;
		float mainPlanetRotY;
		bool dest;
		Quaternion m_rotate;

		public void InitProp(float speed, float angle, float angley, float selfrots)
		{
			relativeSpeed = speed;
			offsetAngle = angle;
			deltaW = angley;
			selfRotateSpeed = selfrots;
			selfAngle = Random.value * 360f;
			transform.localRotation = Quaternion.Euler(offsetAngle, angley, 0f);
		}
		public Quaternion UpdateRotate(float dt, float rty)
		{
			mainPlanetRotY = rty;
			deltaW += relativeSpeed * dt;
			if(selfRotateSpeed != 0f)
				selfAngle += selfRotateSpeed * dt;
			m_rotate = Quaternion.Euler(offsetAngle, deltaW, selfAngle);
			return m_rotate;
		}
		public bool OutOfSight(float ssr)
		{
			if(Mathf.Abs((deltaW + mainPlanetRotY - ssr) % 360) < 5f)
			{
				if(dest)
					return true;
			}
			else
				dest = true;
			return false;
		}
		public void TestUpdate()
		{
			transform.localRotation = m_rotate;
		}
	}
}