#define DRAW_TAIL_NODE_IN_EDITOR

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Pathea.Maths;

namespace RedGrass
{

	public partial class RGScene : MonoBehaviour 
	{
		public RGCreator meshCreator;

		// Renderer chunk center
		INTVECTOR2 mRenderCenter;
		RGLODQuadTree mTree;
		Dictionary<int, RGLODQuadTreeNode> mRootNodes = null;
		Dictionary<int, RGLODQuadTreeNode> mTargetNodes = null;

		#if DRAW_TAIL_NODE_IN_EDITOR
		Dictionary<int, Dictionary<int, RGLODQuadTreeNode>>   mTailNodes;
		#endif

		private int[] mXLodIndex = null;
		private int[] mZLodIndex = null;

		bool mUpdateMesh = false;

		bool mInitIndex = false;


		/// <summary>
		/// Init Renderer part of Vars and Functions 
		/// </summary>
		void Init_Render ()
		{
			mRenderCenter = new INTVECTOR2(-10000, -10000);
			mTree = new RGLODQuadTree(evniAsset);
			mRootNodes = new Dictionary<int, RGLODQuadTreeNode>();
			mTargetNodes = new Dictionary<int, RGLODQuadTreeNode>();

			mTailNodes = new Dictionary<int, Dictionary<int, RGLODQuadTreeNode>>();

			int maxlod = evniAsset.MaxLOD;
			mXLodIndex = new int[maxlod + 1];
			mZLodIndex = new int[maxlod + 1];
			mInitIndex = true;

			if (meshCreator != null)
				meshCreator.Init(evniAsset, data);
		}


		void  UpdateLodIndex()
		{
			int maxlod = evniAsset.MaxLOD;

			if (mXLodIndex == null || mXLodIndex.Length != maxlod + 1)
			{
				mXLodIndex = new int[maxlod + 1];
				mZLodIndex = new int[maxlod + 1];
				mInitIndex = true;

			}
		}

		public void UpdateMeshImmediately()
		{
            lock(meshCreator.procLockObj)
            {
                meshCreator.StopProcess();

                mRenderCenter = mCenter;

                List<GameObject> gos = new List<GameObject>();
                Dictionary<int, RGLODQuadTreeNode> roots = mTree.GetRootNodes();

                foreach (RGLODQuadTreeNode node in roots.Values)
                {
                    if (node != null)
                    {
                        mTree.TraversalNode(node, item0 =>
                                            {
                                                item0.UpdateTimeStamp();
                                                if (item0.gameObject != null)
                                                {
                                                    gos.Add(item0.gameObject);
                                                    item0.gameObject = null;
                                                }
                                            });
                    }
                }

                meshCreator.ClearMeshes(gos);


                // Update Tree
                int maxLod = evniAsset.MaxLOD;

                UpdateLodIndex();

                for (int i = maxLod; i >= 0; i--)
                {
                    int _x = 0;
                    int _z = 0;

                    if (mRenderCenter.x >= 0)
                        _x = mRenderCenter.x / (1 << i) * (1 << i);
                    else
                        _x = mRenderCenter.x / (1 << i) * (1 << i) - (1 << i);

                    if (mRenderCenter.y >= 0)
                        _z = mRenderCenter.y / (1 << i) * (1 << i);
                    else
                        _z = mRenderCenter.y / (1 << i) * (1 << i) - (1 << i);

                    mXLodIndex[i] = _x;
                    mZLodIndex[i] = _z;
                    mTree.UpdateTree(i, mXLodIndex[i], mZLodIndex[i]);
                }

                mTargetNodes = mTree.GetRootNodes();
                mRootNodes = mTargetNodes;

                foreach (KeyValuePair<int, RGLODQuadTreeNode> kvp in mTargetNodes)
                {
                    List<RGLODQuadTreeNode> new_list = new List<RGLODQuadTreeNode>();
                    List<GameObject> old_list = new List<GameObject>();

                    TraverseNode(kvp.Value, old_list, new_list);

                    RGRegion region = new RGRegion();
                    region.nodes = new_list;
                    region.oldGos = old_list;
                    meshCreator.AddReqs(region);
                }

                meshCreator.ProcessReqsImmediatly();
            }
		}


