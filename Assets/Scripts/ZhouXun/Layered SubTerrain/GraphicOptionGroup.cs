using UnityEngine;
using System;

[Serializable]
public class GraphicOptionGroup
{
	public float Fastest;
	public float Fast;
	public float Normal;
	public float Good;
	public float Fantastic;
	
	public float Level(int l)
	{
		switch (l)
		{
		case 1: return Fastest;
		case 2: return Fast;
		case 3: return Normal;
		case 4: return Good;
		case 5: return Fantastic;
		default: return Fastest;
		}
	}
}
