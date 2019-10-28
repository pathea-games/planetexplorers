using UnityEngine;
using Pathea;
namespace WhiteCat
{
	public class VCPAxeHilt : VCPart
	{
		[Header("Original Attributes")]
		
		public float AttackInc = 0.0f;
		public float DurabilityInc = 0.0f;
		public float AttackEnh = 1.0f;
		public float DurabilityEnh = 1.0f;
		
		[Header("Additional Attributes")]
		
		public PEActionType[] m_RemoveEndAction;
		public ActiveAttr m_HandChangeAttr = new ActiveAttr();
		
		public bool showOnVehicle = false;

        public int m_FellSkillID;
        public float m_StaminaCost = 5f;

		public void CopyTo(PEAxe target, CreationData data)
		{
			target.m_RemoveEndAction = m_RemoveEndAction;
			target.m_HandChangeAttr = m_HandChangeAttr;
			target.showOnVehicle = showOnVehicle;
			target.m_FellSkillID = m_FellSkillID;
            target.m_StaminaCost = m_StaminaCost;
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