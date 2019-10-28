using UnityEngine;
using System.Collections;

#if !UNITY_5
[ExecuteInEditMode]
public class CaveColorCorrection : ImageEffectBase {
	public float _threshold = 0.17f;
	public float _maxMultiL = 0.75f;
	public float _maxMultiR = 1.00f;

	// Called by camera to apply image effect
	void OnRenderImage (RenderTexture source, RenderTexture destination) {
		material.SetFloat("_threshold", _threshold);
		material.SetFloat("_maxMultiL", _maxMultiL);
		material.SetFloat("_maxMultiR", _maxMultiR);
		Graphics.Blit (source, destination, material);
	}
}
#else
public class CaveColorCorrection : MonoBehaviour
{
	public Shader shader;
}
#endif
