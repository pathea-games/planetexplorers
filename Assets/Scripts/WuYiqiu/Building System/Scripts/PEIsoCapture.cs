using UnityEngine;
using System.Collections;

public class PEIsoCapture : MonoBehaviour 
{
	public Camera captureCam = null;

	public RenderTexture  photoRT = null;

	public BSB45Computer Computer = null;

	private bool m_Capture = false;
	 

	public void EnableCapture ()
	{
		captureCam.gameObject.SetActive(true);
		m_Capture = true;
	}

	public void DisableCapture ()
	{
		captureCam.gameObject.SetActive(false);
		m_Capture = false;

		for (int i = 0; i < Computer.transform.childCount; i++)
		{
			Destroy(Computer.transform.GetChild(i).gameObject);
		}
	}
	

	Transform m_MeshRoot = null;
	void Awake ()
	{
		photoRT = new RenderTexture(64, 64, 8, RenderTextureFormat.ARGB32);
		captureCam.targetTexture = photoRT;

		m_MeshRoot = Computer.transform;
	}

	void Update ()
	{
		for (int i = 0; i < m_MeshRoot.childCount; i++)
		{
			GameObject go = m_MeshRoot.GetChild(i).gameObject;
			go.layer = m_MeshRoot.gameObject.layer;
		}
	}

	void LateUpdate ()
	{
		Camera cam = Camera.main;
		if (cam == null)
			return;

		if (m_Capture)
		{
			captureCam.transform.position = cam.transform.position;
			captureCam.transform.rotation = cam.transform.rotation;
		}
	}


	void OnDestroy ()
	{
		if (photoRT != null)
			photoRT.Release();
	}

}
