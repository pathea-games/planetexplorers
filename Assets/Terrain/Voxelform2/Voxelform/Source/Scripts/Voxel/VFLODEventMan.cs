using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;
using System.Linq;
using System.IO;
using System.Threading;

public delegate void DelegateNodeVisible(IntVector4 nodePosLod);
public delegate void DelegateNodeInvisible(IntVector4 nodePosLod);
public delegate void DelegateNodeMeshCreated(IntVector4 nodePosLod);
public delegate void DelegateNodeMeshDestroy(IntVector4 nodePosLod);
public delegate void DelegateNodePhyxReady(IntVector4 nodePosLod);
public delegate void DelegateRootPosChange(IntVector4 prePos, IntVector4 postPos);

public partial class LODOctreeNode
{
    public event DelegateNodeVisible handlerVisible;
	public event DelegateNodeInvisible handlerInvisible;
    public event DelegateNodeMeshCreated handlerMeshCreated;
    public event DelegateNodeMeshDestroy handlerMeshDestroy;
    public event DelegateNodePhyxReady handlerPhyxReady;

	public void OnVisible()
	{
		if(handlerVisible != null)
		{
			lock(LODOctreeMan.self.nodeListVisible)
			{
				LODOctreeMan.self.nodeListVisible.Add(this);
				LODOctreeMan.self.nodePosListVisible.Add(PosLod);
			}
		}
	}
	public void OnInvisible()
	{
		if(handlerInvisible != null)
		{
			lock(LODOctreeMan.self.nodeListInvisible)
			{
				LODOctreeMan.self.nodeListInvisible.Add(this);
				LODOctreeMan.self.nodePosListInvisible.Add(PosLod);
			}
		}
		if(!_data[VFVoxelTerrain.self.IdxInLODNodeData].IsEmpty)
		{
			if(handlerMeshDestroy != null)
			{
				lock(LODOctreeMan.self.nodeListMeshDestroy)
				{
					LODOctreeMan.self.nodeListMeshDestroy.Add(this);
					LODOctreeMan.self.nodePosListMeshDestroy.Add(PosLod);
				}
			}
			if(LODOctreeMan.self.HandlerExistMeshDestroy)
			{
				lock(LODOctreeMan.self.posListMeshDestroy)
				{
					LODOctreeMan.self.posListMeshDestroy.Add(PosLod);
				}
			}
		}
	}
	public void HandleVisible(IntVector4 nodePosLod)
	{
		if(handlerVisible != null)
		{		
			try{handlerVisible(nodePosLod);  }
			catch(Exception e){Debug.LogError("[LODEVENT]Error in HandleVisible"+nodePosLod+e);}
		}
	}
	public void HandleInvisible(IntVector4 nodePosLod)
	{
		if(handlerInvisible != null)
		{	
			try{handlerInvisible(nodePosLod);}
			catch(Exception e){Debug.LogError("[LODEVENT]Error in HandleInvisible"+nodePosLod+e);}
		}	
	}
	public void HandleMeshDestroy(IntVector4 nodePosLod)
	{
		if(handlerMeshDestroy != null)
		{	
			try{handlerMeshDestroy(nodePosLod);}
			catch(Exception e){Debug.LogError("[LODEVENT]Error in HandleMeshDestroy"+nodePosLod+e);}
		}	
	}
	public void OnMeshCreated()
	{
		if(handlerMeshCreated != null)
		{
			try{handlerMeshCreated(PosLod);	}
			catch(Exception e){Debug.LogError("[LODEVENT]Error in handlerMeshCreated"+PosLod+e);}
		}
		if(LODOctreeMan.self.HandlerExistMeshCreated)
		{
			LODOctreeMan.self.DispatchNodeEventMeshCreated(PosLod);
		}
	}
	public void OnPhyxReady()
	{
		if(handlerPhyxReady != null)
		{
			try{handlerPhyxReady(PosLod);}
			catch(Exception e){Debug.LogError("[LODEVENT]Error in handlerPhyxReady"+PosLod+e);}
		}
		if(LODOctreeMan.self.HandlerExistMeshPhyxReady)
		{
			LODOctreeMan.self.DispatchNodeEventPhyxReady(PosLod);
		}
	}
	public void OnRootPreMove(int newPosx, int newPosy, int newPosz)
	{
		if(LODOctreeMan.self.HandlerExistRootPosChange)
		{
			lock(LODOctreeMan.self.posListRootPrePos)
			{
				LODOctreeMan.self.posListRootPrePos.Add(PosLod);
				LODOctreeMan.self.posListRootNewPos.Add(new IntVector4(newPosx, newPosy, newPosz, _lod));
			}
		}
	}
}

