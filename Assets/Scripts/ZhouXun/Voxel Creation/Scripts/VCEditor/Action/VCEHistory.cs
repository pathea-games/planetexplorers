using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public static class VCEHistory
{
	public const int MAX_ACTION_CNT = 32;
	public static bool s_Modified = false;
	
	// Undo & Redo lists.
	private static List<VCEAction> m_Undos = null;
	private static List<VCEAction> m_Redos = null;

	// Init
	public static void Init ()
	{
		m_Undos = new List<VCEAction> ();
		m_Redos = new List<VCEAction> ();
	}
	// Destroy
	public static void Destroy ()
	{
		if ( m_Undos != null )
		{
			foreach ( VCEAction action in m_Undos )
			{
				action.Destroy();
			}
			m_Undos.Clear();
			m_Undos = null;
		}
		if ( m_Redos != null )
		{
			foreach ( VCEAction action in m_Redos )
			{
				action.Destroy();
			}
			m_Redos.Clear();
			m_Redos = null;
		}
		GC.Collect();
	}
	// Clear all records
	public static void Clear ()
	{
		ClearUndos();
		ClearRedos();
	}
	// Clear undo list
	private static void ClearUndos ()
	{
		if ( m_Undos != null )
		{
			foreach ( VCEAction action in m_Undos )
			{
				action.Destroy();
			}
			m_Undos.Clear();
		}
	}
	// Clear undo list
	private static void ClearRedos ()
	{
		if ( m_Redos != null )
		{
			foreach ( VCEAction action in m_Redos )
			{
				action.Destroy();
			}
			m_Redos.Clear();
		}
	}
	// Do an action
	public static void AddAction (VCEAction action)
	{
		if ( action.Modifies.Count == 0 )
			return;
		ClearRedos();
		m_Undos.Insert( 0, action );
		if ( m_Undos.Count > MAX_ACTION_CNT )
		{
			m_Undos[MAX_ACTION_CNT].Destroy();
			m_Undos.RemoveAt( MAX_ACTION_CNT );
		}
	}
	// Undo
	public static bool Undo ()
	{
		if ( m_Undos.Count > 0 )
		{
			VCEAction action = m_Undos[0];
			action.Undo();
			m_Redos.Insert(0, action);
			m_Undos.RemoveAt(0);
			GC.Collect();
			return true;
		}
		return false;
	}
	// Redo
	public static bool Redo ()
	{
		if ( m_Redos.Count > 0 )
		{
			VCEAction action = m_Redos[0];
			action.Redo();
			m_Undos.Insert(0, action);
			m_Redos.RemoveAt(0);
			GC.Collect();
			return true;
		}
		return false;		
	}
	// Can undo ?
	public static bool CanUndo ()
	{
		if ( m_Undos == null )
			return false;
		if ( m_Undos.Count > 0 )
			return true;
		return false;
	}
	// Can redo ?
	public static bool CanRedo ()
	{
		if ( m_Redos == null )
			return false;
		if ( m_Redos.Count > 0 )
			return true;
		return false;
	}
}
