using UnityEngine;

public class RotatingGizmo : TransformGizmo
{
	public delegate void DNotify (Quaternion quat);
	public event DNotify OnBeginTargetRotate;
	public event DNotify OnTargetRotate;
	public event DNotify OnEndTargetRotate;

	[SerializeField] Collider XCollider;
	[SerializeField] Collider YCollider;
	[SerializeField] Collider ZCollider;
	[SerializeField] SphereCollider SCollider;
	[SerializeField] Collider ACollider;

	Color xlc;
	Color ylc;
	Color zlc;
	Color slc;
	Color alc;

	Color xpc;
	Color ypc;
	Color zpc;
	
	bool focus_x = false;
	bool focus_y = false;
	bool focus_z = false;
	bool focus_s = false;
	bool focus_a = false;
	
	bool dragging = false;
	Vector3 dragorigin3d = Vector3.zero;
	Vector3 dragdir;
	Vector3 vdragdir;

	Quaternion begin_tar_rot = Quaternion.identity;
	Quaternion new_tar_rot = Quaternion.identity;

	public override bool MouseOver
	{
		get
		{
			return enabled && Focus != null && (focus_x || focus_y || focus_z || focus_s || focus_a || dragging);
		}
	}
	
	public override bool Working
	{
		get
		{
			return enabled && Focus != null && (dragging);
		}
	}
	
	protected override void Start ()
	{
		base.Start ();
		XCollider.transform.localScale = (radius) * Vector3.one;
		YCollider.transform.localScale = (radius) * Vector3.one;
		ZCollider.transform.localScale = (radius) * Vector3.one;
		SCollider.radius = radius * 0.90f;
		ACollider.transform.localScale = (radius * 1.18f) * Vector3.one;

		SCollider.enabled = !hideCenter;
		ACollider.enabled = !hideCenter;
	}

	protected override void Idle ()
	{
		bool on_ui = UnityEngine.EventSystems.EventSystem.current != null &&
			UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();

		Vector3 dir = (transform.position - GizmoGroup.position).normalized;

		Ray ray = cam.ScreenPointToRay(mousePosition);
		RaycastHit rch = new RaycastHit ();
		if (!on_ui) Physics.Raycast(ray, out rch, 11.0f, 1 << gameObject.layer);
		float dot = Vector3.Dot((rch.point - GizmoGroup.position).normalized, dir);
		focus_x = (rch.collider == XCollider && dot > -0.1f);
		focus_y = (rch.collider == YCollider && dot > -0.1f);
		focus_z = (rch.collider == ZCollider && dot > -0.1f);
		focus_s = (rch.collider == SCollider);
		focus_a = (rch.collider == ACollider);

		xlc = focus_x ? HColor : XColor;
		ylc = focus_y ? HColor : YColor;
		zlc = focus_z ? HColor : ZColor;
		slc = focus_s ? HColor : (Color.white * 0.8f);
		alc = focus_a ? HColor : (Color.white * 0.8f);
		xpc = XColor;
		ypc = YColor;
		zpc = ZColor;
		xpc.a = 0.3f;
		ypc.a = 0.3f;
		zpc.a = 0.3f;

		dragging = false;
		dragorigin3d = rch.point;
	}

	protected override void BeginModify ()
	{
		dragging = focus_a || focus_s || focus_x || focus_y || focus_z;
		if (dragging)
		{
			Vector3 rchdir = (dragorigin3d - GizmoGroup.position).normalized;
			Vector3 dir3d = Vector3.zero;
			Vector3 axis_a = (transform.position - GizmoGroup.position).normalized;
			if (focus_x)
				dir3d = -Vector3.Cross(rchdir, GizmoGroup.right).normalized;
			else if (focus_y)
				dir3d = -Vector3.Cross(rchdir, GizmoGroup.up).normalized;
			else if (focus_z)
				dir3d = -Vector3.Cross(rchdir, GizmoGroup.forward).normalized;
			else if (focus_a)
				dir3d = -Vector3.Cross(rchdir, axis_a).normalized;
			else if (focus_s)
			{
				dir3d = -ACollider.transform.right;
			}
			dragdir = (cam.WorldToScreenPoint(dragorigin3d + dir3d) - mousePosition);
			dragdir.z = 0;
			dragdir.Normalize();
			vdragdir = (cam.WorldToScreenPoint(dragorigin3d + ACollider.transform.up) - mousePosition);
			vdragdir.z = 0;
			vdragdir.Normalize();

			begin_tar_rot = Focus.rotation;
			new_tar_rot = Focus.rotation;

			if (OnBeginTargetRotate != null)
				OnBeginTargetRotate(begin_tar_rot);
		}
	}

	protected override void CustomLateUpdate ()
	{
		Quaternion q = Quaternion.identity;
		q.SetLookRotation(GizmoGroup.position - MainCamera.transform.position);
		ACollider.transform.rotation = q;
	}

