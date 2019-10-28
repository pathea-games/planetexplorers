using UnityEngine;
using System.Collections;

public class HitEffectScale : MonoBehaviour
{

    public ParticleSystem root;
    public ParticleSystem[] childs;

    float m_Scale;

    Vector3 m_EmissionScale = Vector3.one;

    public Vector3 EmissionScale
    {
        set
        {
            if(m_EmissionScale != value)
            {
                m_EmissionScale = value;

                transform.localScale = m_EmissionScale;
            }
        }
    }

    public float Scale
    {
        set
        {
            if(Mathf.Abs(m_Scale - value) > 0.1f && value > PETools.PEMath.Epsilon)
            {
                m_Scale = value;

                //root.emissionRate *= m_Scale;

                for (int i = 0; i < childs.Length; i++)
                {
                    childs[i].startSize *= (m_Scale * 0.5f);
                }
            }
        }
    }
}
