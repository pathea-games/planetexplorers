using UnityEngine;
using System.Collections;

public class MoveTaskM1Point : MonoBehaviour
{
	const float angleMax = 180f;
	[SerializeField]float minAngle;
	[SerializeField]float maxAngle;
	[SerializeField]float cd;
	[SerializeField]float fadeSpeed;
	[SerializeField]float maxChangePct = 0.1f;
	float maxChange;
	float target;
	float current;
	float temp;
	RectTransform rt;
	void Start()
	{
		maxChange = angleMax * maxChangePct;
		current = 0.5f * (minAngle + maxAngle);
		rt = GetComponent<RectTransform>();
		rt.eulerAngles = new Vector3(180f, 0f, current);
		StartCoroutine(UpdateTarget());
	}
	void FixedUpdate()
	{
		maxChange = angleMax * maxChangePct;
		//top is test
		temp = Mathf.Lerp(current, target, fadeSpeed);
		if(Mathf.Abs(temp - current) / angleMax < maxChangePct)
			current = temp;
		else
			current = current + maxChange * Mathf.Sign(target - current);
		rt.eulerAngles = new Vector3(180f, 0f, current);
	}
	IEnumerator UpdateTarget()
	{
		while(true)
		{
			target = Random.Range(minAngle, maxAngle);
			yield return new WaitForSeconds(cd);
		}
	}
}
