using UnityEngine;
using System.Collections;

namespace NovaEnv
{
	public class ScreenDropsEmitter : MonoBehaviour
	{
		[HideInInspector] public Executor Executor;
		public GameObject ScreenDropPrefab;

		public bool EmitNow = false;

		void Emit ()
		{
			ScreenDrop drop = Utils.CreateGameObject(ScreenDropPrefab, "Screen Drop", this.transform).GetComponent<ScreenDrop>();
			drop.Executor = this.Executor;
			drop.pos = new Vector3(Random.value * 70 - 35, Random.value * 70 - 35, 0);
			drop.Slip();
		}

		// Use this for initialization
		void Start ()
		{
		
		}
		
		// Update is called once per frame
		void Update ()
		{
			if (Executor != null)
			{
				Ray ray = new Ray((Executor.Settings.MainCamera != null ? Executor.Settings.MainCamera.transform.position : Vector3.zero), Executor.Storm.transform.up);
				if (!Physics.Raycast(ray, 50))
				{
					float p = Executor.Storm.RainDropsCountChange.Evaluate(Executor.Storm.Strength) * 0.4f;
					if (Random.value < p)
						Emit();
					if (EmitNow)
					{
						Emit();
						EmitNow = false;
					}
				}
			}
		}
	}
}