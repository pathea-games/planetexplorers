using UnityEngine;
using System.Collections;

// Every single "action element"
// Base class
public abstract class VCEModify
{
	public abstract void Undo();
	public abstract void Redo();
}
