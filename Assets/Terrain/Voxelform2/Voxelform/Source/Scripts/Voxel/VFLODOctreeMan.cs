#define LODREFRESH_THREADING
#define POSTPOND_DESTROY
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;
using System.Linq;
using System.IO;
using System.Threading;

public delegate ILODNodeData DelegateToCreateLODNodeData(LODOctreeNode node);

public partial class LODOctreeNode
{
	private static int idxDataMax = 0;
	private static List<DelegateToCreateLODNodeData> handlerArrayCreateLODNodeData = new List<DelegateToCreateLODNodeData>();
	public static void ClearNodeDataCreation()
	{
		handlerArrayCreateLODNodeData.Clear();
		idxDataMax = 0;
	}
	public static void AddNodeDataCreation(ILODNodeDataMan nodeDataMan)
	{
		handlerArrayCreateLODNodeData.Add(nodeDataMan.CreateLODNodeData);
		idxDataMax = handlerArrayCreateLODNodeData.Count;
		nodeDataMan.IdxInLODNodeData = idxDataMax - 1;
	}
	public static void CreateNodeData(LODOctreeNode node)
	{
		for (int i = 0; i < idxDataMax; i++) {
			node._data[i].OnDestroyNodeData ();
		}
		node._data = null;
		if (node._child != null) {
			for(int i = 0; i < 8; i++ )	{
				DestroyNodeData(node._child[i]);
			}
		}
	}
	public static void DestroyNodeData(LODOctreeNode node)
	{
		for (int i = 0; i < idxDataMax; i++) {
			node._data[i].OnDestroyNodeData ();
		}
		node._data = null;
		if (node._child != null) {
			for(int i = 0; i < 8; i++ )	{
				DestroyNodeData(node._child[i]);
			}
		}
	}
	public static int[] _halfLens;
	public static void InitHalfLen()
	{
		_halfLens = new int[LODOctreeMan.MaxLod+1];
		for(int lod = 0; lod <= LODOctreeMan.MaxLod; lod++)
			_halfLens[lod] = 1<<(LODOctreeMan.Lod0NodeShift - 1 + lod);
	}
	
