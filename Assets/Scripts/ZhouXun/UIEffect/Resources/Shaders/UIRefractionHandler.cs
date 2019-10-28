using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class UIRefractionHandler : MonoBehaviour
{
	UITexture uitex;
	public Shader _shader;
	public Texture2D MainTex;
	public Texture2D DistortionTex;
	public Texture2D RandomTex;
	public RectOffset Border = new RectOffset ();
	public float Intensity = 1.8f;
	public float Randomness = 0.05f;

	// Use this for initialization
	void Start ()
	{
		uitex = GetComponent<UITexture>();
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (uitex == null)
			return;

		if (uitex.material == null)
			uitex.material = new Material (_shader);

		uitex.material.SetTexture("_MainTex", MainTex);

		uitex.material.SetTexture("_DistortionTex", DistortionTex);
		uitex.material.SetTexture("_RandomTex", RandomTex);

		if (DistortionTex != null)
		{
			uitex.material.SetVector("_SizeSettings", new Vector4 (transform.localScale.x, transform.localScale.y, DistortionTex.width, DistortionTex.height));
			uitex.material.SetVector("_Border", new Vector4 (Border.left, Border.top, Border.right, Border.bottom));
		}
		else
		{
			uitex.material.SetVector("_SizeSettings", new Vector4 (transform.localScale.x, transform.localScale.y, 64, 64));
			uitex.material.SetVector("_Border", new Vector4 (0, 0, 0, 0));
		}
		uitex.material.SetFloat("_FadeTime", Time.time);
		uitex.material.SetFloat("_Intensity", Intensity);
		uitex.material.SetFloat("_Randomness", Randomness);
	}
}
