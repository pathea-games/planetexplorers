using UnityEngine;
using System.Collections;

public class MaterialCopy : MonoBehaviour
{
	// Use this for initialization
	void Awake ()
	{
		Renderer[] rs = GetComponentsInChildren<Renderer>(true);
		foreach ( Renderer r in rs )
		{
			Material[] mats = new Material[r.materials.Length];
			for ( int i = 0; i < mats.Length; ++i )
				mats[i] = Material.Instantiate(r.materials[i]) as Material;
			r.materials = mats;
		}
	}
}
