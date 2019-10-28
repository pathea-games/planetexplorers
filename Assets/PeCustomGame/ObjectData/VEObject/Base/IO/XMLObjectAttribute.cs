using UnityEngine;
using System;

[AttributeUsage(AttributeTargets.Class)]
public class XMLObjectAttribute : Attribute
{
	public XMLObjectAttribute (string name)
	{
		Name = name;
	}
	
	public string Name = "OBJECT";
}