using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Pathea.Maths;

namespace RedGrass
{
	public class RGRegion
	{
		public List<RGLODQuadTreeNode> nodes;
		public List<GameObject> oldGos;
	}

	/// <summary>
	/// Red Grass Mesh Creator
	/// </summary>
	public class RGCreator : DataIO<RGRegion> 
	{
		#region CONST & 

		public const int MaxCreateCountPerFrame = 1;

		#endregion

		#region EDITOR_VALS

		public Material grassMat = null;
		public Material triMat = null;

		#endregion

		Queue<RGRegion> mReqs;
		RGDataSource mData;

		OnReqsFinished onReqsFinished;

		public void Init (EvniAsset evni, RGDataSource data)
		{
			base.Init(evni);
			mData = data;
		}

		#region  inherited_FUNC
		public override bool AddReqs (RGRegion region)
		{
			if (mActive)
			{
				Debug.LogError("The Creator is already acitve");
				return false;
				
			}

			lock(mReqs)
			{
				mReqs.Enqueue(region);
				foreach (RGLODQuadTreeNode node in region.nodes )
					node.SyncTimeStamp();
			}

			return true;
		}

		public override bool AddReqs (List<RGRegion> nodes)
		{

			return false;
		}

		public override void ClearReqs ()
		{

		}

		public override void StopProcess ()
		{
			mActive = false;
			
			lock (mReqs)
			{
				mReqs.Clear();
			}

			lock (mOutputs)
			{
				mOutputs.Clear();
			}

			StopAllCoroutines();
		}

		public override void ProcessReqsImmediatly ()
		{
            try
            {
                mActive = false;

                ProcessReqs(mReqs);

                // Create Mesh
                foreach (OutpuRegion opr in mOutputs)
                {
                    List<OutputGroup> group = opr.groups;
                    for (int j = 0; j < group.Count; j++)
                    {
                        OutputGroup opg = group[j];

                        if (opg.node.IsOutOfDate())
                            continue;

                        RGLODQuadTreeNode node = opg.node;
                        string desc = "Grass Batch [" + node.xIndex.ToString() + ", " + node.zIndex.ToString() + "] _ LOD [" + node.LOD + "]";
                        GameObject go = CreateBatchGos(desc, opg.billboard, opg.tri);
                        node.gameObject = go;

                        if (go != null)
                        {
                            go.transform.parent = transform;
                            go.transform.localPosition = Vector3.zero;

                        }
                    }
                    ClearMeshes(opr.oldGos);
                }

                mOutputs.Clear();
                mIsGenerating = false;
                mActive = false;
            }
            catch
            {

            }
		}

		public override bool Active (OnReqsFinished call_back)
		{
			if (IsEmpty())
			{
				mActive = false;
				return false;
			}

			mActive = true;
			onReqsFinished = call_back;
			mStart = true;
			return true;
		}

		public override void Deactive ()
		{
			mActive = false;
		}

		public override bool isActive ()
		{
			return mActive;
		}
		#endregion

		#region Other_FUNC
		public void ClearMeshes (List<GameObject> gos)
		{
			if (gos == null)
				return;
			foreach (GameObject go in gos)
			{
				MeshFilter[] mfs = go.GetComponentsInChildren<MeshFilter>(true);
				foreach (MeshFilter mf in mfs)
				{
					Mesh.Destroy(mf.mesh);
					mf.mesh = null;
				}
				
				GameObject.Destroy(go);
			}
			
			gos.Clear();
		}
		
		public void ClearMesh (GameObject go)
		{
			if (go == null)
				return;
			
			MeshFilter[] mfs = go.GetComponentsInChildren<MeshFilter>(true);
			foreach (MeshFilter mf in mfs)
			{
				Mesh.Destroy(mf.mesh);
				mf.mesh = null;
			}
			
			GameObject.Destroy(go);
			
		}
		bool IsEmpty ()
		{
			bool empty = false;
			lock (mReqs)
			{
				empty = (mReqs.Count == 0);
			}
			
			return empty;
		}

		/// <summary>
		/// Sets the Y clip range . the higher or the lower is not randerer
		/// </summary>
		public void SetClip(int min_y, int max_y)
		{
			if (grassMat.HasProperty("_Clip"))
				grassMat.SetVector("_Clip", new Vector4(min_y, max_y));
			triMat.SetVector("_Clip", new Vector4(min_y, max_y));
		}

