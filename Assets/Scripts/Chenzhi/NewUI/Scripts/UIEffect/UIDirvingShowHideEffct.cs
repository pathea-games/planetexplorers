using UnityEngine;
using System.Collections;
using PeUIEffect;

namespace PeUIEffect
{
	public class UIDirvingShowHideEffct : UIEffect 
	{
		[SerializeField] AcEffect mAcEffctPosx;

		[System.Serializable] 
		public class TargetSpr
		{
			public Transform mTargetTs;
			Vector3 mPosFrom;
			Vector3 mPosTo;
			UISprite mSpr;
		
			public void Init()
			{
				if (mTargetTs != null)
				{
					mPosTo = mTargetTs.localPosition;
					mPosFrom  = new Vector3(0,mPosTo.y,mPosTo.z);
					mSpr = mTargetTs.GetComponent<UISprite>();
				}
			}

			public void Update(float acValue, Color color)
			{
				if (mTargetTs == null)
					return;
				int pos_x = System.Convert.ToInt32(acValue * mPosTo.x);
				mTargetTs.localPosition = new Vector3(pos_x,mPosTo.y,mPosTo.z);

				if (mSpr != null)
					mSpr.color = color;
			}

			public void OnEffectEnd(bool forward)
			{
				if (mTargetTs != null)
					mTargetTs.localPosition = forward ? mPosTo : mPosFrom;
			}

		}

		[SerializeField] TargetSpr mTopLeft; 
		[SerializeField] TargetSpr mTopRight;
		[SerializeField] TargetSpr mButtomLeft;
		[SerializeField] TargetSpr mButtomRight;

		public override void Play (bool forward)
		{
			time = 0;
			base.Play (forward);
		}

		public override void End ()
		{
			mTopLeft.OnEffectEnd(mForward);
			mTopRight.OnEffectEnd(mForward);
			mButtomLeft.OnEffectEnd(mForward);
			mButtomRight.OnEffectEnd(mForward);
			base.End ();
		}
		// Use this for initialization
		void Start () 
		{
			mTopLeft.Init();
			mTopRight.Init();
			mButtomLeft.Init();
			mButtomRight.Init();
		}

		float time = 0;
		// Update is called once per frame
		void Update () 
		{
			if (m_Runing )
			{
				if (mAcEffctPosx.bActive)
				{
					time += Time.deltaTime;
					float acValue = mForward ? mAcEffctPosx.GetAcValue(time) : (1 - mAcEffctPosx.GetAcValue(time)) ;
					Color  sprColor = mForward ? new Color(1,1,1,0.5f + acValue ) :  new Color(1,1,1, acValue * acValue);
					mTopLeft.Update(acValue,sprColor);
					mTopRight.Update(acValue,sprColor);
					mButtomLeft.Update(acValue,sprColor);
					mButtomRight.Update(acValue,sprColor);
				}
				if (time > mAcEffctPosx.EndTime)
					End();
			}
		}
	}
}