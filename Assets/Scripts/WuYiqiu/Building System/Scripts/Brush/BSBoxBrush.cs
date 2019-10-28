using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BSBoxBrush : BSFreeSizeBrush
{
    public Color addModeGizmoColor = Color.white;
    public Color removeModeGizmoColor = Color.white;

	public bool forceShowRemoveColor = false;

    private BSPattern m_PrevPattern;
	private GameObject modelGizmo;
	private int m_Rot;


	public bool AllowRemoveVoxel = false;

	protected override bool ExtraAdjust ()
	{
        if (BSInput.s_Alt)
        {
            mode = EBSBrushMode.Subtract;
            gizmoCube.color = removeModeGizmoColor;
        }
        else
        {
            mode = EBSBrushMode.Add;
            gizmoCube.color = addModeGizmoColor;
        }

        if (!AllowRemoveVoxel &&  pattern.type == EBSVoxelType.Voxel && mode == EBSBrushMode.Subtract )
		{
			if (modelGizmo != null && modelGizmo.gameObject.activeSelf)
				modelGizmo.gameObject.SetActive(false);
			if (modelGizmo != null && !modelGizmo.gameObject.activeSelf)
				gizmoCube.gameObject.SetActive(false);
            ResetDrawing();


            return false;
		}
		else
		{

			if (gizmoCube != null)
			{
				if (!gizmoCube.gameObject.activeSelf)
					gizmoCube.gameObject.SetActive(true);
			}

			if (BSInput.s_MouseOnUI)
			{
				if (modelGizmo != null && modelGizmo.gameObject.activeSelf)
					modelGizmo.gameObject.SetActive(false);
				return true;
			}
			else
			{
				if (modelGizmo != null && !modelGizmo.gameObject.activeSelf)
					modelGizmo.gameObject.SetActive(true);
			}
		}

		// Set gizmo
		if (pattern != m_PrevPattern)
		{
			if (modelGizmo != null)
			{
				Destroy( modelGizmo.gameObject); 
				modelGizmo = null;
			}
			
			if (pattern.MeshPath != null && pattern.MeshPath != "")
			{
				modelGizmo = GameObject.Instantiate(Resources.Load(pattern.MeshPath)) as GameObject;
				modelGizmo.transform.parent = transform;
				modelGizmo.name = "Model Gizmo";
				Renderer mr = modelGizmo.GetComponent<Renderer>();
				mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
				mr.receiveShadows = false;
			}
			
			
			m_PrevPattern = pattern;
			m_Rot = 0;
		}

		if (pattern.MeshMat != null && modelGizmo != null)
		{
			Renderer gizmo_render = modelGizmo.GetComponent<Renderer>();
			if (gizmo_render != null)
				gizmo_render.material = pattern.MeshMat;
		}

		if (modelGizmo != null)
			modelGizmo.transform.localScale = new Vector3(pattern.size, pattern.size, pattern.size) * dataSource.Scale;

        

        // Ratate the gizmo ?  Only block can rotate
        if (pattern.type == EBSVoxelType.Block)
		{
			if (Input.GetKeyDown(KeyCode.T))
			{
				m_Rot = ++m_Rot > 3 ? 0 : m_Rot;
			}
		}
		
		if (gizmoTrigger.RayCast || forceShowRemoveColor)
        {
			if (gizmoTrigger.RayCast)
				ExtraTips = PELocalization.GetString(8000686);
			if (forceShowRemoveColor)
				ExtraTips = PELocalization.GetString(821000001);
            gizmoCube.color = removeModeGizmoColor;
        }
		else
			ExtraTips = "";

		return true;
	}



	protected override void AfterDo ()
	{
		if (modelGizmo != null)
		{
			Quaternion rot = Quaternion.Euler(0, 90* m_Rot, 0);
			modelGizmo.transform.rotation = Quaternion.Euler(0, 90* m_Rot, 0);
			float half_size = pattern.size * 0.5f;
			Vector3 rot_offset = (rot * new Vector3(- half_size,  -half_size,  -half_size) + new Vector3(half_size, half_size, half_size)) * dataSource.Scale;
			modelGizmo.transform.position = m_GizmoCursor + dataSource.Offset + rot_offset;
		}
	}


	protected override void Do ()
	{
		if (gizmoTrigger.RayCast)
			return;
		

		Vector3 min = Min * dataSource.ScaleInverted;
		Vector3 max = Max * dataSource.ScaleInverted;

		int cnt = pattern.size;
		Vector3 center = new Vector3((cnt-1) / 2.0f, 0, (cnt-1) / 2.0f);

		if (mode == EBSBrushMode.Add)
		{
			List<BSVoxel> new_voxels = new List<BSVoxel>();
			List<IntVector3> indexes = new List<IntVector3>();
			List<BSVoxel> old_voxels = new List<BSVoxel>();
			Dictionary<IntVector3, int> refVoxelMap = new Dictionary<IntVector3, int>();

			for (int x = (int)min.x; x < (int)max.x; x += cnt )
			{
				for (int y = (int)min.y; y < (int)max.y; y += cnt)
				{
					for (int z = (int)min.z; z < (int)max.z; z += cnt )
					{
						for (int ix = 0; ix < cnt; ix++)
						{
							for (int iy = 0; iy < cnt; iy++)
							{
								for (int iz = 0; iz < cnt; iz++)
								{
									// rote if need
									Vector3 offset = Quaternion.Euler(0, 90* m_Rot, 0) * new Vector3(ix - center.x, iy - center.y, iz - center.z) + center;
									
									IntVector3 inpos = new IntVector3(Mathf.FloorToInt( x) + offset.x,
									                                  Mathf.FloorToInt( y) + offset.y,
									                                  Mathf.FloorToInt( z) + offset.z);

									// new voxel
									BSVoxel voxel = pattern.voxelList[ix, iy, iz];
									voxel.materialType = materialType;
									voxel.blockType = BSVoxel.MakeBlockType(voxel.blockType >> 2, ((voxel.blockType & 0X3) + m_Rot) % 4);

									//olde voxel
									BSVoxel old_voxel = dataSource.SafeRead(inpos.x, inpos.y, inpos.z);

									new_voxels.Add(voxel);
									old_voxels.Add(old_voxel);
									indexes.Add(inpos);
									refVoxelMap.Add(inpos, 0);
								}
							}
						}
					}
				}
			}

			// Extra Extendable
			FindExtraExtendableVoxels(dataSource, new_voxels, old_voxels, indexes, refVoxelMap);

			// new Modify ?
			if (indexes.Count != 0)
			{
				BSAction action = new BSAction();
				
				BSVoxelModify modify = new BSVoxelModify(indexes.ToArray(), old_voxels.ToArray(), new_voxels.ToArray(), dataSource, mode);
				
				action.AddModify(modify);
				
				if (action.Do())
					BSHistory.AddAction(action);
			}
		}
		else if (mode == EBSBrushMode.Subtract)
		{
			List<BSVoxel> new_voxels = new List<BSVoxel>();
			List<IntVector3> indexes = new List<IntVector3>();
			List<BSVoxel> old_voxels = new List<BSVoxel>();
			Dictionary<IntVector3, int> refVoxelMap = new Dictionary<IntVector3, int>();

			for (int x = (int)min.x; x < (int)max.x; x += cnt )
			{
				for (int y = (int)min.y; y < (int)max.y; y += cnt)
				{
					for (int z = (int)min.z; z < (int)max.z; z += cnt )
					{
						for (int ix = 0; ix < cnt; ix++)
						{
							for (int iy = 0; iy < cnt; iy++)
							{
								for (int iz = 0; iz < cnt; iz++)
								{
									IntVector3 inpos = new IntVector3(Mathf.FloorToInt(x) + ix,
									                                  Mathf.FloorToInt(y) + iy,
									                                  Mathf.FloorToInt(z) + iz);


									BSVoxel voxel = dataSource.Read(inpos.x, inpos.y, inpos.z);
									
									new_voxels.Add(new BSVoxel());
									indexes.Add(inpos);
									old_voxels.Add(voxel);
									refVoxelMap[inpos] = 0;

									// extendtable
									List<IntVector4> ext_posList = null;
									List<BSVoxel> ext_voxels = null;
									if (dataSource.ReadExtendableBlock (new IntVector4(inpos, 0), out ext_posList, out ext_voxels))
									{
										for (int i = 0; i < ext_voxels.Count; i++)
										{
											IntVector3 _ipos = new IntVector3(ext_posList[i].x, ext_posList[i].y, ext_posList[i].z);
								
											if (_ipos == inpos)
												continue;

											if (!refVoxelMap.ContainsKey(_ipos))
											{
												BSVoxel v = dataSource.Read(_ipos.x, _ipos.y, _ipos.z);
												old_voxels.Add(v);
												indexes.Add(_ipos);
												new_voxels.Add(new BSVoxel());
												refVoxelMap.Add(_ipos, 0);
											}


										}
									}
								}
							}
						}
					}
				}
			}

			// Extra Extendable
			FindExtraExtendableVoxels(dataSource, new_voxels, old_voxels, indexes, refVoxelMap);

			// Action
			if (indexes.Count != 0)
			{
				BSAction action = new BSAction();
				
				BSVoxelModify modify = new BSVoxelModify(indexes.ToArray(), old_voxels.ToArray(), new_voxels.ToArray(), dataSource, mode);
				
				action.AddModify(modify);
				
				if (action.Do())
					BSHistory.AddAction(action);
			}
		}

	}

}
