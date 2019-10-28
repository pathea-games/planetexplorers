using UnityEngine;
using System.Collections.Generic;
using System.Xml;
using System.IO;

public class PeFmodEditor : MonoBehaviour
{
	public static bool active = false;

	//FMODAudioListener listener = null;

	MovingGizmo giz_move = null;
	RotatingGizmo giz_rotate = null;

	GUISkin gskin;

	public static List<FMODAudioSource> savedAudios = null;

	public static void Save ()
	{
		if (savedAudios != null)
		{
			string dir = GameConfig.GetUserDataPath() + "/PlanetExplorers/Audio Saves/";
			string path = dir + "audio_points.xml";
			if ( !Directory.Exists(dir) )
				Directory.CreateDirectory(dir);

			string xmlstr = "";
			xmlstr += "<AUDIOLIST>\r\n";
			foreach (FMODAudioSource audio in savedAudios)
			{
				xmlstr += audio.xml;
			}
			xmlstr += "</AUDIOLIST>\r\n";

			Pathea.IO.FileUtil.SaveBytes(path, System.Text.Encoding.UTF8.GetBytes(xmlstr));
		}
	}

	public static void Load ()
	{
		if (savedAudios != null)
		{
			foreach (FMODAudioSource audio in savedAudios)
				GameObject.Destroy(audio.gameObject);
			savedAudios.Clear();
			savedAudios = null;
		}
		savedAudios = new List<FMODAudioSource> ();
		string dir = GameConfig.GetUserDataPath() + "/PlanetExplorers/Audio Saves/";
		string path = dir + "audio_points.xml";
		if (File.Exists(path))
		{
			XmlDocument xmldoc = new XmlDocument ();
			xmldoc.Load(path);
			foreach (XmlNode node in xmldoc.DocumentElement.ChildNodes)
			{
				if (node.Name == "AUDIO")
				{
					Vector3 pos = Vector3.zero;
					pos.x = XmlConvert.ToSingle(node.Attributes["posx"].Value);
					pos.y = XmlConvert.ToSingle(node.Attributes["posy"].Value);
					pos.z = XmlConvert.ToSingle(node.Attributes["posz"].Value);
					Vector3 rot = Vector3.zero;
					rot.x = XmlConvert.ToSingle(node.Attributes["rotx"].Value);
					rot.y = XmlConvert.ToSingle(node.Attributes["roty"].Value);
					rot.z = XmlConvert.ToSingle(node.Attributes["rotz"].Value);
					string evtpath = node.Attributes["path"].Value;
					float volume = XmlConvert.ToSingle(node.Attributes["volume"].Value);
					float pitch = XmlConvert.ToSingle(node.Attributes["pitch"].Value);
					float mindist = XmlConvert.ToSingle(node.Attributes["mindist"].Value);
					float maxdist = XmlConvert.ToSingle(node.Attributes["maxdist"].Value);
					FMODAudioSource audio = CreateAudioSource(pos, rot);
					audio.path = evtpath;
					audio.volume = volume;
					audio.pitch = pitch;
					audio.minDistance = mindist;
					audio.maxDistance = maxdist;
					foreach (XmlNode pnode in node.ChildNodes)
					{
						if (pnode.Name == "PARAM")
						{
							string pn = pnode.Attributes["name"].Value;
							float pv = XmlConvert.ToSingle(pnode.Attributes["value"].Value);
							audio.SetParam(pn, pv);
						}
					}
					savedAudios.Add(audio);
				}
			}
		}
	}

	public static void AlterSaveState (FMODAudioSourceRTE rte, bool state)
	{
		FMODAudioSource found = savedAudios.Find(iter => iter == rte.audioSrc);
		if (found == null)
		{
			savedAudios.Add(rte.audioSrc);
			rte.gizmoColor = Color.green;
		}
		else
		{
			savedAudios.Remove(rte.audioSrc);
			rte.gizmoColor = Color.yellow;
		}
	}

