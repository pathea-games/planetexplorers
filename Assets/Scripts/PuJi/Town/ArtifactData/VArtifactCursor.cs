using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using Pathea.Maths;

public class VArtifactCursor : MonoBehaviour
{
	#region MONOBEHAVIOUR
	[SerializeField] Transform Bound;
	[SerializeField] Transform OriginTransform;
	[SerializeField] Transform SceneGroup;
	[SerializeField] Transform ObjectGroup;
	[SerializeField] VCMeshMgr TMeshMgr;
	[SerializeField] VCMeshMgr WMeshMgr;

	[SerializeField] VCEMovingGizmo MGizmo;
	[SerializeField] VCERotatingGizmo RGizmo;

	public bool MGizmoEnabled = false;
	public bool RGizmoEnabled = false;

	VCMCComputer Computer;
	LineRenderer[] Edges;
	public VCIsoData ISO;
	public Vector3 Size { get { return Bound.localScale; } }
	public Vector3 Origin
	{
		get { return OriginTransform.position; }
	}
	public Vector3 XDir
	{
		get { return OriginTransform.right; }
	}
	public Vector3 YDir
	{
		get { return OriginTransform.up; }
	}
	public Vector3 ZDir
	{
		get { return OriginTransform.forward; }
	}

	// Use this for initialization
	void Start ()
	{
		Edges = Bound.GetComponentsInChildren<LineRenderer>(true);
		MGizmo.OnMoving = OnMoving;
		MGizmo.OnDrop = OnMovingEnd;
		RGizmo.OnRotating = OnRotating;
		RGizmo.OnDragBegin = OnGizmoBegin;
	}
	
	// Update is called once per frame
	void Update ()
	{
		MGizmo.gameObject.SetActive(MGizmoEnabled);
		RGizmo.gameObject.SetActive(RGizmoEnabled);

		if (ISO == null)
			return;

		MGizmo.transform.position = this.transform.position;
		RGizmo.transform.position = this.transform.position;
		MGizmo.transform.rotation = Quaternion.identity;
		RGizmo.transform.rotation = Quaternion.identity;
	}
	#endregion

	private static string s_PrefabPath = "Artifact/Artifact Cursor";
	public static VArtifactCursor Create (string full_path, int layer = 0)
	{
		GameObject res_go = Resources.Load(s_PrefabPath) as GameObject;
		if (res_go == null)
			throw new Exception("Load artifact cursor prefab failed");
		VArtifactCursor res_cursor = res_go.GetComponent<VArtifactCursor>();
		if (res_cursor == null)
			throw new Exception("Load artifact cursor prefab failed");

		VCIsoData iso_data = LoadIso(full_path);
		if ( iso_data == null )
			throw new Exception("Load artifact file error");

		VArtifactCursor art_cursor = VArtifactCursor.Instantiate(res_cursor) as VArtifactCursor;

		art_cursor.ISO = iso_data;
		art_cursor.SetBoundSize(new Vector3(iso_data.m_HeadInfo.xSize, iso_data.m_HeadInfo.ySize, iso_data.m_HeadInfo.zSize));
		art_cursor.Computer = new VCMCComputer ();
		art_cursor.Computer.Init(new IntVector3(iso_data.m_HeadInfo.xSize, iso_data.m_HeadInfo.ySize, iso_data.m_HeadInfo.zSize),
		                         art_cursor.TMeshMgr,  false);

		foreach ( VCComponentData cdata in iso_data.m_Components )
			cdata.CreateEntity(false, art_cursor.ObjectGroup);
		foreach ( KeyValuePair<int, VCVoxel> kvp in iso_data.m_Voxels )
			art_cursor.Computer.AlterVoxel(kvp.Key, kvp.Value);
		art_cursor.Computer.ReqMesh();
		art_cursor.gameObject.layer = layer;
		Transform[] ts = art_cursor.gameObject.GetComponentsInChildren<Transform>(true);
		foreach (Transform t in ts)
			t.gameObject.layer = layer;
		art_cursor.gameObject.SetActive(true);
		art_cursor.Bound.gameObject.SetActive(true);
		art_cursor.SceneGroup.gameObject.SetActive(true);

		return art_cursor;
	}

