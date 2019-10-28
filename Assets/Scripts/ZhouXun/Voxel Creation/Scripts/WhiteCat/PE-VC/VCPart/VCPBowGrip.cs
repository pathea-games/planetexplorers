using UnityEngine;
using Pathea;

namespace WhiteCat
{
	public class VCPBowGrip : VCPart
	{
		public AttackMode[] m_AttackMode;
		public int[] m_CostItemID;
		public int[] m_SkillID;

		public string m_ReloadAnim = "BowReload";

		public Transform m_ArrowBagTrans;
		public Transform m_LineBone;
		public GameObject[] m_ArrowModel;


		public PEActionType[] m_RemoveEndAction;
		public ActiveAttr m_HandChangeAttr = new ActiveAttr();
		public PEAimAttr m_AimAttr = new PEAimAttr();
		public bool showOnVehicle = false;


		[Header("Attack")]
		public float baseAttack = 100f;
		public float maxExtendAttack = 200f;


		public void CopyTo(PEBow target, CreationData data)
		{
			target.m_AttackMode = m_AttackMode;
			target.m_CostItemID = m_CostItemID;
			target.m_SkillID = m_SkillID;

			target.m_ReloadAnim = m_ReloadAnim;
			target.m_ArrowBagTrans = m_ArrowBagTrans;
			target.m_LineBone = m_LineBone;

			target.m_ArrowModel = m_ArrowModel;

			target.m_RemoveEndAction = m_RemoveEndAction;
			target.m_HandChangeAttr = m_HandChangeAttr;
			target.m_AimAttr = m_AimAttr;
			target.showOnVehicle = showOnVehicle;
		}
	}
}