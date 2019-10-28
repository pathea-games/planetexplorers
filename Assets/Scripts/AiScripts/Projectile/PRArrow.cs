using UnityEngine;
using System.Collections;
using SkillAsset;

public class PRArrow : Projectile
{
	public byte effType = 0;
	/* 8	pos = other.position
	 * 4	pos = self.position
	 * 2	
	 * 1	
	 * default	closet position
	 * 128	qua = self to other
	 * 64	qua = self.rotation
	 * 32	qua = random
	 * 16							
	 * default	identity	*/

    public new void Update()
    {
        base.Update();

        CheckMovementCollision();
    }
}
