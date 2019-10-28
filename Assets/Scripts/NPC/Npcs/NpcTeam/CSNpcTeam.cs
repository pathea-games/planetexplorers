using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Pathea.PeEntityExtNpcPackage;
using Pathea.PeEntityExt;
using Random = UnityEngine.Random;


namespace Pathea
{

    public class  stEnemies
    {
        public  PeEntity mTarget;
        List<PeEntity> mChaseLists;

        public int ChasesCout
        {
            get { return mChaseLists == null ? 0 : mChaseLists.Count; }
        }

        public stEnemies(PeEntity target)
        {
           mTarget = target;
           mChaseLists = new List<PeEntity>();
        }

        public void AddInChaseLists(PeEntity chase)
        {
            mChaseLists.Add(chase);
        }

         public bool RemoveChaseLists(PeEntity chase)
        {
            return mChaseLists.Remove(chase);
        }

        public bool BeTarget(PeEntity enemey)
        {
            return mTarget == null ? false : mTarget.Equals(enemey);
        }

        public bool InChaseLists(PeEntity entity)
        {
            return mChaseLists == null ? false : mChaseLists.Contains(entity);
        }

        public void ClearChaseLists()
        {
            mChaseLists.Clear();
        }
    }

	public class CSNpcTeam : TeamAgent
	{
		public  static   int EAT_Mumber_Num = 4;

		public static   int     Sleep_Mumber_Num = 5;
		public static   float   Sleep_Batch_Time = 10.0f;
		public static   float   Awake_Batch_Time = 10.0f;
		public static   float   Sleep_ALl_Time = 9.0f; 


		public static  int    Chat_member_Num = 5;
		public static  float  Chat_Radiu = 1.0f;

        public static float   Atk_JionR = 16.0f;
        public static float   Atk_RunR  = 32.0f;

		static int mCsNpcNumber;
		public static int CsNpcNumber {get{return mCsNpcNumber ;}}

		static NpcCheckTime mCheckTime;
		public static NpcCheckTime checkTime { get {return mCheckTime;}}

		static CSSleepSlots mCsSleepSlots;
		public static CSSleepSlots CsSleepSlots { get {return mCsSleepSlots;}}

		CSCreator mCScreator;

        public  bool mIndanger = false;
		bool mInit = false;

		//队伍列表
		TeamLine[] m_TeamLines;
		//队伍对应的成员列表
		List<PeEntity>[] m_beInLine;
		//基地NPC所在象限列表
		List<PeEntity>[]   m_CSQuadrantPos;
		//当前在队伍中的成员
		List<PeEntity> m_InLines;
		//当前空闲的基地成员(未参加战斗的)
		List<PeEntity> m_IdleLists;
		//基地面临的敌人列表
		List<PeEntity> m_TeamEnemies;
        //基地核心面敌
       // List<PeEntity>[] m_assemblyEnemies
        Dictionary<PeEntity, List<PeEntity>> m_assemblyEnemies;

        List<stEnemies> m_mapEnemies;

		List<PeEntity>[] m_DicteamEnemies;
	    List<PeEntity>[] m_DicEquipMembers;
		List<PeEntity>[] m_DicJobMembers;

		List<PeEntity>  m_EatQueues;
		Dictionary<int,CSStorage> m_mapCStorages;

		List<PeEntity> m_tempList;

		public CSNpcTeam() : base()
		{
			mInit = false;
			m_TeamLines = new TeamLine[(int)ELineType.Max];
			m_beInLine = new List<PeEntity>[(int)ELineType.Max];

			m_CSQuadrantPos = new List<PeEntity>[(int)EQuadrant.Q4 +1];
			m_CSQuadrantPos[(int)EQuadrant.Q1] = new List<PeEntity>();
			m_CSQuadrantPos[(int)EQuadrant.Q2] = new List<PeEntity>();
			m_CSQuadrantPos[(int)EQuadrant.Q3] = new List<PeEntity>();
			m_CSQuadrantPos[(int)EQuadrant.Q4] = new List<PeEntity>();

			m_IdleLists = new List<PeEntity>();
			m_InLines = new List<PeEntity>();

			m_TeamEnemies = new List<PeEntity>();
			m_tempList = new List<PeEntity>();

			m_DicteamEnemies = new List<PeEntity>[(int)AttackType.Ranged +1];
			m_DicteamEnemies[(int)AttackType.Melee] = new List<PeEntity>();
			m_DicteamEnemies[(int)AttackType.Ranged] = new List<PeEntity>();


			m_DicEquipMembers = new List<PeEntity>[(int)AttackType.Ranged +1];
			m_DicEquipMembers[(int)AttackType.Melee] = new List<PeEntity>();
			m_DicEquipMembers[(int)AttackType.Ranged] = new List<PeEntity>();

			m_DicJobMembers = new List<PeEntity>[(int)ENpcJob.Max];
			m_EatQueues = new List<PeEntity>();
			m_mapCStorages = new Dictionary<int, CSStorage>();

            m_assemblyEnemies = new Dictionary<PeEntity, List<PeEntity>>();
            m_mapEnemies = new List<stEnemies>();

		}

		public void  setCSCreator(CSCreator creator)
		{
			if(creator == null || creator.Assembly == null)
				return ;

			if(mCScreator == null ||  !mCScreator.Equals(creator))
			{
				mCScreator = creator;
				mInit = false; 
			}
			  
		}


        //map enemies 

        stEnemies GetByTarget(PeEntity target)
        {
            for (int i = 0; i < m_mapEnemies.Count; i++)
            {
                if (m_mapEnemies[i].BeTarget(target))
                    return m_mapEnemies[i];
            }
            return null;
        }

        stEnemies GetByChase(PeEntity chase)
        {
            for (int i = 0; i < m_mapEnemies.Count; i++)
            {
                if (m_mapEnemies[i].InChaseLists(chase))
                    return m_mapEnemies[i];
            }
            return null;
        }

        bool mapEnemiesContainTarget(PeEntity target)
        {
            for (int i = 0; i < m_mapEnemies.Count;i++ )
            {
                if (m_mapEnemies[i].BeTarget(target))
                    return true;
            }
            return false;
        }

