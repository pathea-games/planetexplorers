using UnityEngine;
using System.Collections;
using System;
using Pathea;

public class PeViewStudio : MonoBehaviour
{
	private static PeViewStudio _self;
	public static PeViewStudio Self
	{
		get
		{
			if (_self == null)
			{
				if (Application.isPlaying)
				{
					GameObject go = Resources.Load<GameObject>("Prefab/ViewStudio");
					if (go != null)
					{
						GameObject.Instantiate(go);
					}
				}
			}
			return _self;
		}
	}

	private int m_IDInitial = 0;

	/// <summary>
	/// Creates the view controller.
	/// </summary>
	/// <returns>The view controller.</returns>
	/// <param name="param">Parameter.</param>
	public static PeViewController CreateViewController(ViewControllerParam param)
	{
		GameObject res_go = Resources.Load("Prefab/ViewController") as GameObject;

		if (res_go == null)
			throw new Exception("Load view camera cursor prefab failed");

		PeViewController res_controller = res_go.GetComponent<PeViewController>();
		if (res_controller == null)
			throw new Exception("Load iso cursor prefab failed");

		PeViewController controller = PeViewController.Instantiate(res_controller);

		controller.transform.localPosition = Vector3.zero;
		controller.transform.localScale = Vector3.one;
		controller.transform.localRotation = Quaternion.identity;
		controller.SetParam(param);
		controller.ID = Self.m_IDInitial;
		Self.m_IDInitial++;
		return controller;

	}

	/// <summary>
	/// Clones the character model useful.
	/// </summary>
	/// <returns>The character view model.</returns>
	/// <param name="viewCmpt">View cmpt.</param>
	public static GameObject CloneCharacterViewModel(Pathea.BiologyViewCmpt viewCmpt, bool takePhoto = false)
	{
		if (viewCmpt == null) return null;
		GameObject viewGameObj = viewCmpt.CloneModel();
		if (viewGameObj == null) return null;

		ResetCloneModel(viewGameObj, takePhoto);

		return viewGameObj;

	}

	public static GameObject CloneModelObject(GameObject obj, bool takePhoto = false)
	{
		GameObject viewGameObj = Instantiate(obj);
		if (viewGameObj == null) return null;

		ResetCloneModel(viewGameObj, takePhoto);

		return viewGameObj;
	}


	static void ResetCloneModel(GameObject viewGameObj, bool takePhoto)
	{
		viewGameObj.name = "Clone Mesh";
		viewGameObj.transform.parent = Self.transform;
		viewGameObj.transform.localPosition = Vector3.zero;
		viewGameObj.layer = Layer.ShowModel;

		var anim = viewGameObj.GetComponentInChildren<Animator>();
		if (anim)
		{
			var clips = anim.GetCurrentAnimatorClipInfo(0);
			for (int i = 0; i < clips.Length; i++)
			{
				clips[i].clip.SampleAnimation(viewGameObj, clips[i].clip.length * 0.5f);
			}
		}

		CleanUpModel(viewGameObj);

		if (takePhoto)
		{
			//if (anim) Destroy(anim);

			var renderer = viewGameObj.GetComponentInChildren<SkinnedMeshRenderer>();
			if (renderer)
			{
				Mesh mesh = new Mesh();
				renderer.BakeMesh(mesh);

				renderer.gameObject.AddComponent<MeshFilter>().sharedMesh = mesh;
				var meshRenderer = renderer.gameObject.AddComponent<MeshRenderer>();
				meshRenderer.sharedMaterials = renderer.sharedMaterials;
				meshRenderer.useLightProbes = false;

				renderer.enabled = false;
				DestroyImmediate(renderer);
			}
		}
	}


