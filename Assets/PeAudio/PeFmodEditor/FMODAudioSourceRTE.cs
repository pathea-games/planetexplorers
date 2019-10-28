using UnityEngine;
using System.Collections;
using FMOD;
using FMOD.Studio;

public class FMODAudioSourceRTE : MonoBehaviour
{
	public FMODAudioSource audioSrc = null;
	GameObject gizmo;
	Collider col;
	Renderer r;
	Material mat;
	GUISkin gskin;
	GameObject bound;
	Material boundmat;

	public static FMODAudioSourceRTE selected = null;
	public static FMODAudioSourceRTE editing = null;
	public static bool showEditingPanel = true;

	//float camDist = 0;
	float listenDist = float.PositiveInfinity;
	public static FMODAudioListener listener { get { return FMODAudioListener.listener; } }

	public Color gizmoColor = Color.yellow;

	// Use this for initialization
	void Start ()
	{
		audioSrc = GetComponent<FMODAudioSource>();
		gizmo = GameObject.CreatePrimitive(PrimitiveType.Quad);
		gizmo.transform.parent = transform;
		gizmo.transform.localPosition = Vector3.zero;
		r = gizmo.GetComponent<Renderer>();
		mat = Material.Instantiate(Resources.Load<Material>("AudioGizmoMat")) as Material;
		r.material = mat;
		mat.color = Color.yellow;
		col = gizmo.GetComponent<Collider>();
		(col as MeshCollider).convex = true;
		col.isTrigger = true;

		bound = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		Collider c = bound.GetComponent<Collider>();
		Collider.DestroyImmediate(c);
		bound.transform.parent = transform;
		bound.transform.localPosition = Vector3.zero;
		Renderer br = bound.GetComponent<Renderer>();
		boundmat = Material.Instantiate(Resources.Load<Material>("AudioBoundMat")) as Material;
		br.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
		br.receiveShadows = false;
		br.material = boundmat;
		bound.SetActive(false);

		gizmo.layer = LayerMask.NameToLayer("PE Environment");
		gizmo.transform.rotation = Camera.main.transform.rotation;
		gskin = Resources.Load<GUISkin>("AudioRTESkin");

		if (PeFmodEditor.savedAudios != null)
		{
			FMODAudioSource found = PeFmodEditor.savedAudios.Find(iter => iter == audioSrc);
			if (found != null)
				gizmoColor = Color.green;
		}

		if (audioSrc.audioInst != null)
		{
			EventDescription _desc;
			audioSrc.audioInst.getDescription(out _desc);
			_desc.getPath(out inputPath);
		}
	}

	void OnDisable ()
	{
		if (selected == this)
		{
			selected = null;
			editing = null;
			if (OnEditingStateChange != null)
				OnEditingStateChange(this, false);
		}
	}
	
	void OnDestroy ()
	{
		if (selected == this)
		{
			selected = null;
			editing = null;
			if (OnEditingStateChange != null)
				OnEditingStateChange(this, false);
		}
		GameObject.Destroy(gizmo);
		GameObject.Destroy(bound);
		Material.Destroy(mat);
		Material.Destroy(boundmat);
	}
	
