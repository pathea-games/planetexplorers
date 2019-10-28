using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SkillAsset;
using ItemAsset;

public class WaterPump : Gun 
{
	public byte TerrainType = 19;
	public int TansRadius = 2;
	public int Height = 3;
	
	public float MaxOpDistance = 10f;
	
	public Transform mBackPack;
	public Transform mMuzzle;
	
	public int mFireSound;
	public int mPumpSkillId;

	//float mUnActiveTime = 0.5f;

	// Event ==> Add by WuYiqiu
	public  delegate void DirtyVoxelEvent(Vector3 pos, byte terrainType);
	//static public event DirtyVoxelEvent onDirtyVoxel;

	public override void InitEquipment (SkillRunner runner, ItemObject item)
	{
		base.InitEquipment (runner, item);
		Transform[] bones = runner.GetComponentsInChildren<Transform>();
		foreach(Transform tran in bones)
		{
			if(tran.name == "Bow_box")
			{
				mBackPack.transform.parent = tran;
				mBackPack.transform.localPosition = Vector3.zero;
				mBackPack.transform.localScale = Vector3.one;
				mBackPack.transform.localRotation = Quaternion.identity;
				break;
			}
		}
	}
	
	public override void RemoveEquipment ()
	{
		base.RemoveEquipment ();
		if(null != mBackPack)
			Destroy(mBackPack.gameObject);
	}
	
	public override bool CostSkill (ISkillTarget target, int sex = 2, bool buttonDown = false, bool buttonPressed = false)
	{
        if (!base.CostSkill(target, sex, buttonDown, buttonPressed))
        {
            return false;
        }

		if(VFVoxelWater.self.IsInWater(mMuzzle.position)
		   && mShootState == ShootState.Aim
		   && buttonPressed)
		{
			DefaultPosTarget defaultTarget = new DefaultPosTarget(mSkillRunner.transform.position + mSkillRunner.transform.forward);
			if(null != CostSkill(mSkillRunner, mPumpSkillId, defaultTarget))
			{
				AudioManager.instance.Create(transform.position, mFireSound);

				if (GameConfig.IsMultiMode && mMainPlayerEquipment)
				{
                    //Player player = mSkillRunner as Player;
                    //if (null != player)
                    //    player.RPCServer(EPacketType.PT_InGame_WaterPump);
				}

				return true;
			}
		}
		return false;
	}
}
