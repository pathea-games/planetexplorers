using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Camera))]
public class MSScanCam : MonoBehaviour {
	private int layerMaskVFTerrain;
	private int layerMaskMineral;
	public Shader shaderTerrain;

	public Color mainColor = new Color(0.08f,0.4f,0f,0.2f);
	public Color notVisibleColor = new Color(0.0f,0.03f,0.075f,0.2f);
	public Color holoColor = new Color(0.3f,0.7f,0.25f,0.2f);
	public Texture2D noizeTex;
	public float tile = 0.2f;
	public float speed = 2;
	public float body_intens = 0.27f;
	public float holo_intens = 0.8f;
	public float noise_intens = 0.15f;
	public float distance = 300f;

	// Use this for initialization
	void Start () {
		GetComponent<Camera>().enabled = false;
		layerMaskVFTerrain = (1<<Pathea.Layer.VFVoxelTerrain);
		layerMaskMineral = (1<<Pathea.Layer.MetalScan);
		if(shaderTerrain == null)	shaderTerrain = Shader.Find("Diffuse");
	}
	
	// Update is called once per frame
	void Update () {
		if(null == GameUI.Instance.mMainPlayer)
			return;
		Shader.SetGlobalColor("_Color_4Scan", mainColor);
		Shader.SetGlobalColor("_NotVisibleColor_4Scan", notVisibleColor);
		Shader.SetGlobalColor("_HoloColor_4Scan", holoColor);
		Shader.SetGlobalTexture("_NoiseTex_4Scan", noizeTex);
		Shader.SetGlobalFloat("_Tile_4Scan",tile);
		Shader.SetGlobalFloat("_Speed_4Scan",speed);
		Shader.SetGlobalFloat("_BodyIntensity_4Scan",body_intens);
		Shader.SetGlobalFloat("_HoloIntensity_4Scan",holo_intens);
		Shader.SetGlobalFloat("_NoiseIntensity_4Scan",noise_intens);
		Vector4 campos = new Vector4 (Pathea.PeCreature.Instance.mainPlayer.position.x, 
		                              Pathea.PeCreature.Instance.mainPlayer.position.y,
		                              Pathea.PeCreature.Instance.mainPlayer.position.z,
		                              distance);
		Vector4 camforward = new Vector4 (Pathea.PeCreature.Instance.mainPlayer.tr.forward.x, 
		                                  Pathea.PeCreature.Instance.mainPlayer.tr.forward.y,
		                                  Pathea.PeCreature.Instance.mainPlayer.tr.forward.z,
		                              distance);
		Shader.SetGlobalVector("_CameraPos_4Scan",campos);
		Shader.SetGlobalVector("_CameraForward_4Scan",camforward);

		GetComponent<Camera>().clearFlags = CameraClearFlags.SolidColor;
		GetComponent<Camera>().cullingMask = layerMaskVFTerrain;
		GetComponent<Camera>().RenderWithShader(shaderTerrain, null);	
		GetComponent<Camera>().clearFlags = CameraClearFlags.Nothing;
		GetComponent<Camera>().cullingMask = layerMaskMineral;
		GetComponent<Camera>().Render();

	}
}