        bool mapEnemiesContainChase(PeEntity chase)
        {
            for (int i = 0; i < m_mapEnemies.Count; i++)
            {
                if (m_mapEnemies[i].InChaseLists(chase))
                    return true;
            }
            return false;
        }

        bool mapEnemiesRemoveTaregt(PeEntity target)
        {
            stEnemies stenemy = GetByTarget(target);
            if (stenemy == null)
                return false;

            stenemy.ClearChaseLists();
            return  m_mapEnemies.Remove(stenemy);
        }

        void mapEnemiesAddTarget(PeEntity target)
        {
            if (mapEnemiesContainTarget(target))
                return;

            m_mapEnemies.Add(new stEnemies(target));
        }

        void mapEnemiesAddChase(PeEntity chase, params object[] objs)
        {
            if (objs == null)
                return;

            PeEntity target = (PeEntity)objs[0];
            if (target == null)
                return;

            stEnemies stenemy = GetByTarget(target);
            if (stenemy == null)
                return;

            if(!stenemy.InChaseLists(chase))
                stenemy.AddInChaseLists(chase);
        }

        void mapEnemiesRemoveChase(PeEntity chase)
        {
            stEnemies stenemy = GetByChase(chase);
            if (stenemy == null)
                return;

            stenemy.RemoveChaseLists(chase);
        }

        //assembly enemies
        void AddToAssemblyEnemies(PeEntity enemy)
        {
            int atkNum = 0;
            if (enemy.monsterProtoDb != null && enemy.monsterProtoDb.AtkDb != null)
                atkNum = enemy.monsterProtoDb.AtkDb.mNumber;
            else
                atkNum = 4;

            if (!m_assemblyEnemies.ContainsKey(enemy))
            {
                m_assemblyEnemies.Add(enemy, new List<PeEntity>(atkNum));
            }
                
        }


        bool assemblyEnemiesContains(PeEntity member)
        {
            foreach (PeEntity key in m_assemblyEnemies.Keys)
            {
                if (m_assemblyEnemies[key].Contains(member))
                    return true;
            }
            return false;
        }

        void AddToAssemblyProtectMember(PeEntity target,PeEntity member)
        {
            foreach (PeEntity key in m_assemblyEnemies.Keys)
            {
                if (m_assemblyEnemies[key].Contains(member))
                    m_assemblyEnemies[key].Remove(member);
            }
            m_assemblyEnemies[target].Add(member);
        }

        bool RemoveFromAssemblyEnemies(PeEntity enemy)
        {
            return m_assemblyEnemies.Remove(enemy);
        }


		bool BeinLineContainKey(ELineType type)
		{
			return m_beInLine[(int)type] != null;
		}

		void AddInBeinLine(ELineType type,List<PeEntity> lists)
		{
			m_beInLine[(int)type] = lists;
		}

		void removeBeinLine(ELineType type)
		{
			m_beInLine[(int)type].Clear();
			m_beInLine[(int)type] = null;
		}

		bool teamLinesContain(ELineType type)
		{
			return m_TeamLines[(int)type] != null;
		}

		void addInTeamLines(ELineType type,TeamLine line)
		{
			m_TeamLines[(int)type] = line;
		}

		void removeTeamLines(ELineType type)
		{
			m_TeamLines[(int)type] = null;
		}

		void clearTeamLine(ELineType type)
		{
			m_TeamLines[(int)type].ClearMembers();
		}

		//Lists fun
        //m_TeamEnemies
        #region TeamEnemies
        void AddToTeamEnemies(PeEntity enemy)
		{
			if(!m_TeamEnemies.Contains(enemy))
			{
				SwichAddDicteamEnemies(enemy);
				m_TeamEnemies.Add(enemy);
			}

		}

		void RemoveFormTeamEnemies(PeEntity enemy)
		{
			if(m_TeamEnemies.Contains(enemy))
			{
				RemoveFromDicteamEnemies(enemy);
				m_TeamEnemies.Remove(enemy);
			}

		}

        bool TeamEnemiesContain(PeEntity enemy)
        {
            return m_TeamEnemies.Contains(enemy);
        }
        #endregion
        //
		#region m_DicteamEnemies
		bool DicteamEnemiesContain(PeEntity enemy)
		{
			if(enemy == null || enemy.monsterProtoDb == null)
				return false;

			if(enemy.monsterProtoDb.AtkDb.mNeeedEqup)
			{
				if(m_DicteamEnemies[(int)AttackType.Ranged] == null)//!m_DicteamEnemies.ContainsKey(AttackType.Ranged)
					return false;
				
				return m_DicteamEnemies[(int)AttackType.Ranged].Contains(enemy);
			}
			return false;

		}
		void SwichAddDicteamEnemies(PeEntity enemy)
		{
			if(enemy == null || enemy.monsterProtoDb == null)
				return ;

			if(enemy.monsterProtoDb.AtkDb.mNeeedEqup)
			{
				AddToDicteamEnemies(AttackType.Ranged,enemy);
			}
			else
			{
				AddToDicteamEnemies(AttackType.Melee,enemy);
			}
			
		}

		void AddToDicteamEnemies(AttackType Eatk,PeEntity enemy)
		{
			if(m_DicteamEnemies[(int)Eatk] == null)//!m_DicteamEnemies.ContainsKey(Eatk))
				m_DicteamEnemies[(int)Eatk] = new List<PeEntity>();

			m_DicteamEnemies[(int)Eatk].Add(enemy);
		}

		void RemoveFromDicteamEnemies(PeEntity enemy)
		{
			for(int i=0;i<m_DicteamEnemies.Length;i++)
			{
				if(m_DicteamEnemies[i] != null && m_DicteamEnemies[i].Contains(enemy))
					m_DicteamEnemies[i].Remove(enemy);
			}
		}
		#endregion

		//m_DicJobMembers
		#region DicJobMembers
		void ClearDicJobMembers()
		{
			for(int i=0;i<m_DicJobMembers.Length;i++)
			{
				if(m_DicJobMembers[i] != null)
					m_DicJobMembers[i].Clear();
			}
		}

