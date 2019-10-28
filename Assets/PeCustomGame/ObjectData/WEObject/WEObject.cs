using UnityEngine;
using System.Collections;
using System.Xml;
using Pathea.Maths;

public abstract class WEObject : VETRSPoint
{
	[XMLIO(Order = -9, Attr = "prt", Necessary = true)]
	public int Prototype
	{
		get;
		set; 
	}
}
