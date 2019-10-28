using UnityEngine;
using System.Collections;
using Pathea.Maths;
using System.Xml;
using System;

[XMLObject("ITEM")]
public class WEItem : WEEntity
{
	[XMLIO(Attr = "canPickup", DefaultValue = true)]
	public bool CanPickup = true;
}
