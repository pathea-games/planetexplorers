using UnityEngine;
using System.Collections;

public abstract class VETRSPoint : VEPoint
{
	[XMLIO(Attr = "rot")]
	public Quaternion Rotation
	{
		get;
		set;
	}

    [XMLIO(Attr = "scl", DefaultValue = 1f)]
    public Vector3 Scale
    {
        get;
        set;
    }
}
