using UnityEngine;
using System.Collections;

public class BTCoroutine : IEnumerator
{
    IEnumerator m_Enumerator;
    MonoBehaviour m_Behaviour;
    Coroutine m_Coroutine;

    public BTCoroutine(MonoBehaviour behaviour, IEnumerator enumerator)
    {
        m_Behaviour = behaviour;
        m_Enumerator = enumerator;
    }

    public bool IsStart { get { return m_Coroutine != null; } }

    public void Start()
    {
        if (m_Behaviour != null && m_Enumerator != null && m_Coroutine == null)
        {
            m_Coroutine = m_Behaviour.StartCoroutine(m_Enumerator);
        }
    }

    public void Stop()
    {
        if (m_Behaviour != null && m_Enumerator != null && m_Coroutine != null)
        {
            m_Coroutine = null;
            m_Behaviour.StopCoroutine(m_Enumerator);
        }
    }

    #region IEnumerator interface
    public object Current { get { return m_Enumerator.Current; } }
    public bool MoveNext() { return m_Enumerator.MoveNext(); }
    public void Reset() { m_Enumerator.Reset(); }
	#endregion
}
