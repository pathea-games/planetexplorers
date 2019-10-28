using UnityEngine;
using System.Collections;

namespace PeUIEffect
{
	public class MenuParticleEffect : UIEffect 
	{
		[SerializeField] UISprite mSpr1;
		[SerializeField] UISprite mSpr2;
		[SerializeField] float mSpeed = 1;
		[SerializeField] float mEndPos_y = 0;
		[SerializeField] Vector3 SprStartPos = new Vector3(78,55,0);

		UISprite mMoveSpr = null; 

		public override void Play ()
		{
			base.Play ();
			mSpr1.transform.localPosition = SprStartPos;
			mSpr2.transform.localPosition = SprStartPos;
			mMoveSpr = mSpr1;
		}

		public override void End ()
		{
			base.End ();
		}

		// Use this for initialization
		void Start () 
		{
			Play();
		}
		
		// Update is called once per frame
		void Update () 
		{
			if (m_Runing )
			{
				if (mMoveSpr != null)
				{
					Vector3 pos = mMoveSpr.transform.localPosition;
					pos.y  =  (pos.y - (mSpeed * Time.deltaTime) );
					mMoveSpr.transform.localPosition = pos;

					if (pos.y < mEndPos_y)
					{
						mMoveSpr.transform.localPosition = SprStartPos;
						if (mMoveSpr == mSpr1)
							mMoveSpr = mSpr2;
						else 
							mMoveSpr = mSpr1;
					}
				}
			}
		}
	}
}

