using UnityEngine;
using System.Collections;
using PeCustom;

public class UIStopwatchNode : MonoBehaviour
{
	[HideInInspector] public UIStopwatchList List;
	[HideInInspector] public int StopwatchId;
	[HideInInspector] public int Index;
	[SerializeField] UISprite BgSprite;
	[SerializeField] UISprite WarnSprite;
	[SerializeField] UILabel NameLabel;
	[SerializeField] UILabel TimeLabel;

	StopwatchMgr mgr { get { return PeCustomScene.Self.scenario.stopwatchMgr; } }

	// Use this for initialization
	void OnEnable ()
	{
		if (mgr.stopwatches.ContainsKey(StopwatchId))
		{
			Stopwatch sw = mgr.stopwatches[StopwatchId];
			NameLabel.text = sw.name;
			TimeLabel.text = TimeString(sw.timer.Second);
			transform.localPosition = new Vector3 (0, YTarget, 10);
			y = YTarget;
			FadeIn();
		}
		else
		{
			FadeOut();
		}
	}
	
	// Update is called once per frame
	void Update ()
	{
		float warnalpha = 0;
		if (mgr.stopwatches.ContainsKey(StopwatchId))
		{
			Stopwatch sw = mgr.stopwatches[StopwatchId];
			NameLabel.text = sw.name;
			TimeLabel.text = TimeString(sw.timer.Second);
			if (sw.timer.Second < 10.5f && sw.timer.ElapseSpeed < 0)
				warnalpha = 0.5f - Mathf.Cos((float)sw.timer.Second * Mathf.PI * 4) * 0.5f;
		}
		else
		{
			FadeOut();
		}

		fade += fadedir * Time.deltaTime * 3;

		float alpha = Mathf.Clamp01(fade);
		BgSprite.alpha = alpha * 0.6f;
		WarnSprite.alpha = alpha * warnalpha * 0.5f;
		NameLabel.alpha = alpha;
		TimeLabel.alpha = alpha;

		y = Mathf.Lerp(y, YTarget, 0.25f);
		transform.localPosition = new Vector3 (0, Mathf.Round(y), 10);

		if (fade < 0 && fadedir <= 0)
			List.DeleteNode(StopwatchId);
		if (fade > 1)
			fade = 1;
	}

	string TimeString (double sec)
	{
		if (sec < 0)
			sec = 0;
		int isec = Mathf.RoundToInt((float)sec);
		int second = isec % 60;
		int minute = (isec % 3600) / 60;
		int hour = isec / 3600;
		if (hour > 0)
			return hour.ToString("00") + ":" + minute.ToString("00") + ":" + second.ToString("00");
		else
			return minute.ToString("00") + ":" + second.ToString("00");
	}

	float YTarget { get { return -27 - 52 * Index; } }

	float fade = 0;
	float fadedir = 0;
	float y;

	void FadeIn ()
	{
		fadedir = 1;
	}

	void FadeOut ()
	{
		fadedir = -1;
	}
}
