using UnityEngine;
using RootMotion.FinalIK;
using System.Collections;

namespace PEIK
{
	public class Interaction_Ride : Interaction 
	{
		static readonly FullBodyBipedEffector[] HelperEffector =
            new FullBodyBipedEffector[]{FullBodyBipedEffector.RightHand, FullBodyBipedEffector.LeftHand, FullBodyBipedEffector.LeftFoot, FullBodyBipedEffector.RightFoot, FullBodyBipedEffector.LeftThigh, FullBodyBipedEffector.RightThigh };
        static readonly FullBodyBipedEffector[] BeHelperEffector = new FullBodyBipedEffector[]{};
		
		protected override string casterObjName { get { return ""; } }
		protected override string targetObjName { get { return "BeRide"; } }
		protected override FullBodyBipedEffector[] casterEffectors { get { return HelperEffector; } }
		protected override FullBodyBipedEffector[] targetEffectors { get { return BeHelperEffector; } }
	}
}