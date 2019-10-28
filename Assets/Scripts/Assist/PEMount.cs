using UnityEngine;
using System.Collections;
using Pathea;

public class PEMount : MonoBehaviour 
{
    Transform m_Target;

    public Transform Target
    {
        set 
        { 
            if(m_Target != value)
            {
                if (m_Target != null)
                {
                    Motion_Move mover = m_Target.GetComponentInChildren<Motion_Move>();
                    if (mover != null)
                    {
                        mover.enabled = false;
                    }
                }

                m_Target = value;

                if (m_Target != null)
                {
                    Motion_Move mover = m_Target.GetComponentInChildren<Motion_Move>();
                    if (mover != null)
                    {
                        mover.enabled = false;
                    }
                }
            }
        }
    }

    void Update()
    {
        if (m_Target != null)
        {
            //m_Target.position = Vector3.Lerp(m_Target.position, transform.position, Time.deltaTime * 0.5f);
            //m_Target.rotation = Quaternion.Slerp(m_Target.rotation, transform.rotation, Time.deltaTime * 0.5f);

            m_Target.position = transform.position;
            m_Target.rotation = transform.rotation;
        }
    }
}
