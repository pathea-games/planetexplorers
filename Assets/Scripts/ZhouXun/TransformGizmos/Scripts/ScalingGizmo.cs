using UnityEngine;

public class ScalingGizmo : TransformGizmo
{
	public delegate void DNotify (Vector3 scale);
	public event DNotify OnBeginTargetScale;
	public event DNotify OnTargetScale;
	public event DNotify OnEndTargetScale;

	[SerializeField] Renderer XPointRenderer;
	[SerializeField] Renderer YPointRenderer;
	[SerializeField] Renderer ZPointRenderer;
	[SerializeField] Renderer OPointRenderer;

	[SerializeField] BoxCollider OCollider;
	[SerializeField] BoxCollider XCollider;
	[SerializeField] BoxCollider YCollider;
	[SerializeField] BoxCollider ZCollider;
	[SerializeField] BoxCollider XYCollider;
	[SerializeField] BoxCollider YZCollider;
	[SerializeField] BoxCollider ZXCollider;

	Material omat;
	Material xmat;
	Material ymat;
	Material zmat;

	protected override void Start ()
	{
		base.Start();
		xmat = Material.Instantiate(XPointRenderer.material) as Material;
		xmat.color = XColor;
		xmat.SetColor("_RimColor", XColor * 0.25f);
		XPointRenderer.material = xmat;

		ymat = Material.Instantiate(YPointRenderer.material) as Material;
		ymat.color = YColor;
		ymat.SetColor("_RimColor", YColor * 0.25f);
		YPointRenderer.material = ymat;
		
		zmat = Material.Instantiate(ZPointRenderer.material) as Material;
		zmat.color = ZColor;
		zmat.SetColor("_RimColor", ZColor * 0.25f);
		ZPointRenderer.material = zmat;
		
		omat = Material.Instantiate(OPointRenderer.material) as Material;
		omat.color = Color.white;
		omat.SetColor("_RimColor", Color.white * 0.25f);
		OPointRenderer.material = omat;

		XYCollider.enabled = !hideCenter;
		YZCollider.enabled = !hideCenter;
		ZXCollider.enabled = !hideCenter;
	}

	protected override void OnDestroy ()
	{
		Material.Destroy(xmat);
		Material.Destroy(ymat);
		Material.Destroy(zmat);
		Material.Destroy(omat);
		base.OnDestroy();
	}

	float xsign = 1;
	float ysign = 1;
	float zsign = 1;
	
	Color xlc;
	Color ylc;
	Color zlc;
	Color olc;
	Color xpc;
	Color ypc;
	Color zpc;
	
	bool focus_xl = false;
	bool focus_yl = false;
	bool focus_zl = false;
	bool focus_ol = false;

	bool focus_xp = false;
	bool focus_yp = false;
	bool focus_zp = false;
	
	bool xdragging = false;
	bool ydragging = false;
	bool zdragging = false;
	bool dragplane = false;
	Vector3 dragorigin = Vector3.zero;
	Vector3 xdragdir;
	Vector3 ydragdir;
	Vector3 zdragdir;

	Vector3 begin_pos_3d = Vector3.zero;
	Vector3 begin_tar_scale = Vector3.zero;
	Vector3 new_tar_scale = Vector3.zero;

	public override bool MouseOver
	{
		get
		{
			return enabled && Focus != null && (focus_xl || focus_xp || focus_yl || focus_yp ||
			                                    focus_zl || focus_zp || focus_ol ||
			                                    xdragging || ydragging || zdragging);
		}
	}

	public override bool Working
	{
		get
		{
			return enabled && Focus != null && (xdragging || ydragging || zdragging);
		}
	}

	protected override void Idle ()
	{
		bool on_ui = UnityEngine.EventSystems.EventSystem.current != null &&
			UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();

		Vector3 dir = (transform.position - GizmoGroup.position).normalized;
		xsign = Mathf.Sign(Vector3.Dot(dir, GizmoGroup.right));
		ysign = Mathf.Sign(Vector3.Dot(dir, GizmoGroup.up));
		zsign = Mathf.Sign(Vector3.Dot(dir, GizmoGroup.forward));
		
		XYCollider.transform.localPosition = new Vector3 (xsign * half * 0.5f, ysign * half * 0.5f, 0f);
		YZCollider.transform.localPosition = new Vector3 (0f, ysign * half * 0.5f, zsign * half * 0.5f);
		ZXCollider.transform.localPosition = new Vector3 (xsign * half * 0.5f, 0f, zsign * half * 0.5f);
		
		Ray ray = cam.ScreenPointToRay(mousePosition);
		RaycastHit rch = new RaycastHit ();
		if (!on_ui) Physics.Raycast(ray, out rch, 11.0f, 1 << gameObject.layer);
		float angle_x = Vector3.Angle(ray.direction, GizmoGroup.right);
		float angle_y = Vector3.Angle(ray.direction, GizmoGroup.up);
		float angle_z = Vector3.Angle(ray.direction, GizmoGroup.forward);
		
		focus_xl = (rch.collider == XCollider && angle_x > 5 && angle_x < 175 );
		focus_yl = (rch.collider == YCollider && angle_y > 5 && angle_y < 175 );
		focus_zl = (rch.collider == ZCollider && angle_z > 5 && angle_z < 175 );
		focus_ol = (rch.collider == OCollider);
		focus_xp = (rch.collider == YZCollider && Mathf.Abs(angle_x - 90) > 4);
		focus_yp = (rch.collider == ZXCollider && Mathf.Abs(angle_y - 90) > 4);
		focus_zp = (rch.collider == XYCollider && Mathf.Abs(angle_z - 90) > 4);
		xlc = focus_xl ? HColor : XColor;
		ylc = focus_yl ? HColor : YColor;
		zlc = focus_zl ? HColor : ZColor;
		olc = focus_ol ? HColor : Color.white;
		xpc = focus_xp || focus_ol ? HColor : XColor;
		ypc = focus_yp || focus_ol ? HColor : YColor;
		zpc = focus_zp || focus_ol ? HColor : ZColor;
		xpc.a = 0.3f;
		ypc.a = 0.3f;
		zpc.a = 0.3f;
		xmat.color = xlc;
		ymat.color = ylc;
		zmat.color = zlc;
		omat.color = olc;
		xdragging = false;
		ydragging = false;
		zdragging = false;
	}

