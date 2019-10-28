using UnityEngine;
using System.Collections;

namespace PeUIEffect
{
	public class UISpriteScaleEffect : UIEffect 
	{
		[SerializeField] Vector3 mMaxScale;
		[SerializeField] float mSpeed = 80;

		UISprite mSpr = null;
		//bool forward = true;

		public override void Play ()
		{
			base.Play ();
			if (mSpr != null)
			{
				Vector3 ls = gameObject.transform.localScale;
				ls.x = 0;
				mSpr.transform.localScale = ls;
			}
		}

		public override void End ()
		{
			base.End ();
		}


		// Use this for initialization
		void Start () 
		{
			mSpr = GetComponent<UISprite>();
			Play ();
		}
		
		// Update is called once per frame
		void Update () 
		{
			if (m_Runing && mSpr!= null)
			{
				if (gameObject.transform.localScale.x < mMaxScale.x)
				{
					Vector3 ls = gameObject.transform.localScale;
					ls.x += (int) (mSpeed * Time.deltaTime); 
					gameObject.transform.localScale = ls;

				}
				else 
				{
					End();
				}
			}
		}
	}
}