	// Update is called once per frame
	void Update ()
	{
		//camDist = Vector3.Distance(Camera.main.transform.position, transform.position);
		listenDist = float.PositiveInfinity;
		if (listener != null)
			listenDist = Vector3.Distance(listener.transform.position, transform.position);
		gizmo.transform.rotation = Camera.main.transform.rotation;
		if (selected == this)
		{
			if (Input.GetKeyDown(KeyCode.E))
			{
				if (editing == this)
					editing = null;
				else
					editing = this;
				if (OnEditingStateChange != null)
					OnEditingStateChange(this, editing == this);
			}
			if (Input.GetKeyDown(KeyCode.KeypadPlus))
			{
				if (OnSave != null)
					OnSave(this, true);
			}
		}
		mat.color = (selected == this) ? Color.Lerp(gizmoColor, Color.white, 0.8f)
			: new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, gizmoColor.a * 0.7f);
		bound.SetActive(boundactive);
	}

	string inputPath = "event:/";
	//string mindist = "";
	//string maxdist = "";
	bool boundactive = false;
	void OnGUI ()
	{
		boundactive = false;
		GUI.depth = -5000;
		if (editing == this)
		{
			if (Vector3.Dot(Camera.main.transform.forward, (transform.position - Camera.main.transform.position).normalized) > 0.1f)
			{
				GUI.skin = gskin;
				Vector3 screenPoint = Camera.main.WorldToScreenPoint(transform.position);
				screenPoint.x -= 30f;
				screenPoint.y -= 25f;

				int height = 120;
				if (audioSrc.audioInst != null)
				{
					int slidercnt = 0;
					audioSrc.audioInst.getParameterCount(out slidercnt);
					height = 304 + 25*slidercnt;
				}

				GUI.BeginGroup(new Rect(screenPoint.x, Screen.height - screenPoint.y, 300, height));
				GUI.Box(new Rect(0,0,300,height), "");
				GUILayout.BeginHorizontal();
				GUILayout.Space(5);
				GUILayout.BeginVertical();
				GUILayout.Space(8);
				{
					GUILayout.Label("Load Event");
					GUILayout.BeginHorizontal();
					{
						GUILayout.Space(13);
						inputPath = GUILayout.TextField(inputPath, GUILayout.Width(200));
						if (GUILayout.Button("Load", GUILayout.Width(50)))
						{
							audioSrc.path = inputPath;
						}
					}
					GUILayout.EndHorizontal();
					GUILayout.Space(15);
					GUILayout.Label("Audio Settings");
					if (audioSrc.audioInst != null)
					{
						EventDescription desc = null;
						audioSrc.audioInst.getDescription(out desc);
						if (desc != null && desc.isValid())
						{
							bool is3D = false;
							desc.is3D(out is3D);
							bool isOneshot = false;
							desc.isOneshot(out isOneshot);
							float mindist = 0, maxdist = 0;
							if (is3D)
							{
								desc.getMinimumDistance(out mindist);
								desc.getMaximumDistance(out maxdist);
								boundactive = true;
								bound.transform.localScale = Vector3.one * audioSrc.maxDistance * 2f;
							}
							string is3Dstr = is3D ? "3D Sound" : "2D Sound";
							string diststr = is3D ? ("Distance Area ( " + mindist.ToString("0.##") + "m ~ " + maxdist.ToString("0.##") + "m )") : ("");

							if (listener != null)
								LabelField("Distance", listenDist.ToString("#,##0.00") + " m");
							else
								LabelField("Distance", "-");

							LabelField(is3Dstr, diststr);

							audioSrc.minDistance = FloatField("Min Dist", audioSrc.minDistance);
							audioSrc.maxDistance = FloatField("Max Dist", audioSrc.maxDistance);

							LabelField("Is Oneshot", isOneshot.ToString());

							audioSrc.volume = Slider("Volume", audioSrc.volume, 0f, 1f);
							audioSrc.pitch = Slider("Pitch", audioSrc.pitch, 0f, 4f);

							int pcnt = 0;
							audioSrc.audioInst.getParameterCount(out pcnt);

							for (int i = 0; i < pcnt; ++i)
							{
								ParameterInstance pinst = null;
								audioSrc.audioInst.getParameterByIndex(i, out pinst);
								PARAMETER_DESCRIPTION pdesc = new PARAMETER_DESCRIPTION ();
								pinst.getDescription(out pdesc);
								float val = 0f, _val = 0f;
								pinst.getValue(out val);
								_val = Slider(pdesc.name, val, pdesc.minimum, pdesc.maximum);
								if (_val != val)
									pinst.setValue(_val);
							}

							GUILayout.Space(8);
							GUILayout.BeginHorizontal();
							{
								GUILayout.Space(17);
								if (GUILayout.Button("Play", GUILayout.Width(80))) audioSrc.Play();
								GUILayout.Space(4);
								if (GUILayout.Button("Stop", GUILayout.Width(80))) audioSrc.Stop();
								GUILayout.Space(4);
								if (GUILayout.Button("Unload", GUILayout.Width(80))) audioSrc.path = "";
							}
							GUILayout.EndHorizontal();
						}
					}
					else
					{
						GUILayout.BeginHorizontal();
						{
							GUILayout.Space(13);
							GUILayout.Label("No Event Loaded");
						}
						GUILayout.EndHorizontal();
					}
				}
				GUILayout.EndVertical();
				GUILayout.EndHorizontal();
				GUI.EndGroup();
			}
			else
			{
				if (audioSrc.is3D)
				{
					boundactive = true;
					bound.transform.localScale = Vector3.one * audioSrc.maxDistance * 2f;
				}
			}
		}
	}

	void LabelField(string label, string text)
	{
		GUILayout.BeginHorizontal();
		GUILayout.Space(13);
		GUILayout.Label(label, GUILayout.Width(66));
		GUILayout.Label(text);
		GUILayout.EndHorizontal();
	}
	string TextField(string label, string text)
	{
		GUILayout.BeginHorizontal();
		GUILayout.Space(13);
		GUILayout.Label(label, GUILayout.Width(66));
		string txt = GUILayout.TextField(text, GUILayout.Width(180));
		GUILayout.EndHorizontal();
		return txt;
	}
	float FloatField(string label, float value)
	{
		GUILayout.BeginHorizontal();
		GUILayout.Space(13);
		GUILayout.Label(label, GUILayout.Width(66));
		string txt = GUILayout.TextField(value.ToString(), GUILayout.Width(180));
		float v = value;
		GUILayout.EndHorizontal();
		if (float.TryParse(txt, out v))
			return v;
		return value;
	}
	float Slider(string label, float value, float min, float max)
	{
		GUILayout.BeginHorizontal();
		GUILayout.Space(13);
		GUILayout.Label(label, GUILayout.Width(66));
		GUILayout.BeginVertical(GUILayout.Width(130));
		GUILayout.Space(9);
		value = GUILayout.HorizontalSlider(value, min, max);
		GUILayout.EndVertical();
		GUILayout.Space(5);
		GUI.SetNextControlName(label + " edit");
		string edit = GUILayout.TextField(value.ToString("0.##"), GUILayout.Width(55));
		float edit_value = value;
		if (float.TryParse(edit, out edit_value))
			value = Mathf.Clamp(edit_value, min, max);
		GUILayout.EndHorizontal();
		return value;
	}

	public delegate void DNotify (FMODAudioSourceRTE rte, bool state);
	public static event DNotify OnEditingStateChange;
	public static event DNotify OnSave;
}
