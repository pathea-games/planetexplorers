//using UnityEngine;
//using System.Collections;

//public class VCPCtrlTurretProperty : VCPartProperty
//{
//	public int GunType = 1;
//	public int CostItemId = 0;
//	public int AttackType = 3;
//	public float Attack = 0;
//	public float AOERadius = 0;
//	public float FireInterval = 0.5f;
//	public float ProjectileSpeed = 200;
//	public int ImpactEffectID = 0;
//	public int SoundID = 0;
//	public float MaxPitch = 50;
//	public float MinPitch = -30;
//	public float MovingDamp = 0.25f;
//	public float MaxPower = 0;
//	public float EnergyCostCoef = 0.05f;
//	public float EnergyCostOnce { get { return MaxPower * EnergyCostCoef; } }
	
//	public override string Desc ()
//	{
//		string aoedesc = (AOERadius > 0.01f) ? ("Splash damage radius: " + AOERadius.ToString("0.0") + " m\r\n") : ("");
//		return "Attack: " + Attack.ToString("0") + " unit\r\n" + aoedesc +
//				"Firing rate: " + (1.0f / FireInterval).ToString("0.00") + " per second.\r\n" +
//				"Pitch angle limit: " + MinPitch.ToString("0") + " ~ " + MaxPitch.ToString("0") + " degrees\r\n" +
//				"Rotation speed: " + (MovingDamp*10).ToString("0.0") + "\r\n" +
//				"Energy cost: " + EnergyCostOnce.ToString("0.0") + " per shot.\r\n\r\n" +
//				"(This part can be rotated in order to cover the limitation of the pitch angle)";
//	}
//}