public partial class LODOctreeMan
{
	public List<LODOctreeNode> nodeListVisible = new List<LODOctreeNode>();
	public List<LODOctreeNode> nodeListInvisible = new List<LODOctreeNode>();
	public List<LODOctreeNode> nodeListMeshDestroy = new List<LODOctreeNode>();
    public List<IntVector4> nodePosListCollider = new List<IntVector4>();
    public List<IntVector4> nodePosListVisible = new List<IntVector4>();
	public List<IntVector4> nodePosListInvisible = new List<IntVector4>();
	public List<IntVector4> nodePosListMeshDestroy = new List<IntVector4>();
	public List<IntVector4> posListMeshDestroy = new List<IntVector4>();	// general event
	public List<IntVector4> posListRootPrePos = new List<IntVector4>();	// general event
	public List<IntVector4> posListRootNewPos = new List<IntVector4>();	// general event
    public event DelegateNodeMeshCreated handlerNodeMeshCreated;
	public event DelegateNodeMeshDestroy handlerNodeMeshDestroy;
    public event DelegateNodePhyxReady handlerNodePhyxReady;
	public event DelegateRootPosChange handlerRootPosChange;
	public bool HandlerExistMeshCreated{ 	get{return handlerNodeMeshCreated != null;	}}
	public bool HandlerExistMeshDestroy{ 	get{return handlerNodeMeshDestroy != null;	}}
	public bool HandlerExistMeshPhyxReady{	get{return handlerNodePhyxReady   != null;	}}
	public bool HandlerExistRootPosChange{	get{return handlerRootPosChange   != null;	}}

    public bool Contains(IntVector4 node)
    {
        return nodePosListCollider.Contains(node);
    }
    
    public bool Contains(IntVector2 node)
    {
        return nodePosListCollider.Find(ret => ret.x == node.x && ret.z == node.y) != null;
    }

    public IntVector4[] GetNodes(IntVector2 node)
    {
        return nodePosListCollider.FindAll(ret => ret.x == node.x && ret.z == node.y).ToArray();
    }
	
	public void DispatchNodeEventMeshCreated(IntVector4 nodePosLod)
	{
		try{handlerNodeMeshCreated(nodePosLod);}
		catch(Exception e){Debug.LogError("[LODEVENT]:Error in handlerNodeMeshCreated"+nodePosLod+e);}
	}
	public void DispatchNodeEventPhyxReady(IntVector4 nodePosLod)
	{
        if (!nodePosListCollider.Contains(nodePosLod)) nodePosListCollider.Add(nodePosLod);
		try{handlerNodePhyxReady(nodePosLod);}
		catch(Exception e){Debug.LogError("[LODEVENT]:Error in handlerNodePhyxReady"+nodePosLod+e);}
	}
	private void DispatchNodeEvents() // for those events trigered in lod thread
	{
		// general event
		int nNodes = posListMeshDestroy.Count;
		if(nNodes> 0)
		{
			lock(posListMeshDestroy)
			{
				nNodes = posListMeshDestroy.Count;
				for(int i = 0; i < nNodes; i++)
				{
                    if (nodePosListCollider.Contains(posListMeshDestroy[i])) nodePosListCollider.Remove(posListMeshDestroy[i]);
					try{handlerNodeMeshDestroy(posListMeshDestroy[i]);}
					catch(Exception e){Debug.LogError("[LODEVENT]:Error in handlerNodeMeshDestroy"+posListMeshDestroy[i]+e);}
				}
				posListMeshDestroy.Clear();
			}
		}
		nNodes = posListRootPrePos.Count;
		if(nNodes> 0)
		{
			lock(posListRootPrePos)
			{
				nNodes = posListRootPrePos.Count;
				for(int i = 0; i < nNodes; i++)
				{
					try{handlerRootPosChange(posListRootPrePos[i], posListRootNewPos[i]);}
					catch(Exception e){Debug.LogError("[LODEVENT]:Error in handlerRootPosChange"+posListRootPrePos[i]+posListRootNewPos[i]+e);}
				}
				posListRootPrePos.Clear();
				posListRootNewPos.Clear();
			}
		}		
		// node's event
		nNodes = nodeListInvisible.Count;
		if(nNodes> 0)
		{
			lock(nodeListInvisible)
			{
				nNodes = nodeListInvisible.Count;
				for(int i = 0; i < nNodes; i++)
				{
					nodeListInvisible[i].HandleInvisible(nodePosListInvisible[i]);
				}
				nodeListInvisible.Clear();
				nodePosListInvisible.Clear();
			}
		}
		nNodes = nodeListVisible.Count;
		if(nNodes> 0)
		{
			lock(nodeListVisible)
			{
				nNodes = nodeListVisible.Count;
				for(int i = 0; i < nNodes; i++)
				{
					nodeListVisible[i].HandleVisible(nodePosListVisible[i]);
				}
				nodeListVisible.Clear();
				nodePosListVisible.Clear();
			}
		}
		nNodes = nodeListMeshDestroy.Count;
		if(nNodes> 0)
		{
			lock(nodeListMeshDestroy)
			{
				nNodes = nodeListMeshDestroy.Count;
				for(int i = 0; i < nNodes; i++)
				{
					nodeListMeshDestroy[i].HandleMeshDestroy(nodePosListMeshDestroy[i]);
				}
				nodeListMeshDestroy.Clear();
				nodePosListMeshDestroy.Clear();
			}
		}				
	}
	
