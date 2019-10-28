using UnityEngine;
using System.Collections;

namespace TrainingScene
{
	public class EmitreceiverAppear : MonoBehaviour
	{
		public Transform orgreceiver;
		public Transform colreceiver;
		public float fadeTime;
		[HideInInspector] public bool produce;
		[HideInInspector] public bool destroy;
		public float part1time;
		public float minwidth;
		public float reduceSpeed;
		Material mat;
		//GameObject orgreceiverg;
		Vector3 maxScale;
		float ctime = 0f;
		float progress;
		//bool decreaseLight;
//		GlowEffect glowEffect;
		
		void Start()
		{
//			glowEffect = HoloCameraControl.Instance.glowEffect;
			//orgreceiverg = orgreceiver.gameObject;
			mat = transform.GetComponent<MeshRenderer>().material;
			mat.SetTexture(0, HoloCameraControl.Instance.textureBase);
			maxScale = orgreceiver.localScale;
			orgreceiver.localScale = Vector3.zero;
		}	
		void FixedUpdate()
		{
			if(produce)
			{
				ctime += Time.deltaTime;
				FadeHoloTree();
//				glowEffect.glowIntensity = 2f;
				if(ctime >= fadeTime)
				{
					ctime = fadeTime;
					produce = false;
					//decreaseLight = true;
				}
			}
			else if(destroy)
			{
				ctime -= Time.deltaTime;
				FadeHoloTree();
				if(ctime <= 0f)
				{
					ctime = 0f;
					destroy = false;
				}
			}
//			if(decreaseLight)
//			{
//				if(glowEffect.glowIntensity > 1.5f)
//					glowEffect.glowIntensity -= Time.deltaTime * reduceSpeed;
//				else
//					decreaseLight = false;
//			}
		}
		void FadeHoloTree()
		{
			progress = Mathf.Clamp(ctime / fadeTime, 0f, 1f);
			if(progress == 0f)
				orgreceiver.localScale = Vector3.zero;
			else if(progress < part1time)
				orgreceiver.localScale = new Vector3(minwidth * maxScale.x, Mathf.Min(1f, progress / part1time) * maxScale.y, minwidth * maxScale.z);
			else
				orgreceiver.localScale = new Vector3(Mathf.Clamp((progress - part1time) / (1 - part1time), minwidth, 1f) * maxScale.x, maxScale.y, Mathf.Clamp((progress - part1time) / (1 - part1time), minwidth, 1f) * maxScale.z);
			mat.SetFloat("_Scale", 1f / progress);
		}
	}
}
