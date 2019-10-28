using UnityEngine;
using Mono.Data.SqliteClient;
using System;
using System.Collections;
using System.Collections.Generic;
using SkillSystem;
using Pathea;

public interface IDigTerrain
{
	IntVector4 digPosType { get; }

//	bool 	isEnable { get; } 
//	float	damage { get; }
//	float	resourceBouns { get; }
//	float	radius { get; }
//	bool	returnResource { get; }
}

public partial class VFVoxelTerrain : SkEntity, IVxChunkHelperProc
{
	public delegate void DirtyVoxelEvent(Vector3 pos, byte terrainType);
	//static public event DirtyVoxelEvent onDirtyVoxel;

//	IntVector3 _digPos = null;
	private void InitSkEntity()
	{
		Init(onAlterAttribs, null, (int)AttribType.Max);
	}
	private void onAlterAttribs(int idx, float oldValue, float newValue)
	{
		// idx == 0 : hpmax means modding voxels' types
		if(idx == 0)
		{
			SkEntity caster = GetCasterToModAttrib(idx);
			if(null != caster)
			{
				ProcChangeTypes(caster);
			}
			return;
		}
		// idx == 1 : hp means dig voxels
		if(idx == 1)
		{
			SkEntity caster = GetCasterToModAttrib(idx);
			if(null != caster)
			{	
				ProcDigVoxels(caster);
			}
			return;
		}
	}
	private void ProcDigVoxels(SkEntity caster)
	{
        SkInst inst = SkRuntimeInfo.Current as SkInst;
		IDigTerrain digTerrain = caster as IDigTerrain;
		IntVector4 digPos = null;
		if(null == digTerrain || (digPos = digTerrain.digPosType) == IntVector4.Zero || (inst != null && !(inst._target is VFVoxelTerrain)))
			return;

		float damage = caster.GetAttribute((int)AttribType.Atk) * (1f + caster.GetAttribute((int)AttribType.ResDamage));
		float radius = caster.GetAttribute((int)AttribType.ResRange);
		float resourceBonus = caster.GetAttribute((int)AttribType.ResBouns);
		float height = caster.GetAttribute((int)AttribType.ResDamage);
		bool returnResource = digPos.w == 1;
		IntVector3 intPos = digPos.XYZ;

		if(!GameConfig.IsMultiMode)
		{
			//VFVoxel getVoxel = VFVoxelTerrain.self.Voxels.SafeRead(intPos.x, intPos.y, intPos.z);
			List<VFVoxel> removeList = new List<VFVoxel>();
			DigTerrainManager.DigTerrain(intPos,
			                             damage * (returnResource ? 5f : 1f),
			                             radius,
			                             height,
			                             ref removeList, returnResource);
			if(returnResource)
			{
				bool bGetSpItems = false;
				if(caster is SkAliveEntity)
				{
					SkAliveEntity alive = (SkAliveEntity)caster;
					SkillTreeUnitMgr mgr = alive.Entity.GetCmpt<SkillTreeUnitMgr>();
					bGetSpItems = mgr.CheckMinerGetRare();
				}
				Dictionary<int, int> itemGet = DigTerrainManager.GetResouce(removeList, resourceBonus,bGetSpItems);
				if(itemGet.Count > 0)
				{
					List<int> itemsArray = new List<int>(itemGet.Count*2);
					foreach(int intemID in itemGet.Keys)
					{
						itemsArray.Add(intemID);
						itemsArray.Add(itemGet[intemID]);
					}
					caster.Pack += itemsArray.ToArray();
				}
			}
		}
		else
		{
			if(caster != null && caster._net != null)
			{
				//VFVoxel getVoxel = VFVoxelTerrain.self.Voxels.SafeRead(intPos.x,intPos.y, intPos.z);

				bool bGetSpItems = false;
				if (returnResource)
				{
					if (caster is SkAliveEntity)
					{
						SkAliveEntity alive = (SkAliveEntity)caster;
						SkillTreeUnitMgr mgr = alive.Entity.GetCmpt<SkillTreeUnitMgr>();
						bGetSpItems = mgr.CheckMinerGetRare();
					}
				}

				DigTerrainManager.DigTerrainNetwork((SkNetworkInterface)(caster._net), intPos, damage * (returnResource ? 5f : 1f),
													radius, resourceBonus, returnResource, bGetSpItems, 0.3f);
			}
		}
		
	}
	private void ProcChangeTypes(SkEntity caster)
	{
		IDigTerrain digTerrain = caster as IDigTerrain;
		IntVector4 digPos = null;
		if(null == digTerrain || (digPos = digTerrain.digPosType) == IntVector4.Zero)
		{
			return;
		}

		float radius = 2;
		byte targetType = (byte)Mathf.RoundToInt(_attribs.sums[0]);
		IntVector3 intPos = digPos.XYZ;
		if(caster is Pathea.Projectile.SkProjectile)
		{
			SkEntity trueCaster =  ((Pathea.Projectile.SkProjectile)caster).GetSkEntityCaster();
			DigTerrainManager.ChangeTerrain(intPos, radius, targetType, trueCaster);
		}
		else
		{
			DigTerrainManager.ChangeTerrain(intPos, radius, targetType, caster);
		}
	}

	public override void ApplyEff (int effId, SkRuntimeInfo info)
	{
		base.ApplyEff (effId, info);
		Pathea.Effect.EffectBuilder.Instance.RegisterEffectFromSkill(effId, info, transform);
	}
}
