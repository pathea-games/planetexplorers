using PETools;
using SkillSystem;
using Pathea.PeEntityExt;
using Pathea.PeEntityExtTrans;
using Mono.Data.SqliteClient;
using System.Collections.Generic;

namespace Pathea
{
	public class CSSleepSlots
	{
		public CheckSlot startSlots;
		public CheckSlot endSlots;
		public CSSleepSlots(CheckSlot start,CheckSlot end)
		{
			startSlots = start;
			endSlots = end;
		}
	}

	public  class SleepLine : TeamLine
	{

		public SleepLine() : base()
		{

		}

		public override ELineType Type {
			get {
				return ELineType.TeamSleep;
			}
		}

		public override int Priority {
			get {
				return (int)EPriority.Sleep;
			}
		}

		public override bool Go ()
		{
			return false;
		}

		public override bool AddIn (PeEntity member, params object[] objs)
		{
			if(CanAddCooperMember(member,objs))
			{
				return base.AddIn (member, objs);
			}
			return  false;
		}

		public override bool RemoveOut (PeEntity member)
		{
			RemoveFromCooper(member);
			return base.RemoveOut (member);
		}

		public override void OnMsgLine (params object[] objs)
		{
			ELineMsg msg = (ELineMsg)objs[0];
			switch(msg)
			{
			case ELineMsg.Add_Sleep:
				int num = (int)objs[1];
				double startHour = (double)objs[2];
				CreatSleepCooper(num,startHour);
				break;
			}
		}

		void CreatSleepCooper(int num,double startHour)
		{
			SleepCooperation sleep = new SleepCooperation(num,startHour);
			mCooperationLists.Add(sleep);
		}

		public SleepCooperation GetUpCooper()
		{
			for(int i=0;i<mCooperationLists.Count;i++)
			{
				SleepCooperation sleep = mCooperationLists[i] as SleepCooperation;
				if(sleep.GetCurMemberNum() >0)//sleep.IsTimeout() &&
					return sleep;
			}
			return null;
		}


	}
}