	private int _lod;
	private int _reqLod;
	private int _posx, _posy, _posz;
	public LODOctreeNode _parent;
	public LODOctreeNode[] _child;
	public ILODNodeData[] _data;
	// Properties and helpers
	public int CX{ 					get { return _posx >> LODOctreeMan.Lod0NodeShift; } }
	public int CY{ 					get { return _posy >> LODOctreeMan.Lod0NodeShift; } }
	public int CZ{ 					get { return _posz >> LODOctreeMan.Lod0NodeShift; } }
	public int Lod{ 				get{return _lod; } }
	public IntVector4 PosLod{		get{return new IntVector4(_posx, _posy, _posz, _lod);	}	}
	public IntVector4 ChunkPosLod{	get{return new IntVector4(	_posx>>LODOctreeMan.Lod0NodeShift,
			                                                    _posy>>LODOctreeMan.Lod0NodeShift,
			                                                    _posz>>LODOctreeMan.Lod0NodeShift, _lod);}	}
	public Vector3 Center{			get{int halfLen = _halfLens[_lod];	return new Vector3(_posx+halfLen, _posy+halfLen, _posz+halfLen);	}	}
	public bool IsInReq{			get{return _reqLod == _lod;		}	}		
	public LODOctreeNode(LODOctreeNode parent, int lod, int posx, int posy, int posz)
	{
		_lod = lod;
		_reqLod = 0xff;
		_posx = posx;
		_posy = posy;
		_posz = posz;

		_parent = parent;
		_child = null;
		initChildren();
		createNodeData();
	}
	public void Reposition(int newPosx, int newPosy, int newPosz)
	{
		if(_posx == newPosx && _posy == newPosy && _posz == newPosz)	return;

		if(_parent == null) OnRootPreMove(newPosx, newPosy, newPosz);
		if(_reqLod == _lod)	OnInvisible();
		_posx = newPosx;
		_posy = newPosy;
		_posz = newPosz;
		_reqLod = 0xff;	// reset _reqLod to active CreateChunk/DestroyChunk in SetReqLod
		if(_child != null)
		{
			int childLen = _halfLens[_lod];
			for(int i = 0; i < 8; i++ )
			{
				_child[i].Reposition(_posx+childLen * ( i & 1),
				                     _posy+childLen * ((i >> 1)& 1),
				                     _posz+childLen * ((i >> 2)& 1));
			}
		}
	}
	public void SetReqLod(int reqLod)
	{
		// Not do unnessary Create/Destroy. Note:_reqLod will be reset while repositioning
		if(_reqLod == reqLod)	return;

		// Strategy 1: Only update stamp when changing posLod in CreateChunkObj, some buildStep can be reuse.
		// Strategy 2: Update stamp all the time here, buildStep may reset or not reset.
		// Now use strategy 2
		for(int i = 0; i < idxDataMax; i++)		_data[i].UpdateTimeStamp();
		int reqBak = _reqLod;					_reqLod = reqLod;
		if(_reqLod == _lod)
		{
			if(reqBak != _lod)					OnVisible();
			for(int i = 0; i < idxDataMax; i++)	_data[i].BegUpdateNodeData();
		}
		else
		{
			if(reqBak == _lod)					OnInvisible();
			// Here code ensures those in datareading or meshbuilding to be set correct update status
			// So code on datareading or meshbuilding do not need to care about update status
#if !POSTPOND_DESTROY
			for(int i = 0; i < idxDataMax; i++)	_data[i].DestroyGameObject();
#endif
		}
	}
	public void RefreshNodeLod(ref Bounds[] viewBounds, int boundsLod)
	{
		int sideLen = _halfLens[_lod]<<1;
		Vector3 minPosNode = new Vector3(_posx+0.1f, _posy+0.1f, _posz+0.1f);
		Vector3 maxPosNode = new Vector3(_posx+sideLen-0.1f, _posy+sideLen-0.1f, _posz+sideLen-0.1f);
		Vector3 minPosView = viewBounds[boundsLod].min;
		Vector3 maxPosView = viewBounds[boundsLod].max;
		
		// Outside?
		if(	(minPosView.x >= maxPosNode.x || minPosView.y >= maxPosNode.y || minPosView.z >= maxPosNode.z) || 
			(maxPosView.x <= minPosNode.x || maxPosView.y <= minPosNode.y || maxPosView.z <= minPosNode.z) )
		{
			int reqLod = (boundsLod+1) > _lod ? _lod : (boundsLod+1);
			SetFamilyReqLod(reqLod);
		}
		else // Contain?
		if(	minPosView.x <= minPosNode.x && minPosView.y<= minPosNode.y && minPosView.z<= minPosNode.z && 
			maxPosView.x >= maxPosNode.x && maxPosView.y>= maxPosNode.y && maxPosView.z>= maxPosNode.z )
		{
			if(boundsLod > 0)
			{
				RefreshNodeLod(ref viewBounds, boundsLod-1);
			}
			else
			{
				int reqLod = 0;
				SetFamilyReqLod(reqLod);
			}
		}
		else 
		{	// Other
			if(_child != null)
			{
				int childBoundsLod = boundsLod < _child[0]._lod ? boundsLod : (_child[0]._lod-1);
				if(childBoundsLod < 0)
				{
					int reqLod = 0;
					for(int i = 0; i < 8; i++ )
					{
						_child[i].SetReqLod(reqLod);
					}
					SetReqLod(reqLod);
				}
				else
				{
					int minChildLod = boundsLod+1;
					for(int i = 0; i < 8; i++ )
					{
						_child[i].RefreshNodeLod(ref viewBounds, childBoundsLod);
						minChildLod = minChildLod < _child[i]._reqLod ? minChildLod : _child[i]._reqLod;
					}
					SetReqLod(minChildLod);
				}
			}
			else
			{
				Debug.LogError("[RefreshNodeLod]Never run here");
				int reqLod = boundsLod;
				SetReqLod(reqLod);
			}
		}
	}
	public void OnEndUpdateNodeData(ILODNodeData cdata)
	{
		#if POSTPOND_DESTROY
		for(int i = 0; i < idxDataMax; i++)
		{
			if(cdata == _data[i])
			{	
				DestroyInactiveNodeData(i);
				return;
			}
		}
		#endif
	}

