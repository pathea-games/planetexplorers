using UnityEngine;
using System.Collections;

public class PlanetAperture : MonoBehaviour
{
	public Transform planetEdge;
	public Transform sun;
	public float distToPlanet;
	public AnimationCurve blueThickness;
	Camera cmr;
	Vector3 planetCenter;

	float side1;
	float side2;
	float t2;
	void Start()
	{
		cmr = Camera.main;
	}
	void Update()
	{
		planetCenter = planetEdge.parent.position;
		transform.forward = (cmr.transform.position - planetCenter).normalized;
		transform.position = planetCenter + transform.forward * distToPlanet;
		side1 = Vector3.SqrMagnitude(planetEdge.position - planetCenter);
		side2 = Vector3.SqrMagnitude(planetCenter - cmr.transform.position) - side1;
		t2 = side2 / Vector3.SqrMagnitude(transform.position - cmr.transform.position);
		GetComponent<Renderer>().material.SetFloat("_Radius", Mathf.Sqrt(side1 / t2) / transform.localScale.x);
		GetComponent<Renderer>().material.SetFloat("_SunAngle", Vector3.Angle(transform.forward, sun.forward));
		Vector3 fwd = Quaternion.Inverse(transform.rotation) * sun.forward;
		GetComponent<Renderer>().material.SetVector("_SunDirect", fwd);
	}
}
