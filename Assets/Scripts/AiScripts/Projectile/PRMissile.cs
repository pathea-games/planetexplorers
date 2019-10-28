using UnityEngine;
using System.Collections;

public class PRMissile : Projectile
{	
    public new void Update()
	{
		base.Update();

        CheckMovementCollision();
	}
}
