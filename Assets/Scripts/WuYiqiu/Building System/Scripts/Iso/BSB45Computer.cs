using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BSB45Computer : MonoBehaviour 
{
	private Block45OctDataSource _dataSource;
	public Block45OctDataSource DataSource{ get{ return _dataSource; } }
	private List<Block45OctNode> _octNodesToBuild = new List<Block45OctNode>();
	public bool IsInRebuilding{	get{ return SurfExtractorsMan.B45BuildSurfExtractor != null && !SurfExtractorsMan.B45BuildSurfExtractor.IsAllClear; }	}
	

	public void AlterBlockInBuild(int vx, int vy, int vz, B45Block blk)
	{
		DataSource.SafeWrite(blk, vx, vy, vz, 0);

	}

	public void ClearDataDS()
	{
		if (_dataSource != null)
			_dataSource.Clear();
	}

	public void RebuildMesh()
	{
		if(SurfExtractorsMan.B45BuildSurfExtractor != null)
		{
			for (int i = 0; i < _octNodesToBuild.Count; i++) {
				Block45OctNode octNode = _octNodesToBuild [i];
				SurfExtractorsMan.B45BuildSurfExtractor.AddSurfExtractReq (SurfExtractReqB45.Get (octNode.GetStamp (), octNode, ChunkProcPostGenMesh));
			}
		}
	}

	void AddNodeToBuildList(Block45OctNode node)
	{
		if(node.LOD == 0)
		{
			_octNodesToBuild.Add(node);
		}
	}

	void ChunkProcPostGenMesh(IVxSurfExtractReq ireq)
	{
		if (this == null)
			return;
		SurfExtractReqB45 req = ireq as SurfExtractReqB45;
		Block45ChunkGo b45Go = Block45ChunkGo.CreateChunkGo(req, transform);

		req._chunkData.AttachChunkGo(b45Go);
	}

	void Awake()
	{

		_dataSource = new Block45OctDataSource(AddNodeToBuildList);
	}



}
