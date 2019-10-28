using UnityEngine;

public class MousePickableChildCollider : MousePickable
{	
	protected override void OnStart ()
	{
		CollectColliders();
	}
}