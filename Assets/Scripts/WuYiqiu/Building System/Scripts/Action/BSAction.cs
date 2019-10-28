using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BSAction
{
	List<BSModify> m_Modifies = new List<BSModify>();

	public void AddModify (BSModify modify)
	{
		m_Modifies.Add(modify);
	}

	public bool Undo()
	{
		for (int i = m_Modifies.Count - 1; i >= 0; i--)
		{
            //if (! m_Modifies[i].Undo())
            //	m_Modifies.RemoveAt(i);
            //else
            //	i--;
			if (!m_Modifies[i].Undo())
			{
				return false;
			}
        }
		return true;
	}

	public bool Redo()
	{
		for (int i = 0; i < m_Modifies.Count; i++)
		{
            //if (! m_Modifies[i].Redo())
            //	m_Modifies.RemoveAt(i);
            //else
            //	i ++;

            if (!m_Modifies[i].Redo())
				return false;
        }

		return true;
	}

	public bool IsEmpty()
	{
		return m_Modifies.Count == 0;
	}

	public void ClearNullModify()
	{
		for (int i = m_Modifies.Count - 1; i >=0; i--)
		{
			if (m_Modifies[i].IsNull())
				m_Modifies.RemoveAt(i);
		}

	}

	public bool Do()
	{
		return Redo();
	}
}
