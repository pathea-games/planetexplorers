using UnityEngine;
using System.Collections;
using PeUIEffect;

public abstract class UIBaseSighting : MonoBehaviour 
{
	[SerializeField] float mValue = 0;
	public float Value { get{return mValue;}  set {mValue = value;}}
	public bool bShoot = false; 
	protected UISightingOnShootEffect mShootEffect = null;
	public virtual void OnShoot(){bShoot = true;}
	protected virtual void Start()
	{
		mShootEffect = GetComponent<UISightingOnShootEffect>(); 
	}


	protected virtual void Update()
	{
		if (bShoot)
		{
			if (mShootEffect != null)
			{
				mShootEffect.Play();
			}
			bShoot = false; 
		}

		if (mShootEffect != null)
			mValue += mShootEffect.Value;
	}
}
