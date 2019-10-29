using UnityEngine;
using System.Collections;

public class particleTrajectory : MonoBehaviour {

	public Vector3 speed;
	public float lifeTime;

	void Start () {
		Invoke("dest",lifeTime);
	}

	void FixedUpdate () {
		transform.position += speed * Time.deltaTime;
	}

	void dest(){
		Destroy(gameObject);
	}
}