	private void createNodeData()
	{
		_data = new ILODNodeData[idxDataMax];
		// fill the data array
		for(int i = 0; i < idxDataMax; i++)
		{
			_data[i] = handlerArrayCreateLODNodeData[i](this);
		}
	}	
	private void initChildren()
	{
		if(_lod == 0)	return;
		
		_child = new LODOctreeNode[8];
		int childLen = _halfLens[_lod];
		for(int i = 0; i < 8; i++ )
		{
			_child[i] = new LODOctreeNode(this, _lod-1, _posx+childLen * ( i & 1),
			                              _posy+childLen * ((i >> 1)& 1),
			                              _posz+childLen * ((i >> 2)& 1));
		}
	}
	private void SetFamilyReqLod(int reqLod)
	{
		if(_child != null)
		{
			for(int i = 0; i < 8; i++ )
			{
				_child[i].SetFamilyReqLod(reqLod);
			}
		}
		SetReqLod(reqLod);
	}
	private void DestroyChildNodeData(int idxData, int reqLod)
	{
		for(int i = 0; i < 8; i++ )
		{
			if(reqLod != _child[i]._lod)	
				_child[i]._data[idxData].OnDestroyNodeData();
			if(_child[i]._child != null)
				_child[i].DestroyChildNodeData(idxData,reqLod);
		}
	}
	private void DestroyParentNodeData(int idxData, int reqLod)
	{
		//IsNotInUpdate
		if(_parent._child[0]._data[idxData].IsIdle &&
		   _parent._child[1]._data[idxData].IsIdle &&
		   _parent._child[2]._data[idxData].IsIdle &&
		   _parent._child[3]._data[idxData].IsIdle &&
		   _parent._child[4]._data[idxData].IsIdle &&
		   _parent._child[5]._data[idxData].IsIdle &&
		   _parent._child[6]._data[idxData].IsIdle &&
		   _parent._child[7]._data[idxData].IsIdle )
		{
			if(reqLod != _parent._lod)	
				_parent._data[idxData].OnDestroyNodeData();
			if(_parent._parent != null)
				_parent.DestroyParentNodeData(idxData,reqLod);
		}
	}
	private void DestroyInactiveNodeData(int idxData){	
		if(_reqLod != _lod)
		{
			//Debug.LogError("[LOD]OnUpdateFin invoked in an inactive node!");
			_data[idxData].OnDestroyNodeData();
		}
		// Pass this reqLod instead of children/parents' reqLod bacause 
		// these children/parents' reqLod may not be refresh to the latest value
		// when IsUpdating is called in SetReqLod->CreateChunkObject
		if(_child != null)
			DestroyChildNodeData(idxData, _reqLod);
		if(_parent != null)
		{
			// Check if all siblings'req has updated
			if(_parent._child[0]._reqLod <= _reqLod &&
			   _parent._child[1]._reqLod <= _reqLod &&
			   _parent._child[2]._reqLod <= _reqLod &&
			   _parent._child[3]._reqLod <= _reqLod &&
			   _parent._child[4]._reqLod <= _reqLod &&
			   _parent._child[5]._reqLod <= _reqLod &&
			   _parent._child[6]._reqLod <= _reqLod &&
			   _parent._child[7]._reqLod <= _reqLod )
			{
				DestroyParentNodeData(idxData, _reqLod);
			}
		}
	}
}
public class LODOctree
{
	private LODOctreeMan _parentMan;
	//private int _maxLOD;
	//private IntVector3 _sideLen;	// should equal _root._halfLen*2
	private IntVector3 _idxPos;
	private IntVector3 _pos;
	public LODOctreeNode _root;
	private Vector3 _rootCenter;
	private int _rootHalfLen;
	
