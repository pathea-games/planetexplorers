using UnityEngine;
using System.Collections;

namespace TrainingScene
{
	public class EmitlineAppearance : MonoBehaviour
	{
		private static EmitlineAppearance s_instance;
		public static EmitlineAppearance Instance{get{return s_instance;}}
		float lineMaxAlpha = 0.4f;
		//float signMaxColor = 0.5f;
		float fadeTime;
		float ctime;
		float progress;
		float colorNum;
		bool produce;
		bool destroy;
		public Material matLine;
		public Material eff_true;
		public Material eff_false;

		void Awake()
		{
			s_instance = this;
		}
		void Update()
		{
			if(produce)
			{
				ctime += Time.deltaTime;
				SetNewColor();
				if(fadeTime < ctime)
					produce = false;
			}
			else if(destroy)
			{
				ctime -= Time.deltaTime;
				SetNewColor();
				if(ctime < 0)
					destroy = false;
			}
		}
		void SetNewColor()
		{
			progress = ctime / fadeTime;
			colorNum = progress * 0.5f;
			matLine.SetColor("_TintColor", new Color(0.33f, 0.33f, 0.33f, progress * lineMaxAlpha));
			eff_true.SetColor("_TintColor", new Color(colorNum, colorNum, colorNum, 0.5f));
			eff_false.SetColor("_TintColor", new Color(colorNum, colorNum, colorNum, 0.5f));
		}
		public void StartFadeLine(float fade, bool prod)
		{
			fadeTime = fade;
			if(prod)
			{
				produce = true;
				ctime = 0f;
			}
			else
			{
				destroy = true;
				ctime = fadeTime;
			}				
		}
	}
}