		void AddToDicJobMembers(PeEntity member)
		{
			if(member.NpcCmpt == null || member.NpcCmpt.Job == ENpcJob.None)
				return ;

			if(m_DicJobMembers[(int)member.NpcCmpt.Job] == null)//!m_DicJobMembers.ContainsKey(member.NpcCmpt.Job))
				m_DicJobMembers[(int)member.NpcCmpt.Job] = new List<PeEntity>();

			m_DicJobMembers[(int)member.NpcCmpt.Job].Add(member);
		}
		#endregion

		//CSQuadrantPos
		#region CSQuadrantPos

		void ClearCSQuadrantPos()
		{

			for(int i=(int)EQuadrant.Q1;i<m_CSQuadrantPos.Length;i++)
			{
				if(m_CSQuadrantPos[i] != null)
					m_CSQuadrantPos[i].Clear();
			}

		}

        void AddToQuadrantPosList(PeEntity member,Vector3 _centerPos)
        {
            EQuadrant q = Quadrant.GetQuadrant(member.position - _centerPos);
            m_CSQuadrantPos[(int)q].Add(member);
        }

		EQuadrant GetQuadrant(PeEntity entity)
		{
			for(int i=(int)EQuadrant.Q1;i<m_CSQuadrantPos.Length;i++)
			{
				if(m_CSQuadrantPos[i] != null && m_CSQuadrantPos[i].Contains(entity))
					return (EQuadrant)i;
			}
			return EQuadrant.None;
		}

		#endregion

        //EatQueue
        #region EatQueue
        void ClearEatQueue()
		{
			m_EatQueues.Clear();
			m_mapCStorages.Clear();
		}

		void AddToEatQueue(PeEntity npc)
		{
			if(m_EatQueues.Contains(npc))
				return ;

            bool inEattime = CSNpcTeam.checkTime.IsEatTimeSlot((float)GameTime.Timer.HourInDay);
            if (!inEattime)
                return;

            if (NpcEatDb.IsNeedEatsth(npc))
                m_EatQueues.Add(npc);
		}

		void RemoveFormEatQueue(PeEntity npc)
		{
			if(!m_EatQueues.Contains(npc))
				return ;

			RemoveFromLine(ELineType.TeamEat,npc);
			m_mapCStorages.Remove(npc.Id);
			m_EatQueues.Remove(npc);

		}

		bool IsContinueEat(PeEntity npc)
		{
            bool inEattime = CSNpcTeam.checkTime.IsEatTimeSlot((float)GameTime.Timer.HourInDay);
            if (!inEattime)
                return false;

			if (!NpcThinkDb.CanDo(npc, EThinkingType.Dining))
				return false;

            bool continueEat = NpcEatDb.IsNeedEatsth(npc);
            return continueEat;
			
		}
        #endregion

        //m_DicEquipMembers
        #region
        void ClearDicEquip()
        {
			for(int i=0;i<m_DicEquipMembers.Length;i++)
			{
				if(m_DicEquipMembers[i] != null)
				   m_DicEquipMembers[i].Clear();
			}
        }

        void AddToDicEquip(PeEntity member)
        {
            if (ItemAsset.SelectItem.HasCanAttackEquip(member, AttackType.Ranged))
            {
                member.NpcCmpt.SetAtkType(AttackType.Ranged);
                m_DicEquipMembers[(int)AttackType.Ranged].Add(member);
            }
            else
            {
                member.NpcCmpt.SetAtkType(AttackType.Melee);
                m_DicEquipMembers[(int)AttackType.Melee].Add(member);
            }
        }
        #endregion

        #region Line Fun
        void ReflashDicLines()
		{
			for(int i=0;i<m_TeamLines.Length;i++)
			{
				if(m_TeamLines[i] != null)
				   m_TeamLines[i].Go();
			}
		}

		void ReflashAtkTarget()
		{
			if(mCScreator != null && mCScreator.Assembly != null && m_TeamEnemies.Count >0)
			{
				m_tempList.Clear();
				for(int i=0;i<m_TeamEnemies.Count;i++)
				{
					float r = PETools.PEUtil.MagnitudeH(mCScreator.Assembly.Position,m_TeamEnemies[i].position);
					if(r >= mCScreator.Assembly.Radius * 1.5f )
					{
                        //超出范围
						m_tempList.Add(m_TeamEnemies[i]);
					}
				}

				for(int i=0;i<m_tempList.Count;i++)
				{
					//if(m_tempList[i] == null || m_tempList[i].Equals(null))
					m_TeamEnemies.Remove(m_tempList[i]);
					OnTargetLost(m_tempList[i]);
				}
			
			}
			if(m_TeamEnemies.Count <=0)
			{
				if(teamLinesContain(ELineType.TeamAtk))
				{
					clearTeamLine(ELineType.TeamAtk);
					removeTeamLines(ELineType.TeamAtk);
				}

//				if(m_DicLines.ContainsKey(ELineType.TeamAtk))
//				{
//					m_DicLines[ELineType.TeamAtk].ClearMembers();
//					m_DicLines.Remove(ELineType.TeamAtk);
//				}

				if(BeinLineContainKey(ELineType.TeamAtk))//m_beInLine.ContainsKey(ELineType.TeamAtk))
				{
//					m_beInLine[(int)ELineType.TeamAtk].Clear();
//					m_beInLine[(int)ELineType.TeamAtk] = null;
					removeBeinLine(ELineType.TeamAtk);
				}
                mIndanger = false;
			}
			
		}

        void ReflashLists()
        {
            if (mCScreator != null && mCScreator.Assembly != null)
            {
                //teamMembers
                ClearCSQuadrantPos();
                ClearDicEquip();
                for (int i = 0; i < teamMembers.Count; i++)
                {
                    //更新NPC位置象限
                    AddToQuadrantPosList(teamMembers[i], mCScreator.Assembly.Position);

                    //更新NPC装备列表
                    AddToDicEquip(teamMembers[i]);
                }

                //m_IdleLists
                ClearDicJobMembers();
                for (int i = 0; i < m_IdleLists.Count; i++)
                {
                    //更新NPC工作列表
                    AddToDicJobMembers(m_IdleLists[i]);
                    //更新NPCeat队列
                    AddToEatQueue(m_IdleLists[i]);
                }
            }
        }

