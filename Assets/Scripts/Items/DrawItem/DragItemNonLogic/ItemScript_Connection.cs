using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ItemAsset;
using Pathea.Maths;

public class ItemScript_Connection : ItemScript
{
	public List<Vector3> 	mConnectionPoint;

    public override void OnConstruct()
    {
		base.OnConstruct();
		
		gameObject.layer = Pathea.Layer.VFVoxelTerrain;
	}
	
}
