// Stub implementations for additional RootMotion.FinalIK types not covered by existing stubs.
// These stubs exist solely so the Roslyn analyzer project compiles.
// THESE TYPES MUST NOT BE USED IN RUNTIME CODE.

using UnityEngine;
using System.Collections.Generic;

namespace RootMotion.FinalIK
{
    public enum FullBodyBipedEffector
    {
        Body,
        LeftThigh,
        RightThigh,
        LeftFoot,
        RightFoot,
        LeftShoulder,
        RightShoulder,
        LeftHand,
        RightHand
    }

    public class InteractionObject : MonoBehaviour
    {
        public void AddToList(List<InteractionObject> list) { }
    }

    public class InteractionTarget : MonoBehaviour
    {
        public FullBodyBipedEffector effectorType;
        public float weight { get; set; }
    }

    public class InteractionSystem : MonoBehaviour
    {
        public bool inInteraction => false;
        public bool IsPaused(FullBodyBipedEffector effector) => false;
        public void StartInteraction(FullBodyBipedEffector effector, InteractionObject interactionObject, bool interrupt) { }
        public void PauseInteraction(FullBodyBipedEffector effector) { }
        public void ResumeInteraction(FullBodyBipedEffector effector) { }
        public void StopInteraction(FullBodyBipedEffector effector) { }
        public bool IsInInteraction(FullBodyBipedEffector effector) => false;
    }

    public class HitReactionCharacter : MonoBehaviour
    {
        public void Hit(Collider collider, Vector3 force, Vector3 point) { }
    }

    public class OffsetModifier : MonoBehaviour
    {
        public float weight { get; set; }
        // ik: the FullBodyBipedIK component this modifier is attached to.
        // deltaTime: time delta passed by the FinalIK update loop.
        protected FullBodyBipedIK ik;
        protected float deltaTime;
        // Virtual method overridden by PEInertiaHuman
        protected virtual void OnModifyOffset() { }
    }
}

namespace RootMotion.FinalIK.Demos
{
    // HitReactionCharacter is the base class for TestHitBack.cs
    public class HitReactionCharacter : MonoBehaviour
    {
        // cam: Camera reference used by TestHitBack for raycasting.
        public Camera cam;
        public virtual void OnHit(Collider collider, Vector3 force, Vector3 point) { }
        protected virtual void Update() { }
    }
}
