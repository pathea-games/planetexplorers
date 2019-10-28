using UnityEngine;
using System.Collections;

public class TowerDefenceGUI_N : UIBaseWnd
{
	public UILabel 	mLeftWave;
	public UILabel 	mWaveTime;
	public UISlider	mLifeBar;
	
	int mWaveTimeCount;
	
	protected override void InitWindow ()
	{
		base.InitWindow ();
		//if(!GameConfig.IsMultiClient)
			Hide();
	}
	
	public void SetCenterLife(float lifePresent)
	{
		mLifeBar.sliderValue = lifePresent;
	}
	
	public void SetState(int leftWave,int waveTime)
	{
		mLeftWave.text = leftWave.ToString();
		
		if(mWaveTimeCount != waveTime)
		{
			mWaveTimeCount = waveTime;
			string mWaveTimeText = "";
			int hour = waveTime/60;
			int secound = waveTime%60;
			
			if(hour<10)
				mWaveTimeText += "0";
			mWaveTimeText += hour;
			mWaveTimeText += ":";
			if(secound<10)
				mWaveTimeText += "0";
			mWaveTimeText += secound;
			mWaveTime.text = mWaveTimeText;
		}
		
	}
}
