using UnityEngine;
using System.Collections;

namespace PeUIEffect
{
	[System.Serializable]
	public abstract class UIEffect : MonoBehaviour
	{
		public delegate void EffectEvent(UIEffect effect);
		public event EffectEvent e_OnPlay = null;
		public event EffectEvent e_OnEnd = null;

		[SerializeField] protected bool m_Runing = false;
		[SerializeField] protected bool mForward = true;

		public bool Forward {get {return mForward; }}

		public virtual void Play(bool forward)
		{
			mForward = forward;
			Play();
		}

		public virtual void Play()
		{
			m_Runing = true; 
			if (e_OnPlay != null) 
				e_OnPlay(this);
		}
		public virtual void End()
		{
			m_Runing = false;
			if (e_OnEnd != null)
				e_OnEnd(this);
		}
	}
}
