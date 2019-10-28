//using UnityEngine;
//using System.Collections;

//public class VCPMissileLauncherProperty : VCPartProperty
//{
//	public int MissileCount = 4;
//	public int ProjectileId = 0;
//	public int AttackType = 3;
//	public float Attack = 0;
//	public float AOERadius = 0;
//	public float FireInterval = 10;
//	public float LockTime = 2;
//	public int ImpactEffectID = 0;
//	public int SoundID = 0;
//	public float MaxPower = 0;
//	public float EnergyCostCoef = 0.05f;
//	public float EnergyCostOnce { get { return MaxPower * EnergyCostCoef; } }

//	public override string Desc ()
//	{
//		string aoedesc = (AOERadius > 0.01f) ? ("Splash damage radius: " + AOERadius.ToString("0.0") + " m") : ("");
//		return  "Missile Launcher\r\nMissile: " + MissileCount.ToString() + " per shot\r\n" +
//				"Lock Time: " + LockTime.ToString("0.0") + " sec\r\n" + 
//				"Attack: " + Attack.ToString("0") + " unit\r\n" + aoedesc + "\r\n" +
//				"Firing Time Interval: " + FireInterval.ToString("0.0") + " sec.\r\n" +
//				"Energy cost: " + EnergyCostOnce.ToString("0.0") + " per shot.\r\n\r\n" +
//				"This part can be rotated and scaled.";
//	}
//}
