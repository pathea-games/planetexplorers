using UnityEngine;
using RootMotion.FinalIK;

namespace PEIK
{
	[RequireComponent(typeof(FullBodyBipedIK))]
	public abstract class IKOffsetModifier : MonoBehaviour 
	{
		public float m_Weight = 1f; // The master weight
		
		protected abstract void OnModifyOffset();

		protected abstract void OnInit();
		
		[SerializeField]
		[HideInInspector]
		protected FullBodyBipedIK m_FBBIK; // Reference to the FBBIK component
		// not using Time.deltaTime or Time.fixedDeltaTime here, because we don't know if animatePhysics is true or not on the character, so we have to keep track of time ourselves.
		protected float deltaTime { get { return Time.deltaTime; }}
		
		//private float m_LastTime;
		
		protected virtual void Awake() 
		{
			m_FBBIK = GetComponent<FullBodyBipedIK>();
			m_FBBIK.solver.OnPreUpdate += ModifyOffset;
			//m_LastTime = Time.time;
			OnInit();
		}
		
		// The main function that checks for all conditions and calls OnModifyOffset if they are met
		private void ModifyOffset() {
			if (!enabled) return;
			if (m_Weight <= 0f) return;
			if (deltaTime <= 0f) return;
			if (m_FBBIK == null) return;
			m_Weight = Mathf.Clamp(m_Weight, 0f, 1f);
			
			OnModifyOffset();
			
			//m_LastTime = Time.time;
		}
		
		// Remove the delegate when destroyed
		void OnDestroy() {
			if (m_FBBIK != null) m_FBBIK.solver.OnPreUpdate -= ModifyOffset;
		}
	}
}
