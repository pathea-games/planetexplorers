//using UnityEngine;
//using System.Collections;

//public class VCPFrontCannonProperty : VCPartProperty
//{
//	public int GunType = 3;
//	public int CostItemId = 0;
//	public int AttackType = 3;
//	public float Attack = 0;
//	public float AOERadius = 0;
//	public float FireInterval = 0.7f;
//	public float ProjectileSpeed = 80;
//	public int ImpactEffectID = 0;
//	public int SoundID = 0;
//	public float RecoilImpulse = 0; // N.s
//	public float MaxPower = 0;
//	public float EnergyCostCoef = 0.05f;
//	public float EnergyCostOnce { get { return MaxPower * EnergyCostCoef; } }

//	public override string Desc ()
//	{
//		string aoedesc = (AOERadius > 0.01f) ? ("Splash damage radius: " + AOERadius.ToString("0.0") + " m") : ("");
//		return "Attack: " + Attack.ToString("0") + " unit\r\n" + aoedesc + "\r\n" +
//				"Firing rate: " + (1.0f / FireInterval).ToString("0.00") + " per second.\r\n" +
//				"Energy cost: " + EnergyCostOnce.ToString("0.0") + " per shot.\r\n\r\n" +
//				"This part can be rotated.";
//	}
//}
