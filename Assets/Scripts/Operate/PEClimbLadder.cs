using UnityEngine;
using System.Collections;

namespace Pathea.Operate
{
	public class PEClimbLadder : Operation_Single
	{
		public ItemScript_ClimbLadder.OpSide opSide{ get; set;}

		public override bool Do(IOperator oper)
		{
			PEActionParamVQN param = PEActionParamVQN.param;
			param.vec = transform.position;
			param.q = transform.rotation;
			param.n = (int)opSide;
            return oper.DoAction (PEActionType.Climb, param);
		}

		public override bool Do(IOperator oper, PEActionParam para)
		{
			//oper.DoAction(PEActionType.Sleep, transform.position, transform.rotation, id,ainm);			
			return true;
		}

		public override bool UnDo(IOperator oper)
		{
			oper.EndAction(PEActionType.Climb);			
			return true;
		}

		public override EOperationMask GetOperateMask()
		{
			return EOperationMask.ClimbLadder;
		}

		public override bool StartOperate (IOperator oper, EOperationMask mask)
		{
			if(m_Mask == mask)
			{
				Do(oper);
				return true;
			}
			return false;
		}

		public override bool StopOperate (IOperator oper, EOperationMask mask)
		{
			if (m_Mask == mask)
			{
				UnDo(oper);
			}
			return true;
		}
	}
}
