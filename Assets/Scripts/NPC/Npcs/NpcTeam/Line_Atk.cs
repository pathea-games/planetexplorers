using PETools;
using SkillSystem;
using Pathea.PeEntityExt;
using Pathea.PeEntityExtTrans;
using Mono.Data.SqliteClient;
using System.Collections.Generic;

namespace Pathea
{
	public  class AttackLine : TeamLine
	{
		private List<PeEntity> mAtkTargets;
		internal List<Cooperation> tempLists;

		public AttackLine():base()
		{
			mAtkTargets = new List<PeEntity>();
			tempLists = new List<Cooperation>();
		}
		
		public override ELineType Type {
			get {
				return ELineType.TeamAtk;
			}
		}

		public override int Priority {
			get {
				return (int)EPriority.Atk;
			}
		}
		
		#region Listen_Target
		void CooperationEnd(Cooperation self)
		{
			//RemoveOut(self.GetCooperMembers());
			self.DissolveCooper();
			mCooperationLists.Remove(self);
		}


		public void DissolveTheline(PeEntity target)
		{
			AtkCooperation atk = GetAtkCooperByTarget(target);
			if(target != null)
			{
				if(mAtkTargets.Contains(target))
					RemoveAtkTarget(target);
				
			}
			if(atk != null)
			{
				List<PeEntity> lists = atk.GetCooperMembers();
				RemoveOut(lists);
				mCooperationLists.Remove(atk);
				atk.DissolveCooper();
			}
		}

		public void  OnAtkTargetDeath(SkillSystem.SkEntity skSelf, SkillSystem.SkEntity skCaster)
		{
			PeEntity target = skSelf.GetComponent<PeEntity>();
			AtkCooperation atk = GetAtkCooperByTarget(target);
			if(target != null)
			{
				if(mAtkTargets.Contains(target))
					RemoveAtkTarget(target);

			}
			if(atk != null)
			{
				List<PeEntity> lists = atk.GetCooperMembers();
				RemoveOut(lists);
				mCooperationLists.Remove(atk);
				atk.DissolveCooper();
			}
		}
		
		public void OnAtkTargetDestroy(SkillSystem.SkEntity entity)
		{

			for(int i=0;i<mCooperationLists.Count;i++)
			{
				AtkCooperation atk = mCooperationLists[i] as AtkCooperation;
				if(atk != null)
					atk.OnAtkTargetDestroy(entity);
			}
			PeEntity target = entity.GetComponent<PeEntity>();
			if(target != null)
			{
				if(mAtkTargets.Contains(target))
					RemoveAtkTarget(target);
			}
		}

		public void OnAtkTargetLost(PeEntity entity)
		{
			for(int i=0;i<mCooperationLists.Count;i++)
			{
				AtkCooperation atk = mCooperationLists[i] as AtkCooperation;
				if(atk != null)
					atk.OnAtkTargetLost(entity);
			}
			if(entity != null)
			{
				if(mAtkTargets.Contains(entity))
					RemoveAtkTarget(entity);
			}
		}

		#endregion
		
		#region Atk_Target_fun


		public List<PeEntity> GetAtkMemberByTarget(PeEntity target)
		{
			AtkCooperation atk = GetAtkCooperByTarget(target);
			if(atk == null)
				return null;

			return atk.GetAtkCooperMembers();
		}

		public AtkCooperation GetAtkCooperByTarget(PeEntity target)
		{
			for(int i=0;i<mCooperationLists.Count;i++)
			{
				AtkCooperation atk = mCooperationLists[i] as AtkCooperation;
				if(atk != null && atk.HasBeTarget(target))
					return atk;
			}
			return null;
		}

        public AtkCooperation GetAtkCooperByMember(PeEntity member)
        {
            for (int i = 0; i < mCooperationLists.Count; i++)
            {
                AtkCooperation atk = mCooperationLists[i] as AtkCooperation;
                if (atk != null && atk.ContainMember(member))
                    return atk;
            }
            return null;
        }

		public void UpdateAtkTarget()
		{
			for(int i=0;i<mAtkTargets.Count;i++)
			{
				if(BeCooperationTarget(mAtkTargets[i]))
					continue ;
				else
					AddNewTargetAtkCooperation(mAtkTargets[i]);
			}

		}
		
		public  void AddAktTarget(PeEntity target)
		{
			mAtkTargets.Add(target);
		}
		
		public  bool RemoveAtkTarget(PeEntity target)
		{
			return mAtkTargets.Remove(target);
		}
		
