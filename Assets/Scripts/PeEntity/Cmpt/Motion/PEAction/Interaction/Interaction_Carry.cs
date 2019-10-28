using UnityEngine;
using RootMotion.FinalIK;
using Pathea;

namespace PEIK
{
	public class Interaction_Carry : Interaction
	{
		static readonly string CasterObjName = "Carry";
		static readonly string TargetObjName = "BeCarry";
		static readonly FullBodyBipedEffector[] CarrierEffector = new FullBodyBipedEffector[]{FullBodyBipedEffector.LeftHand, FullBodyBipedEffector.RightHand};
		static readonly FullBodyBipedEffector[] BeCarrierEffector = new FullBodyBipedEffector[]{FullBodyBipedEffector.LeftHand, FullBodyBipedEffector.RightHand,
																		FullBodyBipedEffector.Body};

		protected override string casterObjName { get { return CasterObjName; } }
		protected override string targetObjName { get { return TargetObjName; } }
		protected override FullBodyBipedEffector[] casterEffectors { get { return CarrierEffector; } }
		protected override FullBodyBipedEffector[] targetEffectors { get { return BeCarrierEffector; } }
	}
}
