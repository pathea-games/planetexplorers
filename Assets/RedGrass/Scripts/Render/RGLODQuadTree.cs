using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace RedGrass
{
	/// <summary>
	/// Render LOD quad tree node
	/// </summary>
	public class RGLODQuadTreeNode
	{
		public int xIndex;
		public int zIndex;
		
		public int LOD;

		public RGLODQuadTreeNode node1;
		public RGLODQuadTreeNode node2;
		public RGLODQuadTreeNode node3;
		public RGLODQuadTreeNode node4;

		public RGLODQuadTreeNode parent;

		public bool isTail;
		public bool visible;

		int mTimeStamp = 0;
		int mTimeStampOutofdate = 0;

		public void UpdateTimeStamp () 	{ ++mTimeStampOutofdate; }
		public void SyncTimeStamp ()	{ mTimeStamp = mTimeStampOutofdate;}
		public bool IsOutOfDate ()		{ return mTimeStamp != mTimeStampOutofdate; }

		public GameObject gameObject;

		public List<GameObject> oldGos = new List<GameObject>();


		public RGLODQuadTreeNode(int _x, int _z, int _lod)
		{
			xIndex = _x;
			zIndex = _z;
			LOD  = _lod;
			visible = true;
		}

		public List<RGChunk> GetChunks (RGDataSource data)
		{
			List<RGChunk> gc_list = new List<RGChunk>();

			int expand = 1<<LOD;


			for (int x = xIndex; x < xIndex + expand; x++)
			{
				for (int z = zIndex; z < zIndex + expand; z++)
				{
					RGChunk gc = data.Node(x, z);

					if (gc == null)
						continue;
					gc_list.Add(gc);
				}
			}

			return gc_list;
		}

		public bool HasChild()
		{
			return (node1 != null);
		}

		public bool IsDirty (RGDataSource data)
		{
			int expand = 1<<LOD;
			for (int x = xIndex; x < xIndex + expand; x++)
			{
				for (int z = zIndex; z < zIndex + expand; z++)
				{
					RGChunk gc = data.Node(x, z);
					if (gc != null && gc.Dirty)
						return true;
				}
			}
			return false;
		}

		public void Dirty(RGDataSource data, bool dirty)
		{
			int expand = 1<<LOD;
			for (int x = xIndex; x < xIndex + expand; x++)
			{
				for (int z = zIndex; z < zIndex + expand; z++)
				{
					RGChunk gc = data.Node(x, z);
					if (gc != null )
						gc.Dirty = dirty;
				}
			}
		}

	}

	/// <summary>
	/// Render LOD Quad Tree
	/// </summary>
	public class RGLODQuadTree
	{
		private List<Dictionary<int, RGLODQuadTreeNode>> mLODNodes = null;
		private Dictionary<int, Dictionary<int, RGLODQuadTreeNode>> mLODTailNodes = null;

		public delegate void NodeDel(RGLODQuadTreeNode n);

		private EvniAsset mEvni;

		public RGLODQuadTree (EvniAsset evni)
		{
			mLODNodes = new List<Dictionary<int, RGLODQuadTreeNode>>();
			mLODTailNodes = new Dictionary<int, Dictionary<int, RGLODQuadTreeNode>>();
			mEvni = evni;

			int max_lod = mEvni.MaxLOD;

			mLODNodes.Clear();
			for(int i = 0; i < max_lod + 1; i++)
				mLODNodes.Add(new Dictionary<int, RGLODQuadTreeNode>());
		}

		public  Dictionary<int ,Dictionary<int, RGLODQuadTreeNode>>  GetLODTailNode ()
		{
//			mLODTailNodes.Clear();
//			
//			foreach (KeyValuePair<int, RGLODQuadTreeNode> kvp in mLODNodes[mLODNodes.Count - 1])
//			{
//				TraversalTailNode(kvp.Value);
//			}
//			
//			return mLODTailNodes;

			Dictionary<int ,Dictionary<int, RGLODQuadTreeNode>> tail_nodes = new Dictionary<int, Dictionary<int, RGLODQuadTreeNode>>();
			foreach (KeyValuePair<int, RGLODQuadTreeNode> kvp in mLODNodes[mLODNodes.Count - 1])
			{
				TraversalTailNode(kvp.Value, tail_nodes);
			}

			return tail_nodes;
		}


		public  Dictionary<int, RGLODQuadTreeNode> GetRootNodes()
		{
			return mLODNodes[mLODNodes.Count - 1];
		}


		/// <summary>
		/// Traversals the tail node. And add it to Tail Nodes List
		/// </summary>
		/// <param name="node">Node.</param>
		private void TraversalTailNode (RGLODQuadTreeNode node)
		{
			if (node == null)
				return;

			if (node.isTail)
			{
				if (mLODTailNodes.ContainsKey(node.LOD))
				{
					int index = Utils.PosToIndex(node.xIndex, node.zIndex);
					mLODTailNodes[node.LOD].Add(index, node);
				}
				else
				{
					int index = Utils.PosToIndex(node.xIndex, node.zIndex);
					mLODTailNodes.Add(node.LOD, new Dictionary<int, RGLODQuadTreeNode>());
					mLODTailNodes[node.LOD].Add(index, node); 
				}
				return;
			}

			TraversalTailNode (node.node1);
			TraversalTailNode (node.node2);
			TraversalTailNode (node.node3);
			TraversalTailNode (node.node4);
		}

		private void TraversalTailNode (RGLODQuadTreeNode node, Dictionary<int ,Dictionary<int, RGLODQuadTreeNode>> tailNodes)
		{
			if (node == null)
				return;
			
			if (node.isTail)
			{
				if (tailNodes.ContainsKey(node.LOD))
				{
					int index = Utils.PosToIndex(node.xIndex, node.zIndex);
					tailNodes[node.LOD].Add(index, node);
				}
				else
				{
					int index = Utils.PosToIndex(node.xIndex, node.zIndex);
					tailNodes.Add(node.LOD, new Dictionary<int, RGLODQuadTreeNode>());
					tailNodes[node.LOD].Add(index, node); 
				}
				return;
			}

			TraversalTailNode (node.node1, tailNodes);
			TraversalTailNode (node.node2, tailNodes);
			TraversalTailNode (node.node3, tailNodes);
			TraversalTailNode (node.node4, tailNodes);
		}


		/// <summary>
		/// Traversals all the node of the tree, and do something
		/// </summary>
		/// <param name="node">Node.</param>
		/// <param name="del"> the call_back</param>
		public void TraversalNode (RGLODQuadTreeNode node,  NodeDel del)
		{
			if(node.node1 != null)	TraversalNode (node.node1, del);
			if(node.node2 != null)	TraversalNode (node.node2, del);
			if(node.node3 != null)	TraversalNode (node.node3, del);
			if(node.node4 != null)	TraversalNode (node.node4, del);
			del(node);
		}
		public void TraversalNode4Req (RGLODQuadTreeNode node, RGScene scn)
		{
			if(node.node1 != null)	TraversalNode4Req (node.node1, scn);
			if(node.node2 != null)	TraversalNode4Req (node.node2, scn);
			if(node.node3 != null)	TraversalNode4Req (node.node3, scn);
			if(node.node4 != null)	TraversalNode4Req (node.node4, scn);
			scn.OnTraversalNode (node);
		}
		/// <summary>
		/// Updates the quad tree whith special center
		/// </summary>
		public void UpdateTree (int lod, int xIndex, int zIndex)
		{
			int max_lod = mEvni.MaxLOD;

			if (mLODNodes.Count != max_lod+ 1)
			{
				mLODNodes.Clear();
				for(int i = 0; i < max_lod + 1; i++)
					mLODNodes.Add(new Dictionary<int, RGLODQuadTreeNode>());
			}



			if (lod == max_lod)
			{
				int unitSize = 1<<max_lod;
				int expand = mEvni.LODExpandNum[lod] * unitSize;
				Dictionary<int, RGLODQuadTreeNode> tempRootNodes = new Dictionary<int, RGLODQuadTreeNode>();
				
				for (int x = xIndex - expand; x <= xIndex + expand; x += unitSize)
				{
					for ( int z = zIndex - expand; z <= zIndex + expand; z += unitSize)
					{
						int index = Utils.PosToIndex(x, z);
						if (!mLODNodes[lod].ContainsKey(index))
						{
							RGLODQuadTreeNode node = new RGLODQuadTreeNode(x, z, mEvni.MaxLOD);
							node.isTail = true;
							node.visible = true;
							tempRootNodes.Add(index, node);
						}
						else
						{
							mLODNodes[lod][index].isTail = true;
							mLODNodes[lod][index].visible = true;
							tempRootNodes.Add(index, mLODNodes[lod][index]);
						}
					}
				}

				mLODNodes[lod] = tempRootNodes;

			}
			else
			{
				int unitSize = 1<<lod;
				int expand = mEvni.LODExpandNum[lod] * unitSize;

				int hLod = lod + 1;
				int hUnitSize = 1 << hLod;
				
				Dictionary<int, RGLODQuadTreeNode> tempNodes = new Dictionary<int, RGLODQuadTreeNode>();
				
				for (int x = xIndex - expand; x <= xIndex + expand; x += unitSize)
				{
					
					for (int z = zIndex - expand; z <= zIndex + expand; z += unitSize)
					{
						int id = Utils.PosToIndex(x,z);
						if (mLODNodes[lod].ContainsKey(id))
						{
							RGLODQuadTreeNode parent = mLODNodes[lod][id].parent;
							int key = Utils.PosToIndex(parent.node1.xIndex, parent.node1.zIndex);
							if (!tempNodes.ContainsKey(key))
							{
								tempNodes.Add(Utils.PosToIndex(parent.node1.xIndex, parent.node1.zIndex), parent.node1);
								tempNodes.Add(Utils.PosToIndex(parent.node2.xIndex, parent.node2.zIndex), parent.node2);
								tempNodes.Add(Utils.PosToIndex(parent.node3.xIndex, parent.node3.zIndex), parent.node3);
								tempNodes.Add(Utils.PosToIndex(parent.node4.xIndex, parent.node4.zIndex), parent.node4);
							}
							
							parent.isTail = false;
							parent.node1.isTail = true;
							parent.node2.isTail = true;
							parent.node3.isTail = true;
							parent.node4.isTail = true;
						}
						else
						{
							//find the parent
							foreach(KeyValuePair<int, RGLODQuadTreeNode> kvp in mLODNodes[hLod])
							{
								if ( x >= kvp.Value.xIndex && x  < kvp.Value.xIndex + hUnitSize && z >= kvp.Value.zIndex && z < kvp.Value.zIndex + hUnitSize)
								{
									if (!kvp.Value.HasChild())
										CreateChildNode(kvp.Value);
									
									if (!tempNodes.ContainsKey(kvp.Key))
									{
										tempNodes.Add(Utils.PosToIndex(kvp.Value.node1.xIndex, kvp.Value.node1.zIndex), kvp.Value.node1);
										tempNodes.Add(Utils.PosToIndex(kvp.Value.node2.xIndex, kvp.Value.node2.zIndex), kvp.Value.node2);
										tempNodes.Add(Utils.PosToIndex(kvp.Value.node3.xIndex, kvp.Value.node3.zIndex), kvp.Value.node3);
										tempNodes.Add(Utils.PosToIndex(kvp.Value.node4.xIndex, kvp.Value.node4.zIndex), kvp.Value.node4);
									}
									
									kvp.Value.isTail = false;
									kvp.Value.node1.isTail = true;
									kvp.Value.node2.isTail = true;
									kvp.Value.node3.isTail = true;
									kvp.Value.node4.isTail = true;
									
									break;
								}
							}
						}
					}
				}

				foreach (KeyValuePair<int, RGLODQuadTreeNode> kvp in mLODNodes[lod])
				{
					if (!tempNodes.ContainsKey(kvp.Key))
					{	
						RGLODQuadTreeNode node = kvp.Value;
						
						RGLODQuadTreeNode parent = node.parent.parent;
						bool bSet = true;
						while (parent!= null)
						{
							if (parent.isTail)
								bSet = false;
							parent = parent.parent;
						}
						node.parent.isTail = bSet;
						
						node.isTail = false;
					}
				}
				
				mLODNodes[lod] = tempNodes;
			}
		}

		void CreateChildNode (RGLODQuadTreeNode node)
		{
			int unitSize = 1 << (node.LOD- 1);

			node.node1 = new RGLODQuadTreeNode(node.xIndex, node.zIndex, node.LOD - 1);
			node.node2 = new RGLODQuadTreeNode(node.xIndex, node.zIndex + unitSize, node.LOD - 1);
			node.node3 = new RGLODQuadTreeNode(node.xIndex + unitSize, node.zIndex, node.LOD - 1);
			node.node4 = new RGLODQuadTreeNode(node.xIndex + unitSize, node.zIndex + unitSize, node.LOD - 1);

			node.node1.parent = node;
			node.node2.parent = node;
			node.node3.parent = node;
			node.node4.parent = node;
		}
	}
}