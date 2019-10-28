using UnityEngine;
//using UnityEditor;
using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

public delegate void DelegateBlock45ColliderCreated(Block45ChunkGo b45Chunk);
public partial class Block45Man : SkillSystem.SkEntity, IVxChunkHelperProc, ILODNodeDataMan
{
	public static Block45Man self = null;
	public Material[] _b45Materials;
	public Block45OctDataSource DataSource{	get{ return _dataSource;	}	}
	private Block45OctDataSource _dataSource;
	//private List<Block45OctNode> _octNodesToAttach = new List<Block45OctNode>();
	public int _dbgLogicX;
	public int _dbgLogicY;
	public int _dbgLogicZ;
	public int _dbg0;
	public int _dbg1;
	public bool _dbgRead;
	public bool _dbgWrite;
	public bool _dbgLODNode = false;
	public int _dbgLODNodeX;
	public int _dbgLODNodeY;
	public int _dbgLODNodeZ;
	public int _dbgLODNodeW;

	void Awake()
	{
		Block45Man.self = this;
		_dataSource = new Block45OctDataSource(AddOctNewNodeToAttach);
		int layer = Pathea.Layer.VFVoxelTerrain;
		Block45ChunkGo._defLayer = layer;
		Block45ChunkGo._defMats = _b45Materials;
		Block45ChunkGo._defParent = transform;
		VFGoPool<Block45ChunkGo>.PreAlloc(64);
	}
	void Start()
	{
		InitSkEntity();
#if true
        //if (!GameConfig.IsMultiMode && Record.m_RecordBuffer.ContainsKey((int)Record.RecordIndex.RECORD_BLOCK45))
        //    _dataSource.ImportData(Record.m_RecordBuffer[(int)Record.RecordIndex.RECORD_BLOCK45]);
#else
		testDataSource();
#endif
		_bBuildColliderAsync = true;
	}
	void OnDestroy()
	{
		if(SurfExtractor != null)
		{
			SurfExtractor.Reset();
		}
		// If something is left over... (in the editor)
		if (gameObject.transform.childCount > 0)
		{
			// Clean up chunks
			for (int i = gameObject.transform.childCount - 1; i >= 0; --i)
			{
				var child = gameObject.transform.GetChild(i).gameObject;
				DestroyImmediate(child);
			}
		}
		Resources.UnloadUnusedAssets();
	}
	void testDataSource()
	{
		MemoryStream ms = new MemoryStream();
		BinaryWriter bw = new BinaryWriter(ms);
		int dataVer = 2;
		bw.Write(dataVer);
		switch(dataVer)
		{
		case 2:
#if false
			int elementCount = 1;
			bw.Write(elementCount);
			bw.Write(12227);
			bw.Write(124);
			bw.Write(6094);
			bw.Write((byte)11);
			bw.Write((byte)1);
#else
			bw.Write(3);	// count

			bw.Write(1);
			bw.Write(1);
			bw.Write(1);
			bw.Write((byte)12);
			bw.Write((byte)1);
			
			bw.Write(-1);
			bw.Write(1);
			bw.Write(-1);
			bw.Write((byte)13);
			bw.Write((byte)1);

			bw.Write(12227<<1);	//UnScaled
			bw.Write(124<<1);
			bw.Write(6094<<1);
			bw.Write((byte)11);
			bw.Write((byte)1);
#endif
			break;
		}
		bw.Close();
		ms.Close();

		byte[] inpba = ms.ToArray();
		_dataSource.ImportData(ms.ToArray());

		byte[] testba = _dataSource.ExportData();
		String str = "";
		for(int i = 0; i < inpba.Length; i++){
			str += inpba[i] + ", ";
		}
		print (str);
		str = "";
		for(int i = 0; i < inpba.Length; i++){
			str += testba[i] + ", ";
		}
		print (str);
	}
	public void AddOctNewNodeToAttach(Block45OctNode octNode)
	{
		LODOctreeNode node = LodMan.GetParentNodeWithPos(octNode._pos.ToVector3(), octNode._pos.w);
		octNode.AttachLODNode((Block45LODNodeData)node._data[IdxInLODNodeData]);
	}
#region IVxChunkHelperProc_ILODNodeDataMan
	private IntVector3 _tmpBoundPos = new IntVector3 ();
	private List<Block45OctNode> _tmpLstBlock45Datas = new List<Block45OctNode>();
	private int _idxInLODNodeData = 0;
	public int IdxInLODNodeData{	get{return _idxInLODNodeData;} set{_idxInLODNodeData = value;}	}
	public LODOctreeMan LodMan{	get;set;}
	public ILODNodeData CreateLODNodeData(LODOctreeNode node)
	{
		Block45LODNodeData data = new Block45LODNodeData(node);
		data.HelperProc = this;
		return data;
	}
	public void ProcPostLodInit()
	{
		DelayedLoad();
	}
	public void ProcPostLodUpdate ()
	{
		if(_dbgRead)
		{
			B45Block block = _dataSource.Read(_dbgLogicX,_dbgLogicY,_dbgLogicZ);
			_dbg0 = block.blockType;
			_dbg1 = block.materialType;
			_dbgRead = false;
		}
		if(_dbgWrite)
		{
			_dataSource.Write(new B45Block((byte)_dbg0, (byte)_dbg1),_dbgLogicX,_dbgLogicY,_dbgLogicZ);
			_dbgWrite = false;
		}
	}
	public void ProcPostLodRefresh(){}

