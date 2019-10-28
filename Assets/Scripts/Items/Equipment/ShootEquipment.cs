using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ItemAsset;
using SkillAsset;

//public enum ShootState
//{
//	Null,
//	PutOn,
//	Aim,
//	Fire,
//	Reload,
//	PutOff
//}

public class ShootEquipment : Equipment 
{
	[HideInInspector]
	public Vector3		mTarget;
	[HideInInspector]
	protected ShootState mShootState;
	
	public float		mRange = 100f;
	
	public void SetShootState(ShootState ss)
	{
		mShootState = ss;
	}
	
	public virtual DefaultPosTarget GetShootTargetByMouse()
	{
		RaycastHit hitInfo;
        //Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        Ray ray = PeCamera.mouseRay;

        mTarget = ray.origin + ray.direction * mRange;

        if (Physics.Raycast(ray, out hitInfo, mRange, (1 << Pathea.Layer.AIPlayer)
			+ (1 << Pathea.Layer.ProxyPlayer)
            + (1 << Pathea.Layer.VFVoxelTerrain))
			&& !hitInfo.collider.isTrigger
            && Vector3.Distance(hitInfo.point, ray.origin) > Vector3.Distance(mSkillRunner.transform.position + 1f * Vector3.up, ray.origin)
            && Vector3.Angle(mSkillRunner.transform.forward, hitInfo.point - mSkillRunner.transform.position) < 90)
        {
            mTarget = hitInfo.point;
        }
		
        return new DefaultPosTarget(mTarget);
	}
}
