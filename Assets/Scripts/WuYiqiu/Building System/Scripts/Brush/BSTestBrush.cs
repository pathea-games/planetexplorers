using UnityEngine;
using System.Collections;

public class BSTestBrush : BSBrush 
{

	public GameObject Gizmo;

	public bool IsVoxel = true;

	BSMath.DrawTarget m_Target;

	BSGLNearVoxelIndicator m_Indicator;
	BSGLNearBlockIndicator m_BlockIndicator;

	public int BrushSize = 1;

	void Awake ()
	{
		m_Indicator = gameObject.GetComponent<BSGLNearVoxelIndicator>();
		m_BlockIndicator = gameObject.GetComponent<BSGLNearBlockIndicator>();
	}

	void Update ()
	{
		if (VFVoxelTerrain.self == null)
			return;


		if (IsVoxel)
		{
//			Gizmo.transform.localScale = new Vector3(1, 1, 1);
			Gizmo.transform.localScale = new Vector3(BrushSize, BrushSize, BrushSize);
			m_Indicator.enabled = true;
			m_BlockIndicator.enabled = false;
		}
		else
		{
			m_Indicator.enabled = false;
			m_BlockIndicator.enabled = true;
			Gizmo.transform.localScale = new Vector3(0.5f * BrushSize, 0.5f * BrushSize, 0.5f * BrushSize);
			m_BlockIndicator.m_Expand = 1 + BrushSize ;
		}


		IBSDataSource ds = null; //= IsVoxel ? BuildingMan.Voxels : BuildingMan.Blocks;
		BSVoxel voxel;
		if (IsVoxel)
		{
			voxel = new BSVoxel();
			voxel.value0 = 255;
			voxel.value1 = 2;
			ds = BuildingMan.Voxels;
		}
		else
		{
			voxel = new BSVoxel();
			voxel.value0 = B45Block.MakeBlockType(1,0);
			voxel.value1 = 0;
			ds = BuildingMan.Blocks;
		}


//		if (BSMath.RayCastDrawTarget(Camera.main.ScreenPointToRay( Input.mousePosition), out m_Target, BSMath.MC_ISO_VALUE, IsVoxel))
		if (BSMath.RayCastDrawTarget(Camera.main.ScreenPointToRay( Input.mousePosition),
		                             ds, out m_Target, BSMath.MC_ISO_VALUE, false, BuildingMan.Datas))
		{

			IntVector3 realPos = new IntVector3( Mathf.FloorToInt(m_Target.cursor.x * ds.ScaleInverted), 
			                                    Mathf.FloorToInt( m_Target.cursor.y  * ds.ScaleInverted), 
			                                    Mathf.FloorToInt( m_Target.cursor.z * ds.ScaleInverted));
			
			float offset = Mathf.FloorToInt(BrushSize * 0.5f) * ds.Scale;
			//int xMultiplier = 0, yMultiplier = 0, zMultiplier = 0;
			
			
			Vector3 cursor = realPos.ToVector3() * ds.Scale - new Vector3(offset, offset, offset);
			int sign = BrushSize % 2 == 0? 1 : 0; 
			
			if (offset != 0)
			{
				Vector3 offset_v = Vector3.zero;
				
				if (m_Target.rch.normal.x > 0)
					offset_v.x += offset;
				else if (m_Target.rch.normal.x < 0)
					offset_v.x -= (offset - ds.Scale * sign);
				else
					offset_v.x = 0;
				
				if (m_Target.rch.normal.y > 0)
					offset_v.y += offset;
				else if (m_Target.rch.normal.y < 0)
					offset_v.y -= (offset - ds.Scale * sign);
				
				else
					offset_v.y = 0;
				
				if (m_Target.rch.normal.z > 0)
					offset_v.z += offset;
				else if (m_Target.rch.normal.z < 0)
					offset_v.z -=  (offset - ds.Scale * sign);
				else
					offset_v.z = 0;
				
				cursor += offset_v;
			}
			
			
			
			Gizmo.transform.position = cursor + ds.Offset;
			
			if (Input.GetMouseButtonDown(0))
			{
				int cnt = BrushSize;
				
				for (int x = 0; x < cnt; x++)
				{
					for (int y = 0; y < cnt; y++)
					{
						for (int z = 0; z < cnt; z++)
						{
							
							IntVector3 inpos = new IntVector3(Mathf.FloorToInt( cursor.x * ds.ScaleInverted) + x,
							                                  Mathf.FloorToInt( cursor.y * ds.ScaleInverted) + y,
							                                  Mathf.FloorToInt( cursor.z * ds.ScaleInverted) + z);
							
							ds.Write(voxel, inpos.x, inpos.y, inpos.z);
						}
					}
				}
				
			}
//			if (IsVoxel)
//			{
////				Gizmo.transform.position = m_Target.cursor - Vector3.one * 0.5f;
////
////				if (Input.GetMouseButtonDown(0))
////				{
////					VFVoxel voxel = new VFVoxel(255, 2);
////					IntVector3 cursor = m_Target.iCursor;
////					VFVoxelTerrain.self.Voxels.Write(cursor.x, cursor.y, cursor.z, voxel);
////				}
//
//
//			}
//			else
//			{
//
//				IntVector3 realPos = new IntVector3( Mathf.FloorToInt(m_Target.cursor.x * Block45Constants._scaleInverted), 
//				                                    Mathf.FloorToInt( m_Target.cursor.y  * Block45Constants._scaleInverted), 
//				                                    Mathf.FloorToInt( m_Target.cursor.z * Block45Constants._scaleInverted));
//
//				float offset = Mathf.FloorToInt(BrushSize * 0.5f) * Block45Constants._scale;
//				int xMultiplier = 0, yMultiplier = 0, zMultiplier = 0;
//
//
//				Vector3 cursor = realPos.ToVector3() * Block45Constants._scale - new Vector3(offset, offset, offset);
//				int sign = BrushSize % 2 == 0? 1 : 0; 
//
//				if (offset != 0)
//				{
//					Vector3 offset_v = Vector3.zero;
//		
//					if (m_Target.rch.normal.x > 0)
//						offset_v.x += offset;
//					else if (m_Target.rch.normal.x < 0)
//						offset_v.x -= (offset - Block45Constants._scale * sign);
//					else
//						offset_v.x = 0;
//
//					if (m_Target.rch.normal.y > 0)
//						offset_v.y += offset;
//					else if (m_Target.rch.normal.y < 0)
//						offset_v.y -= (offset - Block45Constants._scale * sign);
//
//					else
//						offset_v.y = 0;
//
//					if (m_Target.rch.normal.z > 0)
//						offset_v.z += offset;
//					else if (m_Target.rch.normal.z < 0)
//						offset_v.z -=  (offset - Block45Constants._scale * sign);
//					else
//						offset_v.z = 0;
//
//					cursor += offset_v;
//				}
//
//
//
//				Gizmo.transform.position = cursor ;
//				
//				if (Input.GetMouseButtonDown(0))
//				{
//					int cnt = BrushSize;
//
//					for (int x = 0; x < cnt; x++)
//					{
//						for (int y = 0; y < cnt; y++)
//						{
//							for (int z = 0; z < cnt; z++)
//							{
//								B45Block block = new B45Block(B45Block.MakeBlockType(1,0), 0);
//								IntVector3 inpos = new IntVector3(Mathf.FloorToInt( cursor.x * Block45Constants._scaleInverted) + x,
//								                                  Mathf.FloorToInt( cursor.y * Block45Constants._scaleInverted) + y,
//								                                  Mathf.FloorToInt( cursor.z * Block45Constants._scaleInverted) + z);
//								Block45Man.self.DataSource.Write(block, inpos.x, 
//								                                 inpos.y, 
//								                                 inpos.z);
//							}
//						}
//					}
//
//				}
//			}
		}


	}
	

	protected override void Do ()
	{

	}
}
