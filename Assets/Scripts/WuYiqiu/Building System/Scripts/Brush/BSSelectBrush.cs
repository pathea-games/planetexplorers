using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BSSelectBrush : BSBrush 
{
	#region Selection_part
	// Selection
	protected Dictionary<IntVector3, byte>  m_Selections = new Dictionary<IntVector3, byte>();
	public Dictionary<IntVector3, byte> Selections { get {return m_Selections;} }

	public void AddSelection(IntVector3 ipos, byte val)
	{
		m_Selections.Add(ipos, val);
		m_RecalcBoxes = true;
	}

	public  void RemoveSelection(IntVector3 ipos)
	{
		m_Selections.Remove(ipos);
		m_RecalcBoxes = true;
	}

	public void ClearSelection(BSAction action)
	{
		if (action == null)
		{
			m_Selections.Clear();
			m_RecalcBoxes = true;
		}
		else
		{
			Dictionary<IntVector3, byte> old_selection = new Dictionary<IntVector3, byte>(m_Selections);
			m_Selections.Clear();
			Dictionary<IntVector3, byte> new_selection = new Dictionary<IntVector3, byte>(m_Selections);
			
			BSSelectedBoxModify sm = new BSSelectedBoxModify(old_selection, new_selection, this);
			action.AddModify(sm);

			m_RecalcBoxes = true;
		}
	}

	public void ResetSelection (Dictionary<IntVector3, byte> selection)
	{
		m_Selections = selection;
		CalcBoxes();
	}

	public bool IsEmpty ()
	{
		return m_Selections.Count == 0;
	}

	protected  List<BSTools.SelBox> m_SelectionBoxes = new List<BSTools.SelBox>();
	private  bool m_RecalcBoxes = false;

    public override Bounds brushBound
    {
        get
        {
            BSTools.IntBox intBound = BSTools.SelBox.CalculateBound(m_SelectionBoxes);
            Bounds bound = new Bounds();
            bound.min = new Vector3(intBound.xMin, intBound.yMin, intBound.zMin);
            bound.max = new Vector3(intBound.xMax, intBound.yMax, intBound.zMax);

            if (dataSource == BuildingMan.Blocks)
            {
                bound.min = bound.min * 0.5f;
                bound.max = bound.max * 0.5f;
            }
  
            return bound;
        }
    }

    protected void CalcBoxes ()
	{
		BSTools.LeastBox.Calculate(m_Selections, ref m_SelectionBoxes);
	}
	#endregion

	public const int MaxVoxelCount = 10000;

	public float Depth = 1;
	public Gradient voxelsCountColor;

	public int maxDragSize = 32;

	public Vector3 maxSelectBoxSize = new Vector3(128, 128, 128);


	// Selecting Box
	protected Vector3 m_Begin;
	protected Vector3 m_End;

	ECoordPlane m_Coord = ECoordPlane.XZ; 
	float m_PlanePos = 0;

	protected bool m_Selecting;

	// Draw Target
	private BSMath.DrawTarget m_Target;

	private bool m_Drawing = false;

	// Selected Box Renderer
	public BSGLSelectionBoxes seletionBoxeRenderer;


	private bool m_IsValidBox = true;

	protected IBSDataSource m_PrevDS = null;

	protected BSAction m_Action = new BSAction();

	protected void Start () 
	{
		GlobalGLs.AddGL(this);

	}

	protected void OnDestroy ()
	{

		ClearSelection(null);
	}

	public bool canDo = true;

	Vector3 _beginPos = Vector3.zero;
	IBSDataSource _datasource = null;
	protected void Update () 
	{
		if (dataSource == null)
			return;

		if (BSInput.s_MouseOnUI)
		{
			return;
		}

		if (_datasource == dataSource)
		{
			// Clear and rebuild;
			ClearSelection(null);
			_datasource = dataSource;
		}

		if (seletionBoxeRenderer != null)
		{
			seletionBoxeRenderer.m_Boxes = m_SelectionBoxes;
			seletionBoxeRenderer.scale = dataSource.Scale;
			seletionBoxeRenderer.offset = dataSource.Offset;
		}

		if (m_RecalcBoxes)
		{
			CalcBoxes();
			m_RecalcBoxes = false;
		}

        if (GameConfig.IsInVCE)
            return;

        // Depth
        if (!BSInput.s_Shift && Input.GetKeyDown(KeyCode.UpArrow))
		{
			Depth = ++Depth >= maxDragSize ? maxDragSize : Depth;
			m_GUIAlpha = 5;
		}
		else if (!BSInput.s_Shift && Input.GetKeyDown(KeyCode.DownArrow))
		{
			Depth = --Depth >= 1 ? Depth : 1;
			m_GUIAlpha = 5;
		}

		m_GUIAlpha = Mathf.Lerp(m_GUIAlpha, 0, 0.05f);



		// Drag Box
		//
		Ray ray = Camera.main.ScreenPointToRay( Input.mousePosition);
		if (!m_Selecting)
		{
			// Cancel
			if (BSInput.s_Cancel)
			{
				if (!canDo)
					return;

				Dictionary<IntVector3, byte> old_seletion = new Dictionary<IntVector3, byte>(m_Selections);
				Cancel();
				Dictionary<IntVector3, byte> new_selection = new Dictionary<IntVector3, byte>(m_Selections);

				// For History
				if (old_seletion.Count != 0)
				{
					BSSelectedBoxModify modify = new BSSelectedBoxModify(old_seletion, new_selection, this);
					m_Action.AddModify(modify);

					DoHistory();
				}

				return;
			}

			if (BSMath.RayCastDrawTarget(ray, dataSource, out m_Target, minvol, true, BuildingMan.Datas))
			{
				m_Drawing = true;

				m_Begin = m_Target.snapto;
				_beginPos = m_Begin;
				m_End = m_Begin;

				float vx = Mathf.Abs(m_Target.rch.normal.x);
				float vy = Mathf.Abs(m_Target.rch.normal.y);
				float vz = Mathf.Abs(m_Target.rch.normal.z);

				if ( vx > 0.9f * m_Target.ds.Scale )
				{
					m_Coord = ECoordPlane.ZY;
					m_PlanePos = m_Target.rch.point.x;
				}
				else if (vy > 0.9f * m_Target.ds.Scale)
				{
					m_Coord = ECoordPlane.XZ;
					m_PlanePos = m_Target.rch.point.y;
				}
				else if (vz > 0.9f * m_Target.ds.Scale)
				{
					m_Coord = ECoordPlane.XY;
					m_PlanePos = m_Target.rch.point.z;
				}

				if (Input.GetMouseButtonDown(0))
				{
					if (canDo)
						m_Selecting = true;
				}
			}
			else
				m_Drawing = false;
		}
		// Selecting
		else
		{
			m_Drawing = true;

			// Cancel
			if (BSInput.s_Cancel)
			{
				if (!canDo)
					return;
				m_Selecting = false;
				m_Begin = new Vector3(-10000, -10000, -10000);
				m_End = new Vector3(-10000, -10000, -10000);
				return;
			}

			RaycastHit rch;
			if (BSMath.RayCastCoordPlane(ray, m_Coord, m_PlanePos, out rch) )
			{
				Vector3 point = rch.point - dataSource.Offset;
				if (m_Coord == ECoordPlane.XY)
				{

					// x componet
					float x = 0;
					m_Begin.x = CalcValue(point.x, _beginPos.x, out x);

					// y componet
					float y = 0;
					m_Begin.y = CalcValue(point.y, _beginPos.y, out y);

					m_End.x = x;
					m_End.y = y;
					m_End.z = Mathf.FloorToInt(point.z * dataSource.ScaleInverted) * dataSource.Scale;


					if (m_Target.rch.normal.z > 0)
						m_Begin.z = m_PlanePos - Depth * dataSource.Scale;
					else
						m_Begin.z = m_PlanePos + Depth * dataSource.Scale;

					m_End = Clamp(m_Begin, m_End);

				}
				else if (m_Coord == ECoordPlane.XZ)
				{

					// x componet
					float x = 0;
					m_Begin.x = CalcValue(point.x, _beginPos.x, out x);

					// z componet
					float z = 0;
					m_Begin.z = CalcValue(point.z, _beginPos.z, out z);
					
					m_End.x = x;
					m_End.y = Mathf.FloorToInt(point.y * dataSource.ScaleInverted) * dataSource.Scale;
					m_End.z = z;

					if (m_Target.rch.normal.y > 0)
						m_Begin.y = m_PlanePos - Depth * dataSource.Scale;
					else
						m_Begin.y = m_PlanePos + Depth * dataSource.Scale;

					m_End = Clamp(m_Begin, m_End);
				}
				else if (m_Coord == ECoordPlane.ZY)
				{

					// y componet
					float y = 0;
					m_Begin.y = CalcValue(point.y, _beginPos.y, out y);

					// z componet
					float z = 0;
					m_Begin.z = CalcValue(point.z, _beginPos.z, out z);
					
					m_End.x = Mathf.FloorToInt(point.x * dataSource.ScaleInverted) * dataSource.Scale;
					m_End.y = y;
					m_End.z = z;

					if (m_Target.rch.normal.x > 0)
						m_Begin.x = m_PlanePos - Depth * dataSource.Scale;
					else
						m_Begin.x = m_PlanePos + Depth * dataSource.Scale;

					m_End = Clamp(m_Begin, m_End);
				}
			}

			// In Valid area ?
			if (m_PrevDS != null && m_PrevDS == dataSource)
			{
				if ( m_Selections.Count != 0)
				{
					BSTools.SelBox box = new BSTools.SelBox();
					box.m_Box.xMin = (short)Mathf.FloorToInt( Mathf.Min(m_Begin.x, m_End.x) * dataSource.ScaleInverted);
					box.m_Box.yMin = (short)Mathf.FloorToInt( Mathf.Min(m_Begin.y, m_End.y) * dataSource.ScaleInverted);
					box.m_Box.zMin = (short)Mathf.FloorToInt( Mathf.Min(m_Begin.z, m_End.z) * dataSource.ScaleInverted);
					box.m_Box.xMax = (short)Mathf.FloorToInt( Mathf.Max(m_Begin.x, m_End.x) * dataSource.ScaleInverted);
					box.m_Box.yMax = (short)Mathf.FloorToInt( Mathf.Max(m_Begin.y, m_End.y) * dataSource.ScaleInverted);
					box.m_Box.zMax = (short)Mathf.FloorToInt( Mathf.Max(m_Begin.z, m_End.z) * dataSource.ScaleInverted);

					// Add current box to list temporarily
					m_SelectionBoxes.Add(box);
					BSTools.IntBox bound = BSTools.SelBox.CalculateBound(m_SelectionBoxes);

					Vector3 size = new Vector3(bound.xMax - bound.xMin, bound.yMax - bound.yMin, bound.zMax - bound.zMin);

					if (size.x > maxSelectBoxSize.x  || size.y > maxSelectBoxSize.y || size.z > maxSelectBoxSize.z)
					{
						m_IsValidBox = false;
					}
					else 
						m_IsValidBox = true;

					// revert the list
					m_SelectionBoxes.RemoveAt(m_SelectionBoxes.Count - 1);
				}
				else
					m_IsValidBox = true;
			}
			else
				m_IsValidBox = true;
		
			if (Input.GetMouseButtonUp(0))
			{
				if (!canDo)
					return;

//				bool result = false;

				if (m_IsValidBox)
				{
					// Add
					if (m_PrevDS == dataSource)
					{
						if (BSInput.s_Shift)
						{
							if (m_PrevDS == dataSource)
								/*result = */CalcSelection(0);
						}
						// Substract
						else if (BSInput.s_Alt)
						{
							/*result = */CalcSelection(1);
						}
						// Cross
						else if (BSInput.s_Control)
						{
							/*result = */CalcSelection(2);
						}
						else
						{
							ClearSelection(m_Action);
							/*result = */CalcSelection(0);
						}

						m_PrevDS = dataSource;
					}
					else
					{
						ClearSelection(m_Action);
						/*result = */CalcSelection(0);

						m_PrevDS = dataSource;
					}
				}

				m_Selecting = false;
				ResetValue();

			}

		}
		// Other Ajust
		if ( AfterSelectionUpdate())
			DoHistory ();
	}
	
	void DoHistory ()
	{

		if (!m_Action.IsEmpty())
		{
			BSHistory.AddAction(m_Action);
			m_Action = new BSAction();
		}

	}

	float CalcValue (float end, float beign, out float outPut)
	{
		float sign = Mathf.Sign (end -  beign);
		
		float offset = sign >=0 ? 0 : 1;
		
		int iv = Mathf.FloorToInt((end - beign) *dataSource.ScaleInverted);
		int remainder  = Mathf.Abs( iv % pattern.size );
		
		if (sign > 0)
			remainder = 1 - remainder;
		else
			remainder = remainder == 0 ? 0 : 1 - remainder;
		
		outPut =  Mathf.Floor(end * dataSource.ScaleInverted) * dataSource.Scale + remainder * dataSource.Scale * sign;
		return beign + offset * dataSource.Scale;
	}

	Vector3 Clamp (Vector3 begin, Vector3 end)
	{
		Vector3 result = Vector3.zero;
		float size = maxDragSize * dataSource.Scale;
		
		result.x = begin.x + Mathf.Clamp(end.x - begin.x, -size, size);
		result.y = begin.y + Mathf.Clamp(end.y - begin.y, -size, size);
		result.z = begin.z + Mathf.Clamp(end.z - begin.z, -size, size);
		
		return result;
	}

	protected override void Do ()
	{

	}

	public override void Cancel ()
	{
		ClearSelection(m_Action);
		m_Selecting = false;
		ResetValue();
	}

	void ResetValue ()
	{
		m_Begin = new Vector3(-10000, -10000, -10000);
		m_End = new Vector3(-10000, -10000, -10000);
		m_Target.rch.point = new Vector3(-10000, -10000, -10000);
		m_Target.cursor = new Vector3(-10000, -10000, -10000);
		m_Target.snapto = new Vector3(-10000, -10000, -10000);
	}

	protected virtual bool AfterSelectionUpdate()
	{
		return true;
	}

	/// <summary>
	/// Calculates the selection 
	/// </summary>
	/// <param name="mode">Mode : 0 = add, 1 = substract, 2 = cross</param>
	protected bool CalcSelection (int mode)
	{

		Vector3 iMin = Vector3.zero;
		Vector3 iMax = Vector3.zero;
		iMin.x = Mathf.Min(m_Begin.x, m_End.x);
		iMin.y = Mathf.Min(m_Begin.y, m_End.y);
		iMin.z = Mathf.Min(m_Begin.z, m_End.z);
		iMax.x = Mathf.Max(m_Begin.x, m_End.x);
		iMax.y = Mathf.Max(m_Begin.y, m_End.y);
		iMax.z = Mathf.Max(m_Begin.z, m_End.z);

		Dictionary<IntVector3, byte> selection = new Dictionary<IntVector3, byte>();
		Dictionary<IntVector3, byte> removes  = new Dictionary<IntVector3, byte>();


		if (mode == 0)
		{
			for (float x = iMin.x; x < iMax.x; x += dataSource.Scale)
			{
				for (float y = iMin.y; y < iMax.y; y += dataSource.Scale)
				{
					for (float z = iMin.z; z < iMax.z; z += dataSource.Scale)
					{
						IntVector3 ipos = new IntVector3(Mathf.FloorToInt(x * dataSource.ScaleInverted),
						                                 Mathf.FloorToInt(y * dataSource.ScaleInverted),
						                                 Mathf.FloorToInt(z * dataSource.ScaleInverted));


						List<IntVector4> ext_posList = null;
						List<BSVoxel> ext_voxels = null;
						if (dataSource.ReadExtendableBlock(new IntVector4(ipos, 0), out ext_posList, out ext_voxels))
						{
							for (int i = 0; i < ext_voxels.Count; i++)
							{
								IntVector3 _ipos = new IntVector3(ext_posList[i].x, ext_posList[i].y, ext_posList[i].z);
								if (m_Selections.ContainsKey(_ipos))
									continue;

								selection[_ipos] = (byte)0xfe;
							}
						}
						else
						{
							if (m_Selections.ContainsKey(ipos))
								continue;

							BSVoxel voxel = dataSource.Read(ipos.x, ipos.y, ipos.z);
							if (!dataSource.VoxelIsZero(voxel, 1))
							{
								selection[ipos] = (byte)0xfe;
							}
						}
					}
				}
			}
		}
		else if (mode == 1)
		{
			for (float x = iMin.x; x < iMax.x; x += dataSource.Scale)
			{
				for (float y = iMin.y; y < iMax.y; y += dataSource.Scale)
				{
					for (float z = iMin.z; z < iMax.z; z += dataSource.Scale)
					{
						IntVector3 ipos = new IntVector3(Mathf.FloorToInt(x * dataSource.ScaleInverted),
						                                 Mathf.FloorToInt(y * dataSource.ScaleInverted),
						                                 Mathf.FloorToInt(z * dataSource.ScaleInverted));
						
//						if (!m_Selections.ContainsKey(ipos))
//							continue;

						List<IntVector4> ext_posList = null;
						List<BSVoxel> ext_voxels = null;

						if (dataSource.ReadExtendableBlock(new IntVector4(ipos, 0), out ext_posList, out ext_voxels))
						{
							for (int i = 0; i < ext_voxels.Count; i++)
							{
								IntVector3 _ipos = new IntVector3(ext_posList[i].x, ext_posList[i].y, ext_posList[i].z);
								if (!m_Selections.ContainsKey(_ipos))
									continue;
								
								removes[_ipos] = new byte();
							}
						}
						else
						{
							if (!m_Selections.ContainsKey(ipos))
								continue;

							removes[ipos] = new byte();
						}

					}
				}
			}
		}
		else if (mode == 2)
		{
			for (float x = iMin.x; x < iMax.x; x += dataSource.Scale)
			{
				for (float y = iMin.y; y < iMax.y; y += dataSource.Scale)
				{
					for (float z = iMin.z; z < iMax.z; z += dataSource.Scale)
					{
						IntVector3 ipos = new IntVector3(Mathf.FloorToInt(x * dataSource.ScaleInverted),
						                                 Mathf.FloorToInt(y * dataSource.ScaleInverted),
						                                 Mathf.FloorToInt(z * dataSource.ScaleInverted));

						List<IntVector4> ext_posList = null;
						List<BSVoxel> ext_voxels = null;

						if (dataSource.ReadExtendableBlock(new IntVector4(ipos, 0), out ext_posList, out ext_voxels))
						{
							bool remove = false;
							for (int i = 0; i < ext_voxels.Count; i++)
							{
								if (m_Selections.ContainsKey(new IntVector3(ext_posList[i].x, ext_posList[i].y, ext_posList[i].z)))
								{
									for (int _i = 0; _i < ext_voxels.Count; _i++)
									{
										IntVector3 _ipos = new IntVector3(ext_posList[_i].x, ext_posList[_i].y, ext_posList[_i].z);
										removes[_ipos] = new byte();
									}
									remove = true;
									break;
								}

							}

							if (!remove)
							{
								for (int i = 0; i < ext_voxels.Count; i++)
								{
									selection[new IntVector3(ext_posList[i].x, ext_posList[i].y, ext_posList[i].z)] = (byte)0xfe;
								}
							}
						}
						else
						{

							if (m_Selections.ContainsKey(ipos))
								removes[ipos] = (byte)0xfe;
							else
							{
								BSVoxel voxel = dataSource.Read(ipos.x, ipos.y, ipos.z);
								if (!dataSource.VoxelIsZero(voxel, 1))
								{
									selection[ipos] = (byte)0xfe;
								}
							}
						}
					}
				}
			}

		}


		if (selection.Count - removes.Count + m_Selections.Count > MaxVoxelCount)
		{
			selection.Clear();
			removes.Clear();
			return false;
		}

		// For History
		Dictionary<IntVector3, byte> old_seletion = new Dictionary<IntVector3, byte>(m_Selections);
		bool changed = false;


		foreach (IntVector3 key in removes.Keys)
		{
			changed = true;
			RemoveSelection(key);
		}
		
		
		foreach (KeyValuePair<IntVector3, byte> kvp in selection)
		{
			changed = true;
			AddSelection(kvp.Key, kvp.Value);
		}
		
		selection.Clear();
		removes.Clear();

		// For History
		if (changed)
		{
			Dictionary<IntVector3, byte> new_seletion = new Dictionary<IntVector3, byte>(m_Selections);
			BSSelectedBoxModify modify = new BSSelectedBoxModify(old_seletion, new_seletion, this);
			m_Action.AddModify(modify);
		}

		return true;
	}




	#region UI_TIP
	private float m_GUIAlpha;
	void OnGUI ()
	{
		if ( BSInput.s_MouseOnUI )
			return;
		GUI.skin = VCEditor.Instance.m_GUISkin;
		
		GUI.color = Color.white;
		if ( m_Selecting )
		{
			GUI.color = Color.white;
		}
		else
		{
			GUI.color = new Color(1,1,1,Mathf.Clamp01(m_GUIAlpha));
		}
		if ( Depth > 1 )
			GUI.Label( new Rect(Input.mousePosition.x + 26, Screen.height - Input.mousePosition.y + 5, 100,100), "Depth x " + Depth.ToString(), "CursorText2" );
		GUI.color = new Color(1,1,1,0.5f);
		if ( VCEInput.s_Shift )
			GUI.Label( new Rect(Input.mousePosition.x - 105, Screen.height - Input.mousePosition.y - 75, 100,100), "ADD", "CursorText1" );
		else if ( VCEInput.s_Alt )
			GUI.Label( new Rect(Input.mousePosition.x - 105, Screen.height - Input.mousePosition.y - 75, 100,100), "SUBTRACT", "CursorText1" );
		else if ( VCEInput.s_Control )
			GUI.Label( new Rect(Input.mousePosition.x - 105, Screen.height - Input.mousePosition.y - 75, 100,100), "CROSS", "CursorText1" );

		if (m_Selecting)
		{
			int voxel_count = m_Selections.Count;

			Color color = voxelsCountColor.Evaluate((float)voxel_count / MaxVoxelCount);
			GUI.color = color;
			GUI.Label( new Rect(Input.mousePosition.x + 26, Screen.height - Input.mousePosition.y + 58, 100,100), "Selected Voxels: " + voxel_count.ToString(), "CursorText2" );

			Vector3 iMin = Vector3.zero;
			Vector3 iMax = Vector3.zero;
			iMin.x = Mathf.Min(m_Begin.x, m_End.x);
			iMin.y = Mathf.Min(m_Begin.y, m_End.y);
			iMin.z = Mathf.Min(m_Begin.z, m_End.z);
			iMax.x = Mathf.Max(m_Begin.x, m_End.x);
			iMax.y = Mathf.Max(m_Begin.y, m_End.y);
			iMax.z = Mathf.Max(m_Begin.z, m_End.z);

			IntVector3 size = new IntVector3((iMax.x - iMin.x) * dataSource.ScaleInverted, (iMax.y - iMin.y) * dataSource.ScaleInverted, (iMax.z - iMin.z) * dataSource.ScaleInverted);
			string text = "Pre-Selection: " + size.x.ToString() + " x " + size.z.ToString() + " x " + size.y.ToString();
			GUI.color = new Color(1,1,1,0.8f);
			GUI.Label( new Rect(Input.mousePosition.x + 26, Screen.height - Input.mousePosition.y + 31, 100,100), text, "CursorText2" );
		}


	}
	#endregion

	#region  GL_DRAWING  Quad & Selecting_Box
	public override void OnGL ()
	{
		if (dataSource == null)
			return;
		
		if (!gameObject.activeInHierarchy || !enabled)
			return;
	
		if (m_Target.ds == null)
			return;

		if (!m_Drawing)
			return;

		if (m_Material == null)
		{
			m_Material = new Material(Shader.Find("Lines/Colored Blended"));
			
			m_Material.hideFlags = HideFlags.HideAndDontSave;
			m_Material.shader.hideFlags = HideFlags.HideAndDontSave;
		}



		// Draw selecting box
		if (m_Selecting)
		{
			// Save camera's matrix.
			GL.PushMatrix();

			Vector3 iMin = Vector3.zero;
			Vector3 iMax = Vector3.zero;
			iMin.x = Mathf.Min(m_Begin.x, m_End.x);
			iMin.y = Mathf.Min(m_Begin.y, m_End.y);
			iMin.z = Mathf.Min(m_Begin.z, m_End.z);
			iMax.x = Mathf.Max(m_Begin.x, m_End.x);
			iMax.y = Mathf.Max(m_Begin.y, m_End.y);
			iMax.z = Mathf.Max(m_Begin.z, m_End.z);

			iMax += dataSource.Offset;
			iMin += dataSource.Offset;

			float shrink = 0.02f;
			if (Camera.main != null)
			{
				float dist_min = (Camera.main.transform.position - iMin).magnitude;
				float dist_max = (Camera.main.transform.position - iMax).magnitude;

				dist_max = dist_max > dist_min ? dist_max : dist_min;

				shrink = Mathf.Clamp(dist_max * 0.001f, 0.02f, 0.1f);
			}
			

			iMax += new Vector3(shrink, shrink, shrink);
			iMin -= new Vector3(shrink, shrink, shrink);

			Vector3[] vec = new Vector3 [8]
			{
				new Vector3(iMax.x, iMax.y, iMax.z),
				new Vector3(iMin.x, iMax.y, iMax.z),
				new Vector3(iMin.x, iMin.y, iMax.z),
				new Vector3(iMax.x, iMin.y, iMax.z),
				new Vector3(iMax.x, iMax.y, iMin.z),
				new Vector3(iMin.x, iMax.y, iMin.z),
				new Vector3(iMin.x, iMin.y, iMin.z),
				new Vector3(iMax.x, iMin.y, iMin.z)
			};

			for ( int i = 0; i < 8; ++i )
				vec[i] = vec[i];
			
			Color lineColor = new Color(0.0f, 0.3f, 0.6f, 1.0f);
			Color faceColor = new Color(0.0f, 0.3f, 0.6f, 1.0f);

			if (!m_IsValidBox)
			{
				lineColor = new Color(0.67f, 0.1f, 0.1f);
				faceColor = lineColor;
			}

			lineColor.a = 1.0f;
			faceColor.a *= 0.4f + Mathf.Sin(Time.time*6f) * 0.1f;

			for (int i = 0; i < m_Material.passCount; i++)
			{
				m_Material.SetPass(i);
				GL.Begin(GL.LINES);
				GL.Color(lineColor);
				GL.Vertex(vec[0]); GL.Vertex(vec[1]); GL.Vertex(vec[1]); GL.Vertex(vec[2]);
				GL.Vertex(vec[2]); GL.Vertex(vec[3]); GL.Vertex(vec[3]); GL.Vertex(vec[0]);
				GL.Vertex(vec[4]); GL.Vertex(vec[5]); GL.Vertex(vec[5]); GL.Vertex(vec[6]);
				GL.Vertex(vec[6]); GL.Vertex(vec[7]); GL.Vertex(vec[7]); GL.Vertex(vec[4]);
				GL.Vertex(vec[0]); GL.Vertex(vec[4]); GL.Vertex(vec[1]); GL.Vertex(vec[5]);
				GL.Vertex(vec[2]); GL.Vertex(vec[6]); GL.Vertex(vec[3]); GL.Vertex(vec[7]);
				GL.End();
				
				GL.Begin(GL.QUADS);
				GL.Color(faceColor);
				GL.Vertex(vec[0]); GL.Vertex(vec[1]); GL.Vertex(vec[2]); GL.Vertex(vec[3]);
				GL.Vertex(vec[4]); GL.Vertex(vec[5]); GL.Vertex(vec[6]); GL.Vertex(vec[7]);
				GL.Vertex(vec[0]); GL.Vertex(vec[4]); GL.Vertex(vec[5]); GL.Vertex(vec[1]);
				GL.Vertex(vec[1]); GL.Vertex(vec[5]); GL.Vertex(vec[6]); GL.Vertex(vec[2]);
				GL.Vertex(vec[2]); GL.Vertex(vec[6]); GL.Vertex(vec[7]); GL.Vertex(vec[3]);
				GL.Vertex(vec[3]); GL.Vertex(vec[7]); GL.Vertex(vec[4]); GL.Vertex(vec[0]);
				GL.End();
			}

			// Restore camera's matrix.
			GL.PopMatrix();
		}
		// Draw pre-select face direction
		else
		{
			if (BSInput.s_MouseOnUI)
				return;

			// Save camera's matrix.
			GL.PushMatrix();

			BSMath.DrawTarget dtar = m_Target;
			Vector3 iMin = dtar.rch.point;
			Vector3 iMax = dtar.rch.point;

			Color indicator_color = Color.white;

			// zy plane
			if ( Mathf.Abs(dtar.rch.normal.x) > 0.9f * dtar.ds.Scale )
			{
				iMin.y = Mathf.Floor(iMin.y * dataSource.ScaleInverted) * dataSource.Scale;
				iMin.z = Mathf.Floor(iMin.z * dataSource.ScaleInverted) * dataSource.Scale;
				iMax.y = Mathf.Floor(iMax.y * dataSource.ScaleInverted) * dataSource.Scale + dataSource.Scale;
				iMax.z = Mathf.Floor(iMax.z * dataSource.ScaleInverted) * dataSource.Scale + dataSource.Scale;
				indicator_color = new Color(0.9f, 0.1f, 0.2f, 1.0f);
			}
			// xz plane
			else if ( Mathf.Abs(dtar.rch.normal.y) > 0.9f * dtar.ds.Scale)
			{
				iMin.x = Mathf.Floor(iMin.x * dataSource.ScaleInverted) * dataSource.Scale;
				iMin.z = Mathf.Floor(iMin.z * dataSource.ScaleInverted) * dataSource.Scale;
				iMax.x = Mathf.Floor(iMax.x * dataSource.ScaleInverted) * dataSource.Scale + dataSource.Scale;
				iMax.z = Mathf.Floor(iMax.z * dataSource.ScaleInverted) * dataSource.Scale + dataSource.Scale;
				indicator_color = new Color(0.5f, 1.0f, 0.1f, 1.0f);
			}
			// xy plane
			else if ( Mathf.Abs(dtar.rch.normal.z) > 0.9f * dtar.ds.Scale)
			{
				iMin.y = Mathf.Floor(iMin.y * dataSource.ScaleInverted) * dataSource.Scale;
				iMin.x = Mathf.Floor(iMin.x * dataSource.ScaleInverted) * dataSource.Scale;
				iMax.y = Mathf.Floor(iMax.y * dataSource.ScaleInverted) * dataSource.Scale + dataSource.Scale;
				iMax.x = Mathf.Floor(iMax.x * dataSource.ScaleInverted) * dataSource.Scale + dataSource.Scale;
				indicator_color = new Color(0.1f, 0.6f, 1.0f, 1.0f);
			}

			iMax += dataSource.Offset;
			iMin += dataSource.Offset;

			float shrink = 0.02f;
			if (Camera.main != null)
			{
				float dist_min = (Camera.main.transform.position - iMin).magnitude;
				float dist_max = (Camera.main.transform.position - iMax).magnitude;
				
				dist_max = dist_max > dist_min ? dist_max : dist_min;
				
				shrink = Mathf.Clamp(dist_max * 0.002f, 0.02f, 0.1f);
			}

			iMax += new Vector3(shrink, shrink, shrink);
			iMin -= new Vector3(shrink, shrink, shrink);

			Vector3[] vec = new Vector3 [8]
			{
				new Vector3(iMax.x, iMax.y, iMax.z),
				new Vector3(iMin.x, iMax.y, iMax.z),
				new Vector3(iMin.x, iMin.y, iMax.z),
				new Vector3(iMax.x, iMin.y, iMax.z),
				new Vector3(iMax.x, iMax.y, iMin.z),
				new Vector3(iMin.x, iMax.y, iMin.z),
				new Vector3(iMin.x, iMin.y, iMin.z),
				new Vector3(iMax.x, iMin.y, iMin.z)
			};

			for ( int i = 0; i < 8; ++i )
				vec[i] = vec[i];
			
			Color lineColor = indicator_color;
			Color faceColor = indicator_color;


			
			lineColor.a = 1.0f;
			faceColor.a *= 0.7f + Mathf.Sin(Time.time*6f) * 0.1f;

			for (int i = 0; i < m_Material.passCount; i++)
			{
				m_Material.SetPass(i);
				GL.Begin(GL.LINES);
				GL.Color(lineColor);
				GL.Vertex(vec[0]); GL.Vertex(vec[1]); GL.Vertex(vec[1]); GL.Vertex(vec[2]);
				GL.Vertex(vec[2]); GL.Vertex(vec[3]); GL.Vertex(vec[3]); GL.Vertex(vec[0]);
				GL.Vertex(vec[4]); GL.Vertex(vec[5]); GL.Vertex(vec[5]); GL.Vertex(vec[6]);
				GL.Vertex(vec[6]); GL.Vertex(vec[7]); GL.Vertex(vec[7]); GL.Vertex(vec[4]);
				GL.Vertex(vec[0]); GL.Vertex(vec[4]); GL.Vertex(vec[1]); GL.Vertex(vec[5]);
				GL.Vertex(vec[2]); GL.Vertex(vec[6]); GL.Vertex(vec[3]); GL.Vertex(vec[7]);
				GL.End();
				
				GL.Begin(GL.QUADS);
				GL.Color(faceColor);
				GL.Vertex(vec[0]); GL.Vertex(vec[1]); GL.Vertex(vec[2]); GL.Vertex(vec[3]);
				GL.Vertex(vec[4]); GL.Vertex(vec[5]); GL.Vertex(vec[6]); GL.Vertex(vec[7]);
				GL.Vertex(vec[0]); GL.Vertex(vec[4]); GL.Vertex(vec[5]); GL.Vertex(vec[1]);
				GL.Vertex(vec[1]); GL.Vertex(vec[5]); GL.Vertex(vec[6]); GL.Vertex(vec[2]);
				GL.Vertex(vec[2]); GL.Vertex(vec[6]); GL.Vertex(vec[7]); GL.Vertex(vec[3]);
				GL.Vertex(vec[3]); GL.Vertex(vec[7]); GL.Vertex(vec[4]); GL.Vertex(vec[0]);
				GL.End();
			}

			// Restore camera's matrix.
			GL.PopMatrix();
		}



	}
	#endregion
}
