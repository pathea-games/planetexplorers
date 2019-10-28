using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ItemAsset;

public class EditBuilding : GLBehaviour 
{
	public BlockBuilding mBlockBuilding;
	
	public bool mSelected;
	
	bool mDrawGL;
	
	Dictionary<IntVector3, B45Block> mBlocks;
	
	Block45CurMan mB45Building;
	
	Dictionary<AssetReq, CreatItemInfo> mReqList = new Dictionary<AssetReq, CreatItemInfo>();
	
	void Awake()
	{
		m_Material = new Material( Shader.Find("Lines/Colored Blended") );
	    m_Material.hideFlags = HideFlags.HideAndDontSave;
	    m_Material.shader.hideFlags = HideFlags.HideAndDontSave;
		GlobalGLs.AddGL(this);
	}
	
	public void Init(BlockBuilding building, Block45CurMan perfab)
	{
		mBlockBuilding = building;
		
		Vector3 size;
		
		List<Vector3> npcPosition;
		
		List<CreatItemInfo> itemList;
		
		Dictionary<int, BuildingNpc> npcIdNum;
		
		mBlockBuilding.GetBuildingInfo(out size, out mBlocks, out npcPosition,out itemList,out npcIdNum);
		
		BoxCollider col = gameObject.AddComponent<BoxCollider>();
		col.center = 0.5f * size + 0.5f * Vector3.up;
		col.size = size;
		
		mB45Building = Instantiate(perfab) as Block45CurMan;
		mB45Building.transform.parent = transform;
		mB45Building.transform.localPosition = Vector3.zero;
		mB45Building.transform.localRotation = Quaternion.identity;
		mB45Building.transform.localScale = Vector3.one;
		
		Invoke("BuildBuilding", 0.5f);
		
		foreach(CreatItemInfo item in itemList)
		{
			ItemProto itemData = ItemProto.GetItemData(item.mItemId);
//			AssetBundleReq req = AssetBundlesMan.Instance.AddReq(itemData.m_ModelPath, Vector3.zero, Quaternion.identity);
//			req.ReqFinishWithReqHandler += OnSpawned;
//			mReqList[req] = item;
			
			GameObject go = Instantiate(Resources.Load(itemData.resourcePath)) as GameObject;
			go.transform.parent = transform;
			go.transform.transform.localPosition = item.mPos;
			go.transform.transform.localRotation = item.mRotation;
			go.transform.transform.localScale = Vector3.one;
		}
	}
	
	void BuildBuilding()
	{
		foreach(IntVector3 index in mBlocks.Keys)
			mB45Building.DataSource.SafeWrite(mBlocks[index], index.x, index.y, index.z, 0);
	}
	
	public void OnSpawned(GameObject go, AssetReq req)
	{
		if(mReqList.ContainsKey(req))
		{
			go.transform.parent = transform;
			go.transform.transform.localPosition = mReqList[req].mPos;
			go.transform.transform.localRotation = mReqList[req].mRotation;
			go.transform.transform.localScale = Vector3.one;
			mReqList.Remove(req);
		}
		else
		{
			Destroy(go);
		}
	}
	
	public bool DeletEnable { get { return mReqList.Count == 0; } }
	
	void OnMouseUpAsButton()
	{
		// Can select and delet the item only if creat item ended
		if(mReqList.Count == 0) 
			TownEditor.Instance.OnBuildingSelected(this);
	}
	
	void OnMouseDrag()
	{
		TownEditor.Instance.OnBuildingDrag(this);
	}
	
