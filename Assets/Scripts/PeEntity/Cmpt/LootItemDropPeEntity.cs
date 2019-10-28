using UnityEngine;
using ItemAsset;
using Pathea;
using ItemAsset.PackageHelper;
using System.Collections;
using System.Collections.Generic;

public class LootItemDropPeEntity 
{
	static List<PeEntity> DeathLists = new List<PeEntity>();

	public static bool HasLootEntity()
	{
		return DeathLists !=null ? DeathLists.Count >0 : false;
	}
	public static void AddPeEntity(PeEntity entity)
	{
		if(DeathLists == null)
			DeathLists = new List<PeEntity>();

		if(entity.monster == null)
			return ;

		if(!DeathLists.Contains(entity))
			DeathLists.Add(entity);

		return;
	}

	public static bool RemovePeEntity(PeEntity entity)
	{
		return  DeathLists.Remove(entity);
	}

	public static bool ContainsPeEnity(PeEntity entity)
	{
		return DeathLists.Contains(entity);
	}

	public static List<PeEntity> GetEntities(Vector3 pos,float radiu)
	{
		List<PeEntity> tmpEntities = new List<PeEntity>();
		for(int i=0;i<DeathLists.Count;i++)
		{
			if(Match(DeathLists[i],pos,radiu))
				tmpEntities.Add(DeathLists[i]);
		}
		return tmpEntities;
	}

	public static PeEntity GetLootEntity(Vector3 pos,float radiu)
	{
		List<PeEntity> entities = GetEntities(pos,radiu);
		if(entities == null || entities.Count <=0)
			return null;

		PeEntity lootentity = entities[Random.Range(0,entities.Count)];
		//RemovePeEntity(lootentity);
		return lootentity;
	}

	private static bool Match(PeEntity entity, Vector3 pos, float radius)
	{
		if(entity != null)
			return PETools.PEUtil.SqrMagnitudeH(entity.position, pos) <= (radius + entity.maxRadius) * (radius + entity.maxRadius);
		else
			return false;
	}


}
