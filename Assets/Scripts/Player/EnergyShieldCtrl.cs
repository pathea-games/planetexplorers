using UnityEngine;
using System.Collections;
using SkillAsset;

public class EnergyShieldCtrl : MonoBehaviour
{
	EnergySheildHandler mSheild;
	
	public SkillRunner	mSkillRunner;
	
	public bool			mActive;
	// Use this for initialization
	void Start () {
		mSheild = GetComponent<EnergySheildHandler>();
	}
	
	void OnTriggerEnter(Collider other)
	{
		Projectile proj = other.gameObject.GetComponent<Projectile>();
		if(null != proj)
			HitShield(proj);
	}
	
	public void HitShield(Projectile proj)
	{
		if(!proj.ShieldHasBeenHitted(this))
		{
			if(mActive)
			{
				Vector3 dir = proj.transform.position - transform.position;
				mSheild.Impact(transform.position + dir.normalized);
				proj.ApplyDamageReduce(mSkillRunner.ApplyEnergyShieldAttack(proj), this);
			}
		}
	}
}
