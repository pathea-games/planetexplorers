using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

// user's action
public class VCEAction
{
	public VCEAction ()
	{
		Modifies = new List<VCEModify> ();
	}
	public void Destroy ()
	{
		if ( Modifies != null )
		{
			Modifies.Clear();
			Modifies = null;
		}
	}
	public void Clear ()
	{
		if ( Modifies != null )
		{
			Modifies.Clear();
		}
	}
	public void Undo ()
	{
		for ( int i = Modifies.Count - 1; i >= 0; --i )
		{
			Modifies[i].Undo();
		}
		VCEHistory.s_Modified = true;
	}
	public void Redo ()
	{
		for ( int i = 0; i < Modifies.Count; ++i )
		{
			Modifies[i].Redo();
		}
		VCEHistory.s_Modified = true;
	}
	public void DoButNotRegister ()
	{
		for ( int i = 0; i < Modifies.Count; ++i )
			Modifies[i].Redo();
	}
	public void Do ()
	{
		Redo();
		Register();
	}
	public void Register ()
	{
		VCEHistory.AddAction(this);
		VCEHistory.s_Modified = true;
	}
	
	public List<VCEModify> Modifies = null;
}
