using UnityEngine;

namespace Pathea.Projectile
{
	public class SkProjectileDamageScale : MonoBehaviour
	{
		public float disDamageF = 0.001f;
		public float damageScale{ get { return Mathf.Clamp01(1f - m_MoveDis * disDamageF); } }
		float m_MoveDis;
		Vector3 m_LastPos;
		// Use this for initialization
		void Start ()
		{
			m_LastPos = transform.position;
		}
		
		// Update is called once per frame
		void Update () 
		{
			m_MoveDis += Vector3.Distance(transform.position, m_LastPos);
			m_LastPos = transform.position;
		}
	}
}
