using UnityEngine;
using Mono.Data.SqliteClient;
using System;
using System.Collections;
using System.Collections.Generic;

namespace SkillSystem
{
	public class SkTest : MonoBehaviour
	{
		void Awake()
		{
			SkData skDigTerrain = new SkData();
			skDigTerrain._condToLoop = SkCond.Create("lasthit");
			skDigTerrain._pretimeOfMain = new float[]{0.5f};
			skDigTerrain._postimeOfMain = new float[]{0.5f};
			skDigTerrain._events = new List<List<SkTriggerEvent>>();
			skDigTerrain._events.Add(new List<SkTriggerEvent>());
			SkTriggerEvent skDigEvent = new SkTriggerEvent();
			skDigEvent._cond = SkCond.Create("CondToDig");
			skDigEvent._modsTarget = SkAttribsModifier.Create("mad,0,1,-1");
			skDigTerrain._events[0].Add(skDigEvent);
			SkData.s_SkillTbl = new Dictionary<int, SkData>();
			SkData.s_SkillTbl.Add(0, skDigTerrain);
		}
		void Update()
		{
			if(Input.GetKeyDown(KeyCode.P) && Input.GetKey(KeyCode.Mouse0))
			{
				VFVoxelTerrain.self.StartSkill(VFVoxelTerrain.self, 0);
			}
		}
	}
}
