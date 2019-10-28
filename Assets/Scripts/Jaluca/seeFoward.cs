using UnityEngine;
using System.Collections;

public class seeFoward : MonoBehaviour {
	public Vector3 position;
	public Vector3 foward;
	public Vector3 eulerAngle;
	public Vector4 rotation;			
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		position = transform.position;
		foward = transform.forward;
		eulerAngle = transform.rotation.eulerAngles;
		rotation = new Vector4(transform.rotation.x, transform.rotation.y, transform.rotation.z, transform.rotation.w);
		Debug.DrawLine(transform.position,transform.position + 30f * transform.forward, Color.blue);
		Debug.DrawLine(transform.position,transform.position + 30f * transform.up, Color.green);
		Debug.DrawLine(transform.position,transform.position + 30f * transform.right, Color.red);
	}
}
