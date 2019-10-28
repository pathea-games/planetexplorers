using UnityEngine;
using System.Collections;

public class BuildOpItem : GLBehaviour 
{
	public BuildingGui_N.OpType 	mType;  //0:Item	1:Npc
	public int 	mItemID;
	
	bool		mDrawGL = false;
	bool		mSelected = false;
	
	public bool Selected{get{return mSelected;}}
	
	void Awake()
	{
		m_Material = new Material(Shader.Find("Lines/Colored Blended"));
	    m_Material.hideFlags = HideFlags.HideAndDontSave;
	    m_Material.shader.hideFlags = HideFlags.HideAndDontSave;
		GlobalGLs.AddGL(this);
	}
	
	void OnMouseUpAsButton()
	{
		BuildingGui_N.Instance.OnBuildOpItemSel(this);
	}
	
	void OnMouseOver()
	{
		mDrawGL = true;
	}
	
	public void SetActive(bool active)
	{
		mSelected = active;
	}
	
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
