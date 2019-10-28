using UnityEngine;
using System.Collections;

public class LSubTerrEditor : MonoBehaviour
{
	public GUISkin GSkin = null;
	public int BeginIndex = 0;
	public int EndIndex = 0;
	public float Density = 3.0F;
	public float Radius = 5F;
	public float MinWidthScale = 1;
	public float MaxWidthScale = 1;
	public float MinHeightScale = 1;
	public float MaxHeightScale = 1;
	public bool Eraser = false;
	public int EraserHeightUBB = 2;
	public int EraserHeightLBB = -3;
	public int EraserFilterBegin = 0;
	public int EraserFilterEnd = 1023;
	private Vector3 Focus = Vector3.zero;
	private Vector3 Normal = Vector3.zero;
	private Vector2 [] TreePositions;
	private int TreeCount = 0;
	public const int MAXPOSITION = 2048;
	// Use this for initialization
	void Start ()
	{
		TreePositions = new Vector2 [MAXPOSITION];
		RandOnce();
	}
	
	private int lastBeginIndex = 0;
	// Update is called once per frame
	void Update ()
	{
		if ( BeginIndex != lastBeginIndex && Mathf.Abs(BeginIndex - EndIndex) > 3 )
			EndIndex = BeginIndex;
		lastBeginIndex = BeginIndex;
		
		if ( Input.GetAxis("Mouse ScrollWheel") > 0 )
		{
			Radius *= 1.15f;
			if ( Radius > 32F )
				Radius = 32F;
			RandOnce();
		}
		else if ( Input.GetAxis("Mouse ScrollWheel") < 0 )
		{
			Radius *= 0.82f;
			if ( Radius < 1F )
				Radius = 1F;
			RandOnce();
		}
		
		if ( Input.GetKeyDown(KeyCode.KeypadPlus) || Input.GetKeyDown(KeyCode.Plus) || Input.GetKeyDown(KeyCode.Equals) )
		{
			Density *= 0.9f;
			RandOnce();
		}
		if ( Input.GetKeyDown(KeyCode.Minus) || Input.GetKeyDown(KeyCode.KeypadMinus) )
		{
			Density *= 1.18f;
			RandOnce();
		}
		float MinDensity = 2.0f;
		if ( Density > 36 )
			Density = 36;
		if ( Density < MinDensity )
			Density = MinDensity;
		
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit rch;
		if ( Physics.Raycast(ray, out rch, 500, 1 << Pathea.Layer.VFVoxelTerrain) )
		{
			Focus = rch.point;
			Normal = rch.normal;
		}
		else
		{
			Focus = Vector3.zero;
			Normal = Vector3.zero;
		}
		if ( Input.GetKeyDown(KeyCode.X) )
		{
			RandOnce();
		}
		if ( Input.GetKeyDown(KeyCode.R) )
		{
			Eraser = !Eraser;
		}
		
		bool processing = false;
		for ( int l = 0; l < LSubTerrainMgr.Instance.LayerCreators.Length; ++l )
		{
			if ( LSubTerrainMgr.Instance.LayerCreators[l].bProcessing )
			{
				processing = true;
				break;
			}
		}
		if ( Input.GetKeyDown(KeyCode.N) && !processing )
		{
			if ( Eraser )
			{
				for ( float x = Mathf.Floor(Focus.x - Radius); x <= Mathf.Ceil(Focus.x + Radius); x += 1.0f )
				{
					for ( float z = Mathf.Floor(Focus.z - Radius); z <= Mathf.Ceil(Focus.z + Radius); z += 1.0f )
					{
						if ( Normal.y > 0 )
						{
							Vector3 pos = new Vector3(x, Focus.y+Radius, z);
							if ( Physics.Raycast(pos, Vector3.down, out rch, 2*Radius, 1 << Pathea.Layer.VFVoxelTerrain) )
							{
								if ( (rch.point - Focus).magnitude <= Radius + 0.5f )
								{
									for ( float h = rch.point.y + EraserHeightLBB; h <= rch.point.y + EraserHeightUBB; h += 1 )
										LSubTerrainMgr.DeleteTreesAtPos(new IntVector3(rch.point.x, h, rch.point.z), EraserFilterBegin, EraserFilterEnd);
								}
							}
						}
						else
						{
							Vector3 pos = new Vector3(x, Focus.y-Radius, z);
							if ( Physics.Raycast(pos, Vector3.up, out rch, 2*Radius, 1 << Pathea.Layer.VFVoxelTerrain) )
							{
								if ( (rch.point - Focus).magnitude <= Radius + 0.5f )
								{
									for ( float h = rch.point.y + EraserHeightLBB; h <= rch.point.y + EraserHeightUBB; h += 1 )
										LSubTerrainMgr.DeleteTreesAtPos(new IntVector3(rch.point.x, h, rch.point.z), EraserFilterBegin, EraserFilterEnd);
								}
							}
						}
					}
				}
			}
			else
			{
				for ( int i = 0; i < TreeCount; ++i )
				{
					int prototype = (int)((Random.value) * (Mathf.Abs((float)EndIndex - (float)BeginIndex) + 0.99999F) + Mathf.Min(BeginIndex, EndIndex));
					float widthscale = Random.value * (MaxWidthScale - MinWidthScale) + MinWidthScale;
					float heightscale = Random.value * (MaxHeightScale - MinHeightScale) + MinHeightScale;
					if ( widthscale < 0.1f )
						widthscale = 0.1f;
					if ( widthscale > 3.0f )
						widthscale = 3.0f;
					if ( heightscale < 0.1f )
						heightscale = 0.1f;
					if ( heightscale > 3.0f )
						heightscale = 3.0f;
					if ( Normal.y > 0 )
					{
						Vector3 pos = Focus + Vector3.up * Radius;
						pos.x += TreePositions[i].x;
						pos.z += TreePositions[i].y;
						if ( Physics.Raycast(pos, Vector3.down, out rch, 2*Radius, 1 << Pathea.Layer.VFVoxelTerrain) )
						{
							LSubTerrainMgr.AddTree(rch.point, prototype, widthscale, heightscale);
						}
					}
					else
					{
						Vector3 pos = Focus - Vector3.up * Radius;
						pos.x += TreePositions[i].x;
						pos.z += TreePositions[i].y;
						if ( Physics.Raycast(pos, Vector3.up, out rch, 2*Radius, 1 << Pathea.Layer.VFVoxelTerrain) )
						{
							LSubTerrainMgr.AddTree(rch.point, prototype, widthscale, heightscale);
						}
					}
				}
			}
			LSubTerrainMgr.RefreshAllLayerTerrains();
			LSubTerrainMgr.CacheAllNodes();
		}
	}
	
