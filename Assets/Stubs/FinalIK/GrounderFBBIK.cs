// Stub: RootMotion.FinalIK.GrounderFBBIK
// Referenced in IKCmpt.cs, Motion_Move_Human.cs, BiologyViewRoot.cs.
// Ground alignment component -- used as field type, no member access beyond GetComponent.

using UnityEngine;

namespace RootMotion.FinalIK
{
    /// <summary>
    /// Ground alignment component for FullBodyBipedIK. Exposes weight, spineBend,
    /// and a solver property to access maxFootRotationAngle.
    /// </summary>
    public class GrounderFBBIK : MonoBehaviour
    {
        public float weight { get; set; }
        public float spineBend { get; set; }
        public IKSolverFullBodyBiped solver => new IKSolverFullBodyBiped();
    }
}