	private static VCIsoData LoadIso(string path)
	{
		try
		{
			VCIsoData iso_data = new VCIsoData ();
			string fullpath = path;
			using ( FileStream fs = new FileStream (fullpath, FileMode.Open, FileAccess.Read) )
			{
				byte[] iso_buffer = new byte [(int)(fs.Length)];
				fs.Read(iso_buffer, 0, (int)(fs.Length));
				fs.Close();
				if (iso_data.Import(iso_buffer, new VCIsoOption(false)))
					return iso_data;
				else
					return null;
			}
		}
		catch (Exception e)
		{
			Debug.LogError("Loading ISO Error : " + e.ToString());
			return null;
		}
	}

	void SetBoundSize (Vector3 size)
	{
		Edges = Bound.GetComponentsInChildren<LineRenderer>(true);
		Bound.localScale = size;
		float s = size.x + size.y + size.z;
		foreach (LineRenderer edge in Edges)
			edge.SetWidth(s * 0.01f, s * 0.01f);
		size.y = 0;
		Bound.localPosition = -size * 0.5f;
		SceneGroup.localPosition = Bound.localPosition;
	}

	public delegate void OnOutputVoxel(int x, int y, int z, VCVoxel voxel);

	public void OutputVoxels(Vector3 offset, OnOutputVoxel output_function)
	{
		if (ISO == null)
			return;
		if (output_function == null)
			return;
		foreach ( KeyValuePair<int, VCVoxel> kvp in ISO.m_Voxels )
		{
			Vector3 lpos = new Vector3(kvp.Key & 0x3ff, kvp.Key >> 20, (kvp.Key >> 10) & 0x3ff);
			Vector3 wpos = OriginTransform.position
				+ lpos.x * OriginTransform.right
				+ lpos.y * OriginTransform.up
				+ lpos.z * OriginTransform.forward;
			wpos += offset;

			INTVECTOR3 wpos_floor = new INTVECTOR3(Mathf.FloorToInt(wpos.x), Mathf.FloorToInt(wpos.y), Mathf.FloorToInt(wpos.z));
			INTVECTOR3 wpos_ceil = new INTVECTOR3(Mathf.CeilToInt(wpos.x), Mathf.CeilToInt(wpos.y), Mathf.CeilToInt(wpos.z));

			if (wpos_floor == wpos_ceil)
			{
				output_function(wpos_floor.x, wpos_floor.y, wpos_floor.z, kvp.Value);
			}
			else
			{
				for (int x = wpos_floor.x; x <= wpos_ceil.x; ++x)
				{
					for (int y = wpos_floor.y; y <= wpos_ceil.y; ++y)
					{
						for (int z = wpos_floor.z; z <= wpos_ceil.z; ++z)
						{
							float deltax = 1 - Mathf.Abs(wpos.x - x);
							float deltay = 1 - Mathf.Abs(wpos.y - y);
							float deltaz = 1 - Mathf.Abs(wpos.z - z);
							float u = deltax * deltay * deltaz;
							if (u < 0.5f)
								u = u / (0.5f+u);
							else
								u = 0.5f / (1.5f-u);
							VCVoxel voxel = kvp.Value;
							voxel.Volume = (byte)Mathf.CeilToInt(voxel.Volume * u);
							if (voxel.Volume > 1)
								output_function(x,y,z,voxel);
						}
					}
				}
			}
		}
	}

	void OnMoving (Vector3 ofs)
	{
		transform.position += ofs;
	}
	void OnMovingEnd ()
	{
		Vector3 pos = transform.position;
		pos.x = Mathf.RoundToInt(pos.x);
		pos.y = Mathf.RoundToInt(pos.y);
		pos.z = Mathf.RoundToInt(pos.z);
		transform.position = pos;
	}

	Quaternion oldrot;
	void OnGizmoBegin ()
	{
		oldrot = transform.rotation;
	}
	void OnRotating (Vector3 axis, float angle)
	{
		transform.rotation = oldrot;
		transform.Rotate(axis, angle, Space.World);
	}
}
