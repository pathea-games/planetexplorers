using UnityEngine;
using System.Collections;

public class particleFactory : MonoBehaviour {
	public bool loop;
	public float cycleTime;
	public int explodeNum;
	public float interval;
	public GameObject unit;
	public Color color;

	float pastTime = 0f;
	float unitTime = 0f;
	int totalNum = 1;
	GameObject obj;
	void Start () {
		obj = Instantiate(unit, transform.position, transform.rotation) as GameObject;
		obj.GetComponent<ParticleSystem>().startColor = color;
		obj.GetComponent<ParticleSystem>().startRotation = Vector3.Angle(transform.forward,Vector3.forward) / 57.29578f * Mathf.Sign(transform.forward.x);
		obj.transform.parent = transform;
	}
	

	void FixedUpdate () {
		if(!loop)
		{
			pastTime += Time.deltaTime;
			if(pastTime > cycleTime)
			{
				pastTime = 0f;
				obj = Instantiate(unit, transform.position, transform.rotation) as GameObject;
				obj.GetComponent<ParticleSystem>().startColor = color;
				obj.GetComponent<ParticleSystem>().startRotation = Vector3.Angle(transform.forward,Vector3.forward) / 57.29578f * Mathf.Sign(transform.forward.x);
				obj.transform.parent = transform;
				totalNum = 1;
				unitTime = -Time.deltaTime;
			}
			if(totalNum >= explodeNum)
					return;
		}
		if(unitTime >= interval)
		{
			obj = Instantiate(unit, transform.position, transform.rotation) as GameObject;
			obj.GetComponent<ParticleSystem>().startColor = color;
			obj.GetComponent<ParticleSystem>().startRotation = Vector3.Angle(transform.forward,Vector3.forward) / 57.29578f * Mathf.Sign(transform.forward.x);
			obj.transform.parent = transform;
			unitTime -= interval;
			totalNum ++;
		}
		else
			unitTime += Time.deltaTime;



	}
}
