using UnityEngine;
using System.Collections;

public class BSFreeSizeBrush : BSBrush 
{
	[SerializeField] protected BSGizmoTriggerEvent gizmoTrigger;

	public override Bounds brushBound { get { return gizmoTrigger.boxCollider.bounds; } }

	public Vector3 maxDragSize;

	protected BSMath.DrawTarget m_Target;

	public enum EPhase
	{
		Free,
		DragPlane,
		AdjustHeight,
		Drawing
	}

	protected EPhase m_Phase = EPhase.Free;

	protected Vector3 m_Begin;
	protected Vector3 m_End;

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

	public BSGizmoCubeMesh gizmoCube;
	
	public ECoordPlane DragPlane = ECoordPlane.XZ; 


	private Vector3 m_PointBeforeAdjustHeight;
	protected Vector3 m_Cursor = Vector3.zero;
	protected Vector3 m_GizmoCursor = Vector3.zero;

	protected void Start () 
	{
		GlobalGLs.AddGL(this);
	}

	Vector3 _beginPos = Vector3.zero;
	Vector3 _prevMousePos = Vector3.zero;
	protected void Update () 
	{
		if (dataSource == null)
			return;

		if (pattern == null)
			return;

		if ( !ExtraAdjust() )
			return;

        if (GameConfig.IsInVCE)
            return;

        gizmoCube.m_VoxelSize = dataSource.Scale;

		if (m_Phase == EPhase.Free)
		{
			if (BSInput.s_MouseOnUI)
			{
				gizmoCube.gameObject.SetActive(false);
				return;
			}

			if (BSInput.s_Cancel)
			{
				ResetDrawing ();
			}

			// Ray cast voxel
			Ray ray = Camera.main.ScreenPointToRay( Input.mousePosition);

			bool ignoreDiagonal = (mode == EBSBrushMode.Add);
			bool cast = BSMath.RayCastDrawTarget(ray, dataSource, out m_Target, minvol, ignoreDiagonal, BuildingMan.Datas);
			if (cast)
			{
				Vector3 cursor = Vector3.zero;

				if (mode == EBSBrushMode.Add)
				{
					cursor = CalcCursor(m_Target, dataSource, pattern.size);

					m_Cursor = m_Target.rch.point;

				}
				else if (mode == EBSBrushMode.Subtract)
				{
					cursor = CalcSnapto(m_Target, dataSource, pattern);

					m_Cursor = m_Target.rch.point;
				}
				m_GizmoCursor = cursor;

				_drawGL = true;
				gizmoCube.CubeSize = new IntVector3(pattern.size, pattern.size, pattern.size);
				gizmoCube.gameObject.SetActive (true);
				gizmoCube.transform.position = cursor + dataSource.Offset;

				UpdateGizmoTrigger();

				float offset = pattern.size * dataSource.Scale;
				Vector3 begin = cursor;
				Vector3 end = begin + new Vector3(offset, offset, offset);
				_glCenter =  cursor + new Vector3(offset, offset, offset) * 0.5f + dataSource.Offset;
				_glCenter.y = begin.y;

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
				ResetDrawing();

			// Switch the panel
			if (Input.GetKeyDown(KeyCode.G))
			{
				if (DragPlane == ECoordPlane.XZ)
					DragPlane = ECoordPlane.ZY;
				else if (DragPlane == ECoordPlane.ZY)
					DragPlane = ECoordPlane.XY;
				else if (DragPlane == ECoordPlane.XY)
					DragPlane = ECoordPlane.XZ;

				Debug.Log("Switch the drag reference plane : " + DragPlane.ToString());
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

					// other phase Drag plane
					DragPlaneExtraDo(DragPlane);

					gizmoCube.transform.position = Min +  dataSource.Offset;

					gizmoCube.CubeSize = new IntVector3((int)Size.x, (int)Size.y, (int)Size.z);

					UpdateGizmoTrigger();
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

//			ECoordAxis axis = ECoordAxis.Y;
			if (DragPlane == ECoordPlane.XZ)
			{
				float adjustHeight = m_PointBeforeAdjustHeight.y;
				BSMath.RayAdjustHeight(ray, ECoordAxis.Y,  m_PointBeforeAdjustHeight, out adjustHeight);

				// y
				float y = 0;
				m_Begin.y = CalcValue(adjustHeight, _beginPos.y, out y);
				m_End.y = y + m_EndOffset.y;

			}
			else if (DragPlane == ECoordPlane.XY)
			{
				float adjustHeight = m_PointBeforeAdjustHeight.z;
				BSMath.RayAdjustHeight(ray, ECoordAxis.Z, m_PointBeforeAdjustHeight, out adjustHeight);


				// z 
				float z = 0;
				m_Begin.z = CalcValue(adjustHeight, _beginPos.z, out z);
				m_End.z = z + m_EndOffset.z;
			}
			else if (DragPlane == ECoordPlane.ZY)
			{
				float adjustHeight = m_PointBeforeAdjustHeight.x;
				BSMath.RayAdjustHeight(ray, ECoordAxis.X, m_PointBeforeAdjustHeight, out adjustHeight);

				// x
				float x = 0;
				m_Begin.x = CalcValue(adjustHeight, _beginPos.x, out x);

				m_End.x = x + m_EndOffset.x;
			}

			m_End = Clamp(m_Begin, m_End);

			// Other adjustHeight
			AdjustHeightExtraDo(DragPlane);


			gizmoCube.transform.position = Min +  dataSource.Offset;
			
			gizmoCube.CubeSize = new IntVector3((int)Size.x, (int)Size.y, (int)Size.z);

			UpdateGizmoTrigger();

            // Click mouse and change phase
            //if (Input.GetMouseButtonDown(0))
            if (PeInput.Get(PeInput.LogicFunction.Build))
            {
				Do();
				m_Phase = EPhase.Free;
			}
		}
	

		AfterDo();
	}

	void UpdateGizmoTrigger ()
	{
		// Has gizmo Trigger?
		if (gizmoTrigger)
		{
			Vector3 trigget_size = new Vector3(gizmoCube.CubeSize.x * dataSource.Scale, 
			                                   gizmoCube.CubeSize.y * dataSource.Scale,
			                                   gizmoCube.CubeSize.z * dataSource.Scale);
			gizmoTrigger.boxCollider.size =  trigget_size;
			gizmoTrigger.boxCollider.center = trigget_size * 0.5f;
		}
	}

	// Fine tuning the relative height
	Vector3 m_EndOffset = Vector3.zero;
	void FineTuningHeight (ECoordAxis axis)
	{
		// Fine tuning the relative height
		if (Input.GetKeyDown(KeyCode.UpArrow))
		{
			if (axis == ECoordAxis.X)
			{
				m_EndOffset.x += dataSource.Scale * pattern.size;

			}
			else if (axis == ECoordAxis.Y)
			{
				m_EndOffset.y += dataSource.Scale * pattern.size;
			}
			else if (axis == ECoordAxis.Z)
			{
				m_EndOffset.z += dataSource.Scale * pattern.size;
			}
		}
		else if (Input.GetKeyDown(KeyCode.DownArrow))
		{
			if (axis == ECoordAxis.X)
			{
				m_EndOffset.x -= dataSource.Scale * pattern.size;
			}
			else if (axis == ECoordAxis.Y)
			{
				m_EndOffset.y -= dataSource.Scale * pattern.size;
			}
			else if (axis == ECoordAxis.Z)
			{
				m_EndOffset.z -= dataSource.Scale * pattern.size;
			}
		}

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
		gizmoCube.CubeSize = new IntVector3(pattern.size, pattern.size, pattern.size);
		gizmoCube.gameObject.SetActive (false);
		m_EndOffset = Vector3.zero;
		_drawGL = false;
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


	#region VIRTUAL_FUNC

	protected override void Do ()
	{
		
	}

	protected virtual bool ExtraAdjust()
	{
		return true;
	}

	protected virtual void AfterDo ()
	{

	}

	protected virtual void AdjustHeightExtraDo (ECoordPlane drag_plane)
	{

	}

	protected virtual void DragPlaneExtraDo (ECoordPlane drag_plane)
	{

	}

	#endregion


	#region GUI_FUNC
	public string ExtraTips = "";
	void OnGUI ()
	{
		if (BuildingMan.Self != null)
			GUI.skin = BuildingMan.Self.guiSkin;
		if ( m_Phase != EPhase.Free )
		{
			IntVector3 size = Size;
			string text = size.x.ToString() + " x " + size.z.ToString() + " x " + size.y.ToString();
			GUI.color = new Color(1,1,1,0.8f);
			GUI.Label( new Rect(Input.mousePosition.x + 24, Screen.height - Input.mousePosition.y + 26, 100,100), text, "CursorText2" );
			if (!string.IsNullOrEmpty(ExtraTips))
			{
				GUI.color = Color.yellow;
				GUI.Label( new Rect(Input.mousePosition.x + 24, Screen.height - Input.mousePosition.y + 66, 100,100), "("+ExtraTips+")", "CursorText2" );
				GUI.color = Color.white;
			}
		}
	}
	#endregion

	#region GL_DRAWING  CIRCLE & ARROW
	Vector3 _glCenter;
	public float circleRadius = 3.0f;

	public virtual bool CanDrawGL()
	{
		return true;
	}

	bool _drawGL = true;
	public override void OnGL ()
	{
		if (dataSource == null)
			return;

		if (pattern == null)
			return;

		if (!gameObject.activeInHierarchy || !enabled)
			return;

		if (!_drawGL || !CanDrawGL())
			return;

		if (BSInput.s_MouseOnUI)
			return;

		if (m_Material == null)
		{
//			m_Material = new Material(Shader.Find("Lines/Colored Blended"));
			m_Material = new Material(Shader.Find("Unlit/Transparent Colored"));

			m_Material.hideFlags = HideFlags.HideAndDontSave;
			m_Material.shader.hideFlags = HideFlags.HideAndDontSave;
		}

		// Save camera's matrix.
		GL.PushMatrix();

		Color bc = Color.white;
		if (DragPlane == ECoordPlane.XZ)
		{
			bc = new Color(0, 1f, 0.07f, 0.05f);
		}
		else if (DragPlane == ECoordPlane.XY)
		{
			bc = new Color(0.29f, 0.5f, 0.9f, 0.05f);
		}
		else if (DragPlane == ECoordPlane.ZY)
		{
			bc = new Color(1f, 0.32f, 0.42f, 0.05f);
		}


		Vector3 dir = Camera.main == null ? Vector3.up : Camera.main.transform.forward;

		if (DragPlane == ECoordPlane.XZ)
		{
			for (int i = 0; i < m_Material.passCount; i++)
			{
				m_Material.SetPass(i);

				// Draw Arrow
				GL.Begin(GL.LINES);

				Color lc = bc * 1.2f;
				lc.a = 0.7f;
				GL.Color(lc);

				Vector3 drawpos = _glCenter;
				GL.Vertex3(drawpos.x, drawpos.y - 0.2f, drawpos.z);
				GL.Vertex3(drawpos.x, drawpos.y + 2.5f, drawpos.z);
				GL.Vertex3(drawpos.x, drawpos.y + 2.5f, drawpos.z);
				GL.Vertex3(drawpos.x + 0.2f, drawpos.y + 2f, drawpos.z);
				GL.Vertex3(drawpos.x, drawpos.y + 2.5f, drawpos.z);
				GL.Vertex3(drawpos.x - 0.2f, drawpos.y + 2f, drawpos.z);
				GL.Vertex3(drawpos.x, drawpos.y + 2.5f, drawpos.z);
				GL.Vertex3(drawpos.x, drawpos.y + 2f, drawpos.z + 0.2f);
				GL.Vertex3(drawpos.x, drawpos.y + 2.5f, drawpos.z);
				GL.Vertex3(drawpos.x, drawpos.y + 2f, drawpos.z - 0.2f);
				GL.End();

				//GL.Begin(GL.TRIANGLES);

				//GL.Color(bc);

//				// Draw circle
//				int cnt = 30;
//				Vector3 _center = _glCenter;
//				_center.y += 0.1F;
//				for (float c = 1; c <= cnt; c ++)
//				{
//					Vector3 v1;
//					Vector3 v2;
//
//					float thea =  (c - 1)/cnt * ( 2 * Mathf.PI);
//					v1.x = circleRadius * Mathf.Cos(thea);
//					v1.z = circleRadius * Mathf.Sin(thea);
//					v1.y = 0;
//					v1 += _center;
//
//					thea = c / cnt * (2 * Mathf.PI);
//					v2.x = circleRadius * Mathf.Cos(thea);
//					v2.z = circleRadius * Mathf.Sin(thea);
//					v2.y = 0;
//					v2 += _center;
//				
//
//					GL.Vertex(v2);
//					GL.Vertex(_center);
//					GL.Vertex(v1);
//				}
//
//				GL.End();

			}
		}
		else if (DragPlane == ECoordPlane.XY)
		{
			for (int i = 0; i < m_Material.passCount; i++)
			{
				m_Material.SetPass(i);

				// Draw Arrow
				GL.Begin(GL.LINES);
				
				Color lc = bc * 1.2f;
				lc.a = 0.7f;
				GL.Color(lc);

				int sign = dir.z >= 0 ? -1 : 1;
				Vector3 drawpos = _glCenter;
				GL.Vertex3(drawpos.x, drawpos.y, drawpos.z - 0.2f * sign);
				GL.Vertex3(drawpos.x, drawpos.y, drawpos.z + 2.5f * sign);
				GL.Vertex3(drawpos.x, drawpos.y, drawpos.z + 2.5f * sign);
				GL.Vertex3(drawpos.x + 0.2f, drawpos.y, drawpos.z + 2.0f * sign);
				GL.Vertex3(drawpos.x, drawpos.y, drawpos.z + 2.5f * sign);
				GL.Vertex3(drawpos.x - 0.2f, drawpos.y, drawpos.z + 2f * sign);
				GL.Vertex3(drawpos.x, drawpos.y, drawpos.z + 2.5f * sign);
				GL.Vertex3(drawpos.x, drawpos.y + 0.2f, drawpos.z + 2.0f * sign);
				GL.Vertex3(drawpos.x, drawpos.y, drawpos.z + 2.5f * sign);
				GL.Vertex3(drawpos.x, drawpos.y -0.2f, drawpos.z + 2.0f * sign);
				GL.End();


//				// Draw circle
//				GL.Begin(GL.TRIANGLES);
//				
//				GL.Color(bc);
//				
//				int cnt = 30;
//				for (float c = 1; c <= cnt; c ++)
//				{
//					Vector3 v1;
//					Vector3 v2;
//					
//					float thea =  (c - 1)/cnt * ( 2 * Mathf.PI);
//					v1.x = circleRadius * Mathf.Cos(thea);
//					v1.z = 0;
//					v1.y = circleRadius * Mathf.Sin(thea);
//					v1 += _glCenter;
//					
//					thea = c / cnt * (2 * Mathf.PI);
//					v2.x = circleRadius * Mathf.Cos(thea);
//					v2.z = 0;
//					v2.y = circleRadius * Mathf.Sin(thea);
//					v2 += _glCenter;
//					
//					GL.Vertex(v2);
//					GL.Vertex(_glCenter);
//					GL.Vertex(v1);
//				}
//				
//				GL.End();
			}
		}
		else if (DragPlane == ECoordPlane.ZY)
		{
			for (int i = 0; i < m_Material.passCount; i++)
			{
				m_Material.SetPass(i);

				// Draw Arrow
				GL.Begin(GL.LINES);
				
				Color lc = bc * 1.2f;
				lc.a = 0.7f;
				GL.Color(lc);

				int sign = dir.x >= 0 ? -1 : 1;
				Vector3 drawpos = _glCenter;
				GL.Vertex3(drawpos.x - 0.2f * sign, drawpos.y, drawpos.z);
				GL.Vertex3(drawpos.x + 2.5f * sign, drawpos.y, drawpos.z);
				GL.Vertex3(drawpos.x + 2.5f * sign, drawpos.y, drawpos.z);
				GL.Vertex3(drawpos.x + 2.0f * sign, drawpos.y, drawpos.z + 0.2f);
				GL.Vertex3(drawpos.x + 2.5f * sign, drawpos.y, drawpos.z);
				GL.Vertex3(drawpos.x + 2.0f * sign, drawpos.y, drawpos.z - 0.2f);
				GL.Vertex3(drawpos.x + 2.5f * sign, drawpos.y, drawpos.z);
				GL.Vertex3(drawpos.x + 2.0f * sign, drawpos.y + 0.2f, drawpos.z);
				GL.Vertex3(drawpos.x + 2.5f * sign, drawpos.y, drawpos.z);
				GL.Vertex3(drawpos.x + 2.0f * sign, drawpos.y -0.2f, drawpos.z);
				GL.End();

//				// Draw circle
//				GL.Begin(GL.TRIANGLES);
//
//				GL.Color(bc);
//				
//				int cnt = 30;
//				for (float c = 1; c <= cnt; c ++)
//				{
//					Vector3 v1;
//					Vector3 v2;
//					
//					float thea =  (c - 1)/cnt * ( 2 * Mathf.PI);
//					v1.x = 0;
//					v1.z = circleRadius * Mathf.Cos(thea);
//					v1.y = circleRadius * Mathf.Sin(thea);
//					v1 += _glCenter;
//					
//					thea = c / cnt * (2 * Mathf.PI);
//					v2.x = 0;
//					v2.z = circleRadius * Mathf.Cos(thea);
//					v2.y = circleRadius * Mathf.Sin(thea);
//					v2 += _glCenter;
//					
//					GL.Vertex(v2);
//					GL.Vertex(_glCenter);
//					GL.Vertex(v1);
//				}
				
//				GL.End();
			}
		}

		// Restore camera's matrix.
		GL.PopMatrix();
	}

	#endregion

}
