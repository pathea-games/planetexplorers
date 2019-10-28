using UnityEngine;
using Mono.Data.SqliteClient;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

[Flags]
public enum EDependChunkType
{
	ChunkNotAvailable = 0,
	ChunkTerEmp = 1,
	ChunkTerCol = 2,
	ChunkBlkEmp = 4,
	ChunkBlkCol = 8,
	ChunkWatEmp = 16,
	ChunkWatCol = 32,
	//ChunkOthEmp = 64,		//other type could be added for extension
	//ChunkOthCol = 128,	//other type for extension
	ChunkTerMask = ChunkTerEmp | ChunkTerCol,
	ChunkBlkMask = ChunkBlkEmp | ChunkBlkCol,
	ChunkWatMask = ChunkWatEmp | ChunkWatCol,
	ChunkEmpMask = ChunkTerEmp | ChunkBlkEmp | ChunkWatEmp,
	ChunkColMask = ChunkTerCol | ChunkBlkCol | ChunkWatCol,
}

public interface ISceneObjActivationDependence
{
	bool IsDependableForAgent(ISceneObjAgent agent, ref EDependChunkType type);
}

public class SceneStaticObjDependence : ISceneObjActivationDependence
{
	int _nLastFrame = 0;
	List<ISceneObjAgent> _sceneObjs = null;
	List<Vector3> _posToCheck = new List<Vector3> (128);
	List<IBoundInScene> _boundsToCheck = new List<IBoundInScene> (128);
	public SceneStaticObjDependence(List<ISceneObjAgent> objs)
	{
		_sceneObjs = objs;
	}
	public bool IsDependableForAgent(ISceneObjAgent agent, ref EDependChunkType type)
	{
		int n = _sceneObjs.Count;
		if (Time.frameCount != _nLastFrame) {
			_nLastFrame = Time.frameCount;

			_posToCheck.Clear();
			_boundsToCheck.Clear();
			for (int i = 0; i < n; i++) {
				ISceneObjAgent dp = _sceneObjs [i];
				if (dp != null && !dp.NeedToActivate && dp.Go == null) {
					if(dp.Bound != null){
						_boundsToCheck.Add(dp.Bound);
					} else {
						_posToCheck.Add(dp.Pos);
					}
				}
			}
		}

		n = _posToCheck.Count;
		for (int i = 0; i < n; i++) {
			if (Vector3.SqrMagnitude (_posToCheck[i] - agent.Pos) < 64)	//8m
				return false;
		}

		n = _boundsToCheck.Count;
		for (int i = 0; i < n; i++) {
			if(_boundsToCheck[i].Contains(agent.Pos, agent.TstYOnActivate))
				return false;
		}
		return true;
	}
}

public class SceneChunkDependence : ISceneObjActivationDependence
{
	public static SceneChunkDependence Instance
	{
		get{
			if(_inst == null){
				_inst = new SceneChunkDependence();
			}
			return _inst;
		}
	}
	static SceneChunkDependence _inst = null;

	IntVector4 _tmpPos = new IntVector4();
	Dictionary<IntVector4, EDependChunkType> _lstValidChunks = new Dictionary<IntVector4, EDependChunkType>();
	void SetDependChunkType(IntVector4 cposlod, EDependChunkType type, bool bAdd)
	{
		lock (_lstValidChunks) {
			EDependChunkType val = 0;
			_lstValidChunks.TryGetValue(cposlod, out val);
			if(bAdd)		val |= type;
			else			val &= ~type;

			if(val == 0){	_lstValidChunks.Remove(cposlod);				}
			else 		{	_lstValidChunks[new IntVector4(cposlod)] = val;	}
		}
	}
	void GetDependChunkType(IntVector4 cposlod, out EDependChunkType val)
	{
		lock (_lstValidChunks) {
			_lstValidChunks.TryGetValue(cposlod, out val);
		}
	}
	void ClearAllDependChunks()
	{
		lock (_lstValidChunks) {
			_lstValidChunks.Clear();
		}
	}

	public void Reset()
	{
		ClearAllDependChunks();
	}
	public void ValidListAdd(IntVector4 cposlod, EDependChunkType type)
	{
		SetDependChunkType (cposlod, type, true);
		SceneMan.OnActivationDependenceDirty(true);
	}
	public void ValidListRemove(IntVector4 cposlod, EDependChunkType type)
	{
		SetDependChunkType (cposlod, type, false);
		SceneMan.OnActivationDependenceDirty(false);
	}
	public bool IsDependableForAgent(ISceneObjAgent agent, ref EDependChunkType type)
	{
		lock(_lstValidChunks)
		{
			Vector3 pos = agent.Pos;
			int x = ((int)pos.x)>>LODOctreeMan.Lod0NodeShift;
			int z = ((int)pos.z)>>LODOctreeMan.Lod0NodeShift;
			EDependChunkType v = 0;
			_tmpPos.x = x;
			_tmpPos.z = z;
			_tmpPos.w = 0;
			if(agent.TstYOnActivate)
			{
				int y = ((int)pos.y)>>LODOctreeMan.Lod0NodeShift;
				_tmpPos.y = y;
				GetDependChunkType(_tmpPos, out v);
				if((v&EDependChunkType.ChunkBlkMask) != 0 &&
				   //(v&EDependChunkType.ChunkWatMask) != 0 &&
				   (v&EDependChunkType.ChunkTerMask) != 0){
					type |= v;
					return true;
				}
				return false;				
			}
			else
			{
				// Note: here may be buggy on ChunkTerCol
				int y = ((int)SceneMan.LastRefreshPos.y)>>LODOctreeMan.Lod0NodeShift;
				_tmpPos.y = y;
				GetDependChunkType(_tmpPos, out v);
				if((v&EDependChunkType.ChunkBlkMask) != 0 && 
				   //(v&EDependChunkType.ChunkWatCol) != 0 && 
				   (v&EDependChunkType.ChunkTerCol) != 0){
					type |= v;
					return true;
				}

				int ofs = 1;
				while(ofs <= 8)
				{
					_tmpPos.y = y-ofs;
					GetDependChunkType(_tmpPos, out v);
					if((v&EDependChunkType.ChunkBlkMask) != 0 && 
					   //(v&EDependChunkType.ChunkWatCol) != 0 && 
					   (v&EDependChunkType.ChunkTerCol) != 0){
						type |= v;
						return true;
					}
					_tmpPos.y = y+ofs;
					GetDependChunkType(_tmpPos, out v);
					if((v&EDependChunkType.ChunkBlkMask) != 0 && 
					   //(v&EDependChunkType.ChunkWatCol) != 0 && 
					   (v&EDependChunkType.ChunkTerCol) != 0){
						type |= v;
						return true;
					}
					ofs++;
				}
				return false;
			}
		}
	}
}
