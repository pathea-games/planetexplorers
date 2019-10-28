using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PRShockwave : Projectile
{
    public Transform trigger;
    public float maxRadius;

    float progress = 0f;
	float radius;
	List<Collider> coll = new List<Collider>();
    void OnTriggerEnter(Collider other)
    {
        if (IsIgnoreCollider(other))
            return;
		if (coll.Contains(other))
			return;
		coll.Add(other);

        TriggerCollider(other);
    }

    public new void Update()
    {
        base.Update();

        if (trigger != null)
        {
			progress += Time.deltaTime / existTime;
            radius = maxRadius * progress;
            
            trigger.localScale = new Vector3(radius, trigger.localScale.y, radius);
        }
    }
}
