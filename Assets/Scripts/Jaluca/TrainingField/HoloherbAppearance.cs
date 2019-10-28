using UnityEngine;
using System.Collections;

namespace TrainingScene{
	public class HoloherbAppearance : MonoBehaviour {
        public GameObject col;
		public Transform maincmr;
		public Transform orgherb;
		public float fadeTime;
		[HideInInspector] public bool produce;
		[HideInInspector] public bool destroy;
		public float part1time;
		public float minwidth;
		public float reduceSpeed;
		Material mat;
		float ctime = 0f;
		float progress;

		void Start () {
			maincmr = Camera.main.transform;
			mat = transform.GetComponent<MeshRenderer>().material;
			mat.SetTexture(0, HoloCameraControl.Instance.textureBase);
		}			
		void FixedUpdate () {
			if(produce)
			{
				ctime += Time.deltaTime;
				FadeHoloherb();
				if(ctime >= fadeTime)
				{
					ctime = fadeTime;
					produce = false;
				}
			}
			else if(destroy)
			{
				ctime -= Time.deltaTime;
				FadeHoloherb();
				if(ctime <= 0f)
				{
					ctime = 0f;	
					destroy = false;
				}
			}
		}
		void LateUpdate()
		{
			transform.forward = -maincmr.forward;
		}
		void FadeHoloherb(){
			progress = Mathf.Clamp(ctime / fadeTime, 0f, 1f);
			if(progress == 0f)
				orgherb.localScale = Vector3.zero;
			else if(progress < part1time)
				orgherb.localScale = new Vector3(minwidth, Mathf.Min(1f, progress / part1time), minwidth);			
			else
				orgherb.localScale = new Vector3(Mathf.Clamp((progress - part1time) / (1 - part1time), minwidth, 1f), 1f, Mathf.Clamp((progress - part1time) / (1 - part1time), minwidth, 1f));
			mat.SetFloat("_Scale", 1f / progress);
		}
	}
}