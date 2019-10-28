using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BSGLNearBlockIndicator : GLBehaviour 
{
	public static Vector3 InvalidPos { get { return new Vector3 (-100,-100,-100); } }
	public Vector3 m_Center = InvalidPos;
	public int m_Expand = 4;
	public Gradient m_BoxColors;

	//Material _LineMaterial = null;

	void Start ()
	{
		GlobalGLs.AddGL(this);
		
	}

	void Update ()
	{
		BSMath.DrawTarget dtar;
		
		if ( BSMath.RayCastDrawTarget(Camera.main.ScreenPointToRay( Input.mousePosition), BuildingMan.Blocks, out dtar,  BSMath.MC_ISO_VALUE, true, BuildingMan.Datas))
		{
			m_Center = new Vector3(Mathf.FloorToInt(dtar.snapto.x * Block45Constants._scaleInverted) * Block45Constants._scale,
			                       Mathf.FloorToInt(dtar.snapto.y * Block45Constants._scaleInverted) * Block45Constants._scale,
			                       Mathf.FloorToInt(dtar.snapto.z * Block45Constants._scaleInverted) * Block45Constants._scale);
			
		}
		else
		{
			m_Center = InvalidPos;
		}
	}

	void OnDisable()
	{
		m_Center = InvalidPos;
	}

	public override void OnGL ()
	{
		if (!enabled || !gameObject.activeInHierarchy)
			return;
		
		if (BSInput.s_MouseOnUI)
			return;

//		if (_LineMaterial == null)
//		{
//			_LineMaterial = new Material(Shader.Find("Lines/Colored Blended"));
//			_LineMaterial.hideFlags = HideFlags.HideAndDontSave;
//			_LineMaterial.shader.hideFlags = HideFlags.HideAndDontSave;
//		}

		if (m_Material == null)
		{
			m_Material = new Material(Shader.Find("Unlit/Transparent Colored"));
			m_Material.hideFlags = HideFlags.HideAndDontSave;
			m_Material.shader.hideFlags = HideFlags.HideAndDontSave;
		}

		GL.PushMatrix();
		
//		_LineMaterial.SetPass(0);
		m_Material.SetPass(0);

		// Edge
		float expand = m_Expand * Block45Constants._scale;

		List<Bounds> boxes = new List<Bounds>();
		List<Color>	 line_colors = new List<Color>();

		for ( float x = -expand; x <= expand; x+= Block45Constants._scale)
		{
			for ( float y = -expand; y <= expand; y+= Block45Constants._scale)
			{
				for ( float z = -expand; z <= expand; z+= Block45Constants._scale)
				{
					Vector3 pos = new Vector3(x+m_Center.x, y+m_Center.y, z+m_Center.z);

					B45Block block = Block45Man.self.DataSource.SafeRead(Mathf.FloorToInt(pos.x * Block45Constants._scaleInverted), 
					                                                     Mathf.FloorToInt(pos.y * Block45Constants._scaleInverted),
 									                                     Mathf.FloorToInt(pos.z * Block45Constants._scaleInverted));
				
//					bool draw = false;
					bool contain_block = false;
					if (block.blockType != 0 || block.materialType != 0)
						contain_block = true;

					VFVoxel voxel = VFVoxelTerrain.self.Voxels.SafeRead(Mathf.FloorToInt(pos.x + 0.5f), Mathf.FloorToInt(pos.y + 0.5f), Mathf.FloorToInt(pos.z + 0.5f));

					Bounds box = new Bounds (Vector3.zero, Vector3.zero);

					float sv = (float)(Mathf.Max(Mathf.Abs(x), Mathf.Abs(y), Mathf.Abs(z))) / (float)(expand);
					Color lc = m_BoxColors.Evaluate(sv);


					if (voxel.Volume > 1)
					{
						box.SetMinMax(new Vector3(pos.x, pos.y , pos.z), 
						              new Vector3(pos.x + 0.5f, pos.y + 0.5f, pos.z + 0.5f));

						// Shrink
						float shrink = 0.02f;
						if (Camera.main != null)
						{
							float dist_min = (Camera.main.transform.position - box.min).magnitude;
							float dist_max = (Camera.main.transform.position - box.max).magnitude;
							
							dist_max = dist_max > dist_min ? dist_max : dist_min;
							
							shrink = Mathf.Clamp(dist_max * 0.002f, 0.02f, 0.1f);
						}


						box.min -= new Vector3(shrink, shrink, shrink);
						box.max += new Vector3(shrink, shrink, shrink);

						Color vc = m_BoxColors.Evaluate((float)voxel.Volume / 255.0f);
						lc.r = vc.r;
						lc.g = vc.g;
						lc.b = vc.b;
//						draw = true;


						boxes.Add(box);
						line_colors.Add(lc);
					}
					else if (contain_block)
					{
						box.SetMinMax(new Vector3(pos.x, pos.y, pos.z), 
						              new Vector3(pos.x + 0.5f, pos.y + 0.5f, pos.z + 0.5f));

						// Shrink
						float shrink = 0.02f;
						if (Camera.main != null)
						{
							float dist_min = (Camera.main.transform.position - box.min).magnitude;
							float dist_max = (Camera.main.transform.position - box.max).magnitude;
							
							dist_max = dist_max > dist_min ? dist_max : dist_min;
							
							shrink = Mathf.Clamp(dist_max * 0.002f, 0.02f, 0.1f);
						}


						box.min -= new Vector3(shrink, shrink, shrink);
						box.max += new Vector3(shrink, shrink, shrink);

						boxes.Add(box);
						line_colors.Add(lc);
//						draw = true;
					}



				}
			}
		}

		// Shrink

		// Draw line
		GL.Begin(GL.LINES);
		for (int i = 0; i < boxes.Count; i++)
		{
			Color lc = line_colors[i];
			lc.a *= 1.5f;
			if (lc.a> 0.01f)
			{
				Bounds box = boxes[i];
				GL.Color(line_colors[i]);
				GL.Vertex3( box.min.x, box.min.y, box.min.z );
				GL.Vertex3( box.max.x, box.min.y, box.min.z );
				GL.Vertex3( box.min.x, box.min.y, box.max.z );
				GL.Vertex3( box.max.x, box.min.y, box.max.z );
				GL.Vertex3( box.min.x, box.max.y, box.min.z );
				GL.Vertex3( box.max.x, box.max.y, box.min.z );
				GL.Vertex3( box.min.x, box.max.y, box.max.z );
				GL.Vertex3( box.max.x, box.max.y, box.max.z );
				
				GL.Vertex3( box.min.x, box.min.y, box.min.z );
				GL.Vertex3( box.min.x, box.max.y, box.min.z );
				GL.Vertex3( box.min.x, box.min.y, box.max.z );
				GL.Vertex3( box.min.x, box.max.y, box.max.z );
				GL.Vertex3( box.max.x, box.min.y, box.min.z );
				GL.Vertex3( box.max.x, box.max.y, box.min.z );
				GL.Vertex3( box.max.x, box.min.y, box.max.z );
				GL.Vertex3( box.max.x, box.max.y, box.max.z );
				
				GL.Vertex3( box.min.x, box.min.y, box.min.z );
				GL.Vertex3( box.min.x, box.min.y, box.max.z );
				GL.Vertex3( box.min.x, box.max.y, box.min.z );
				GL.Vertex3( box.min.x, box.max.y, box.max.z );
				GL.Vertex3( box.max.x, box.min.y, box.min.z );
				GL.Vertex3( box.max.x, box.min.y, box.max.z );
				GL.Vertex3( box.max.x, box.max.y, box.min.z );
				GL.Vertex3( box.max.x, box.max.y, box.max.z );
			}
		}
		GL.End();

		// Draw quad
		GL.Begin(GL.QUADS);
		for (int i = 0; i < boxes.Count; i++)
		{
			if (line_colors[i].a > 0.01f)
			{
				Bounds box = boxes[i];
				GL.Color(line_colors[i]);
				GL.Vertex3( box.min.x, box.min.y, box.min.z );
				GL.Vertex3( box.min.x, box.min.y, box.max.z );
				GL.Vertex3( box.min.x, box.max.y, box.max.z );
				GL.Vertex3( box.min.x, box.max.y, box.min.z );
							
				GL.Vertex3( box.max.x, box.min.y, box.min.z );
				GL.Vertex3( box.max.x, box.min.y, box.max.z );
				GL.Vertex3( box.max.x, box.max.y, box.max.z );
				GL.Vertex3( box.max.x, box.max.y, box.min.z );
							
				GL.Vertex3( box.min.x, box.min.y, box.min.z );
				GL.Vertex3( box.min.x, box.min.y, box.max.z );
				GL.Vertex3( box.max.x, box.min.y, box.max.z );
				GL.Vertex3( box.max.x, box.min.y, box.min.z );
				
				GL.Vertex3( box.min.x, box.max.y, box.min.z );
				GL.Vertex3( box.min.x, box.max.y, box.max.z );
				GL.Vertex3( box.max.x, box.max.y, box.max.z );
				GL.Vertex3( box.max.x, box.max.y, box.min.z );
				
				GL.Vertex3( box.min.x, box.min.y, box.min.z );
				GL.Vertex3( box.min.x, box.max.y, box.min.z );
				GL.Vertex3( box.max.x, box.max.y, box.min.z );
				GL.Vertex3( box.max.x, box.min.y, box.min.z );
				
				GL.Vertex3( box.min.x, box.min.y, box.max.z );
				GL.Vertex3( box.min.x, box.max.y, box.max.z );
				GL.Vertex3( box.max.x, box.max.y, box.max.z );
				GL.Vertex3( box.max.x, box.min.y, box.max.z );
			}
		}
		GL.End();

		GL.PopMatrix();
	}
}
