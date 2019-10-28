using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BSPointBrush : BSBrush 
{
	public BSGizmoCubeMesh GizmoCube;
	public int MaxOffset = 5;
    public Color addModeGizmoColor = Color.white;
    public Color removeModeGizmoColor = Color.white;

    [SerializeField] BSGizmoTriggerEvent GizmoTrigger;

	public override Bounds brushBound {
		get {
			return GizmoTrigger.boxCollider.bounds;
		}
	}


	// Model Gizmo
	private GameObject Gizmo;

	// Ray cast draw target
	private BSMath.DrawTarget  m_Target;
	

	private Vector3 m_Cursor;
	private int m_Rot;

	private Vector3 m_FocusOffset;

	private BSPattern prvePattern = null;
	
	void Start () 
	{
		m_Target = new BSMath.DrawTarget();
		pattern = BSPattern.DefaultV1;
	}

	void Update ()
	{
		if (pattern == null)
			return;

		if (dataSource == null)
			return;

        if (GameConfig.IsInVCE)
            return;

        if (BSInput.s_MouseOnUI)
		{
			if (Gizmo != null)
				Gizmo.SetActive(false);

			GizmoCube.gameObject.SetActive(false);
			return;
		}

        if (BSInput.s_Alt)
        {
            mode = EBSBrushMode.Subtract;
            GizmoCube.color = removeModeGizmoColor;
        }
        else
        {
            mode = EBSBrushMode.Add;
            GizmoCube.color = addModeGizmoColor;
        }

        if (GizmoTrigger.RayCast)
            GizmoCube.color = removeModeGizmoColor;

        if (mode == EBSBrushMode.Add)
		{
			AddMode();
		}
		else if (mode == EBSBrushMode.Subtract)
		{
			SubtractMode();
		}
	}

	void AddMode ()
	{
		// Set gizmo
		if (pattern != prvePattern)
		{
			if (Gizmo != null)
			{
				Destroy( Gizmo.gameObject); 
				Gizmo = null;
			}

			if (pattern.MeshPath != null && pattern.MeshPath != "")
			{
				Gizmo = GameObject.Instantiate(Resources.Load(pattern.MeshPath)) as GameObject;
				Gizmo.transform.parent = transform;
			}

			
			prvePattern = pattern;
			m_Rot = 0;
		}


		if (pattern.MeshMat != null && Gizmo != null)
		{
			Renderer gizmo_render = Gizmo.GetComponent<Renderer>();
			if (gizmo_render != null)
				gizmo_render.material = pattern.MeshMat;
		}
		
		// Ratate the gizmo ?  Only block can rotate
		if (pattern.type == EBSVoxelType.Block)
		{
			if (Input.GetKeyDown(KeyCode.T))
			{
				m_Rot = ++m_Rot > 3 ? 0 : m_Rot;
			}
			
		}
		
		// Gizmo rotate
		Vector3 rot_offset = Vector3.zero;
		if (Gizmo != null)
		{
			Quaternion rot = Quaternion.Euler(0, 90* m_Rot, 0);
			Gizmo.transform.rotation = Quaternion.Euler(0, 90* m_Rot, 0);
			float half_size = pattern.size * 0.5f;
			rot_offset = (rot * new Vector3(- half_size,  -half_size,  -half_size) + new Vector3(half_size, half_size, half_size)) * dataSource.Scale;

			// Gizmo scale
			Gizmo.transform.localScale = new Vector3(pattern.size, pattern.size, pattern.size) * dataSource.Scale;
		}
		

		// default gizmo cube
		GizmoCube.m_VoxelSize = dataSource.Scale;
		GizmoCube.CubeSize = new IntVector3(pattern.size, pattern.size, pattern.size);
		Vector3 trigget_size = new Vector3(GizmoCube.CubeSize.x * dataSource.Scale, 
		                                   GizmoCube.CubeSize.y * dataSource.Scale,
		                                   GizmoCube.CubeSize.z * dataSource.Scale);
		GizmoTrigger.boxCollider.size =  trigget_size;
		GizmoTrigger.boxCollider.center = trigget_size * 0.5f;
		
		// Ray cast voxel
		Ray ray = Camera.main.ScreenPointToRay( Input.mousePosition);
		if (BSMath.RayCastDrawTarget(ray, dataSource, out m_Target, minvol, true, BuildingMan.Datas))
		{

			// Show the Gizmo
			if (Gizmo != null)
				Gizmo.SetActive(true);
			GizmoCube.gameObject.SetActive(true);

			m_Cursor = CalcCursor(m_Target, dataSource, pattern.size);

			// Gizmo position
			FocusAjust();
			
			m_Cursor += m_FocusOffset * dataSource.Scale;
			if (Gizmo != null)
				Gizmo.transform.position = m_Cursor + dataSource.Offset + rot_offset ;
			GizmoCube.transform.position = m_Cursor + dataSource.Offset;
			
			// Bursh do
			if (Input.GetMouseButtonDown(0) && !GizmoTrigger.RayCast)
			{
				Do(); 
			}
		}
	}

	void SubtractMode ()
	{
		m_Rot = 0;
		prvePattern = null;

		if (Gizmo != null)
		{
			Destroy( Gizmo.gameObject); 
			Gizmo = null;
		}

		GizmoCube.gameObject.SetActive(true);

		GizmoCube.m_VoxelSize = dataSource.Scale;
		GizmoCube.CubeSize = new IntVector3(pattern.size, pattern.size, pattern.size);

		// Ray cast voxel
		Ray ray = Camera.main.ScreenPointToRay( Input.mousePosition);
		if (BSMath.RayCastDrawTarget(ray, dataSource, out m_Target, minvol, false))
		{

			// Show the Gizmo
			if (Gizmo != null)
				Gizmo.SetActive(true);
            if (!GizmoCube.gameObject.activeSelf)
                GizmoCube.gameObject.SetActive(true);

            // Cursor position
            m_Cursor = CalcSnapto(m_Target, dataSource, pattern);

			// Gizmo position
			FocusAjust();
			
			m_Cursor += m_FocusOffset * dataSource.Scale;
			if (Gizmo != null)
				Gizmo.transform.position = m_Cursor + dataSource.Offset;
			GizmoCube.transform.position = m_Cursor + dataSource.Offset;

			// Bursh do
			if (Input.GetMouseButtonDown(0))
			{
				Do(); 
			}
		}
        else
        {
            if (Gizmo != null)
                Gizmo.SetActive(false);
            if (GizmoCube.gameObject.activeSelf)
                GizmoCube.gameObject.SetActive(false);
        }
	}

	// 
	private Vector3 _prveMousePos = Vector3.zero;
	void FocusAjust ()
	{
		if (_prveMousePos == Input.mousePosition)
		{
			if (BSInput.s_Up)
				m_FocusOffset += Vector3.up;
			else if (BSInput.s_Down)
				m_FocusOffset -= Vector3.up;
			else if (BSInput.s_Right)
				m_FocusOffset += Vector3.right;
			else if (BSInput.s_Left)
				m_FocusOffset += Vector3.left;
			else if (BSInput.s_Forward)
				m_FocusOffset += Vector3.forward;
			else if (BSInput.s_Back)
				m_FocusOffset += Vector3.back;

		}
		else
		{
			_prveMousePos = Input.mousePosition;
			m_FocusOffset.x = 0;
			m_FocusOffset.y = 0;
			m_FocusOffset.z = 0;
		}
	}

	protected override void Do ()
	{
		int cnt = pattern.size;
		Vector3 center = new Vector3((cnt-1) / 2.0f, 0, (cnt-1) / 2.0f);

		if (mode == EBSBrushMode.Add)
		{
			List<BSVoxel> new_voxels = new List<BSVoxel>();
			List<IntVector3> indexes = new List<IntVector3>();
			List<BSVoxel> old_voxels = new List<BSVoxel>();


			for (int x = 0; x < cnt; x++)
			{
				for (int y = 0; y < cnt; y++)
				{
					for (int z = 0; z < cnt; z++)
					{
						// rote if need
						Vector3 offset = Quaternion.Euler(0, 90* m_Rot, 0) * new Vector3(x - center.x, y - center.y, z - center.z) + center;
						
						IntVector3 inpos = new IntVector3(Mathf.FloorToInt( m_Cursor.x * dataSource.ScaleInverted) + offset.x,
						                                  Mathf.FloorToInt( m_Cursor.y * dataSource.ScaleInverted) + offset.y,
						                                  Mathf.FloorToInt( m_Cursor.z * dataSource.ScaleInverted) + offset.z);

						// new voxel
						BSVoxel voxel = pattern.voxelList[x, y, z];
						voxel.materialType = materialType;
						voxel.blockType = BSVoxel.MakeBlockType(voxel.blockType >> 2, ((voxel.blockType & 0X3) + m_Rot) % 4);

						//olde voxel
						BSVoxel old_voxel = dataSource.SafeRead(inpos.x, inpos.y, inpos.z);

						new_voxels.Add(voxel);
						old_voxels.Add(old_voxel);
						indexes.Add(inpos);
					}
				}
			}

			// Modify
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

			for (int x = 0; x < cnt; x++)
			{
				for (int y = 0; y < cnt; y++)
				{
					for (int z = 0; z < cnt; z++)
					{
						IntVector3 inpos = new IntVector3(Mathf.FloorToInt( m_Cursor.x * dataSource.ScaleInverted) + x,
						                                  Mathf.FloorToInt( m_Cursor.y * dataSource.ScaleInverted) + y,
						                                  Mathf.FloorToInt( m_Cursor.z * dataSource.ScaleInverted) + z);

						BSVoxel voxel = dataSource.Read(inpos.x, inpos.y, inpos.z);
						new_voxels.Add(new BSVoxel());
						indexes.Add(inpos);
						old_voxels.Add(voxel);
					}
				}
			}

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
