using UnityEngine;
using System.Collections;

public class TRMultiImpulse : TRImpulse
{
	public float maxUp;
	public float maxRight;

	protected override void Emit(Vector3 targetPos, Transform emitTrans, int index)
    {
		if(index != 0)
			targetPos += new Vector3((Random.value - 0.5f) * maxRight, (Random.value - 0.5f) * maxUp, (Random.value - 0.5f) * maxRight);
		base.Emit(targetPos, emitTrans, index);
    }
}
