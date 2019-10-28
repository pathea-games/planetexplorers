using UnityEngine;
using System.Collections;

public class AiBehaveTree : MonoBehaviour
{
    bool m_valid = true;

    public virtual bool valid
    {
        get { return m_valid; }
        set 
        {
            if (!value && m_valid)
                Reset();
         
            m_valid = value;
        }
    }

    public virtual void Reset() { }

    public virtual bool isMember { get { return false; } }

    public virtual bool isSingle { get { return false; } }

    public virtual bool isGroup { get { return false; } }
}
