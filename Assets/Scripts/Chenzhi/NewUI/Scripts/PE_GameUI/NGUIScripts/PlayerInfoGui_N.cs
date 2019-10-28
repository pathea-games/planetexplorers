using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ItemAsset;

/*
public class PlayerInfoGui_N : GUIWindowBase
{
	public PlayerEquipGui_N mPlayerEquipGui_N;
	
	public UILabel			mName;
	public UILabel			mLifeCount;
	public UILabel			mComfortCount;
	public UILabel			mOxygenCount;
	public UILabel			mShield;
	public UILabel			mEnergy;
	public UILabel			mAtk;
	public UILabel			mDef;
	
	
//	Vector3		mOffsetPos = new Vector3(-308f,0,0);
	
	void Update()
	{
		Player mPlayer = PlayerFactory.mMainPlayer;
		if(mPlayer != null)
		{
			mLifeCount.text = ((int)mPlayer.GetAttribute(Pathea.AttribType.Hp)).ToString() 
								+ "/" + (int)mPlayer.GetAttribute(Pathea.AttribType.HpMax);
			mComfortCount.text = ((int)mPlayer.GetAttribute(Pathea.AttribType.Stamina)).ToString()
								+ "/" + (int)mPlayer.GetAttribute(Pathea.AttribType.StaminaMax);
			mOxygenCount.text = ((int)mPlayer.GetAttribute(Pathea.AttribType.Oxygen)).ToString()
			                    + "/" + (int)mPlayer.GetAttribute(Pathea.AttribType.OxygenMax);
			
			ItemObject shield = mPlayer.ShieldEnergy;
			ItemObject energy = mPlayer.Battery;
			if(null != shield)
			{
				if(null != energy)
					mShield.text = ((int)shield.GetProperty(ItemProperty.ShieldEnergy)).ToString() + "/" + (int)shield.GetProperty(ItemProperty.ShieldMax);
				else
					mShield.text = "0/" + (int)shield.GetProperty(ItemProperty.ShieldMax);
			}
			else
				mShield.text = "0/0";
			
			if(null != energy)
				mEnergy.text = ((int)energy.GetProperty(ItemProperty.BatteryPower)).ToString() + "/" + (int)energy.GetProperty(ItemProperty.BatteryPowerMax);
			else
				mEnergy.text = "0/0";
			
			mAtk.text = ((int)mPlayer.GetAttribute(Pathea.AttribType.Atk)).ToString();
			mDef.text = ((int)mPlayer.GetAttribute(Pathea.AttribType.Def)).ToString();
			if (GameConfig.IsMultiMode)
				mName.text = mPlayer.name;
			else
				mName.text = GameConfig.Account;
		}
		
//		if(GameGui_N.Instance.mPlayerEquipGui.gameObject.active)
//			transform.localPosition = GameGui_N.Instance.mPlayerEquipGui.transform.localPosition + mOffsetPos;
	}
}

*/
