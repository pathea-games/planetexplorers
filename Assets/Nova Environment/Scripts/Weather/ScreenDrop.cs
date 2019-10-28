using UnityEngine;
using System.Collections.Generic;

namespace NovaEnv
{
	public class ScreenDrop : MonoBehaviour
	{
		[HideInInspector] public Executor Executor;
		LineRenderer liner;
		public Transform drop;
		public float InitStrength = -0.1f;
		public float DryingSpeed = 0.02f;

		public Vector3 pos
		{
			get { return drop.localPosition; }
			set { drop.localPosition = value; }
		}

		Vector3 vel = Vector3.zero;
		List<Vector3> verts = new List<Vector3> ();

		// Use this for initialization
		void Start ()
		{
			liner = GetComponent<LineRenderer>();
			liner.SetVertexCount(2);
			liner.SetPosition(0, new Vector3(0f, 0.5f, 0f));
			liner.SetPosition(1, new Vector3(0f, 0.5f, 0f));
			verts.Add(pos + transform.InverseTransformDirection(drop.up) * 0.5f); 
		}
		
		// Update is called once per frame
		void Update ()
		{
			if (Random.value < 0.02f)
				Slip();
			pos = pos + vel * Time.deltaTime;
			verts[verts.Count - 1] = pos + transform.InverseTransformDirection(drop.up) * 0.5f;

			// Assign verts
			liner.SetVertexCount(verts.Count);
			int j = 0;
			for (int i = verts.Count - 1; i >= 0; --i, ++j)
			{
				liner.SetPosition(j, verts[i]);
			}
			InitStrength = InitStrength * Mathf.Clamp01(1 - DryingSpeed);
			liner.material.SetFloat("_DistortionStrength", InitStrength);
			drop.GetComponent<Renderer>().material.SetFloat("_DistortionStrength", InitStrength);
			if (Mathf.Abs(InitStrength) < 0.001f)
				GameObject.Destroy(this.gameObject);
		}

		public void Slip ()
		{
			Vector3 wind = Executor.Wind.WindDirection;
			wind.y = 0;
			wind = transform.parent.InverseTransformDirection(wind);
			wind.y = 0;
			wind = wind * 0.07f + Vector3.down;
			wind += Random.insideUnitSphere * 0.3f;
			wind.z = 0;
			
			vel = wind * (Random.value * 25.0f + 0.2f) * Mathf.Clamp01(Executor.WetCoef - 0.2f);
			drop.localRotation = Quaternion.LookRotation(Vector3.forward, -vel.normalized);
			verts.Add(pos + transform.InverseTransformDirection(drop.up) * 0.5f);
		}
	}
}