		void ReflashEatState()
		{
			m_tempList.Clear();
			for(int i=0;i<m_EatQueues.Count;i++)
			{
				if(!IsContinueEat(m_EatQueues[i]))
					m_tempList.Add(m_EatQueues[i]);	
			}

			for(int i=0;i<m_tempList.Count;i++)
			{
				RemoveFormEatQueue(m_tempList[i]);
			}
		}

		private  bool ComPearPriority(TeamLine _newline,TeamLine _oldline)
		{
			return _newline.Priority < _oldline.Priority;
		}

		private void AddinLine(ELineType type,PeEntity member,params object[] objs)
		{
			if(!BeinLineContainKey(type))
				AddInBeinLine(type,new List<PeEntity>());

			//根据队列优先级确定该成员
			ELineType oldType = member.NpcCmpt.lineType;
			bool inOldLine = teamLinesContain(oldType) && m_TeamLines[(int)oldType].ContainEntity(member);//m_DicLines.ContainsKey(oldType) && m_DicLines[oldType].ContainEntity(member);
			bool canaddNew = inOldLine ? ComPearPriority(m_TeamLines[(int)type],m_TeamLines[(int)oldType]) : true;

			if(canaddNew && m_TeamLines[(int)type].AddIn(member,objs))
			{
                if (type == ELineType.TeamAtk)
                    mapEnemiesAddChase(member, objs);

				if(!m_beInLine[(int)type].Contains(member))
				   m_beInLine[(int)type].Add(member);

				if(m_IdleLists.Contains(member))
				   m_IdleLists.Remove(member);

				if(!m_InLines.Contains(member))
				   m_InLines.Add(member);
			}

		}

		private void RemoveFromLine(ELineType type,PeEntity member)
		{
            if (type == ELineType.TeamAtk)
                mapEnemiesRemoveChase(member);

			if(!m_IdleLists.Contains(member))
			   m_IdleLists.Add(member);

			if(m_InLines.Contains(member))
			   m_InLines.Remove(member);

			if(BeinLineContainKey(type) && m_beInLine[(int)type].Contains(member))
			    m_beInLine[(int)type].Remove(member);

			if(teamLinesContain(type) && m_TeamLines[(int)type].ContainEntity(member))
				m_TeamLines[(int)type].RemoveOut(member);

			if(teamLinesContain(type) && m_TeamLines[(int)type].Linemembers.Count == 0)
				removeTeamLines(type);
		}

        private void RemoveFromLines(List<PeEntity> lists)
        {
            for (int i = 0; i < lists.Count; i++)
            {
                m_InLines.Remove(lists[i]);
            }

        }

		private bool CanAddInLine(ELineType type,PeEntity member,params object[] objs)
		{
			return !member.IsDeath() && BeinLineContainKey(type) && !m_beInLine[(int)type].Contains(member) && needAddInMember(type) && suitLine(type,member,objs);              
		}

		private bool needAddInMember(ELineType type)
		{
			return teamLinesContain(type) && m_TeamLines[(int)type].needAddMemeber(); 
		}

		private bool suitLine(ELineType type,PeEntity member,params object[] objs)
		{
			return m_TeamLines[(int)type].Suitline(member,objs);
		}

		private bool ContainInLine(PeEntity member,ELineType type)
		{
			return teamLinesContain(type) && m_TeamLines[(int)type].ContainEntity(member);
		}

		private AttackLine NewAtkLine()
		{
			ELineType _type = ELineType.TeamAtk;
			AttackLine atk = new AttackLine();
			addInTeamLines(_type,atk);

			if(!BeinLineContainKey(_type))
				AddInBeinLine(_type,new List<PeEntity>());

			return atk;
		}

		private void NewChatLine()
		{
			ELineType _type = ELineType.TeamChat;
			chatLine chat = new chatLine(4);
			addInTeamLines(_type,chat);
			CreatChatCentor(m_DicJobMembers[(int)ENpcJob.Resident]);
			if(!BeinLineContainKey(_type))
				AddInBeinLine(_type,new List<PeEntity>());
		}

		private void NewSleepLine()
		{
			ELineType _type = ELineType.TeamSleep;
			SleepLine sleep = new SleepLine();
			addInTeamLines(_type,sleep);
			if(!BeinLineContainKey(_type))
				AddInBeinLine(_type,new List<PeEntity>());
		}

		private void NewEatLine()
		{
			ELineType _type = ELineType.TeamEat;
			EatLine eat = new EatLine();
			addInTeamLines(_type,eat);		
			if(!BeinLineContainKey(_type))
				AddInBeinLine(_type,new List<PeEntity>());

			MsgLine(ELineType.TeamEat,ELineMsg.Add_Eat,CSNpcTeam.EAT_Mumber_Num);
		}

		//在对应列表里找到合适的NPC
		private bool Find(ELineType type,List<PeEntity> findlists,params object[] objs)
		{
			for(int i=0;i<findlists.Count;i++)
			{
				if(CanAddInLine(type,findlists[i],objs))
				{
					AddinLine(type,findlists[i],objs);
				}
				
				if(!needAddInMember (type))
					return true ;
			}
			return false;
		}

		//按照象限分配NPC
		private void DistributeByQuadrant(PeEntity target,int _needNum)
		{
			ELineType _type = ELineType.TeamAtk;
			EQuadrant q0 = Quadrant.GetQuadrant(target.position - mCScreator.Assembly.Position);
			EQuadrant q1 = Quadrant.Add(q0);
			EQuadrant q2 = Quadrant.Minus(q0);
			EQuadrant q3 = Quadrant.Add(q1);;

			if(target.target != null)
			{
				Enemy tgtEnemy =  target.target.GetAttackEnemy();
				if(tgtEnemy != null && tgtEnemy.entityTarget != null && teamMembers != null && teamMembers.Contains(tgtEnemy.entityTarget))
				{
					if(CanAddInLine(_type,tgtEnemy.entityTarget,target))
					{
						AddinLine(_type,tgtEnemy.entityTarget,target);
					}
				}
			}

			List<PeEntity> lists = m_CSQuadrantPos[(int)q0];
			//在当前象限找队员
			if(!Find(_type,lists,target))
			{
				lists = m_CSQuadrantPos[(int)q1];
				if(!Find(_type,lists,target))
				{
					lists = m_CSQuadrantPos[(int)q2];
					if(!Find(_type,lists,target))
					{
						
						lists = m_CSQuadrantPos[(int)q3];
						Find(_type,lists,target);
					}
				}
			}
		}