		public void AddNewTargetAtkCooperation(PeEntity target)
		{
			if(target != null && target.monsterProtoDb != null)
			{
				int num;
				if(target.monsterProtoDb.AtkDb.mNumber != 0)
					num = target.monsterProtoDb.AtkDb.mNumber;
				else
					num = CSNpcTeam.CsNpcNumber;

				AtkCooperation atk = new AtkCooperation(num,1);
				atk.AddAktTarget(target);
				mCooperationLists.Add(atk);
			}
		}

        public AtkCooperation NewTargetAtkCooperation(PeEntity target)
        {
            if (target != null && target.monsterProtoDb != null)
            {
                int num;
                if (target.monsterProtoDb.AtkDb.mNumber != 0)
                    num = target.monsterProtoDb.AtkDb.mNumber;
                else
                    num = CSNpcTeam.CsNpcNumber;

                AtkCooperation atk = new AtkCooperation(num, 1);
                atk.AddAktTarget(target);
                mCooperationLists.Add(atk);
                return atk;
            }
            return null;
        }


        public bool ChangeCooperTarget(AtkCooperation cooper,PeEntity target)
        {
            if (cooper == null)
                return false;

            cooper.AddAktTarget(target);
            return true;

        }
		
		public bool BeCooperationTarget(PeEntity target)
		{
			for(int i=0;i<mCooperationLists.Count;i++)
			{
				AtkCooperation atk = mCooperationLists[i] as AtkCooperation;
				if(atk == null)
					continue;
				
				if(atk.HasBeTarget(target))
				{
					atk.SetAtkTarget(target);
					return true;
				}
			}
			
			for(int i=0;i<mCooperationLists.Count;i++)
			{
				AtkCooperation atk = mCooperationLists[i] as AtkCooperation;
				if(atk == null)
					continue;
				
				if(atk.CanBeTarget(target))
				{
					atk.AddAktTarget(target);
					return true;
				}
			}
			return false;
		}
		

		public void AddNewMemberAtkCooperation(PeEntity member)
		{
			AtkCooperation atk = new AtkCooperation(2,1);
			member.target.SetEnityCanAttack(true);
			//atk.onCooperationEnd += CooperationEnd;
			atk.AddMember(member);
			mCooperationLists.Add(atk);
		}
		

		bool IsInEnems(PeEntity member,PeEntity target)
		{
			if(member == null || target == null || member.target == null)
			  return false;

			List<Enemy> ens = member.target.GetEnemies();
			for(int i=0;i<ens.Count;i++)
			{
				if(ens[i].entityTarget == target)
					return true;
			}
			return false;
		}
		
		
		#endregion
		
		#region Override

		public override bool Go ()
		{
			if(mLinemembers == null)
				return false;

			for(int i=0;i<mLinemembers.Count;i++)
			{
				if(mLinemembers[i].NpcCmpt != null && mLinemembers[i].target != null)
				{
					mLinemembers[i].NpcCmpt.SetLineType(ELineType.TeamAtk);
					mLinemembers[i].target.SetEnityCanAttack(true);
					mLinemembers[i].target.SetCanAtiveWeapon(true);
				}
			}



			UpdateAtkTarget();
			return true;
		}
		
		public override bool AddIn (PeEntity member,params object[] objs)
		{		
			if(objs == null || objs.Length <=0)
			{
				return base.AddIn(member);
			}

			if(CanAddCooperMember(member,(PeEntity)objs[0]))
			{
				if(member.NpcCmpt != null)
				   member.NpcCmpt.SetLineType(ELineType.TeamAtk);

				if(member.target != null)
				   member.target.SetEnityCanAttack(true);
				return  base.AddIn(member);

			}
			
			return false;
		}

		public override bool RemoveOut (PeEntity member)
		{
			RemoveFromCooper(member);
            member.target.SetEnityCanAttack(true); //// used false
			member.NpcCmpt.BattleMgr.SetSelectEnemy(null);
			return base.RemoveOut(member);

		}

		public override void OnMsgLine (params object[] objs)
		{
			ELineMsg msg = (ELineMsg)objs[0];
			switch(msg)
			{
			case ELineMsg.ADD_Target:
			{
				PeEntity target = (PeEntity)objs[1];
				if(!mAtkTargets.Contains(target))
				{
					AddAktTarget(target);
					UpdateAtkTarget();
				}
			}
				break;
			case ELineMsg.REMOVE_Target:break;
			default:
				break;
			}
			
			return ;
		}

        //public override bool Suitline (PeEntity member,params object[] objs)
        //{
        //    if(objs == null)
        //        return false;

        //    PeEntity target = (PeEntity)objs[0];
        //    return IsInEnems(member,target);
        //}
		#endregion
	}
}


