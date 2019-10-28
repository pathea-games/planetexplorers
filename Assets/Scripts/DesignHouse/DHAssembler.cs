using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

/*2

// Design house assembler
//[ExecuteInEditMode]
public class DHAssembler : MonoBehaviour {
	enum CubeType{
		Cube_Null,
		Cube_White,
		Cube_Red,
		Cube_Green,
		Cube_Blue,
		CubeType_MAX,
	}
	Color[] cubeColor = new Color[(int)CubeType.CubeType_MAX]
	{
		Color.clear,
		Color.white,
		Color.red,
		Color.green,
		Color.blue,
	};
	KeyCode[] cubeKey = new KeyCode[(int)CubeType.CubeType_MAX]
	{
		KeyCode.Escape,
		KeyCode.Keypad0,
		KeyCode.Keypad1,
		KeyCode.Keypad2,
		KeyCode.Keypad3,
	};
	Dictionary<IntVector3, DHElem> m_elemList = new Dictionary<IntVector3, DHElem>();
	VFStdVoxelDataSource m_voxelDataSource;
	List<VFVoxelChunkData> m_chunkListToRebuild;

	GameObject m_productGo;
	MeshFilter m_productMf;
	public int m_xChunkCount = 1, m_yChunkCount = 1, m_zChunkCount = 1;
	public Material m_material;
	
	bool bLeftCtrl = false;
	
	// Update is called once per frame
	float wantedZDistanceFromCamera = 8.0f; 
	CubeType cubeCursor = CubeType.Cube_Null;
	Vector3 curPosition = Vector3.zero;
	const int SizeOfGrid = 4;
	private int SizeAxisX, SizeAxisY, SizeAxisZ;
	
	void Start()
	{
		// coordination line
		SizeAxisX = SizeOfGrid*m_xChunkCount*32;
		SizeAxisY = SizeOfGrid*m_yChunkCount*32;
		SizeAxisZ = SizeOfGrid*m_zChunkCount*32;
		
		// game object
		m_productGo = new GameObject();
		m_productGo.name = "Product";
		m_productMf = m_productGo.AddComponent<MeshFilter>();
        m_productGo.AddComponent<MeshRenderer>();
 		
		m_voxelDataSource = new VFStdVoxelDataSource(m_xChunkCount, m_yChunkCount, m_zChunkCount);
		m_chunkListToRebuild = new List<VFVoxelChunkData>();
		for(int x = 0; x < m_xChunkCount; x++){
			for(int y = 0; y < m_yChunkCount; y++){
				for(int z = 0; z < m_zChunkCount; z++){
					m_voxelDataSource.writeChunk(x,y,z, VFVoxelChunkCompressed.Create(m_voxelDataSource, new IntVector3(x,y,z), new VFVoxel(0)));
				}
			}
		}
		StartCoroutine(DHAssembler.Chunk2Mesh(this));
	}
	void Update ()
	{
		for(int i = 1; i < (int)CubeType.CubeType_MAX; i++)
		{
			if(Input.GetKeyDown(cubeKey[i]))
			{
				cubeCursor = (CubeType)i;
			}
		}

		if(cubeCursor != CubeType.Cube_Null)
		{
			int x = (int)(curPosition.x/SizeOfGrid);
			int y = (int)(curPosition.y/SizeOfGrid);
			int z = (int)(curPosition.z/SizeOfGrid);
			IntVector3 pos = new IntVector3(x,y,z);
			
			if(Input.GetMouseButton(0))
			{
				if(m_elemList.ContainsKey(pos))
				{
					m_elemList.Remove(pos);
				}
				m_elemList.Add(pos, new DHElem((byte)cubeCursor));
				ReqUpdateMesh(pos);
			}
			else
			if(Input.GetMouseButton(1))
			{
				if(m_elemList.ContainsKey(pos))
				{
					m_elemList.Remove(pos);
				}
				//m_elemList.Add(pos, new DHElem((byte)cubeCursor));
				ReqUpdateMesh(pos);
			}
			else
			{
				if(bLeftCtrl)
				{
					float fScroll = Input.GetAxis("Mouse ScrollWheel");
					if(Mathf.Abs(fScroll) > 0.01f)
					{
						DHElem elem;
						if(m_elemList.TryGetValue(pos, out elem))
						{
							elem.Volume = (byte)Mathf.Clamp((int)(elem.Volume + fScroll*10), 128, 255);
							Debug.Log("Volume changed to "+elem.Volume);
							ReqUpdateMesh(pos);
						}
					}
				}
			}
			
			if(Input.GetKeyDown(cubeKey[0]))
			{
				cubeCursor = CubeType.Cube_Null;
				m_elemList.Clear();
				AssembleProduct();
			}
		}
	}
	void OnDrawGizmos()
	{
		Gizmos.color = new Color(0,0,1,0.2f);
		for(int z = 0; z < SizeAxisZ; z+=SizeOfGrid)
		{
			Gizmos.DrawLine(new Vector3(0,0,z), new Vector3(0,SizeAxisY,z));
			Gizmos.DrawLine(new Vector3(0,0,z), new Vector3(SizeAxisX,0,z));
		}
		Gizmos.color = new Color(0,1,0,0.2f);
		for(int y = 0; y < SizeAxisY; y+=SizeOfGrid)
		{
			Gizmos.DrawLine(new Vector3(0,y,0), new Vector3(0,y,SizeAxisZ));
		}
		Gizmos.color = new Color(1,0,0,0.2f);
		for(int x = 0; x < SizeAxisX; x+=SizeOfGrid)
		{
			Gizmos.DrawLine(new Vector3(x,0,0), new Vector3(x,0,SizeAxisZ));
		}
				
		Gizmos.color = new Color(1,0,0);
		Gizmos.DrawLine(new Vector3(0,0,0), new Vector3(SizeAxisX,0,0));
		Gizmos.color = new Color(0,1,0);
		Gizmos.DrawLine(new Vector3(0,0,0), new Vector3(0,SizeAxisY,0));
		Gizmos.color = new Color(0,0,1);
		Gizmos.DrawLine(new Vector3(0,0,0), new Vector3(0,0,SizeAxisZ));
		
		int cnt = m_elemList.Count;
		for(int i = 0; i < cnt; i++)
		{
			KeyValuePair<IntVector3, DHElem> pair = m_elemList.ElementAt(i);
			DHElem elem = pair.Value;
			IntVector3 pos = pair.Key;
			Gizmos.color = cubeColor[elem.Type];
			Vector3 cubeSize = new Vector3(SizeOfGrid,SizeOfGrid,SizeOfGrid);
			Vector3 vPos = new Vector3(pos.x,pos.y,pos.z)*SizeOfGrid;
			Gizmos.DrawCube(vPos+cubeSize*0.5f, cubeSize);
		}
		
		if(cubeCursor != CubeType.Cube_Null)
		{
#if false
			// orth cam,rot(0,0,0),
	        Vector3 curScreenSpace = new Vector3(Input.mousePosition.x, Input.mousePosition.y, wantedZDistanceFromCamera);
	        Vector3 curPosition = Camera.main.ScreenToWorldPoint(curScreenSpace) + Vector3.right;
			Gizmos.DrawCube(curPosition, new Vector3(1,1,1));
#else
			Color col = cubeColor[(int)cubeCursor];
			Gizmos.color = new Color(col.r, col.g, col.b, 0.1f);
			Gizmos.DrawLine(curPosition, new Vector3(curPosition.x,curPosition.y,0));
			Gizmos.DrawLine(curPosition, new Vector3(curPosition.x,0,curPosition.z));
			Gizmos.DrawLine(curPosition, new Vector3(0,curPosition.y,curPosition.z));
			
			Gizmos.color = col;
			Vector3 cubeSize = new Vector3(SizeOfGrid,SizeOfGrid,SizeOfGrid);
			Vector3 cubePos = curPosition+cubeSize*0.5f;
			Gizmos.DrawWireCube(cubePos, cubeSize);
			Gizmos.DrawWireCube(new Vector3(0,cubePos.y,cubePos.z), new Vector3(0.1f,cubeSize.y,cubeSize.z));
			Gizmos.DrawWireCube(new Vector3(cubePos.x,0,cubePos.z), new Vector3(cubeSize.x,0.1f,cubeSize.z));
#endif
		}
		
	}
	void OnGUI()
	{
		bLeftCtrl = Input.GetKey(KeyCode.LeftControl);
		if(!bLeftCtrl)
		{
			float fScroll = Input.GetAxis("Mouse ScrollWheel");
			float fScrollThreshold = 0.01f;
			if( Mathf.Abs(fScroll) > fScrollThreshold )
			{
				wantedZDistanceFromCamera += fScroll > fScrollThreshold ? 1 : -1;
				if(wantedZDistanceFromCamera < 0)	wantedZDistanceFromCamera = 0;
				if(wantedZDistanceFromCamera > SizeAxisZ-SizeOfGrid)	wantedZDistanceFromCamera = SizeAxisZ-SizeOfGrid;
			}
		}

		Plane xyPlane = new Plane(Vector3.forward, -wantedZDistanceFromCamera);
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		float dist;
		if(xyPlane.Raycast(ray, out dist))
		{
			curPosition = ray.GetPoint(dist);
			curPosition = new Vector3(Mathf.Floor(curPosition.x/SizeOfGrid), Mathf.Floor(curPosition.y/SizeOfGrid), Mathf.Floor(curPosition.z/SizeOfGrid))*SizeOfGrid;
			if(curPosition.x < 0)	curPosition.x = 0;
			if(curPosition.y < 0)	curPosition.y = 0;
			if(curPosition.z < 0)	curPosition.z = 0;
			
			if(curPosition.x > SizeAxisX-SizeOfGrid)	curPosition.x = SizeAxisX-SizeOfGrid;
			if(curPosition.y > SizeAxisY-SizeOfGrid)	curPosition.y = SizeAxisY-SizeOfGrid;
			//if(curPosition.z > SizeAxis-SizeOfGrid)	curPosition.z = SizeAxis-SizeOfGrid;
		}
		
		if(Application.isEditor)
		{
			GUI.Label(new Rect(10,10,160,20), ""+(curPosition/SizeOfGrid));
		}
		if(GUI.Button(new Rect(10,30,100,20), "Rotate X axis"))
		{
			//Vector3
		}
		GUI.Button(new Rect(10,50,100,20), "Rotate Y axis");
		GUI.Button(new Rect(10,70,100,20), "Rotate Z axis");
	}
	void OnDestroy()
	{
		for(int x = 0; x < m_xChunkCount; x++){
			for(int y = 0; y < m_yChunkCount; y++){
				for(int z = 0; z < m_zChunkCount; z++){
					VFVoxelChunk vc = m_voxelDataSource.readChunk(x,y,z) as VFVoxelChunk;
					if(vc!=null)
					{
						Destroy(vc);
					}
				}
			}
		}
		
		if(m_productGo != null )
		{
			if(m_productMf.mesh != null)
			{
				Destroy(m_productMf.mesh);
				m_productMf.mesh = null;
			}
			Destroy(m_productGo);
			m_productGo = null;
		}
	}
	
	void ReqUpdateMesh(IntVector3 pos)
	{
		DHElem elem;
		if(!m_elemList.TryGetValue(pos, out elem))
		{
			elem = new DHElem(0);
			elem.Volume = 0;
		}
		{
			int srcIdx = 0;


			for(int _z = 0; _z < DHElem.AxisSize; _z++)
			{
				for(int _y = 0; _y < DHElem.AxisSize; _y++)
				{
					for(int _x = 0; _x < DHElem.AxisSize; _x++)
					{
						m_voxelDataSource.Write(pos.x+_x, pos.y+_y, pos.z+_z, elem.vData[srcIdx++]);
						int chunkX = (pos.x+_x)>>VoxelTerrainConstants._shift;
						int chunkY = (pos.y+_y)>>VoxelTerrainConstants._shift;
						int chunkZ = (pos.z+_z)>>VoxelTerrainConstants._shift;
						
						VFVoxelTerrain.AddDirtyChunksToRebuildList(chunkX, chunkY, chunkZ, m_voxelDataSource, m_chunkListToRebuild);
					}
				}
			}
		}
	}
	
	// Helper
	public void AssembleProduct()
	{
		if(m_productMf.mesh != null)
		{
			Destroy(m_productMf.mesh);
			m_productMf.mesh = null;
		}
			

		List<CombineInstance> combineList = new List<CombineInstance>();
		for(int x = 0; x < m_xChunkCount; x++){
			for(int y = 0; y < m_yChunkCount; y++){
				for(int z = 0; z < m_zChunkCount; z++){
					VFVoxelChunk vc = m_voxelDataSource.readChunk(x,y,z) as VFVoxelChunk;
					if(vc != null)
					{
						CombineInstance combine = new CombineInstance();
						combine.mesh = vc._mesh;
						combine.transform = vc.transform.localToWorldMatrix;
						combineList.Add(combine);
					}
				}
			}
		}
		m_productMf.mesh.CombineMeshes(combineList.ToArray());
		m_productMf.mesh.Optimize();
		m_productGo.GetComponent<MeshRenderer>().material = m_material;
	}
	public static IEnumerator Chunk2Mesh(DHAssembler asm)
	{
		List<VFVoxelChunk> _chunkListInComputing = new List<VFVoxelChunk>();
		while(true)
		{

			if(asm.m_chunkListToRebuild.Count > 0 && oclMarchingCube.numChunks == 0)
			{
				{
					_chunkListInComputing.Clear();
					int i;
					for(i = 0; i < asm.m_chunkListToRebuild.Count; i++ )
					{
						VFVoxelChunkData chunk = asm.m_chunkListToRebuild[i];
						if(chunk != null)
						{
							if (oclMarchingCube.numChunks + 1 < oclMarchingCube.MAX_CHUNKS )
							{
								oclMarchingCube.AddChunkVolumeData(chunk.__chunkData);
								_chunkListInComputing.Add(chunk);
							}
							else
							{
								break;
							}
						}
					}
					asm.m_chunkListToRebuild.RemoveRange(0, i);
				}
						
				if(oclMarchingCube.numChunks > 0){
					uint numChunks = oclMarchingCube.numChunks;
					oclMarchingCube.computeIsosurface();
					for(int n = 0; n < numChunks; n++)
					{
						int chunkTotalVerts = n==numChunks-1 ? (oclScanLaucher.sumData[0]-oclScanLaucher.ofsData[n]) : (oclScanLaucher.ofsData[n+1]-oclScanLaucher.ofsData[n]);
						//if(chunkTotalVerts == 0)
						//	continue;
						
				       	Vector3[] vert = new Vector3[chunkTotalVerts];
				        Vector3[] norm = new Vector3[chunkTotalVerts];
						Vector4[] tang = new Vector4[chunkTotalVerts];
						Color[] colors = new Color[chunkTotalVerts];
						
						Array.Copy( oclMarchingCube.hPosArray.Target as Vector3[],oclScanLaucher.ofsData[n],vert,0,chunkTotalVerts);
						Array.Copy(oclMarchingCube.hNormArray.Target as Vector3[],oclScanLaucher.ofsData[n],norm,0,chunkTotalVerts);
						Array.Copy(oclMarchingCube.hTxInfoArray.Target as Vector4[],oclScanLaucher.ofsData[n],tang,0,chunkTotalVerts);
						for(int i = 0; i < chunkTotalVerts; i++)
						{
							colors[i] = Color.white;
						}
				        
						_chunkListInComputing[n]._mesh.Clear();
						_chunkListInComputing[n]._mesh.name = "ocl_mesh_0";
						_chunkListInComputing[n]._mesh.vertices = vert;
				        _chunkListInComputing[n]._mesh.normals = norm;
						_chunkListInComputing[n]._mesh.tangents = tang;
						_chunkListInComputing[n]._mesh.colors = colors;
						vert = null;
						norm = null;
						colors = null;
						
						_chunkListInComputing[n]._mesh.subMeshCount = 1;
						int[] subindice = new int[chunkTotalVerts];
						Array.Copy(MCOutputData.indiceConst, 0, subindice, 0, chunkTotalVerts);
						_chunkListInComputing[n]._mesh.SetTriangles(subindice, 0);
						_chunkListInComputing[n]._meshRenderer.material = asm.m_material;
					}
				}
			}
			yield return 0;
		}
	}
	public static void Model2AssembleData()
	{
	}
}
*/