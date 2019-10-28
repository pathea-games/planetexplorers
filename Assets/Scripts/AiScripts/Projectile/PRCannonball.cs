using UnityEngine;
using System.Collections;
using SkillAsset;

public class PRCannonball : Projectile
{
    public new void Update()
    {
        base.Update();

        CheckMovementCollision();
    }
}
