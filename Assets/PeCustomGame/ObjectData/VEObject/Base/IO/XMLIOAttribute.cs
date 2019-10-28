using UnityEngine;
using System;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class XMLIOAttribute : Attribute
{
	public XMLIOAttribute ()
	{

	}

	public XMLIOAttribute (string attr)
	{
		Order = 0;
		Attr = attr;
		Necessary = false;
		DefaultValue = null;
	}

	public int Order = 0;
	public string Attr = "xmlattr";
	public bool Necessary = false;
	public object DefaultValue;
}
