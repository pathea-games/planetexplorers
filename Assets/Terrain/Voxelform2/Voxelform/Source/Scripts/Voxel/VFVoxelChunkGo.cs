using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class VFVoxelChunkGo : MonoBehaviour,IRecyclable
{
    public delegate void OnChunkColliderCreated(VFVoxelChunkGo chunk);
    public static event OnChunkColliderCreated CreateChunkColliderEvent;

    public delegate void OnChunkColliderDestroy(VFVoxelChunkGo chunk);
    public static event OnChunkColliderDestroy DestroyChunkColliderEvent;

	public delegate void OnChunkColliderRebuild(VFVoxelChunkGo chunk);
	public static event OnChunkColliderRebuild RebuildChunkColliderEvent;

	public static Transform DefParent;
	public static Material DefMat;
	public static int DefLayer;
	
	int _terType = int.MinValue;
	bool _bPrimal = false;

	public MeshFilter Mf;
	public MeshRenderer Mr;
	public MeshCollider Mc;
	public VFVoxelChunkData Data = null;
	public VFTransVoxelGo TransvoxelGo = null;
	public VFVoxelChunkGo OriginalChunkGo = null;
	public  bool IsMeshReady    {	get{ return Mf.sharedMesh != null && Mf.sharedMesh.vertexCount > 0;}}
	public  bool IsColliderReady{	get{ return OriginalChunkGo != null || Mc.sharedMesh != null;}}
	public  bool IsOriginalGo   {   get{ return Mr.enabled == false; }}

	void Awake()
	{
		Mf = gameObject.AddComponent<MeshFilter>();
		Mr = gameObject.AddComponent<MeshRenderer>();
		Mc = gameObject.AddComponent<MeshCollider>();
		Mr.sharedMaterial = DefMat;
		Mf.sharedMesh = new Mesh();
		gameObject.layer = DefLayer;
		transform.parent = DefParent;
		name = "chunk_";				
	}
	
	public static VFVoxelChunkGo CreateChunkGo(IVxSurfExtractReq req, Material mat = null, int layer = 0)
	{
		if(req.FillMesh(null) == 0)	return null;

		VFVoxelChunkGo vfGo = VFGoPool<VFVoxelChunkGo>.GetGo();
		int nMesh = req.FillMesh(vfGo.Mf.sharedMesh);
		vfGo._bPrimal = true;
		if (mat != null){			vfGo.Mr.sharedMaterial = mat;		}
		if (layer != 0){			vfGo.gameObject.layer = layer;		}
		vfGo.gameObject.SetActive(true);

		while(nMesh > 0)
		{
			VFVoxelChunkGo subGo = VFGoPool<VFVoxelChunkGo>.GetGo();
			nMesh = req.FillMesh(subGo.Mf.sharedMesh);
			subGo.transform.parent = vfGo.transform;
			subGo.transform.localScale = Vector3.one;
			subGo.transform.localPosition = Vector3.zero;
			if (mat != null){			subGo.Mr.sharedMaterial = mat;		}
			if (layer != 0){			subGo.gameObject.layer = layer;		}
			subGo.gameObject.SetActive(true);
		}		

		return vfGo;
	}
	public static VFVoxelChunkGo CreateChunkGo(Mesh sharedMesh)
	{
		VFVoxelChunkGo vfGo = VFGoPool<VFVoxelChunkGo>.GetGo();
		vfGo.Mf.sharedMesh = sharedMesh;
		vfGo._bPrimal = true;
		vfGo.gameObject.SetActive(true);
		return vfGo;
	}
	public void SetTransGo(IVxSurfExtractReq req, int faceMask)
	{
		if(TransvoxelGo != null)
		{
			VFGoPool<VFTransVoxelGo>.FreeGo(TransvoxelGo);
			TransvoxelGo = null;
		}
		if(faceMask != 0)
		{
			VFTransVoxelGo go = VFGoPool<VFTransVoxelGo>.GetGo();
			req.FillMesh(go._mf.sharedMesh);
			go._faceMask = faceMask;
			go.transform.parent = transform;
			//go.transform.localScale = Vector3.one;
			go.transform.localPosition = Vector3.zero;
			go.gameObject.SetActive(true);
			TransvoxelGo = go;	
		}
	}
	public void OnRecycle()
	{
		gameObject.SetActive(false);
		if(IsOriginalGo)							Mr.enabled = true;
		else if(_bPrimal)
		{	
			if(Mc.sharedMesh != null)	OnColliderDestroy();
		}

		//Note: Directly mesh.Clear() will cause rebuilding MeshCollider which cause spike;
		//		Destroy do not destroy it until the end of this frame, which cause FillMesh may fill the willDestroy mesh
		// 		So use DestroyImmediate instead.
		//DestroyImmediate(_mf.sharedMesh, true);
		Mc.sharedMesh = null;
		Mf.sharedMesh.Clear ();
		_terType = int.MinValue;
		_bPrimal = false;
		Data = null;
		if(OriginalChunkGo != null)
		{
			Debug.LogError("[VFChunkGo]Free original chunk go" + OriginalChunkGo.name);
			VFGoPool<VFVoxelChunkGo>.FreeGo(OriginalChunkGo);
			OriginalChunkGo = null;
		}
		if(TransvoxelGo != null)
		{
			VFGoPool<VFTransVoxelGo>.FreeGo(TransvoxelGo);
			TransvoxelGo = null;
		}
		// Reset to default
		Mr.sharedMaterial = DefMat;
		gameObject.layer = DefLayer;
		transform.parent = DefParent;
		for(int i = transform.childCount-1; i >= 0; i--)
		{
			Transform child = transform.GetChild(i);
			VFGoPool<VFVoxelChunkGo>.FreeGo(child.GetComponent<VFVoxelChunkGo>());
		}
	}
	public void OnColliderReady()
	{
		Profiler.BeginSample ("OnColReady");
        bool isNewCollider = (OriginalChunkGo == null);
  
		if(OriginalChunkGo != null)
		{
			VFGoPool<VFVoxelChunkGo>.FreeGo(OriginalChunkGo);
			OriginalChunkGo = null;

			if(RebuildChunkColliderEvent != null)
			{
				try{RebuildChunkColliderEvent(this);}
				catch{Debug.LogError("Error in RebuildChunkColliderEvent:"+transform.position);}
			}
			Profiler.EndSample ();
			return;
		}
		else if(Data != null)
		{
			Data.OnGoColliderReady();
		}

        if (CreateChunkColliderEvent != null && isNewCollider)
        {
			try{CreateChunkColliderEvent(this);	}
			catch{Debug.LogError("Error in CreateChunkColliderEvent:"+transform.position);}
        }
		Profiler.EndSample ();
	}
	public void OnColliderDestroy()
	{
		if(!_bPrimal)	return;

        if (DestroyChunkColliderEvent != null && Mr.enabled)
        {
			try{DestroyChunkColliderEvent(this);}
			catch{Debug.LogError("Error in DestroyChunkColliderEvent:"+transform.position);}
        }
	}
	void OnDestroy() // Destroy in VFTerrain
	{
		Mc.sharedMesh = null;
		_bPrimal = false;
		Data = null;
		TransvoxelGo = null;
		Destroy(Mf.sharedMesh);
	}	
	void OnWillRenderObject()
	{
		if (_bPrimal && gameObject.layer == VFVoxelWater.s_layer && !VFVoxelWater.s_bSeaInSight) {
			if(Pathea.PeGameMgr.IsStory){
				//aispawn_story sea:27/28 and 24 beach
				if(_terType == int.MinValue && Data != null && Data.LOD <= 0){
					_terType = PeMappingMgr.Instance.GetAiSpawnMapId(new Vector2(transform.position.x, transform.position.z));
				}
				if(_terType == 27 || _terType == 28 || _terType == 24){
					VFVoxelWater.s_bSeaInSight = true;
				}
			} else {
				VFVoxelWater.s_bSeaInSight = true;
			}
		}
	}


#if UNITY_EDITOR	
	public int _transvoxelMask;
	public byte xnpre,ynpre,znpre;
	public byte vol;
	public byte typ;
	public bool bApplyRead {
		get{ return false; }
		set{
			if(value == true){
				int index = Data.IsHollow ? 0 : VFVoxelChunkData.OneIndexNoPrefix(xnpre,ynpre,znpre);
				int indexVT = index*VFVoxel.c_VTSize;
				vol = Data.DataVT[indexVT];
				typ = Data.DataVT[indexVT+1];
				//OutputGameObject<AudioListener>();
				//OutputHeightGradAtPPos();
			}
		}
	}
	public bool bApplyWrite{
		get{ return false; }
		set{
			if(value == true){
				//int index = Data.IsHollow ? 0 : VFVoxelChunkData.OneIndexNoPrefix(xnpre,ynpre,znpre);
				Data.WriteVoxelAtIdx(xnpre-VoxelTerrainConstants._numVoxelsPrefix, 
				                      ynpre-VoxelTerrainConstants._numVoxelsPrefix, 
				                      znpre-VoxelTerrainConstants._numVoxelsPrefix, 
				                      new VFVoxel(vol, typ));
			}
		}
	}
	public bool bGenTrans{
		get{ return false; }
		set{
			if(value == true){
				if(TransvoxelGo != null)
				{
					GameObject.Destroy(TransvoxelGo);
					TransvoxelGo = null;
				}				
				if(_transvoxelMask != 0)
				{
					TransvoxelGoCreator.CreateTransvoxelGo(this, _transvoxelMask);
				}	
			}
		}
	}

	float GetHeight(int x, int z, int MaxHeight = 128)
	{
		byte vol   = VFVoxelTerrain.self.Voxels.Read(x,  MaxHeight,z).Volume;
		byte volDn = VFVoxelTerrain.self.Voxels.Read(x,--MaxHeight,z).Volume;
		while(volDn < 128 && MaxHeight > 1)
		{
			vol = volDn;
			volDn = VFVoxelTerrain.self.Voxels.Read(x,--MaxHeight,z).Volume;
		}
		float volDif = (float)(vol - volDn);
		float p = (128 - volDn) / volDif;
		return MaxHeight+p;
	}
    //void OutputHeightGradAtPPos()
    //{
    //    Player p = PlayerFactory.mMainPlayer;
    //    if(p == null)	return;

    //    Vector3 pos = p.transform.position;
    //    int ix = Mathf.FloorToInt(pos.x);
    //    int iz = Mathf.FloorToInt(pos.z);
    //    float fHeightXL = GetHeight(ix,iz-1);
    //    float fHeightXR = GetHeight(ix,iz+1);
    //    float fHeightZL = GetHeight(ix-1,iz);
    //    float fHeightZR = GetHeight(ix+1,iz);
    //    float sx = fHeightZR - fHeightZL;	// _iC is x
    //    float sz = fHeightXR - fHeightXL;	// _iR is z
    //    Debug.Log ("Height["+ix+","+iz+"]:"+pos.y+"/"+GetHeight(ix,iz)+"|Normal:["+(-sx)+","+2.0f+","+sz+"]");
    //}
	void OutputGameObject<T>() where T : Component
	{
		foreach (GameObject obj in UnityEngine.Object.FindObjectsOfType(typeof(GameObject)))
		{
			if (obj.GetComponent<T>() != null)
			{
				GameObject go = obj;
				string pathName = go.name;
				while(go.transform.parent != null)
				{
					go = go.transform.parent.gameObject;
					pathName = go.name+"|"+pathName;
				}
				Debug.Log(pathName);
			}
		}
	}
	#endif
}
