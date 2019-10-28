using UnityEngine;
using System.Collections;

namespace PeAudio
{
	public class BGADesc
	{
		public string path;
	}
	
	public class BGAInst : MonoBehaviour
	{
		public FMODAudioSource audioSrc;
		public float targetVolume = 0;
		
		float silenceTime = 0;
		
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
			if (targetVolume < 0.001f && audioSrc.volume < 0.005f)
			{
				silenceTime += Time.deltaTime;
				if (silenceTime > 5f)
				{
					if (gameObject.GetComponents<BGAInst>().Length == 1)
						GameObject.Destroy(this.gameObject);
					else
						Destroy(this);
					return;
				}
			}
			else
			{
				silenceTime = 0f;
			}
			audioSrc.volume = Mathf.Lerp(audioSrc.volume, targetVolume, 0.05f);
		}
	}
}