	public LODOctree(LODOctreeMan man, int maxlod, IntVector3 idxPos,  IntVector3 sideLen)
	{
		_parentMan = man;
		//_maxLOD = maxlod;
		//_sideLen = sideLen;
		_idxPos = idxPos;
		_pos = idxPos*sideLen;
        _root = new LODOctreeNode(null, maxlod, 
		                          (int)LODOctreeMan.InitPos.x,
		                          (int)LODOctreeMan.InitPos.y,
		                          (int)LODOctreeMan.InitPos.z);	// this pos can cause root to reposition.
		_rootHalfLen = LODOctreeNode._halfLens[_root.Lod];
		_rootCenter = _root.Center;
	}
	public void OnDestroy()
	{
		if (_root != null) {
			LODOctreeNode.DestroyNodeData(_root);
		}
	}
	public bool RefreshPos(Vector3 camPos)
	{
		if(!_parentMan._viewBounds.Contains(_rootCenter))
		{
			//REPOS: To tweak maxLod's dist can cause reposition delay which may promote performance more or less.
			
			//here pos is constant, something like index.
			//(posCenter + viewBounds.size*n) is in [campos-viewBounds.extents, campos+viewBounds.extents)
			//so n is in [(campos-viewBounds.extents-posCenter)/viewBounds.size, (campos+viewBounds.extents-posCenter)/viewBounds.size)
			//newPos is n*viewBounds.size + pos
			Vector3 newBoundPos = camPos + _parentMan._viewBounds.extents;
			newBoundPos.x -= _pos.x + _rootHalfLen;
			newBoundPos.y -= _pos.y + _rootHalfLen;
			newBoundPos.z -= _pos.z + _rootHalfLen;
			int newPosx = (Mathf.FloorToInt(newBoundPos.x/_parentMan._viewBoundsSize.x))*_parentMan._viewBoundsSize.x + _pos.x;
			int newPosy = (Mathf.FloorToInt(newBoundPos.y/_parentMan._viewBoundsSize.y))*_parentMan._viewBoundsSize.y + _pos.y;
			int newPosz = (Mathf.FloorToInt(newBoundPos.z/_parentMan._viewBoundsSize.z))*_parentMan._viewBoundsSize.z + _pos.z;
			//Debug.Log("Tree out of viewscope, repositioning...");
			_root.Reposition(newPosx, newPosy, newPosz);
			_rootCenter = _root.Center;	// Recalculate center
			return true;
		}
		return false;
	}
	public void FillTreeNodeArray(ref LODOctreeNode[][,,] lodTreeNodes)
	{
		TraverseFillTreeNodeArray(_root, ref lodTreeNodes, _idxPos);
	}
	private void TraverseFillTreeNodeArray(LODOctreeNode node, ref LODOctreeNode[][,,] lodTreeNodes, IntVector3 startIdx)
	{
		lodTreeNodes[node.Lod][startIdx.x,startIdx.y,startIdx.z] = node;
		if(node.Lod > 0)
		{
			for(int i = 0; i < 8; i++)
			{
				IntVector3 idx = new IntVector3((startIdx.x<<1)+( i & 1),
												(startIdx.y<<1)+((i >> 1)& 1),
												(startIdx.z<<1)+((i >> 2)& 1));
				TraverseFillTreeNodeArray(node._child[i], ref lodTreeNodes, idx);
			}
		}
	}
}

public partial class LODOctreeMan
{
	public const int Lod0NodeShift = VoxelTerrainConstants._shift;
	public const int Lod0NodeSize = 1 << Lod0NodeShift;
	public const int MaxLod = 4;	//inclusive
	public static int _maxLod{get; set;}	//0:256; 1:512; 2:1024; 3:2048
	private const int __defxLodRootChunkCount = 8;
	private const int __defyLodRootChunkCount = 4;
	private const int __defzLodRootChunkCount = 8;
	private static int __xLodRootChunkCount = __defxLodRootChunkCount;
	private static int __yLodRootChunkCount = __defyLodRootChunkCount;
	private static int __zLodRootChunkCount = __defzLodRootChunkCount;
	public static int _xLodRootChunkCount{	get{	return __xLodRootChunkCount;							}	}
	public static int _yLodRootChunkCount{	get{	return __yLodRootChunkCount;							}	}
	public static int _zLodRootChunkCount{	get{	return __zLodRootChunkCount;							}	}
	public static int _xChunkCount{			get{	return __xLodRootChunkCount<<_maxLod;					}	}
	public static int _yChunkCount{			get{	return __yLodRootChunkCount<<_maxLod;					}	}
	public static int _zChunkCount{			get{	return __zLodRootChunkCount<<_maxLod;					}	}
	public static int _xLod0VoxelCount{		get{	return __xLodRootChunkCount<<Lod0NodeShift;				}	}
	public static int _yLod0VoxelCount{		get{	return __yLodRootChunkCount<<Lod0NodeShift;				}	}
	public static int _zLod0VoxelCount{		get{	return __zLodRootChunkCount<<Lod0NodeShift;				}	}
	private static int _xVoxelCount{		get{	return __xLodRootChunkCount<<(_maxLod+Lod0NodeShift);	}	}
	private static int _yVoxelCount{		get{	return __yLodRootChunkCount<<(_maxLod+Lod0NodeShift);	}	}
	private static int _zVoxelCount{		get{	return __zLodRootChunkCount<<(_maxLod+Lod0NodeShift);	}	}
	public static void ResetRootChunkCount(int xCnt = __defxLodRootChunkCount, 
	                                       int yCnt = __defyLodRootChunkCount, 
	                                       int zCnt = __defzLodRootChunkCount)
	{
		__xLodRootChunkCount = xCnt;
		__yLodRootChunkCount = yCnt;
		__zLodRootChunkCount = zCnt;
	}

