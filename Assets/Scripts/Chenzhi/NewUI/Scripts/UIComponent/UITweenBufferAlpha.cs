using UnityEngine;
using System.Collections;

public class UITweenBufferAlpha : UITweener 
{
	public float from = 1f;
	public float to = 1f;

	public bool refreshWidget = true;

	UIWidget[] mWidgets = null;
	UIEffectAlpha[] mEffectHandlers = null;

	//GameObject mGo = null;

	void Awake ()
	{
		
	}

	override protected void OnUpdate (float factor, bool isFinished) 
	{
		if (refreshWidget)
		{
			mWidgets = gameObject.GetComponentsInChildren<UIWidget>(true);
            //log:lz-2016.05.26 未激活的UIEffectAlpha,也要设置Alpha,不然在界面打开后激活的组件会看不到
            mEffectHandlers = gameObject.GetComponentsInChildren<UIEffectAlpha>(true);
			refreshWidget = false;
		}

		float a = Mathf.Lerp(from, to, factor);
		if (mWidgets != null)
		{
			foreach (UIWidget wd in mWidgets)
			{
				wd.bufferAlpha = a; 
				wd.MarkAsChanged();
			}
		}

		if (mEffectHandlers != null)
		{
			foreach (UIEffectAlpha sh in mEffectHandlers)
			{
				sh.alpha = a;
			}
		}
	}

	static public TweenAlpha Begin (GameObject go, float duration, float alpha)
	{
		TweenAlpha comp = UITweener.Begin<TweenAlpha>(go, duration);
		comp.from = comp.alpha;
		comp.to = alpha;
		
		if (duration <= 0f)
		{
			comp.Sample(1f, true);
			comp.enabled = false;
		}
		return comp;
	}
}
