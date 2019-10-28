using UnityEngine;
using WhiteCat;
using WhiteCat.Internal;

public class DrawTweenInterpolator : TweenBase
{
	public float y
	{
		get { return transform.position.y; }
		set { transform.position = new Vector3(0, value, 0); }
	}


	public override void OnTween(float factor)
	{
		y = factor;
	}


	float _original;


	public override void OnRecord()
	{
		_original = y;
	}


	public override void OnRestore()
	{
		y = _original;
	}


	void OnDrawGizmos()
	{
		Gizmos.color = new Color(1, 1, 1, 0.5f);
		Gizmos.DrawLine(Vector3.zero, Vector3.up);
		Gizmos.DrawLine(Vector3.up, Vector2.one);
		Gizmos.DrawLine(Vector2.one, Vector3.right);
		Gizmos.DrawLine(Vector3.right, Vector3.zero);

		if (!interpolator) return;

		Vector3 last = new Vector3(0, interpolator.Interpolate(0));
		Vector3 current = Vector3.zero;

		Gizmos.color = Color.green;
		for(current.x = 0.02f; current.x <= 1; current.x += 0.02f)
		{
			current.y = interpolator.Interpolate(current.x);
			Gizmos.DrawLine(last, current);
			last = current;
		}

		Gizmos.color = Color.magenta;

		last.x = 0;
		last.y = y;
		current.x = interpolator.normalizedTime;
		current.y = last.y;
		Gizmos.DrawLine(last, current);

		last.x = current.x;
		last.y = 0;
		Gizmos.DrawLine(current, last);
	}
}