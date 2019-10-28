using UnityEngine;
using System.Collections;


namespace Pathea.Operate
{

	public class PESit : Operation_Single 
	{
		//public int id;
		public string Ainm;
		[SerializeField] int m_BuffID;
		//public Transform SitPos;
		public override bool Do(IOperator oper)
		{
			PEActionParamVQSN param = PEActionParamVQSN.param;
			param.vec = transform.position;
			param.q = transform.rotation;
			param.str = Ainm;
			param.n = m_BuffID;
            return oper.DoAction(PEActionType.Sit, param);	
		}
		
		public override bool Do(IOperator oper, PEActionParam para)
		{
			return true;
		}
		
		public override bool UnDo(IOperator oper)
		{
			oper.EndAction(PEActionType.Sit);		
			return true;
		}
		
		public override bool CanOperateMask(EOperationMask mask)
		{
			if(base.CanOperateMask(mask))
			{
				float angle = Vector3.Angle(transform.up, Vector3.up);
				if (angle < WhiteCat.PEVCConfig.instance.bedLimitAngle)
				{
					return true;
				}
			}
			return false;
		}
		
		
		public override EOperationMask GetOperateMask()
		{
			return EOperationMask.Sit;
		}

	}
}
