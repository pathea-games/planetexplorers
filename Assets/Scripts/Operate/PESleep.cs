using UnityEngine;
using System.Collections;

namespace Pathea.Operate
{
    public class PESleep : Operation_Single
    {
		int id = 30200055;
		string Ainm = "Sleep";
		public PECureSleep mCurSleep;

		public override bool Do(IOperator oper)
        {
			PEActionParamVQNS param = PEActionParamVQNS.param;
			param.vec = transform.position;
			param.q = transform.rotation;
			param.n = id;
			param.str = Ainm;
            return oper.DoAction(PEActionType.Sleep, param);	
        }

		public override bool Do(IOperator oper, PEActionParam para)
		{
            bool execRusult = oper.DoAction(PEActionType.Sleep, para);
            if (execRusult&&mCurSleep != null)
            {
                mCurSleep.AddOperator(oper);
            }
			return execRusult;
		}

		public override bool UnDo(IOperator oper)
        {
			oper.EndAction(PEActionType.Sleep);

            //确保动作完全结束
            bool isEnd = true;
            if (mCurSleep != null && isEnd)
				mCurSleep.RemveOperator();

            return isEnd;
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
            return EOperationMask.Sleep;
        }

//		public void InitSleep(int buffid,string _Ainm)
//		{
//			id = buffid;
//			Ainm = _Ainm;
//		}
    }
}
