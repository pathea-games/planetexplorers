using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Pathea.Operate
{
    public class PEMachine : Operation_Multiple
    {
        public PEWork[] works;

        public override List<Operation_Single> Singles
        {
            get { return new List<Operation_Single>(works); }
        }
    }
}