		//按照武器分配NPC
		private void DistributeByEquip(PeEntity target,int _needNum)
		{
			ELineType _type = ELineType.TeamAtk;
			List<PeEntity> lists = m_DicEquipMembers[(int)AttackType.Ranged];
			Find(_type,lists,target);
		}

		//根据敌人分配战队
		private void DistributeAtkLine()
		{
			if(mCScreator == null || mCScreator.Assembly == null)
				return ;

			if(m_TeamEnemies.Count >0 && !teamLinesContain(ELineType.TeamAtk))//m_DicLines.ContainsKey(ELineType.TeamAtk))
				NewAtkLine();

			//給战斗队伍分配目标
			if(teamLinesContain(ELineType.TeamAtk))
			{
				for(int i=0;i<m_TeamEnemies.Count;i++)
				   MsgLine(ELineType.TeamAtk,ELineMsg.ADD_Target,m_TeamEnemies[i]);
			}


			//建立战斗队伍
 			bool addNew = needAddInMember (ELineType.TeamAtk);
			int needNum = 0;
			if(teamLinesContain(ELineType.TeamAtk))
				needNum= m_TeamLines[(int)ELineType.TeamAtk].GetNeedMemNumber();

			bool bdEquip = false;
			if(addNew)
			{
				for(int i=0;i<m_TeamEnemies.Count;i++)
				{
					if(DicteamEnemiesContain(m_TeamEnemies[i]))
						bdEquip = true;
				}

				if(bdEquip)
				{
					List<PeEntity> temlist0 = m_DicteamEnemies[(int)AttackType.Ranged];
					//List<PeEntity> temlist1 = m_DicteamEnemies[(int)AttackType.Melee];

					for(int i=0;i<temlist0.Count;i++)
					{
						DistributeByEquip(temlist0[i],needNum);
					}

					for(int i=0;i<m_TeamEnemies.Count;i++)
					{
						if(!DicteamEnemiesContain(m_TeamEnemies[i]))
						   DistributeByQuadrant(m_TeamEnemies[i],needNum);
					}
				}
				else
				{
					for(int i=0;i<m_TeamEnemies.Count;i++)				
						DistributeByQuadrant(m_TeamEnemies[i],needNum);
				}
			}

			DistributeAtkBench();
		}

        private void DistributeAtkBench()
        {
			int idleCnt = m_IdleLists.Count;
            for (int i = 0; i < m_TeamEnemies.Count;i++ )
            {
				for (int j = 0; j < idleCnt; j++)
                {
					if(m_IdleLists[j] == null || m_IdleLists[j].Equals(null) || m_IdleLists[j].IsDead() || m_IdleLists[j].isRagdoll)
						continue;

					stEnemies byChaser = GetByChase(m_IdleLists[j]);
					if(byChaser != null)
					{
						m_IdleLists[j].target.SetEnityCanAttack(true);
						m_IdleLists[j].NpcCmpt.BattleMgr.ChoiceTheEnmey(m_IdleLists[j],byChaser.mTarget);
						continue;
					}

                    stEnemies chases = GetByTarget(m_TeamEnemies[i]);
					bool canChase = chases != null && m_TeamEnemies[i].monsterProtoDb != null ? chases.ChasesCout < m_TeamEnemies[i].monsterProtoDb.AtkDb.mChaseNumber : true;
					bool EquipMatch = ItemAsset.SelectItem.MatchEnemyEquip(m_IdleLists[j], m_TeamEnemies[i]);
					bool InJoinR =  EquipMatch && PETools.PEUtil.InRange(m_TeamEnemies[i].position, m_IdleLists[j].position, Atk_JionR);
                    bool InRunR = PETools.PEUtil.InRange(m_TeamEnemies[i].position, m_IdleLists[j].position, Atk_RunR);

					if (!canChase || !EquipMatch)
                    {
                        if (InRunR)
                        { 
                        //npc need run
							m_IdleLists[j].NpcCmpt.SetCanIdle(false);
                        }
                        else
                        {
                            m_IdleLists[j].NpcCmpt.SetCanIdle(true);
                        }

                        continue;
                    }

                    if (InJoinR)
                    {
						//AddinLine(ELineType.TeamAtk,m_IdleLists[j],m_TeamEnemies[i]);
						m_IdleLists[j].target.SetEnityCanAttack(true);
						m_IdleLists[j].NpcCmpt.BattleMgr.ChoiceTheEnmey(m_IdleLists[j],m_TeamEnemies[i]);
						mapEnemiesAddChase(m_IdleLists[j],m_TeamEnemies[i]);
                        continue;
                    }

                    if (InRunR)
                    {
                        //npc need run
						m_IdleLists[j].NpcCmpt.SetCanIdle(false);
                    }
                    else
                    {
						m_IdleLists[j].NpcCmpt.SetCanIdle(true);
                    }
                }
            }

           
        }

