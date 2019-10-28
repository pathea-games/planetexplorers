using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PRGhost : Projectile
{
	public float attackInterval;

	List<Collider> targets = new List<Collider>();
	public new void Start()
	{
		base.Start();
		StartCoroutine(logic());
	}
	
	IEnumerator logic()
	{
		while(true){
			while(true){
				if(targets.Count == 0)				
					yield return new WaitForSeconds(0.02f);				
				else
					break;
			}
			if(attackInterval == 0f)
			{
                TriggerCollider(targets[0]);
				break;
			}
			else
			{
				foreach(Collider c in targets){
					if(c == null)
						continue;
                    TriggerColliderInterval(c);				
				}
				yield return new WaitForSeconds(attackInterval);
			}
		}		
	}

    void OnTriggerEnter(Collider other)
    {
        if (IsIgnoreCollider(other))
            return;
		if (targets.Count != 0)
		{
			foreach(Collider c in targets)
			{
				if (c == null) continue;
				if (c.transform == other.transform)
					return;
			}
		}
		targets.Add(other);
    }
	
	void OnTriggerExit(Collider other)
	{
		if(targets.Contains(other))
			targets.Remove(other);
	}
}
 