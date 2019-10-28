using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class BSIsoSelectBrush : BSBrush 
{
	[SerializeField] protected BSGizmoTriggerEvent gizmoTrigger;

	public Vector3 maxDragSize;

    public override Bounds brushBound
    {
        get
        {
            if(gizmoTrigger != null)
                return gizmoTrigger.boxCollider.bounds;
            else
            {
                Bounds bound = new Bounds();
                bound.min = Min;
                bound.max = Max;
                return bound;
            }
        }
    }

    private BSMath.DrawTarget m_Target;

	public enum EPhase
	{
		Free,
		DragPlane,
		AdjustHeight,
		Drawing
	}

	private EPhase m_Phase = EPhase.Free;

	private Vector3 m_Begin;
	private Vector3 m_End;

	protected Vector3 Min
	{
		get
		{
			return new Vector3 (Mathf.Min(m_Begin.x, m_End.x), 
				Mathf.Min(m_Begin.y, m_End.y), 
				Mathf.Min(m_Begin.z, m_End.z));
		}
	}

	protected Vector3 Max
	{
		get
		{
			return new Vector3 (Mathf.Max(m_Begin.x, m_End.x),
				Mathf.Max(m_Begin.y, m_End.y),
				Mathf.Max(m_Begin.z, m_End.z));
		}
	}

	public Vector3 Size
	{
		get
		{
			return ((Max - Min)) * dataSource.ScaleInverted;
		}
	}

	public BSBoundGizmo gizmoBox;

	public ECoordPlane DragPlane = ECoordPlane.XZ; 


	public List<IntVector3> GetSelectionPos ()
	{
		IntVector3 min = Min * dataSource.ScaleInverted;
		IntVector3 max = Max * dataSource.ScaleInverted;

		List<IntVector3> pos = new List<IntVector3>();

		for (int x = min.x; x < max.x; x++)
		{
			for (int y = min.y; y < max.y; y++)
			{
				for (int z = min.z; z < max.z; z++)
				{
					BSVoxel voxel = dataSource.Read(x, y, z);
					if (!dataSource.VoxelIsZero(voxel, 0))
						pos.Add(new IntVector3(x, y, z));
				}
			}
		}

		return pos;
	}

	public bool SaveToIso(string IsoName, byte[] icon_tex, out BSIsoData outData)
	{
		outData = null;

		List<IntVector3> selections = GetSelectionPos();
		if (selections.Count == 0)
			return false;

		// Only the block can save to be ISO
		if (pattern.type != EBSVoxelType.Block)
		{
			Debug.LogWarning("The iso is not support the Voxel");
			return false;
		}

		BSIsoData iso = new BSIsoData();
		iso.Init(pattern.type);
		iso.m_HeadInfo.Name = IsoName;

		IntVector3 min = new IntVector3(selections[0]);
		IntVector3 max = new IntVector3(selections[0]);

		for (int i = 1; i < selections.Count; i++)
		{
			min.x = (min.x > selections[i].x ? selections[i].x:min.x);
			min.y = (min.y > selections[i].y ? selections[i].y:min.y);
			min.z = (min.z > selections[i].z ? selections[i].z:min.z);
			max.x = (max.x < selections[i].x ? selections[i].x:max.x);
			max.y = (max.y < selections[i].y ? selections[i].y:max.y);
			max.z = (max.z < selections[i].z ? selections[i].z:max.z);
		}

		iso.m_HeadInfo.xSize = max.x - min.x + 1;
		iso.m_HeadInfo.ySize = max.y - min.y + 1;
		iso.m_HeadInfo.zSize = max.z - min.z + 1;
		iso.m_HeadInfo.IconTex = icon_tex;

		for (int i = 0; i < selections.Count; i++)
		{
			BSVoxel voxel = dataSource.SafeRead(selections[i].x, selections[i].y, selections[i].z);
			int key = BSIsoData.IPosToKey(selections[i].x - min.x, selections[i].y - min.y, selections[i].z - min.z);
			iso.m_Voxels.Add(key, voxel);
		}

		iso.CaclCosts();

		string FilePath = GameConfig.GetUserDataPath() + BuildingMan.s_IsoPath;
		/*bool r = */SaveFile(FilePath, iso);

		if (SaveFile(FilePath, iso))
		{
			outData = iso;
			return true;
		}
		else
			return false;
	}

	bool SaveFile (string file_path, BSIsoData iso)
	{
		if (!Directory.Exists(file_path))
			Directory.CreateDirectory(file_path);

		file_path +=  iso.m_HeadInfo.Name + BuildingMan.s_IsoExt;

		try
		{
			using(FileStream fileStream = new FileStream(file_path, FileMode.Create, FileAccess.Write))
			{
				BinaryWriter bw = new BinaryWriter(fileStream);
				byte[] datas = iso.Export();
				bw.Write(datas);
				bw.Close();
			}

			Debug.Log("Save building ISO successfully");
			return true;
		}
		catch (System.Exception)
		{
			//			new PeTipMsg ("Failed to create file, please check  the name", PeTipMsg.EMsgLevel.Error, PeTipMsg.EMsgType.Misc);
			return false;
		}
	}

	private Vector3 m_PointBeforeAdjustHeight;
	protected Vector3 m_Cursor = Vector3.zero;
	Vector3 _beginPos = Vector3.zero;
	Vector3 _prevMousePos = Vector3.zero;

    void Update () 
	{
		if (dataSource == null)
			return;

		if (GameConfig.IsInVCE)
			return;

		if (m_Phase == EPhase.Free)
		{
			if (BSInput.s_MouseOnUI)
			{
				gizmoBox.gameObject.SetActive(false);
				return;
			}

			if (BSInput.s_Cancel)
			{
				ResetDrawing();	
			}

			// Ray cast voxel
			Ray ray = Camera.main.ScreenPointToRay( Input.mousePosition);

			BSMath.DrawTarget blockTgt = new BSMath.DrawTarget();
			BSMath.DrawTarget voxelTgt = new BSMath.DrawTarget();
			bool castBlock = BSMath.RayCastDrawTarget(ray, dataSource, out blockTgt, minvol, true);
			bool castVoxel = BSMath.RayCastDrawTarget(ray, dataSource, out voxelTgt, minvol, true, BuildingMan.Voxels);

			if (castBlock || castVoxel)
			{
				if (!gizmoBox.gameObject.activeSelf)
					gizmoBox.gameObject.SetActive(true);

				Vector3 cursor = m_Target.cursor;
				if (castBlock && castVoxel)
				{
					if (blockTgt.rch.distance <= voxelTgt.rch.distance)
					{
						m_Target = blockTgt;
						cursor = CalcSnapto(m_Target, dataSource, pattern);
						m_Cursor = m_Target.rch.point;
					}
					else
					{
						m_Target = voxelTgt;
						cursor = CalcCursor(m_Target, dataSource, 1);
						m_Cursor = m_Target.rch.point;
					}
				}
				else if (castBlock)
				{
					m_Target = blockTgt;
					cursor = CalcSnapto(m_Target, dataSource, pattern);
					m_Cursor = m_Target.rch.point;
				}
				else if (castVoxel)
				{
					m_Target = voxelTgt;
					cursor = CalcCursor(m_Target, dataSource, 1);
					m_Cursor = m_Target.rch.point;
				}
				

				gizmoBox.size = Vector3.one * dataSource.Scale;
				gizmoBox.position = cursor + dataSource.Offset;

				float offset = dataSource.Scale;
				Vector3 begin = cursor;
				Vector3 end = begin + new Vector3(offset, offset, offset);

				// Click mouse and change phase
				if (Input.GetMouseButtonDown(0))
				{
					m_Begin = begin;
					_beginPos = begin;
					m_End  = end;

					m_Phase = EPhase.DragPlane;
					_prevMousePos = Input.mousePosition;
				}
			}
			else
			{
				ResetDrawing();
			}
		}
		else if (m_Phase == EPhase.DragPlane)
		{
			if (BSInput.s_Cancel)
			{
				ResetDrawing ();
				return;
			}

			RaycastHit rch;
			Ray ray = Camera.main.ScreenPointToRay( Input.mousePosition);

			float position = 0;

			if (DragPlane == ECoordPlane.XY)
			{
				position = m_Cursor.z;
			}
			else if (DragPlane == ECoordPlane.XZ)
			{
				position = m_Cursor.y;
			}
			else if (DragPlane == ECoordPlane.ZY)
				position = m_Cursor.x;

			if ( BSMath.RayCastCoordPlane(ray, DragPlane, position, out rch) )
			{
				m_PointBeforeAdjustHeight = rch.point;
				if (!Vector3.Equals(_prevMousePos, Input.mousePosition))
				{
					Vector3 point = rch.point - dataSource.Offset;

					if (DragPlane == ECoordPlane.XZ)
					{
						// x
						float x = 0;
						m_Begin.x = CalcValue(point.x, _beginPos.x, out x);

						// z
						float z = 0;
						m_Begin.z = CalcValue(point.z, _beginPos.z, out z);

						m_End = new Vector3(x, _beginPos.y + (pattern.size) * dataSource.Scale, z);
						m_End = Clamp(m_Begin, m_End);

					}
					else if (DragPlane == ECoordPlane.XY)
					{
						// x
						float x = 0;
						m_Begin.x = CalcValue(point.x, _beginPos.x, out x);

						// y
						float y = 0;
						m_Begin.y = CalcValue(point.y, _beginPos.y, out y);

						m_End = new Vector3(x, y, _beginPos.z + (pattern.size) * dataSource.Scale);
						m_End = Clamp(m_Begin, m_End);
					}
					else if (DragPlane == ECoordPlane.ZY)
					{
						// y
						float y = 0;
						m_Begin.y = CalcValue(point.y, _beginPos.y, out y);

						// z
						float z = 0;
						m_Begin.z = CalcValue(point.z, _beginPos.z, out z);

						m_End = new Vector3(_beginPos.x + (pattern.size) * dataSource.Scale, y, z);
						m_End = Clamp(m_Begin, m_End);
					}

					gizmoBox.position = Min +  dataSource.Offset;

					gizmoBox.size = Size * dataSource.Scale;

				}


				// Click mouse and change phase
				if (Input.GetMouseButtonUp(0))
				{
					m_Phase = EPhase.AdjustHeight;
					_prevMousePos = new Vector3(-100, -100, -100);
				}
			}
		}
		else if (m_Phase == EPhase.AdjustHeight)
		{
			if (BSInput.s_Cancel)
			{
				ResetDrawing ();
				return;
			}

			Ray ray = Camera.main.ScreenPointToRay( Input.mousePosition);
			if (DragPlane == ECoordPlane.XZ)
			{
				float adjustHeight = m_PointBeforeAdjustHeight.y;
				BSMath.RayAdjustHeight(ray, ECoordAxis.Y,  m_PointBeforeAdjustHeight, out adjustHeight);

				// y
				float y = 0;
				m_Begin.y = CalcValue(adjustHeight, _beginPos.y, out y);
				m_End.y = y;
			}
			else if (DragPlane == ECoordPlane.XY)
			{
				float adjustHeight = m_PointBeforeAdjustHeight.z;
				BSMath.RayAdjustHeight(ray, ECoordAxis.Z, m_PointBeforeAdjustHeight, out adjustHeight);


				// z 
				float z = 0;
				m_Begin.z = CalcValue(adjustHeight, _beginPos.z, out z);
				m_End.z = z;
			}
			else if (DragPlane == ECoordPlane.ZY)
			{
				float adjustHeight = m_PointBeforeAdjustHeight.x;
				BSMath.RayAdjustHeight(ray, ECoordAxis.X, m_PointBeforeAdjustHeight, out adjustHeight);

				// x
				float x = 0;
				m_Begin.x = CalcValue(adjustHeight, _beginPos.x, out x);
				m_End.x = x;

			}

			m_End = Clamp(m_Begin, m_End);

			gizmoBox.position = Min +  dataSource.Offset;

			gizmoBox.size = Size * dataSource.Scale;

			if (PeInput.Get(PeInput.LogicFunction.Build))
			{
				Do();
				m_Phase = EPhase.Drawing;
			}
		}
		else if (m_Phase == EPhase.Drawing)
		{
			if (BSInput.s_Cancel)
			{
				ResetDrawing ();
				return;
			}

			if (BSInput.s_Delete)
			{
				DeleteVoxels();
			}
		}
	}

	public void DeleteVoxels ()
	{
		List<IntVector3> selections = GetSelectionPos();
		if (selections.Count == 0)
			return;


		List<BSVoxel> new_voxels = new List<BSVoxel>();
		List<BSVoxel> old_voxels = new List<BSVoxel>();

		for (int i = 0; i < selections.Count; i++)
		{
			BSVoxel voxel = dataSource.Read(selections[i].x, selections[i].y, selections[i].z);

			new_voxels.Add(new BSVoxel());
			old_voxels.Add(voxel);
		}

		BSAction action = new BSAction();
		BSVoxelModify vm = new BSVoxelModify(selections.ToArray(), old_voxels.ToArray(), new_voxels.ToArray(), dataSource, EBSBrushMode.Subtract);

		action.AddModify(vm);
		vm.Redo();
		BSHistory.AddAction(action);

		ResetDrawing ();
	}

	float CalcValue (float end, float beign, out float outPut)
	{
		float sign = Mathf.Sign (end -  beign);

		float offset = sign >=0 ? 0 : pattern.size;

		int iv = Mathf.FloorToInt((end - beign) *dataSource.ScaleInverted);
		int remainder  = Mathf.Abs( iv % pattern.size );

		if (sign > 0)
			remainder = pattern.size - remainder;
		else
			remainder = remainder == 0 ? 0 : pattern.size - remainder;

		outPut =  Mathf.Floor(end * dataSource.ScaleInverted) * dataSource.Scale + remainder * dataSource.Scale * sign;
		return beign + offset * dataSource.Scale;
	}

	protected void ResetDrawing ()
	{
		m_Phase = EPhase.Free;
		gizmoBox.size = Vector3.one * dataSource.Scale;
		gizmoBox.gameObject.SetActive (false);
		//m_EndOffset = Vector3.zero;
		//_drawGL = false;
	}

	Vector3 Clamp (Vector3 begin, Vector3 end)
	{
		Vector3 result = Vector3.zero;
		Vector3 size = maxDragSize * dataSource.Scale;

		result.x = begin.x + Mathf.Clamp(end.x - begin.x, -size.x, size.x);
		result.y = begin.y + Mathf.Clamp(end.y - begin.y, -size.y, size.y);
		result.z = begin.z + Mathf.Clamp(end.z - begin.z, -size.z, size.z);

		return result;
	}

	protected override void Do ()
	{
		
	}
}
