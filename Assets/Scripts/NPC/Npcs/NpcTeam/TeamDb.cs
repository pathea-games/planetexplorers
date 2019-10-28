using PETools;
using SkillSystem;
using Pathea.PeEntityExt;
using Pathea.PeEntityExtTrans;
using Mono.Data.SqliteClient;
using System.Collections.Generic;
using  UnityEngine;

namespace Pathea
{
	public class ChatTeamDb
	{
		public Vector3 TRMovePos;
		public Vector3 CenterPos;
		public void Set(object[] objs)
		{
			TRMovePos = (Vector3)objs[0];
			CenterPos = (Vector3)objs[1];
		}
	}

	public class TeamDb 
	{ 
		public static ChatTeamDb LoadchatTeamDb(PeEntity npc)
		{
			if(npc == null || npc.NpcCmpt == null || npc.NpcCmpt.TeamData == null)
				return null;

			ChatTeamDb chatTeamDb = new ChatTeamDb();
			chatTeamDb.Set(npc.NpcCmpt.TeamData);
			return chatTeamDb;

		}
	}
}

