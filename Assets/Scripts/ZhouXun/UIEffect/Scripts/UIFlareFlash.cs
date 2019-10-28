using UnityEngine;
using System.Collections;

public class UIFlareFlash : MonoBehaviour
{
	UITexture uitex;
	public float During = 0.5f;
	public Vector3 BeginScale;
	public Vector3 EndScale;
	public Gradient ColorDuring;
	public AnimationCurve Curve;
	public float Randomness = 0.0f;
	public float RandomT = 20;

	float t = 0;

	void OnEnable ()
	{
		uitex = GetComponent<UITexture>();
	}
	
	void OnDisable ()
	{
		t = 0;
	}
	
	void Update ()
	{
		if (uitex != null)
		{
			float p = Curve.Evaluate(t/During);
			p *= ((Mathf.PerlinNoise(Time.time*RandomT, Time.time*RandomT) * 2 - 1) * Randomness + 1);
			transform.localScale = Vector3.Lerp(BeginScale, EndScale, p);
			uitex.color = ColorDuring.Evaluate(p);
		}
		t += Time.deltaTime;
	}
}