	void OnMouseOver()
	{
		mDrawGL = true;
	}
	
//	public void SetActive(bool active)
//	{
//		mSelected = active;
//	}
//	
	public override void OnGL ()
	{
		if(null != GetComponent<Collider>() && (mDrawGL || mSelected))
		{
			mDrawGL = false;
			Vector3 [] vert1 = new Vector3 [8];
			
			vert1[0] = new Vector3(GetComponent<Collider>().bounds.min.x, GetComponent<Collider>().bounds.min.y,GetComponent<Collider>().bounds.min.z);
			vert1[1] = new Vector3(GetComponent<Collider>().bounds.max.x, GetComponent<Collider>().bounds.min.y,GetComponent<Collider>().bounds.min.z);
			vert1[2] = new Vector3(GetComponent<Collider>().bounds.min.x, GetComponent<Collider>().bounds.max.y,GetComponent<Collider>().bounds.min.z);
			vert1[3] = new Vector3(GetComponent<Collider>().bounds.max.x, GetComponent<Collider>().bounds.max.y,GetComponent<Collider>().bounds.min.z);
			vert1[4] = new Vector3(GetComponent<Collider>().bounds.min.x, GetComponent<Collider>().bounds.min.y,GetComponent<Collider>().bounds.max.z);
			vert1[5] = new Vector3(GetComponent<Collider>().bounds.max.x, GetComponent<Collider>().bounds.min.y,GetComponent<Collider>().bounds.max.z);
			vert1[6] = new Vector3(GetComponent<Collider>().bounds.min.x, GetComponent<Collider>().bounds.max.y,GetComponent<Collider>().bounds.max.z);
			vert1[7] = new Vector3(GetComponent<Collider>().bounds.max.x, GetComponent<Collider>().bounds.max.y,GetComponent<Collider>().bounds.max.z);
		
			GL.PushMatrix();
		    // Set the current material
		    m_Material.SetPass(0);
	
			// Draw Lines -- twelve edges
			GL.Begin(GL.LINES);
			GL.Color(Color.yellow);
			
		    GL.Vertex3(vert1[0].x, vert1[0].y, vert1[0].z);
			GL.Vertex3(vert1[1].x, vert1[1].y, vert1[1].z);
		    GL.Vertex3(vert1[2].x, vert1[2].y, vert1[2].z);
	        GL.Vertex3(vert1[3].x, vert1[3].y, vert1[3].z);
		    GL.Vertex3(vert1[4].x, vert1[4].y, vert1[4].z);
	        GL.Vertex3(vert1[5].x, vert1[5].y, vert1[5].z);
		    GL.Vertex3(vert1[6].x, vert1[6].y, vert1[6].z);
	        GL.Vertex3(vert1[7].x, vert1[7].y, vert1[7].z);
		    GL.Vertex3(vert1[0].x, vert1[0].y, vert1[0].z);
		    GL.Vertex3(vert1[4].x, vert1[4].y, vert1[4].z);
			GL.Vertex3(vert1[1].x, vert1[1].y, vert1[1].z);
	        GL.Vertex3(vert1[5].x, vert1[5].y, vert1[5].z);
		    GL.Vertex3(vert1[2].x, vert1[2].y, vert1[2].z);
		    GL.Vertex3(vert1[6].x, vert1[6].y, vert1[6].z);
	        GL.Vertex3(vert1[3].x, vert1[3].y, vert1[3].z);
	        GL.Vertex3(vert1[7].x, vert1[7].y, vert1[7].z);
		    GL.Vertex3(vert1[0].x, vert1[0].y, vert1[0].z);
		    GL.Vertex3(vert1[2].x, vert1[2].y, vert1[2].z);
			GL.Vertex3(vert1[1].x, vert1[1].y, vert1[1].z);
	        GL.Vertex3(vert1[3].x, vert1[3].y, vert1[3].z);
		    GL.Vertex3(vert1[4].x, vert1[4].y, vert1[4].z);
		    GL.Vertex3(vert1[6].x, vert1[6].y, vert1[6].z);
	        GL.Vertex3(vert1[5].x, vert1[5].y, vert1[5].z);
	        GL.Vertex3(vert1[7].x, vert1[7].y, vert1[7].z);			
			
			GL.End();
				
			// Draw Quads -- six faces
	        GL.Begin(GL.QUADS);
			if(mSelected)
				GL.Color(new Color(0.0f,0f,0.2f,0.5f));
			else
				GL.Color(new Color(0.0f,0.2f,0f,0.5f));
			
		   	GL.Vertex3(vert1[0].x, vert1[0].y, vert1[0].z);
			GL.Vertex3(vert1[1].x, vert1[1].y, vert1[1].z);
	       	GL.Vertex3(vert1[3].x, vert1[3].y, vert1[3].z);
		   	GL.Vertex3(vert1[2].x, vert1[2].y, vert1[2].z);
		   	GL.Vertex3(vert1[4].x, vert1[4].y, vert1[4].z);
	       	GL.Vertex3(vert1[5].x, vert1[5].y, vert1[5].z);
	       	GL.Vertex3(vert1[7].x, vert1[7].y, vert1[7].z);
		   	GL.Vertex3(vert1[6].x, vert1[6].y, vert1[6].z);
	       	GL.Vertex3(vert1[3].x, vert1[3].y, vert1[3].z);
		   	GL.Vertex3(vert1[2].x, vert1[2].y, vert1[2].z);
		   	GL.Vertex3(vert1[6].x, vert1[6].y, vert1[6].z);
	       	GL.Vertex3(vert1[7].x, vert1[7].y, vert1[7].z);
		   	GL.Vertex3(vert1[0].x, vert1[0].y, vert1[0].z);
			GL.Vertex3(vert1[1].x, vert1[1].y, vert1[1].z);
	       	GL.Vertex3(vert1[5].x, vert1[5].y, vert1[5].z);
		   	GL.Vertex3(vert1[4].x, vert1[4].y, vert1[4].z);
			GL.Vertex3(vert1[1].x, vert1[1].y, vert1[1].z);
	       	GL.Vertex3(vert1[5].x, vert1[5].y, vert1[5].z);
	       	GL.Vertex3(vert1[7].x, vert1[7].y, vert1[7].z);
	       	GL.Vertex3(vert1[3].x, vert1[3].y, vert1[3].z);
		    GL.Vertex3(vert1[0].x, vert1[0].y, vert1[0].z);
		    GL.Vertex3(vert1[4].x, vert1[4].y, vert1[4].z);
		    GL.Vertex3(vert1[6].x, vert1[6].y, vert1[6].z);
		   	GL.Vertex3(vert1[2].x, vert1[2].y, vert1[2].z);
			
			GL.End();
			
	        // Restore camera's matrix.
	        GL.PopMatrix();
		}
	}
}
