using UnityEngine;
using Mono.Data.SqliteClient;
using System;
using System.Collections;
using System.Collections.Generic;
using SkillSystem;
using Pathea;

public partial class Block45Man : SkEntity
{
	private void InitSkEntity()
	{
		Init(onAlterAttribs, null, (int)AttribType.Max);
	}
	private void onAlterAttribs(int idx, float oldValue, float newValue)
	{
		// idx == 1 : hp means dig b45s
		if (idx == 0)
		{
			new PeTipMsg(PELocalization.GetString(DigTerrainManager.DonotChangeVoxelNotice), "", PeTipMsg.EMsgLevel.Norm);
		}
		else if(idx == 1)
		{
			SkEntity caster = GetCasterToModAttrib(idx);
			if(null != caster)
			{	
				ProcDigB45s(caster);
			}
			return;
		}
	}
	private void ProcDigB45s(SkEntity caster)
	{
        SkInst inst = SkRuntimeInfo.Current as SkInst;
		IDigTerrain digTerrain = caster as IDigTerrain;
        IntVector4 digPos = null;
		if(null == digTerrain || (digPos = digTerrain.digPosType) == IntVector4.Zero || (inst != null && !(inst._target is Block45Man)))
			return;
		
		float damage = caster.GetAttribute((int)AttribType.Atk);
		float radius = caster.GetAttribute((int)AttribType.ResRange);
		float height = caster.GetAttribute((int)AttribType.ResRange);
        //		float resourceBonus = caster.GetAttribute((int)AttribType.ResBouns);
        //		bool returnResource = digPos.w == 1;

        PeEntity entity = caster != null ? caster.GetComponent<PeEntity>() : null;
        if (entity != null && entity.monster != null)
        {
            radius = entity.bounds.extents.x + 1;
            height = entity.bounds.extents.y + 1;
        }

		if(null != inst && null != inst._colInfo)
		{
			int effecID = 0;
			if(radius < 3)
				effecID = 258;
			else if(radius < 5f)
				effecID = 259;
			else
				effecID = 260;
			Pathea.Effect.EffectBuilder.Instance.Register(effecID, null, inst._colInfo.hitPos, Quaternion.identity);
		}

        if (!GameConfig.IsMultiMode)
		{
			List<B45Block> removeList = new List<B45Block>();
			DigTerrainManager.DigBlock45(digPos.ToIntVector3(), damage*0.2f, radius, height, ref removeList, false);

		}
		else
		{
			/// TODO : code for multiplayer
		}		
	}
	
	public override void ApplyEff (int effId, SkRuntimeInfo info)
	{
		base.ApplyEff (effId, info);
		Pathea.Effect.EffectBuilder.Instance.RegisterEffectFromSkill(effId, info, transform);
	}
}
