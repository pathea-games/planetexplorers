using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Experimental.Director;

public class PEAnimatorFootDecal : StateMachineBehaviour
{
    class FootDecal
    {
        public Transform parent;
        public Transform tr;
        public int soundID;

        float m_CurHeight;
        float m_PreHeight;
        float m_CurVelocity;
        float m_PreVelocity;

        bool m_Land;

        public FootDecal(Transform argParent, Transform argTrans, int soundid)
        {
            parent = argParent;
            tr = argTrans;
            soundID = soundid;
        }

        void OnLand()
        {
            AudioManager.instance.Create(tr.position, soundID);
        }

        void OnFan()
        {

        }

        public void Update(float minSpeed)
        {
            if (tr == null) return;

            m_CurHeight = parent.InverseTransformPoint(tr.position).y;

            m_CurVelocity = m_CurHeight - m_PreHeight;

            if (m_CurVelocity < -minSpeed && m_PreVelocity < -minSpeed)
                m_Land = false;

            if(m_CurVelocity >= 0.0f && !m_Land)
            {
                m_Land = true;

                OnLand();
            }

            m_PreHeight = m_CurHeight;
            m_PreVelocity = m_CurVelocity;
        }
    }

    public float minSpeed;
    public string[] foots;

    bool m_Init;
    List<FootDecal> m_FootDecals;

    void Init(Transform trans)
    {
        m_FootDecals = new List<FootDecal>();
        for (int i = 0; i < foots.Length; i++)
        {
            Transform tr = PETools.PEUtil.GetChild(trans, foots[i]);
            if(tr != null)
            {
                m_FootDecals.Add(new FootDecal(trans, tr, 838));
            }
        }
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateUpdate(animator, stateInfo, layerIndex);

        if(!m_Init)
        {
            m_Init = true;

            Init(animator.transform);
        }

        for (int i = 0; i < m_FootDecals.Count; i++)
        {
            m_FootDecals[i].Update(minSpeed);
        }
    }
}
