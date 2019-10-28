using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class BillboardTerrain : MonoBehaviour
{
	public int xPos;
	public int zPos;
	public Vector3 m_Center;
	private Dictionary<int, List<TreeInfo>> m_AllTrees = new Dictionary<int, List<TreeInfo>> ();
	private Mesh _BMesh;

	public void SetTrees ( List<TreeInfo> tree_list )
	{
		Reset();
		int tcnt = 0;
		foreach ( TreeInfo ti in tree_list )
		{
			if ( ti.m_protoTypeIdx >= LSubTerrainMgr.Instance.GlobalPrototypeBillboardList.Length )
				continue;
			if ( LSubTerrainMgr.Instance.GlobalPrototypeBillboardList[ti.m_protoTypeIdx] == null )
				continue;
			if ( !m_AllTrees.ContainsKey(ti.m_protoTypeIdx) )
				m_AllTrees.Add(ti.m_protoTypeIdx, new List<TreeInfo> ());
			m_AllTrees[ti.m_protoTypeIdx].Add(ti);
			tcnt++;
		}

		if ( tcnt < 1 )
		{
			Reset();
			return;
		}

		Material[] mats = new Material[m_AllTrees.Count];
		int idx = 0;
		foreach ( KeyValuePair<int, List<TreeInfo>> iter in m_AllTrees )
			mats[idx++] = LSubTerrainMgr.Instance.GlobalPrototypeBillboardList[iter.Key];

		Vector3[] verts = new Vector3[tcnt*4] ;
		Vector3[] norms = new Vector3[tcnt*4] ;
		Vector2[] uvs = new Vector2[tcnt*4] ;
		List<int[]> indices = new List<int[]> ();

		int ofs = 0;
		foreach ( KeyValuePair<int, List<TreeInfo>> iter in m_AllTrees )
		{
			List<TreeInfo> list = iter.Value;
			int[] idxs = new int[list.Count * 6];
			indices.Add(idxs);
			int iofs = 0;
			foreach ( TreeInfo ti in list )
			{
				Bounds b = LSubTerrainMgr.Instance.GlobalPrototypeBounds[iter.Key];
				float xsize = Mathf.Max(b.extents.x, b.extents.z) * ti.m_widthScale;
				float ysize = b.extents.y * ti.m_heightScale;
				Vector3 center = b.center;
				center.x *= ti.m_widthScale;
				center.y *= ti.m_heightScale;
				center.z *= ti.m_widthScale;
				Vector3 wpos = LSubTerrUtils.TreeTerrainPosToWorldPos(xPos,zPos,ti.m_pos) + center;
				Vector3 pos = wpos - this.transform.position;
				Vector3 zaxis = wpos - m_Center;
				zaxis.y = 0;
				zaxis.Normalize();
				Vector3 yaxis = Vector3.up;
				Vector3 xaxis = Vector3.Cross(yaxis, zaxis);

				verts[ofs*4 + 0] = pos + xaxis * xsize + yaxis * ysize;
				verts[ofs*4 + 1] = pos - xaxis * xsize + yaxis * ysize;
				verts[ofs*4 + 2] = pos - xaxis * xsize - yaxis * ysize;
				verts[ofs*4 + 3] = pos + xaxis * xsize - yaxis * ysize;
				
				norms[ofs*4 + 0] = Vector3.up;
				norms[ofs*4 + 1] = Vector3.up;
				norms[ofs*4 + 2] = Vector3.up;
				norms[ofs*4 + 3] = Vector3.up;

				uvs[ofs*4 + 0] = new Vector2 (0,1);
				uvs[ofs*4 + 1] = new Vector2 (1,1);
				uvs[ofs*4 + 2] = new Vector2 (1,0);
				uvs[ofs*4 + 3] = new Vector2 (0,0);

				idxs[iofs*6 + 0] = ofs*4 + 0;
				idxs[iofs*6 + 1] = ofs*4 + 1;
				idxs[iofs*6 + 2] = ofs*4 + 2;
				idxs[iofs*6 + 3] = ofs*4 + 2;
				idxs[iofs*6 + 4] = ofs*4 + 3;
				idxs[iofs*6 + 5] = ofs*4 + 0;

				ofs++;
				iofs++;
			}
		}

		_BMesh = new Mesh ();
		_BMesh.subMeshCount = mats.Length;
		_BMesh.vertices = verts;
		_BMesh.normals = norms;
		_BMesh.uv = uvs;
		for ( int i = 0; i < indices.Count; ++i )
			_BMesh.SetTriangles(indices[i], i);

		MeshFilter mf = GetComponent<MeshFilter>();
		mf.mesh = _BMesh;
		MeshRenderer mr = GetComponent<MeshRenderer>();
		mr.materials = mats;
	}
	public void Reset ()
	{
		MeshFilter mf = GetComponent<MeshFilter>();
		MeshRenderer mr = GetComponent<MeshRenderer>();

		mf.mesh = null;
		mf.sharedMesh = null;
		mr.materials = new Material[0] ;
		mr.sharedMaterials = new Material[0] ;

		foreach ( KeyValuePair<int, List<TreeInfo>> iter in m_AllTrees )
		{
			if ( iter.Value != null )
				iter.Value.Clear();
		}
		m_AllTrees.Clear();
		if ( _BMesh != null )
		{
			Mesh.Destroy(_BMesh);
			_BMesh = null;
		}
	}
	void OnDestroy ()
	{
		Reset();
	}
}
