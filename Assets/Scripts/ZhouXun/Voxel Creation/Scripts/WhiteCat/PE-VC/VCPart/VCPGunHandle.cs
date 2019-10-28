using UnityEngine;
using System.Collections;
using Pathea;

namespace WhiteCat
{
	public class VCPGunHandle : VCPart
	{
		public Transform m_PivotPoint;
		public Transform m_FirstHandPoint;
		public Transform m_SecondHandPoint;

		[Header("Additional Attributes")]

		public PEActionType[] m_RemoveEndAction;
		public ActiveAttr m_HandChangeAttr = new ActiveAttr();

		[Space(8)]
		public float m_AccuracyMin = 1f;
		public float m_AccuracyMax = 5f;
		public float m_AccuracyPeriod = 5f;
		public float m_AccuracyShrinkSpeed = 3f;
		public float m_CenterUpDisMax = 10f;
		public float m_CenterUpShrinkSpeed = 3f;

		[Space(8)]
		public string m_ReloadAnim;
		public GameObject m_MagazineObj;
		public Transform m_MagazinePos;
		public int m_MagazineEffectID;
		public int m_MagazineSize = 30;
		public int m_ReloadSoundID;


		[Header("Original Attributes")]

		public bool DualHand = false;
		public int GunType = 0;
		
		public bool showOnVehicle = false;

		public void CopyTo(PEGun target)
		{
			target.m_RemoveEndAction = m_RemoveEndAction;
			target.m_HandChangeAttr = m_HandChangeAttr;
			target.m_AimAttr.m_AccuracyMin = m_AccuracyMin;
			target.m_AimAttr.m_AccuracyMax = m_AccuracyMax;
			target.m_AimAttr.m_AccuracyPeriod = m_AccuracyPeriod;
			target.m_AimAttr.m_AccuracyShrinkSpeed = m_AccuracyShrinkSpeed;
			target.m_AimAttr.m_CenterUpDisMax = m_CenterUpDisMax;
			target.m_AimAttr.m_CenterUpShrinkSpeed = m_CenterUpShrinkSpeed;
			target.m_ReloadAnim = m_ReloadAnim;
			target.m_MagazineObj = m_MagazineObj;
			target.m_MagazinePos = m_MagazinePos;
			target.m_MagazineEffectID = m_MagazineEffectID;
			target.m_Magazine = new Magazine();
			target.m_Magazine.m_Size = m_MagazineSize;
			target.m_Magazine.m_Value = m_MagazineSize;
			target.m_ReloadSoundID = m_ReloadSoundID;
			target.showOnVehicle = showOnVehicle;
		}



		//protected override string BuildDescription()
		//{
		//	string type = DualHand ? "Dual Hand" : "Single Hand";
		//	string guntype = "";
		//	switch (GunType)
		//	{
		//		case 1: guntype = "Bullet gun"; break;
		//		case 2: guntype = "Beam gun"; break;
		//		case 3: guntype = "Explosive gun"; break;
		//		default: guntype = "Unknown type gun"; break;
		//	}
		//	type = "Gun type: " + guntype + " [" + type + "]";
		//	return "Gun handle, mandatory component.\r\n\r\n" + type + "\r\n\r\nTo create a gun, you must add both handle and muzzle.";
		//}
	}
}