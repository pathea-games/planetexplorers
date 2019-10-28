using UnityEngine;
using System.Collections.Generic;
using WhiteCat.UnityExtension;

namespace WhiteCat
{
	public class ParticlePlayer : MonoBehaviour
	{
		[SerializeField]
		List<ParticleSystem> particles;

		[SerializeField]
		new Light light;

		[SerializeField]
		float lightDelay = 0.025f;

		[SerializeField]
		float lightDuration = 0.05f;


		float time;


		void Reset()
		{
			enabled = false;
			particles = new List<ParticleSystem>(4);

			transform.TraverseHierarchy
				(
					(trans, depth) =>
					{
						var particle = trans.GetComponent<ParticleSystem>();
						if (particle) particles.Add(particle);

						var light = trans.GetComponent<Light>();
						if (light) this.light = light;
					}
				);
		}


		void Update()
		{
			time += Time.deltaTime;

			if (light.enabled)
			{
				if (time >= lightDuration)
				{
					enabled = false;
				}
			}
			else
			{
				if (time >= lightDelay)
				{
					light.enabled = true;
					time = 0;
				}
			}
		}


		void OnEnable()
		{
			for(int i=particles.Count - 1; i>=0; i--)
				particles[i].Play(false);

			if (light) time = 0;
			else enabled = false;
		}


		void OnDisable()
		{
			if(light) light.enabled = false;
		}
	}

}