		List<RedGrassInstance> _grasses 	 = new List<RedGrassInstance>();
		List<RedGrassInstance> _triGrasses = new List<RedGrassInstance>();
		public void ProcessReqs (Queue<RGRegion> reqs)
		{
			while (true)
			{
				RGRegion region = null;
				lock (reqs)
				{
					if (mReqs.Count == 0)
						break;
					
					region = reqs.Dequeue();
				}
				
				OutpuRegion opr = new OutpuRegion();
				
				foreach (RGLODQuadTreeNode node in region.nodes)
				{
					List<RGChunk> chunks = node.GetChunks(mData);

					if (chunks == null)
						continue;
					
					foreach (RGChunk ck in chunks)
					{
                        foreach (KeyValuePair<int, RedGrassInstance> kvp in ck.grasses)
                        {
                            if (kvp.Value.Layer == 0)
                                _grasses.Add(kvp.Value);
                            else
                                _triGrasses.Add(kvp.Value);

                            if (ck.HGrass.ContainsKey(kvp.Key))
                            {
                                foreach (RedGrassInstance rgi in ck.HGrass[kvp.Key])
                                {
                                    if (rgi.Layer == 0)
                                        _grasses.Add(rgi);
                                    else
                                        _triGrasses.Add(rgi);
                                }
                            }
                        }
					}
					
					if (node.LOD >= mEvni.LODDensities.Length)
						node.LOD = mEvni.LODDensities.Length - 1;
					if (node.LOD < 0)
						node.LOD = 0;
                    RedGrassMeshComputer.ComputeMesh(_grasses, _triGrasses, mEvni.Density * mEvni.LODDensities[node.LOD]);

					_grasses.Clear();
					_triGrasses.Clear();
                    
					// Get Billboard output struct
					OutputGroup opg = new OutputGroup();
					opg.node = node;
					
					
					int billboardCnt = RedGrassMeshComputer.s_Output.BillboardCount; 
					int offsetCnt = 0;
					
					while (billboardCnt > 0)
					{
						int cnt = Mathf.Min(billboardCnt, mEvni.MeshQuadMaxCount);
						Vector3[] verts = new Vector3 [cnt*4];
						Vector3[] norms = new Vector3 [cnt*4];
						Vector2[] uvs = new Vector2 [cnt*4];
						Vector2[] uv2s = new Vector2 [cnt*4];
						Color32[] colors32 = new Color32 [cnt*4];
						int[] indices = new int [cnt*6];
						
						System.Array.Copy(RedGrassMeshComputer.s_Output.Verts, offsetCnt*4,  verts, 0, cnt*4);
						System.Array.Copy(RedGrassMeshComputer.s_Output.Norms, offsetCnt*4, norms, 0, cnt*4);
						System.Array.Copy(RedGrassMeshComputer.s_Output.UVs, offsetCnt*4, uvs, 0, cnt*4);
						System.Array.Copy(RedGrassMeshComputer.s_Output.UV2s, offsetCnt*4, uv2s, 0, cnt*4);
						System.Array.Copy(RedGrassMeshComputer.s_Output.Color32s, offsetCnt*4, colors32, 0, cnt*4);
						System.Array.Copy(RedGrassMeshComputer.s_Output.Indices, indices, cnt*6);
						
						opg.billboard.mVerts.Add(verts);
						opg.billboard.mNorms.Add(norms);
						opg.billboard.mUVs.Add(uvs);
						opg.billboard.mUV2s.Add(uv2s);
						opg.billboard.mColors32.Add(colors32);
						opg.billboard.mIndices.Add(indices);
						
						billboardCnt = billboardCnt -  mEvni.MeshQuadMaxCount;
						offsetCnt += mEvni.MeshQuadMaxCount;
					}
					
					// Get Tri-Angel output struct
					int triCnt = RedGrassMeshComputer.s_Output.TriquadCount;
					offsetCnt = RedGrassMeshComputer.s_Output.BillboardCount;
					
					while (triCnt > 0)
					{
						int cnt = Mathf.Min(triCnt, mEvni.MeshQuadMaxCount);
						
						Vector3[] verts = new Vector3 [cnt*4];
						Vector3[] norms = new Vector3 [cnt*4];
						Vector2[] uvs = new Vector2 [cnt*4];
						Vector2[] uv2s = new Vector2 [cnt*4];
						Color32[] colors32 = new Color32 [cnt*4];
						int[] indices = new int [cnt*6];
						
						System.Array.Copy(RedGrassMeshComputer.s_Output.Verts, offsetCnt*4,  verts, 0, cnt*4);
						System.Array.Copy(RedGrassMeshComputer.s_Output.Norms, offsetCnt*4, norms, 0, cnt*4);
						System.Array.Copy(RedGrassMeshComputer.s_Output.UVs, offsetCnt*4, uvs, 0, cnt*4);
						System.Array.Copy(RedGrassMeshComputer.s_Output.UV2s, offsetCnt*4, uv2s, 0, cnt*4);
						System.Array.Copy(RedGrassMeshComputer.s_Output.Color32s, offsetCnt*4, colors32, 0, cnt*4);
						System.Array.Copy(RedGrassMeshComputer.s_Output.Indices, indices, cnt*6);
						
						opg.tri.mVerts.Add(verts);
						opg.tri.mNorms.Add(norms);
						opg.tri.mUVs.Add(uvs);
						opg.tri.mUV2s.Add(uv2s);
						opg.tri.mColors32.Add(colors32);
						opg.tri.mIndices.Add(indices);
						
						triCnt = triCnt -  mEvni.MeshQuadMaxCount;
						offsetCnt += mEvni.MeshQuadMaxCount;
					}
					
					opr.groups.Add(opg);
					opr.oldGos = region.oldGos;
				}
				
				lock (mOutputs)
				{
					mOutputs.Add(opr);
				}
			}

		}

