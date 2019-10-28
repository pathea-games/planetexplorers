using UnityEngine;
using System.Collections;

public class ReplaceShaderU3D : MonoBehaviour
{
	void Awake()
	{
		Renderer[] renders = transform.GetComponentsInChildren<Renderer>();
		foreach(Renderer rd in renders)
		{
			foreach(Material mat in rd.materials)
			{
				Shader sh = CorruptShaderReplacement.FindValidShader(mat.shader.name);
				if (sh != null)
					mat.shader = sh;
			}
		}
	}
}
