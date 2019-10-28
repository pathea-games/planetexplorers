using UnityEngine;
using System.Collections.Generic;

public abstract class TransformGizmo : MonoBehaviour
{
	public Camera MainCamera;
	public bool useCustomMouse = false;
	public Vector3 customMousePosition;
	public Material material;
	public bool hideCenter = false;
	public List<Transform> Targets;
	public Transform Focus
	{
		get
		{
			if (Targets == null || Targets.Count == 0)
				return null;
			else
				return Targets[Targets.Count-1];
		}
	}
	public Space Orientation;

	[SerializeField] protected Transform GizmoGroup;

	[SerializeField] protected Color XColor;
	[SerializeField] protected Color YColor;
	[SerializeField] protected Color ZColor;
	[SerializeField] protected Color HColor;
	protected Material linemat;
	protected Camera cam;

	public virtual bool MouseOver { get { return false; } }
	public virtual bool Working { get { return false; } }


	public Vector3 mousePosition
	{
		get { return useCustomMouse ? customMousePosition : Input.mousePosition; }
	}


	protected virtual void Awake ()
	{
		CreateLineMaterials();
		Targets = new List<Transform> (4);
	}

	protected virtual void Start ()
	{
		cam = gameObject.GetComponent<Camera>();

		if (cam == null)
		{
			cam = gameObject.AddComponent<Camera>();

			cam.backgroundColor = Color.clear;
			cam.cullingMask = 1 << gameObject.layer;
			cam.clearFlags = CameraClearFlags.Depth;
			cam.orthographic = false;
            cam.depth = 10000;
			cam.farClipPlane = 10f;
			cam.nearClipPlane = 3f;
			cam.renderingPath = RenderingPath.Forward;
			cam.useOcclusionCulling = false;
			cam.hdr = false;
		}

		cam.fieldOfView = MainCamera.fieldOfView;
		cam.targetTexture = MainCamera.targetTexture;
		cam.enabled = true;
    }

	protected virtual void OnDestroy ()
	{
		Material.Destroy(linemat);
	}

	// Update is called once per frame
	void Update ()
	{
		for (int i = Targets.Count - 1; i >= 0; --i)
		{
			if (Targets[i] == null)
				Targets.RemoveAt(i);
		}

		bool work = (Focus != null) && (MainCamera != null);
		GizmoGroup.gameObject.SetActive(work);
		cam.enabled = work;
		if (Focus != null)
		{
			if (!Input.GetMouseButton(0) && !Input.GetMouseButton(1) && !Input.GetMouseButton(2))
			{
				if (Input.GetMouseButtonUp(0))
				{
					EndModify();
				}
				Idle();
			}
			else
			{
				if (Input.GetMouseButtonDown(0))
				{
					BeginModify();
				}
				if (Input.GetMouseButton(0))
				{
					Modifying();
				}
			}
		}
	}

	void LateUpdate ()
	{
		if (Focus != null)
		{
			cam.fieldOfView = MainCamera.fieldOfView;
			Vector3 dir = (MainCamera.transform.position - Focus.position).normalized;
			float angle = Mathf.Clamp(Vector3.Angle(MainCamera.transform.forward, -dir), -80, 80) * Mathf.Deg2Rad;
			float dist = 7.5f / Mathf.Cos(angle);

			this.transform.position = Focus.position + dir * dist;
			this.transform.rotation = MainCamera.transform.rotation;
			GizmoGroup.position = Focus.position;
			if (Orientation == Space.Self)
				GizmoGroup.rotation = Focus.rotation;
			else
				GizmoGroup.rotation = Quaternion.identity;
			CustomLateUpdate();
		}
	}

	void OnPostRender ()
	{
		if (Focus != null)
		{
			GL.PushMatrix();
			
			if ( linemat.SetPass(0) )
				OnGL();
			
			GL.PopMatrix();
		}
	}

	protected abstract void OnGL ();
	protected virtual void CustomLateUpdate () {}
	protected abstract void Idle ();
	protected abstract void BeginModify ();
	protected abstract void Modifying ();
	protected abstract void EndModify ();

	void CreateLineMaterials()
	{
		if (!linemat) 
		{
			if (material) linemat = Instantiate(material);
			else linemat = Instantiate(WhiteCat.PEVCConfig.instance.handleMaterial);

			linemat.hideFlags = HideFlags.HideAndDontSave;
			linemat.shader.hideFlags = HideFlags.HideAndDontSave;
		}
	}
}
