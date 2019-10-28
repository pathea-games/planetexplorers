using UnityEngine;
using System.Collections;

public class UIFadeInOut : MonoBehaviour
{
	public float During = 0.5f;
	public AnimationCurve FadeInX;
	public AnimationCurve FadeInY;
	public AnimationCurve FadeOutX;
	public AnimationCurve FadeOutY;

	float t = 0;
	public float direction = 0;

	public void FadeIn ()
	{
		direction = 1;
	}

	public void FadeOut ()
	{
		direction = -1;
	}
	
	// Update is called once per frame
	void Update ()
	{
		t += Time.deltaTime / During * direction;
		t = Mathf.Clamp01(t);

		if (t <= 0)
			transform.localScale = new Vector3 (0,0,1);
		else if (t >= 1)
			transform.localScale = new Vector3 (1,1,1);
		else if (direction > 0)
			transform.localScale = new Vector3 (FadeInX.Evaluate(t), FadeInY.Evaluate(t), 1);
		else
			transform.localScale = new Vector3 (FadeOutX.Evaluate(t), FadeOutY.Evaluate(t), 1);
	}
}
