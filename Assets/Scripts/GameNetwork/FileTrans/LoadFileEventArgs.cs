using UnityEngine;
using System.Collections;
using System;

public class LoadFileEventArgs : EventArgs
{
	private ulong _hashCode;
	internal ulong HashCode { get { return _hashCode; } }
	
	internal LoadFileEventArgs(ulong hashCode)
	{
		_hashCode = hashCode;
	}
}
