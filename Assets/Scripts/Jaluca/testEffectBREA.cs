using System.Collections;
using UnityEngine;

public enum testBreathType{FRO,CIRCLE}
class testEffectBREA : MonoBehaviour
{
	public float anglePerCycle = 0;
	public float timePerCycle = 0;
	public float offsetRadius =0;
	public float delayTime = 0.1f;
	public Transform effect = null;
	public testBreathType breathType = testBreathType.FRO;
	public int listNum;
	
	float currentAngle = 0f;
	float currentTime = 0f;
	Queue locus = new Queue();
	CapsuleCollider coli;
	Vector3 tempPosition;
	Vector3 tempForward;
		
	public void Start () {
		this.transform.position = Vector3.forward * offsetRadius;
		this.transform.forward = Vector3.forward;
		if (this.GetComponent<Collider>() != null) 
		{
			coli = this.GetComponent<Collider>() as CapsuleCollider;
			StartCoroutine (LengthUp ());
		}
	}

	public void Update () {
		currentTime += Time.deltaTime;
		if(breathType == testBreathType.FRO)
		{
			currentAngle = (currentTime - (int)(currentTime / timePerCycle) * timePerCycle) / timePerCycle * anglePerCycle;
			if((int)(currentTime / timePerCycle) % 2 == 1)
				currentAngle = anglePerCycle - currentAngle;
		}
		else if(breathType == testBreathType.CIRCLE)
		{
			currentAngle = currentTime / timePerCycle * anglePerCycle;
		}
		tempForward = new Vector3(Mathf.Sin(currentAngle * Mathf.PI / 180f), 0f, Mathf.Cos(currentAngle * Mathf.PI / 180f));
		tempPosition = tempForward * offsetRadius;
		if(delayTime != 0f)
		{
			locus.Enqueue(tempPosition);
			locus.Enqueue(tempForward);
			listNum = locus.Count;
			if(currentTime > delayTime){
				this.transform.position = (Vector3)locus.Dequeue();
				this.transform.forward = (Vector3)locus.Dequeue();
			}
		}
		else
		{
			transform.position = tempPosition;
			transform.forward = tempForward;
		}
		if(effect != null)
		{
			effect.position = tempPosition;
			effect.forward = tempForward;
		}
	}
	
	IEnumerator LengthUp(){	
		float maxLength = coli.height;		
		float unit = (coli.height - coli.radius) / (delayTime / 0.1f);
		while(true)
		{
			if(delayTime == 0f || coli == null)
				yield return new WaitForSeconds(100f);
			else
			{
				coli.height = coli.radius - unit;
				while(true){
					coli.height += unit;
					coli.center = new Vector3(0f, 0f, coli.height / 2f);
					if(coli.height > maxLength){
						coli.height = maxLength;
						coli.center = new Vector3(0f, 0f, maxLength / 2f);
						break;
					}
					yield return new WaitForSeconds(0.1f);
				}
			}

		}

	}
}