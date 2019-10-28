using UnityEngine;
using System.Collections;

public class UIFlowing : MonoBehaviour
{
	UITexture uitex;
	public float During = 0.5f;
	public float Counter = -1;
	public Vector3 BeginPosition;
	public Vector3 EndPosition;
	public Vector3 BeginScale = Vector3.one;
	public Vector3 EndScale = Vector3.one;
	public Gradient ColorDuring;
	public AnimationCurve PosCurve;
	public AnimationCurve ScaleCurve;

	public float Pre = 0f;
	public float Post = 0.5f;

	float t = 0;
	float c = 0;
	
	void OnEnable ()
	{
		uitex = GetComponent<UITexture>();
		c = Counter;
	}
	
	void OnDisable ()
	{
		t = 0;
	}

	void Update ()
	{
		float p = PosCurve.Evaluate((t-Pre)/During);
		transform.localPosition = Vector3.Lerp(BeginPosition, EndPosition, p);
		ScaleCurve.Evaluate((t-Pre)/During);
		transform.localScale = Vector3.Lerp(BeginScale, EndScale, p);
		if (uitex != null)
		{
			uitex.color = ColorDuring.Evaluate(p);
		}
		if (c != 0)
			t += Time.deltaTime;
		if (t > During + Pre + Post)
		{
			c--;
			t = 0;
		}
	}
}
