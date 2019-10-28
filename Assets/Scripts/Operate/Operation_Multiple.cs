using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Pathea.Operate
{
	public abstract class Operation_Multiple : Operation
    {
        Bounds m_Bounds;
        Bounds m_OperateBounds;

		public abstract List<Operation_Single> Singles { get; }

        public Bounds LocalBounds {
            get{
                if(m_Bounds.size == Vector3.zero)
                    m_Bounds = PETools.PEUtil.GetLocalViewBoundsInChildren(gameObject);
                return m_Bounds;
            }
        }
        public Bounds OperateBoundsBounds {
            get{
                if (m_OperateBounds.size == Vector3.zero){
                    m_OperateBounds = PETools.PEUtil.GetLocalViewBoundsInChildren(gameObject);

                    if(m_OperateBounds.size != Vector3.zero)
                        m_OperateBounds.Expand(3.0f);
                }
                return m_OperateBounds;
            }
        }

        public Operation_Single GetStartOperate(EOperationMask mask)
        {
            if (Singles != null)
                return Singles.Find(ret => ret != null && ret.CanOperateMask(mask));
            else
                return null;
        }

		Operation_Single GetStopOperate(IOperator oper, EOperationMask mask)
        {
            if (Singles != null)
                return Singles.Find(ret => ret != null && ret.m_Mask == mask && ret.ContainsOperator(oper));
            else
                return null;
        }

        public override bool CanOperate(Transform trans)
        {
            return OperateBoundsBounds.Contains(transform.InverseTransformPoint(trans.position));
        }

		public override bool IsIdle()
        {
			foreach (Operation_Single single in Singles)
            {
				if (!single.IsIdle())
                    return false;
            }

            return true;
        }

        public override EOperationMask GetOperateMask()
        {
            EOperationMask tmpMask = EOperationMask.None;

            if (Singles != null)
            {
				foreach (Operation_Single single in Singles)
                {
                    if (single != null)
                    {
                        tmpMask |= single.m_Mask;
                    }
                }
            }

            return tmpMask;
        }

        public override bool CanOperateMask(EOperationMask mask)
        {
            if (Singles != null)
            {
				foreach (Operation_Single single in Singles)
                {
                    if (single != null && single.CanOperateMask(mask))
                        return true;
                }
            }

            return false;
        }

        public override bool ContainsOperator(IOperator oper)
        {
            if (Singles != null)
            {
				foreach (Operation_Single single in Singles)
                {
                    if (single != null && single.ContainsOperator(oper))
                        return true;
                }
            }

            return false;
        }

		public override bool StartOperate(IOperator oper, EOperationMask mask)
        {
			Operation_Single single = GetStartOperate(mask);
            if (single != null)
            {
                return single.StartOperate(oper, mask);
            }
			return false;
        }

		public override bool StartOperate(IOperator oper, EOperationMask mask, PEActionParam para)
		{
			Operation_Single single = GetStartOperate(mask);
			if (single != null)
			{
				return single.StartOperate(oper, mask, para);
			}
			return false;
		}


		public override bool StopOperate(IOperator oper, EOperationMask mask)
        {
			Operation_Single single = GetStopOperate(oper, mask);
            if (single != null)
            {
                return single.StopOperate(oper, mask);
            }
			return true;
        }

        public void OnDrawGizmosSelected()
        {
            PETools.PEUtil.DrawBounds(transform, OperateBoundsBounds, Color.red);
        }
    }
}
