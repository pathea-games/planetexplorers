using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Pathea
{
	public  abstract class Cooperation
	{
		internal List<PeEntity> mCooperMembers;
		internal int            mCooperMemNum;

		public Cooperation(int number)
		{
			mCooperMemNum = number;
			mCooperMembers = new List<PeEntity>(mCooperMemNum);
		}

//		public   delegate void CooperationEnd(Cooperation self);
//		public   CooperationEnd onCooperationEnd;

		public abstract  void DissolveCooper();

		public virtual bool IsneedMember()
		{
			return  mCooperMemNum > mCooperMembers.Count;
		}

		public virtual int GetCurMemberNum()
		{
			return mCooperMembers.Count;
		}

		public virtual int GetCooperNeedNum()
		{
			return mCooperMemNum;
		}

		public virtual List<PeEntity> GetCooperMembers()
		{
			return mCooperMembers;
		}

		public virtual bool AddMember(PeEntity entity)
		{
			if(mCooperMembers.Count >= mCooperMemNum)
				return false;

			mCooperMembers.Add(entity);
			return true;
		}

		public virtual bool ContainMember(PeEntity entity)
		{
			return  mCooperMembers.Contains(entity);
		}

		public virtual bool RemoveMember(PeEntity entity)
		{
			if(mCooperMembers.Remove(entity) && entity.NpcCmpt != null)
			{
				entity.NpcCmpt.SetLineType(ELineType.IDLE);
				return true;
			}
			return false;
		}

		public virtual bool CanAddMember(params object[] objs)
		{
			return mCooperMembers.Count <mCooperMemNum;
		}

		public virtual void ClearCooper()
		{
			mCooperMembers.Clear();
		}
	}




}

