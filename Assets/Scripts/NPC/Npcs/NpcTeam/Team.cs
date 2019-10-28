using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using ItemAsset;
using Pathea.Operate;
using PETools;
using SkillSystem;
using Pathea.PeEntityExt;
using Pathea.PeEntityExtTrans;
using Mono.Data.SqliteClient;

namespace Pathea
{
	public interface  Team 
	{
		List<PeEntity> GetTeamMembers();
		bool AddInTeam(List<PeEntity> members,bool Isclear = true);
		bool DissolveTeam();

	}
	
	public abstract class TeamAgent :Team
	{
		internal List<PeEntity> teamMembers;
		public virtual void InitTeam() {teamMembers = new List<PeEntity>();}

		public TeamAgent(){}
		public abstract List<PeEntity> GetTeamMembers();
		public abstract bool ReFlashTeam();

		public abstract bool ClearTeam();
		public abstract bool RemoveFromTeam(PeEntity members);
		public abstract bool ContainMember(PeEntity members);
		public abstract bool RemoveFromTeam(List<PeEntity> members);
		public abstract bool AddInTeam(List<PeEntity> members,bool Isclear = true);
		public abstract bool AddInTeam(PeEntity members);


		public abstract void OnAlertInform(PeEntity enemy);
		public abstract void OnClearAlert();
		public abstract bool DissolveTeam();

	}


}



