using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class UISpecularHandler : UIEffectAlpha
{
	UITexture uitex;
	public Shader SpecShader;
	public Texture2D MainTex;
	public Texture2D SpecularMask;
	public Texture2D RandomTex;
	public RectOffset Border = new RectOffset ();
	public float Intensity = 1.8f;
	public Color Color0;
	public Color Color1;
	public Color Color2;
	public Color Color3;
	public float Randomness = 0.05f;
	public float WaveThreshold = 0.28f;
	public float WaveLength = 20f;
	public float WaveSpeed = 4f;
	

	// Use this for initialization
	void Start ()
	{
		uitex = GetComponent<UITexture>();
		uitex.material = new Material (SpecShader);
	
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (uitex == null)
			return;

		if (uitex.material == null)
			uitex.material = new Material (SpecShader);

		uitex.material.SetTexture("_MainTex", MainTex);

		uitex.material.SetTexture("_SpecularMask", SpecularMask);
		uitex.material.SetTexture("_RandomTex", RandomTex);

		if (SpecularMask != null)
		{
			uitex.material.SetVector("_SizeSettings", new Vector4 (transform.localScale.x, transform.localScale.y, SpecularMask.width, SpecularMask.height));
			uitex.material.SetVector("_Border", new Vector4 (Border.left, Border.top, Border.right, Border.bottom));
		}
		else
		{
			uitex.material.SetVector("_SizeSettings", new Vector4 (transform.localScale.x, transform.localScale.y, 64, 64));
			uitex.material.SetVector("_Border", new Vector4 (0, 0, 0, 0));
		}
		uitex.material.SetFloat("_FadeTime", Time.time);
		Color _color0 = Color.Lerp(Color.black, Color0, alpha);
		uitex.material.SetColor("_Color0", _color0);
		Color _color1 = Color.Lerp(Color.black, Color1, alpha);
		uitex.material.SetColor("_Color1", _color1);
		Color _color2 = Color.Lerp(Color.black, Color2, alpha);
		uitex.material.SetColor("_Color2", _color2);
		Color _color3 = Color.Lerp(Color.black, Color3, alpha);
		uitex.material.SetColor("_Color3", _color3);
		uitex.material.SetFloat("_Intensity", Intensity);
		uitex.material.SetVector("_OtherSettings", new Vector4 (Randomness, WaveThreshold, WaveLength, WaveSpeed));
	}
}