        private void DistributeAsseblyProtect(PeEntity target)
        {
            if (m_assemblyEnemies == null || !m_assemblyEnemies.ContainsKey(target))
                return;

            if (teamMembers == null)
                return;

            if (target.monsterProtoDb == null || target.monsterProtoDb.AtkDb == null)
                return;

            if (m_assemblyEnemies[target].Count >= target.monsterProtoDb.AtkDb.mNumber)
                return ;

			if (m_TeamLines == null || !teamLinesContain(ELineType.TeamAtk))
                return;

			AttackLine atk = m_TeamLines[(int)ELineType.TeamAtk] as AttackLine;
            if (atk == null)
                return;

            AtkCooperation atkCooper = atk.GetAtkCooperByTarget(target);
			if(atkCooper == null)
				atkCooper = atk.NewTargetAtkCooperation(target);

			if (atkCooper != null && !atkCooper.IsneedMember())
				return;


            for (int i = 0; i < teamMembers.Count; i++)
            {
                if (m_assemblyEnemies[target].Count >= target.monsterProtoDb.AtkDb.mNumber)
                    return;

                if (!ItemAsset.SelectItem.MatchEnemyEquip(teamMembers[i], target))
                    continue;

                if (assemblyEnemiesContains(teamMembers[i]))            
                    continue;
                
                if(atkCooper.ContainMember(teamMembers[i]))
                {
                    AddToAssemblyProtectMember(target, teamMembers[i]);
                    continue;
                }

                AtkCooperation _cooper = atk.GetAtkCooperByMember(teamMembers[i]);
                if (_cooper != null)              				
					RemoveFromLine(ELineType.TeamAtk,teamMembers[i]);

				if(!teamLinesContain(ELineType.TeamAtk))
				{
					AttackLine Atk = NewAtkLine();
					Atk.AddAktTarget(target);
					Atk.NewTargetAtkCooperation(target);
				}
					

				AddinLine(ELineType.TeamAtk,teamMembers[i],target);
                AddToAssemblyProtectMember(target, teamMembers[i]);

            }
        }

        //休闲分配
        private void DistributeChatLine()
        {
            if (mCScreator == null || mCScreator.Assembly == null || m_DicJobMembers[(int)ENpcJob.Resident] != null)
            {
                ELineType _type = ELineType.TeamChat;
                bool intime = CSNpcTeam.checkTime.IsChatTimeSlot((float)GameTime.Timer.HourInDay);
				bool hasCreate = teamLinesContain(_type);

                if (intime && !mIndanger)
                {
                    bool needAdd = needAddInMember(_type);
                    bool createNew = intime && !hasCreate;
                    if (createNew) NewChatLine();
                    if (needAdd)
                    {
                        List<PeEntity> restLists = m_DicJobMembers[(int)ENpcJob.Resident];
                        Find(_type, restLists, null);
                    }
                }
                else
                {
                    if (hasCreate)
                    {
                        m_tempList.Clear();
						m_tempList.AddRange(m_TeamLines[(int)_type].Linemembers);
                        for (int i = 0; i < m_tempList.Count; i++)
                        {
                            RemoveFromLine(_type, m_tempList[i]);
                        }
                        MsgLine(ELineType.TeamChat, ELineMsg.Clear_chat);
						if (teamLinesContain(_type))
                            removeTeamLines(_type);
                    }

                }

            }

        }

		private void CreatChatCentor(List<PeEntity > lists)
		{
			int maxNum = 1 + (Mathf.Abs(lists.Count - 10)/10);
			for(int index=0;index<lists.Count;index++)
			{
				chatLine chat  = m_TeamLines[(int)ELineType.TeamChat] as chatLine;
				if(chat != null && chat.CenterNum < maxNum)
				{
					EQuadrant eq = GetQuadrant(lists[index]);
					if(suitLine(ELineType.TeamChat,lists[index],eq))
						MsgLine(ELineType.TeamChat,ELineMsg.Add_chatCentor,eq,lists[index]);
					
					if(CanAddInLine(ELineType.TeamChat,lists[index],eq))
					{
						AddinLine(ELineType.TeamChat,lists[index],null);
					}
				}

			}
		}

		//sleep分配
		Double starttime;
		Double  getUpstartTime = 0;
		private void DistributeSleepLine()
		{
			if(mCScreator != null || mCScreator.Assembly != null )
			{
				ELineType _type = ELineType.TeamSleep;

                if (mIndanger)
                {
					if (teamLinesContain(_type))
                    {
						SleepLine sleep = m_TeamLines[(int)_type] as SleepLine;
                        if (sleep != null && sleep.Linemembers != null && sleep.Linemembers.Count > 0)
                        {
                            m_tempList.Clear();
                            m_tempList.AddRange(sleep.Linemembers);
                            for (int i = 0; i < m_tempList.Count; i++)
                            {
                                RemoveFromLine(_type, m_tempList[i]);
                            }
                        }

                    }
                    return;
                }
				bool start = CSNpcTeam.CsSleepSlots.startSlots.InSlot((float)GameTime.Timer.HourInDay);
				bool end = CSNpcTeam.CsSleepSlots.endSlots.InSlot((float)GameTime.Timer.HourInDay);
				if(start)
				{
					bool hascreate = teamLinesContain(_type);
					if(!hascreate)
					{
						starttime = GameTime.Timer.Minute;
						NewSleepLine();
					}
					if(GameTime.Timer.Minute - starttime > CSNpcTeam.Sleep_Batch_Time)
					{
						if(m_TeamLines[(int)_type].Linemembers.Count < m_IdleLists.Count)
							MsgLine(_type,ELineMsg.Add_Sleep,CSNpcTeam.Sleep_Mumber_Num,GameTime.Timer.Hour);
						else
							return ;

						starttime = GameTime.Timer.Minute;
						List<PeEntity> lists = m_IdleLists;
						Find(_type,lists);
					}
				}
		
				if(end)
				{
					if(getUpstartTime == 0)
						getUpstartTime = GameTime.Timer.Minute;

					if(GameTime.Timer.Minute - getUpstartTime > CSNpcTeam.Awake_Batch_Time)
					{
						if(teamLinesContain(_type))
						{
							SleepLine sleep = m_TeamLines[(int)_type] as SleepLine;
							if(sleep != null)
							{
								Cooperation cooper = sleep.GetUpCooper();
								if(cooper != null)
								{
									m_tempList.Clear();
									m_tempList.AddRange(cooper.GetCooperMembers());
									for(int i=0;i<m_tempList.Count;i++)
									{
										RemoveFromLine(_type,m_tempList[i]);
									}							
									
								}
							}

						}
						getUpstartTime = GameTime.Timer.Minute;
					}
				}

				if(!end && !start)
				{
					if(teamLinesContain(_type))
					{
						SleepLine sleep = m_TeamLines[(int)_type] as SleepLine;
						if(sleep != null && sleep.Linemembers != null && sleep.Linemembers.Count >0)
						{
							m_tempList.Clear();
							m_tempList.AddRange(sleep.Linemembers);
							for(int i=0;i<m_tempList.Count;i++)
							{
								RemoveFromLine(_type,m_tempList[i]);
							}							
						}
						
					}
				}

			}
		}
		//eat分配
		private void DistributeEatLine()
		{
			if(mCScreator != null || mCScreator.Assembly != null)
			{
				ELineType _type = ELineType.TeamEat;
				bool hascreate = teamLinesContain(_type);
				bool intime = CSNpcTeam.checkTime.IsEatTimeSlot((float)GameTime.Timer.HourInDay);
				if(intime && !mIndanger)
				{
					if(!hascreate)
						NewEatLine();

					if(m_TeamLines[(int)_type].Linemembers.Count <CSNpcTeam.EAT_Mumber_Num)
					{
						List<PeEntity> lists = m_EatQueues;
						Find(_type,lists);
					}

				}
			}
		}
        #endregion


