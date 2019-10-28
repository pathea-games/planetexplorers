using UnityEngine;
using System.Collections;

namespace Pathea.Operate
{
    public interface IOperator
    {
        IOperation Operate { get; set; }
		bool IsActionRunning(PEActionType type);
        bool DoAction(PEActionType type, PEActionParam para = null);
        void EndAction(PEActionType type);
    }

    public interface IOperation
    {
        Transform Trans { get; }

        bool CanOperate(Transform trans);
        bool CanOperateMask(EOperationMask mask);
        bool ContainsMask(EOperationMask mask);
        bool ContainsOperator(IOperator oper);

        bool StartOperate(IOperator oper, EOperationMask mask);
		bool StartOperate(IOperator oper, EOperationMask mask, PEActionParam para);
        bool StopOperate(IOperator oper, EOperationMask mask);
    }

    public enum EOperationMask
    {
        None    = 0,
        Sleep   = 1,
        Eat     = 2,
        Work    = 4,
		Cure    = 8,
		Lay     = 16,
		Practice   = 32,
		ClimbLadder = 64,
		Sit	= 128,
        Ride =256, //lz-2016.12.21  Æï
        Max
    }

    public abstract class Operation : MonoBehaviour, IOperation
    {
        internal EOperationMask m_Mask;

        public Transform Trans { get { return transform; } }

        public void Awake()
        {
            m_Mask = GetOperateMask();
        }

        public bool ContainsMask(EOperationMask mask)
        {
            return (m_Mask & mask) != 0;
        }

		public virtual bool IsIdle() { return true; }

        public abstract bool CanOperate(Transform trans);
        public abstract EOperationMask GetOperateMask();
        public abstract bool CanOperateMask(EOperationMask mask);
        public abstract bool ContainsOperator(IOperator oper);
		public abstract bool StartOperate(IOperator oper, EOperationMask mask);
		public abstract bool StartOperate(IOperator oper, EOperationMask mask, PEActionParam para);
        public abstract bool StopOperate(IOperator oper, EOperationMask mask);
    }
}

