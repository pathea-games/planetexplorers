using UnityEngine;
using Mono.Data.SqliteClient;
using System;
using System.Collections;
using System.Collections.Generic;

namespace SkillSystem
{
	public class SkEffect
	{
		internal int _id;
		internal int _seId;
		internal int _emitId;
		internal int _camEffId;
		internal int _procEffId;
		internal bool _repelEff;	// ikAnim/ragdoll effect id
		internal string[] _anms;
		internal int[] _effId;		// partical effect id

		internal static Dictionary<int, SkEffect> s_SkEffectTbl;
		public static void LoadData()
		{
			if(s_SkEffectTbl != null)		return;
			
			SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable("skEffect");
			//reader.Read(); // skip title if needed
			s_SkEffectTbl = new Dictionary<int, SkEffect>();
			while (reader.Read())
			{
				SkEffect skEffect = new SkEffect();
				skEffect._id = Convert.ToInt32(reader.GetString(reader.GetOrdinal("_id")));
				skEffect._anms = reader.GetString(reader.GetOrdinal("_anms")).Split(',');
				skEffect._seId = Convert.ToInt32(reader.GetString(reader.GetOrdinal("_seId")));
				skEffect._emitId = Convert.ToInt32(reader.GetString(reader.GetOrdinal("_emitId")));
				skEffect._repelEff = Convert.ToBoolean(reader.GetString(reader.GetOrdinal("_spEff")));
				skEffect._camEffId = Convert.ToInt32(reader.GetString(reader.GetOrdinal("_camEffId")));
				try{
					skEffect._procEffId = Convert.ToInt32(reader.GetString(reader.GetOrdinal("_procEff")));
				}catch{
				}
				{// EffId s
					string[] strEffIds = reader.GetString(reader.GetOrdinal("_effId")).Split(',');
					skEffect._effId = new int[strEffIds.Length];
					for(int i = 0; i < strEffIds.Length; i++)
					{
						skEffect._effId[i] = Convert.ToInt32(strEffIds[i]);
					}
				}

				s_SkEffectTbl.Add(skEffect._id, skEffect);
			}
		}
		private string PickAnim(SkEntity entity, SkRuntimeInfo info)
		{
			SkInst inst = info as SkInst;
			if (inst != null) {
				switch (_anms [0].ToLower ()) {
				case "rnd":
					return _anms [SkInst.s_rand.Next (1, _anms.Length)];
				case "inc":
					{
						int idx = inst.GuideCnt - 1;
						idx %= (_anms.Length - 1);
						return _anms [idx + 1];	//1+(GuideCnt-1)
					}
				case "dir":
					return _anms [1 + inst.GetAtkDir ()];
				}
			}
			return _anms[0];
		}
		public void Apply(SkEntity entity, SkRuntimeInfo info)
		{
			if(_seId > 0)				entity.ApplySe(_seId, info);
			if(_repelEff)				entity.ApplyRepelEff(info);
			if(_emitId > 0)				entity.ApplyEmission(_emitId, info);
			if(_anms.Length > 0) 		entity.ApplyAnim(PickAnim(entity, info), info);
			foreach(int eid in _effId){	if(eid > 0) entity.ApplyEff(eid, info);	}
			if(_camEffId > 0)			entity.ApplyCamEff(_camEffId, info);
			if (_procEffId > 0)			SkEffExFunc.Apply (_procEffId, entity, info);
		}
	}
}
