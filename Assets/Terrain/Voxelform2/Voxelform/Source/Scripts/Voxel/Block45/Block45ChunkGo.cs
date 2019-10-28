
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Block45ChunkGo : MonoBehaviour, IRecyclable
{
	public static Transform _defParent;
	public static int _defLayer;
	public static Material[] _defMats;
	
	public MeshFilter _mf;
	public MeshRenderer _mr;
	public MeshCollider _mc;

	public  bool IsMeshReady    {	get{ return _mf.sharedMesh != null && _mf.sharedMesh.vertexCount > 0;	}}
	public  bool IsColliderReady{	get{ return _mc.sharedMesh != null;										}}
	public Block45OctNode _data = null;

	void Awake()
	{
		_mf = gameObject.AddComponent<MeshFilter>();
		_mr = gameObject.AddComponent<MeshRenderer>();
		_mc = gameObject.AddComponent<MeshCollider>();
		//_mr.sharedMaterial = _defMat;
		gameObject.layer = _defLayer;
		transform.parent = _defParent;
		name = "b45Chnk_";
	}

	public static Block45ChunkGo CreateChunkGo(IVxSurfExtractReq req, Transform parent = null)
	{
		if(req.FillMesh(null) == 0)	return null;

		Block45ChunkGo b45Go = VFGoPool<Block45ChunkGo>.GetGo();
		req.FillMesh(b45Go._mf.mesh);

		// set material
		SurfExtractReqB45 b45ret = req as SurfExtractReqB45;
		if(b45ret != null)
		{
			List<Material> tmpMatList = new List<Material>();		
			for(int i =0; i < b45ret.matCnt; i++)
			{
				tmpMatList.Add(_defMats[b45ret.materialMap[i]]);
			}			
			b45Go._mr.sharedMaterials = tmpMatList.ToArray();
		}
		if(parent != null)
		{
			b45Go.transform.parent = parent;
		}
		b45Go.gameObject.SetActive(true);
		return b45Go;
	}
#if UNITY_EDITOR
	public bool bShowData {
		get{ return false; }
		set{
			if(value == true){
				if(_data != null && _data.VecData !=null)
				{
					foreach(BlockVec vec in _data.VecData)
					{
						Debug.Log("["+vec.x+","+vec.y+","+vec.z+","+vec._byte0+","+vec._byte1+"]");
					}
				}
				else
				{
					Debug.LogError("No Data to show!!");
				}
			}
		}
	}
	public bool bRebuild {
		get{ return false; }
		set{
			if(value == true){
				if(_data != null && _data.NodeData != null)
				{
					_data.FreeChkData();
					_data.NodeData.AddToBuildList(_data);
				}
				else
				{
					Debug.LogError("No Data to rebuild!!");
				}
			}
		}
	}
#endif
	public void OnSetCollider()
	{
		_mc.sharedMesh = null;
		_mc.sharedMesh = _mf.sharedMesh;
		try
		{
			Block45Man.self.OnBlock45ColCreated(this);
		}
		catch(System.Exception e)
		{
			Debug.LogError("Exception OnBlock45ColCreated:"+e);
		}
	}
	public void OnRecycle()
	{
		gameObject.SetActive(false);

		if(_mc.sharedMesh != null)
		{
			try
			{
				Block45Man.self.OnBlock45ColDestroy(this);
			}
			catch(System.Exception e)
			{
				Debug.LogError("Exception OnBlock45ColDestroy:"+e);
			}
		}
		//Note: mesh.Clear() will cause rebuilding MeshCollider which cause spike;
		//		Destroy do not destroy it until the end of this frame, which cause FillMesh may fill the willDestroy mesh
		// 		So use DestroyImmediate instead.
		//DestroyImmediate(_mf.mesh);
		DestroyImmediate(_mf.sharedMesh);
		name = "b45Chnk_";
		if(transform.parent != _defParent)
		{
			gameObject.layer = _defLayer;
			transform.parent = _defParent;
		}
		_data = null;
	}
}
