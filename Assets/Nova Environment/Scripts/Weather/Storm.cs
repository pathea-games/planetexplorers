using UnityEngine;
using System.Collections;

namespace NovaEnv
{
	public class Storm : MonoBehaviour
	{
		[HideInInspector] public Executor Executor;
		public float WindTiltCoef = 0.15f;
		float strength = 0;
		public float Strength
		{
			get { return strength; }
			set
			{
				if (Mathf.Abs(strength - value) > 0.01f)
				{
					strength = value;
				}
			}
		}

		public AudioSource StormAudio;
		public AudioSource RainDropsAudio;
		public ParticleSystem RainDropsNear;
		public ParticleSystem RainDropsFar;
		public ParticleSystem RainCollisionNear;
		public ParticleSystem RainCollisionFar;
		public ParticleSystem RainSpray;

		public AnimationCurve StormSoundVolChange;
		public AnimationCurve RainDropsSoundVolChange;
		public AnimationCurve RainDropsCountChange;
		public AnimationCurve RainCollisionCountChange;
		public AnimationCurve RainSprayCountChange;

		void UpdateStrength ()
		{
			float sound_scale = 1;
			float hide_coef = 0;

			if (Executor.Settings.MainCamera != null)
			{
				//
				// House Test
				//

				Vector3 camPos = Executor.Settings.MainCamera.transform.position;

				for (float pitch = 30f; pitch < 90.0f; pitch += 15f)
				{
					float step = 360.0f;
					if (pitch < 40)
						step = 22.5f;
					else if (pitch < 70)
						step = 30f;
					else if (pitch < 80)
						step = 45f;
					else
						step = 360f;
					for (float yaw = 0f; yaw < 359.9f; yaw += step)
					{
						Ray ray = new Ray(camPos, new Vector3(Mathf.Cos(yaw * Mathf.Deg2Rad) * Mathf.Cos(pitch * Mathf.Deg2Rad),
						                                      Mathf.Sin(pitch * Mathf.Deg2Rad),
						                                      Mathf.Sin(yaw * Mathf.Deg2Rad) * Mathf.Cos(pitch * Mathf.Deg2Rad)));

						Ray invray = new Ray(ray.GetPoint(50), -ray.direction.normalized);

						RaycastHit rch;
						if (Physics.Raycast(ray, out rch, 50))
						{
							float d = rch.distance;
							hide_coef += Mathf.Sin(pitch * Mathf.Deg2Rad) * (step / 360f);
							if (pitch > 40 && Physics.Raycast(invray, out rch, 50))
							{
								float thick = Mathf.Clamp01((rch.distance - d) / 15);
								hide_coef += Mathf.Sin(pitch * Mathf.Deg2Rad) * (step / 360f) * thick;
							}
						}
					}
				}

				sound_scale -= Mathf.Clamp01(hide_coef * 0.12f);
			}

			float water_scale = 1f / Executor.UnderwaterDensity;

			float global_sv = Executor.Settings.SoundVolume;
			float rain_em = Executor.Settings.MaxRainParticleEmissiveRate;
			StormAudio.volume = Mathf.Lerp(StormAudio.volume, global_sv * StormSoundVolChange.Evaluate(Strength) * sound_scale * water_scale, 0.03f);
			RainDropsAudio.volume = Mathf.Lerp(RainDropsAudio.volume, global_sv * RainDropsSoundVolChange.Evaluate(Strength) * sound_scale * water_scale, 0.03f);
			RainDropsNear.emissionRate = rain_em * RainDropsCountChange.Evaluate(Strength);
			RainDropsFar.emissionRate = rain_em * RainDropsCountChange.Evaluate(Strength);
			RainCollisionNear.emissionRate = rain_em * RainCollisionCountChange.Evaluate(Strength);
			RainCollisionFar.emissionRate = rain_em * RainCollisionCountChange.Evaluate(Strength);
			RainSpray.emissionRate = rain_em * RainSprayCountChange.Evaluate(Strength) * 0.2f;

			RainDropsFar.gameObject.SetActive(Executor.Underwater <= 0);
			RainDropsNear.gameObject.SetActive(Executor.Underwater <= 0);
			RainCollisionFar.gameObject.SetActive(Executor.Underwater <= 0);
			RainCollisionNear.gameObject.SetActive(Executor.Underwater <= 0);
			RainSpray.gameObject.SetActive(Executor.Underwater <= 0);
		}

		void Update ()
		{
			UpdateStrength();
		}
	}
}
