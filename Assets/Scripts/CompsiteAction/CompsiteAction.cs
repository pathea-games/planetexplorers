using UnityEngine;
using System.Collections;

namespace Pathea
{
    public class HumanCarry
    {
        public interface ICarrier
        {
            void Begin();
            void Move();
            void End();
        }

        public interface IBeCarry
        {
            void Begin();
            void Move();
            void End();
        }

        ICarrier mCarrier;
        IBeCarry mBeCarry;

        public HumanCarry(ICarrier carrier, IBeCarry becarry)
        {
            mCarrier = carrier;
            mBeCarry = becarry;
        }

        public void Begin()
        {
            mCarrier.Begin();
            mBeCarry.Begin();
        }

        public void Move()
        {
            mCarrier.Move();
            mCarrier.Move();
        }

        public void End()
        {
            mCarrier.End();
            mBeCarry.End();
        }
    }

    public class Conference
    {
        public Conference()
        {
        
        }
    }
}