		public GameObject ComputeNow (RGLODQuadTreeNode node, RGDataSource data, float density)
		{
            try
            {
                List<RGChunk> chunks = node.GetChunks(data);

                if (chunks == null)
                    return null;

                foreach (RGChunk ck in chunks)
                {
                    foreach (KeyValuePair<int, RedGrassInstance> kvp in ck.grasses)
                    {
                        if (kvp.Value.Layer == 0)
                            _grasses.Add(kvp.Value);
                        else
                            _triGrasses.Add(kvp.Value);

                        if (ck.HGrass.ContainsKey(kvp.Key))
                        {
                            foreach (RedGrassInstance rgi in ck.HGrass[kvp.Key])
                            {
                                if (rgi.Layer == 0)
                                    _grasses.Add(rgi);
                                else
                                    _triGrasses.Add(rgi);
                            }
                        }
                    }
                }

                RedGrassMeshComputer.ComputeMesh(_grasses, _triGrasses, density);
                _grasses.Clear();
                _triGrasses.Clear();

                // Get Billboard output struct
                OutputGroup opg = new OutputGroup();

                int billboardCnt = RedGrassMeshComputer.s_Output.BillboardCount;
                int offsetCnt = 0;

                while (billboardCnt > 0)
                {
                    int cnt = Mathf.Min(billboardCnt, mEvni.MeshQuadMaxCount);
                    Vector3[] verts = new Vector3[cnt * 4];
                    Vector3[] norms = new Vector3[cnt * 4];
                    Vector2[] uvs = new Vector2[cnt * 4];
                    Vector2[] uv2s = new Vector2[cnt * 4];
                    Color32[] colors32 = new Color32[cnt * 4];
                    int[] indices = new int[cnt * 6];

                    System.Array.Copy(RedGrassMeshComputer.s_Output.Verts, offsetCnt * 4, verts, 0, cnt * 4);
                    System.Array.Copy(RedGrassMeshComputer.s_Output.Norms, offsetCnt * 4, norms, 0, cnt * 4);
                    System.Array.Copy(RedGrassMeshComputer.s_Output.UVs, offsetCnt * 4, uvs, 0, cnt * 4);
                    System.Array.Copy(RedGrassMeshComputer.s_Output.UV2s, offsetCnt * 4, uv2s, 0, cnt * 4);
                    System.Array.Copy(RedGrassMeshComputer.s_Output.Color32s, offsetCnt * 4, colors32, 0, cnt * 4);
                    System.Array.Copy(RedGrassMeshComputer.s_Output.Indices, indices, cnt * 6);

                    opg.billboard.mVerts.Add(verts);
                    opg.billboard.mNorms.Add(norms);
                    opg.billboard.mUVs.Add(uvs);
                    opg.billboard.mUV2s.Add(uv2s);
                    opg.billboard.mColors32.Add(colors32);
                    opg.billboard.mIndices.Add(indices);

                    billboardCnt = billboardCnt - mEvni.MeshQuadMaxCount;
                    offsetCnt += mEvni.MeshQuadMaxCount;
                }

                // Get Tri-Angel output struct
                int triCnt = RedGrassMeshComputer.s_Output.TriquadCount;
                offsetCnt = RedGrassMeshComputer.s_Output.BillboardCount;

                while (triCnt > 0)
                {
                    int cnt = Mathf.Min(triCnt, mEvni.MeshQuadMaxCount);

                    Vector3[] verts = new Vector3[cnt * 4];
                    Vector3[] norms = new Vector3[cnt * 4];
                    Vector2[] uvs = new Vector2[cnt * 4];
                    Vector2[] uv2s = new Vector2[cnt * 4];
                    Color32[] colors32 = new Color32[cnt * 4];
                    int[] indices = new int[cnt * 6];

                    System.Array.Copy(RedGrassMeshComputer.s_Output.Verts, offsetCnt * 4, verts, 0, cnt * 4);
                    System.Array.Copy(RedGrassMeshComputer.s_Output.Norms, offsetCnt * 4, norms, 0, cnt * 4);
                    System.Array.Copy(RedGrassMeshComputer.s_Output.UVs, offsetCnt * 4, uvs, 0, cnt * 4);
                    System.Array.Copy(RedGrassMeshComputer.s_Output.UV2s, offsetCnt * 4, uv2s, 0, cnt * 4);
                    System.Array.Copy(RedGrassMeshComputer.s_Output.Color32s, offsetCnt * 4, colors32, 0, cnt * 4);
                    System.Array.Copy(RedGrassMeshComputer.s_Output.Indices, indices, cnt * 6);

                    opg.tri.mVerts.Add(verts);
                    opg.tri.mNorms.Add(norms);
                    opg.tri.mUVs.Add(uvs);
                    opg.tri.mUV2s.Add(uv2s);
                    opg.tri.mColors32.Add(colors32);
                    opg.tri.mIndices.Add(indices);

                    triCnt = triCnt - mEvni.MeshQuadMaxCount;
                    offsetCnt += mEvni.MeshQuadMaxCount;
                }

                string desc = "Grass Batch [" + node.xIndex.ToString() + ", " + node.zIndex.ToString() + "] _ LOD [" + node.LOD + "]";
                GameObject go = CreateBatchGos(desc, opg.billboard, opg.tri);
                node.gameObject = go;

                if (go != null)
                {
                    go.transform.parent = transform;
                    go.transform.localPosition = Vector3.zero;

                    if (go.activeSelf != node.visible)
                        go.SetActive(node.visible);

                }

                return go;
            }
            catch
            {
                return null;
            }
		}


