using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class UIHoloLinesHandler : MonoBehaviour
{
	UITexture uitex;
	public Shader HoloShader;
	public Color MainColor;
	public Texture2D MainTex;
	public Texture2D TexH;
	public Texture2D TexV;
	public float Intensity = 1.8f;
	public Vector4 TileAndSpeed = new Vector4 (48,7,1,1);

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
			uitex.material = new Material (HoloShader);

		uitex.material.SetColor("_Color", MainColor);
		uitex.material.SetTexture("_MainTex", MainTex);
		uitex.material.SetTexture("_TexH", TexH);
		uitex.material.SetTexture("_TexV", TexV);

		uitex.material.SetFloat("_Intensity", Intensity);
		uitex.material.SetVector("_TileSpeed", TileAndSpeed);
	}
}
