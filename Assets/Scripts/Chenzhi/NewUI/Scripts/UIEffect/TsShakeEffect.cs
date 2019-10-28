using UnityEngine;
using System.Collections;

namespace PeUIEffect
{
	[System.Serializable]
	public class AcEffect
	{
		public bool bActive;
		public AnimationCurve mAcShakeEffect;

		public float  mScale = 1;
		public float  mPitch= 1;
		
		public float GetAcValue(float key)
		{
			return mAcShakeEffect.Evaluate(key / mScale) * mPitch;
		}

		public float EndTime{get{return mAcShakeEffect.keys.Length>0 ? mAcShakeEffect.keys[mAcShakeEffect.length -1].time * mScale : 0;}}
	}
	
	[System.Serializable]
	public class TsShakeEffect : UIEffect
	{
		public AcEffect effectPos_x;
		public AcEffect effectPos_y;

		public float Pitch = 1;
		
		public Transform TargetTs
		{
			get{return mTargetTs;}
			set{mTargetTs = value;}
		}
		
		Transform mTargetTs;
		float time = 0;
		Vector3 pos = Vector3.zero;

		public void Play(float _pitch)
		{
			Pitch *= _pitch;
			Play ();
		}

		public override void Play ()
		{
			if (m_Runing)
				return ;

			if (mTargetTs == null)
				mTargetTs = gameObject.transform;

			time = 0;
			pos = mTargetTs.localPosition;
			base.Play ();
			
		}
		
		public override void End ()
		{
			mTargetTs.localPosition = pos;
			base.End ();
		}
		
		
		void Update ()
		{
			if (m_Runing)
			{
				if(mTargetTs == null)
					return;

				time += Time.deltaTime;
				Vector3 pos_ef = Vector3.zero;

				if (effectPos_x.bActive)
					pos_ef.x += (int) (effectPos_x.GetAcValue(time) * Pitch);
				
				if (effectPos_y.bActive)
					pos_ef.y += (int) (effectPos_y.GetAcValue(time) * Pitch);
				
				mTargetTs.localPosition = pos + pos_ef; 
				
				if (time > EndTime)
					End();
			}
		}
		
		float EndTime{ get{return (effectPos_x.EndTime > effectPos_y.EndTime) ? effectPos_x.EndTime : effectPos_y.EndTime;} }
		
	}
}