        #region Override
        public override void InitTeam ()
		{
			if(!mInit)
			{		
				if(mCScreator != null && mCScreator.Assembly != null)
				{
					base.InitTeam ();
					PeEventGlobal.Instance.DeathEvent.AddListener(OnEntityDeath);
					PeEventGlobal.Instance.DestroyEvent.AddListener(OnEntityDestroy);

					if(m_TeamLines == null)
						m_TeamLines = new TeamLine[(int)ELineType.Max];

					mCScreator.Assembly.AddEventListener(OnEntityEventListener);
					InitCheckTime();
					mInit = true;
				}
			}

		}

		void InitCheckTime()
		{
			mCheckTime = new NpcCheckTime();
			mCheckTime.AddSlots(EScheduleType.Chat);
			mCheckTime.AddSlots(EScheduleType.Eat);

			NPCScheduleData.Item item = NPCScheduleData.Get((int)EScheduleType.Sleep);
			mCsSleepSlots = new CSSleepSlots(item.Slots[0],item.Slots[1]);
		}

		public override List<PeEntity> GetTeamMembers ()
		{
			return teamMembers;
		}

		public override bool ContainMember (PeEntity members)
		{
			return teamMembers.Contains(members);
		}

		public override bool RemoveFromTeam (PeEntity member)
		{
			if(teamMembers.Contains(member))
			{
				member.target.SetEnityCanAttack(true);
				return  teamMembers.Remove(member);
			}
			return false;

		}

		public override bool RemoveFromTeam (List<PeEntity> members)
		{
			for(int i=0;i<members.Count;i++)
			{
				RemoveFromTeam(members[i]);
			}
			return true;
		}

        public override bool AddInTeam (List<PeEntity> members,bool Isclear = true)
		{
			if(teamMembers == null)
				return false;

			if(Isclear)
				ClearTeam();

			for(int i=0;i<members.Count;i++)
			{
				AddInTeam(members[i]);
			}
			return true;
		}

		public override bool AddInTeam (PeEntity member)
		{
			if(teamMembers == null || member.NpcCmpt == null || member.NpcCmpt.Creater == null || member.NpcCmpt.IsFollower)
				return false;

            member.target.SetEnityCanAttack(true);// used false
			member.NpcCmpt.BattleMgr.SetSelectEnemy(null);
			teamMembers.Add(member);
			return true;
		}


        /*************************************
       * 清楚team成员
       * ********************************/
		public override bool ClearTeam ()
		{
			for(int i=0;i<teamMembers.Count;i++)
			{
				teamMembers[i].target.SetEnityCanAttack(true);
			}
			teamMembers.Clear();
			return true;
		}
        /*************************************
       * 解散team
       * ********************************/
		public override bool DissolveTeam ()
		{
			ClearTeam ();
			m_IdleLists.Clear();
			m_InLines.Clear();
			return true;
		}
	

        /*************************************
       * 更新基地基本数据
       * ********************************/
		public override bool ReFlashTeam ()
		{
			if(mCScreator != null && mCScreator.Assembly != null)
			{
                mCsNpcNumber = teamMembers.Count;
				m_IdleLists.Clear();
         
                //更新攻击目标（是否有效无效则移除）
				ReflashAtkTarget();

				for(int i=0;i<teamMembers.Count;i++)
				{
					if(!ContainInLine(teamMembers[i],ELineType.TeamAtk))
                    {
                        teamMembers[i].NpcCmpt.SetCanIdle(true);
                        m_IdleLists.Add(teamMembers[i]);
                    }
						
				}

                ReflashLists();
				////AtkLine
				//DistributeAtkLine();
                //更新NPC的吃饭状态
                ReflashEatState();
				//chatLine
				DistributeChatLine();
                //eatline
				DistributeEatLine();
                //sleepline
				DistributeSleepLine();
				//lines
				ReflashDicLines();
				return true;
			}
 			
			return false;
		}

		#endregion

		#region Event_response

        /*************************************
        * 基地损坏通知
        * ********************************/
		void OnEntityEventListener(int event_id, CSEntity entity, object arg)
		{
			if(event_id == CSConst.eetDestroy)
			{
				DissolveTeam ();
				if(mCScreator != null && mCScreator.Assembly != null)
				{
					mCScreator.Assembly.RemoveEventListener(OnEntityEventListener);
					mCScreator = null;
				}

			}
		}

        /*****************************************
         * 基地怪物入侵
         * *************************************/
		public override void OnAlertInform (PeEntity enemy)
		{
           
			//记录敌人
            if (TeamEnemiesContain(enemy))
                return;

			AddToTeamEnemies(enemy);
            mapEnemiesAddTarget(enemy);
			//DistributeAtkLine();
			//基地战斗
			if(teamLinesContain(ELineType.TeamAtk))
				m_TeamLines[(int)ELineType.TeamAtk].Go();

            mIndanger = true;
			return ;
		}

