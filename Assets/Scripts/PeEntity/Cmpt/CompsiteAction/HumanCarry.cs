using UnityEngine;
using System.Collections;

namespace Pathea
{
    public class HumanCarrier : PeCmpt, HumanCarry.ICarrier
    {

        void HumanCarry.ICarrier.Begin()
        {
            throw new System.NotImplementedException();
        }

        void HumanCarry.ICarrier.Move()
        {
            throw new System.NotImplementedException();
        }

        void HumanCarry.ICarrier.End()
        {
            throw new System.NotImplementedException();
        }
    }


    public class HumanBeCarrier : PeCmpt, HumanCarry.IBeCarry
    {
        void HumanCarry.IBeCarry.Begin()
        {
            throw new System.NotImplementedException();
        }

        void HumanCarry.IBeCarry.Move()
        {
            throw new System.NotImplementedException();
        }

        void HumanCarry.IBeCarry.End()
        {
            throw new System.NotImplementedException();
        }
    }
}