	// Attach general event
	public void AttachEvents(	DelegateNodeMeshCreated meshCreatedHandler, 
								DelegateNodeMeshDestroy meshDestroyHandler,
								DelegateNodePhyxReady phyxReadyHandler,
								DelegateRootPosChange posChangeHandler)
	{
		if(meshCreatedHandler != null)	handlerNodeMeshCreated += meshCreatedHandler;
		if(meshDestroyHandler != null)	handlerNodeMeshDestroy += meshDestroyHandler;
		if(phyxReadyHandler   != null)	handlerNodePhyxReady   += phyxReadyHandler;
		if(posChangeHandler   != null)	handlerRootPosChange   += posChangeHandler;
	}
	public void DetachEvents(	DelegateNodeMeshCreated meshCreatedHandler, 
								DelegateNodeMeshDestroy meshDestroyHandler,
								DelegateNodePhyxReady phyxReadyHandler,
								DelegateRootPosChange posChangeHandler)
	{
		if(meshCreatedHandler != null)	handlerNodeMeshCreated -= meshCreatedHandler;
		if(meshDestroyHandler != null)	handlerNodeMeshDestroy -= meshDestroyHandler;
		if(phyxReadyHandler   != null)	handlerNodePhyxReady   -= phyxReadyHandler;
		if(posChangeHandler   != null)	handlerRootPosChange   -= posChangeHandler;
	}
	// Attach node's event
	public void AttachNodeEvents(	DelegateNodeVisible visibleHandler, 
									DelegateNodeInvisible invisibleHandler, 
									DelegateNodeMeshCreated meshCreatedHandler,
									DelegateNodeMeshDestroy meshDestroyHandler,
									DelegateNodePhyxReady phyxReadyHandler,
									Vector3 pos, int nodeLod = 0)
	{
		LODOctreeNode node = GetParentNodeWithPos(pos, nodeLod);
		if(visibleHandler     != null)	node.handlerVisible     += visibleHandler;
		if(invisibleHandler   != null)	node.handlerInvisible   += invisibleHandler;
		if(meshCreatedHandler != null)	node.handlerMeshCreated += meshCreatedHandler;
		if(meshDestroyHandler != null)	node.handlerMeshDestroy += meshDestroyHandler;
		if(phyxReadyHandler   != null)	node.handlerPhyxReady   += phyxReadyHandler;
	}
	public void DetachNodeEvents(	DelegateNodeVisible visibleHandler, 
									DelegateNodeInvisible invisibleHandler, 
									DelegateNodeMeshCreated meshCreatedHandler,
									DelegateNodeMeshDestroy meshDestroyHandler,
									DelegateNodePhyxReady phyxReadyHandler,
									Vector3 pos, int nodeLod = 0)
	{
		LODOctreeNode node = GetParentNodeWithPos(pos, nodeLod);
		if(visibleHandler     != null)	node.handlerVisible     -= visibleHandler;
		if(invisibleHandler   != null)	node.handlerInvisible   -= invisibleHandler;
		if(meshCreatedHandler != null)	node.handlerMeshCreated -= meshCreatedHandler;
		if(meshDestroyHandler != null)	node.handlerMeshDestroy -= meshDestroyHandler;
		if(phyxReadyHandler   != null)	node.handlerPhyxReady   -= phyxReadyHandler;
	}

