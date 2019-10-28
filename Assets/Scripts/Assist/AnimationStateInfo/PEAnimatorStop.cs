using UnityEngine;
using System.Collections;

public class PEAnimatorStop : PEAnimatorState
{
    bool m_Interrupt;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);

        m_Interrupt = false;
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateUpdate(animator, stateInfo, layerIndex);

        if (animator.deltaPosition.sqrMagnitude < 0.1f * 0.1f || Entity.Rigid == null)
            return;

        float radius = Entity.maxRadius;
        Vector3 point1 = Entity.Rigid.worldCenterOfMass - Vector3.up * Entity.maxHeight * 0.5f;
        Vector3 point2 = Entity.Rigid.worldCenterOfMass + Vector3.up * Entity.maxHeight * 0.5f;
        Vector3 velocity = animator.deltaPosition;

        RaycastHit hitInfo;
		int layer = 1 << Pathea.Layer.VFVoxelTerrain
				| 1 << Pathea.Layer.Building
				| 1 << Pathea.Layer.SceneStatic
				| 1 << Pathea.Layer.Unwalkable
				| 1 << Pathea.Layer.NearTreePhysics;
        if (Physics.CapsuleCast(point1, point2, radius, velocity, out hitInfo, velocity.magnitude * 5, layer))
        {
            if (!m_Interrupt)
            {
                if (Vector3.Angle(hitInfo.normal, Vector3.up) > 45.0f)
                {
                    m_Interrupt = true;
                    animator.SetBool("Interrupt", true);
                }
            }
            //Debug.DrawRay(hitInfo.point, hitInfo.normal.normalized * 5.0f, Color.red);
        }
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateExit(animator, stateInfo, layerIndex);

        m_Interrupt = false;
    }
}
