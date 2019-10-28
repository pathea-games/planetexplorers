using UnityEngine;
using System.Collections;

public class testEffectFactory : MonoBehaviour {
	public bool loop;
	public float cycleTime;
	public int explodeNum;
	public float interval;
	public GameObject unit;
	public GameObject target;

	float pastTime = 0f;
	float unitTime = 0f;
	int totalNum = 1;
	GameObject unitObj;
	GameObject targetObj;
	void Start () {
		if(null == target)
			return;
		targetObj = Instantiate(target, transform.position, transform.rotation) as GameObject;
		unitObj = Instantiate(unit, transform.position, transform.rotation) as GameObject;
		targetObj.transform.parent = transform;
		unitObj.transform.parent = transform;
		if (targetObj.GetComponent<testEffectTarget> () != null) 
		{
			targetObj.GetComponent<testEffectTarget> ().attacker = unitObj;
			targetObj.GetComponent<testEffectTarget> ().InitPos();
		}
		/*if(unitObj.GetComponent<testEffectM140528>() == null)
		{
			unitObj.transform.FindChild("00test1").GetComponent<testEffectM140528>().target = targetObj;
			unitObj.transform.FindChild("00test2").GetComponent<testEffectM140528>().target = targetObj;
			unitObj.transform.FindChild("00test3").GetComponent<testEffectM140528>().target = targetObj;
			unitObj.transform.FindChild("00test4").GetComponent<testEffectM140528>().target = targetObj;
		}*/
	}
	

	void FixedUpdate () {
		if(!loop)
		{
			pastTime += Time.deltaTime;
			if(pastTime > cycleTime && cycleTime != 0f)
			{
				pastTime = 0f;
				targetObj = Instantiate(target, transform.position, transform.rotation) as GameObject;
				unitObj = Instantiate(unit, transform.position, transform.rotation) as GameObject;
				targetObj.transform.parent = transform;
				unitObj.transform.parent = transform;
				if (targetObj.GetComponent<testEffectTarget> () != null) 
				{
					targetObj.GetComponent<testEffectTarget> ().attacker = unitObj;
					targetObj.GetComponent<testEffectTarget> ().InitPos();
				}
				/*if(unitObj.GetComponent<testEffectM140528>() == null)
				{
					unitObj.transform.FindChild("00test1").GetComponent<testEffectM140528>().target = targetObj;
					unitObj.transform.FindChild("00test2").GetComponent<testEffectM140528>().target = targetObj;
					unitObj.transform.FindChild("00test3").GetComponent<testEffectM140528>().target = targetObj;
					unitObj.transform.FindChild("00test4").GetComponent<testEffectM140528>().target = targetObj;
				}*/
				totalNum = 1;
				unitTime = -Time.deltaTime;
			}
			if(totalNum >= explodeNum)
					return;
		}
		if(unitTime >= interval)
		{
			targetObj = Instantiate(target, transform.position, transform.rotation) as GameObject;
			unitObj = Instantiate(unit, transform.position, transform.rotation) as GameObject;
			targetObj.transform.parent = transform;
			unitObj.transform.parent = transform;
			if (targetObj.GetComponent<testEffectTarget> () != null) 
			{
				targetObj.GetComponent<testEffectTarget> ().attacker = unitObj;
				targetObj.GetComponent<testEffectTarget> ().InitPos();
			}
			/*if(unitObj.GetComponent<testEffectM140528>() == null)
			{
				unitObj.transform.FindChild("00test1").GetComponent<testEffectM140528>().target = targetObj;
				unitObj.transform.FindChild("00test2").GetComponent<testEffectM140528>().target = targetObj;
				unitObj.transform.FindChild("00test3").GetComponent<testEffectM140528>().target = targetObj;
				unitObj.transform.FindChild("00test4").GetComponent<testEffectM140528>().target = targetObj;
			}*/
			unitTime -= interval;
			totalNum ++;
		}
		else
			unitTime += Time.deltaTime;



	}
}
