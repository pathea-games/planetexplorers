using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace PeUIEffect
{
	public class DivingLabelEffect : UIEffect 
	{
		[SerializeField] float mSpeed = 2;
		[SerializeField] int mMaxLineWidth = 250;
		[SerializeField] List<UILabel> mLbList = new List<UILabel>();

		UIDirvingShowHideEffct dirvingEffct;
		// Use this for initialization
		void Start () 
		{
			dirvingEffct = GetComponent<UIDirvingShowHideEffct>();
			dirvingEffct.e_OnEnd += OnUIDringShow;
		}

		void OnUIDringShow(UIEffect effct)
		{
			if (effct.Forward)
				Play();
		}

		public override void Play ()
		{
			foreach (UILabel lb in mLbList)
				lb.lineWidth = 0;
			base.Play ();
		}

		// Update is called once per frame
		void Update () 
		{
			if (m_Runing)
			{
				foreach (UILabel lb in mLbList)
				{
					lb.lineWidth += Convert.ToInt32( mSpeed * lb.text.Length );
					if (lb.lineWidth > mMaxLineWidth)
						End();
				}
			}
		}
	}
}
