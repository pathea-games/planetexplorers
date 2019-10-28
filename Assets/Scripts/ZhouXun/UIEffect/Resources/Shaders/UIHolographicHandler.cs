using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class UIHolographicHandler : UIEffectAlpha
{
	UITexture uitex;
	public Shader HoloShader;
	public Color MainColor;
	public Texture2D MainTex;
	public Texture2D RandomTex;
	public Texture2D RandomTex2;
	public Texture2D AlphabetTex;
	public float Intensity = 1.8f;
	public float Speed = 1;
	public float Twinkle = 0.7f;
	public Vector2 Tile = new Vector2 (48,7);

	// Use this for initialization
	void Start ()
	{
		uitex = GetComponent<UITexture>();
		uitex.material = new Material (HoloShader);
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (uitex == null)
			return;
		
		if (uitex.material == null)
			uitex.material = new Material (HoloShader);

		Color _color = Color.Lerp(Color.black, MainColor, alpha);
		uitex.material.SetColor("_Color", _color);
		uitex.material.SetTexture("_MainTex", MainTex);
		uitex.material.SetTexture("_RandomTex", RandomTex);
		uitex.material.SetTexture("_RandomTex2", RandomTex2);
		uitex.material.SetTexture("_AlphabetTex", AlphabetTex);

		uitex.material.SetFloat("_Intensity", Intensity);
		uitex.material.SetFloat("_Speed", Speed);
		uitex.material.SetFloat("_Height", transform.localScale.y);
		uitex.material.SetFloat("_Twinkle", GetTwinke());
		uitex.material.SetVector("_Tile", new Vector4 (Tile.x, Tile.y, 0, 0));
	}

	float GetTwinke()
	{
		if (GameUI.Instance != null)
			return GameUI.Instance.bReflashUI ? Twinkle : 0.01f;
		else 
			return Twinkle;
	}
}
