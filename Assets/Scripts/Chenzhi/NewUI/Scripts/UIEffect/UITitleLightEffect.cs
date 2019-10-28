using UnityEngine;
using System.Collections;

namespace PeUIEffect
{
	public class UITitleLightEffect : UIEffect 
	{
		//UITexture tex;
		void Awake()
		{
			//tex = GetComponent<UITexture>();
		}

		public override void Play ()
		{
			base.Play ();
		}

		public override void End ()
		{
			base.End ();
		} 

		public void Update()
		{
			if (m_Runing)
			{
				//tex.uvRect = new Rect(
			}
		}
	}
}