		/// <summary>
		/// Updates the rendering of LOD Meshes, and Add the requests to
		/// the creator.
		/// </summary>
		public void UpdateMesh ()
		{
			if (meshCreator.isActive())
				return;

			if (!mUpdateMesh)
				return;


			mUpdateMesh = false;

			mRenderCenter = mCenter;

			// Update Tree
			int maxLod = evniAsset.MaxLOD;

			if (!mInitIndex)
			{
				for (int i = maxLod; i >= 0; i--)
				{
					int _x = 0;
					int _z = 0;
					
					if (mRenderCenter.x >= 0)
						_x = mRenderCenter.x / (1 << i) * (1 << i);
					else
						_x = mRenderCenter.x / (1 << i) * (1 << i) - (1 << i);
					
					if (mRenderCenter.y >= 0)
						_z = mRenderCenter.y / (1 << i) * (1 << i);
					else
						_z = mRenderCenter.y / (1 << i) * (1 << i) - (1 << i);

					if (_x != mXLodIndex[i] || _z != mZLodIndex[i])
					{
						mXLodIndex[i] = _x;
						mZLodIndex[i] = _z;
						mTree.UpdateTree(i, mXLodIndex[i], mZLodIndex[i]);
					}
				}
			}
			else
			{
				for (int i = maxLod; i >= 0; i--)
				{
					int _x = 0;
					int _z = 0;
					
					if (mRenderCenter.x >= 0)
						_x = mRenderCenter.x / (1 << i) * (1 << i);
					else
						_x = mRenderCenter.x / (1 << i) * (1 << i) - (1 << i);
					
					if (mRenderCenter.y >= 0)
						_z = mRenderCenter.y / (1 << i) * (1 << i);
					else
						_z = mRenderCenter.y / (1 << i) * (1 << i) - (1 << i);

					mXLodIndex[i] = _x;
					mZLodIndex[i] = _z;
					mTree.UpdateTree(i, mXLodIndex[i], mZLodIndex[i]);
				}

				mInitIndex = false;
			}
		

			// 
			mTargetNodes = mTree.GetRootNodes();

			DestroyExtraGo();

			mRootNodes = mTargetNodes;

			int count = 0;
			foreach (KeyValuePair<int, RGLODQuadTreeNode> kvp in mTargetNodes)
			{
				List<RGLODQuadTreeNode> new_list = new List<RGLODQuadTreeNode>();
				List<GameObject> old_list = new List<GameObject>();

				TraverseNode(kvp.Value, old_list, new_list);

				if (new_list.Count != 0)
					count ++;


				RGRegion region = new RGRegion();
				region.nodes = new_list;
				region.oldGos = old_list;
				meshCreator.AddReqs(region);
			}

			meshCreator.Active(null);

		}

		RGLODQuadTree.NodeDel _delTrue  = item0 =>{		if (item0.gameObject != null && item0.gameObject.activeSelf != true ) item0.gameObject.SetActive (true );	};
		RGLODQuadTree.NodeDel _delFalse = item0 =>{		if (item0.gameObject != null && item0.gameObject.activeSelf != false) item0.gameObject.SetActive (false);	};
		void CullObject()
		{
			#region Culling_Object
			// Culling Object
			Plane[] camraPlanes = GeometryUtility.CalculateFrustumPlanes(Camera.main);
			foreach (KeyValuePair<int, RGLODQuadTreeNode> kvp in mRootNodes)
			{
				INTVECTOR3 pos = Utils.IndexToPos(kvp.Key);
				pos.x = ( pos.x << evniAsset.SHIFT);
				pos.z = ( pos.z << evniAsset.SHIFT);
				int size =  evniAsset.CHUNKSIZE * (1 << kvp.Value.LOD);
				int halfSize = size / 2;
				Bounds bound = new Bounds(new Vector3(pos.x + halfSize, 500, pos.z + halfSize), new Vector3(size, 1100, size) );
				if (GeometryUtility.TestPlanesAABB(camraPlanes, bound)){
					kvp.Value.visible = true;
					mTree.TraversalNode(kvp.Value, _delTrue);
				}else{
					kvp.Value.visible = false;
					mTree.TraversalNode(kvp.Value, _delFalse);
				}
            }
			#endregion
		}

