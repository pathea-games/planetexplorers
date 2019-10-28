using UnityEngine;
using System.Collections;
using Pathea.Maths;
using FMOD;
using FMOD.Studio;

namespace PeAudio
{
	public class BGMDesc
	{
		public string path;
		public int musicCount;
		public Range1D changeTime;
	}

	public class BGMInst : MonoBehaviour
	{
		public FMODAudioSource audioSrc;

		private float targetVolume = 0;
		private float preDampRate = 1;
		private float postDampRate = 1;
		private float pretime = 0;
		private float posttime = 0;
		//private bool fading = false;
		private string nextAudioPath = "";

		void Awake ()
		{
			audioSrc = gameObject.AddComponent<FMODAudioSource> ();

		}

		void OnDestroy ()
		{
			FMODAudioSource.Destroy(audioSrc);
		}

		void Update ()
		{
			if (nextAudioPath != audioSrc.path)
			{
				if (pretime > 0)
				{
					pretime -= Time.deltaTime;
				}
				else
				{

					targetVolume = 0;
					if (audioSrc.volume < 0.005f)
					{
						posttime -= Time.deltaTime;
						if (posttime <= 0)
							audioSrc.path = nextAudioPath;
					}
				}
				if (Mathf.Abs(audioSrc.volume - targetVolume) > 0.002f)
					audioSrc.volume = Mathf.Lerp(audioSrc.volume, targetVolume, preDampRate);
				else
					audioSrc.volume = targetVolume;
			}
			else
			{
				pretime = 99999;
				targetVolume = 1;
				posttime = 99999;
				if (Mathf.Abs(audioSrc.volume - targetVolume) > 0.002f)
					audioSrc.volume = Mathf.Lerp(audioSrc.volume, targetVolume, postDampRate);
				else
					audioSrc.volume = targetVolume;
			}
		}

		public void ChangeBGM (string path, float prewarm, float postwarm, float predamp, float postdamp)
		{
			nextAudioPath = path;
			pretime = prewarm;
			posttime = postwarm;
			preDampRate = predamp;
			postDampRate = postdamp;
		}
	}
}