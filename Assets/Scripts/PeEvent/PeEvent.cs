using UnityEngine;
using System.Collections.Generic;

namespace PeEvent
{
    public class GeneralArg : EventArg
    {

    }

    public class AttributeChangedArg : EventArg
    {
		public Pathea.AttribType eType;
		public Pathea.SkAliveEntity attEntity;
    }

    public class KillEventArg : EventArg
    {
        public Pathea.PeEntity killer;
        public Pathea.PeEntity victim;
    }
}
