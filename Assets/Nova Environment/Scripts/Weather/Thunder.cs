using UnityEngine;
using System;

namespace NovaEnv
{
	[Serializable]
	public class ThunderPrototype
	{
		public AnimationCurve LightIntensityChange;
		public float Delay = 0.7f;
		public AudioClip Sound;
	}

	public class Thunder : MonoBehaviour
	{
		[HideInInspector] public Executor Executor;

		public Light ThunderLight;
		public AudioSource ThunderAudio;
		public ThunderPrototype[] Protos = new ThunderPrototype[0];

		bool isPlaying = false;
		int protoIndex = 0;
		float playTime = 0;
		float strength = 0;

		public float Freq = 4;
		float checkTime = 0;

		bool valid
		{
			get
			{
				if (Executor == null)
					return false;
				if (Protos == null)
					return false;
				if (Protos.Length == 0)
					return false;
				if (ThunderLight == null)
					return false;
				if (ThunderAudio == null)
					return false;
				return true;
			}
		}

		void Play ()
		{
			if (!valid)
				return;
			if (!isPlaying)
			{
				isPlaying = true;
				protoIndex = Mathf.FloorToInt(UnityEngine.Random.value * (Protos.Length - 0.001f));
				strength = Mathf.Sqrt(UnityEngine.Random.value);
				ThunderAudio.clip = Protos[protoIndex].Sound;
				transform.localPosition = new Vector3 (UnityEngine.Random.value * 500, 300, UnityEngine.Random.value * 500);
				playTime = 0;
				ThunderLight.intensity = 0;
				ThunderLight.cullingMask = Executor.Settings.LightCullingMask;
				ThunderLight.enabled = true;
				ThunderAudio.volume = 0;
				ThunderAudio.enabled = false;
			}
		}

		void EndPlaying ()
		{
			isPlaying = false;
			ThunderLight.intensity = 0;
			ThunderLight.enabled = false;
			ThunderAudio.volume = 0;
			ThunderAudio.enabled = false;
		}

		void Update ()
		{
			if (!valid)
			{
				EndPlaying();
				return;
			}
			if (isPlaying)
			{
				playTime += Time.deltaTime;
				if (playTime > Protos[protoIndex].Delay && !ThunderAudio.enabled)
				{
					ThunderAudio.enabled = true;
					ThunderAudio.volume = strength * Executor.Settings.SoundVolume;
					ThunderAudio.Play();
				}
				if (ThunderAudio.clip.length - ThunderAudio.time < 1)
				{
					ThunderAudio.volume = strength * Executor.Settings.SoundVolume * (ThunderAudio.clip.length - ThunderAudio.time);
				}
				ThunderLight.intensity = Protos[protoIndex].LightIntensityChange.Evaluate(playTime) * strength;
				if (playTime > 15)
					EndPlaying();
			}
			else
			{
				checkTime += Time.deltaTime;
				if (checkTime > Freq)
				{
					checkTime = 0;
					float p = (Executor.WetCoef - 0.6f) * 0.5f;
					if (UnityEngine.Random.value < p)
						Play();
				}
			}
		}
	}
}
