using PETools;
using SkillSystem;
using Pathea.PeEntityExt;
using Pathea.PeEntityExtTrans;
using Mono.Data.SqliteClient;
using System.Collections.Generic;

namespace Pathea
{
	public class chatLine : TeamLine 
	{
		private int centorNum;
		private List<PeEntity> mchatCentors;
		public chatLine(int _cNum) : base()
		{
			centorNum = _cNum;
			mchatCentors = new List<PeEntity>(centorNum);
		}

		public override ELineType Type {
			get {
				return ELineType.TeamChat;
			}
		}

		public override int Priority {
			get {
				return (int)EPriority.Rest;
			}
		}

		public int CenterNum { get { return mchatCentors.Count;}}

		public override void OnMsgLine (params object[] objs)
		{
			ELineMsg msg = (ELineMsg)objs[0];
			switch(msg)
			{
			case ELineMsg.Add_chatCentor:
				EQuadrant Q = (EQuadrant)objs[1];
				PeEntity centor = (PeEntity)objs[2];
				if(centor != null)
				{
					AddrestCenter(centor,Q);
				}
				break;
			case ELineMsg.Clear_chat:
				ClearCenter();
				break;
			}

			return ;
		}


		public override bool Go ()
		{
			if(mLinemembers == null)
				return false;

			UpdataChat();
			return false;
		}

		public override bool CanAddCooperMember (PeEntity member, params object[] objs)
		{
			for(int i=0;i<mCooperationLists.Count;i++)
			{
				if(mCooperationLists[i].ContainMember(member))
					return false;
			}
			
			for(int i=0;i<mCooperationLists.Count;i++)
			{
				if(mCooperationLists[i].CanAddMember(member.position))
				{
					mCooperationLists[i].AddMember(member);
					return true;
				}
				
			}
			return false;
		}

		public override bool memberCanAddIn (PeEntity member, params object[] objs)
		{
			for(int i=0;i<mCooperationLists.Count;i++)
			{
				if(mCooperationLists[i].ContainMember(member))
					return false;
			}
			
			for(int i=0;i<mCooperationLists.Count;i++)
			{
				if(mCooperationLists[i].CanAddMember(member.position))
				{
					return true;
				}
				
			}
			return false;
		}
		public override bool AddIn (PeEntity member, params object[] objs)
		{
			if(CanAddCooperMember(member,objs))
			{
				if(member.NpcCmpt != null)
				   member.NpcCmpt.SetLineType(ELineType.TeamChat);

				return base.AddIn (member, objs);
			}
			return  false;
		}

		public override bool RemoveOut (PeEntity member)
		{
			RemoveFromCooper(member);
			return base.RemoveOut (member);
		}

        public override bool Suitline(PeEntity member, params object[] objs)
        {
            if (objs == null)
                return true;

            return member.NpcCmpt != null ? !member.NpcCmpt.IsNeedMedicine : false;
        }

		void CreatchatCooper(PeEntity centor,EQuadrant q,UnityEngine.Vector3 centorPos)
		{
			chatCooperation rest = new chatCooperation(UnityEngine.Random.Range(2,CSNpcTeam.Chat_member_Num+1));
            rest.setCentor(centor, q, centorPos);
			mCooperationLists.Add(rest);
		}



		void UpdataChat()
		{
			for(int i=0;i<mLinemembers.Count;i++)
			{
				if(mLinemembers[i].NpcCmpt != null)
				  mLinemembers[i].NpcCmpt.SetLineType(ELineType.TeamChat);
			}
		}

		bool QuadrantHasCentor(EQuadrant q)
		{
			for(int i=0;i<mCooperationLists.Count;i++)
			{
				chatCooperation ret = mCooperationLists[i] as chatCooperation;
				if(ret != null && ret.quadrant == q)
				{
					return  true;
				}
			}
			return false;
		}

		void AddrestCenter(PeEntity centor,EQuadrant q)
		{

            UnityEngine.Vector3 CenterPos = PETools.PEUtil.GetEmptyPositionOnGround(centor.position, 2.0f, 5.0f);
			if(CenterPos !=  UnityEngine.Vector3.zero  && !mchatCentors.Contains(centor) && mchatCentors.Count < centorNum && !QuadrantHasCentor(q) && JudgeCenter(centor))
			{
				mchatCentors.Add(centor);
                CreatchatCooper(centor, q, CenterPos);
			}
		}

		bool JudgeCenter(PeEntity centor,float radiu = 20.0f)
		{
			for(int i=0;i<mchatCentors.Count;i++)
			{
				if(PETools.PEUtil.MagnitudeH(centor.position,mchatCentors[i].position) < radiu)
					return false;
			}
			return true;
		}

		void RemoveCentor(PeEntity centor)
		{
			if(mchatCentors.Contains(centor))
				mchatCentors.Remove(centor);
		}

		void ClearCenter()
		{
			mchatCentors.Clear();
		}

		public bool CanAddcenter()
		{
			return mchatCentors.Count <= 0;
		}

	}
}