        /*****************************************
        * 威胁解除
        * 
        * *************************************/
		public override void OnClearAlert ()
		{
			if(m_beInLine[(int)ELineType.TeamAtk].Count <= 0)
				return ;

			m_TeamLines[(int)ELineType.TeamAtk].ClearMembers();
			m_beInLine[(int)ELineType.TeamAtk].Clear();

			return ;
		}

        /*****************************************
        * 目标死亡通知
        * 
        * *************************************/
		void OnEntityDeath(SkillSystem.SkEntity skSelf, SkillSystem.SkEntity skCaster)
		{
			PeEntity peSlef = skSelf.GetComponent<PeEntity>();
			if(peSlef == null)
				return ;

			OnTargetLost(peSlef);
			OnTeammberDeath(peSlef);
		}

        /*****************************************
        * 目标删除通知
        * 
        * *************************************/
		void OnEntityDestroy(SkillSystem.SkEntity entity)
		{
			PeEntity peSlef = entity.GetComponent<PeEntity>();
			if(peSlef == null)
				return ;

			OnTargetLost(peSlef);
            RemoveFromTeam(peSlef);
		}


		/*****************************************
        * 基地NPC死亡
        * 
        * *************************************/
		public  void OnTeammberDeath(PeEntity entity)
		{
			if(!ContainMember(entity))
				return ;


			RemoveFromLine(ELineType.TeamAtk,entity);
		}

        /*****************************************
        * 目标丢失
        * 
        * *************************************/
		public  void OnTargetLost(PeEntity entity)
		{
			if(entity == null || !m_TeamEnemies.Contains(entity))
				return ;
			
			if(teamLinesContain(ELineType.TeamAtk))
			{
				AttackLine atkline = m_TeamLines[(int)ELineType.TeamAtk] as AttackLine;
				if(atkline != null)
				{
					m_tempList.Clear();
					List<PeEntity> lists = atkline.GetAtkMemberByTarget(entity);
					if(lists != null)
						m_tempList.AddRange(lists);
					
					for(int i=0;i<m_tempList.Count;i++)
					{
						RemoveFromLine(ELineType.TeamAtk,m_tempList[i]);
					}
					RemoveFormTeamEnemies(entity);
                    mapEnemiesRemoveTaregt(entity);
					atkline.DissolveTheline(entity);

					if(mCScreator ==null || mCScreator.Assembly == null)
					{
						for(int i=0;i<m_tempList.Count;i++)
						{
							m_tempList[i].target.SetEnityCanAttack(true);
						}
					}
				}
			}

            RemoveFromAssemblyEnemies(entity);
		}

        /*****************************************
        * 基地核心被攻击
        * 
        * *************************************/
        public void OnAssemblyHpChange(SkillSystem.SkEntity caster, float hpChange)
        {
            PESkEntity _caster = PETools.PEUtil.GetCaster(caster) as PESkEntity;
            if (_caster == null)
                return;

            PeEntity _caPeentity = _caster.GetComponent<PeEntity>();
            if (_caPeentity == null)
                return;

            if (teamMembers == null)
                return;

            for (int i = 0; i < teamMembers.Count; i++)
            {
                if (!teamMembers[i].Equals(null) && teamMembers[i].target != null)
                {
                    teamMembers[i].target.TransferHatred(_caPeentity, Mathf.Abs(hpChange * 10.0f));
                }
            }

        }


        /*****************************************
        * line发送信息：
        * objs每个line需要的信息
        * *************************************/
        void MsgLine(ELineType type,params object[] objs)
		{
			if(teamLinesContain(type))
				m_TeamLines[(int)type].OnMsgLine(objs);
		}

        /*************************************
         * 攻击方式变化（武器的变化导致攻击方式变化）
         * ********************************/
		public  void OnAttackTypeChange(PeEntity entity,AttackType oldType,AttackType newType)
		{
			if(oldType == AttackType.Ranged && newType == AttackType.Melee )
			{
				if(teamLinesContain(ELineType.TeamAtk))
				{
					AttackLine atkline = m_TeamLines[(int)ELineType.TeamAtk] as AttackLine;
					if(atkline != null && atkline.ContainEntity(entity))
					{
						RemoveFromLine(ELineType.TeamAtk,entity);
					}
				}
			}
		}

        /*************************************
        * 队伍变化
        * ********************************/
		public void OnLineChange(PeEntity member,ELineType oldType,ELineType newType)
		{
			bool inOldLine = teamLinesContain(oldType) && m_TeamLines[(int)oldType].ContainEntity(member);
			if(!inOldLine)
				return ;

			if(!teamLinesContain(newType))
				return ;

			if(ComPearPriority(m_TeamLines[(int)newType],m_TeamLines[(int)oldType]))
			{
				RemoveFromLine(oldType,member);
			}
		}

        /*************************************
       * npc工作变更
       * ********************************/
		public void OnCsNpcJobChange(PeEntity member,ENpcJob oldJob,ENpcJob newJob)
		{
			if(oldJob != ENpcJob.Resident)
				return ;

			if(newJob == ENpcJob.None || newJob == ENpcJob.Resident)
				return ;

            if (newJob == ENpcJob.Follower)
			{
                List<ELineType> lists = new List<ELineType>();
                if (teamMembers != null && teamMembers.Contains(member) && m_TeamLines != null)
				{
					for(int i=0;i<m_TeamLines.Length;i++)
					{
						if(m_TeamLines[i] != null && m_TeamLines[i].ContainEntity(member))
							lists.Add((ELineType)i);
					}
                    for (int i = 0; i < lists.Count;i++ )
                    {
                        RemoveFromLine(lists[i], member);
                    }
					RemoveFromTeam(member);
				}
				return ;
			}

			ELineType type = ELineType.TeamChat;
			if(teamLinesContain(type) && m_TeamLines[(int)type].ContainEntity(member))
			{
				RemoveFromLine(type,member);
			}
		}

        /*************************************
       * npc移除基地
       * ********************************/
		public void OnCsNPcRemove(PeEntity member)
		{
			RemoveFromTeam(member);
		}
		#endregion

	}
}
