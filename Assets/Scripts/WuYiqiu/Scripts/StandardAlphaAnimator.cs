using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class StandardAlphaAnimator : MonoBehaviour, ICloneModelHelper
{
    public static bool s_Enable = true;
	public delegate void DNotify();
	public event DNotify onFadeIn;
	public event DNotify onFadeOut;


	public bool _GenFadeIn = false;
	public bool _GenFadeOut = false;
	public bool _RestShader = false;

	public bool LensFade = false;

	public enum BlendMode
	{
		Opaque,
		Cutout,
		Fade,		// Old school alpha-blending mode, fresnel does not affect amount of transparency
		Transparent // Physically plausible transparency mode, implemented as alpha pre-multiply
	}
	

	public SkinnedMeshRenderer m_SkinnedRenderer = null;
	SkinnedMeshRenderer _copySkinRenderer = null;
	Transform m_Trans;

	public BlendMode TransparentMode = BlendMode.Transparent; 

	void Awake ()
	{
		m_Trans = transform;
	}
	
	void OnDisable()
	{
		if (_copySkinRenderer != null)
			_copySkinRenderer.enabled = false;
	}
	

	bool _needRevert = false;
	void Update ()
	{
        if (!s_Enable)
            return;

		if (m_SkinnedRenderer == null)
			m_SkinnedRenderer = gameObject.GetComponent<SkinnedMeshRenderer>();


		if (m_SkinnedRenderer == null)
			return;

//		if (_copySkinRenderer != null)
//		{
//			_copySkinRenderer.transform.localPosition = m_SkinnedRenderer.transform.localPosition;
//			_copySkinRenderer.transform.localRotation = m_SkinnedRenderer.transform.localRotation;
//			_copySkinRenderer.transform.localScale = m_SkinnedRenderer.transform.localScale;
//		}

		CustomFadeUpdate();

		if (LensFade)
			UpdateLensFadeInOut();
		else
			_nearCam = false;

		if (!_nearCam && !_customFading)
		{
			if (_needRevert)
			{
				RevertMode();
				_needRevert = false;
			}
		}
		else
			_needRevert = true;


		if (_GenFadeIn)
		{
			FadeIn();
			_GenFadeIn = false;

		}

		if (_GenFadeOut)
		{
			FadeOut();
			_GenFadeOut = false;
		}

//		if (_RestShader)
//		{
//			foreach(Material mat in m_SkinnedRenderer.materials)
//			{
//				mat.shader = Shader.Find( mat.shader.name);
//			}
//			_RestShader = false;
//		}

	}
	

	#region Lens_FadeInOut
	public float MinDis = 1.5f;
	public float HeightOffset = 1.4f;
	
	public Material DepthOnlyMat = null;
	
	bool _nearCam = false;
	void UpdateLensFadeInOut()
	{
		if (Camera.main == null || _disable)
			return;

		Camera cam = Camera.main;
		
		Vector3 model_pos =  m_Trans.position;
		model_pos.y += HeightOffset;
		
		float dis = MinDis;
		float skin_dis = dis * 1.2f;

		float cur_dis = Vector3.SqrMagnitude(model_pos - cam.transform.position);

		if (cur_dis < skin_dis * skin_dis)
		{
			if (cur_dis < dis * dis)
			{
				if (_copySkinRenderer != null)
				{
					_copySkinRenderer.enabled = m_SkinnedRenderer.enabled;
				}

				_nearCam = true;

				CreateCopySkinnedRenderer();

				foreach (Material mat in m_SkinnedRenderer.materials)
				{
					SetupMaterialWithBlendMode(mat, TransparentMode);
				}

			}
			else
			{
				_nearCam = false;
			}
		}
		else
			_nearCam = false;
	}
	#endregion


	#region Custom_FadeInOut

	float m_FadeTime = 5;
	float m_CurTime;
    float m_CurAlpha = 1.0f;
	
	public enum EMode
	{
		None,
		FadeIn,
		FadeOut,
	}
	
	EMode m_Mode = EMode.None;

	bool _customFading = false;
	bool _disable = false;
	void CustomFadeUpdate ()
	{
		if (m_Mode != EMode.None)
		{
			CreateCopySkinnedRenderer();

			foreach (Material mat in m_SkinnedRenderer.materials)
			{
				SetupMaterialWithBlendMode(mat, TransparentMode);
			}
			_needRevert = true;
			
			m_CurTime += Time.deltaTime;

			// Mode : Fade In
			if (m_Mode == EMode.FadeIn)
			{
				_disable = false;
				if (Mathf.Abs(m_CurAlpha - 1.0f) <= 0.001f)
				{
					m_Mode = EMode.None;
					m_CurTime = 0.0f;
					_customFading = false;

					if (onFadeIn != null)
						onFadeIn();
				}
				else
				{
					m_CurAlpha = Mathf.Lerp(0, 1, m_CurTime / m_FadeTime);
					
					foreach (Material mat in m_SkinnedRenderer.materials)
					{
						Color color = mat.GetColor("_Color");
						color.a = m_CurAlpha;
						mat.SetColor("_Color",  color);
					}
					
					_customFading = true;
				}
			}
			// Mode : Fade Out
			else if (m_Mode == EMode.FadeOut)
			{
				_disable = true;
				if (m_CurAlpha < 0.001f)
				{
					m_Mode = EMode.None;
					m_CurTime = 0;

					if (onFadeOut != null)
						onFadeOut();

					if (_copySkinRenderer != null)
						_copySkinRenderer.enabled = false;
				}
				else
				{
					m_CurAlpha = Mathf.Lerp(1, 0, m_CurTime / m_FadeTime);

					foreach (Material mat in m_SkinnedRenderer.materials)
					{
						Color color = mat.GetColor("_Color");
						color.a = m_CurAlpha;
						mat.SetColor("_Color",  color);
					}

					_customFading = true;
				}
			}
		}
	}
	
	public void SetAlpha (float alpha)
	{
        if (!s_Enable)
            return;

        if (m_SkinnedRenderer == null)
            m_SkinnedRenderer = gameObject.GetComponent<SkinnedMeshRenderer>();
		CreateCopySkinnedRenderer();

		foreach (Material mat in m_SkinnedRenderer.materials)
		{
			SetupMaterialWithBlendMode(mat, TransparentMode);
		}

		foreach (Material mat in m_SkinnedRenderer.materials)
		{
			Color color = mat.GetColor("_Color");
			color.a = alpha;
			mat.SetColor("_Color",  color);
		}

	}

	public void ResetView()
	{
		if (!s_Enable)
			return;

		if (m_SkinnedRenderer == null)
			m_SkinnedRenderer = gameObject.GetComponent<SkinnedMeshRenderer>();

		foreach (Material mat in m_SkinnedRenderer.materials)
		{
			StandardAlphaAnimator.SetupMaterialWithBlendMode(mat, (StandardAlphaAnimator.BlendMode)mat.GetFloat("_Mode"));
		}

		foreach (Material mat in m_SkinnedRenderer.materials)
		{
			Color color = mat.GetColor("_Color");
			color.a = 1;
			mat.SetColor("_Color", color);
		}
		m_SkinnedRenderer.enabled = true;
    }



	public void FadeIn (float time = 2.0f)
	{
        //if (Mathf.Abs(m_CurAlpha - 1.0f) < 0.0001f)
        //    return;

		m_Mode = EMode.FadeIn;
		m_FadeTime = time;
        m_CurTime = Mathf.InverseLerp(0.0f, 1.0f, m_CurAlpha) * m_FadeTime;
	}

	public void FadeOut (float time = 2.0f)
	{
        //if (Mathf.Abs(m_CurAlpha) < 0.0001f)
        //    return;

		m_Mode = EMode.FadeOut;
		m_FadeTime = time;
        m_CurTime = Mathf.InverseLerp(1.0f, 0.0f, m_CurAlpha) * m_FadeTime;
	}

	void RevertMode ()
	{
		foreach (Material mat in m_SkinnedRenderer.materials)
		{
			Color color = mat.GetColor("_Color");
			color.a = 1;
			mat.SetColor("_Color",  color);
			SetupMaterialWithBlendMode(mat, (BlendMode)mat.GetFloat("_Mode"));
		}

		if (_copySkinRenderer != null)
			_copySkinRenderer.enabled = false;
	}

	#endregion

	void CreateRagdollGameObject()
    {
        Pathea.BiologyViewCmpt view = GetComponentInParent<Pathea.BiologyViewCmpt>();
        if(view != null && view.monoRagdollCtrlr != null)
        {
            Transform tr = PETools.PEUtil.GetChild(view.monoRagdollCtrlr.transform, name);
            if(tr != null)
            {
                GameObject go = new GameObject("Skinned_Copy");
                go.transform.parent = tr.parent;
            }
        }
    }

	void CreateCopySkinnedRenderer()
	{
		if (_copySkinRenderer == null)
		{
            CreateRagdollGameObject();

			GameObject go = new GameObject("Skinned_Copy");
			go.transform.parent = m_SkinnedRenderer.transform.parent;
			go.transform.localPosition = Vector3.zero;
			go.transform.localRotation = Quaternion.identity;
			
			_copySkinRenderer = go.AddComponent<SkinnedMeshRenderer>();
			_copySkinRenderer.rootBone = m_SkinnedRenderer.rootBone;
			_copySkinRenderer.bones = m_SkinnedRenderer.bones;
			_copySkinRenderer.localBounds = m_SkinnedRenderer.localBounds;
			_copySkinRenderer.lightmapIndex = m_SkinnedRenderer.lightmapIndex;
			_copySkinRenderer.lightmapScaleOffset = m_SkinnedRenderer.lightmapScaleOffset;

//			foreach(Material m in m_SkinnedRenderer.materials)
//			{
//				m.shader = Shader.Find( m.shader.name);
//			}
			if (DepthOnlyMat != null)
			{

				Material[] mats = new Material[m_SkinnedRenderer.materials.Length];
				for (int i = 0; i < mats.Length; i++)
				{
					Material copy_mat = Material.Instantiate(DepthOnlyMat);
					copy_mat.hideFlags = HideFlags.DontSave;
					mats[i] = copy_mat;
					CopyOrgMatToDepthMat(m_SkinnedRenderer.materials[i], mats[i]);
				}
				
				_copySkinRenderer.materials = mats;
			}
			else
				_copySkinRenderer.materials = m_SkinnedRenderer.materials;
			
			_copySkinRenderer.sharedMesh = m_SkinnedRenderer.sharedMesh;
			_copySkinRenderer.updateWhenOffscreen = m_SkinnedRenderer.updateWhenOffscreen;

			if (m_SkinnedRenderer.enabled)
				_copySkinRenderer.enabled  = true;
		}
		else
		{


			if (_copySkinRenderer.materials.Length != m_SkinnedRenderer.materials.Length || _copySkinRenderer.sharedMesh == null)
			{
			
				foreach (Material m in _copySkinRenderer.materials)
				{
					Material.Destroy(m);
				}

				Material[] mats = new Material[m_SkinnedRenderer.materials.Length];
				for (int i = 0; i < mats.Length; i++)
				{
					mats[i] = Material.Instantiate(DepthOnlyMat);
					mats[i].hideFlags = HideFlags.DontSave;
					CopyOrgMatToDepthMat(m_SkinnedRenderer.materials[i], mats[i]);
				}

				_copySkinRenderer.materials = mats;
				_copySkinRenderer.sharedMesh = m_SkinnedRenderer.sharedMesh;

			}
			else
			{
				for (int i = 0; i < _copySkinRenderer.materials.Length; i++)
				{
					CopyOrgMatToDepthMat(m_SkinnedRenderer.materials[i], _copySkinRenderer.materials[i]);
				}
			}

			_copySkinRenderer.rootBone = m_SkinnedRenderer.rootBone;
			_copySkinRenderer.bones = m_SkinnedRenderer.bones;
			_copySkinRenderer.localBounds = m_SkinnedRenderer.localBounds;
			_copySkinRenderer.lightmapIndex = m_SkinnedRenderer.lightmapIndex;
			_copySkinRenderer.lightmapScaleOffset = m_SkinnedRenderer.lightmapScaleOffset;
			_copySkinRenderer.updateWhenOffscreen = m_SkinnedRenderer.updateWhenOffscreen;


			if (m_SkinnedRenderer.enabled)
				_copySkinRenderer.enabled  = true;
		}


	}


	void CopyOrgMatToDepthMat (Material org_mat, Material depth_mat)
	{
		if ( depth_mat.HasProperty("_MainTex"))
			depth_mat.SetTexture("_MainTex", org_mat.GetTexture("_MainTex"));
		if (depth_mat.HasProperty("_Cutoff"))
			depth_mat.SetFloat("_Cutoff", org_mat.GetFloat("_Cutoff"));

		BlendMode mode = (BlendMode) org_mat.GetFloat("_Mode");
		if (mode == BlendMode.Cutout)
			depth_mat.EnableKeyword("_ALPHATEST_ON");
		else
			depth_mat.DisableKeyword("_ALPHATEST_ON");
	}
	
	public static void SetupMaterialWithBlendMode(Material material, BlendMode blendMode)
	{
		switch (blendMode)
		{
		case BlendMode.Opaque:
			material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
			material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
			material.SetInt("_ZWrite", 1);
			material.DisableKeyword("_ALPHATEST_ON");
			material.DisableKeyword("_ALPHABLEND_ON");
			material.DisableKeyword("_ALBEDO_ALPHATEST_ON");
			material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
			material.renderQueue = -1;
			break;
		case BlendMode.Cutout:
			material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
			material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
			material.SetInt("_ZWrite", 1);
			material.EnableKeyword("_ALPHATEST_ON");
			material.DisableKeyword("_ALPHABLEND_ON");
			material.DisableKeyword("_ALBEDO_ALPHATEST_ON");
			material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
			material.renderQueue = 2450;
			break;
		case BlendMode.Fade:
			material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
			material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
			material.SetInt("_ZWrite", 0);
			material.DisableKeyword("_ALPHATEST_ON");
			material.EnableKeyword("_ALPHABLEND_ON");
			material.EnableKeyword("_ALBEDO_ALPHATEST_ON");
			material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
			material.renderQueue = 3000;
			break;
		case BlendMode.Transparent:
			material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
			material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
			material.SetInt("_ZWrite", 0);
			material.DisableKeyword("_ALPHATEST_ON");
			material.DisableKeyword("_ALPHABLEND_ON");
			material.EnableKeyword("_ALBEDO_ALPHATEST_ON");
			material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
			material.renderQueue = 3000;
			break;
		}
	}
}
