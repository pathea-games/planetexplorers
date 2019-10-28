using UnityEngine;
using System.Collections;


namespace Pathea.Operate
{
	public class PELay : Operation_Single
	{
		public string LayAnim;
		public Transform m_StandTrans;

		public override bool Do(IOperator oper)
		{
			// vec3 pos1 float rot1(stand) vec3 pos2 float rot2(bed) string animName
			PEActionParamVFVFS param = PEActionParamVFVFS.param;
			param.vec1 = m_StandTrans.position;
			param.f1 = m_StandTrans.rotation.eulerAngles.y;
			param.vec2 = transform.position;
			param.f2 = transform.rotation.eulerAngles.y;
			param.str = LayAnim;
            return oper.DoAction(PEActionType.Cure, param);			
		}

		public override bool Do(IOperator oper, PEActionParam para)
		{
			//oper.DoAction(PEActionType.Sleep, transform.position, transform.rotation, id,ainm);			
			return true;
		}

		public override bool UnDo(IOperator oper)
		{
			oper.EndAction(PEActionType.Cure);			
			return true;
		}

		public override EOperationMask GetOperateMask()
		{
			return EOperationMask.Lay;
		}

	}
}

