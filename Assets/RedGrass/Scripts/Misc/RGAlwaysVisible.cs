using UnityEngine;
using System.Collections;

namespace RedGrass
{
	public class RGAlwaysVisible : MonoBehaviour
	{
		// Update is called once per frame
		void Update ()
		{
			if ( Camera.main != null )
				transform.position = Camera.main.transform.position + Camera.main.transform.forward * 20.0f;
		}
	}
}
