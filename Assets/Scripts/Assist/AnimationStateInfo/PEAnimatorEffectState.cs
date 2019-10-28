using UnityEngine;
using System.Collections;
using Pathea.Effect;

public class PEAnimatorEffectState : PEAnimatorState
{
    public int effectId;

    GameObject m_EffectObject;
    EffectBuilder.EffectRequest m_Request;

    void OnEffectSpawned(GameObject obj)
    {
        m_EffectObject = obj;
    }

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);

        if(effectId > 0)
        {
            m_Request = EffectBuilder.Instance.Register(effectId, null, animator.transform);
            m_Request.SpawnEvent += OnEffectSpawned;
        }
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateExit(animator, stateInfo, layerIndex);

        if (m_EffectObject != null)
            GameObject.Destroy(m_EffectObject);

        if (m_Request != null)
            m_Request.SpawnEvent -= OnEffectSpawned;
    }
}
