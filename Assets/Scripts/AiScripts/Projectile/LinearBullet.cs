using UnityEngine;
using System.Collections;

public class LinearBullet : MonoBehaviour {
	public float speed;
	void FixedUpdate() 
	{
		transform.position += transform.forward * speed * Time.deltaTime;
	}
}
