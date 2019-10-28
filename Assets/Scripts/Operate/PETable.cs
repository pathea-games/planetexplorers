using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Pathea.Operate
{
    public class PETable : Operation_Multiple
    {
        public PEEat[] eats;

        public override List<Operation_Single> Singles
        {
            get { return new List<Operation_Single>(eats); }
        }
    }
}