	protected override void Modifying ()
	{
		if (dragging)
		{
			Vector3 mousexy = new Vector3 (Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"), 0);
			float delta = Vector3.Dot(mousexy, dragdir) * 5;
			float vdelta = Vector3.Dot(mousexy, vdragdir) * 5;
			Quaternion old_rot = new_tar_rot;
			Vector3 axis_a = (transform.position - GizmoGroup.position).normalized;

			Quaternion delta_rot = Quaternion.identity;

			if (focus_x)
				delta_rot = Quaternion.AngleAxis(delta, GizmoGroup.right) * delta_rot;
			if (focus_y)
				delta_rot = Quaternion.AngleAxis(delta, GizmoGroup.up) * delta_rot;
			if (focus_z)
				delta_rot = Quaternion.AngleAxis(delta, GizmoGroup.forward) * delta_rot;
			if (focus_s)
			{
				delta_rot = Quaternion.AngleAxis(delta, ACollider.transform.up) * Quaternion.AngleAxis(vdelta, ACollider.transform.right) * delta_rot;
			}
			if (focus_a)
				delta_rot = Quaternion.AngleAxis(delta, axis_a) * delta_rot;

			new_tar_rot = delta_rot * new_tar_rot;

			// Submit
			if (old_rot != new_tar_rot)
			{
				foreach (Transform t in Targets)
				{
					if (t != null)
						t.rotation = delta_rot * t.rotation;
				}
				if (OnTargetRotate != null)
					OnTargetRotate(new_tar_rot);
			}
		}
	}

	protected override void EndModify ()
	{
		if (dragging)
		{
			if (begin_tar_rot != new_tar_rot)
			{
				if (OnEndTargetRotate != null)
					OnEndTargetRotate(new_tar_rot);
			}
		}
	}
	
	const float radius = 0.9f;
	const float interval = 6.0f;
	protected override void OnGL ()
	{
		Vector3 origin = GizmoGroup.position;
		Vector3 xdir = GizmoGroup.right;
		Vector3 ydir = GizmoGroup.up;
		Vector3 zdir = GizmoGroup.forward;
		Vector3 wdir = ACollider.transform.forward;
		Vector3 udir = ACollider.transform.right;
		Vector3 vdir = ACollider.transform.up;

		GL.Begin(GL.LINES);

		if (!hideCenter)
		{
			GL.Color(slc);
			for (float angle = 0; angle <= 360.01f - interval; angle += interval)
			{
				Vector3 p0 = origin + udir * Mathf.Cos(angle * Mathf.Deg2Rad) * radius * 0.94f + vdir * Mathf.Sin(angle * Mathf.Deg2Rad) * radius * 0.94f;
				float _angle1 = angle + interval;
				Vector3 p1 = origin + udir * Mathf.Cos(_angle1 * Mathf.Deg2Rad) * radius * 0.94f + vdir * Mathf.Sin(_angle1 * Mathf.Deg2Rad) * radius * 0.94f;
				GL.Vertex(p0);
				GL.Vertex(p1);
			}

			GL.Color(alc);
			for (float angle = 0; angle <= 360.01f - interval; angle += interval)
			{
				Vector3 p0 = origin + udir * Mathf.Cos(angle * Mathf.Deg2Rad) * radius * 1.18f + vdir * Mathf.Sin(angle * Mathf.Deg2Rad) * radius * 1.18f;
				float _angle1 = angle + interval;
				Vector3 p1 = origin + udir * Mathf.Cos(_angle1 * Mathf.Deg2Rad) * radius * 1.18f + vdir * Mathf.Sin(_angle1 * Mathf.Deg2Rad) * radius * 1.18f;
				GL.Vertex(p0);
				GL.Vertex(p1);
			}
		}

		GL.Color(xlc);
		for (float angle = 0; angle <= 360.01f - interval; angle += interval)
		{
			Vector3 p0 = origin + ydir * Mathf.Cos(angle * Mathf.Deg2Rad) * radius + zdir * Mathf.Sin(angle * Mathf.Deg2Rad) * radius;
			bool back = Vector3.Dot(wdir.normalized, (p0 - origin).normalized) > 0.17f;
			if (back) continue;
			float _angle1 = angle + interval;
			Vector3 p1 = origin + ydir * Mathf.Cos(_angle1 * Mathf.Deg2Rad) * radius + zdir * Mathf.Sin(_angle1 * Mathf.Deg2Rad) * radius;
			GL.Vertex(p0);
			GL.Vertex(p1);
		}

		GL.Color(ylc);
		for (float angle = 0; angle <= 360.01f - interval; angle += interval)
		{
			Vector3 p0 = origin + zdir * Mathf.Cos(angle * Mathf.Deg2Rad) * radius + xdir * Mathf.Sin(angle * Mathf.Deg2Rad) * radius;
			bool back = Vector3.Dot(wdir.normalized, (p0 - origin).normalized) > 0.17f;
			if (back) continue;
			float _angle1 = angle + interval;
			Vector3 p1 = origin + zdir * Mathf.Cos(_angle1 * Mathf.Deg2Rad) * radius + xdir * Mathf.Sin(_angle1 * Mathf.Deg2Rad) * radius;
			GL.Vertex(p0);
			GL.Vertex(p1);
		}

		GL.Color(zlc);
		for (float angle = 0; angle <= 360.01f - interval; angle += interval)
		{
			Vector3 p0 = origin + xdir * Mathf.Cos(angle * Mathf.Deg2Rad) * radius + ydir * Mathf.Sin(angle * Mathf.Deg2Rad) * radius;
			bool back = Vector3.Dot(wdir.normalized, (p0 - origin).normalized) > 0.17f;
			if (back) continue;
			float _angle1 = angle + interval;
			Vector3 p1 = origin + xdir * Mathf.Cos(_angle1 * Mathf.Deg2Rad) * radius + ydir * Mathf.Sin(_angle1 * Mathf.Deg2Rad) * radius;
			GL.Vertex(p0);
			GL.Vertex(p1);
		}

		GL.End();
	}

	void OnDisable ()
	{
		dragging = false;
		focus_a = false;
		focus_x = false;
		focus_y = false;
		focus_z = false;
		focus_s = false;
	}
}
