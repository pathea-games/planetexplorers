using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class VCMeshMgr : MonoBehaviour
{
	public float m_VoxelSize;
	public Material m_MeshMat;
    public bool m_DaggerMesh = false; //R:0 L :1
    public bool m_LeftSidePos = false;
	public Dictionary<int, Color32> m_ColorMap = null;
	private Dictionary<int, List<GameObject>> m_MeshGOs = null;
	
	// Chunk pos to int key
	public static int ChunkPosToKey(IntVector3 pos)
	{
		return (pos.x) | (pos.z << 10) | (pos.y << 20);
	}
	public static int ChunkPosToKey(int x, int y, int z)
	{
		return (x) | (z << 10) | (y << 20);
	}
	// Int key to chunk pos

	public static IntVector3 KeyToChunkPos(int key)
	{
		return new IntVector3 ( key & 0x3ff, key >> 20, (key >> 10) & 0x3ff );
	}
	
	public void Init ()
	{
		m_MeshGOs = new Dictionary<int, List<GameObject>> ();
	}
	
	// Set a GameObject to a specified position and index
	public void Set (IntVector3 pos, int index, GameObject go)
	{
		// Check index, avoid error
		if ( index < 0 || index > 255 )
		{
			Debug.LogError("[VCMeshMgr] set index error");
			return;
		}
		// Create a key
		int hash = ChunkPosToKey(pos);
		if ( !m_MeshGOs.ContainsKey(hash) )
			m_MeshGOs.Add(hash, new List<GameObject>());
		// Create list element
		List<GameObject> list = m_MeshGOs[hash];
		for ( int i = list.Count; i <= index; ++i )
			list.Add(null);
		// Assign GameObject
		if ( list[index] != null )
		{
			// Have old gameobject, destroy it!
			// but i will try to avoid this situation
			if ( list[index] != go )
			{
				Debug.LogWarning("This position already have a gameobject, will destroy it!");
				MeshFilter mf = list[index].GetComponent<MeshFilter>();
				if ( mf != null )
				{
					Mesh.Destroy(mf.mesh);
				}
				GameObject.Destroy(list[index]);
			}
		}
		list[index] = go;
		// Set Transform
		SetGO(pos, index);
	}
	public void SetGO (IntVector3 pos, int index)
	{
		int hash = ChunkPosToKey(pos);
		if ( !m_MeshGOs.ContainsKey(hash) )
			return;
		List<GameObject> list = m_MeshGOs[hash];
		if ( list == null )
			return;
		if ( index >= list.Count )
			return;
		GameObject go = list[index];
		float chunk_size = m_VoxelSize * VoxelTerrainConstants._numVoxelsPerAxis;
		go.name = "vc_" + pos.x.ToString() + pos.y.ToString() + pos.z.ToString() + "_" + index.ToString();
		go.transform.parent = this.transform;
		go.transform.localScale = m_VoxelSize * Vector3.one;
		go.transform.localRotation = Quaternion.identity;
		go.transform.localPosition = new Vector3 (pos.x * chunk_size, pos.y * chunk_size, pos.z * chunk_size) - 0.5f * m_VoxelSize * Vector3.one;
		go.layer = this.gameObject.layer;
	}
	// Query a GameObject list at a specified position
	public List<GameObject> Query (IntVector3 pos)
	{
		int hash = ChunkPosToKey(pos);
		if ( m_MeshGOs.ContainsKey(hash) )
		{
			return m_MeshGOs[hash];
		}
		return null;
	}
	// Query a GameObject list at a specified position
	public GameObject QueryAtIndex (IntVector3 pos, int index)
	{
		int hash = ChunkPosToKey(pos);
		if ( m_MeshGOs.ContainsKey(hash) )
		{
			List<GameObject> list = m_MeshGOs[hash];
			if ( list != null )
			{
				if ( index < list.Count )
					return list[index];
			}
		}
		return null;
	}
	
	// Query a specified chunk's position in this manager
	public IntVector3 QueryPos (GameObject go)
	{
		if ( go == null )
			return null;
		foreach ( KeyValuePair<int, List<GameObject>> kvp in m_MeshGOs )
		{
			if ( kvp.Value != null )
			{
				foreach ( GameObject _go in kvp.Value )
				{
					if ( _go == go )
						return KeyToChunkPos(kvp.Key);
				}
			}
		}
		return null;
	}
	// Is a GameObject exist in this manager
	public bool Exist (GameObject go)
	{
		foreach ( KeyValuePair<int, List<GameObject>> kvp in m_MeshGOs )
		{
			if ( kvp.Value != null )
			{
				foreach ( GameObject _go in kvp.Value )
				{
					if ( _go == go )
						return true;
				}
			}
		}
		return false;
	}
	// Is a GameObject exist at a specified position
	public bool Exist (IntVector3 pos, GameObject go)
	{
		int hash = ChunkPosToKey(pos);
		if ( !m_MeshGOs.ContainsKey(hash) )
			return false;
		List<GameObject> list = m_MeshGOs[hash];
		if ( list != null )
		{
			foreach ( GameObject _go in list )
			{
				if ( _go == go )
					return true;
			}
		}
		return false;
	}
	// Clear all GameObjects at a specified position
	public void Clear (IntVector3 pos)
	{
		int hash = ChunkPosToKey(pos);
		if ( m_MeshGOs.ContainsKey(hash) )
		{
			List<GameObject> list = m_MeshGOs[hash];
			if ( list != null )
			{
				foreach ( GameObject go in list )
				{
					if ( go != null )
					{
						MeshFilter mf = go.GetComponent<MeshFilter>();
						if ( mf != null )
						{
							Mesh.Destroy(mf.mesh);
						}
						GameObject.Destroy(go);
					}
				}
				list.Clear();
			}
			m_MeshGOs.Remove(hash);
		}
	}
	// Clamp list count at a specified position
	public void Clamp (IntVector3 pos, int target_count)
	{
		int hash = ChunkPosToKey(pos);
		if ( m_MeshGOs.ContainsKey(hash) )
		{
			List<GameObject> list = m_MeshGOs[hash];
			if ( list != null )
			{
				while ( list.Count > target_count )
				{
					if ( list[target_count] != null )
					{
						MeshFilter mf = list[target_count].GetComponent<MeshFilter>();
						if ( mf != null )
						{
							Mesh.Destroy(mf.mesh);
						}
						GameObject.Destroy(list[target_count]);
					}
					list.RemoveAt(target_count);
				}
			}
			if ( target_count == 0 )
				m_MeshGOs.Remove(hash);
		}
	}
	// Destroy all GameObjects in the manager
	public void FreeGameObjects ()
	{
		foreach ( KeyValuePair<int, List<GameObject>> kvp in m_MeshGOs )
		{
			if ( kvp.Value != null )
			{
				foreach ( GameObject go in kvp.Value )
				{
					if ( go != null )
					{
						MeshFilter mf = go.GetComponent<MeshFilter>();
						if ( mf != null )
						{
							if ( mf.mesh != null )
							{
								Mesh.Destroy(mf.mesh);
								mf.mesh = null;
							}
						}
						GameObject.Destroy(go);
					}
				}
				kvp.Value.Clear();
			}
		}
		m_MeshGOs.Clear();
	}
	
	public void SetMeshMat( Material mat )
	{
		foreach ( KeyValuePair<int, List<GameObject>> kvp in m_MeshGOs )
		{
			if ( kvp.Value != null )
			{
				foreach ( GameObject go in kvp.Value )
				{
					if ( go != null )
					{
						MeshRenderer mr = go.GetComponent<MeshRenderer>();
						if ( mr != null )
						{
							mr.material = mat;
						}
					}
				}
			}
		}
	}
	
    //
	// Mesh Color
	//
	
	// Key -- Vertex -- Pos convert
	public static int MeshVertexToColorKey(Vector3 vertex, int cx, int cy, int cz)
	{
		return VCIsoData.IPosToColorKey(new Vector3(vertex.x + cx*VoxelTerrainConstants._numVoxelsPerAxis - 0.5f, 
			                                        vertex.y + cy*VoxelTerrainConstants._numVoxelsPerAxis - 0.5f, 
			                                        vertex.z + cz*VoxelTerrainConstants._numVoxelsPerAxis - 0.5f));
	}
	public static int WorldPosToColorKey(Vector3 worldpos)
	{
		return VCIsoData.IPosToColorKey(worldpos/VCEditor.s_Scene.m_Setting.m_VoxelSize);
	}
	// Update all meshes' color
	public void UpdateAllMeshColor()
	{
        //Win32.HiPerfTimer hpt = new Win32.HiPerfTimer ();
        //hpt.Start();
		if ( m_ColorMap == null )
			return;
		if ( m_MeshGOs != null )
		{
	        foreach ( KeyValuePair<int, List<GameObject>> kvp in m_MeshGOs )
			{
				List<GameObject> list = kvp.Value;
				if ( list != null )
				{
					foreach ( GameObject meshgo in list )
					{
						if ( meshgo != null )
						{
							MeshFilter mf = meshgo.GetComponent<MeshFilter>();
							if ( mf != null )
								UpdateMeshColor(mf);
						}
					}
				}
			}
		}
        //hpt.Stop();
        //hpt.PrintDuration("UpdateAllMeshColors");
	}
	// Update a specified mesh's color
	public void UpdateMeshColor( MeshFilter mf )
	{
		if ( m_ColorMap == null )
			return;
		Mesh mesh = mf.mesh;
		if ( mesh == null )
			return;
		IntVector3 chunkpos = QueryPos(mf.gameObject);
		if ( chunkpos == null )
			return;

		Vector3 [] vertices = mesh.vertices;
		Color [] colors = mesh.colors;
		int l = vertices.Length;
		Color32 temp_color;
		Color32 null_color = VCIsoData.BLANK_COLOR;
		for ( int i = 0; i < l; i++ )
		{
			int key = MeshVertexToColorKey( vertices[i], chunkpos.x, chunkpos.y, chunkpos.z );
			if ( m_ColorMap.TryGetValue( key, out temp_color ) )
				colors[i] = temp_color;
			else
				colors[i] = null_color;
		}
		mesh.colors = colors;
	}
	// Prepare collider
	public bool m_ColliderDirty = false;
	public void PrepareMeshColliders()
	{
		foreach ( KeyValuePair<int, List<GameObject>> kvp in m_MeshGOs )
		{
			List<GameObject> list = kvp.Value;
			if ( list != null )
			{
				foreach ( GameObject go in list )
				{
					if ( go != null )
					{
						MeshCollider mc = go.GetComponent<MeshCollider>();
						MeshFilter mf = go.GetComponent<MeshFilter>();
						if ( mf != null )
						{
							if ( mc != null )
							{
								if ( mc.sharedMesh == null )
									mc.sharedMesh = mf.mesh;
							}
							else
							{
								go.AddComponent<MeshCollider>().sharedMesh = mf.mesh;
								Debug.LogWarning("In design, this cannot be happen");
							}
						}
					}
				}
			}
		}
		m_ColliderDirty = false;
	}
	
	#region U3D_INTERNAL_FUNCS
	// Cons & Des
	void Awake ()
	{
		Init();
	}
	void OnDestroy ()
	{
		FreeGameObjects();
	}
	// Use this for initialization
	void Start ()
	{
	
	}
	// Update is called once per frame
	void Update ()
	{
//		if ( Time.frameCount % 100 == 0 )
//		{
//			SetMeshMat(m_MeshMat);
//		}
	}
	#endregion
}