		#endregion

		#region THREAD_VALS & FUNC

		public class OutputStruct
		{
			public List<Vector3[]> mVerts;
			public List<Vector3[]> mNorms;
			public List<Vector2[]> mUVs;
			public List<Vector2[]> mUV2s;
			public List<Color32[]> mColors32;
			public List<int[]> mIndices;
			
			public OutputStruct ()
			{
				mVerts = new List<Vector3[]>();
				mNorms = new List<Vector3[]>();
				mUVs = new List<Vector2[]>();
				mUV2s = new List<Vector2[]>();
				mColors32 = new List<Color32[]>();
				mIndices = new List<int[]>();
			}
			
			public void Clear()
			{
				mVerts.Clear();
				mNorms.Clear();
				mUVs.Clear();
				mUV2s.Clear();
				mColors32.Clear();
				mIndices.Clear();
			}
			
			public bool Empity ()
			{
				return mVerts.Count == 0;
			}
			
			public int GetCount ()
			{
				return  mVerts.Count;
			}
		}

		// Out put for Behaviour 
		public class OutputGroup
		{
			public OutputStruct  billboard;
			public OutputStruct  tri;
			
			public RGLODQuadTreeNode node;
			
			public OutputGroup()
			{
				billboard = new OutputStruct();
				tri 	   = new OutputStruct();
			}
			
		}


		public class OutpuRegion
		{
			public List<OutputGroup> groups;
			public List<GameObject> oldGos;

			public OutpuRegion()
			{
				groups = new List<OutputGroup>();
			}
		}


		Thread mThread = null;
		bool mRunning = true;
		bool mActive = false;

		/// <summary>
		/// The thread is start?
		/// </summary>
		bool mStart = false;

		/// <summary>
		/// Outputs for inneral create mesh 
		/// </summary>
		List<OutpuRegion> mOutputs = new List<OutpuRegion>();