	// Utils
	public bool IsPosVisible(IntVector4 pos)
	{
		return _viewBoundsLod[pos.w].Contains(pos.ToVector3());
	}
	public bool IsPosValid(IntVector4 pos)
	{
		if(pos.w != 0) 						return false;
		// lod 0
		LODOctreeNode node = GetNodeWithCPos(pos.x >> VoxelTerrainConstants._shift,
		                                     pos.y >> VoxelTerrainConstants._shift,
		                                     pos.z >> VoxelTerrainConstants._shift,
		                                     pos.w);
		if(node == null)					return false;
		
		int n = node._data.Length;
		for(int i = 0; i < n; i++)
		{
			//TODO : code
			//if(!node._data[i]..IsReady)		return false;
		}
		return true;
	}
	public LODOctreeNode GetNodeWithCPos(int cx, int cy, int cz, int lod)
	{
		int chunkNumX = LODOctreeMan._xChunkCount >> lod;
		int chunkNumY = LODOctreeMan._yChunkCount >> lod;
		int chunkNumZ = LODOctreeMan._zChunkCount >> lod;
		int cxround = (cx>>lod)%chunkNumX;
		int cyround = (cy>>lod)%chunkNumY;
		int czround = (cz>>lod)%chunkNumZ;
		if(cxround < 0)	cxround += chunkNumX;
		if(cyround < 0)	cyround += chunkNumY;
		if(czround < 0)	czround += chunkNumZ;
		LODOctreeNode curNode = _lodTreeNodes[lod][cxround, cyround, czround];
		if(curNode.CX == cx && curNode.CY == cy && curNode.CZ == cz){
			return curNode;
		}
		return null;
	}
	public LODOctreeNode GetParentNodeWithPos(Vector3 pos, int nodeLod = 0)
	{
		int nX = Mathf.FloorToInt(pos.x/_treeSideLen);
		int nY = Mathf.FloorToInt(pos.y/_treeSideLen);
		int nZ = Mathf.FloorToInt(pos.z/_treeSideLen);
		int idxX = nX%LODOctreeMan._xLodRootChunkCount;
		int idxY = nY%LODOctreeMan._yLodRootChunkCount;
		int idxZ = nZ%LODOctreeMan._zLodRootChunkCount;
		if(idxX < 0)	idxX += LODOctreeMan._xLodRootChunkCount;
		if(idxY < 0)	idxY += LODOctreeMan._yLodRootChunkCount;
		if(idxZ < 0)	idxZ += LODOctreeMan._zLodRootChunkCount;
		int idx  = (idxX*LODOctreeMan._yLodRootChunkCount+idxY)*LODOctreeMan._zLodRootChunkCount+idxZ;
		
		LODOctreeNode curNode = _lodTrees[idx]._root;
		if(nodeLod == LODOctreeMan._maxLod)			return curNode;
		
		IntVector3 ofsPos = new IntVector3(pos.x-nX*_treeSideLen, pos.y-nY*_treeSideLen, pos.z-nZ*_treeSideLen);
		for(int lod = LODOctreeMan._maxLod; lod > nodeLod; lod--)
		{
			int childLen = LODOctreeNode._halfLens[lod];
			int idxChild = 0;
			if(ofsPos.x >= childLen){	idxChild+=1; ofsPos.x -= childLen; }
			if(ofsPos.y >= childLen){	idxChild+=2; ofsPos.y -= childLen; }
			if(ofsPos.z >= childLen){	idxChild+=4; ofsPos.z -= childLen; }
			curNode = curNode._child[idxChild];
		}
		return curNode;
	}
}
