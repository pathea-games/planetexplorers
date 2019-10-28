using UnityEngine;
using Mono.Data.SqliteClient;
using System;
using System.Collections;
using System.Collections.Generic;

namespace SkillSystem
{
	public static class SkEffExFunc
	{
		internal static void Apply(int procEffId, SkEntity entity, SkRuntimeInfo info)
		{
			switch (procEffId) {
			case 1:
				DoEff1(entity, info);
				break;
			case 2:
				DoEff2(entity, info);
				break;
			default:
				break;
			}
		}

		static void DoEff1(SkEntity entity, SkRuntimeInfo info)
		{
			// Add code
			SkBuffInst buff = info as SkBuffInst;
			if(null != buff)
			{
				entity.OnBuffAdd(buff._buff._id);
			}
		}

		static void DoEff2(SkEntity entity, SkRuntimeInfo info)
		{
			// Add code
			SkBuffInst buff = info as SkBuffInst;
			if(null != buff)
			{
				entity.OnBuffRemove(buff._buff._id);				
			}
		}
	}
}
