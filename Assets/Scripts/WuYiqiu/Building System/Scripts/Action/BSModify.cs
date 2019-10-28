using UnityEngine;
using System.Collections;

public abstract class BSModify  
{
	public abstract bool Undo();
	public abstract bool Redo();

	public virtual bool IsNull()
	{
		return false;
	}
}
