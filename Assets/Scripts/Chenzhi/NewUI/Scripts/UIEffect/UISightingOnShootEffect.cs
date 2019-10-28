using UnityEngine;
using System.Collections;
namespace PeUIEffect
{
	public class UISightingOnShootEffect : UIEffect 
	{
		[SerializeField] AcEffect effect;
		[SerializeField] float mValue;

		public float Value {get {return mValue;}}


		public override void Play ()
		{
			base.Play ();
			time = 0;
		}

		public override void End ()
		{
			base.End ();
			mValue = 0;
		}

		float time = 0; 
		void Update()
		{
			if (m_Runing)
			{
				time += Time.deltaTime;
				mValue = effect.GetAcValue(time);
				if ( time > effect.EndTime)
					End();
			}
			else 
				time = 0;
		}
	}
}
