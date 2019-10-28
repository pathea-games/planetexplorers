using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class B45ChunkGo : MonoBehaviour
{
	public event DelegateB45ColliderCreated OnColliderCreated;
	public event DelegateB45ColliderCreated OnColliderDestroy;

	public B45ChunkData _data;
	public Mesh _mesh;
	public MeshCollider _meshCollider;
	public GameObject _transvoxelGo;

	public void AttachEvents(DelegateB45ColliderCreated created = null, 
	                                DelegateB45ColliderCreated destroy = null)
	{
		OnColliderCreated += created;
		OnColliderDestroy += destroy;
	}

	public void DetachEvents(DelegateB45ColliderCreated created = null, 
	                                DelegateB45ColliderCreated destroy = null)
	{
		OnColliderCreated -= created;
		OnColliderDestroy -= destroy;
	}

	void HandleColliderCreatedEvent()
	{
		if(OnColliderCreated != null && _mesh.vertexCount > 0)
		{
			OnColliderCreated(this);
		}
	}

	void HandleColliderDestroyEvent()
	{
		if(OnColliderDestroy != null && _mesh.vertexCount > 0)
		{
			OnColliderDestroy(this);
		}
	}

	public void SetCollider()
	{
		_meshCollider.sharedMesh = null;
		_meshCollider.sharedMesh = _mesh;

		HandleColliderCreatedEvent();
	}

	public void Destroy()
	{
		HandleColliderDestroyEvent();
		GameObject.Destroy(gameObject);
	}

	void OnDestroy()
	{
		//HandleColliderDestroyEvent();

	    //VFVoxelTerrain.self.SaveChunkByRecord();
        //VFVoxelTerrain.self.SaveChunkByRecord1();
		if(_data != null)	_data._maskTrans = 0;	// _sometime _data is null
		
		// Destroy all parts of chunkGo mesh and trans
		GameObject.Destroy(_mesh);
		_mesh = null;
		_transvoxelGo = null;
		foreach (Transform child in transform)
		{
			GameObject.Destroy(child.gameObject);
		}
	}
#if UNITY_EDITOR	
	public bool bStartRead = false;
	public bool bApplyWrite = false;
	public bool bGenTrans = false;
	public int _transvoxelMask;
	public byte xnpre,ynpre,znpre;
	public byte vol;
	void Update()
	{
		if(bStartRead)
		{
			bStartRead = false;
			int index = _data.IsHollow ? 0 : B45ChunkData.OneIndexNoPrefix(xnpre,ynpre,znpre);
			int indexVT = index*B45Block.Block45Size;
	        vol = _data.DataVT[indexVT];
		}
		if(bApplyWrite)
		{
			bApplyWrite = false;
			int index = _data.IsHollow ? 0 : B45ChunkData.OneIndexNoPrefix(xnpre,ynpre,znpre);
			int indexVT = index*B45Block.Block45Size;
			_data.WriteVoxelAtIdx(xnpre-VoxelTerrainConstants._numVoxelsPrefix, 
								ynpre-VoxelTerrainConstants._numVoxelsPrefix, 
								znpre-VoxelTerrainConstants._numVoxelsPrefix, 
								new B45Block(vol, _data.DataVT[indexVT+1]));
		}
	}
#endif
}
