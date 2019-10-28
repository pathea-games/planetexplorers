using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BSB45DiagonalBrush : BSFreeSizeBrush 
{
	public bool upVSeal = true;
	public bool upBSeal = false;

	public int m_Rot = 0;

	[SerializeField] BSB45Computer m_Computer;

	[SerializeField] Material meshMat;
	
	public override Bounds brushBound {
		get {
			Bounds bound = new Bounds();
			bound.min = Min;
			bound.max = Max;
			return bound;
		}
	}

	protected override bool ExtraAdjust ()
	{
		if (dataSource != BuildingMan.Blocks)
		{
			if (gizmoCube.gameObject.activeSelf)
				gizmoCube.gameObject.SetActive(false);

			return false;
		}

        // Ratate the gizmo ?  Only block can rotate
        if (pattern.type == EBSVoxelType.Block)
        {
            if (Input.GetKeyDown(KeyCode.T))
            {
                m_Rot = ++m_Rot > 3 ? 0 : m_Rot;
                _prevSize = Vector3.zero;
            }
        }

        return true;
	}

	public override bool CanDrawGL ()
	{
		if (dataSource == BuildingMan.Blocks)
			return true;

		return false;

	}

	private Vector3 _prevSize = new Vector3(0, 0, 0);
	

	protected override void AdjustHeightExtraDo (ECoordPlane drag_plane)
	{
		if (m_Rot == 0 || m_Rot == 2)
		{
			Vector3 begin = m_Begin * dataSource.ScaleInverted;
			Vector3 end   = m_End * dataSource.ScaleInverted;

			if (drag_plane == ECoordPlane.XZ)
			{
				Vector3 size =  CalcuSizeY(begin, end, ECalcuHeight.XDir);
				
				end = begin + size;
				m_End = new Vector3(end.x * dataSource.Scale,
				                    end.y * dataSource.Scale,
				                    end.z * dataSource.Scale);


			}
			else if (drag_plane == ECoordPlane.ZY)
			{
				Vector3 size =  CalcuSizeX(begin, end, ECalcuHeight.XDir);
				
				end = begin + size;
				m_End = new Vector3(end.x * dataSource.Scale,
				                    end.y * dataSource.Scale,
				                    end.z * dataSource.Scale);
			}
		}
		else if (m_Rot == 1 || m_Rot == 3)
		{
			Vector3 begin = m_Begin * dataSource.ScaleInverted;
			Vector3 end   = m_End * dataSource.ScaleInverted;

			if (drag_plane == ECoordPlane.XZ)
			{
				Vector3 size =  CalcuSizeY(begin, end, ECalcuHeight.ZDir);
				
				end = begin + size;
				m_End = new Vector3(end.x * dataSource.Scale,
				                    end.y * dataSource.Scale,
				                    end.z * dataSource.Scale);
			}
			else if (drag_plane == ECoordPlane.XY)
			{
				Vector3 size =  CalcuSizeX(begin, end, ECalcuHeight.ZDir);
				
				end = begin + size;
				m_End = new Vector3(end.x * dataSource.Scale,
				                    end.y * dataSource.Scale,
				                    end.z * dataSource.Scale);
			}
		}
	}

	protected override void DragPlaneExtraDo (ECoordPlane drag_plane)
	{
		Vector3 begin = m_Begin * dataSource.ScaleInverted;
		Vector3 end   = m_End * dataSource.ScaleInverted;

		if (m_Rot == 0 || m_Rot == 2)
		{
			if (drag_plane == ECoordPlane.XY)
			{

				Vector3 size =  CalcuSizeXY(begin, end);

				end = begin + size;
				m_End = new Vector3(end.x * dataSource.Scale,
				                    end.y * dataSource.Scale,
				                    end.z * dataSource.Scale);
			}
		}
		else if (m_Rot == 1 || m_Rot == 3)
		{
			if (drag_plane == ECoordPlane.ZY)
			{
				
				Vector3 size =  CalcuSizeYZ(begin, end);
				
				end = begin + size;
				m_End = new Vector3(end.x * dataSource.Scale,
				                    end.y * dataSource.Scale,
				                    end.z * dataSource.Scale);
			}
		}
	}

	static bool EqualsZero (float v)
	{
		return v > -0.0001f && v < 0.0001f ;
	}


    //int _oldRot = 0;
	protected override void AfterDo ()
	{
		if (m_Phase != EPhase.Free)
		{
			Vector3 size = Size;

			if (!Vector3.Equals(size, _prevSize))
			{

				m_Computer.ClearDataDS();

				DestroyPreviewMesh();

				_prevSize = size;

				Debug.LogWarning(" Prev Size :" + _prevSize.ToString() + " Size :" + size.ToString());

				// X Positive
				if (m_Rot == 0)
				{
					IntVector3 isize = CorrectSizeXY(size);
					int xMin = 0;
					int xMax = (int)isize.x;
					int zMin = 0;
					int zMax = (int)isize.z;
//					int yMin = 0;
					int yMax = (int) isize.y;
//					int xMin = 0;
//					int xMax = (int)size.x;
//					int zMin = 0;
//					int zMax = (int)size.z;
//					int yMin = 0;
//					int yMax = (int) size.y;


//					int s = 0;
//					if (size.x >= size.y)
//					{
//						s = Mathf.RoundToInt (size.x / size.y);
//						xMax -= 1;
//						yMax = Mathf.FloorToInt(xMax / s);
//						xMax = s * yMax;
//					}
//					else
//					{
//						s =  Mathf.RoundToInt ( size.y / size.x);
//						yMax -=  1;
//						xMax = Mathf.FloorToInt(yMax / s);
//						yMax = s * xMax;
//					}



					if (xMax < 1 || yMax  < 1 || zMax <= 0)
					{
						return;
					}


					for (int z = zMin; z < zMax; z++)
					{
						IntVector3 up = new IntVector3(xMax, yMax, z);
						IntVector3 dn = new IntVector3(xMin, 0, z);

						ComputerPreview(up, dn, upVSeal, upBSeal, materialType);
						m_Computer.RebuildMesh();

					}
				}
				// Z Postive
				else if (m_Rot == 1)
				{
					IntVector3 isize = CorrectSizeZY(size);
					int xMin = 0;
					int xMax = isize.x;
					int zMin = 0;
					int zMax = isize.z;
//					int yMin = 0;
					int yMax = isize.y;



				    if (zMax < 1 || yMax  < 1 || xMax <= 0)
					{
						return;
					}

					for (int x = xMin; x < xMax; x++)
					{
						IntVector3 up = new IntVector3(x, yMax, zMax);
						IntVector3 dn = new IntVector3(x, 0, zMin);
						
						ComputerPreview(up, dn, upVSeal, upBSeal, materialType);
						m_Computer.RebuildMesh();
					}
				}
				// X Negative
				else if (m_Rot == 2)
				{
					IntVector3 isize = CorrectSizeXY(size);
					int xMin = 0;
					int xMax = (int)isize.x;
					int zMin = 0;
					int zMax = (int)isize.z;
//					int yMin = 0;
					int yMax = (int) isize.y;

					if (xMax < 1 || yMax  < 1 || zMax <= 0)
					{
						return;
					}

					for (int z = zMin; z < zMax; z++)
					{
						IntVector3 up = new IntVector3(xMin, yMax, z);
						IntVector3 dn = new IntVector3(xMax, 0, z);
						
						ComputerPreview(up, dn, upVSeal, upBSeal, materialType);
						m_Computer.RebuildMesh();
						
					}
				}
				// Z Negative
				else if (m_Rot == 3)
				{
					IntVector3 isize = CorrectSizeZY(size);
					int xMin = 0;
					int xMax = isize.x;
					int zMin = 0;
					int zMax = isize.z;
//					int yMin = 0;
					int yMax = isize.y;

					if (zMax < 1 || yMax  < 1 || xMax <= 0)
					{
						return;
					}
					
					for (int x = xMin; x < xMax; x++)
					{
						IntVector3 up = new IntVector3(x, yMax, zMin);
						IntVector3 dn = new IntVector3(x, 0, zMax);
						
						ComputerPreview(up, dn, upVSeal, upBSeal, materialType);
						m_Computer.RebuildMesh();
					}
				}
					
			}
		}
		else
			DestroyPreviewMesh();

		
		
		if (meshMat != null)
		{
			meshMat.renderQueue = 3000;
			Renderer[] renderers = m_Computer.GetComponentsInChildren<Renderer>(true);
			foreach (Renderer r in renderers)
			{
				if (meshMat != null)
					r.material = meshMat;
			}
		}


	}


	void DestroyPreviewMesh()
	{
		//Destroy mesh
		for (int i = 0; i < m_Computer.transform.childCount; i++)
		{
			Destroy(m_Computer.transform.GetChild(i).gameObject);
		}
	}
	
	protected override void Do ()
	{

		Vector3 min = Min;
//		Vector3 max = Max;

		Vector3 size = Size;

		List<BSVoxel> new_voxels = new List<BSVoxel>();
		List<IntVector3> indexes = new List<IntVector3>();
		List<BSVoxel> old_voxels = new List<BSVoxel>();
		Dictionary<IntVector3, int>  refVoxelMap = new Dictionary<IntVector3, int>();
		
		// x positive direction
		if (m_Rot == 0)
		{
			IntVector3 isize = CorrectSizeXY(size);
			int xMin = Mathf.FloorToInt(min.x * BSBlock45Data.s_ScaleInverted);
			int xMax = xMin + isize.x;
			int zMin = Mathf.FloorToInt(min.z * BSBlock45Data.s_ScaleInverted);
			int zMax = zMin + isize.z;
			int yMin = Mathf.FloorToInt( min.y * BSBlock45Data.s_ScaleInverted);
			int yMax = yMin + isize.y;

			if(xMax < xMin + 1 || yMax  < yMin + 1 || zMax <= zMin)
			{
				return;
			}



			for (int z = zMin; z < zMax; z++)
			{
				IntVector3 up = new IntVector3(xMax, yMax, z);
				IntVector3 dn = new IntVector3(xMin, yMin, z);

				ApplyBevel2_10(up, dn, upVSeal, upBSeal, BuildingMan.Blocks, materialType, new_voxels, old_voxels, indexes, refVoxelMap);
			}
		}
		// z positive direction
		else if (m_Rot == 1)
		{
			IntVector3 isize = CorrectSizeZY(size);
			int xMin = Mathf.FloorToInt(min.x * BSBlock45Data.s_ScaleInverted);
			int xMax = xMin + isize.x;
			int zMin = Mathf.FloorToInt(min.z * BSBlock45Data.s_ScaleInverted);
			int zMax = zMin + isize.z;
			int yMin = Mathf.FloorToInt( min.y * BSBlock45Data.s_ScaleInverted);
			int yMax = yMin + isize.y;

			if ( (xMax < xMin + 1 || yMax  < yMin + 1 || zMax <= zMin))
				return;

			for (int x = xMin; x < xMax; x++)
			{
				IntVector3 up = new IntVector3(x, yMax, zMax);
				IntVector3 dn = new IntVector3(x, yMin, zMin);
				
				ApplyBevel2_10(up, dn, upVSeal, upBSeal, BuildingMan.Blocks, materialType, new_voxels, old_voxels, indexes, refVoxelMap);
			}
		}
		// x negative direction
		else if (m_Rot == 2)
		{
			IntVector3 isize = CorrectSizeXY(size);
			int xMin = Mathf.FloorToInt(min.x * BSBlock45Data.s_ScaleInverted);
			int xMax = xMin + isize.x;
			int zMin = Mathf.FloorToInt(min.z * BSBlock45Data.s_ScaleInverted);
			int zMax = zMin + isize.z;
			int yMin = Mathf.FloorToInt( min.y * BSBlock45Data.s_ScaleInverted);
			int yMax = yMin + isize.y;


			if ((xMax < xMin + 1 || yMax  < yMin + 1 || zMax <= zMin))
				return;


			for (int z = zMin; z < zMax; z++)
			{
				IntVector3 up = new IntVector3(xMin, yMax, z);
				IntVector3 dn = new IntVector3(xMax, yMin, z);
				
				ApplyBevel2_10(up, dn, upVSeal, upBSeal, BuildingMan.Blocks, materialType, new_voxels, old_voxels, indexes, refVoxelMap);
			}
		}
		// z negative direction
		else if (m_Rot == 3)
		{
			IntVector3 isize = CorrectSizeZY(size);
			int xMin = Mathf.FloorToInt(min.x * BSBlock45Data.s_ScaleInverted);
			int xMax = xMin + isize.x;
			int zMin = Mathf.FloorToInt(min.z * BSBlock45Data.s_ScaleInverted);
			int zMax = zMin + isize.z;
			int yMin = Mathf.FloorToInt( min.y * BSBlock45Data.s_ScaleInverted);
			int yMax = yMin + isize.y;

			if ( (xMax < xMin + 1 || yMax  < yMin + 1 || zMax <= zMin))
				return;

			for (int x = xMin; x < xMax; x++)
			{
				IntVector3 up = new IntVector3(x, yMax, zMin);
				IntVector3 dn = new IntVector3(x, yMin, zMax);
				
				ApplyBevel2_10(up, dn, upVSeal, upBSeal, BuildingMan.Blocks, materialType, new_voxels, old_voxels, indexes, refVoxelMap);
			}
		}


		// Extra Extendable
		FindExtraExtendableVoxels(dataSource, new_voxels, old_voxels, indexes, refVoxelMap);

		// Action
		if (indexes.Count != 0)
		{
			BSAction action = new BSAction();
			
			BSVoxelModify modify = new BSVoxelModify(indexes.ToArray(), old_voxels.ToArray(), new_voxels.ToArray(), dataSource, EBSBrushMode.Add);
			
			action.AddModify(modify);
			
			if (action.Do())
				BSHistory.AddAction(action);
		}
		
	}
	
	#region CALCULATE_SIZE_FUNC
	enum ECalcuHeight
	{
		XDir,
		ZDir
	}
	
	Vector3 CalcuSizeY (IntVector3 begin, IntVector3 end, ECalcuHeight type)
	{
		Vector3 size = new Vector3(end.x - begin.x,
		                           end.y - begin.y,
		                           end.z - begin.z);
		
		Vector3 abs_size = new Vector3(Mathf.Abs(size.x), Mathf.Abs(size.y), Mathf.Abs(size.z));
		
		float abs_l = 0.0f;
		if (type == ECalcuHeight.XDir)
		{
			abs_l = abs_size.x;
		}
		else if (type == ECalcuHeight.ZDir)
		{
			abs_l = abs_size.z;
		}
		else
			return size;
		
		float s = Mathf.Abs(abs_l / size.y);
		
		if (s < 1)
		{
			if (s < 0.3334f)
			{
				size.y = Mathf.Sign(size.y) *  abs_l * 3;
			}
			else if (s > 0.3333f && s <= 0.5f)
			{
				size.y = Mathf.Sign(size.y) * abs_l * 2;
			}
			else
			{
				size.y = Mathf.Sign(size.y) * abs_l;
			}
		}
		else if (s > 1)
		{
			if (s < 3 )
			{
				float v = abs_l % 2;
				if (EqualsZero(v) )
				{
					size.y = Mathf.Sign(size.y) * abs_l * 0.5f;
				}
				else
				{
					v = abs_l% 3;
					if (EqualsZero(v))
					{
						size.y = Mathf.Sign(size.y) * abs_l / 3;
					}
					else
						size.y = 1;
				}
			}
			else if (s >= 3)
			{
				float v = abs_l % 3;
				if (EqualsZero(v))
				{
					size.y = Mathf.Sign(size.y) * abs_l / 3;
				}
				else
					size.y = 1;
			}
		}
		
		
		return size;
		
	}
	
	Vector3 CalcuSizeXDirOfY (IntVector3 begin, IntVector3 end)
	{
		Vector3 size = new Vector3(end.x - begin.x,
		                           end.y - begin.y,
		                           end.z - begin.z);
		
		Vector3 abs_size = new Vector3(Mathf.Abs(size.x), Mathf.Abs(size.y), Mathf.Abs(size.z));
		float s = Mathf.Abs((float)size.x / size.y);
		
		if (s < 1)
		{
			if (s < 0.3334f)
			{
				size.y = Mathf.Sign(size.y) * ((int) abs_size.x * 3);
			}
			else if (s > 0.3333f && s <= 0.5f)
			{
				size.y = Mathf.Sign(size.y) * ((int) abs_size.x * 2);
			}
			else
			{
				size.y = Mathf.Sign(size.y) * (abs_size.x);
			}
		}
		else if (s > 1)
		{
			if (s < 3 )
			{
				float v = abs_size.x % 2;
				if (v > -0.0001f && v < 0.0001f )
				{
					size.y = Mathf.Sign(size.y) * abs_size.x * 0.5f;
				}
				else
				{
					v = abs_size.x % 3;
					if (v > -0.0001f && v < 0.0001f )
					{
						size.y = Mathf.Sign(size.y) * abs_size.x / 3;
					}
					else
						size.y = 1;
				}
			}
			else if (s >= 3)
			{
				float v = abs_size.x % 3;
				if (v > -0.0001f && v < 0.0001f )
				{
					size.y = Mathf.Sign(size.y) * abs_size.x / 3;
				}
				else
				{
					size.y = 1;
				}
			}
		}
		
		return size;
		
	}
	
	
	Vector3 CalcuSizeX (IntVector3 begin, IntVector3 end, ECalcuHeight type)
	{
		Vector3 size = new Vector3(end.x - begin.x,
		                           end.y - begin.y,
		                           end.z - begin.z);
		
		Vector3 abs_size = new Vector3(Mathf.Abs(size.x), Mathf.Abs(size.y), Mathf.Abs(size.z));
		
		float abs_l = 0.0f;
		float sign = 1.0f;
		if (type == ECalcuHeight.XDir)
		{
			abs_l = abs_size.x;
			sign = Mathf.Sign(size.x);
		}
		else if (type == ECalcuHeight.ZDir)
		{
			abs_l = abs_size.z;
			sign = Mathf.Sign(size.z);
		}
		else
			return size;
		
		float s = Mathf.Abs(abs_l / size.y);
		
		if (s < 1)
		{
			if (s < 0.5f)
			{
				float v = abs_size.y % 3;
				if (EqualsZero(v) )
				{
					abs_l = sign * abs_size.y / 3;
				}
				else
					abs_l = 1;
			}
			else if ( s >= 0.5f)
			{
				float v = abs_size.y % 2;
				if (EqualsZero(v))
				{
					abs_l = sign * abs_size.y * 0.5f;
				}
				else
				{
					v = abs_size.y % 3;
					if (EqualsZero(v) )
					{
						abs_l = sign * abs_size.y / 3;
					}
					else
						abs_l = 1;
				}
			}
		}
		else if (s > 1)
		{
			if (s < 2)
				abs_l = sign * abs_size.y;
			else if (s >= 2 && s < 3 )
				abs_l =  sign * abs_size.y * 2;
			else if (s >= 3)
				abs_l = sign * abs_size.y * 3;
		}
		
		if (type == ECalcuHeight.XDir)
			size.x = abs_l;
		else if (type == ECalcuHeight.ZDir)
		{
			size.z = abs_l;
		}
		
		return size;
		
	}
	
	Vector3  CalcuSizeXY (IntVector3 begin, IntVector3 end)
	{
		Vector3 size = new Vector3(end.x - begin.x,
		                           end.y - begin.y,
		                           end.z - begin.z);
		
		Vector3 abs_size = new Vector3(Mathf.Abs(size.x), Mathf.Abs(size.y), Mathf.Abs(size.z));
		float s = Mathf.Abs((float)size.x / size.y);
		
		if (s < 1)
		{
			if (s < 0.3334f)
			{
				size.y = Mathf.Sign(size.y) * ((int) abs_size.x * 3);
			}
			else if (s > 0.3333f && s <= 0.5f)
			{
				size.y = Mathf.Sign(size.y) * ((int) abs_size.x * 2);
			}
			else
			{
				size.y = Mathf.Sign(size.y) * (abs_size.x);
			}
		}
		else if (s > 1)
		{
			if (s < 2)
			{
				size.x = Mathf.Sign(size.x) * abs_size.y;
			}
			else if (s >= 2 && s < 3 )
			{
				size.x =  Mathf.Sign(size.x) * abs_size.y * 2;
				
			}
			else if (s >= 3)
			{
				size.x =  Mathf.Sign(size.x) * abs_size.y * 3;
			}
		}
		
		
		return size;
	}
	
	Vector3  CalcuSizeYZ (IntVector3 begin, IntVector3 end)
	{
		Vector3 size = new Vector3(end.x - begin.x,
		                           end.y - begin.y,
		                           end.z - begin.z);
		
		Vector3 abs_size = new Vector3(Mathf.Abs(size.x), Mathf.Abs(size.y), Mathf.Abs(size.z));
		float s = Mathf.Abs((float)size.z / size.y);
		
		if (s < 1)
		{
			if (s < 0.3334f)
			{
				size.y = Mathf.Sign(size.y) * ((int) abs_size.z * 3);
			}
			else if (s > 0.3333f && s <= 0.5f)
			{
				size.y = Mathf.Sign(size.y) * ((int) abs_size.z * 2);
			}
			else
			{
				size.y = Mathf.Sign(size.y) * (abs_size.z);
			}
		}
		else if (s > 1)
		{
			if (s < 2)
			{
				size.z = Mathf.Sign(size.z) * abs_size.y;
			}
			else if (s >= 2 && s < 3 )
			{
				size.z =  Mathf.Sign(size.z) * abs_size.y * 2;
				
			}
			else if (s >= 3)
			{
				size.z =  Mathf.Sign(size.z) * abs_size.y * 3;
			}
		}
		
		return size;
	}
	
	
	IntVector3 CorrectSizeXY (Vector3 size)
	{
		IntVector3 isize = new IntVector3((int)size.x, (int)size.y, (int)size.z);
		
//		int s = 0;
//		if (size.x >= size.y)
//		{
//			s = (int) (size.x / size.y);
//			isize.x -= 1;
//			isize.y = Mathf.FloorToInt(size.x / s);
//			isize.x = s * isize.y;
//		}
//		else
//		{
//			s =  (int) ( size.y / size.x);
//			isize.y -=  1;
//			isize.x = Mathf.FloorToInt(isize.y / s);
//			isize.y = s * isize.x;
//		}

		int s = 0;
		if (size.x >= size.y)
		{
			s = Mathf.RoundToInt (size.x / size.y);
			isize.x -= 1;
			isize.y = Mathf.FloorToInt(isize.x / s);
			isize.x = s * isize.y;
		}
		else
		{
			s =  Mathf.RoundToInt ( size.y / size.x);
			isize.y -=  1;
			isize.x = Mathf.FloorToInt(isize.y / s);
			isize.y = s * isize.x;
		}
		
		return isize;
	}
	
	
	IntVector3 CorrectSizeZY (Vector3 size)
	{
		IntVector3 isize = new IntVector3((int)size.x, (int)size.y, (int)size.z);
		
		int s = 0;
		if (size.z >= size.y)
		{
			s =  (int)(size.z / size.y);
			isize.z -= 1;
			isize.y = Mathf.FloorToInt(isize.z / s);
			isize.z = s * isize.y;
		}
		else
		{
			s =  (int)( size.y / size.z);
			isize.y -=  1;
			isize.z = Mathf.FloorToInt(isize.y / s);
			isize.y = s * isize.z;
		}
		
		return isize;
	}
	
	#endregion


	#region APPLY_THE_CHANGE
	public static void MakeExtendableBS(int primitiveType, int rotation, int extendDir, int length, int materialType, out BSVoxel b0, out BSVoxel b1)
	{
		B45Block block0, block1;
		B45Block.MakeExtendableBlock(primitiveType, rotation, extendDir, length, materialType, out block0, out block1);
		b0 = new BSVoxel(block0);
		b1 = new BSVoxel(block1);
	}

	public static void MakeExtendableBS(int primitiveType, int rotation, int extendDir, int length, int materialType, out B45Block b0, out B45Block b1)
	{
		B45Block block0, block1;
		B45Block.MakeExtendableBlock(primitiveType, rotation, extendDir, length, materialType, out block0, out block1);
		b0 = block0;
		b1 = block1;
	}

	void WriteBSVoxel(IBSDataSource ds, List<BSVoxel> new_voxels, List<BSVoxel> old_voxels, List<IntVector3> indexes, Dictionary<IntVector3, int> refMap ,BSVoxel voxel, int x, int y, int z)
	{
		//				ds.SafeWrite(voxel, x, y, z);
		// Old voxel 
		IntVector3 idx = new IntVector3(x, y, z);
		BSVoxel old = ds.SafeRead(idx.x, idx.y, idx.z);		
		old_voxels.Add(old);
		new_voxels.Add(voxel);
		indexes.Add(idx);
		refMap[idx] = 0;
	}
	// should be 2 multiply cur coord
	public void ApplyBevel2_10(IntVector3 up, IntVector3 dn, bool upVSeal, bool dnVSeal, IBSDataSource ds, byte matType, 
	                           List<BSVoxel> new_voxels, List<BSVoxel> old_voxels, List<IntVector3> indexes, Dictionary<IntVector3, int> refMap)
	{

		int dy = up.y - dn.y;
		int dx = up.x - dn.x;
		int dz = up.z - dn.z;
		int adx = Mathf.Abs(dx);
		int adz = Mathf.Abs(dz);

		int t0 = 2, t1 = 10;

		int l = 0;
		int n = 0;
		IntVector3 beg = dn;
//		int idxXZ = 0;
		int maxXZ = adx;
		int xstep = dx >  0 ? 1 : -1;
		int zstep = 0;
		int xext0 = 0;
		int zext0 = 0;
		int xext1 = 1;
		int zext1 = 0;
		int xext2 = 2;
		int zext2 = 0;
		int r = up.x > dn.x ? 2 : 0;
		int e = B45Block.EBX;
		if(adx < adz)
		{
//			idxXZ = 1;
			maxXZ = adz;
			xstep = 0;
			zstep = dz >  0 ? 1 : -1;
			xext1 = 0;
			zext1 = 1;
			xext2 = 0;
			zext2 = 2;
			r = up.z > dn.z ? 1 : 3;
			e = B45Block.EBZ;
		}
		if(dy < 2*maxXZ && maxXZ < 2*dy)
		{
			n = Mathf.Min(dy, maxXZ);
			BSVoxel bOrg = new BSVoxel((byte)((t0<<2)|r), matType);
			BSVoxel bFlp = new BSVoxel((byte)((t1<<2)|((r+2)&3)), matType);
			if(dnVSeal)
			{
				WriteBSVoxel(ds, new_voxels, old_voxels, indexes, refMap, bFlp, beg.x, beg.y-1, beg.z);
			}
			for(int i = 0; i < n; i++)
			{
				WriteBSVoxel(ds, new_voxels, old_voxels, indexes, refMap, bOrg, beg.x+i*xstep, beg.y+i, beg.z+i*zstep);
				WriteBSVoxel(ds, new_voxels, old_voxels, indexes, refMap, bFlp, beg.x+(i+1)*xstep, beg.y+i, beg.z+(i+1)*zstep);
			}
			if(upVSeal)
			{
				WriteBSVoxel(ds, new_voxels, old_voxels, indexes, refMap, bOrg, beg.x+n*xstep, beg.y+n, beg.z+n*zstep);
			}
		}
		else
		{
			BSVoxel bOrg0, bOrg1, bOrg2;
			BSVoxel bFlp0, bFlp1, bFlp2;
			if(dy > maxXZ)
			{
				e = B45Block.EBY;
				l = dy/maxXZ; if(l > 3) l = 3;
				n = maxXZ;
				MakeExtendableBS(t0, r,       e, l, matType, out bOrg0, out bOrg1);
				MakeExtendableBS(t1, (r+2)&3, e, l, matType, out bFlp0, out bFlp1);
				bOrg2 = bOrg1;	// dumb block voxel for original block
				bFlp2 = bFlp1;	// dumb block voxel for flipping block
				if(dnVSeal)
				{
					WriteBSVoxel(ds, new_voxels, old_voxels, indexes, refMap, bFlp0, beg.x, beg.y-l+0, beg.z);
					WriteBSVoxel(ds, new_voxels, old_voxels, indexes, refMap, bFlp1, beg.x, beg.y-l+1, beg.z);
					if(l == 3)
					{
						WriteBSVoxel(ds, new_voxels, old_voxels, indexes, refMap, bFlp2, beg.x, beg.y-l+2, beg.z);
					}
				}
				for(int i = 0; i < n; i++)
				{
					WriteBSVoxel(ds, new_voxels, old_voxels, indexes, refMap, bOrg0, beg.x+i*xstep, beg.y+i*l+0, beg.z+i*zstep);
					WriteBSVoxel(ds, new_voxels, old_voxels, indexes, refMap, bOrg1, beg.x+i*xstep, beg.y+i*l+1, beg.z+i*zstep);
					if(l == 3)
					{
						WriteBSVoxel(ds, new_voxels, old_voxels, indexes, refMap, bOrg2, beg.x+i*xstep, beg.y+i*l+2, beg.z+i*zstep);
					}

					WriteBSVoxel(ds, new_voxels, old_voxels, indexes, refMap, bFlp0, beg.x+(i+1)*xstep, beg.y+i*l+0, beg.z+(i+1)*zstep);
					WriteBSVoxel(ds, new_voxels, old_voxels, indexes, refMap, bFlp1, beg.x+(i+1)*xstep, beg.y+i*l+1, beg.z+(i+1)*zstep);
					if(l == 3)	
					{
						WriteBSVoxel(ds, new_voxels, old_voxels, indexes, refMap, bFlp2, beg.x+(i+1)*xstep, beg.y+i*l+2, beg.z+(i+1)*zstep);
					}
				}
				if(upVSeal)
				{
					WriteBSVoxel(ds, new_voxels, old_voxels, indexes, refMap, bOrg0, beg.x+n*xstep, beg.y+n*l+0, beg.z+n*zstep);
					WriteBSVoxel(ds, new_voxels, old_voxels, indexes, refMap, bOrg1, beg.x+n*xstep, beg.y+n*l+1, beg.z+n*zstep);
					if(l == 3)	
					{
						WriteBSVoxel(ds, new_voxels, old_voxels, indexes, refMap, bOrg2, beg.x+n*xstep, beg.y+n*l+2, beg.z+n*zstep);
					}
				}			
			}
			else
			{
				l = maxXZ/dy; if(l > 3) l = 3;
				n = dy;
				MakeExtendableBS(t0, r,       e, l, matType, out bOrg0, out bOrg1);
				MakeExtendableBS(t1, (r+2)&3, e, l, matType, out bFlp0, out bFlp1);
				bOrg2 = bOrg1;	// dumb block voxel for original block
				bFlp2 = bFlp1;	// dumb block voxel for flipping block
				if(dnVSeal)
				{
					WriteBSVoxel(ds, new_voxels, old_voxels, indexes, refMap, bFlp0, beg.x+xext0, beg.y-1, beg.z+zext0);
					WriteBSVoxel(ds, new_voxels, old_voxels, indexes, refMap, bFlp1, beg.x+xext1, beg.y-1, beg.z+zext1);					            
					if(l == 3)	
					{
						WriteBSVoxel(ds, new_voxels, old_voxels, indexes, refMap, bFlp2, beg.x+xext2, beg.y-1, beg.z+zext2);
					}
				}
				for(int i = 0; i < dy; i++)
				{
					WriteBSVoxel(ds, new_voxels, old_voxels, indexes, refMap, bOrg0, beg.x+i*l*xstep+xext0, beg.y+i, beg.z+i*l*zstep+zext0);
					WriteBSVoxel(ds, new_voxels, old_voxels, indexes, refMap, bOrg1, beg.x+i*l*xstep+xext1, beg.y+i, beg.z+i*l*zstep+zext1);
					if(l == 3)	
					{
						WriteBSVoxel(ds, new_voxels, old_voxels, indexes, refMap, bOrg2, beg.x+i*l*xstep+xext2, beg.y+i, beg.z+i*l*zstep+zext2);
					}

					WriteBSVoxel(ds, new_voxels, old_voxels, indexes, refMap, bFlp0, beg.x+(i+1)*l*xstep+xext0, beg.y+i, beg.z+(i+1)*l*zstep+zext0);
					WriteBSVoxel(ds, new_voxels, old_voxels, indexes, refMap, bFlp1, beg.x+(i+1)*l*xstep+xext1, beg.y+i, beg.z+(i+1)*l*zstep+zext1);
					if(l == 3)
					{
						WriteBSVoxel(ds, new_voxels, old_voxels, indexes, refMap, bFlp2, beg.x+(i+1)*l*xstep+xext2, beg.y+i, beg.z+(i+1)*l*zstep+zext2);
					}
				}
				if(upVSeal)
				{
					WriteBSVoxel(ds, new_voxels, old_voxels, indexes, refMap, bOrg0, beg.x+n*l*xstep+xext0, beg.y+n, beg.z+n*l*zstep+zext0);
					WriteBSVoxel(ds, new_voxels, old_voxels, indexes, refMap, bOrg1, beg.x+n*l*xstep+xext1, beg.y+n, beg.z+n*l*zstep+zext1);

					if(l == 3)	
					{
						WriteBSVoxel(ds, new_voxels, old_voxels, indexes, refMap, bOrg2, beg.x+n*l*xstep+xext2, beg.y+n, beg.z+n*l*zstep+zext2);
					}
				}
			}
		}


	}
	
	public void ComputerPreview(IntVector3 up, IntVector3 dn, bool upVSeal, bool dnVSeal, byte matType)
	{
		int dy = up.y - dn.y;
		int dx = up.x - dn.x;
		int dz = up.z - dn.z;
		int adx = Mathf.Abs(dx);
		int adz = Mathf.Abs(dz);
		
		int t0 = 2, t1 = 10;
		
		int l = 0;
		int n = 0;
		IntVector3 beg = dn;
//		int idxXZ = 0;
		int maxXZ = adx;
		int xstep = dx >  0 ? 1 : -1;
		int zstep = 0;
		int xext0 = 0;
		int zext0 = 0;
		int xext1 = 1;
		int zext1 = 0;
		int xext2 = 2;
		int zext2 = 0;
		int r = up.x > dn.x ? 2 : 0;
		int e = B45Block.EBX;
		if(adx < adz)
		{
//			idxXZ = 1;
			maxXZ = adz;
			xstep = 0;
			zstep = dz >  0 ? 1 : -1;
			xext1 = 0;
			zext1 = 1;
			xext2 = 0;
			zext2 = 2;
			r = up.z > dn.z ? 1 : 3;
			e = B45Block.EBZ;
		}
		if(dy < 2*maxXZ && maxXZ < 2*dy)
		{
			n = Mathf.Min(dy, maxXZ);
			B45Block bOrg = new B45Block((byte)((t0<<2)|r), matType);
			B45Block bFlp = new B45Block((byte)((t1<<2)|((r+2)&3)), matType);

			if(dnVSeal)
			{
				m_Computer.AlterBlockInBuild(beg.x, beg.y-1, beg.z, bFlp);
			}
			for(int i = 0; i < n; i++)
			{
				m_Computer.AlterBlockInBuild(beg.x+i*xstep, beg.y+i, beg.z+i*zstep, bOrg);
				m_Computer.AlterBlockInBuild(beg.x+(i+1)*xstep, beg.y+i, beg.z+(i+1)*zstep, bFlp);

			}
			if(upVSeal)
			{
				m_Computer.AlterBlockInBuild(beg.x+n*xstep, beg.y+n, beg.z+n*zstep, bOrg);
			}
		}
		else
		{
			B45Block bOrg0, bOrg1, bOrg2;
			B45Block bFlp0, bFlp1, bFlp2;
			if(dy > maxXZ)
			{
				e = B45Block.EBY;
				l = dy/maxXZ; if(l > 3) l = 3;
				n = maxXZ;
				MakeExtendableBS(t0, r,       e, l, matType, out bOrg0, out bOrg1);
				MakeExtendableBS(t1, (r+2)&3, e, l, matType, out bFlp0, out bFlp1);
				bOrg2 = bOrg1;	// dumb block voxel for original block
				bFlp2 = bFlp1;	// dumb block voxel for flipping block
				if(dnVSeal)
				{
					m_Computer.AlterBlockInBuild(beg.x, beg.y-l+0, beg.z, bFlp0);
					m_Computer.AlterBlockInBuild(beg.x, beg.y-l+1, beg.z, bFlp1);
					if(l == 3)	
						m_Computer.AlterBlockInBuild(beg.x, beg.y-l+2, beg.z, bFlp2);
				}
				for(int i = 0; i < n; i++)
				{
					m_Computer.AlterBlockInBuild(beg.x+i*xstep, beg.y+i*l+0, beg.z+i*zstep, bOrg0);
					m_Computer.AlterBlockInBuild(beg.x+i*xstep, beg.y+i*l+1, beg.z+i*zstep, bOrg1);
					if(l == 3)	
						m_Computer.AlterBlockInBuild(beg.x+i*xstep, beg.y+i*l+2, beg.z+i*zstep, bOrg2);
					
					m_Computer.AlterBlockInBuild(beg.x+(i+1)*xstep, beg.y+i*l+0, beg.z+(i+1)*zstep, bFlp0);
					m_Computer.AlterBlockInBuild(beg.x+(i+1)*xstep, beg.y+i*l+1, beg.z+(i+1)*zstep, bFlp1);
					if(l == 3)	
						m_Computer.AlterBlockInBuild(beg.x+(i+1)*xstep, beg.y+i*l+2, beg.z+(i+1)*zstep, bFlp2);
				}
				if(upVSeal)
				{
					m_Computer.AlterBlockInBuild(beg.x+n*xstep, beg.y+n*l+0, beg.z+n*zstep, bOrg0);
					m_Computer.AlterBlockInBuild(beg.x+n*xstep, beg.y+n*l+1, beg.z+n*zstep, bOrg1);
					if(l == 3)	
						m_Computer.AlterBlockInBuild(beg.x+n*xstep, beg.y+n*l+2, beg.z+n*zstep, bOrg2);
				}
			}
			else
			{
				l = maxXZ/dy; if(l > 3) l = 3;
				n = dy;
				MakeExtendableBS(t0, r,       e, l, matType, out bOrg0, out bOrg1);
				MakeExtendableBS(t1, (r+2)&3, e, l, matType, out bFlp0, out bFlp1);
				bOrg2 = bOrg1;	// dumb block voxel for original block
				bFlp2 = bFlp1;	// dumb block voxel for flipping block
				if(dnVSeal)
				{
					m_Computer.AlterBlockInBuild(beg.x+xext0, beg.y-1, beg.z+zext0, bFlp0);
					m_Computer.AlterBlockInBuild(beg.x+xext1, beg.y-1, beg.z+zext1, bFlp1);
					if(l == 3)	
						m_Computer.AlterBlockInBuild(beg.x+xext2, beg.y-1, beg.z+zext2, bFlp2);
				}
				for(int i = 0; i < dy; i++)
				{
					m_Computer.AlterBlockInBuild(beg.x+i*l*xstep+xext0, beg.y+i, beg.z+i*l*zstep+zext0, bOrg0);
					m_Computer.AlterBlockInBuild(beg.x+i*l*xstep+xext1, beg.y+i, beg.z+i*l*zstep+zext1, bOrg1);
					if(l == 3)	
						m_Computer.AlterBlockInBuild(beg.x+i*l*xstep+xext2, beg.y+i, beg.z+i*l*zstep+zext2, bOrg2);
					
					m_Computer.AlterBlockInBuild(beg.x+(i+1)*l*xstep+xext0, beg.y+i, beg.z+(i+1)*l*zstep+zext0, bFlp0);
					m_Computer.AlterBlockInBuild(beg.x+(i+1)*l*xstep+xext1, beg.y+i, beg.z+(i+1)*l*zstep+zext1, bFlp1);
					if(l == 3)	
						m_Computer.AlterBlockInBuild(beg.x+(i+1)*l*xstep+xext2, beg.y+i, beg.z+(i+1)*l*zstep+zext2, bFlp2);
				}
				if(upVSeal)
				{
					m_Computer.AlterBlockInBuild(beg.x+n*l*xstep+xext0, beg.y+n, beg.z+n*l*zstep+zext0, bOrg0);
					m_Computer.AlterBlockInBuild(beg.x+n*l*xstep+xext1, beg.y+n, beg.z+n*l*zstep+zext1, bOrg1);
					if(l == 3)	
						m_Computer.AlterBlockInBuild(beg.x+n*l*xstep+xext2, beg.y+n, beg.z+n*l*zstep+zext2, bOrg2);
				}

			}
		}
	}
	#endregion
}
