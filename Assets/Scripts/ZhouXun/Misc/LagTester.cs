using UnityEngine;
using System;
using System.Collections;

public class LagTester : MonoBehaviour
{
	public GUISkin gskin;
	HighStopwatch watch;

	Int64 counter = 0L;
	public int frame = 0;
	public double time = 0.0;

	public static double threshold = 10000f;

	string text = "";
	int showtextCounter = 0;

	// Use this for initialization
	void Start ()
	{
		try { watch = new HighStopwatch(); } catch {}
		if (watch != null)
			counter = watch.Value;
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (threshold > 1000f)
			return;
		double deltaTime = UpdateTime();
		if (deltaTime > threshold)
		{
			text = "Lag detected : " + (deltaTime * 1000).ToString("0.0") + " ms";

//			if (Pathea.PeCreature.Instance.mainPlayer != null)
//			if (PeTipsMsgMan.Instance != null)
//				new PeTipMsg(text, PeTipMsg.EMsgLevel.Warning);
//
			showtextCounter = 200;
		}
	}

	double UpdateTime ()
	{
		if (watch == null || watch.Frequency == 0L)
		{
			time = 0.0;
			return 0.0;
		}
		Int64 _counter = counter;
		counter = watch.Value;
		double dt = (double)(counter - _counter) / (double)(watch.Frequency);
		time += dt;
		++frame;
		return dt;
	}

	void OnGUI ()
	{
		showtextCounter--;
		if (showtextCounter > 0)
		{
			GUI.skin = gskin;
			GUI.color = Color.yellow;
			GUI.Label(new Rect(Screen.width - 152, Screen.height - 22, 180, 20), text);
			GUI.color = Color.white;
		}
	}
}
