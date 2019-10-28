using UnityEngine;
using System.Collections;
using Pathea;

namespace WhiteCat
{
	public class VCPSwordHilt : VCPart
	{
		[Header("Original Attributes")]

		public float AttackInc = 0.0f;
		public float DurabilityInc = 0.0f;
		public float AttackEnh = 1.0f;
		public float DurabilityEnh = 1.0f;

		[Header("Additional Attributes")]

		public PEActionType[] m_RemoveEndAction;
		public ActiveAttr m_HandChangeAttr = new ActiveAttr();

		public AttackMode[] m_AttackMode;
		public PeSword.AttackSkill[] m_AttackSkill;
		public float[] m_StaminaCost;

		public bool showOnVehicle = false;

        [Header("Original DoubleHilt")]
        public string m_LHandPutOnBone = "mountMain";
        public string m_LHandPutOffBone = "mountBack";
        public GameObject m_LHandWeapon;
        [SerializeField]
        Pathea.PEAttackTrigger m_peAttacktrigger = null;

        public Pathea.PEAttackTrigger Attacktrigger { get { return m_peAttacktrigger; } }


		public virtual void CopyTo(PeSword target, CreationData data)
		{
			target.m_RemoveEndAction = m_RemoveEndAction;
			target.m_HandChangeAttr = m_HandChangeAttr;
			target.showOnVehicle = showOnVehicle;

			target.m_AttackMode = new AttackMode[m_AttackMode.Length];
			System.Array.Copy(m_AttackMode, target.m_AttackMode, m_AttackMode.Length);

			target.m_AttackSkill = new PeSword.AttackSkill[m_AttackSkill.Length];
			System.Array.Copy(m_AttackSkill, target.m_AttackSkill, m_AttackSkill.Length);

			target.m_StaminaCost = new float[m_StaminaCost.Length];
			for (int i = 0; i < m_StaminaCost.Length; i++)
			{
				target.m_StaminaCost[i] = m_StaminaCost[i] * Mathf.Clamp(
					data.m_Attribute.m_Weight / PEVCConfig.instance.swordStandardWeight,
					PEVCConfig.instance.minStaminaCostRatioOfWeight,
					PEVCConfig.instance.maxStaminaCostRatioOfWeight);
			}

            PETwoHandWeapon twohand = target as PETwoHandWeapon;
            if (twohand != null)
            {
                twohand.m_LHandPutOnBone = m_LHandPutOnBone;
                twohand.m_LHandPutOffBone = m_LHandPutOffBone;

               // twohand.m_LHandWeapon = m_LHandWeapon;
            }
		}


		//protected override string BuildDescription()
		//{
		//	string desc = "";
		//	if (AttackInc > 0.01f)
		//		desc = desc + "Base attack: " + AttackInc.ToString() + "\r\n";
		//	if (AttackEnh > 1.001f)
		//		desc = desc + "Attack up: " + ((AttackEnh - 1) * 100).ToString() + " %\r\n";
		//	if (DurabilityInc > 0.01f)
		//		desc = desc + "Base durability: " + DurabilityInc.ToString() + "\r\n";
		//	if (DurabilityEnh > 1.001f)
		//		desc = desc + "Durability up: " + ((DurabilityEnh - 1) * 100).ToString() + " %\r\n";
		//	desc = desc + "\r\nA sword can only have one hilt";
		//	return desc;
		//}
	}

 
}