	public static LODOctreeMan self{get; private set;}
	public static System.Object obj4LockLod = new System.Object(); //for CameraPos & DestroyGoList exchangement in threads
	public static readonly Vector3 InitPos = new Vector3(0, -999999.0f, 0); // cause node reposition

	private Thread _threadLodRefresh;
	private Vector3 _curCamPos;
	private Vector3 _lastCamPos;
	private bool _bThreadOn = true;
	private int _sqrRefreshThreshold;
	private int _treeSideLen;
	private Bounds[] _viewBoundsLod;
	internal IntVector3 _viewBoundsSize;
	private LODOctree[] _lodTrees;
	private LODOctreeNode[][,,] _lodTreeNodes;
	private event Action _procPostInit;
	private event Action _procPostUpdate;
	private event Action _procPostRefresh;
	public LODOctreeNode[][,,] LodTreeNodes{get{	return _lodTreeNodes;									}	}
	public Bounds _viewBounds{				get{	return _viewBoundsLod[LODOctreeMan._maxLod];			}	}
	public Bounds _Lod0ViewBounds{			get{	return _viewBoundsLod[0];								}	}
	public bool IsFirstRefreshed{			get{	return _lastCamPos.y > InitPos.y+1;						}	}
	public Vector3 LastRefreshPos{			get{	return _lastCamPos;										}	}
	public Transform Observer{ 				get; set; }

	public LODOctreeMan(ILODNodeDataMan[] lodNodeDataMans, int maxlod, int refreshThreshold = 1, Transform observer = null)
	{
		self = this;
		_maxLod = maxlod;
		Observer = observer;

		int xChunkCount = LODOctreeMan._xChunkCount;
		int yChunkCount = LODOctreeMan._yChunkCount;
		int zChunkCount = LODOctreeMan._zChunkCount;
		int xVoxelCount = LODOctreeMan._xVoxelCount;
		int yVoxelCount = LODOctreeMan._yVoxelCount;
		int zVoxelCount = LODOctreeMan._zVoxelCount;

		_curCamPos = InitPos;
		_lastCamPos = InitPos;
		_sqrRefreshThreshold = refreshThreshold * refreshThreshold;
		_treeSideLen = Lod0NodeSize<<LODOctreeMan._maxLod;
		_viewBoundsSize =new IntVector3(xVoxelCount, yVoxelCount, zVoxelCount);
		_viewBoundsLod = new Bounds[LODOctreeMan._maxLod+1];
		int lod = 0;
		for(lod = LODOctreeMan._maxLod; lod >= 0; lod--)
		{
			int shift = LODOctreeMan._maxLod - lod;
            _viewBoundsLod[lod] = new Bounds(InitPos,
							new Vector3(xVoxelCount>>shift,
										yVoxelCount>>shift,
										zVoxelCount>>shift));
		}

#if false
		lodNodeDataMans = new ILODNodeDataMan[]{	 VFVoxelTerrain.self
													,VFVoxelWater.self
													,Block45Man.self
													};
#endif
		LODOctreeNode.ClearNodeDataCreation();
		foreach(ILODNodeDataMan ndataMan in lodNodeDataMans)
		{
			LODOctreeNode.AddNodeDataCreation(ndataMan);
			ndataMan.LodMan = this;
			_procPostInit += ndataMan.ProcPostLodInit;
			_procPostUpdate += ndataMan.ProcPostLodUpdate;
			_procPostRefresh += ndataMan.ProcPostLodRefresh;
		}
		LODOctreeNode.InitHalfLen();
		_lodTreeNodes = new LODOctreeNode[LODOctreeMan._maxLod+1][,,];
		for(lod = 0; lod <= LODOctreeMan._maxLod; lod++)
		{
			_lodTreeNodes[lod] = new LODOctreeNode[xChunkCount>>lod, yChunkCount>>lod, zChunkCount>>lod];
		}
		
		_lodTrees = new LODOctree[LODOctreeMan._xLodRootChunkCount*
		                          LODOctreeMan._yLodRootChunkCount*
		                          LODOctreeMan._zLodRootChunkCount];
		int i = 0;
		for(int x = 0; x < LODOctreeMan._xLodRootChunkCount; x++)
			for(int y = 0; y < LODOctreeMan._yLodRootChunkCount; y++)
				for(int z = 0; z < LODOctreeMan._zLodRootChunkCount; z++)
				{
					_lodTrees[i] = new LODOctree(this, LODOctreeMan._maxLod, 
						new IntVector3(x,y,z), 
						new IntVector3(_treeSideLen, _treeSideLen, _treeSideLen) );
					_lodTrees[i].FillTreeNodeArray(ref _lodTreeNodes);
					i++;
				}

		if(_procPostInit !=null)		_procPostInit();

#if LODREFRESH_THREADING		
		StartLodThread();
#endif
	}
	private void ReqRefresh(Vector3 camPos)
	{
#if LODREFRESH_THREADING		
		lock(obj4LockLod)
		{
			_curCamPos = camPos;	
		}
#else
		_curCamPos = camPos;	
		ExecRefresh();
#endif
		if (_procPostUpdate != null)	_procPostUpdate ();
		DispatchNodeEvents();
	}
	public void Reset(){				ReqRefresh(InitPos);	}
	public bool ReqRefresh()
	{
		if(Observer == null)			return false;

		ReqRefresh(Observer.position);
		return true;
	}	
	public void ReqDestroy()
	{
		if(self == null)				return;

		_bThreadOn = false;
		try{
			_threadLodRefresh.Join();
			MonoBehaviour.print("[LOD]Thread stopped");
		}catch{
			MonoBehaviour.print("[LOD]Thread stopped with exception");
		}
		self = null;
	}

