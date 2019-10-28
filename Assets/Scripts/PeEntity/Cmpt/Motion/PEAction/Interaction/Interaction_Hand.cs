using UnityEngine;
using RootMotion.FinalIK;
using System.Collections;

namespace PEIK
{
	public class Interaction_Hand : Interaction 
	{
		static readonly string CasterObjName = "Hand";
		static readonly string TargetObjName = "BeHand";
		static readonly FullBodyBipedEffector[] HelperEffector = new FullBodyBipedEffector[]{FullBodyBipedEffector.RightHand};
		static readonly FullBodyBipedEffector[] BeHelperEffector = new FullBodyBipedEffector[]{};
		
		protected override string casterObjName { get { return CasterObjName; } }
		protected override string targetObjName { get { return TargetObjName; } }
		protected override FullBodyBipedEffector[] casterEffectors { get { return HelperEffector; } }
		protected override FullBodyBipedEffector[] targetEffectors { get { return BeHelperEffector; } }
	}
}