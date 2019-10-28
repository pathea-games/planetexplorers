using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Pathea.Operate
{
	public abstract class Operation_Single : Operation
    {
		static EOperationMask[] pairOperationTypes = new EOperationMask[]{
			EOperationMask.ClimbLadder,
		};
		static PEActionType[] pairActionTypes = new PEActionType[]{
			PEActionType.Climb,
		};

        IOperator m_Operator;

        public abstract bool Do(IOperator oper);
		public abstract bool Do(IOperator oper, PEActionParam para);
        public abstract bool UnDo(IOperator oper);

        public IOperator Operator
        {
            get 
			{
				if(null == m_Operator || m_Operator.Equals(null))
					return null;
				return m_Operator; 
			}
            set
            {
                m_Operator = value;
            }
        }

        public override bool ContainsOperator(IOperator oper)
        {
			if (IsIdle ())
				return false;

			if(null == Operator)
				return false;
            return m_Operator.Equals(oper);
        }

        public override bool CanOperateMask(EOperationMask mask)
        {
            if (m_Mask == mask)
            {
				return IsIdle();
			}
			
			return false;
        }

		public override bool IsIdle()
        {
			if (null == Operator)
				return true;

			int idx = Array.IndexOf (pairOperationTypes, GetOperateMask ());
			if(idx >= 0)
			{
				if(!m_Operator.IsActionRunning(pairActionTypes[idx]))
				{
					m_Operator = null;
					return true;
				}
			}

			return false;
        }

        public override bool StartOperate(IOperator oper, EOperationMask mask)
        {
            if (null == oper || oper.Equals(null)) return false;
            if (m_Mask == mask && !ContainsOperator(oper))
            {
                Operator = oper;
                oper.Operate = this;
                if (!Do(oper))
                {
                    Operator = null;
                    oper.Operate = null;
                    return false;
                }
                return true;
            }
            return false;
        }

		public override bool StartOperate(IOperator oper, EOperationMask mask, PEActionParam para)
		{
            if (null == oper || oper.Equals(null)) return false;
            if (m_Mask == mask && !ContainsOperator(oper))
            {
                Operator = oper;
                oper.Operate = this;
                if (!Do(oper,para))
                {
                    Operator = null;
                    oper.Operate = null;
                    return false;
                }
                return true;
            }
            return false;
		}

        public override bool StopOperate(IOperator oper, EOperationMask mask)
		{
			if(null == oper || oper.Equals(null)) return false;
            if (m_Mask == mask && ContainsOperator(oper))
            {
                if(UnDo(oper))
				{
					m_Operator = null;
					oper.Operate = null;
					return true;
				}
				return false;
            }
			return true;
        }

        public override bool CanOperate(Transform trans)
        {
            return false;
        }

        public override EOperationMask GetOperateMask()
        {
            return EOperationMask.None;
        }
    }
}