	private void StartLodThread()
	{
		_threadLodRefresh = new Thread(new ThreadStart(threadLodRefreshExec));
		_threadLodRefresh.Start();
	}
	private void threadLodRefreshExec()
	{
		_bThreadOn = true;
		while(_bThreadOn)
		{
			try{						ExecRefresh();				}
			catch(Exception e){			Debug.LogWarning(e);		}
			Thread.Sleep(8);
		}
		ExecDestroy ();
	}
	private void ExecDestroy()
	{
		int n = _lodTrees.Length;
		for (int i = 0; i < n; i++) {
			_lodTrees[i].OnDestroy();
		}
		_lodTrees = null;
	}
	private void ExecRefresh()
	{
		Vector3 camPos;
#if LODREFRESH_THREADING
		lock(obj4LockLod){ 				camPos = _curCamPos; }
#else
		camPos = _curCamPos;
#endif
		ExecRefresh(camPos);
		if(_procPostRefresh != null)	_procPostRefresh();
	}
	private bool ExecRefresh(Vector3 camPos)
	{
		if(camPos.y <= -99999)//-LODOctreeMan._yVoxelCount)
		{
			_lastCamPos = camPos;
			return false;
		}
						
		Vector3 vDist = new Vector3(camPos.x - _lastCamPos.x, 
									camPos.y - _lastCamPos.y, 
									camPos.z - _lastCamPos.z);
		if(vDist.sqrMagnitude > _sqrRefreshThreshold)
		{
			for(int lod = 0; lod <= LODOctreeMan._maxLod; lod++)
			{
				_viewBoundsLod[lod].center = camPos;
			}
	
			// Note: here use contain center pos, in contrast, RefreshNodeLod use caontain the whole bound
			for(int i = 0; i < _lodTrees.Length; i++)
			{
				_lodTrees[i].RefreshPos(camPos);
			}		
			// Update lods----the algo(recurse from parent to child) is more efficient than from child to parent.
			if(LODOctreeMan._maxLod > 0)
			{
				// From parent to child
				for(int i = 0; i < _lodTrees.Length; i++)
				{
					_lodTrees[i]._root.RefreshNodeLod(ref _viewBoundsLod, LODOctreeMan._maxLod-1);
				}
			}
			else
			{
				for(int i = 0; i < _lodTrees.Length; i++)
				{
					_lodTrees[i]._root.SetReqLod(0);
				}
			}
			_lastCamPos = camPos;
			return true;
		}
		return false;
	}	

	public void ClearNodes()
	{
		for(int i = 0; i < _lodTrees.Length; i++)
		{
			ClearNode(_lodTrees[i]._root);
		}
	}
	private void ClearNode(LODOctreeNode node)
	{
		if (node._data [0].IsEmpty) {
			node._data = null;
		}
		if (node._child != null) {
			for(int i = 0; i < 8; i++){
				ClearNode(node._child[i]);
			}
		}
	}
}

