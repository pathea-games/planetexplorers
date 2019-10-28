using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Pathea
{
	public class chatCooperation : Cooperation
	{
		public class DataV
		{
			
			int _useid;
			Vector3 _Pos;
			bool _Isused;
			public bool IsUsed { get { return _Isused ;}}
			public Vector3 Pos { get { return _Pos;}}
			public int  UseID  { get { return _useid;}}
			
			public DataV(Vector3 v3,bool isused)
			{
				_Pos = v3;
				_Isused = isused;
			}
			
			public void UsePos(bool use,int protoId)
			{
				_Isused = use;
				_useid = protoId;
			}
			
		}
		
		List<DataV> mchatPoses;
		PeEntity mcentor;
		//float addinRadiu = 20.0f;

		EQuadrant mEQuadrant;
		Vector3 mCenterPos;
		public EQuadrant quadrant { get {return mEQuadrant;}}
		public chatCooperation(int memberNum):base(memberNum)
		{
			mchatPoses = new List<DataV>();
		}
		
		public void setCentor(PeEntity _centor,EQuadrant q,Vector3 centorPos)
		{
			mcentor = _centor;
			mEQuadrant = q;
            mCenterPos = centorPos;
			calculateRestPos();
		}
		
		void calculateRestPos()
		{
            Vector3 dir = mcentor.position - mCenterPos;
			float angle = 360.0f/mCooperMemNum;
			for(int i=0;i<mCooperMemNum;i++)
			{
				float ag = angle *i;
				mchatPoses.Add(new DataV (PETools.PEUtil.GetRandomPosition(mCenterPos, -dir, 0.8f*CSNpcTeam.Chat_Radiu, CSNpcTeam.Chat_Radiu, ag,ag),false));
			}

		}
		
		public override bool AddMember (PeEntity entity)
		{
			for(int i=0;i<mchatPoses.Count;i++)
			{
				if(!mchatPoses[i].IsUsed)
				{
					//use Pos
					if(entity.NpcCmpt != null && base.AddMember (entity))
					{
						entity.NpcCmpt.setTeamData(mchatPoses[i].Pos,mCenterPos);
						entity.NpcCmpt.SetLineType(ELineType.TeamChat);
						mchatPoses[i].UsePos(true,entity.ProtoID);
						return true;
					}
					
				}
			}
			return false;
		}
		
		public override bool RemoveMember (PeEntity entity)
		{
			for(int i=0;i<mchatPoses.Count;i++)
			{
				if(entity.NpcCmpt != null && mchatPoses[i].UseID == entity.ProtoID)
				{
					mchatPoses[i].UsePos(false,0);
					entity.NpcCmpt.SetLineType(ELineType.IDLE);
					return base.RemoveMember (entity);
				}
			}
			return false;
			
			//remove pos
		}
		public override void DissolveCooper ()
		{
			mCooperMembers.Clear();
		}

        //public override bool CanAddMember (params object[] objs)
        //{
        //    Vector3 pos = (Vector3)objs[0];
        //    float   dis = PETools.PEUtil.SqrMagnitude(mCenterPos,pos);
        //    bool    canadd = dis <= addinRadiu;
        //    return  canadd && base.CanAddMember (objs) ;
        //}
		
		
	}
}

