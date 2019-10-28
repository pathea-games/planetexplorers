using UnityEngine;
using System.Collections;

namespace PeUIEffect
{
	public class UIGridEffect : UIEffect 
	{
		public float Speed = 1;
		float FadeTime = 0;

		void Start()
		{
			Play();
		}

		public override void Play ()
		{
			FadeTime = 0;
			base.Play ();
		}
		public override void End ()
		{
			FadeTime =0;
			base.End ();
			GameObject.Destroy(this.gameObject.transform.parent.gameObject);
		}
		// Update is called once per frame
	 	void Update ()
		{
			if (m_Runing)
			{
				FadeTime += Time.deltaTime * Speed;
				GetComponent<Renderer>().material.SetFloat("_FadeTime", FadeTime);

				if (FadeTime > 1)
					End ();
			}
		}
		
	}
}
