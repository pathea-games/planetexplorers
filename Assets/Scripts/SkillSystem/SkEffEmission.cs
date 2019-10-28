using UnityEngine;
using Mono.Data.SqliteClient;
using System;
using System.Collections;
using System.Collections.Generic;
using ItemAsset;

namespace SkillSystem
{
	public class SkEffEmission	// Copy/paste from old system, refactoring needed
	{
        internal string _path;
        internal string _bone;
        internal int _sound;
        internal int _effect;
        internal Vector3 _axis;

		//The id of the skill for cast this item
		//internal int _castSkillId;
		internal static SkEffEmission Create(SqliteDataReader reader)
		{
			string desc = reader.GetString(reader.GetOrdinal("_descEmission"));
			string[] strings = desc.Split(',');
			if (strings.Length < 10)
			{
				return null;
			}
			
			SkEffEmission ret = new SkEffEmission();
            ret._path = strings[0];
            ret._bone = strings[1];
            ret._sound = Convert.ToInt32(strings[2]);
            ret._effect = Convert.ToInt32(strings[3]);
            ret._axis = PETools.PEUtil.ToVector3(strings[4], '_');
			return ret;
		}
	}
}