	protected override void BeginModify ()
	{
		xdragging = focus_xl || focus_yp || focus_zp || focus_ol;
		ydragging = focus_yl || focus_zp || focus_xp || focus_ol;
		zdragging = focus_zl || focus_xp || focus_yp || focus_ol;
		
		if (xdragging || ydragging || zdragging)
		{
			xdragdir = (cam.WorldToScreenPoint(GizmoGroup.position + GizmoGroup.right) - mousePosition);
			ydragdir = (cam.WorldToScreenPoint(GizmoGroup.position + GizmoGroup.up) - mousePosition);
			zdragdir = (cam.WorldToScreenPoint(GizmoGroup.position + GizmoGroup.forward) - mousePosition);
			xdragdir.z = 0;
			ydragdir.z = 0;
			zdragdir.z = 0;
			xdragdir.Normalize();
			ydragdir.Normalize();
			zdragdir.Normalize();

			begin_pos_3d = Focus.position;
			begin_tar_scale = Focus.localScale;
			new_tar_scale = Focus.localScale;
			
			dragplane = GetDragPos_Plane(out dragorigin);

			if (OnBeginTargetScale != null)
			{
				OnBeginTargetScale(begin_tar_scale);
			}
		}
	}

	protected override void Modifying ()
	{
		if (xdragging || ydragging || zdragging)
		{
			Vector3 mousexy = new Vector3 (Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"), 0);
			float begin_dist = Mathf.Min(Vector3.Distance(MainCamera.transform.position, begin_pos_3d), 1024f);
			float dist = Mathf.Min(Vector3.Distance(MainCamera.transform.position, new_tar_scale), 1024f);
			float xoffset = Vector3.Dot(mousexy, xdragdir) * dist / 5000f;
			float yoffset = Vector3.Dot(mousexy, ydragdir) * dist / 5000f;
			float zoffset = Vector3.Dot(mousexy, zdragdir) * dist / 5000f;
			Vector3 old_scale = new_tar_scale;

			Vector3 delta_scl = Vector3.zero;

			if (xdragging && !ydragging && !zdragging)
			{
				delta_scl += xoffset * Vector3.right;
			}
			if (ydragging && !zdragging && !xdragging)
			{
				delta_scl += yoffset * Vector3.up;
			}
			if (zdragging && !xdragging && !ydragging)
			{
				delta_scl += zoffset * Vector3.forward;
			}
			if (dragplane)
			{
				Vector3 new_pos;
				if (GetDragPos_Plane(out new_pos))
				{
					if (Vector3.Distance(new_pos, MainCamera.transform.position) < begin_dist * 50f)
					{
						delta_scl = Vector3.zero;
						if (focus_ol)
						{
							delta_scl = -(mousexy.y * dist / 5000f) * Vector3.one;
						}
						else
						{
							Vector3 _delta = new_pos - dragorigin;
							delta_scl.x = Vector3.Dot(_delta, GizmoGroup.right) * xsign;
							delta_scl.y = Vector3.Dot(_delta, GizmoGroup.up) * ysign;
							delta_scl.z = Vector3.Dot(_delta, GizmoGroup.forward) * zsign;
						}
						dragorigin = new_pos;
					}
				}
			}

			new_tar_scale += delta_scl;
			
			// Submit
			if (old_scale != new_tar_scale)
			{
				foreach (Transform t in Targets)
				{
					if (t != null)
						t.localScale += delta_scl;
				}
				if (OnTargetScale != null)
					OnTargetScale(new_tar_scale);
			}
		}
	}

	protected override void EndModify ()
	{
		if (xdragging || ydragging || zdragging)
		{
			if (begin_tar_scale != new_tar_scale)
			{
				if (OnEndTargetScale != null)
					OnEndTargetScale(new_tar_scale);
			}
		}
	}

	bool GetDragPos_Plane (out Vector3 pos)
	{
		pos = Vector3.zero;
		if (xdragging && ydragging && !zdragging)
		{
			Ray ray = MainCamera.ScreenPointToRay(mousePosition);
			Plane p = new Plane (GizmoGroup.forward, begin_pos_3d);
			float enter = 0;
			if (p.Raycast(ray, out enter))
			{
				pos = ray.GetPoint(enter);
				return true;
			}
		}
		else if (ydragging && zdragging && !xdragging)
		{
			Ray ray = MainCamera.ScreenPointToRay(mousePosition);
			Plane p = new Plane (GizmoGroup.right, begin_pos_3d);
			float enter = 0;
			if (p.Raycast(ray, out enter))
			{
				pos = ray.GetPoint(enter);
				return true;
			}
		}
		else if (zdragging && xdragging && !ydragging)
		{
			Ray ray = MainCamera.ScreenPointToRay(mousePosition);
			Plane p = new Plane (GizmoGroup.up, begin_pos_3d);
			float enter = 0;
			if (p.Raycast(ray, out enter))
			{
				pos = ray.GetPoint(enter);
				return true;
			}
		}
		else if (xdragging && ydragging && zdragging)
		{
			Ray ray = MainCamera.ScreenPointToRay(mousePosition);
			Plane p = new Plane ((transform.position - GizmoGroup.position).normalized, begin_pos_3d);
			float enter = 0;
			if (p.Raycast(ray, out enter))
			{
				pos = ray.GetPoint(enter);
				return true;
			}
		}
		return false;
	}
	
	const float end = 0.81f;
	const float half = 0.24f;
	protected override void OnGL ()
	{
		Vector3 origin = GizmoGroup.position;
		Vector3 xdir = GizmoGroup.right;
		Vector3 ydir = GizmoGroup.up;
		Vector3 zdir = GizmoGroup.forward;
		Vector3 xend = origin + xdir * end;
		Vector3 yend = origin + ydir * end;
		Vector3 zend = origin + zdir * end;
		Vector3 xhalf = origin + xdir * half * xsign;
		Vector3 yhalf = origin + ydir * half * ysign;
		Vector3 zhalf = origin + zdir * half * zsign;
		Vector3 xyhalf = origin + xdir * half * xsign + ydir * half * ysign;
		Vector3 yzhalf = origin + ydir * half * ysign + zdir * half * zsign;
		Vector3 zxhalf = origin + zdir * half * zsign + xdir * half * xsign;

		GL.Begin(GL.LINES);

		GL.Color(xlc);
		GL.Vertex(origin);
		GL.Vertex(xend);
		GL.Color(ylc);
		GL.Vertex(origin);
		GL.Vertex(yend);
		GL.Color(zlc);
		GL.Vertex(origin);
		GL.Vertex(zend);

		if (!hideCenter)
		{
			GL.Color(new Color(xpc.r, xpc.g, xpc.b, 1));
			GL.Vertex(origin);
			GL.Vertex(yhalf);
			GL.Vertex(yhalf);
			GL.Vertex(yzhalf);
			GL.Vertex(yzhalf);
			GL.Vertex(zhalf);
			GL.Vertex(zhalf);
			GL.Vertex(origin);

			GL.Color(new Color(ypc.r, ypc.g, ypc.b, 1));
			GL.Vertex(origin);
			GL.Vertex(zhalf);
			GL.Vertex(zhalf);
			GL.Vertex(zxhalf);
			GL.Vertex(zxhalf);
			GL.Vertex(xhalf);
			GL.Vertex(xhalf);
			GL.Vertex(origin);

			GL.Color(new Color(zpc.r, zpc.g, zpc.b, 1));
			GL.Vertex(origin);
			GL.Vertex(xhalf);
			GL.Vertex(xhalf);
			GL.Vertex(xyhalf);
			GL.Vertex(xyhalf);
			GL.Vertex(yhalf);
			GL.Vertex(yhalf);
			GL.Vertex(origin);
		}

		GL.End();

		if (!hideCenter)
		{
			GL.Begin(GL.QUADS);
			GL.Color(xpc);
			GL.Vertex(origin);
			GL.Vertex(yhalf);
			GL.Vertex(yzhalf);
			GL.Vertex(zhalf);

			GL.Color(ypc);
			GL.Vertex(origin);
			GL.Vertex(zhalf);
			GL.Vertex(zxhalf);
			GL.Vertex(xhalf);

			GL.Color(zpc);
			GL.Vertex(origin);
			GL.Vertex(xhalf);
			GL.Vertex(xyhalf);
			GL.Vertex(yhalf);
			GL.End();
		}
	}

	void OnDisable ()
	{
		xdragging = false;
		ydragging = false;
		zdragging = false;
		dragplane = false;
		focus_xl = false;
		focus_yl = false;
		focus_zl = false;
		focus_xp = false;
		focus_yp = false;
		focus_zp = false;
		focus_ol = false;
	}
}
