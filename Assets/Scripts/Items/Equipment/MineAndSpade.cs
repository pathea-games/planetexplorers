using UnityEngine;
using System.Collections;
using SkillAsset;

public class MineAndSpade : Equipment 
{
	public OpCubeCtrl mOpCube;
	float mUnActiveTime = 0.5f;

	VFTerrainTarget mTarget;

	public override void InitEquipment (SkillRunner runner, ItemAsset.ItemObject item)
	{
		base.InitEquipment (runner, item);
		if(mMainPlayerEquipment)
		{
			mOpCube.Active = true;
			mOpCube.Enable = true;
		}
		else
			mOpCube.Active = false;
	}

	public override void RemoveEquipment ()
	{
		base.RemoveEquipment ();
		GameObject.Destroy(mOpCube.gameObject);
	}
	
	public override bool CostSkill (ISkillTarget target, int sex = 2, bool buttonDown = false, bool buttonPressed = false)
	{
		if(mMainPlayerEquipment)
		{
			mUnActiveTime = 0.5f;
			mOpCube.Active = true;
		}
		if(mSkillMaleId.Count == 0 || mSkillFemaleId.Count == 0)
			return false;
        if (!base.CostSkill(target, sex, buttonDown, buttonPressed))
        {
            return false;
        }

		if(mOpCube.Active && mOpCube.Enable)
		{
			int castSkillId = 0;
			
			switch(sex)
			{
			case 1:
				castSkillId = mSkillFemaleId[0];
				break;
			case 2:
				castSkillId = mSkillMaleId[0];
				break;
			}
			
			EffSkillInstance skillInstance = mSkillRunner.GetRunningEff(castSkillId);
			
			if(null != skillInstance && buttonPressed)
			{
	            skillInstance.mSkillCostTimeAdd = true;
				skillInstance.mNextTarget = mTarget;
			}
			else if(null == skillInstance && buttonDown)
			{
				skillInstance = CostSkill(mSkillRunner, castSkillId, mTarget);
			}
			return null != skillInstance;
		}
		return false;
	}
	
	void LateUpdate()
	{
		if(mMainPlayerEquipment)
		{
			CheckTerrain();
			mUnActiveTime -= Time.deltaTime;
			if(mUnActiveTime < 0)
				mOpCube.Active = false;
		}
	}

	void CheckTerrain()
	{
		Ray ray = PeCamera.mouseRay;
		RaycastHit hitInfo;
		mOpCube.Enable = false;
		if(Physics.Raycast(ray, out hitInfo, 100f, 1<<Pathea.Layer.VFVoxelTerrain))
		{
			VFVoxelChunkGo chunk = hitInfo.collider.gameObject.GetComponent<VFVoxelChunkGo>();
			if (chunk != null)
			{
				mOpCube.Enable = true;
				IntVector3 basePos = new IntVector3();
				
				if(hitInfo.normal.x == 0 || hitInfo.point.x -(int)hitInfo.point.x > 0.5f)
					basePos.x = Mathf.RoundToInt(hitInfo.point.x);
				else
					basePos.x = hitInfo.normal.x > 0 ? Mathf.CeilToInt(hitInfo.point.x) : Mathf.FloorToInt(hitInfo.point.x);
				
				if(hitInfo.normal.y == 0 || hitInfo.point.y -(int)hitInfo.point.y > 0.5f)
					basePos.y = Mathf.RoundToInt(hitInfo.point.y);
				else
					basePos.y = hitInfo.normal.y > 0 ? Mathf.CeilToInt(hitInfo.point.y) : Mathf.FloorToInt(hitInfo.point.y);
				
				if(hitInfo.normal.z == 0 || hitInfo.point.z -(int)hitInfo.point.z > 0.5f)
					basePos.z = Mathf.RoundToInt(hitInfo.point.z);
				else
					basePos.z = hitInfo.normal.z > 0 ? Mathf.CeilToInt(hitInfo.point.z) : Mathf.FloorToInt(hitInfo.point.z);
				
				IntVector3 nearestPos = basePos;
				
				float minDis = 100f;
				
				VFVoxel voxel = VFVoxelTerrain.self.Voxels.SafeRead(basePos.x, basePos.y, basePos.z);
				
				if (voxel.Volume < 127)
				{
					for(int i = -1; i <= 1; i++)
					{
						for(int j = -1; j <= 1; j++)
						{
							for(int k = -1; k <= 1; k++)
							{
								IntVector3 now = new IntVector3(basePos.x + i, basePos.y + j, basePos.z + k);
								voxel = VFVoxelTerrain.self.Voxels.SafeRead(now.x , now.y, now.z);
								if(voxel.Volume > 127)
								{
									float nowDis = (now.ToVector3() - hitInfo.point).magnitude;
									if(minDis >= nowDis)
									{
										nearestPos = now;
										minDis = nowDis;
									}
								}
							}
						}
					}
				}
				
				mOpCube.transform.position = nearestPos.ToVector3();
				voxel = VFVoxelTerrain.self.Voxels.SafeRead(nearestPos.x, nearestPos.y, nearestPos.z);

				mOpCube.Enable = Vector3.Distance(mSkillRunner.transform.position + Vector3.up, hitInfo.point) < 3f
					&& voxel.Volume > 0
					&& mSkillRunner.GetComponent<Rigidbody>().velocity.sqrMagnitude < 9;
				if(mOpCube.Enable)
					mTarget = new VFTerrainTarget(hitInfo.point, nearestPos, ref voxel);
			}
			else
				mOpCube.Enable = false;
		}
	}
}
