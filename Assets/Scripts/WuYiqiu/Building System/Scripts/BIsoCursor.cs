using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;

public class BIsoCursor : MonoBehaviour 
{
	[SerializeField] Transform Bound;
	public Transform OriginTransform;
	[SerializeField] Transform BlockGroup;

	public BSGizmoTriggerEvent gizmoTrigger;
	

	public BSIsoData ISO;

	private BSB45Computer Computer;
	//LineRenderer[] Edges;

	public BSIsoHeadData testHead;


	// Create ISO
	private static string s_PrefabPath =  "Prefab/Iso Cursor";
	public static BIsoCursor CreateIsoCursor (string full_path, int layer = 0)
	{
		GameObject res_go = Resources.Load(s_PrefabPath) as GameObject;

		if (res_go == null)
			throw new Exception("Load iso cursor prefab failed");

		BIsoCursor res_cursor = res_go.GetComponent<BIsoCursor>();
		if (res_cursor == null)
			throw new Exception("Load iso cursor prefab failed");

		BIsoCursor iso_cursor = BIsoCursor.Instantiate(res_cursor) as BIsoCursor;

		BSIsoData iso = LoadISO(full_path);
		iso_cursor.ISO = iso;
		 
		if (iso.m_HeadInfo.Mode == EBSVoxelType.Block)
		{
			Vector3 size = new Vector3(iso.m_HeadInfo.xSize, iso.m_HeadInfo.ySize, iso.m_HeadInfo.zSize);
			iso_cursor.SetBoundSizeOfBlock(size, iso_cursor.gameObject);
			iso_cursor.Computer = iso_cursor.BlockGroup.gameObject.AddComponent<BSB45Computer>();

			foreach (KeyValuePair<int, BSVoxel> kvp in iso_cursor.ISO.m_Voxels)
			{
				IntVector3 index = BSIsoData.KeyToIPos(kvp.Key);
				iso_cursor.Computer.AlterBlockInBuild(index.x, index.y, index.z, kvp.Value.ToBlock());
			}
			
			iso_cursor.Computer.RebuildMesh();
		}
		else if (iso.m_HeadInfo.Mode == EBSVoxelType.Voxel)
		{
			Debug.LogError("Cant Support the iso voxel");
			Destroy(res_go);
			Destroy(iso_cursor.gameObject);
			return null;
		}


		Transform[] ts = iso_cursor.gameObject.GetComponentsInChildren<Transform>(true);
		foreach (Transform t in ts)
			t.gameObject.layer = layer;

		iso_cursor.gameObject.SetActive(true);
		iso_cursor.BlockGroup.gameObject.SetActive(true);
		iso_cursor.Bound.gameObject.SetActive(true);

		iso_cursor.testHead = iso.m_HeadInfo;

		return iso_cursor;
	}

	private static BSIsoData LoadISO(string full_path)
	{
		try
		{
			BSIsoData iso_data = new BSIsoData ();

			using ( FileStream fs = new FileStream (full_path, FileMode.Open, FileAccess.Read) )
			{
				byte[] iso_buffer = new byte [(int)(fs.Length)];
				fs.Read(iso_buffer, 0, (int)(fs.Length));
				fs.Close();
				if (iso_data.Import(iso_buffer))
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

	public void SetOriginOffset (Vector3 offset)
	{
		OriginTransform.localPosition = offset;

		Vector3 size = Vector3.zero;

		if (ISO != null)
			size = new Vector3(ISO.m_HeadInfo.xSize % 2, 0, ISO.m_HeadInfo.zSize % 2) * BSBlock45Data.s_Scale;

		BlockGroup.localPosition = offset;
		Bound.localPosition = offset + size;
	}

	public delegate void OnOutputVoxel(Dictionary<int, BSVoxel> voxels, Vector3 originalPos);

	public void OutputVoxels(Vector3 offset, OnOutputVoxel output_function)
	{
		if (ISO == null)
			return;

		if (output_function == null)
			return;

		output_function(ISO.m_Voxels, OriginTransform.position);
	}

	void Start ()
	{
		//Edges = Bound.GetComponentsInChildren<LineRenderer>(true);
	}

	void SetBoundSizeOfBlock (Vector3 size, GameObject go)
	{
		//Edges = Bound.GetComponentsInChildren<LineRenderer>(true);

		Vector3 final_size = size * BSBlock45Data.s_Scale;
		Bound.localScale = final_size;
//		float s = final_size.x + final_size.y + final_size.z;
//		foreach (LineRenderer edge in Edges)
//			edge.SetWidth(s * 0.01f, s * 0.01f);
	}
	
}
