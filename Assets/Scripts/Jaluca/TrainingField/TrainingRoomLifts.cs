using UnityEngine;
using System.Collections;

namespace TrainingScene
{
	public class TrainingRoomLifts : MonoBehaviour 
	{
		[SerializeField] Rigidbody liftA;
		[SerializeField] Rigidbody liftB;
		[SerializeField] float coolingTime = 5f;
		[SerializeField] float minHeight = -7.75f;
		[SerializeField] float maxHeight = -1.37f;
		[SerializeField] float moveSpeed = 1f;
		float m_ChangeTime;
		bool m_AMoveUp;

		void Start()
		{
			m_ChangeTime = Time.time + coolingTime;
			Vector3 pos = liftA.transform.localPosition;
			pos.y = minHeight;
			liftA.transform.localPosition = pos;

			pos = liftB.transform.localPosition;
			pos.y = maxHeight;
			liftB.transform.localPosition = pos;
		}
		
		void FixedUpdate()
		{
			if(Time.time >= m_ChangeTime)
			{
				m_ChangeTime = Time.time + coolingTime;
				m_AMoveUp = !m_AMoveUp;
			}
			else
			{
				Vector3 pos = liftA.transform.localPosition;
				pos.y = Mathf.Clamp(pos.y + (m_AMoveUp?1f:-1f) * Time.fixedDeltaTime * moveSpeed, minHeight, maxHeight);
				liftA.MovePosition(liftA.transform.parent.TransformPoint(pos));

				pos = liftB.transform.localPosition;
				pos.y = Mathf.Clamp(pos.y + (!m_AMoveUp?1f:-1f) * Time.fixedDeltaTime * moveSpeed, minHeight, maxHeight);
				liftB.MovePosition(liftB.transform.parent.TransformPoint(pos));
			}
		}
	}	
}
