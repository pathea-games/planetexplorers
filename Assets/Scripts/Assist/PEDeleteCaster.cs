using UnityEngine;
using System.Collections;
using SkillSystem;
using Pathea.Effect;

public class PEDeleteCaster : MonoBehaviour, ISkEffectEntity
{
    Transform m_Delete;

    public SkInst Inst
    {
        set { if (value != null) m_Delete = value._caster.transform; }
    }

    void Start()
    {
        if (m_Delete != null)
        {
            GameObject.Destroy(m_Delete.gameObject);
        }

        GameObject.Destroy(gameObject);
    }
}
