// Stub: RootMotion.FinalIK base types
// Provides IK, IKSolver, IKSolverFullBodyBiped, IKSolverAim, IKSolverCCD,
// IKEffector, and IKMapping for compilation against proprietary FinalIK plugin.

using UnityEngine;

namespace RootMotion.FinalIK
{
    /// <summary>
    /// Base solver class. Provides shared IK properties accessed via .solver on IK components.
    /// </summary>
    public class IKSolver
    {
        public Vector3 IKPosition;
        public float IKPositionWeight;
        public Transform target;
        public Transform transform;
        public Vector3 axis = Vector3.forward;

        public delegate void UpdateDelegate();
        public UpdateDelegate OnPreUpdate;

        public virtual void SetIKPositionWeight(float weight)
        {
            IKPositionWeight = weight;
        }

        public virtual void SetIKPosition(Vector3 position)
        {
            IKPosition = position;
        }

        public virtual void Update() { }
    }

    /// <summary>
    /// Stub effector with position offset and target bone.
    /// </summary>
    public class IKEffectorFBBIK
    {
        public Vector3 positionOffset;
        public float positionWeight;
        public float rotationWeight;
        public Transform target;
        public Transform bone;
    }

    /// <summary>
    /// Full body biped IK solver with arm mappings, iteration control,
    /// and effector/root accessors used by game IK code.
    /// </summary>
    public class IKSolverFullBodyBiped : IKSolver
    {
        public int iterations = 4;
        public IKMapping leftArmMapping = new IKMapping();
        public IKMapping rightArmMapping = new IKMapping();
        public int clampSmoothing;
        public float maxFootRotationAngle = 45f;

        public IKEffectorFBBIK GetEffector(FullBodyBipedEffector effector) => new IKEffectorFBBIK();
        public Transform GetRoot() => null;
    }

    /// <summary>
    /// Aim IK solver with directional axis, target, and clamp smoothing.
    /// </summary>
    public class IKSolverAim : IKSolver
    {
        public int clampSmoothing { get; set; }
    }

    /// <summary>
    /// Cyclic Coordinate Descent IK solver.
    /// </summary>
    public class IKSolverCCD : IKSolver
    {
    }

    /// <summary>
    /// Stub mapping type used by IKSolverFullBodyBiped arm mappings.
    /// </summary>
    public class IKMapping
    {
        public float weight { get; set; }
    }

    /// <summary>
    /// IK effector with target and weight.
    /// </summary>
    public class IKEffector
    {
        public Transform target;
        public float positionWeight;
    }

    /// <summary>
    /// Base IK MonoBehaviour. Subclasses (AimIK, FullBodyBipedIK, CCDIK) expose a
    /// typed .solver property. Game code accesses solver members through this.
    /// </summary>
    public class IK : MonoBehaviour
    {
        protected IKSolver _solver = new IKSolver();

        public virtual IKSolver GetIKSolver()
        {
            return _solver;
        }

        public void Disable()
        {
            enabled = false;
        }

        public void Enable()
        {
            enabled = true;
        }
    }
}
