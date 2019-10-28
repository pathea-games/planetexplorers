using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class BSHistory  
{
	const int s_MaxCount = 5;

	static List<BSAction>  m_Undos = new List<BSAction>();
	static List<BSAction>  m_Redos = new List<BSAction>();

	public static void Clear()
	{
		m_Redos.Clear();
		m_Undos.Clear();
	}

	public static void AddAction (BSAction action)
	{
		ClearNullAction();

		if (m_Undos.Count == s_MaxCount )
		{
			m_Undos.RemoveAt(0);
		}

		m_Undos.Add(action);
		m_Redos.Clear();
	}

	public static void Undo()
	{
		ClearNullAction();

		if (m_Undos.Count != 0)
		{
			int index = m_Undos.Count -1;
			m_Undos[index].Undo();

			m_Redos.Add(m_Undos[index]);
			m_Undos.RemoveAt(index);
		}
	}

	public static void Redo()
	{
		ClearNullAction();

		if (m_Redos.Count != 0)
		{
			int index = m_Redos.Count - 1;
			m_Redos[index].Redo();

			m_Undos.Add(m_Redos[index]);
			m_Redos.RemoveAt(index);
		}
	}


	public static void ClearNullAction ()
	{
		for (int i = m_Undos.Count - 1; i >= 0; i--)
		{
			m_Undos[i].ClearNullModify();
			if (m_Undos[i].IsEmpty())
				m_Undos.RemoveAt(i);
		}

		for (int i = m_Redos.Count - 1; i >= 0; i--)
		{
			m_Redos[i].ClearNullModify();
			if (m_Redos[i].IsEmpty())
				m_Redos.RemoveAt(i);
		}
	}
}
