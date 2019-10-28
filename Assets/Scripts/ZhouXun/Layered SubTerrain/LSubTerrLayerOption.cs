using UnityEngine;
using System;

// The option class of each terrain layer in layered-subterrain system
[Serializable]
public class LSubTerrLayerOption
{
	public string Name = "";
	public float MinTreeHeight = 0F;
	public float MaxTreeHeight = 10000F;
	public GraphicOptionGroup BillboardDist;
	public GraphicOptionGroup BillboardFadeLen;
}