	static void CleanUpModel(GameObject viewObj)
	{
		MonoBehaviour[] equipments = viewObj.GetComponentsInChildren<MonoBehaviour>();
		for (int i = 0; i < equipments.Length; i++)
		{
			if (equipments[i] is ICloneModelHelper)
			{
				(equipments[i] as ICloneModelHelper).ResetView();
			}
		}

		Renderer[] renders = viewObj.GetComponentsInChildren<Renderer>();
		foreach (Renderer render in renders)
		{
			//SkinnedMeshRenderer skin_renderer = render as SkinnedMeshRenderer;
			//if (skin_renderer != null)
			//{
			//	StandardAlphaAnimator alpha_animator = skin_renderer.gameObject.GetComponent<StandardAlphaAnimator>();
			//	if (alpha_animator != null)
			//	{
			//		foreach ( Material mat in skin_renderer.materials)
			//			StandardAlphaAnimator.SetupMaterialWithBlendMode(mat, (StandardAlphaAnimator.BlendMode) mat.GetFloat("_Mode"));
			//	}
			//}

			// Destory
			Component[] cmpts = render.gameObject.GetComponents<Component>();
			foreach (Component cmpt in cmpts)
			{
				if (cmpt.GetType() == typeof(SkinnedMeshRenderer)
					|| cmpt.GetType() == typeof(WhiteCat.SkinnedMeshRendererHelper)
					|| cmpt.GetType() == typeof(Animator)
					|| cmpt.GetType() == typeof(Transform)
					|| cmpt.GetType() == typeof(MeshRenderer)
					|| cmpt.GetType() == typeof(WhiteCat.ArmorBones))
					continue;

				Component.Destroy(cmpt);
			}

			render.gameObject.layer =
				render.gameObject.layer == Layer.GIEProductLayer ? Layer.ShowModelCreation : Layer.ShowModel;
		}

		Projector[] projectors = viewObj.GetComponentsInChildren<Projector>();
		for (int i = 0; i < projectors.Length; i++)
			projectors[i].gameObject.layer = Layer.ShowModelCreation;
	}

	/// <summary>
	/// The Photo Controller
	/// </summary>
	[SerializeField]
	PePhotoController _photoController;


	/// <summary>
	/// Takes the photo of the PeEnity.
	/// </summary>
	/// <returns>The photo.</returns>
	/// <param name="viewCmpt">View cmpt of the PeEntity.</param>
	/// <param name="width">Width.</param>
	/// <param name="height">Height.</param>
	/// <param name="local_pos">reletive position of target.</param>
	/// <param name="local_rot">local rotation.</param>
	public static Texture2D TakePhoto(Pathea.BiologyViewCmpt viewCmpt, int width, int height, Vector3 local_pos, Quaternion local_rot)
	{
		GameObject cloneGo = CloneCharacterViewModel(viewCmpt, true);
		if (cloneGo == null) return null;
		return DoTakePhoto(cloneGo, width, height, local_pos, local_rot);
	}


	public static Texture2D TakePhoto(GameObject model, int width, int height, Vector3 local_pos, Quaternion local_rot)
	{
		GameObject cloneGo = CloneModelObject(model, true);
		if (cloneGo == null) return null;
		return DoTakePhoto(cloneGo, width, height, local_pos, local_rot);
	}


	static Texture2D DoTakePhoto(GameObject cloneGo, int width, int height, Vector3 local_pos, Quaternion local_rot)
	{
		ViewControllerParam param = ViewControllerParam.DefaultPortrait;
		param.texWidth = width;
		param.texHeight = height;
		cloneGo.transform.localPosition = new Vector3(-500, 0, 0);
		Self._photoController.Set(cloneGo.transform, param);
		Self._photoController.SetLocalTrans(local_pos, local_rot);

		//Profiler.BeginSample ("Enable PhotoCtrl");
		Self._photoController.gameObject.SetActive (true);
		//Profiler.EndSample ();
		Texture2D tex = Self._photoController.TakePhoto();
		//Profiler.BeginSample ("Disable PhotoCtrl");
		Self._photoController.gameObject.SetActive (false);
		//Profiler.EndSample ();

		cloneGo.SetActive(false);
		//Destroy(cloneGo);
		return tex;
	}


	#region QUCK_VARS
	public static Vector3 s_HeadPhotoPos = new Vector3(-0.01f, 0.213f, -0.155f);
	public static Quaternion s_HeadPhotoRot = Quaternion.Euler(54.1671f, 344.2581f, 77.74331f);
	public static Vector3 s_ViewPos = new Vector3(0f, 1.27f, 1.8f);
	#endregion


	void Awake()
	{
		if (_self != null)
			throw new Exception("Only one viewStudio is exist");
		_self = this;
		_photoController.gameObject.SetActive (false);
	}
/*
	void Update()
	{
		if (Input.GetKeyUp (KeyCode.P)) {
			TakePhoto(Pathea.MainPlayer.Instance.entity.biologyViewCmpt, 1,1,Vector3.one,Quaternion.identity);
		}
	}
*/
}
