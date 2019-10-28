using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Pathea.Operate
{
	public class PECureSleep : MonoBehaviour 
	{
		IOperator Operator;
		PeEntity mSleepEntity;
		float mStartLayTime;

        bool mIsPase = false;
        //bool mIsStop = false;

		bool hasOperator{ get{ return null != Operator && !Operator.Equals(null);} }
		public  float REOVE_TIME;
		// Use this for initialization
		void Start () 
		{
			
		}
		
		// Update is called once per frame
        void Update()
        {
            if (hasOperator && mSleepEntity == null)
            {
                OperateCmpt cmpt = Operator as OperateCmpt;
                if (cmpt != null)
                    mSleepEntity = cmpt.Entity;
            }

            if (mSleepEntity != null && mSleepEntity.Alnormal != null)
            {
                if (REOVE_TIME != 0)
                {
                    if (Time.time - mStartLayTime >= REOVE_TIME && !mIsPase)
                    {
                        //时间到了，正常结束动作
                        if (Operator != null && !Operator.Equals(null) && Operator.Operate != null && !Operator.Operate.Equals(null)) // && Operator.Operate.ContainsOperator(Operator) && Operator.IsActionRunning(PEActionType.Sleep)
                        {
                            bool IsEnd = Operator.Operate.StopOperate(Operator, EOperationMask.Sleep);
                            if (IsEnd && mSleepEntity != null && mSleepEntity.NpcCmpt != null)
                            {
                                mSleepEntity.NpcCmpt.IsNeedMedicine = false;
                                EndAlnormal(mSleepEntity);
                                mSleepEntity = null;
                            }

                        }
                        else if (mSleepEntity != null && mSleepEntity.NpcCmpt != null) //被打断结束了动作
                        {
                            mSleepEntity.NpcCmpt.IsNeedMedicine = false;
                            EndAlnormal(mSleepEntity);
                            mSleepEntity = null;
                        }


                    }
                }
            }
        }

        void EndAlnormal(PeEntity entity)
        {
            List<PEAbnormalType> lists = entity.Alnormal.GetActiveAbnormalList();
            if (lists != null)
            {
                for (int i = 0; i < lists.Count; i++)
                {
                    entity.Alnormal.EndAbnormalCondition(lists[i]);
                }
            }
        }

		public void AddOperator(IOperator oper)
		{
			Operator = oper;
			mStartLayTime = mIsPase ? mStartLayTime : Time.time;
            mIsPase = false;
		}

		public void RemveOperator()
		{
			Operator = null;

            mIsPase = Time.time - mStartLayTime >= REOVE_TIME ? false : true;
			mStartLayTime = mIsPase ? mStartLayTime : 0;

		}

	}
}

