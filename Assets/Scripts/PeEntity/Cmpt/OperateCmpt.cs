using UnityEngine;
using System.Collections;
using Pathea.Operate;

namespace Pathea
{
    public class OperateCmpt : PeCmpt, IOperator
    {
        IOperation m_Operate;

        MotionMgrCmpt m_Motion;

        public override void Start()
        {
            base.Start();

            m_Motion = GetComponent<MotionMgrCmpt>();
        }

        public bool HasOperate
        {
            get { return m_Operate != null && !m_Operate.Equals(null); }
        }

        public IOperation Operate
        {
            get { return m_Operate; }
            set { m_Operate = value; }
        }
		public bool IsActionRunning(PEActionType type)
		{
			return m_Motion != null && m_Motion.IsActionRunning(type);
		}
        public bool DoAction(PEActionType type, PEActionParam para)
        {
            if (m_Motion != null)
            {
                return m_Motion.DoAction(type, para);
            }
            return false;
        }
        public void EndAction(PEActionType type)
        {
            if (m_Motion != null)
            {
                m_Motion.EndAction(type);
            }
        }
    }
}