	// Use this for initialization
	void Start () 
	{
		savedAudios = new List<FMODAudioSource> ();

		GameObject go_move = GameObject.Instantiate(Resources.Load("Prefabs/Moving Gizmo")) as GameObject;
		go_move.transform.parent = this.transform;
		giz_move = go_move.GetComponent<MovingGizmo>();
		giz_move.MainCamera = Camera.main;
		go_move.SetActive(false);

		GameObject go_rotate = GameObject.Instantiate(Resources.Load("Prefabs/Rotating Gizmo")) as GameObject;
		go_rotate.transform.parent = this.transform;
		giz_rotate = go_rotate.GetComponent<RotatingGizmo>();
		giz_rotate.MainCamera = Camera.main;
		go_rotate.SetActive(false);

		FMODAudioSourceRTE.OnEditingStateChange += OnAudioEdit;
		FMODAudioSourceRTE.OnSave += AlterSaveState;
		gskin = Resources.Load<GUISkin>("AudioRTESkin");
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (!active)
		{
			GameObject.Destroy(this.gameObject);
			return;
		}

		if (Input.GetMouseButtonDown(0) && FMODAudioSourceRTE.editing == null)
		{
			RaycastHit rch;
			if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out rch, 2000f, 1 << LayerMask.NameToLayer("PE Environment")))
			{
				FMODAudioSourceRTE castrte = rch.collider.gameObject.GetComponentInParent<FMODAudioSourceRTE>();
				FMODAudioSourceRTE.selected = castrte;
			}
			else
			{
				FMODAudioSourceRTE.selected = null;
			}
		}

		if (Input.GetKeyDown(KeyCode.Delete) && !Application.isEditor ||
		    Input.GetKeyDown(KeyCode.Comma) && Application.isEditor)
		{
			savedAudios.Remove(FMODAudioSourceRTE.selected.audioSrc);
			GameObject.Destroy(FMODAudioSourceRTE.selected.gameObject);
			FMODAudioSourceRTE.selected = null;
		}
	}

	void OnDestroy ()
	{
		//MonoBehaviour.Destroy(listener);
		FMODAudioSourceRTE.OnEditingStateChange -= OnAudioEdit;
		FMODAudioSourceRTE.OnSave -= AlterSaveState;
	}

	Rect windowRect = new Rect(200, 100, 265, 200);
	void OnGUI ()
	{
		GUI.depth = -10000;
		GUI.skin = gskin;
		windowRect = GUI.Window(6131504, windowRect, FMODWindow, "FMOD Editor");
	}

	void FMODWindow (int id)
	{
		GUI.depth = -10000;
		GUI.DragWindow(new Rect(0,0,265,18));
//		if (GUI.Button(new Rect(15,30,110,25), "Create Listener"))
//		{
//			if (FMODAudioListener.listener == null)
//			{
//				GameObject go = GameObject.Find("AudioListener");
//				if (go != null)
//				{
//					listener = go.AddComponent<FMODAudioListener>();
//				}
//			}
//		}

		if (GUI.Button(new Rect(15,30,110,25), "Create Audio"))
		{
			RaycastHit rch;
			Vector3 pos = Camera.main.transform.position + Camera.main.transform.forward * 5f;
			if (Physics.Raycast(new Ray(Camera.main.transform.position, Camera.main.transform.forward), out rch, 5f))
			{
				pos = rch.point;
			}
			CreateAudioSource(pos, Vector3.zero);
		}

		if (giz_move.gameObject.activeSelf)
			GUI.color = Color.red;
		if (GUI.Button(new Rect(15,65,110,25), "Moving Gizmo"))
		{
			giz_move.gameObject.SetActive(true);
			giz_rotate.gameObject.SetActive(false);
		}
		GUI.color = Color.white;

		if (GUI.Button(new Rect(140,30,110,25), "No Gizmo"))
		{
			giz_move.gameObject.SetActive(false);
			giz_rotate.gameObject.SetActive(false);
		}

		if (giz_rotate.gameObject.activeSelf)
			GUI.color = Color.red;
		if (GUI.Button(new Rect(140,65,110,25), "Rotation Gizmo"))
		{
			giz_move.gameObject.SetActive(false);
			giz_rotate.gameObject.SetActive(true);
		}
		GUI.color = Color.white;

//		if (GUI.Button(new Rect(140,100,110,25), "Show/Hide Audios"))
//		{
//			FMODAudioSource.rteActive = !FMODAudioSource.rteActive;
//		}
		
		if (GUI.Button(new Rect(15,125,110,25), "Load"))
		{
			Load();
		}

		if (GUI.Button(new Rect(140,125,110,25), "Save"))
		{
			Save();
		}
		GUI.Label(new Rect(17,163,250,25), savedAudios.Count.ToString() + " audio(s) will be saved.");
	}

	static FMODAudioSource CreateAudioSource (Vector3 pos, Vector3 rot)
	{
		GameObject group = GameObject.Find("Audio RTE Group");
		if (group == null)
		{
			group = new GameObject ("Audio RTE Group");
			group.transform.position = Vector3.zero;
		}
		GameObject audio_go = new GameObject ("Audio Source");
		audio_go.transform.position = pos;
		audio_go.transform.eulerAngles = rot;
		audio_go.transform.parent = group.transform;
		return audio_go.AddComponent<FMODAudioSource>();
	}

	void OnAudioEdit (FMODAudioSourceRTE rte, bool edit)
	{
		giz_move.Targets.Clear();
		giz_rotate.Targets.Clear();
		if (edit)
		{
			giz_move.Targets.Add(rte.transform);
			giz_rotate.Targets.Add(rte.transform);
		}
	}
}
