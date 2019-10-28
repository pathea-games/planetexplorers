using UnityEngine;
using System.Collections;
using Pathea.Maths;

namespace PeAudio
{
	public class SPOTDesc
	{
		public string path;
		public float density;
		public Range1D heightRange;
		public Range1D minDistRange;
		public Range1D maxDistRange;
	}
	
	public class SPOTInst : MonoBehaviour
	{
		public FMODAudioSource audioSrc;
		
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
			
		}
	}
}