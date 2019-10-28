using UnityEngine;
using System.Collections;

namespace EpsilonIndi{
	public class PlanetAppearance : MonoBehaviour {
		public float diameter;
		float c_diameter = 1f;
		void Start()
		{
			transform.localScale = Vector3.one * c_diameter;
		}
		void Update()
		{
			if(diameter != c_diameter)
			{
				c_diameter = diameter;
				transform.localScale = Vector3.one * c_diameter;
			}
		}	
	}

}