        public object procLockObj = new object(); 

		void Process ()
		{
            try
            {
                while (mRunning)
                {
                    if (!mActive)
                    {
                        Thread.Sleep(5);
                        continue;
                    }
                    lock (procLockObj)
                    {
                        ProcessReqs(mReqs);
                    }
                    mStart = false;
                    Thread.Sleep(5);
                }
            }
            catch
            {
                StopProcess();
                mThread = new Thread(new ThreadStart(Process));
                mThread.Start();
            }

		}

		#endregion


		#region UNITY_INNER_FUNC
		
		void Awake ()
		{
			mReqs = new Queue<RGRegion>();
			mOutputs = new List<OutpuRegion>();
			RedGrassMeshComputer.Init();
		}
		
		// Use this for initialization
		void Start () 
		{
			// initializate the thread and start
			mThread = new Thread(new ThreadStart(Process));
			mThread.Start();
		}
		
		void OnDestroy()
		{
			mRunning = false;
		}


		void Update () 
		{

			if (mActive && !mStart)
			{
				if (!mIsGenerating)
					StartCoroutine("Generate");
			}


		}

		bool mIsGenerating = false;
		const int c_Interval = 8;
		IEnumerator Generate ()
		{
			mIsGenerating = true;
			int cnt = 0;
			foreach (OutpuRegion opr in mOutputs)
			{
				List<OutputGroup> group = opr.groups;
				for (int j = 0; j < group.Count; j++)
				{
					OutputGroup opg = group[j];

					if (opg.node.IsOutOfDate())
						continue;

					opg.node.Dirty(mData, false);

					RGLODQuadTreeNode node = opg.node;
					string desc = "Grass Batch [" + node.xIndex.ToString() + ", " + node.zIndex.ToString() +"] _ LOD [" + node.LOD + "]";
					GameObject go = CreateBatchGos(desc, opg.billboard, opg.tri);
					node.gameObject = go;

					if (go != null)
					{
						go.transform.parent = transform;
						go.transform.localPosition = Vector3.zero; 

						if (go.activeSelf != node.visible)
							go.SetActive(node.visible);

						if (cnt % c_Interval == 0)
							yield return 0;

						cnt++;
					}

				}

				ClearMeshes(opr.oldGos);
			}

			if (onReqsFinished != null)
				onReqsFinished();

			mOutputs.Clear();
			mIsGenerating = false;
			mActive = false;

		}




		GameObject CreateBatchGos (string desc, OutputStruct billboard, OutputStruct tri)
		{
			GameObject batchGo = null;
			// Billboad Grass mesh
			for (int i =0; i < billboard.GetCount(); i ++)
			{
				GameObject go = new GameObject();
				if (batchGo != null)
				{
					go.transform.parent = batchGo.transform;
					go.name = "Billboard";
				}
				else
				{
					batchGo = go;
					go.name = desc + " Billboard";
				}
				
				MeshFilter mf = go.AddComponent<MeshFilter>();
				go.AddComponent<MeshRenderer>().material = grassMat;
				
				mf.mesh.vertices = billboard.mVerts[i];
				mf.mesh.triangles = billboard.mIndices[i];
				mf.mesh.normals = billboard.mNorms[i];
				mf.mesh.uv = billboard.mUVs[i];
				mf.mesh.uv2 = billboard.mUV2s[i];
				mf.mesh.colors32 = billboard.mColors32[i];
			}


			// Tri-angle Grass mesh
			for (int i =0; i < tri.GetCount(); i ++)
			{
				GameObject go = new GameObject();
				if (batchGo != null)
				{
					go.transform.parent = batchGo.transform;
					go.name = "Tri-angle";
				}
				else
				{
					batchGo = go;
					go.name = desc + " Tri-angle";
				}
				
				MeshFilter mf = go.AddComponent<MeshFilter>();
				//mf.renderer.material = GrassMgr.Instance.m_GrassMat;
				go.AddComponent<MeshRenderer>().material = triMat;
				
				mf.mesh.vertices = tri.mVerts[i];
				mf.mesh.triangles = tri.mIndices[i];
				mf.mesh.normals = tri.mNorms[i];
				mf.mesh.uv = tri.mUVs[i];
				mf.mesh.uv2 = tri.mUV2s[i];
				mf.mesh.colors32 = tri.mColors32[i];
			}

			return batchGo;
		}
		
		#endregion
	}
}
