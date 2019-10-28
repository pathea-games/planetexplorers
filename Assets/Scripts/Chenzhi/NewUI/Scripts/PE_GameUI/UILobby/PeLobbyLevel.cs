using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Mono.Data.SqliteClient;
using Pathea;
using System;

public class PeLobbyLevel
{
	public int level; 
	public int exp;
	public int nextExp;

    public class Mgr : PeSingleton<Mgr>, IPesingleton
	{
        void IPesingleton.Init()
        {
			LoadData();
		}

		List<PeLobbyLevel> data;

		public void LoadData()
		{
			SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable("lobbylevel");

			data = new List<PeLobbyLevel>();
			while (reader.Read())
			{
				PeLobbyLevel lobbyLevel = new PeLobbyLevel();
				lobbyLevel.level = Convert.ToInt32(reader.GetString(reader.GetOrdinal("level")));
				lobbyLevel.exp =  Convert.ToInt32(reader.GetString(reader.GetOrdinal("exp")));
				lobbyLevel.nextExp = Convert.ToInt32(reader.GetString(reader.GetOrdinal("next")));
				data.Add(lobbyLevel);
			}

			data.Sort(delegate(PeLobbyLevel x, PeLobbyLevel y) {
				if (x.level == y.level)
					return 0;
				else if (x.level > y.level)
					return 1;
				else
					return -1;
			});
		}

		PeLobbyLevel MaxLevel {get {return  data.Count > 0 ? data[data.Count - 1] : null;}}

		public PeLobbyLevel GetLevel(float exp)
		{
			if (exp <=0)
				return data.Count > 0 ? data[0] : null;
			if ( exp >= MaxLevel.exp)
				return MaxLevel;
			foreach (PeLobbyLevel lobbyLevel in data)
			{
				if (exp < lobbyLevel.exp)
					return lobbyLevel;
			}
			return null;
		}
	}

}
