using PETools;
using SkillSystem;
using Pathea.PeEntityExt;
using Pathea.PeEntityExtTrans;
using Mono.Data.SqliteClient;
using System.Collections.Generic;

namespace Pathea
{
	public  class EatLine : TeamLine
	{
		public EatLine():base()
		{
		}

		public override ELineType Type {
			get {
				return ELineType.TeamEat;
			}
		}

		public override bool Go ()
		{
			return false;
		}

		public override int Priority {
			get {
				return (int)EPriority.Eat;
			}
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
			case ELineMsg.Add_Eat:
				int num = (int)objs[1];
				CreatEatCooper(num);
				break;
			}
		}

		void CreatEatCooper(int num)
		{
			EatCooperation eat = new EatCooperation(num);
			mCooperationLists.Add(eat);
		}
	}
}