	public IVxSurfExtractor SurfExtractor{ get{	return SurfExtractorsMan.B45BuildSurfExtractor;	} }
	public int  ChunkSig{ get{ return 0;	}	}
	public void ChunkProcPreSetDataVT(ILODNodeData cdata, byte[] data, bool bFromPool){}
	public void ChunkProcPreLoadData(ILODNodeData nData)
	{
		if(_dataSource == null || _dataSource.RootNode == null) return;

		Block45LODNodeData b45NodeData = nData as Block45LODNodeData;
		int size = 1 << (LODOctreeMan.Lod0NodeShift + b45NodeData.ChunkPosLod.w);
		_tmpBoundPos.x = b45NodeData.ChunkPosLod.x << VoxelTerrainConstants._shift;
		_tmpBoundPos.y = b45NodeData.ChunkPosLod.y << VoxelTerrainConstants._shift;
		_tmpBoundPos.z = b45NodeData.ChunkPosLod.z << VoxelTerrainConstants._shift;
		_tmpLstBlock45Datas.Clear ();
		lock (b45NodeData) {
			Block45OctNode.FindNodesCenterInside (_tmpBoundPos, size, b45NodeData.LOD, _dataSource.RootNode, ref _tmpLstBlock45Datas);
			b45NodeData.SetBlock45Datas (_tmpLstBlock45Datas);
		}
	}
	public bool ChunkProcExtractData(ILODNodeData ndata)
	{
		//TODO :
		return true;
	}
	public VFVoxel ChunkProcExtractData(ILODNodeData ndata, int x, int y, int z)
	{
		//TODO :
		return new VFVoxel();
	}
	public void ChunkProcPostGenMesh(IVxSurfExtractReq ireq)
	{
		SurfExtractReqB45 req = ireq as SurfExtractReqB45;
		Block45ChunkGo b45Go = Block45ChunkGo.CreateChunkGo(req);
		req._chunkData.AttachChunkGo(b45Go);
	}
	public void OnBegUpdateNodeData(ILODNodeData ndata)
	{
		if(LODOctreeMan._maxLod == 0)
		{
			Block45LODNodeData cdata = ndata as Block45LODNodeData;
			if(cdata.LOD == 0 && cdata.IsNodePosChange()){
				SceneChunkDependence.Instance.ValidListRemove(cdata.ChunkPosLod, EDependChunkType.ChunkBlkMask);
			}
		}
	}
	public void OnEndUpdateNodeData(ILODNodeData ndata)
	{
		Block45LODNodeData cdata = ndata as Block45LODNodeData;
		if(cdata.LOD == 0 && cdata.IsAllOctNodeReady())
		{
			EDependChunkType type = cdata.IsEmpty 	? EDependChunkType.ChunkBlkEmp
													: EDependChunkType.ChunkBlkCol;
			SceneChunkDependence.Instance.ValidListAdd(cdata.ChunkPosLod, type);
		}
	}
	public void OnDestroyNodeData(ILODNodeData ndata)
	{
		Block45LODNodeData cdata = ndata as Block45LODNodeData;
		IntVector4 cpos = cdata.ChunkPosLod;
		if(cpos != null && cpos.w == 0)
		{
			SceneChunkDependence.Instance.ValidListRemove(cpos, EDependChunkType.ChunkBlkMask);
		}
	}
#endregion

#region Dig
	static void Dig(IntVector3 blockUnitPos, float durDec, ref List<B45Block> removeList)
	{
		B45Block getBlock = Block45Man.self._dataSource.SafeRead(blockUnitPos.x, blockUnitPos.y, blockUnitPos.z);
		if (getBlock.blockType == 0)
			return;
		
		float digPower = durDec;
		// NaturalResAsset.NaturalRes resData = NaturalResAsset.NaturalRes.GetTerrainResData(getVoxel.Type);
		// if(null != resData)	digPower *= resData.m_duration;
		// else 				Debug.LogWarning("VoxelType[" + getVoxel.Type + "] does't have NaturalRes data.");
		
		if (digPower >= 127)
		{
			getBlock.blockType = 0;
			Block45Man.self._dataSource.SafeWrite(new B45Block(0), blockUnitPos.x, blockUnitPos.y, blockUnitPos.z);
			removeList.Add(getBlock);
		}
		return;
	}
	public static int DigBlock (IntVector3 intPos, float durDec,float radius, float height, ref List<B45Block> removeList, bool square = true)
	{
		for (int x = -Mathf.RoundToInt(radius); x <= Mathf.RoundToInt(radius); ++x)
		{
			for (int z = -Mathf.RoundToInt(radius); z <= Mathf.RoundToInt(radius); ++z)
			{
				for(int y = -Mathf.RoundToInt(height); y <= Mathf.RoundToInt(height); ++y)
				{
					IntVector3 idx = new IntVector3 (intPos.x + x, intPos.y + y, intPos.z + z);
					
					float sqrMagnitude = x * x + y * y + z * z;				
					if(!square && sqrMagnitude > radius * radius)
						continue;
					
					Dig(idx, durDec, ref removeList);					
					// Call back
					//if (onDigTerrain != null)
					//	onDigTerrain(idx);
				}
			}
		}		
		return removeList.Count;
	}
#endregion
#region SaveLoad
	Pathea.PeRecordReader _record;
	public void Export(Pathea.PeRecordWriter w)
	{
		BinaryWriter bw = w.binaryWriter;
		if (bw == null)
		{
			Debug.LogError("On WriteRecord FileStream is null!");
			return;
		}
		_dataSource.Export(bw);
	}
	public void Import(Pathea.PeRecordReader r)
	{
		_record = r;
	}
	void DelayedLoad()
	{
		if (null == _record)
		{
			return;
		}

        if (!_record.Open())
        {
            return;
        }

		BinaryReader br = _record.binaryReader;
		_dataSource.Import(br);
		_record.Close();
		_record = null;
	}
#endregion