		/// <summary>
		/// Draws the area in Unity Editor.
		/// </summary>
		void  DrawAreaInEditor ()
		{
			if (Application.isEditor)
			{
				mTailNodes = mTree.GetLODTailNode();
				
				for (int i = 0; i <= evniAsset.MaxLOD; ++i)
				{
					if (!mTailNodes.ContainsKey(i))
						continue;
					foreach (RGLODQuadTreeNode node in mTailNodes[i].Values)
					{
						Vector3 wPos = new Vector3(node.xIndex * evniAsset.CHUNKSIZE, 0, evniAsset.CHUNKSIZE * node.zIndex);
						Vector3 _va = wPos +  (Vector3.right + Vector3.forward) * 4F;
						Vector3 _vb = wPos + Vector3.right * (evniAsset.CHUNKSIZE * (1<<node.LOD) - 4F) + Vector3.forward * 4F;
						Vector3 _vc = wPos + (Vector3.right + Vector3.forward) * (evniAsset.CHUNKSIZE * (1<<node.LOD) - 4F);
						Vector3 _vd = wPos + Vector3.forward * (evniAsset.CHUNKSIZE * (1<<node.LOD) - 4F) + Vector3.right * 4F;
						Color color = new Color(1.0f, node.LOD*0.2f, node.LOD*0.4f ,1.0f);
						
						Debug.DrawLine(_va, _vb, color);
						Debug.DrawLine(_vb, _vc, color);
						Debug.DrawLine(_vc, _vd, color);
						Debug.DrawLine(_vd, _va, color);
					}
				}
			}
		}


		/// <summary>
		/// Destroies the extra Gameobject, such as gamobjects out of arrange.
		/// </summary>
		void DestroyExtraGo ()
		{
			// Delete the GameObject is not in target list
			List<int> delete_list = new List<int>();

			foreach (KeyValuePair<int, RGLODQuadTreeNode> kvp in mRootNodes)
			{
				if (!mTargetNodes.ContainsKey(kvp.Key))
				{
					List<GameObject> go_list = new List<GameObject>();
					TraverseNode(kvp.Value, go_list);

					meshCreator.ClearMeshes(go_list); 

					delete_list.Add(kvp.Key);
				}
			}

			foreach(int id in delete_list)
				mRootNodes.Remove(id);
		}


		/// <summary>
		/// Traverses the node, and get the vaild gameobject list
		/// </summary>
		void TraverseNode (RGLODQuadTreeNode node, List<GameObject> go_list)
		{
			if (node == null)
				return;

			mTree.TraversalNode(node, item0 =>
			                               {
												item0.UpdateTimeStamp();
												if (item0.gameObject != null)
													go_list.Add(item0.gameObject);
											});	
		}

		/// <summary>
		/// Traverses the node, and get the gamobject that the node is not tail,
		/// and add the tail node to new list
		/// </summary>
		private void TraverseNode (RGLODQuadTreeNode node, List<GameObject> old_list,  List<RGLODQuadTreeNode> new_list)
		{
			if (node == null)
				return;

			mTree.TraversalNode(node, item0 =>
			                               {
												if (item0.isTail)
												{
													if (item0.gameObject == null)
														new_list.Add(item0);
												}
												else
												{
													item0.UpdateTimeStamp();
													if (item0.gameObject != null)
														old_list.Add(item0.gameObject);
												}
											});
		}



		void RefreshDirtyReqs ()
		{
			if (!meshCreator.isActive() )
			{
                lock(meshCreator.procLockObj)
                {
                    if (RGChunk.AnyDirty)
                    {
                        Dictionary<int, RGLODQuadTreeNode>.ValueCollection vals = mRootNodes.Values;
                        foreach (RGLODQuadTreeNode v in vals)
                        {
                            if (v != null)
                            {
                                mTree.TraversalNode4Req(v, this);
                            }
                        }
                        RGChunk.AnyDirty = false;
                    }
                }
			}
		}

		public void OnTraversalNode(RGLODQuadTreeNode node)
		{
			if (node.isTail && node.IsDirty(data))
			{
				// Destroy old
				meshCreator.ClearMesh(node.gameObject);
				
                meshCreator.ComputeNow(node, data, evniAsset.LODDensities[node.LOD] * evniAsset.Density);
                
				node.Dirty(data, false);
            }
        }
    }
    
}
