using UnityEngine;
using System.Collections;
using Steer3D;

public class SteerGUI : MonoBehaviour
{
	public SteerAgent agent;
	public RectTransform desired_rect;
	public RectTransform velocity_rect;

	// Use this for initialization
	void Start ()
	{
	
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (dragging)
		{
			if (Input.GetMouseButtonUp(0))
				dragging = false;
			Vector3 vec = transform.InverseTransformPoint(Input.mousePosition);
			vec = Vector3.ClampMagnitude(vec, 90f);
			if (vec.magnitude < 5f)
				vec = Vector3.zero;
			desired_rect.anchoredPosition = new Vector2 (vec.x, vec.y);
		}
		Vector3 desired = new Vector3(desired_rect.anchoredPosition.x, 0, desired_rect.anchoredPosition.y) / 90f;
		agent.AddDesiredVelocity(desired, 4f, 0.8f);
		Vector3 vel = agent.velocity * 90f;
		velocity_rect.anchoredPosition = new Vector2 (vel.x, vel.z);
	}

	bool dragging = false;
	public void OnPointerDown ()
	{
		dragging = true;
	}
}
