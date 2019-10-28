using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Pathea.Operate
{
    public class PEBed : Operation_Multiple
    {
        public PESleep[] sleeps;

        public override List<Operation_Single> Singles
        {
            get { return (sleeps==null || sleeps.Length == 0) ? null : new List<Operation_Single>(sleeps); }
        }
    }
}