	private void RandOnce()
	{
		TreePositions[0] = Vector2.zero;
		TreeCount = 1;
		for ( int i = 1; i < MAXPOSITION; ++i )
		{
			bool conflict = true;
			int x = 0;
			while (conflict && x < 64)
			{
				TreePositions[i] = Random.insideUnitCircle * Radius;
				conflict = false;
				for ( int j = 0; j < i; j++ )
				{
					if ( (TreePositions[i] - TreePositions[j]).magnitude < Density )
					{
						conflict = true;
						break;
					}
				}
				x++;
			}
			if ( x > 60 )
			{
				break;
			}
			else
			{
				TreeCount++;
			}
		}
	}
	
	public void DoGL()
	{
		if ( Focus.sqrMagnitude < 1 )
			return;
		
		Material _LineMaterial = WhiteCat.PEVCConfig.instance.lineMaterial;
	    _LineMaterial.hideFlags = HideFlags.HideAndDontSave;
	    _LineMaterial.shader.hideFlags = HideFlags.HideAndDontSave;
		
		// Save camera's matrix.
		GL.PushMatrix();
		
	    // Set the current material
	    _LineMaterial.SetPass(0);
		
		for ( float deg = 0; deg < 359.5F; deg += 4F )
		{
//			int idx = Mathf.RoundToInt(deg);
			Vector3 A = Focus;
			A.x += Radius * Mathf.Cos(deg*Mathf.Deg2Rad);
			A.z += Radius * Mathf.Sin(deg*Mathf.Deg2Rad);
			Vector3 B = Focus;
			B.x += Radius * Mathf.Cos((deg+4)*Mathf.Deg2Rad);
			B.z += Radius * Mathf.Sin((deg+4)*Mathf.Deg2Rad);
			RaycastHit rch;
			if ( Normal.y > 0 )
			{
				if ( Physics.Raycast(A + Vector3.up * Radius, Vector3.down, out rch, 2*Radius, 1 << Pathea.Layer.VFVoxelTerrain) )
				{
					A = rch.point;
					A += Vector3.up * 0.1F;
					if ( Physics.Raycast(B + Vector3.up * Radius, Vector3.down , out rch, 2*Radius, 1 << Pathea.Layer.VFVoxelTerrain) )
					{
						B = rch.point;
						B += Vector3.up * 0.1F;
						GL.Begin(GL.LINES);
						GL.Color(Eraser ? new Color(0.3f,0.5f,1.0f,0.15f):new Color(0.2f,0.5f,0.0f,0.15f));
				    	GL.Vertex3(A.x, A.y, A.z);
						GL.Vertex3(B.x, B.y, B.z);
						GL.End();
			        	GL.Begin(GL.QUADS);
			        	GL.Color(Eraser ? new Color(0.00f,0.00f,0.0f,0.0f):new Color(0.03f,0.1f,0.0f,0.15f));
				   		GL.Vertex3(A.x, A.y, A.z);
						GL.Vertex3(B.x, B.y, B.z);
			       		GL.Vertex3(B.x, B.y + 0.7f, B.z);
				   		GL.Vertex3(A.x, A.y + 0.7f, A.z);
						GL.End();
					}
				}
			}
			else
			{
				if ( Physics.Raycast(A + Vector3.down * Radius, Vector3.up, out rch, 2*Radius, 1 << Pathea.Layer.VFVoxelTerrain) )
				{
					A = rch.point;
					A -= Vector3.up * 0.1F;
					if ( Physics.Raycast(B + Vector3.down * Radius, Vector3.up , out rch, 2*Radius, 1 << Pathea.Layer.VFVoxelTerrain) )
					{
						B = rch.point;
						B -= Vector3.up * 0.1F;
						GL.Begin(GL.LINES);
						GL.Color(Eraser ? new Color(0.3f,0.5f,1.0f,0.15f):new Color(0.2f,0.5f,0.0f,0.15f));
				    	GL.Vertex3(A.x, A.y, A.z);
						GL.Vertex3(B.x, B.y, B.z);
						GL.End();
			        	GL.Begin(GL.QUADS);
			        	GL.Color(Eraser ? new Color(0.00f,0.00f,0.0f,0.0f):new Color(0.03f,0.1f,0.0f,0.15f));
				   		GL.Vertex3(A.x, A.y, A.z);
						GL.Vertex3(B.x, B.y, B.z);
			       		GL.Vertex3(B.x, B.y - 0.7f, B.z);
				   		GL.Vertex3(A.x, A.y - 0.7f, A.z);
						GL.End();
					}
				}
			}
		}
		
		if ( !Eraser )
		{
			for ( int i = 0; i < TreeCount; ++i )
			{
				RaycastHit rch;
				if ( Normal.y > 0 )
				{
					Vector3 pos = Focus + Vector3.up * Radius;
					pos.x += TreePositions[i].x;
					pos.z += TreePositions[i].y;
					if ( Physics.Raycast(pos, Vector3.down, out rch, 2*Radius, 1 << Pathea.Layer.VFVoxelTerrain) )
					{
						Vector3 drawpos = rch.point + Vector3.up*0.3F;
						GL.Begin(GL.LINES);
						GL.Color(new Color(0.4f,1.0f,0.0f,0.5f));
					    GL.Vertex3(drawpos.x - 0.5f, drawpos.y, drawpos.z);
						GL.Vertex3(drawpos.x + 0.5f, drawpos.y, drawpos.z);
					    GL.Vertex3(drawpos.x, drawpos.y, drawpos.z - 0.5f);
						GL.Vertex3(drawpos.x, drawpos.y, drawpos.z + 0.5f);
					    GL.Vertex3(drawpos.x, drawpos.y - 0.5f, drawpos.z);
						GL.Vertex3(drawpos.x, drawpos.y + 2.0f, drawpos.z);
						GL.Vertex3(drawpos.x, drawpos.y + 2.0f, drawpos.z);
						GL.Vertex3(drawpos.x + 0.2f, drawpos.y + 1.5f, drawpos.z);
						GL.Vertex3(drawpos.x, drawpos.y + 2.0f, drawpos.z);
						GL.Vertex3(drawpos.x - 0.2f, drawpos.y + 1.5f, drawpos.z);
						GL.Vertex3(drawpos.x, drawpos.y + 2.0f, drawpos.z);
						GL.Vertex3(drawpos.x, drawpos.y + 1.5f, drawpos.z + 0.2f);
						GL.Vertex3(drawpos.x, drawpos.y + 2.0f, drawpos.z);
						GL.Vertex3(drawpos.x, drawpos.y + 1.5f, drawpos.z - 0.2f);
						GL.End();
					}
				}
				else
				{
					Vector3 pos = Focus - Vector3.up * Radius;
					pos.x += TreePositions[i].x;
					pos.z += TreePositions[i].y;
					if ( Physics.Raycast(pos, Vector3.up, out rch, 2*Radius, 1 << Pathea.Layer.VFVoxelTerrain) )
					{
						Vector3 drawpos = rch.point - Vector3.up*0.3F;
						GL.Begin(GL.LINES);
						GL.Color(new Color(0.4f,1.0f,0.0f,0.5f));
					    GL.Vertex3(drawpos.x - 0.5f, drawpos.y, drawpos.z);
						GL.Vertex3(drawpos.x + 0.5f, drawpos.y, drawpos.z);
					    GL.Vertex3(drawpos.x, drawpos.y, drawpos.z - 0.5f);
						GL.Vertex3(drawpos.x, drawpos.y, drawpos.z + 0.5f);
					    GL.Vertex3(drawpos.x, drawpos.y - 2.0f, drawpos.z);
						GL.Vertex3(drawpos.x, drawpos.y + 0.5f, drawpos.z);
					    GL.Vertex3(drawpos.x, drawpos.y - 2.0f, drawpos.z);
						GL.Vertex3(drawpos.x + 0.2f, drawpos.y - 1.5f, drawpos.z);
						GL.Vertex3(drawpos.x, drawpos.y - 2.0f, drawpos.z);
						GL.Vertex3(drawpos.x - 0.2f, drawpos.y - 1.5f, drawpos.z);
						GL.Vertex3(drawpos.x, drawpos.y - 2.0f, drawpos.z);
						GL.Vertex3(drawpos.x, drawpos.y - 1.5f, drawpos.z + 0.2f);
						GL.Vertex3(drawpos.x, drawpos.y - 2.0f, drawpos.z);
						GL.Vertex3(drawpos.x, drawpos.y - 1.5f, drawpos.z - 0.2f);
						GL.End();
					}
				}
			}
		}
        // Restore camera's matrix.
        GL.PopMatrix();
	}
}
