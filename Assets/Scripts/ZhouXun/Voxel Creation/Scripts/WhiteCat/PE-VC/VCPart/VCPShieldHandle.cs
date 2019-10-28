using UnityEngine;
using System.Collections;

namespace WhiteCat
{
	public class VCPShieldHandle : VCPart
	{
		public Transform m_PivotPoint;

		public float BaseDefence = 0.0f;
		public float BaseDurability = 0.0f;
		public float DefenceEnh = 1.0f;
		public float DurabilityEnh = 1.0f;


		//protected override string BuildDescription()
		//{
		//	string desc = "";
		//	if (BaseDefence > 0.01f)
		//		desc = desc + "Base defense: " + BaseDefence.ToString() + "\r\n";
		//	if (DefenceEnh > 1.001f)
		//		desc = desc + "Defense up: " + ((DefenceEnh - 1) * 100).ToString("0.0") + " %\r\n";
		//	if (BaseDurability > 0.01f)
		//		desc = desc + "Base durability: " + BaseDurability.ToString() + "\r\n";
		//	if (DurabilityEnh > 1.001f)
		//		desc = desc + "Durability up: " + ((DurabilityEnh - 1) * 100).ToString("0.0") + " %\r\n";
		//	desc = desc + "\r\nA sheild can only have one handle";
		//	return desc;
		//}
	}
}