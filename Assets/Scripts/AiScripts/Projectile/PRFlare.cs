using UnityEngine;
using System.Collections;

public class PRFlare : Pathea.Projectile.SkProjectile
{
    public float timeSign1;
	public float timeSign2;
	public float maxRange;
	public float finalRange;
	public float maxIntensity;
	public float finalIntensity;
	public float existTime;

    new Light light;
	float startTime;
	float process;
	//bool blew = true;
	//bool valid = true;	//palyer has multiple colider, which may causes a projectile makes multiple effects

//    void OnTriggerEnter(Collider other)
//    {
//        if (IsIgnoreCollider(other))
//            return;
//
//		if (!valid)
//			return;
//		
//		valid = false;
//        TriggerCollider(other);
//    }

    public void Start()
    {
//        base.Start();

        light = GetComponentInChildren<Light>();
		startTime = Time.time;
		
        StartCoroutine(FlareIntensity());
    }

    IEnumerator FlareIntensity()
    {
		while(true) 
		{
			process = Time.time - startTime;
	        if(process <= timeSign1)
			{
				light.range = Mathf.Lerp(0f, maxRange, process / timeSign1);
				light.intensity = Mathf.Lerp(0f, maxIntensity, process / timeSign1);
			}
			else if(process <= timeSign2)
			{
				light.range = maxRange;
				light.intensity = maxIntensity;
			}
			else
			{
				light.range = Mathf.Lerp(maxRange, finalRange, (process - timeSign2) / (existTime - timeSign2));
				light.intensity = Mathf.Lerp(maxIntensity, finalIntensity, (process - timeSign2) / (existTime - timeSign2));
			}
			yield return new WaitForSeconds(0.05f);	
		}
    }

	public override void OnDestroy ()
	{
		base.OnDestroy ();
		Pathea.Effect.EffectBuilder.Instance.Register(67, null, transform.position, Quaternion.identity);
	}
}
