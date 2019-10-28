using UnityEngine;
using System.Collections;

namespace WhiteCat
{

	public enum EArmType
	{
		Ammo,
		Energy,
		Bomb,
		Arrow
	}

	public class VCPGunMuzzle : VCPart
	{
		public Transform Start = null;
		public Transform End = null;
		public Transform MuzzleEffect = null;
		public int SkillId = 0;


		[Header("Additional Attributes")]

		public Transform m_AimTrans;
		public UISightingTelescope.SightingType m_AimPointType;
		[Range(0.1f, 1f)]
		public float m_FireStability = 0.5f;
		public float m_AccuracyDiffusionRate = 1f;
		public float m_CenterUpDisPerShoot = 3f;

		[Space(8)]
		public GameObject m_ChargeEffectGo;
		public int m_ShellCaseEffectID;
		public ShootMode m_ShootMode = ShootMode.SingleShoot;
		public AmmoType m_AmmoType = AmmoType.Bullet;
		public int[] m_AmmoItemIDList = new int[] { 11000001 };

		[Space(8)]
		public float m_ChargeEnergySpeed = 0.5f;
		public float m_RechargeEnergySpeed = 3f;
		public float m_RechargeDelay = 1.5f;
		public float[] m_ChargeTime = new float[] { 0.8f, 1.5f };
		public float m_EnergyPerShoot = 1f;

		[Space(8)]
		public int[] m_SkillIDList = new int[] { 20219924 };
		public float m_FireRate = 0.3f;

		[Space(8)]
		public int m_ChargeSoundID;
		public int m_ChargeLevelUpSoundID;
		public int m_DryFireSoundID;
		public int m_ShootSoundID;
		public int m_ChargeLevelUpEffectID;

		public AttackMode[] m_AttackMode;


		[Header("Original Attributes")]

		public int GunType = 0;
		public bool Multishot = false;
		public float FireInterval = 0.5f;

		public int CostItemId = 0;
		public EArmType ArmType = EArmType.Ammo;
		public int ProjectileId = 0;
		public int SoundId = 0;

		public float Attack = 0.0f;
		public int AttackType = 3;

		public float Accuracy = 0.5f;


		public void CopyTo(PEGun target)
		{
			target.m_AimAttr.m_AimTrans = m_AimTrans;
			target.m_AimAttr.m_AimPointType = m_AimPointType;
			target.m_AimAttr.m_FireStability = m_FireStability;
			target.m_AimAttr.m_AccuracyDiffusionRate = m_AccuracyDiffusionRate;
			target.m_AimAttr.m_CenterUpDisPerShoot = m_CenterUpDisPerShoot;
			target.m_ChargeEffectGo = m_ChargeEffectGo;
			target.m_ShellCaseEffectID = m_ShellCaseEffectID;
			target.m_ShootMode = m_ShootMode;
			target.m_AmmoType = m_AmmoType;
			target.m_AmmoItemIDList = m_AmmoItemIDList;
			target.m_ChargeEnergySpeed = m_ChargeEnergySpeed;
			target.m_RechargeEnergySpeed = m_RechargeEnergySpeed;
			target.m_RechargeDelay = m_RechargeDelay;
			target.m_ChargeTime = m_ChargeTime;
			target.m_EnergyPerShoot = m_EnergyPerShoot;
			target.m_SkillIDList = m_SkillIDList;
			target.m_FireRate = m_FireRate;
			target.m_ChargeSoundID = m_ChargeSoundID;
			target.m_ChargeLevelUpSoundID = m_ChargeLevelUpSoundID;
			target.m_DryFireSoundID = m_DryFireSoundID;
			target.m_ShootSoundID = m_ShootSoundID;
			target.m_ChargeLevelUpEffectID = m_ChargeLevelUpEffectID;

			target.m_AttackMode = new AttackMode[m_AttackMode.Length];
			System.Array.Copy(m_AttackMode, target.m_AttackMode, m_AttackMode.Length);
		}


		//protected override string BuildDescription()
		//{
		//	string type = Multishot ? "Multishot" : "Singleshot";
		//	string guntype = "";
		//	switch (GunType)
		//	{
		//		case 1: guntype = "Bullet gun"; break;
		//		case 2: guntype = "Beam gun"; break;
		//		case 3: guntype = "Explosive gun"; break;
		//		default: guntype = "Unknown type gun"; break;
		//	}
		//	type = "Gun type: " + guntype + " [" + type + "]";
		//	string fs = "\r\n";
		//	if (Multishot)
		//		fs = "\r\nFiring rate: " + (1.0f / FireInterval).ToString("0.0") + " per second\r\n";
		//	return "Gun muzzle, mandatory component.\r\n\r\n" + type + fs + "Attack: " + Attack.ToString("0.0")
		//		+ "\r\nAccuracy: " + (Accuracy * 60).ToString("0") + "'  (" + (Accuracy * 1.7453f).ToString("0.00") + "m error / 100m)" +
		//			"\r\n\r\nTo create a gun, you must add both handle and muzzle.";
		//}


		public void PlayMuzzleEffect()
		{
			GameObject eff_go = GameObject.Instantiate(MuzzleEffect.gameObject) as GameObject;
			eff_go.transform.parent = MuzzleEffect.transform.parent;
			eff_go.transform.localPosition = MuzzleEffect.transform.localPosition;
			eff_go.transform.localRotation = MuzzleEffect.transform.localRotation;
			eff_go.transform.localScale = MuzzleEffect.transform.localScale;
			eff_go.SetActive(true);
			Renderer[] prs = eff_go.GetComponentsInChildren<Renderer>();
			foreach (Renderer pr in prs)
				pr.enabled = true;
			ParticleSystem[] particles = eff_go.GetComponentsInChildren<ParticleSystem>();
			foreach (ParticleSystem particle in particles)
				particle.Play();
		}
	}

}