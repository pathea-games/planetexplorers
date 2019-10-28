using UnityEngine;
using System.Collections;
using WhiteCat;

public class AddTweenInCode : BaseBehaviour
{
	private TweenInterpolator interpolator;
	private Vector2 from;
	private Vector2 to;


	void Awake()
	{
		interpolator = TweenInterpolator.Create(
			gameObject: gameObject,
			isPlaying: false,
			duration: 0.8f,
			method: TweenMethod.EaseInBackOut,
			onUpdate: factor => rectTransform.anchoredPosition = from + (to - from) * factor);
	}


	void Update ()
	{
		if(Input.GetMouseButtonDown(0))
		{
			from = rectTransform.anchoredPosition;

			RectTransformUtility.ScreenPointToLocalPointInRectangle(
				rectTransform.parent as RectTransform, Input.mousePosition, null, out to);

			interpolator.Replay();
		}
	}
	
}
