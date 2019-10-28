using System.Collections;
using UnityEngine;

public enum impType{FORWARD, FOUNTAIN}
public enum destroyType{DESTROY, REFRESH}
class testEffectIMP : MonoBehaviour
{
	public Vector3 speed;
	public float gravity= 0;
	public float existTime = 0;
	public impType impt = 0;
	public destroyType dest = 0;
	public bool followRotate = false;
	public Vector3 selfRotate;
	
	Vector3 speedVector;
	Vector3 speedY;
	float rotateAngle = 0f;
	float rotateSpeed;
	float pastTime = 0f;

	void Start(){
		rotateSpeed = selfRotate.magnitude;
	}

	public void Update () {
		if(pastTime == 0f){
			transform.position = Vector3.zero;
			if(impt == impType.FORWARD)
				speedVector = speed;
			else if(impt == impType.FOUNTAIN)
				speedVector = new Vector3(Random.value - 0.5f, Random.value * 0.5f + 0.5f, Random.value - 0.5f).normalized * speed.magnitude;
			speedY = Vector3.zero;
			this.GetComponent<Rigidbody>().velocity = speedVector;
		}else{
			speedY += Vector3.down * gravity * Time.deltaTime;
			this.GetComponent<Rigidbody>().velocity = speedVector + speedY;
		}
		if(followRotate)
			this.transform.rotation = Quaternion.FromToRotation(Vector3.forward, this.GetComponent<Rigidbody>().velocity);
		if (selfRotate != Vector3.zero) 
		{
			rotateAngle += rotateSpeed * Time.deltaTime;
			this.transform.rotation *= Quaternion.AngleAxis(rotateAngle, selfRotate);
		}
		pastTime += Time.deltaTime;
		if(pastTime > existTime)
		{
			if(dest == destroyType.DESTROY)
				Destroy(gameObject);
			else if(dest == destroyType.REFRESH)
				pastTime = 0f;;
		}
			
	}
}