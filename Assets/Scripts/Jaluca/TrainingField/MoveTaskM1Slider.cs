using UnityEngine;
using System.Collections;

public class MoveTaskM1Slider : MonoBehaviour
{
	const float sliderMax = 230f;
	[SerializeField]float minPct;
	[SerializeField]float maxPct;
	[SerializeField]float cd;
	[SerializeField]float fadeSpeed;
	[SerializeField]float maxChangePct = 0.1f;
	float min;
	float max;
	float maxChange;
	float target;
	float current;
	float temp;
	RectTransform rt;
	void Start()
	{
		min = sliderMax * minPct;
		max = sliderMax * maxPct;
		maxChange = sliderMax * maxChangePct;
		current = 0.5f * (min + max);
		rt = GetComponent<RectTransform>();
		rt.offsetMax = new Vector2(current - sliderMax, 0f);
		StartCoroutine(UpdateTarget());
	}
	void FixedUpdate()
	{
		temp = Mathf.Lerp(current, target, fadeSpeed);
		if(Mathf.Abs(temp - current) / sliderMax < maxChangePct)
			current = temp;
		else
			current = current + maxChange * Mathf.Sign(target - current);

		rt.offsetMax = new Vector2(current - sliderMax, 0f);
	}
	IEnumerator UpdateTarget()
	{
		while(true)
		{
			target = Random.Range(min, max);
			yield return new WaitForSeconds(cd);
		}
	}
}
