using UnityEngine;
using System.Collections;

public class testEffectTarget : MonoBehaviour {
	public float lifeTime = 2f;
	public float radius = 100f;
	public float speed = 5f; 	
	public GameObject attacker;
	float timeNow = 0f;

	public void InitPos () {
		//Vector3 seed = new Vector3(2f * Random.value - 1f, 2f * Random.value - 1f, 2f * Random.value - 1f);
		//Vector3 seed = new Vector3 (Random.value > 0.5 ? 1 : -1, Random.value > 0.5 ? 1 : -1, Random.value > 0.5 ? 1 : -1);
		Vector3 seed = Random.rotation * (Vector3.up * radius);
		transform.position += seed;
		if(transform.position.y < 0)
			transform.position = new Vector3(transform.position.x, -transform.position.y, transform.position.z);

		if(speed > 0f)
			transform.GetComponent<Rigidbody>().velocity = new Vector3(Random.value, Random.value, Random.value).normalized * speed;
	}
	

	void Update () {
		timeNow += Time.deltaTime;
		if(attacker == null)
			Destroy(gameObject);
		if(timeNow >= lifeTime)
			Destroy(gameObject);
	}
}
