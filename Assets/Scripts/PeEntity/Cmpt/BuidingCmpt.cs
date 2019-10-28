using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SkillSystem;

namespace Pathea
{
	public class BuidingCmpt : PeCmpt
	{
		
		// Use this for initialization
		new void Start () {

			if(Entity.peSkEntity != null)
			{
				Entity.peSkEntity.onHpReduce += OnDamage;
				Entity.peSkEntity.attackEvent += OnAttack;
				Entity.peSkEntity.onSkillEvent += OnSkillTarget;
			}
		}
		
		// Update is called once per frame
		void Update () {
			
		}

		void OnAttack(SkEntity skEntity, float damage)
		{
			PeEntity tarEntity = skEntity.GetComponent<PeEntity>();
			if (tarEntity != null && tarEntity != Entity)
			{
                NpcHatreTargets.Instance.TryAddInTarget(Entity,tarEntity,damage);
			}
		}
		
		void OnDamage (SkEntity entity, float damage)
		{
			if (null == Entity.peSkEntity || null == entity)
				return;
			
			PeEntity peEntity = entity.GetComponent<PeEntity>();
			if(peEntity == Entity)
				return ;

            NpcHatreTargets.Instance.TryAddInTarget(Entity, peEntity, damage,true);
		}
		
		void OnSkillTarget(SkEntity caster)
		{
			if (null == Entity.peSkEntity || null == caster)
				return;
			
			int playerID = (int)Entity.peSkEntity.GetAttribute ((int)AttribType.DefaultPlayerID);
            //SkEntity Ca_entity = PETools.PEUtil.GetCaster(caster);
			PeEntity peEntity = caster.GetComponent<PeEntity>();
			if(peEntity == Entity)
				return ;
			
			float tansDis = peEntity.IsBoss ? 128f : 64f;
			bool canTrans = false;
			if (GameConfig.IsMultiClient)
			{
				if (ForceSetting.Instance.GetForceType(playerID) == EPlayerType.Human)
					canTrans = true;
			}
			else
			{
				if (ForceSetting.Instance.GetForceID(playerID) == 1)
					canTrans = true;
			}
			
			if (canTrans)
			{
				List<PeEntity> entities = EntityMgr.Instance.GetEntities (Entity.peTrans.position, tansDis, playerID, false, Entity);
				for(int i = 0; i < entities.Count; i++)
				{
					if (entities[i] == null)
						continue;
					
					if (!entities[i].Equals (Entity) && entities[i].target != null) 
					{
						entities[i].target.OnTargetSkill(peEntity.skEntity);
					}
				}
			}
		}
	}
}