	IntVector3 _boundSize = new IntVector3(LODOctreeMan._xLod0VoxelCount, LODOctreeMan._yLod0VoxelCount, LODOctreeMan._zLod0VoxelCount);
	IntVector3 _boundPos = new IntVector3 ();
	IntVector4 _lodCenterPos = new IntVector4 ();
	List<Block45OctNode> _lstBlock45Datas = new List<Block45OctNode>();
	bool _bBuildColliderAsync;
	static int _frameCnt;
	void FixedUpdate()
	{
		if (LodMan != null && LodMan.Observer != null && _frameCnt != Time.frameCount && _dataSource.RootNode != null)
		{	
			_frameCnt = Time.frameCount;
			_lstBlock45Datas.Clear();
			if (_bBuildColliderAsync){ // cur pos has the highest priority
				_lodCenterPos.x = Mathf.FloorToInt(LodMan.LastRefreshPos.x);
				_lodCenterPos.y = Mathf.FloorToInt(LodMan.LastRefreshPos.y);
				_lodCenterPos.z = Mathf.FloorToInt(LodMan.LastRefreshPos.z);
				_lodCenterPos.w = 0;
				Block45OctNode node = Block45OctNode.GetNodeRO(_lodCenterPos, _dataSource.RootNode);
				if(node != null){
					if(node.NodeData != null){
						node = node.NodeData.PickNodeToSetCol();
						if(node != null){
							_lstBlock45Datas.Add(node);
						}
					}
				}
			}
			if(_lstBlock45Datas.Count <= 0){
				_boundPos = LodMan._Lod0ViewBounds.min;
				GetNodesToGenCol0(_boundPos, _boundSize, _dataSource.RootNode, ref _lstBlock45Datas);
			}
			int n = _lstBlock45Datas.Count;
			for (int i = 0; i < n; i++) {
				Block45OctNode node = _lstBlock45Datas [i];
				if(node == null || node.ChunkGo == null)	//
					continue;

				colliderBuilding = true;
				node.ChunkGo.OnSetCollider ();
				if (node.NodeData != null && node.NodeData.IsAllOctNodeReady ()) {
					SceneChunkDependence.Instance.ValidListAdd (node.NodeData.ChunkPosLod, EDependChunkType.ChunkBlkCol);
				}
				if (_bBuildColliderAsync)
					return;
			}
			colliderBuilding = false;
		}
	}
	// strategy 0: check center inside
	// strategy 1: check whole inside
	void GetNodesToGenCol0(IntVector3 boundPos, IntVector3 boundSize, Block45OctNode root, ref List<Block45OctNode> outNodesList)
	{
		if (root._pos.w == 0) {
			if (root.IsCenterInside (boundPos, boundSize) && 
			    root.ChunkGo != null  && root.ChunkGo._mc != null && root.ChunkGo._mc.sharedMesh == null) {
				outNodesList.Add (root);
			}
		} else if (!root.IsLeaf) {
            if (root.IsOverlapped(boundPos, boundSize) && null != root._children)
            {
				for (int i = 0; i < 8; i++) {
					GetNodesToGenCol0 (boundPos, boundSize, root._children [i], ref outNodesList);
				}
			}
		}
	}
	void GetNodesToGenCol1(IntVector3 boundPos, IntVector3 boundSize, Block45OctNode root, ref List<Block45OctNode> outNodesList) // ignore y to improve performance
	{
		if (root._pos.w == 0) {
			if (root.IsWholeInside (boundPos, boundSize) && 
			    root.ChunkGo != null && root.ChunkGo._mc.sharedMesh == null) {
				outNodesList.Add (root);
			}
		} else if(!root.IsLeaf){
			if(root.IsWholeInside (boundPos, boundSize)){
				AddChildrenNodesToGenCol(root, ref outNodesList);
			}else if(root.IsOverlapped (boundPos, boundSize)){
				for (int i = 0; i < 8; i++) {
					GetNodesToGenCol1 (boundPos, boundSize, root._children [i], ref outNodesList);
				}
			}
		}
	}
	void AddChildrenNodesToGenCol(Block45OctNode root, ref List<Block45OctNode> outNodesList)
	{
		if (root._pos.w == 0) {
			if (root.ChunkGo != null && root.ChunkGo._mc.sharedMesh == null) {
				outNodesList.Add (root);
			}
		} else if (!root.IsLeaf) {
			for (int i = 0; i < 8; i++) {
				AddChildrenNodesToGenCol (root._children [i], ref outNodesList);
			}
		}
	}

	public event DelegateBlock45ColliderCreated OnColliderCreated;
	public event DelegateBlock45ColliderCreated OnColliderDestroy;
	bool colliderBuilding;
	public bool isColliderBuilding {get{return colliderBuilding;}}
	public void AttachEvents(DelegateBlock45ColliderCreated created = null,
	                         DelegateBlock45ColliderCreated destroy = null)
	{
		OnColliderCreated += created;
		OnColliderDestroy += destroy;
	}	
	public void DetachEvents(DelegateBlock45ColliderCreated created = null,
	                         DelegateBlock45ColliderCreated destroy = null)
	{
		OnColliderCreated -= created;
		OnColliderDestroy -= destroy;
	}
	public void onPlayerPosReady(Transform trans){
		//TODO : code
	}
	public void OnBlock45ColCreated(Block45ChunkGo b45Go)
	{
		if(OnColliderCreated != null)
		{
			OnColliderCreated(b45Go);
		}
	}
	public void OnBlock45ColDestroy(Block45ChunkGo b45Go)
	{
		if(OnColliderDestroy != null)
		{
			OnColliderDestroy(b45Go);
		}
	}


}
// ApplyBevel2_10 is in BSB45DiagonalBrush now.
