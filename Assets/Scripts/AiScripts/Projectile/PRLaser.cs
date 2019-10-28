using UnityEngine;
using System.Collections;

public class PRLaser : Projectile
{
    public float delayTime;
    public float intervalTime;

    Collider myCollider;
	float nextStandardTime;

    public new void Start()
    {
        base.Start();

        StartCoroutine(logic());
    }

    bool CheckTargetValid()
    {
        if (emitRunner == null)
            return false;

        //AiObject aiObj = emitRunner as AiObject;

        //if (aiObj != null 
        //    && (aiObj.enemy == null 
        //    || !aiObj.enemy.valid))
        //    return false;

        return true;
    }

    Collider GetDamageCollider()
    {
        if (emitTransform != null)
        {
            Vector3 direction = transform.position - emitTransform.position;
            RaycastHit[] hitsInfos = Physics.SphereCastAll(emitTransform.position, DamageRadius, direction, direction.magnitude + 0.5f);
            hitsInfos = AiUtil.SortHitInfoFromDistance(hitsInfos);
            foreach (RaycastHit hitInfo in hitsInfos)
            {
                if (!IsIgnoreCollider(hitInfo.collider))
                    return hitInfo.collider;
            }
        }

        return null;
    }
    
    IEnumerator logic()
    {
		yield return new WaitForSeconds(delayTime);

        while (true)
        {
            if (!CheckTargetValid())
            {
                DestroyProjectile();
                yield break;
            }

            myCollider = GetDamageCollider();

            if (myCollider != null)
            {
                TriggerColliderInterval(myCollider);
                yield return new WaitForSeconds(intervalTime);
            }
            else
            {
                yield return new WaitForSeconds(0.02f);
            }
					
        }
    }
}
